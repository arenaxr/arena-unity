/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine.TestTools;

namespace ArenaUnity.Tests
{
    /// <summary>
    /// Runs the envelope layer against the eight real persisted-scene dumps in
    /// Tests/JSON Scenes. These are captures of the arena-persist REST response, so they
    /// are the closest thing the repository has to production input, and they cost
    /// almost nothing to assert against.
    ///
    /// Three of them (material, meeting, particles) use the legacy "attributes" member,
    /// so this suite also exercises the ArenaMessageJson migration on real data rather
    /// than on hand-written fixtures.
    /// </summary>
    [TestFixture]
    public class ArenaSceneCorpusTests
    {
        private static readonly string[] SceneFiles =
        {
            "arena.json", "asdf.json", "example.json", "lobby.json",
            "material.json", "meeting.json", "particles.json", "render.json",
        };

        /// <summary>
        /// Resolves Tests/JSON Scenes inside whichever project has this package added.
        /// PackageInfo.FindForAssembly is the supported way to turn an assembly into an
        /// on-disk package path, and works for a package added from disk, from git, or
        /// from the registry cache.
        /// </summary>
        private static string SceneDirectory()
        {
            PackageInfo package = PackageInfo.FindForAssembly(typeof(ArenaSceneCorpusTests).Assembly);
            if (package == null)
                Assert.Ignore("Could not resolve the ARENA package path; is this assembly inside the package?");

            return Path.Combine(package.resolvedPath, "Tests", "JSON Scenes");
        }

        private static IEnumerable<TestCaseData> SceneCases()
        {
            foreach (string file in SceneFiles)
                yield return new TestCaseData(file).SetName("Scene_" + file.Replace('.', '_'));
        }

        private static string ReadScene(string fileName)
        {
            string path = Path.Combine(SceneDirectory(), fileName);
            if (!File.Exists(path))
                Assert.Fail($"corpus fixture missing: {path}");
            return File.ReadAllText(path);
        }

        [Test]
        public void Corpus_HasAllEightFixtures()
        {
            string dir = SceneDirectory();
            foreach (string file in SceneFiles)
                Assert.That(File.Exists(Path.Combine(dir, file)), Is.True, file);
        }

        [TestCaseSource(nameof(SceneCases))]
        public void Scene_IsAJsonArrayOfEnvelopes(string fileName)
        {
            JArray parsed = JArray.Parse(ReadScene(fileName));

            Assert.That(parsed.Count, Is.GreaterThan(0), "a scene dump must not be empty");
            foreach (JToken entry in parsed)
                Assert.That(entry.Type, Is.EqualTo(JTokenType.Object));
        }

        /// <summary>
        /// The load path every scene takes: deserialize the persist array into typed
        /// envelopes. ArenaMessageJson's [OnError] handler downgrades any binding failure
        /// to a Debug.LogWarning and swallows it, so a silent schema mismatch would
        /// otherwise leave no trace - hence the log assertion.
        /// </summary>
        [TestCaseSource(nameof(SceneCases))]
        public void Scene_DeserializesIntoEnvelopesWithoutLoggingWarnings(string fileName)
        {
            var messages = JsonConvert.DeserializeObject<List<ArenaMessageJson>>(ReadScene(fileName));

            Assert.That(messages, Is.Not.Null);
            Assert.That(messages.Count, Is.GreaterThan(0));

            // Any unexpected Debug.LogWarning from [OnError] surfaces here.
            LogAssert.NoUnexpectedReceived();
        }

        [TestCaseSource(nameof(SceneCases))]
        public void Scene_EveryEnvelopeHasAnObjectId(string fileName)
        {
            var messages = JsonConvert.DeserializeObject<List<ArenaMessageJson>>(ReadScene(fileName));

            foreach (ArenaMessageJson message in messages)
                Assert.That(message.object_id, Is.Not.Null.And.Not.Empty);
        }

        /// <summary>
        /// Whether a fixture used "data" or the legacy "attributes", every envelope must
        /// end up with a populated data member after deserialization.
        /// </summary>
        [TestCaseSource(nameof(SceneCases))]
        public void Scene_EveryEnvelopeEndsUpWithData(string fileName)
        {
            var messages = JsonConvert.DeserializeObject<List<ArenaMessageJson>>(ReadScene(fileName));

            foreach (ArenaMessageJson message in messages)
                Assert.That(message.data, Is.Not.Null, message.object_id);
        }

        /// <summary>
        /// Re-serialization must be idempotent: serialize, read back, serialize again,
        /// and get the same bytes. This is what makes it safe to round trip a persisted
        /// scene through the client and back to the persistence store.
        /// </summary>
        [TestCaseSource(nameof(SceneCases))]
        public void Scene_ReserializationIsStable(string fileName)
        {
            var first = JsonConvert.DeserializeObject<List<ArenaMessageJson>>(ReadScene(fileName));
            string once = JsonConvert.SerializeObject(first);

            var second = JsonConvert.DeserializeObject<List<ArenaMessageJson>>(once);
            string twice = JsonConvert.SerializeObject(second);

            Assert.That(twice, Is.EqualTo(once), "serialization must reach a fixed point");
        }

        /// <summary>
        /// After a round trip the legacy member must be gone everywhere, on every fixture
        /// that used it. Asserted on the whole corpus rather than per-file so the three
        /// legacy fixtures cannot quietly stop being legacy.
        /// </summary>
        [TestCaseSource(nameof(SceneCases))]
        public void Scene_ReserializationNeverEmitsLegacyAttributes(string fileName)
        {
            var messages = JsonConvert.DeserializeObject<List<ArenaMessageJson>>(ReadScene(fileName));
            JArray reserialized = JArray.Parse(JsonConvert.SerializeObject(messages));

            foreach (JToken entry in reserialized)
                Assert.That(entry["attributes"], Is.Null,
                    "attributes must have been migrated into data");
        }

        /// <summary>
        /// Confirms the corpus still contains legacy-shaped input. If someone
        /// "modernizes" the fixtures, the migration path silently stops being covered -
        /// this is the test that notices.
        /// </summary>
        [Test]
        public void Corpus_StillContainsLegacyAttributesFixtures()
        {
            var legacy = SceneFiles
                .Where(f => JArray.Parse(ReadScene(f)).Any(e => e["attributes"] != null))
                .ToArray();

            Assert.That(legacy, Is.EquivalentTo(new[] { "material.json", "meeting.json", "particles.json" }),
                "the legacy-attributes fixtures are what cover ArenaMessageJson.OnDeserialized");
        }

        /// <summary>
        /// Every object_type present in the corpus should be either an A-Frame primitive
        /// this package can build or one of the known non-primitive types. A new type
        /// appearing in a fixture without a matching renderer is worth knowing about.
        /// </summary>
        [Test]
        public void Corpus_ObjectTypesAreAllRecognized()
        {
            // The exact set of non-primitive object_type values present in the corpus
            // today. Kept as a closed list rather than an open "anything goes" check so
            // that a new type appearing in a fixture is a deliberate decision.
            //
            // NOTE: "prism" (meeting.json) is NOT handled anywhere in this package - it
            // is neither in ArenaUnity.primitives nor matched by any ApplyRender path.
            // It is listed here to keep this test honest about what the corpus contains;
            // see the report for the follow-up.
            var knownNonPrimitives = new HashSet<string>
            {
                "arenaui-button-panel", "arenaui-card", "arenaui-prompt", "entity",
                "gaussian_splatting", "gltf-model", "image", "light", "pcd-model",
                "prism", "text", "thickline", "threejs-scene", "urdf-model",
            };

            var unknown = new SortedSet<string>();
            foreach (string file in SceneFiles)
            {
                foreach (JToken entry in JArray.Parse(ReadScene(file)))
                {
                    JToken data = entry["data"] ?? entry["attributes"];
                    string objectType = data?["object_type"]?.Value<string>();
                    if (string.IsNullOrEmpty(objectType)) continue;

                    if (!ArenaUnity.primitives.Contains(objectType) &&
                        !knownNonPrimitives.Contains(objectType))
                    {
                        unknown.Add(objectType);
                    }
                }
            }

            Assert.That(unknown, Is.Empty,
                "unrecognized object_type values in the corpus: " + string.Join(", ", unknown));
        }
    }
}
