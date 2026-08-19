/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Globalization;
using ArenaUnity.Schemas;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaUnity.Tests
{
    /// <summary>
    /// The coordinate translations in Runtime/ArenaUnity.cs. CONTRIBUTING.md rule #5
    /// requires that everything crossing the MQTT boundary go through these methods, so
    /// their behaviour is the A-Frame interoperability contract: A-Frame is right-up-back
    /// (RUB) and Unity is right-up-forward (RUF), which reduces to negating Z on
    /// positions and negating X (and optionally Y) on quaternions.
    ///
    /// Needs UnityEngine for Vector3 / Quaternion / Color, hence the EditMode assembly.
    /// </summary>
    [TestFixture]
    public class ArenaUnityConversionTests
    {
        private const float Tol = 1e-3f;   // ArenaFloat rounds to 3 decimals

        // ============================================================ ArenaFloat

        [TestCase(1.23456f, 1.235f, TestName = "ArenaFloat_RoundsUpAtFourthDecimal")]
        [TestCase(-1.23456f, -1.235f, TestName = "ArenaFloat_RoundsNegativesSymmetrically")]
        [TestCase(1.2341f, 1.234f, TestName = "ArenaFloat_RoundsDown")]
        [TestCase(1f, 1f, TestName = "ArenaFloat_LeavesWholeNumbers")]
        [TestCase(0f, 0f, TestName = "ArenaFloat_Zero")]
        [TestCase(0.0004f, 0f, TestName = "ArenaFloat_BelowHalfMilliRoundsToZero")]
        [TestCase(0.0006f, 0.001f, TestName = "ArenaFloat_AboveHalfMilliRoundsToOneMilli")]
        public void ArenaFloat_RoundsToThreeDecimals(float input, float expected)
        {
            Assert.That(ArenaUnity.ArenaFloat(input), Is.EqualTo(expected).Within(1e-6f));
        }

        // ============================================================= position

        /// <summary>The single most important assertion in this file: Z is negated.</summary>
        [Test]
        public void ToArenaPosition_NegatesZOnly()
        {
            ArenaVector3Json a = ArenaUnity.ToArenaPosition(new Vector3(1f, 2f, 3f));

            Assert.That(a.X, Is.EqualTo(1f).Within(Tol));
            Assert.That(a.Y, Is.EqualTo(2f).Within(Tol));
            Assert.That(a.Z, Is.EqualTo(-3f).Within(Tol));
        }

        [Test]
        public void ToUnityPosition_NegatesZOnly()
        {
            Vector3 u = ArenaUnity.ToUnityPosition(new ArenaVector3Json { X = 1f, Y = 2f, Z = 3f });

            Assert.That(u.x, Is.EqualTo(1f).Within(Tol));
            Assert.That(u.y, Is.EqualTo(2f).Within(Tol));
            Assert.That(u.z, Is.EqualTo(-3f).Within(Tol));
        }

        [Test]
        public void ToUnityPosition_VectorOverload_NegatesZOnly()
        {
            Vector3 u = ArenaUnity.ToUnityPosition(new Vector3(1f, 2f, 3f));
            Assert.That(u, Is.EqualTo(new Vector3(1f, 2f, -3f)));
        }

        [TestCase(0f, 0f, 0f)]
        [TestCase(1f, 2f, 3f)]
        [TestCase(-4.5f, 0.25f, 100f)]
        [TestCase(0.125f, -0.5f, -0.001f)]
        public void Position_RoundTripsThroughArenaAndBack(float x, float y, float z)
        {
            var original = new Vector3(x, y, z);
            Vector3 back = ArenaUnity.ToUnityPosition(ArenaUnity.ToArenaPosition(original));

            Assert.That(back.x, Is.EqualTo(original.x).Within(Tol));
            Assert.That(back.y, Is.EqualTo(original.y).Within(Tol));
            Assert.That(back.z, Is.EqualTo(original.z).Within(Tol));
        }

        /// <summary>
        /// The array overload mutates its argument in place and returns it - a caller
        /// that expected a copy would silently corrupt a shared mesh vertex array, so the
        /// aliasing is pinned deliberately.
        /// </summary>
        [Test]
        public void ToUnityPosition_ArrayOverload_MutatesInPlaceAndReturnsSameArray()
        {
            var positions = new[] { new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f) };
            Vector3[] returned = ArenaUnity.ToUnityPosition(positions);

            Assert.That(returned, Is.SameAs(positions), "the overload returns its argument");
            Assert.That(positions[0], Is.EqualTo(new Vector3(1f, 2f, -3f)));
            Assert.That(positions[1], Is.EqualTo(new Vector3(4f, 5f, -6f)));
        }

        /// <summary>
        /// The string form is what goes into A-Frame "x y z" attribute values. The
        /// culture is pinned so the assertion does not depend on the CI runner's locale;
        /// see the note in the report about this method's locale sensitivity.
        /// </summary>
        [Test]
        public void ToArenaPositionString_NegatesZAndRoundsToThreeDecimals()
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                Assert.That(ArenaUnity.ToArenaPositionString(new Vector3(1.5f, 2f, 3f)),
                    Is.EqualTo("1.5 2 -3"));
                Assert.That(ArenaUnity.ToArenaPositionString(new Vector3(1.23456f, 0f, -0.5f)),
                    Is.EqualTo("1.235 0 0.5"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        /// <summary>
        /// ToUnityPositionString splits into at most 3 parts and indexes [1] and [2]
        /// unconditionally, so a one- or two-component string throws - unlike
        /// ArenaVector3JsonConverter, which tolerates both. Pinned as the asymmetry it is.
        /// </summary>
        [Test]
        public void ToUnityPositionString_ParsesThreeComponentsAndNegatesZ()
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                Vector3 v = ArenaUnity.ToUnityPositionString("1.5 2 3");
                Assert.That(v.x, Is.EqualTo(1.5f).Within(Tol));
                Assert.That(v.y, Is.EqualTo(2f).Within(Tol));
                Assert.That(v.z, Is.EqualTo(-3f).Within(Tol));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Test]
        public void ToUnityPositionString_TooFewComponents_Throws_PinsCurrentBehaviour()
        {
            // PINS CURRENT BEHAVIOUR: Runtime/ArenaUnity.cs:81-89 indexes axis[1] and
            // axis[2] with no length check, so a partial coordinate string throws rather
            // than defaulting the missing components to 0 the way
            // ArenaVector3JsonConverter.ParseVector3String does. If the two are ever
            // reconciled, this expectation flips to a Vector3 comparison.
            Assert.Throws<IndexOutOfRangeException>(() => ArenaUnity.ToUnityPositionString("1"));
            Assert.Throws<IndexOutOfRangeException>(() => ArenaUnity.ToUnityPositionString("1 2"));
        }

        // ============================================================= rotation

        [Test]
        public void ToArenaRotationQuat_NegatesXAndYByDefault()
        {
            var q = new Quaternion(0.1f, 0.2f, 0.3f, 0.4f);
            ArenaRotationJson a = ArenaUnity.ToArenaRotationQuat(q);

            Assert.That(a.X, Is.EqualTo(-0.1f).Within(Tol));
            Assert.That(a.Y, Is.EqualTo(-0.2f).Within(Tol));
            Assert.That(a.Z, Is.EqualTo(0.3f).Within(Tol));
            Assert.That(a.W, Is.EqualTo(0.4f).Within(Tol));
        }

        [Test]
        public void ToArenaRotationQuat_WithoutInvertY_NegatesXOnly()
        {
            var q = new Quaternion(0.1f, 0.2f, 0.3f, 0.4f);
            ArenaRotationJson a = ArenaUnity.ToArenaRotationQuat(q, invertY: false);

            Assert.That(a.X, Is.EqualTo(-0.1f).Within(Tol));
            Assert.That(a.Y, Is.EqualTo(0.2f).Within(Tol));
            Assert.That(a.Z, Is.EqualTo(0.3f).Within(Tol));
            Assert.That(a.W, Is.EqualTo(0.4f).Within(Tol));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Rotation_RoundTripsThroughArenaAndBack(bool invertY)
        {
            var original = new Quaternion(0.1f, -0.2f, 0.3f, 0.9f);
            Quaternion back = ArenaUnity.ToUnityRotationQuat(
                ArenaUnity.ToArenaRotationQuat(original, invertY), invertY);

            Assert.That(back.x, Is.EqualTo(original.x).Within(Tol));
            Assert.That(back.y, Is.EqualTo(original.y).Within(Tol));
            Assert.That(back.z, Is.EqualTo(original.z).Within(Tol));
            Assert.That(back.w, Is.EqualTo(original.w).Within(Tol));
        }

        /// <summary>
        /// The Unity-to-Unity overload is its own inverse: applying it twice restores the
        /// original, because negating a component twice is the identity.
        /// </summary>
        [Test]
        public void ToUnityRotationQuat_QuaternionOverload_IsItsOwnInverse()
        {
            var original = new Quaternion(0.1f, -0.2f, 0.3f, 0.9f);
            Quaternion twice = ArenaUnity.ToUnityRotationQuat(
                ArenaUnity.ToUnityRotationQuat(original));

            Assert.That(twice.x, Is.EqualTo(original.x).Within(1e-5f));
            Assert.That(twice.y, Is.EqualTo(original.y).Within(1e-5f));
            Assert.That(twice.z, Is.EqualTo(original.z).Within(1e-5f));
            Assert.That(twice.w, Is.EqualTo(original.w).Within(1e-5f));
        }

        /// <summary>
        /// The GLTF spin is a 180 degree yaw, applied via eulerAngles. Only yaw-only
        /// rotations are asserted here: eulerAngles renormalizes its output, so a
        /// round trip through it is not the identity for arbitrary orientations. See the
        /// report for what was deliberately left untested.
        /// </summary>
        [Test]
        public void UnityToGltfRotationQuat_AddsOneHundredEightyDegreesOfYaw()
        {
            Assert.That(Quaternion.Angle(
                    ArenaUnity.UnityToGltfRotationQuat(Quaternion.identity),
                    Quaternion.Euler(0f, 180f, 0f)),
                Is.LessThan(0.01f));
        }

        [TestCase(0f)]
        [TestCase(90f)]
        [TestCase(180f)]
        [TestCase(270f)]
        public void GltfRotation_RoundTripsForYawOnlyRotations(float yaw)
        {
            var original = Quaternion.Euler(0f, yaw, 0f);
            Quaternion back = ArenaUnity.GltfToUnityRotationQuat(
                ArenaUnity.UnityToGltfRotationQuat(original));

            Assert.That(Quaternion.Angle(original, back), Is.LessThan(0.01f));
        }

        [Test]
        public void ToUnityRotationEuler_OfZeroAngles_IsIdentity()
        {
            Quaternion q = ArenaUnity.ToUnityRotationEuler(
                new ArenaRotationJson { X = 0f, Y = 0f, Z = 0f, W = 1f });

            Assert.That(Quaternion.Angle(q, Quaternion.identity), Is.LessThan(0.01f));
        }

        [Test]
        public void ToArenaRotationEuler_OfIdentity_IsZeroAngles()
        {
            Vector3 euler = ArenaUnity.ToArenaRotationEuler(Quaternion.identity);

            Assert.That(euler.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(euler.y, Is.EqualTo(0f).Within(0.01f));
            Assert.That(euler.z, Is.EqualTo(0f).Within(0.01f));
        }

        // ================================================================ scale

        /// <summary>Scale is the one triple that is NOT Z-negated.</summary>
        [Test]
        public void Scale_DoesNotNegateAnyAxis()
        {
            ArenaVector3Json a = ArenaUnity.ToArenaScale(new Vector3(1f, 2f, 3f));
            Assert.That(a.X, Is.EqualTo(1f).Within(Tol));
            Assert.That(a.Y, Is.EqualTo(2f).Within(Tol));
            Assert.That(a.Z, Is.EqualTo(3f).Within(Tol));

            Vector3 u = ArenaUnity.ToUnityScale(a);
            Assert.That(u, Is.EqualTo(new Vector3(1f, 2f, 3f)));
        }

        [TestCase(1f, 1f, 1f)]
        [TestCase(0.5f, 2f, 4f)]
        [TestCase(-1f, 0.25f, 10f)]
        public void Scale_RoundTripsThroughArenaAndBack(float x, float y, float z)
        {
            var original = new Vector3(x, y, z);
            Vector3 back = ArenaUnity.ToUnityScale(ArenaUnity.ToArenaScale(original));

            Assert.That(back.x, Is.EqualTo(original.x).Within(Tol));
            Assert.That(back.y, Is.EqualTo(original.y).Within(Tol));
            Assert.That(back.z, Is.EqualTo(original.z).Within(Tol));
        }

        // ================================================================ color

        [Test]
        public void ToArenaColor_EmitsLowercaseSixDigitHex()
        {
            Assert.That(ArenaUnity.ToArenaColor(Color.red), Is.EqualTo("#ff0000"));
            Assert.That(ArenaUnity.ToArenaColor(Color.black), Is.EqualTo("#000000"));
            Assert.That(ArenaUnity.ToArenaColor(Color.white), Is.EqualTo("#ffffff"));
        }

        [Test]
        public void Color_RoundTripsThroughHex()
        {
            foreach (string hex in new[] { "#ee82ee", "#000000", "#ffffff", "#123456" })
            {
                Assert.That(ArenaUnity.ToArenaColor(ArenaUnity.ToUnityColor(hex)),
                    Is.EqualTo(hex), hex);
            }
        }

        /// <summary>
        /// A CSS colour name must survive the ArenaCssColors normalization step inside
        /// ToUnityColor and come back out as the matching hex.
        /// </summary>
        [Test]
        public void ToUnityColor_AcceptsCssColourNames()
        {
            Assert.That(ArenaUnity.ToArenaColor(ArenaUnity.ToUnityColor("violet")),
                Is.EqualTo("#ee82ee"));
            Assert.That(ArenaUnity.ToArenaColor(ArenaUnity.ToUnityColor("RED")),
                Is.EqualTo("#ff0000"));
        }

        [Test]
        public void ToUnityColor_WithOpacity_SetsAlphaAndLeavesRgb()
        {
            Color c = ArenaUnity.ToUnityColor("#ee82ee", 0.5f);

            Assert.That(c.a, Is.EqualTo(0.5f).Within(1e-4f));
            Assert.That(ArenaUnity.ToArenaColor(c), Is.EqualTo("#ee82ee"));
        }

        // ======================================================= object types

        [TestCase("gltf-model", true)]
        [TestCase("handLeft", true)]
        [TestCase("handRight", true)]
        [TestCase("box", false)]
        [TestCase("", false)]
        [TestCase("GLTF-MODEL", false, TestName = "IsGltfType_IsCaseSensitive")]
        [TestCase(null, false)]
        public void IsGltfType_RecognizesExactlyTheThreeGltfBackedTypes(string objectType, bool expected)
        {
            Assert.That(ArenaUnity.IsGltfType(objectType), Is.EqualTo(expected));
        }

        [Test]
        public void Primitives_IsTheEighteenEntryAFrameGeometryList()
        {
            Assert.That(ArenaUnity.primitives, Is.EquivalentTo(new[]
            {
                "box", "capsule", "circle", "cone", "cube", "cylinder", "dodecahedron",
                "icosahedron", "octahedron", "plane", "ring", "roundedbox", "sphere",
                "tetrahedron", "torus", "torusKnot", "triangle", "videosphere",
            }));
            Assert.That(ArenaUnity.primitives, Is.Unique);
            Assert.That(ArenaUnity.primitives, Contains.Item("cube"),
                "'cube' is deprecated but must stay for backwards compatibility");
        }

        [Test]
        public void LineSinglePixelInMeters_IsStable()
        {
            Assert.That(ArenaUnity.LineSinglePixelInMeters, Is.EqualTo(0.005f).Within(1e-9f));
        }

        // ================================================= TimeSpanToString

        [Test]
        public void TimeSpanToString_Zero_IsZeroSeconds()
        {
            Assert.That(ArenaUnity.TimeSpanToString(TimeSpan.Zero), Is.EqualTo("0 seconds"));
        }

        [Test]
        public void TimeSpanToString_SingularAndPlural()
        {
            Assert.That(ArenaUnity.TimeSpanToString(TimeSpan.FromSeconds(1)), Is.EqualTo("1 second"));
            Assert.That(ArenaUnity.TimeSpanToString(TimeSpan.FromSeconds(2)), Is.EqualTo("2 seconds"));
            Assert.That(ArenaUnity.TimeSpanToString(TimeSpan.FromMinutes(1)), Is.EqualTo("1 minute"));
            Assert.That(ArenaUnity.TimeSpanToString(TimeSpan.FromHours(1)), Is.EqualTo("1 hour"));
            Assert.That(ArenaUnity.TimeSpanToString(TimeSpan.FromDays(1)), Is.EqualTo("1 day"));
            Assert.That(ArenaUnity.TimeSpanToString(TimeSpan.FromDays(2)), Is.EqualTo("2 days"));
        }

        [Test]
        public void TimeSpanToString_TrimsTheTrailingSeparator()
        {
            // 1 day, 2 hours, 3 minutes and no seconds: the minutes clause contributes a
            // trailing ", " that has to be removed.
            Assert.That(ArenaUnity.TimeSpanToString(new TimeSpan(1, 2, 3, 0)),
                Is.EqualTo("1 day, 2 hours, 3 minutes"));
        }

        [Test]
        public void TimeSpanToString_FullCombination()
        {
            Assert.That(ArenaUnity.TimeSpanToString(new TimeSpan(1, 2, 3, 4)),
                Is.EqualTo("1 day, 2 hours, 3 minutes, 4 seconds"));
        }

        [Test]
        public void TimeSpanToString_SkipsZeroComponentsInTheMiddle()
        {
            Assert.That(ArenaUnity.TimeSpanToString(new TimeSpan(0, 1, 0, 1)),
                Is.EqualTo("1 hour, 1 second"));
        }

        /// <summary>
        /// PINS CURRENT BEHAVIOUR (quirk): Runtime/ArenaUnity.cs:56-66 tests each
        /// component with span.Duration() - the absolute value - but formats with the
        /// signed component, so a negative TimeSpan renders a negative number. The
        /// callers pass elapsed durations, so this is latent rather than live; it is
        /// pinned so a future refactor does not change it by accident.
        /// </summary>
        [Test]
        public void TimeSpanToString_NegativeSpan_RendersNegativeComponents()
        {
            Assert.That(ArenaUnity.TimeSpanToString(TimeSpan.FromSeconds(-5)),
                Is.EqualTo("-5 seconds"));
        }

        // ===================================================== MergeRawJson

        [Test]
        public void MergeRawJson_BothNull_IsEmptyObject()
        {
            Assert.That(ArenaUnity.MergeRawJson(null, null), Is.EqualTo("{}"));
        }

        [Test]
        public void MergeRawJson_OnlyPrimary_ReturnsPrimary()
        {
            JObject result = JObject.Parse(ArenaUnity.MergeRawJson(new { a = 1 }, null));
            Assert.That(result["a"].Value<int>(), Is.EqualTo(1));
        }

        [Test]
        public void MergeRawJson_OnlySecondary_ReturnsSecondary()
        {
            JObject result = JObject.Parse(ArenaUnity.MergeRawJson(null, new { b = 2 }));
            Assert.That(result["b"].Value<int>(), Is.EqualTo(2));
        }

        [Test]
        public void MergeRawJson_SecondaryWinsOnConflict()
        {
            JObject result = JObject.Parse(ArenaUnity.MergeRawJson(new { a = 1 }, new { a = 2 }));
            Assert.That(result["a"].Value<int>(), Is.EqualTo(2));
        }

        [Test]
        public void MergeRawJson_UnionsDisjointMembers()
        {
            JObject result = JObject.Parse(ArenaUnity.MergeRawJson(new { a = 1 }, new { b = 2 }));
            Assert.That(result["a"].Value<int>(), Is.EqualTo(1));
            Assert.That(result["b"].Value<int>(), Is.EqualTo(2));
        }

        [Test]
        public void MergeRawJson_MergesNestedObjectsRatherThanReplacingThem()
        {
            JObject result = JObject.Parse(ArenaUnity.MergeRawJson(
                new { outer = new { x = 1 } },
                new { outer = new { y = 2 } }));

            Assert.That(result["outer"]["x"].Value<int>(), Is.EqualTo(1));
            Assert.That(result["outer"]["y"].Value<int>(), Is.EqualTo(2));
        }

        /// <summary>
        /// Arrays are REPLACED, not concatenated - MergeArrayHandling.Replace. This is
        /// what makes it possible to shorten an A-Frame array property (a shortened
        /// "srcs" list, say) instead of accumulating stale entries forever.
        /// </summary>
        [Test]
        public void MergeRawJson_ReplacesArraysInsteadOfConcatenatingThem()
        {
            JObject result = JObject.Parse(ArenaUnity.MergeRawJson(
                new { items = new[] { 1, 2, 3 } },
                new { items = new[] { 9 } }));

            Assert.That(result["items"].Type, Is.EqualTo(JTokenType.Array));
            Assert.That(((JArray)result["items"]).Count, Is.EqualTo(1),
                "arrays must be replaced, not concatenated");
            Assert.That(result["items"][0].Value<int>(), Is.EqualTo(9));
        }
    }
}
