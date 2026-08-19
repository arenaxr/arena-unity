/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System;
using ArenaUnity.Schemas.Converter;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ArenaUnity.PureTests
{
    /// <summary>
    /// ArenaColorJsonConverter / ArenaColorArrayJsonConverter normalize inbound CSS
    /// colour names to hex on the way in, and pass values straight through on the way
    /// out. These tests drive them the way Newtonsoft drives them in production - via
    /// a DTO carrying the [JsonConverter] attribute - rather than calling ReadJson by
    /// hand, so attribute wiring is covered too.
    /// </summary>
    [TestFixture]
    public class ArenaColorJsonConverterTests
    {
        private class ColorProbe
        {
            [JsonProperty(PropertyName = "color")]
            [JsonConverter(typeof(ArenaColorJsonConverter))]
            public string Color;
        }

        private class ColorArrayProbe
        {
            [JsonProperty(PropertyName = "colors")]
            [JsonConverter(typeof(ArenaColorArrayJsonConverter))]
            public string[] Colors;
        }

        // ------------------------------------------------------- scalar converter

        [TestCase("\"violet\"", "#EE82EE", TestName = "Read_NamedColour_BecomesHex")]
        [TestCase("\"VIOLET\"", "#EE82EE", TestName = "Read_NamedColour_IsCaseInsensitive")]
        [TestCase("\"  violet \"", "#EE82EE", TestName = "Read_NamedColour_IsTrimmed")]
        [TestCase("\"#AABBCC\"", "#AABBCC", TestName = "Read_Hex_PassesThrough")]
        [TestCase("\"rgb(1,2,3)\"", "rgb(1,2,3)", TestName = "Read_Rgb_PassesThrough")]
        [TestCase("\"notacolor\"", "notacolor", TestName = "Read_UnknownName_PassesThrough")]
        [TestCase("null", null, TestName = "Read_Null_StaysNull")]
        public void Scalar_Read(string colorJson, string expected)
        {
            var probe = JsonConvert.DeserializeObject<ColorProbe>("{\"color\":" + colorJson + "}");
            Assert.That(probe.Color, Is.EqualTo(expected));
        }

        [Test]
        public void Scalar_Write_EmitsTheStoredValueVerbatim()
        {
            var json = JsonConvert.SerializeObject(new ColorProbe { Color = "#EE82EE" });
            Assert.That(json, Is.EqualTo("{\"color\":\"#EE82EE\"}"));
        }

        [Test]
        public void Scalar_Write_HandlesNull()
        {
            var json = JsonConvert.SerializeObject(new ColorProbe { Color = null });
            Assert.That(json, Is.EqualTo("{\"color\":null}"));
        }

        /// <summary>
        /// Read then write must land on hex and stay there: a scene persisted with
        /// "violet" comes back as "#EE82EE" and republishes as "#EE82EE".
        /// </summary>
        [Test]
        public void Scalar_RoundTrip_NormalizesOnceThenIsStable()
        {
            var first = JsonConvert.DeserializeObject<ColorProbe>("{\"color\":\"violet\"}");
            var written = JsonConvert.SerializeObject(first);
            Assert.That(written, Is.EqualTo("{\"color\":\"#EE82EE\"}"));

            var second = JsonConvert.DeserializeObject<ColorProbe>(written);
            Assert.That(JsonConvert.SerializeObject(second), Is.EqualTo(written));
        }

        // -------------------------------------------------------- array converter

        [Test]
        public void Array_Read_NormalizesEveryElement()
        {
            var probe = JsonConvert.DeserializeObject<ColorArrayProbe>(
                "{\"colors\":[\"violet\",\"RED\",\"#123456\",\"notacolor\"]}");

            Assert.That(probe.Colors, Is.EqualTo(new[] { "#EE82EE", "#FF0000", "#123456", "notacolor" }));
        }

        [Test]
        public void Array_Read_EmptyArray_StaysEmpty()
        {
            var probe = JsonConvert.DeserializeObject<ColorArrayProbe>("{\"colors\":[]}");
            Assert.That(probe.Colors, Is.Not.Null);
            Assert.That(probe.Colors, Is.Empty);
        }

        /// <summary>
        /// A bare string where an array is expected is legacy-tolerated and becomes a
        /// single-element array. This is the compatibility path that lets an older
        /// scene document with `"colors": "red"` still load.
        /// </summary>
        [Test]
        public void Array_Read_BareString_BecomesSingleElementArray()
        {
            var probe = JsonConvert.DeserializeObject<ColorArrayProbe>("{\"colors\":\"violet\"}");
            Assert.That(probe.Colors, Is.EqualTo(new[] { "#EE82EE" }));
        }

        [Test]
        public void Array_Read_Null_StaysNull()
        {
            var probe = JsonConvert.DeserializeObject<ColorArrayProbe>("{\"colors\":null}");
            Assert.That(probe.Colors, Is.Null);
        }

        [Test]
        public void Array_Write_EmitsElementsVerbatim()
        {
            var json = JsonConvert.SerializeObject(
                new ColorArrayProbe { Colors = new[] { "#EE82EE", "#FF0000" } });
            Assert.That(json, Is.EqualTo("{\"colors\":[\"#EE82EE\",\"#FF0000\"]}"));
        }

        [Test]
        public void Array_RoundTrip_IsStable()
        {
            var first = JsonConvert.DeserializeObject<ColorArrayProbe>("{\"colors\":[\"violet\",\"red\"]}");
            var written = JsonConvert.SerializeObject(first);
            var second = JsonConvert.DeserializeObject<ColorArrayProbe>(written);
            Assert.That(JsonConvert.SerializeObject(second), Is.EqualTo(written));
        }

        /// <summary>
        /// PINS CURRENT BEHAVIOUR (bug): ArenaColorArrayJsonConverter.WriteJson at
        /// Runtime/Schemas/Converter/ArenaColorJsonConverter.cs:274-280 iterates
        /// `value` with no null guard, so serializing a null string[] throws
        /// NullReferenceException (wrapped by Newtonsoft in a JsonSerializationException
        /// when reached through a member).
        ///
        /// ReadJson at line 249-250 legitimately returns null for a JSON null, so the
        /// converter can be handed back exactly what it cannot write. In practice
        /// Newtonsoft short-circuits null *member* values without calling the converter
        /// (see the test below), which hides the fault on the common path - but the
        /// sibling scalar ArenaColorJsonConverter.WriteJson does guard null (line
        /// 235-238), so this remains an inconsistency rather than a design choice.
        ///
        /// CORRECT BEHAVIOUR would be to emit a JSON null, matching the scalar
        /// converter and matching what ReadJson can produce:
        ///     if (value == null) { writer.WriteNull(); return; }
        /// When that fix lands, flip these two tests to assert
        /// `{"colors":null}` and no throw.
        /// </summary>
        [Test]
        public void Array_Write_Null_Throws_PinsCurrentBuggyBehaviour()
        {
            Assert.Throws<NullReferenceException>(() =>
                new ArenaColorArrayJsonConverter().WriteJson(
                    new JsonTextWriter(new System.IO.StringWriter()),
                    null,
                    JsonSerializer.CreateDefault()));
        }

        [Test]
        public void Array_ReadNullThenWrite_DoesNotThrow_BecauseNewtonsoftShortCircuitsMemberNulls()
        {
            // Documents the mitigation that makes the bug above hard to hit in
            // practice: for a null member value Newtonsoft writes JSON null itself and
            // never calls the converter, so the ordinary read-then-write round trip of
            // `{"colors": null}` survives. The converter is still wrong; it is simply
            // only reachable by invoking it directly, or from a context where
            // Newtonsoft does dispatch a null (e.g. a null element inside a container
            // typed to string[]). If the null guard is added, this test keeps passing.
            var probe = JsonConvert.DeserializeObject<ColorArrayProbe>("{\"colors\":null}");
            Assert.That(probe.Colors, Is.Null, "precondition: ReadJson returns null");
            Assert.That(JsonConvert.SerializeObject(probe), Is.EqualTo("{\"colors\":null}"));
        }
    }
}
