/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Globalization;
using System.IO;
using ArenaUnity.Schemas.Converter;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ArenaUnity.PureTests
{
    /// <summary>
    /// The two legacy-tolerance converters. Both exist so that scene documents written
    /// by older clients - where a component was a bare `true` or a bare `"1 2 3"`
    /// rather than an object - still load. Their acceptance matrix is the compatibility
    /// contract, so it is spelled out one token type at a time.
    ///
    /// These call ReadJson directly rather than through a DTO, because several rows of
    /// the matrix (notably the existingValue fallthrough) are not otherwise reachable.
    /// </summary>
    [TestFixture]
    public class ArenaLegacyValueConverterTests
    {
        /// <summary>Stand-in for a schema component object. Deliberately local to the
        /// test project: the real DTOs pull in UnityEngine and belong to the Unity
        /// EditMode suite.</summary>
        private class Probe
        {
            [JsonProperty(PropertyName = "n")]
            public int N = 7;

            [JsonProperty(PropertyName = "s")]
            public string S = "def";
        }

        /// <summary>
        /// Positions a reader on the first token of <paramref name="json"/> and hands it
        /// to the converter, exactly as Newtonsoft would.
        /// </summary>
        private static T ReadBool<T>(string json, T existingValue = null) where T : class, new()
        {
            var converter = new ArenaBooleanObjectJsonConverter<T>();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                reader.Read();
                return converter.ReadJson(reader, typeof(T), existingValue,
                    existingValue != null, JsonSerializer.CreateDefault());
            }
        }

        private static string ReadString(string json, string existingValue = null)
        {
            var converter = new ArenaStringObjectJsonConverter();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                reader.Read();
                return converter.ReadJson(reader, typeof(string), existingValue,
                    existingValue != null, JsonSerializer.CreateDefault());
            }
        }

        private static string Write<T>(JsonConverter<T> converter, T value)
        {
            var sw = new StringWriter();
            using (var writer = new JsonTextWriter(sw))
                converter.WriteJson(writer, value, JsonSerializer.CreateDefault());
            return sw.ToString();
        }

        // ============================================ ArenaBooleanObjectJsonConverter

        [Test]
        public void Bool_Null_ReturnsNull()
        {
            Assert.That(ReadBool<Probe>("null"), Is.Null);
        }

        [Test]
        public void Bool_True_ReturnsFreshInstanceWithSchemaDefaults()
        {
            var result = ReadBool<Probe>("true");
            Assert.That(result, Is.Not.Null);
            Assert.That(result.N, Is.EqualTo(7), "defaults must be the type's own, not zeroed");
            Assert.That(result.S, Is.EqualTo("def"));
        }

        [Test]
        public void Bool_False_ReturnsNull_MeaningComponentAbsent()
        {
            Assert.That(ReadBool<Probe>("false"), Is.Null);
        }

        [TestCase("\"\"", TestName = "Bool_EmptyString_ReturnsDefaultInstance")]
        [TestCase("\"anything\"", TestName = "Bool_NonEmptyString_ReturnsDefaultInstance")]
        [TestCase("\"false\"", TestName = "Bool_StringLiteralFalse_StillReturnsDefaultInstance")]
        public void Bool_AnyString_ReturnsDefaultInstance(string json)
        {
            // Note the third case: the *string* "false" is not the *boolean* false, so
            // it enables the component. That asymmetry is what the converter's comment
            // ("any non-null string equates to a default-enabled component") intends.
            var result = ReadBool<Probe>(json);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.N, Is.EqualTo(7));
        }

        [Test]
        public void Bool_Object_DeserializesNormally()
        {
            var result = ReadBool<Probe>("{\"n\":42,\"s\":\"hi\"}");
            Assert.That(result, Is.Not.Null);
            Assert.That(result.N, Is.EqualTo(42));
            Assert.That(result.S, Is.EqualTo("hi"));
        }

        [Test]
        public void Bool_PartialObject_LeavesUnmentionedMembersAtTheirDefaults()
        {
            var result = ReadBool<Probe>("{\"n\":42}");
            Assert.That(result.N, Is.EqualTo(42));
            Assert.That(result.S, Is.EqualTo("def"));
        }

        [Test]
        public void Bool_UnhandledTokenType_FallsBackToExistingValue()
        {
            var existing = new Probe { N = 99 };
            // A number is none of null/bool/string/object, so the final
            // `return existingValue` at line 52 is taken.
            Assert.That(ReadBool("123", existing), Is.SameAs(existing));
            Assert.That(ReadBool("[1,2]", existing), Is.SameAs(existing));
        }

        [Test]
        public void Bool_UnhandledTokenType_WithNoExistingValue_ReturnsNull()
        {
            Assert.That(ReadBool<Probe>("123"), Is.Null);
        }

        [Test]
        public void Bool_Write_AlwaysEmitsTheFullObject()
        {
            var json = Write(new ArenaBooleanObjectJsonConverter<Probe>(), new Probe { N = 5, S = "x" });
            Assert.That(json, Is.EqualTo("{\"n\":5,\"s\":\"x\"}"));
        }

        [Test]
        public void Bool_Write_EmitsDefaultsRatherThanShorthandTrue()
        {
            // Asymmetric on purpose: a component read from `true` is written back as the
            // expanded object, never as `true`.
            var reread = ReadBool<Probe>("true");
            Assert.That(Write(new ArenaBooleanObjectJsonConverter<Probe>(), reread),
                Is.EqualTo("{\"n\":7,\"s\":\"def\"}"));
        }

        /// <summary>
        /// PINS CURRENT BEHAVIOUR (bug): ArenaBooleanObjectJsonConverter.WriteJson at
        /// Runtime/Schemas/Converter/ArenaBooleanObjectJsonConverter.cs:58 calls
        /// JObject.FromObject(value) with no null guard, so writing null throws
        /// ArgumentNullException - even though ReadJson at lines 26 and 31 returns null
        /// for both JSON null and boolean false.
        ///
        /// CORRECT BEHAVIOUR would be to emit JSON null (or `false`, mirroring the
        /// shorthand it accepts on read):
        ///     if (value == null) { writer.WriteNull(); return; }
        /// When that fix lands, flip this to assert the emitted token instead of the throw.
        /// </summary>
        [Test]
        public void Bool_Write_Null_Throws_PinsCurrentBuggyBehaviour()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Write(new ArenaBooleanObjectJsonConverter<Probe>(), null));
        }

        // ============================================= ArenaStringObjectJsonConverter

        [Test]
        public void String_Null_ReturnsNull()
        {
            Assert.That(ReadString("null"), Is.Null);
        }

        [TestCase("\"1 2 3\"", "1 2 3", TestName = "String_AFrameCoordinateString_PassesThrough")]
        [TestCase("\"\"", "", TestName = "String_EmptyString_PassesThrough")]
        [TestCase("\"  padded  \"", "  padded  ", TestName = "String_IsNotTrimmed")]
        public void String_StringToken_PassesThrough(string json, string expected)
        {
            Assert.That(ReadString(json), Is.EqualTo(expected));
        }

        [Test]
        public void String_Object_BecomesCompactJsonString()
        {
            Assert.That(ReadString("{ \"x\": 1, \"y\": 2, \"z\": 3 }"),
                Is.EqualTo("{\"x\":1,\"y\":2,\"z\":3}"),
                "whitespace must be stripped: Formatting.None");
        }

        [Test]
        public void String_Array_BecomesCompactJsonString()
        {
            Assert.That(ReadString("[ 1, 2, 3 ]"), Is.EqualTo("[1,2,3]"));
        }

        [Test]
        public void String_NestedObject_BecomesCompactJsonString()
        {
            Assert.That(ReadString("{\"a\":{\"b\":[1,{\"c\":2}]}}"),
                Is.EqualTo("{\"a\":{\"b\":[1,{\"c\":2}]}}"));
        }

        [Test]
        public void String_Write_EmitsTheStoredValueVerbatim()
        {
            Assert.That(Write(new ArenaStringObjectJsonConverter(), "1 2 3"), Is.EqualTo("\"1 2 3\""));
        }

        [Test]
        public void String_Write_Null_EmitsJsonNull()
        {
            Assert.That(Write(new ArenaStringObjectJsonConverter(), null), Is.EqualTo("null"));
        }

        /// <summary>
        /// PINS CURRENT BEHAVIOUR (bug): a JSON boolean becomes the .NET
        /// Boolean.ToString() spelling "True"/"False", capital first letter, because
        /// Runtime/Schemas/Converter/ArenaStringObjectJsonConverter.cs:36 does
        /// `reader.Value?.ToString()`.
        ///
        /// These values are animation to/from properties published over MQTT to the
        /// A-Frame web client, which expects JavaScript-style lowercase "true"/"false".
        /// CORRECT BEHAVIOUR would lowercase booleans (and use InvariantCulture for all
        /// primitives), e.g.
        ///     return Convert.ToString(reader.Value, CultureInfo.InvariantCulture)
        ///            is string s && reader.Value is bool ? s.ToLowerInvariant() : s;
        /// When fixed, change the expectations here to "true"/"false".
        /// </summary>
        [TestCase("true", "True")]
        [TestCase("false", "False")]
        public void String_BooleanToken_UsesDotNetCasing_PinsCurrentBuggyBehaviour(string json, string expected)
        {
            Assert.That(ReadString(json), Is.EqualTo(expected));
        }

        [Test]
        public void String_NumberToken_UnderInvariantCulture_IsDotSeparated()
        {
            using (new CultureScope(CultureInfo.InvariantCulture))
            {
                Assert.That(ReadString("1.5"), Is.EqualTo("1.5"));
                Assert.That(ReadString("42"), Is.EqualTo("42"));
            }
        }

        /// <summary>
        /// PINS CURRENT BEHAVIOUR (bug): under a comma-decimal locale the numeric branch
        /// at Runtime/Schemas/Converter/ArenaStringObjectJsonConverter.cs:36 emits
        /// "1,5" instead of "1.5", because `reader.Value?.ToString()` formats with
        /// CurrentCulture. The resulting animation to/from value is unparseable by the
        /// A-Frame web client on the other end of the MQTT topic.
        ///
        /// CORRECT BEHAVIOUR:
        ///     return Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
        /// When fixed, this test should assert "1.5" under de-DE as well - i.e. delete
        /// the culture dependence from the expectation, not the test.
        ///
        /// The culture is pinned explicitly here rather than inherited, so the result
        /// does not depend on the CI runner's locale.
        /// </summary>
        [Test]
        public void String_NumberToken_UnderCommaDecimalLocale_IsCommaSeparated_PinsCurrentBuggyBehaviour()
        {
            using (new CultureScope(new CultureInfo("de-DE")))
            {
                Assert.That(ReadString("1.5"), Is.EqualTo("1,5"));
            }
        }

        /// <summary>Swaps CurrentCulture for the duration of a block and restores it.</summary>
        private sealed class CultureScope : IDisposable
        {
            private readonly CultureInfo previous;

            public CultureScope(CultureInfo culture)
            {
                previous = CultureInfo.CurrentCulture;
                CultureInfo.CurrentCulture = culture;
            }

            public void Dispose()
            {
                CultureInfo.CurrentCulture = previous;
            }
        }
    }
}
