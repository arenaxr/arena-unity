/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas.Converter;
using NUnit.Framework;

namespace ArenaUnity.PureTests
{
    /// <summary>
    /// ArenaCssColors is the bridge between A-Frame's CSS colour names and the hex
    /// strings Unity's ColorUtility.TryParseHtmlString accepts, so both the lookup
    /// table's integrity and Normalize's passthrough rules are wire contracts.
    /// </summary>
    [TestFixture]
    public class ArenaCssColorsTests
    {
        private static readonly Regex SixDigitHex = new Regex("^#[0-9A-Fa-f]{6}$");

        // ---------------------------------------------------------------- Normalize

        [TestCase("violet", "#EE82EE", TestName = "Normalize_ExactNameHit")]
        [TestCase("VIOLET", "#EE82EE", TestName = "Normalize_IsCaseInsensitive")]
        [TestCase("Violet", "#EE82EE", TestName = "Normalize_IsCaseInsensitive_MixedCase")]
        [TestCase("  violet  ", "#EE82EE", TestName = "Normalize_TrimsSurroundingWhitespace")]
        [TestCase("\tviolet\n", "#EE82EE", TestName = "Normalize_TrimsTabsAndNewlines")]
        [TestCase("#abc", "#abc", TestName = "Normalize_PassesShortHexThrough")]
        [TestCase("#AABBCC", "#AABBCC", TestName = "Normalize_PassesLongHexThroughUnchanged")]
        [TestCase("  #AABBCC  ", "#AABBCC", TestName = "Normalize_TrimsThenPassesHexThrough")]
        [TestCase("rgb(1,2,3)", "rgb(1,2,3)", TestName = "Normalize_PassesRgbThrough")]
        [TestCase("rgba(1,2,3,0.5)", "rgba(1,2,3,0.5)", TestName = "Normalize_PassesRgbaThrough")]
        [TestCase("notacolor", "notacolor", TestName = "Normalize_PassesUnknownNameThrough")]
        [TestCase("  notacolor  ", "notacolor", TestName = "Normalize_TrimsUnknownName")]
        [TestCase("", "", TestName = "Normalize_PassesEmptyThrough")]
        [TestCase("   ", "", TestName = "Normalize_WhitespaceOnlyBecomesEmpty")]
        public void Normalize_Cases(string input, string expected)
        {
            Assert.That(ArenaCssColors.Normalize(input), Is.EqualTo(expected));
        }

        [Test]
        public void Normalize_Null_ReturnsNull()
        {
            Assert.That(ArenaCssColors.Normalize(null), Is.Null);
        }

        /// <summary>
        /// "rebeccapurple" is in the table but also starts with "r" - it must not be
        /// mistaken for the "rgb" prefix check, which uses StartsWith("rgb").
        /// </summary>
        [Test]
        public void Normalize_NameStartingWithR_StillHitsTheTable()
        {
            Assert.That(ArenaCssColors.Normalize("rebeccapurple"), Is.EqualTo("#663399"));
            Assert.That(ArenaCssColors.Normalize("red"), Is.EqualTo("#FF0000"));
            Assert.That(ArenaCssColors.Normalize("royalblue"), Is.EqualTo("#4169E1"));
        }

        // ------------------------------------------------------------ Table integrity

        /// <summary>
        /// The class comment at Runtime/Schemas/Converter/ArenaColorJsonConverter.cs:20
        /// claims "All 148 CSS named colors". Assert the code matches its own comment,
        /// so a dropped or duplicated entry is caught rather than silently shipped.
        /// </summary>
        [Test]
        public void NameToHex_HasExactly148Entries_MatchingItsOwnCodeComment()
        {
            Assert.That(ArenaCssColors.NameToHex.Count, Is.EqualTo(148));
        }

        [Test]
        public void NameToHex_EveryValueIsASixDigitHexTriplet()
        {
            var bad = ArenaCssColors.NameToHex
                .Where(kv => !SixDigitHex.IsMatch(kv.Value))
                .Select(kv => $"{kv.Key}={kv.Value}")
                .ToArray();

            Assert.That(bad, Is.Empty, "entries whose value is not #RRGGBB: " + string.Join(", ", bad));
        }

        [Test]
        public void NameToHex_EveryKeyIsLowercaseAndFreeOfWhitespace()
        {
            var bad = ArenaCssColors.NameToHex.Keys
                .Where(k => k != k.ToLowerInvariant() || k.Trim() != k)
                .ToArray();

            Assert.That(bad, Is.Empty, "keys that are not trimmed lowercase: " + string.Join(", ", bad));
        }

        [Test]
        public void NameToHex_LookupIsCaseInsensitive()
        {
            Assert.That(ArenaCssColors.NameToHex.ContainsKey("VIOLET"), Is.True);
            Assert.That(ArenaCssColors.NameToHex.ContainsKey("DarkSlateGray"), Is.True);
        }

        /// <summary>
        /// CSS defines these as exact synonyms. If a hand edit changes one spelling's
        /// hex and not the other, a scene renders differently depending on which
        /// spelling the author used.
        /// </summary>
        [TestCase("gray", "grey")]
        [TestCase("darkgray", "darkgrey")]
        [TestCase("lightgray", "lightgrey")]
        [TestCase("dimgray", "dimgrey")]
        [TestCase("slategray", "slategrey")]
        [TestCase("lightslategray", "lightslategrey")]
        [TestCase("darkslategray", "darkslategrey")]
        [TestCase("fuchsia", "magenta")]
        [TestCase("aqua", "cyan")]
        public void NameToHex_SynonymPairsAgree(string a, string b)
        {
            Assert.That(ArenaCssColors.NameToHex.ContainsKey(a), Is.True, $"missing '{a}'");
            Assert.That(ArenaCssColors.NameToHex.ContainsKey(b), Is.True, $"missing '{b}'");
            Assert.That(ArenaCssColors.NameToHex[a], Is.EqualTo(ArenaCssColors.NameToHex[b]),
                $"'{a}' and '{b}' are CSS synonyms and must map to the same hex");
        }

        /// <summary>
        /// Spot-check the four CSS colours whose values are most often mistyped,
        /// pinned against the MDN named-colour table the class cites.
        /// </summary>
        [TestCase("black", "#000000")]
        [TestCase("white", "#FFFFFF")]
        [TestCase("gray", "#808080")]
        [TestCase("silver", "#C0C0C0")]
        [TestCase("lime", "#00FF00")]
        [TestCase("green", "#008000")]
        [TestCase("navy", "#000080")]
        [TestCase("teal", "#008080")]
        public void NameToHex_SpotChecksAgainstMdn(string name, string hex)
        {
            Assert.That(ArenaCssColors.NameToHex[name], Is.EqualTo(hex));
        }

        /// <summary>
        /// Every table entry must survive its own Normalize round trip: a named colour
        /// resolves to its hex, and feeding that hex back is idempotent.
        /// </summary>
        [Test]
        public void Normalize_IsIdempotentOverTheWholeTable()
        {
            foreach (KeyValuePair<string, string> kv in ArenaCssColors.NameToHex)
            {
                string once = ArenaCssColors.Normalize(kv.Key);
                Assert.That(once, Is.EqualTo(kv.Value), kv.Key);
                Assert.That(ArenaCssColors.Normalize(once), Is.EqualTo(kv.Value), kv.Key + " (twice)");
            }
        }
    }
}
