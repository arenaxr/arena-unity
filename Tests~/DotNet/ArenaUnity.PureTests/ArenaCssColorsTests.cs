/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System;
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
        /// The complete CSS Color Module Level 4 named-colour table, as an independent
        /// oracle for the 148 entries in ArenaCssColors.NameToHex. The tests above only
        /// check the table's shape - count, hex format, key casing, synonym agreement and
        /// a handful of spot values - so before this existed a single mistyped digit in
        /// any of the other 131 entries shipped silently and rendered an ARENA scene in
        /// the wrong colour.
        ///
        /// Transcribed from the CSS named-colour list (the same list MDN publishes and
        /// the class comment cites), cross-checked against the `color-name` reference
        /// table at https://github.com/colorjs/color-name. Values are frozen by the CSS
        /// spec: if this table and NameToHex ever disagree, NameToHex is the side that is
        /// wrong.
        /// </summary>
        private const string CssLevel4NamedColours =
            "aliceblue=#F0F8FF, antiquewhite=#FAEBD7, aqua=#00FFFF, aquamarine=#7FFFD4," +
            "azure=#F0FFFF, beige=#F5F5DC, bisque=#FFE4C4, black=#000000," +
            "blanchedalmond=#FFEBCD, blue=#0000FF, blueviolet=#8A2BE2, brown=#A52A2A," +
            "burlywood=#DEB887, cadetblue=#5F9EA0, chartreuse=#7FFF00, chocolate=#D2691E," +
            "coral=#FF7F50, cornflowerblue=#6495ED, cornsilk=#FFF8DC, crimson=#DC143C," +
            "cyan=#00FFFF, darkblue=#00008B, darkcyan=#008B8B, darkgoldenrod=#B8860B," +
            "darkgray=#A9A9A9, darkgreen=#006400, darkgrey=#A9A9A9, darkkhaki=#BDB76B," +
            "darkmagenta=#8B008B, darkolivegreen=#556B2F, darkorange=#FF8C00, darkorchid=#9932CC," +
            "darkred=#8B0000, darksalmon=#E9967A, darkseagreen=#8FBC8F, darkslateblue=#483D8B," +
            "darkslategray=#2F4F4F, darkslategrey=#2F4F4F, darkturquoise=#00CED1, darkviolet=#9400D3," +
            "deeppink=#FF1493, deepskyblue=#00BFFF, dimgray=#696969, dimgrey=#696969," +
            "dodgerblue=#1E90FF, firebrick=#B22222, floralwhite=#FFFAF0, forestgreen=#228B22," +
            "fuchsia=#FF00FF, gainsboro=#DCDCDC, ghostwhite=#F8F8FF, gold=#FFD700," +
            "goldenrod=#DAA520, gray=#808080, green=#008000, greenyellow=#ADFF2F," +
            "grey=#808080, honeydew=#F0FFF0, hotpink=#FF69B4, indianred=#CD5C5C," +
            "indigo=#4B0082, ivory=#FFFFF0, khaki=#F0E68C, lavender=#E6E6FA," +
            "lavenderblush=#FFF0F5, lawngreen=#7CFC00, lemonchiffon=#FFFACD, lightblue=#ADD8E6," +
            "lightcoral=#F08080, lightcyan=#E0FFFF, lightgoldenrodyellow=#FAFAD2, lightgray=#D3D3D3," +
            "lightgreen=#90EE90, lightgrey=#D3D3D3, lightpink=#FFB6C1, lightsalmon=#FFA07A," +
            "lightseagreen=#20B2AA, lightskyblue=#87CEFA, lightslategray=#778899, lightslategrey=#778899," +
            "lightsteelblue=#B0C4DE, lightyellow=#FFFFE0, lime=#00FF00, limegreen=#32CD32," +
            "linen=#FAF0E6, magenta=#FF00FF, maroon=#800000, mediumaquamarine=#66CDAA," +
            "mediumblue=#0000CD, mediumorchid=#BA55D3, mediumpurple=#9370DB, mediumseagreen=#3CB371," +
            "mediumslateblue=#7B68EE, mediumspringgreen=#00FA9A, mediumturquoise=#48D1CC, mediumvioletred=#C71585," +
            "midnightblue=#191970, mintcream=#F5FFFA, mistyrose=#FFE4E1, moccasin=#FFE4B5," +
            "navajowhite=#FFDEAD, navy=#000080, oldlace=#FDF5E6, olive=#808000," +
            "olivedrab=#6B8E23, orange=#FFA500, orangered=#FF4500, orchid=#DA70D6," +
            "palegoldenrod=#EEE8AA, palegreen=#98FB98, paleturquoise=#AFEEEE, palevioletred=#DB7093," +
            "papayawhip=#FFEFD5, peachpuff=#FFDAB9, peru=#CD853F, pink=#FFC0CB," +
            "plum=#DDA0DD, powderblue=#B0E0E6, purple=#800080, rebeccapurple=#663399," +
            "red=#FF0000, rosybrown=#BC8F8F, royalblue=#4169E1, saddlebrown=#8B4513," +
            "salmon=#FA8072, sandybrown=#F4A460, seagreen=#2E8B57, seashell=#FFF5EE," +
            "sienna=#A0522D, silver=#C0C0C0, skyblue=#87CEEB, slateblue=#6A5ACD," +
            "slategray=#708090, slategrey=#708090, snow=#FFFAFA, springgreen=#00FF7F," +
            "steelblue=#4682B4, tan=#D2B48C, teal=#008080, thistle=#D8BFD8," +
            "tomato=#FF6347, turquoise=#40E0D0, violet=#EE82EE, wheat=#F5DEB3," +
            "white=#FFFFFF, whitesmoke=#F5F5F5, yellow=#FFFF00, yellowgreen=#9ACD32";

        private static Dictionary<string, string> ExpectedNameToHex()
        {
            var expected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string entry in CssLevel4NamedColours.Split(','))
            {
                string[] parts = entry.Trim().Split('=');
                expected.Add(parts[0], parts[1]);
            }
            return expected;
        }

        [Test]
        public void NameToHex_MatchesTheCssLevel4NamedColourTableEntryForEntry()
        {
            Dictionary<string, string> expected = ExpectedNameToHex();

            Assert.That(expected.Count, Is.EqualTo(148),
                "the expectation table itself must have 148 entries");
            Assert.That(ArenaCssColors.NameToHex.Keys, Is.EquivalentTo(expected.Keys),
                "NameToHex and the CSS named-colour list disagree on which names exist");

            var wrong = ArenaCssColors.NameToHex
                .Where(kv => expected.ContainsKey(kv.Key) &&
                             !string.Equals(kv.Value, expected[kv.Key], StringComparison.OrdinalIgnoreCase))
                .Select(kv => $"{kv.Key}={kv.Value} (css says {expected[kv.Key]})")
                .ToArray();

            Assert.That(wrong, Is.Empty, "wrong hex values: " + string.Join(", ", wrong));
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
