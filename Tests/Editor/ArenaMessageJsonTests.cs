/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Linq;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace ArenaUnity.Tests
{
    /// <summary>
    /// ArenaMessageJson's [OnDeserialized] hook migrates the legacy "attributes"
    /// member into "data". Three of the eight scene fixtures in Tests/JSON Scenes use
    /// "attributes", so this is a live compatibility path, not a historical curiosity.
    /// </summary>
    [TestFixture]
    public class ArenaMessageJsonTests
    {
        [Test]
        public void Deserialize_LegacyAttributes_IsMigratedIntoData()
        {
            var msg = JsonConvert.DeserializeObject<ArenaMessageJson>(
                "{\"object_id\":\"box-1\",\"action\":\"create\"," +
                "\"attributes\":{\"object_type\":\"box\",\"position\":{\"x\":1,\"y\":2,\"z\":3}}}");

            Assert.That(msg.data, Is.Not.Null, "attributes must be promoted to data");

            var data = (JToken)msg.data;
            Assert.That(data["object_type"].Value<string>(), Is.EqualTo("box"));
            Assert.That(data["position"]["x"].Value<int>(), Is.EqualTo(1));
        }

        /// <summary>
        /// After migration the legacy member must not be re-emitted, or the same document
        /// would carry the payload twice on the way back out over MQTT.
        /// </summary>
        [Test]
        public void Reserialize_AfterMigration_EmitsDataAndNotAttributes()
        {
            var msg = JsonConvert.DeserializeObject<ArenaMessageJson>(
                "{\"object_id\":\"box-1\",\"attributes\":{\"object_type\":\"box\"}}");

            string json = JsonConvert.SerializeObject(msg);
            JObject parsed = JObject.Parse(json);

            Assert.That(parsed["data"], Is.Not.Null, "data must be present");
            Assert.That(parsed["attributes"], Is.Null, "attributes must have been consumed");
            Assert.That(parsed["data"]["object_type"].Value<string>(), Is.EqualTo("box"));
        }

        /// <summary>
        /// The guard is `data == null`, so an explicit "data" wins and "attributes" is
        /// left alone. No fixture in the corpus sends both, but a mixed-version publisher
        /// could, and the precedence must be deterministic.
        /// </summary>
        [Test]
        public void Deserialize_WhenBothPresent_DataWins()
        {
            var msg = JsonConvert.DeserializeObject<ArenaMessageJson>(
                "{\"object_id\":\"box-1\",\"data\":{\"object_type\":\"sphere\"}," +
                "\"attributes\":{\"object_type\":\"box\"}}");

            var data = (JToken)msg.data;
            Assert.That(data["object_type"].Value<string>(), Is.EqualTo("sphere"),
                "an explicit data member must not be overwritten by attributes");
        }

        [Test]
        public void Deserialize_WithNeitherMember_LeavesDataNull()
        {
            var msg = JsonConvert.DeserializeObject<ArenaMessageJson>(
                "{\"object_id\":\"box-1\",\"action\":\"delete\"}");

            Assert.That(msg.data, Is.Null);
            Assert.That(JObject.Parse(JsonConvert.SerializeObject(msg))["data"], Is.Null,
                "a null data must be elided by ShouldSerializedata");
        }

        [Test]
        public void Deserialize_PopulatesTheTypedEnvelopeMembers()
        {
            var msg = JsonConvert.DeserializeObject<ArenaMessageJson>(
                "{\"object_id\":\"box-1\",\"action\":\"create\",\"type\":\"object\"," +
                "\"persist\":true,\"ttl\":30,\"overwrite\":false,\"timestamp\":\"2024-01-01T00:00:00Z\"}");

            Assert.That(msg.object_id, Is.EqualTo("box-1"));
            Assert.That(msg.action, Is.EqualTo("create"));
            Assert.That(msg.type, Is.EqualTo("object"));
            Assert.That(msg.persist, Is.True);
            Assert.That(msg.ttl, Is.EqualTo(30f));
            Assert.That(msg.overwrite, Is.False);
            Assert.That(msg.timestamp, Is.EqualTo("2024-01-01T00:00:00Z"));
        }

        /// <summary>
        /// A freshly constructed envelope must serialize to nothing at all: every member
        /// is nullable and guarded by a ShouldSerialize. This is the wire-economy contract
        /// for outbound MQTT - only what changed goes on the wire.
        /// </summary>
        [Test]
        public void FreshInstance_SerializesToEmptyObject()
        {
            Assert.That(JsonConvert.SerializeObject(new ArenaMessageJson()), Is.EqualTo("{}"));
        }

        [Test]
        public void Serialize_OmitsUnsetMembersIndividually()
        {
            var msg = new ArenaMessageJson { object_id = "box-1" };
            JObject parsed = JObject.Parse(JsonConvert.SerializeObject(msg));

            Assert.That(parsed.Properties().Count(), Is.EqualTo(1));
            Assert.That(parsed["object_id"].Value<string>(), Is.EqualTo("box-1"));
        }

        /// <summary>
        /// Unrecognized members are preserved via [JsonExtensionData] rather than
        /// dropped, which is how createdAt / updatedAt survive a persist round trip.
        ///
        /// PINS CURRENT BEHAVIOUR (quirk): a value that *looks* like a date does not
        /// survive verbatim. Newtonsoft's default DateParseHandling.DateTime converts it
        /// to a DateTime while filling the extension-data JToken, so the milliseconds
        /// arena-persist sends ("...T00:00:00.000Z") come back out dropped
        /// ("...T00:00:00Z"). The instant is preserved; the literal is not, and reading
        /// the re-parsed token back as a string yields a culture-formatted DateTime
        /// rather than an ISO-8601 one - which is why this asserts the instant and the
        /// emitted JSON instead.
        ///
        /// If the exact literal ever matters on the wire, the fix is
        /// DateParseHandling.None on the reader; these expectations then become plain
        /// string equality again.
        /// </summary>
        [Test]
        public void UnknownMembers_SurviveARoundTrip()
        {
            var msg = JsonConvert.DeserializeObject<ArenaMessageJson>(
                "{\"object_id\":\"box-1\",\"createdAt\":\"2024-01-01T00:00:00.000Z\"," +
                "\"nonce\":\"abc-123\"}");

            string json = JsonConvert.SerializeObject(msg);
            JObject parsed = JObject.Parse(json);

            // A member that is not date-shaped survives byte for byte.
            Assert.That(parsed["nonce"].Value<string>(), Is.EqualTo("abc-123"));

            // A date-shaped one keeps its instant...
            Assert.That(parsed["createdAt"], Is.Not.Null, "createdAt must not be dropped");
            Assert.That(parsed["createdAt"].Value<DateTime>().ToUniversalTime(),
                Is.EqualTo(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

            // ...but not its milliseconds.
            Assert.That(json, Does.Contain("\"createdAt\":\"2024-01-01T00:00:00Z\""));
        }
    }
}
