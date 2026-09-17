/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System;
using ArenaUnity;
using NUnit.Framework;

namespace ArenaUnity.Tests
{
    /// <summary>
    /// ArenaMqttClient.Base64UrlDecode is used at ArenaMqttClient.cs:571, :689 and :820
    /// to read the payload segment of a JWT (id_token.Split('.')[1] and
    /// auth.token.Split('.')[1]). JWT segments are base64URL-encoded, so the URL-safe
    /// alphabet is exactly what this method has to handle.
    ///
    /// It does not. See the pinning tests below.
    ///
    /// This suite lives in the Unity EditMode assembly rather than the plain .NET one
    /// only because ArenaMqttClient derives from a MonoBehaviour; the method itself is
    /// static and touches nothing but System.Text and System.Convert.
    /// </summary>
    [TestFixture]
    public class ArenaBase64UrlDecodeTests
    {
        // A JWT payload with a non-ASCII display-name claim. Its byte sequence is one
        // whose base64 encoding needs character 62 of the alphabet, which is '+' in
        // standard base64 and '-' in base64url. Real identity providers emit exactly
        // this shape whenever a user's name is outside Latin-1.
        private const string PayloadJson =
            "{\"sub\":\"jdoe\",\"name\":\"大文字\",\"aud\":\"arena\"}";

        // The same payload in each alphabet, unpadded the way a JWT segment arrives.
        private const string StandardAlphabet =
            "eyJzdWIiOiJqZG9lIiwibmFtZSI6IuWkp+aWh+WtlyIsImF1ZCI6ImFyZW5hIn0";
        private const string UrlSafeAlphabet =
            "eyJzdWIiOiJqZG9lIiwibmFtZSI6IuWkp-aWh-WtlyIsImF1ZCI6ImFyZW5hIn0";

        // ------------------------------------------------- what does work: repadding

        /// <summary>
        /// The padding half of the method is correct: an unpadded segment of any length
        /// is restored to a multiple of four.
        /// </summary>
        [TestCase("eyJhIjoxfQ", "{\"a\":1}", TestName = "Decode_RepadsTwoEquals")]
        [TestCase("eyJhIjoxfQ==", "{\"a\":1}", TestName = "Decode_AcceptsAlreadyPaddedInput")]
        [TestCase("", "", TestName = "Decode_EmptyStringDecodesToEmptyString")]
        public void Decode_RepadsUnpaddedInput(string encoded, string expected)
        {
            Assert.That(ArenaMqttClient.Base64UrlDecode(encoded), Is.EqualTo(expected));
        }

        /// <summary>
        /// A payload that happens to use only the 62 characters common to both
        /// alphabets decodes correctly, which is why the fault below is intermittent
        /// rather than total: most tokens simply never contain '-' or '_'.
        /// </summary>
        [Test]
        public void Decode_PayloadInStandardAlphabet_Succeeds()
        {
            Assert.That(ArenaMqttClient.Base64UrlDecode(StandardAlphabet), Is.EqualTo(PayloadJson));
        }

        // ------------------------------------------------ what does not work: alphabet

        /// <summary>
        /// PINS CURRENT BEHAVIOUR (bug): Runtime/ArenaMqttClient.cs:869-873 pads the
        /// input but never translates the URL-safe alphabet back to standard base64, so
        /// '-' and '_' reach Convert.FromBase64String, which rejects them.
        ///
        /// Refs https://github.com/arenaxr/arena-unity/issues/180
        ///
        /// Every caller feeds this method a JWT payload segment, and JWTs are base64URL
        /// by specification (RFC 7519 / RFC 4648 section 5). The result is a sign-in
        /// that fails for some users and not others, depending on whether their token's
        /// bytes happen to need alphabet positions 62 or 63.
        ///
        /// CORRECT BEHAVIOUR is to map the alphabet before decoding:
        ///     string standard = base64.Replace('-', '+').Replace('_', '/');
        ///     string padded = standard.PadRight(standard.Length + (4 - standard.Length % 4) % 4, '=');
        ///     return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        ///
        /// When issue #180 is fixed, these three tests should be replaced by
        /// assertions that the URL-safe forms decode to the same value as the standard
        /// forms - the test named
        /// Decode_UrlSafePayload_ShouldEqualStandardPayload_CurrentlyIgnored below is
        /// already written for that, and only needs its [Ignore] attribute removed.
        /// </summary>
        [Test]
        public void Decode_UrlSafePayloadContainingDash_Throws_PinsCurrentBuggyBehaviour()
        {
            Assert.Throws<FormatException>(() => ArenaMqttClient.Base64UrlDecode(UrlSafeAlphabet));
        }

        /// <summary>
        /// The other half of the URL-safe alphabet. "__8" is base64url for the two bytes
        /// 0xFF 0xFF, whose standard base64 form is "//8=" - the shortest input that
        /// exercises alphabet position 63.
        ///
        /// Refs https://github.com/arenaxr/arena-unity/issues/180
        /// </summary>
        [Test]
        public void Decode_UrlSafeInputContainingUnderscore_Throws_PinsCurrentBuggyBehaviour()
        {
            Assert.Throws<FormatException>(() => ArenaMqttClient.Base64UrlDecode("__8"));
        }

        /// <summary>
        /// The assertion the fix for issue #180 should make pass. Left in place, and
        /// ignored, so that whoever fixes the bug finds a ready-made regression test
        /// instead of having to invent the vectors again.
        /// </summary>
        [Test]
        [Ignore("Enable when https://github.com/arenaxr/arena-unity/issues/180 is fixed: Base64UrlDecode must map '-' to '+' and '_' to '/'.")]
        public void Decode_UrlSafePayload_ShouldEqualStandardPayload_CurrentlyIgnored()
        {
            Assert.That(ArenaMqttClient.Base64UrlDecode(UrlSafeAlphabet), Is.EqualTo(PayloadJson));
            Assert.That(ArenaMqttClient.Base64UrlDecode(UrlSafeAlphabet),
                Is.EqualTo(ArenaMqttClient.Base64UrlDecode(StandardAlphabet)));
        }

        /// <summary>
        /// Documents the null precondition: the method dereferences base64.Length
        /// immediately, so a missing token segment surfaces as a NullReferenceException
        /// rather than a handled error. Callers reach this via Split('.')[1], which
        /// throws its own IndexOutOfRangeException for a malformed token, so neither
        /// path is currently guarded.
        /// </summary>
        [Test]
        public void Decode_Null_ThrowsNullReference_PinsCurrentBehaviour()
        {
            Assert.Throws<NullReferenceException>(() => ArenaMqttClient.Base64UrlDecode(null));
        }
    }
}
