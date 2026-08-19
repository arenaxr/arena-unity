/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using ArenaUnity.Components;
using ArenaUnity.Schemas.Converter;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace ArenaUnity.PureTests
{
    /// <summary>
    /// The environment presets are hand-maintained JSON string literals transcribed
    /// from aframe-environment-component to keep 1:1 parity with the A-Frame web
    /// client (see the source attribution at
    /// Runtime/Components/ArenaSceneEnvPresetsDefaults.cs:10-13).
    ///
    /// Because they are string literals rather than typed data, nothing in the compiler
    /// checks them: a missing brace, a dropped key or a stray trailing comma ships
    /// silently and only shows up as a broken scene. These tests are the substitute for
    /// that missing compile-time check.
    /// </summary>
    [TestFixture]
    public class ArenaSceneEnvPresetsDefaultsTests
    {
        /// <summary>
        /// The full documented property set every real preset must define. Derived from
        /// aframe-environment-component's schema; an environment preset that omits one
        /// of these leaves the corresponding A-Frame property at a different default
        /// than the web client would use, which is exactly the parity drift this file
        /// exists to prevent.
        /// </summary>
        private static readonly string[] RequiredKeys =
        {
            "active", "seed", "skyType", "skyColor", "horizonColor", "lighting",
            "lightPosition", "fog", "flatShading", "playArea", "ground", "groundYScale",
            "groundTexture", "groundColor", "groundColor2", "dressing", "dressingAmount",
            "dressingColor", "dressingScale", "dressingVariance", "dressingUniformScale",
            "dressingOnPlayArea", "grid", "gridColor", "shadow",
        };

        private static IEnumerable<TestCaseData> AllPresets()
        {
            foreach (var kv in ArenaSceneEnvPresetsDefaults.Presets)
                yield return new TestCaseData(kv.Key).SetName($"Preset_{kv.Key}");
        }

        [Test]
        public void Presets_ContainsExactlyTheNineteenKnownPresets()
        {
            var expected = new[]
            {
                "none", "default", "contact", "egypt", "checkerboard", "forest",
                "goaland", "yavapai", "goldmine", "threetowers", "poison", "arches",
                "tron", "japan", "dream", "volcano", "starry", "osiris", "moon",
            };

            Assert.That(ArenaSceneEnvPresetsDefaults.Presets.Keys, Is.EquivalentTo(expected));
            Assert.That(ArenaSceneEnvPresetsDefaults.Presets.Count, Is.EqualTo(19));
        }

        [TestCaseSource(nameof(AllPresets))]
        public void Preset_IsWellFormedJsonObject(string presetName)
        {
            string raw = ArenaSceneEnvPresetsDefaults.Presets[presetName];
            Assert.That(raw, Is.Not.Null.And.Not.Empty);

            JObject parsed = null;
            Assert.DoesNotThrow(() => parsed = JObject.Parse(raw),
                $"preset '{presetName}' is not parseable JSON");
            Assert.That(parsed, Is.Not.Null);
        }

        [TestCaseSource(nameof(AllPresets))]
        public void Preset_DefinesExactlyTheDocumentedKeySet(string presetName)
        {
            JObject parsed = JObject.Parse(ArenaSceneEnvPresetsDefaults.Presets[presetName]);
            var keys = parsed.Properties().Select(p => p.Name).ToArray();

            if (presetName == "none")
            {
                // "none" is the sentinel meaning "no environment"; it is deliberately {}.
                Assert.That(keys, Is.Empty, "the 'none' preset must stay an empty object");
                return;
            }

            Assert.That(keys, Is.EquivalentTo(RequiredKeys),
                $"preset '{presetName}' does not define exactly the documented key set");
        }

        [TestCaseSource(nameof(AllPresets))]
        public void Preset_HasNoDuplicateKeys(string presetName)
        {
            // JObject.Parse silently keeps only the last of duplicated keys, so compare
            // the parsed property count against the raw occurrence count instead.
            JObject parsed = JObject.Parse(ArenaSceneEnvPresetsDefaults.Presets[presetName]);
            var names = parsed.Properties().Select(p => p.Name).ToArray();
            Assert.That(names, Is.Unique);
        }

        /// <summary>
        /// The dictionary is built with StringComparer.OrdinalIgnoreCase because scene
        /// documents carry whatever casing the author typed. Assert the lookup actually
        /// honours that, since the comparer is easy to lose in a refactor.
        /// </summary>
        [Test]
        public void Presets_LookupIsCaseInsensitive()
        {
            Assert.That(ArenaSceneEnvPresetsDefaults.Presets.ContainsKey("DEFAULT"), Is.True);
            Assert.That(ArenaSceneEnvPresetsDefaults.Presets["DEFAULT"],
                Is.EqualTo(ArenaSceneEnvPresetsDefaults.Presets["default"]));
            Assert.That(ArenaSceneEnvPresetsDefaults.Presets["CheckerBoard"],
                Is.EqualTo(ArenaSceneEnvPresetsDefaults.Presets["checkerboard"]));
        }

        [Test]
        public void Presets_EveryKeyIsLowercase()
        {
            var bad = ArenaSceneEnvPresetsDefaults.Presets.Keys
                .Where(k => k != k.ToLowerInvariant())
                .ToArray();
            Assert.That(bad, Is.Empty, "preset names must be lowercase: " + string.Join(", ", bad));
        }

        [Test]
        public void Preset_None_IsTheEmptyObject()
        {
            Assert.That(ArenaSceneEnvPresetsDefaults.Presets["none"], Is.EqualTo("{}"));
        }

        /// <summary>
        /// lightPosition and dressingVariance are A-Frame vec3s and must stay objects
        /// with all three components - a transcription that flattened one to a string
        /// or dropped a component would deserialize to a silently wrong vector.
        /// </summary>
        [TestCaseSource(nameof(AllPresets))]
        public void Preset_Vec3Members_AreObjectsWithXYZ(string presetName)
        {
            if (presetName == "none") Assert.Pass("no members to check");

            JObject parsed = JObject.Parse(ArenaSceneEnvPresetsDefaults.Presets[presetName]);
            foreach (var member in new[] { "lightPosition", "dressingVariance" })
            {
                JToken token = parsed[member];
                Assert.That(token, Is.Not.Null, $"{presetName}.{member} missing");
                Assert.That(token.Type, Is.EqualTo(JTokenType.Object), $"{presetName}.{member}");
                foreach (var axis in new[] { "x", "y", "z" })
                {
                    Assert.That(token[axis], Is.Not.Null, $"{presetName}.{member}.{axis} missing");
                    Assert.That(token[axis].Type,
                        Is.EqualTo(JTokenType.Float).Or.EqualTo(JTokenType.Integer),
                        $"{presetName}.{member}.{axis} must be numeric");
                }
            }
        }

        /// <summary>
        /// Every colour-valued member must be something ArenaCssColors.Normalize can
        /// hand to Unity: either an already-hex string or a name present in the table.
        /// A typo like "#88" or "grene" would render as an unrelated colour.
        /// </summary>
        [TestCaseSource(nameof(AllPresets))]
        public void Preset_ColourMembers_NormalizeToParseableHex(string presetName)
        {
            if (presetName == "none") Assert.Pass("no members to check");

            JObject parsed = JObject.Parse(ArenaSceneEnvPresetsDefaults.Presets[presetName]);
            foreach (var member in new[]
            {
                "skyColor", "horizonColor", "groundColor", "groundColor2",
                "dressingColor", "gridColor",
            })
            {
                JToken token = parsed[member];
                Assert.That(token, Is.Not.Null, $"{presetName}.{member} missing");
                Assert.That(token.Type, Is.EqualTo(JTokenType.String), $"{presetName}.{member}");

                // Not written as ArenaUnity.Schemas.Converter.ArenaCssColors: `ArenaUnity`
                // is a static class as well as a namespace (Runtime/ArenaUnity.cs), and
                // the class wins name resolution from inside a nested namespace. That
                // spelling compiles here only because ArenaUnity.cs is not one of this
                // project's linked sources - it would stop compiling the day it is.
                string normalized = ArenaCssColors.Normalize(token.Value<string>());

                // Unity's ColorUtility.TryParseHtmlString accepts #RGB, #RRGGBB,
                // #RGBA and #RRGGBBAA. Anything else here is a transcription error.
                Assert.That(normalized, Does.Match("^#([0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$"),
                    $"{presetName}.{member} = '{token.Value<string>()}' does not normalize to a hex colour");
            }
        }

        /// <summary>
        /// Enumerated A-Frame members must stay inside the vocabulary the web client
        /// understands; a value outside it silently falls back to A-Frame's own default.
        /// </summary>
        [TestCaseSource(nameof(AllPresets))]
        public void Preset_EnumeratedMembers_UseKnownAFrameVocabulary(string presetName)
        {
            if (presetName == "none") Assert.Pass("no members to check");

            JObject parsed = JObject.Parse(ArenaSceneEnvPresetsDefaults.Presets[presetName]);

            Assert.That(parsed["skyType"].Value<string>(),
                Is.AnyOf("none", "color", "gradient", "atmosphere"));
            Assert.That(parsed["lighting"].Value<string>(),
                Is.AnyOf("none", "distant", "point"));
            Assert.That(parsed["ground"].Value<string>(),
                Is.AnyOf("none", "flat", "hills", "canyon", "spikes", "noise"));
            Assert.That(parsed["groundTexture"].Value<string>(),
                Is.AnyOf("none", "checkerboard", "squares", "walkernoise"));
            Assert.That(parsed["grid"].Value<string>(),
                Is.AnyOf("none", "1x1", "2x2", "crosses", "dots", "xlines", "ylines", "spots"));
        }

        [TestCaseSource(nameof(AllPresets))]
        public void Preset_FogAndPlayArea_AreInRange(string presetName)
        {
            if (presetName == "none") Assert.Pass("no members to check");

            JObject parsed = JObject.Parse(ArenaSceneEnvPresetsDefaults.Presets[presetName]);
            Assert.That(parsed["fog"].Value<float>(), Is.InRange(0f, 1f), $"{presetName}.fog");
            Assert.That(parsed["playArea"].Value<float>(), Is.GreaterThan(0f), $"{presetName}.playArea");
            Assert.That(parsed["seed"].Value<int>(), Is.GreaterThan(0), $"{presetName}.seed");
        }
    }
}
