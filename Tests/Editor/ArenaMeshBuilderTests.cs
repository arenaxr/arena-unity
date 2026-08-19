/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaUnity.Tests
{
    /// <summary>
    /// The procedural mesh builders under Runtime/ArenaMesh. Only the deterministic,
    /// publicly reachable builders are covered, and only with counts and dimensions that
    /// follow directly from the source - the point is to catch an off-by-one in a segment
    /// loop, not to re-derive the geometry.
    ///
    /// Deliberately not covered: ParametricPlaneRandom and ParametricPlanePerlin (not
    /// deterministic / not seeded), the internal Tetrahedron / Dodecahedron / TorusKnot /
    /// Triangle builders (not visible outside the runtime assembly), anything needing
    /// Shader.Find, and the AprilTag builders (their assembly requires AR Foundation,
    /// which is not a package dependency).
    /// </summary>
    [TestFixture]
    public class ArenaMeshBuilderTests
    {
        private readonly List<Mesh> created = new List<Mesh>();

        private Mesh Track(Mesh mesh)
        {
            created.Add(mesh);
            return mesh;
        }

        [TearDown]
        public void DestroyCreatedMeshes()
        {
            foreach (Mesh mesh in created)
                if (mesh != null) Object.DestroyImmediate(mesh);
            created.Clear();
        }

        private static int TriangleCount(Mesh mesh)
        {
            return mesh.triangles.Length / 3;
        }

        private static int NonDegenerateTriangleCount(Mesh mesh)
        {
            int[] t = mesh.triangles;
            int count = 0;
            for (int i = 0; i < t.Length; i += 3)
                if (t[i] != t[i + 1] && t[i + 1] != t[i + 2] && t[i] != t[i + 2]) count++;
            return count;
        }

        // ============================================================== plane

        /// <summary>
        /// PlaneBuilder adds one to each segment count (PlaneBuilder.cs:22-23), so a
        /// grid of s segments has s+1 vertices per side. Vertices are (w)(h) and
        /// triangles are 2(w-1)(h-1) where w and h are the incremented counts.
        /// </summary>
        [TestCase(1, 1, 4, 2, TestName = "Plane_OneByOne")]
        [TestCase(2, 2, 9, 8, TestName = "Plane_TwoByTwo")]
        [TestCase(3, 2, 12, 12, TestName = "Plane_ThreeByTwo")]
        [TestCase(4, 4, 25, 32, TestName = "Plane_FourByFour")]
        public void Plane_VertexAndTriangleCountsFollowSegmentCounts(
            int wSegments, int hSegments, int expectedVertices, int expectedTriangles)
        {
            Mesh mesh = Track(PlaneBuilder.Build(1f, 1f, wSegments, hSegments));

            Assert.That(mesh.vertexCount, Is.EqualTo(expectedVertices));
            Assert.That(TriangleCount(mesh), Is.EqualTo(expectedTriangles));
        }

        [Test]
        public void Plane_BoundsMatchRequestedWidthAndHeight()
        {
            Mesh mesh = Track(PlaneBuilder.Build(2f, 3f, 1, 1));

            Assert.That(mesh.bounds.size.x, Is.EqualTo(2f).Within(1e-4f));
            Assert.That(mesh.bounds.size.y, Is.EqualTo(3f).Within(1e-4f));
            Assert.That(mesh.bounds.size.z, Is.EqualTo(0f).Within(1e-4f), "a default plane is flat");
        }

        [Test]
        public void Plane_IsCenteredOnTheOrigin()
        {
            Mesh mesh = Track(PlaneBuilder.Build(2f, 4f, 2, 2));

            Assert.That(mesh.bounds.center.x, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(mesh.bounds.center.y, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void Plane_HasOneUvPerVertex()
        {
            Mesh mesh = Track(PlaneBuilder.Build(1f, 1f, 3, 3));

            Assert.That(mesh.uv.Length, Is.EqualTo(mesh.vertexCount));
        }

        [Test]
        public void Plane_SubdividingRaisesCountsMonotonically()
        {
            Mesh coarse = Track(PlaneBuilder.Build(1f, 1f, 1, 1));
            Mesh fine = Track(PlaneBuilder.Build(1f, 1f, 4, 4));

            Assert.That(fine.vertexCount, Is.GreaterThan(coarse.vertexCount));
            Assert.That(TriangleCount(fine), Is.GreaterThan(TriangleCount(coarse)));
        }

        // ============================================================= sphere

        /// <summary>
        /// SphereBuilder allocates (lon+1)*lat+2 vertices: a ring of lon+1 per latitude
        /// band, plus the two poles (SphereBuilder.cs:21).
        /// </summary>
        [TestCase(24, 16, 402, TestName = "Sphere_DefaultSegments")]
        [TestCase(8, 4, 38, TestName = "Sphere_CoarseSegments")]
        [TestCase(12, 6, 80, TestName = "Sphere_MediumSegments")]
        public void Sphere_VertexCountFollowsSegmentCounts(int lon, int lat, int expectedVertices)
        {
            Mesh mesh = Track(SphereBuilder.Build(1f, lon, lat));

            Assert.That(mesh.vertexCount, Is.EqualTo(expectedVertices));
        }

        /// <summary>
        /// The real triangles are lon for the top cap, 2*lon*(lat-1) for the bands and
        /// lon for the bottom cap, i.e. 2*lon*lat in total.
        /// </summary>
        [TestCase(24, 16, 768, TestName = "Sphere_DefaultTriangleCount")]
        [TestCase(8, 4, 64, TestName = "Sphere_CoarseTriangleCount")]
        [TestCase(12, 6, 144, TestName = "Sphere_MediumTriangleCount")]
        public void Sphere_HasExactlyTwoLonLatNonDegenerateTriangles(int lon, int lat, int expected)
        {
            Mesh mesh = Track(SphereBuilder.Build(1f, lon, lat));

            Assert.That(NonDegenerateTriangleCount(mesh), Is.EqualTo(expected));
        }

        /// <summary>
        /// PINS CURRENT BEHAVIOUR (bug): SphereBuilder.cs:54 allocates the index array as
        /// `new int[len * 2 * 3]` - six indices per vertex - but only fills
        /// 3*lon + 6*lon*(lat-1) + 3*lon of them. The remainder stays zero, so every
        /// sphere ships a tail of degenerate (0,0,0) triangles: 36 of them at the default
        /// 24x16 segmentation, about 4.5% of the index buffer.
        ///
        /// They have zero area so nothing renders wrong, but they are pure waste in every
        /// sphere mesh and they make triangle counts misleading.
        ///
        /// CORRECT BEHAVIOUR is to size the array to what is used:
        ///     int[] triangles = new int[(2 * lonSegments * latSegments) * 3];
        /// This test is written to pass either way - it asserts only that any extra
        /// triangles beyond the real ones are degenerate - so fixing the bug does not
        /// break it. The companion test above pins the real count.
        /// </summary>
        [TestCase(24, 16)]
        [TestCase(8, 4)]
        public void Sphere_AnyExtraTrianglesAreDegenerateFiller(int lon, int lat)
        {
            Mesh mesh = Track(SphereBuilder.Build(1f, lon, lat));

            int real = 2 * lon * lat;
            int total = TriangleCount(mesh);

            Assert.That(total, Is.GreaterThanOrEqualTo(real));
            Assert.That(NonDegenerateTriangleCount(mesh), Is.EqualTo(real),
                "every triangle beyond the real ones must be degenerate filler");
        }

        [TestCase(1f)]
        [TestCase(0.5f)]
        [TestCase(3f)]
        public void Sphere_EveryVertexLiesWithinTheRequestedRadius(float radius)
        {
            Mesh mesh = Track(SphereBuilder.Build(radius, 24, 16));

            foreach (Vector3 v in mesh.vertices)
            {
                Assert.That(v.magnitude, Is.LessThanOrEqualTo(radius * 1.0001f));
                Assert.That(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z), Is.False);
            }
        }

        /// <summary>
        /// The poles are placed at exactly +/- radius on Y, so the vertical extent is
        /// exact. The horizontal extent is slightly under the diameter because no latitude
        /// ring falls exactly on the equator for an even band count.
        /// </summary>
        [TestCase(1f)]
        [TestCase(2.5f)]
        public void Sphere_BoundsMatchTheRequestedRadius(float radius)
        {
            Mesh mesh = Track(SphereBuilder.Build(radius, 24, 16));

            Assert.That(mesh.bounds.size.y, Is.EqualTo(radius * 2f).Within(1e-3f));
            Assert.That(mesh.bounds.size.x, Is.LessThanOrEqualTo(radius * 2f + 1e-3f));
            Assert.That(mesh.bounds.size.x, Is.GreaterThan(radius * 2f * 0.98f));
            Assert.That(mesh.bounds.size.z, Is.LessThanOrEqualTo(radius * 2f + 1e-3f));
            Assert.That(mesh.bounds.size.z, Is.GreaterThan(radius * 2f * 0.98f));
        }

        [Test]
        public void Sphere_HasOneUvPerVertex()
        {
            Mesh mesh = Track(SphereBuilder.Build(1f, 24, 16));

            Assert.That(mesh.uv.Length, Is.EqualTo(mesh.vertexCount));
        }

        // ======================================================== polyhedra

        /// <summary>
        /// At details 0 PolyhedronBuilder takes the flat-normal path: one unique vertex
        /// per index, so vertexCount equals the face-index count and the triangle count is
        /// a third of it (PolyhedronBuilder.cs:40-49). An icosahedron has 20 faces and an
        /// octahedron 8.
        /// </summary>
        [Test]
        public void Icosahedron_AtZeroDetailsHasTwentyFlatShadedFaces()
        {
            Mesh mesh = Track(IcosahedronBuilder.Build(1f, 0));

            Assert.That(mesh.vertexCount, Is.EqualTo(60), "20 faces x 3 unique vertices");
            Assert.That(TriangleCount(mesh), Is.EqualTo(20));
        }

        [Test]
        public void Octahedron_AtZeroDetailsHasEightFlatShadedFaces()
        {
            Mesh mesh = Track(OctahedronBuilder.Build(1f, 0));

            Assert.That(mesh.vertexCount, Is.EqualTo(24), "8 faces x 3 unique vertices");
            Assert.That(TriangleCount(mesh), Is.EqualTo(8));
        }

        /// <summary>
        /// The polyhedron builders normalize their base vertices before scaling, so at
        /// details 0 every vertex sits exactly on the sphere of the requested radius. This
        /// is the assertion that pins the radius parameter, in place of a bounds check that
        /// would depend on the solid's orientation.
        /// </summary>
        [TestCase(1f)]
        [TestCase(0.25f)]
        [TestCase(5f)]
        public void Icosahedron_EveryVertexSitsOnTheRequestedRadius(float radius)
        {
            Mesh mesh = Track(IcosahedronBuilder.Build(radius, 0));

            foreach (Vector3 v in mesh.vertices)
                Assert.That(v.magnitude, Is.EqualTo(radius).Within(radius * 1e-3f));
        }

        [TestCase(1f)]
        [TestCase(2f)]
        public void Octahedron_EveryVertexSitsOnTheRequestedRadius(float radius)
        {
            Mesh mesh = Track(OctahedronBuilder.Build(radius, 0));

            foreach (Vector3 v in mesh.vertices)
                Assert.That(v.magnitude, Is.EqualTo(radius).Within(radius * 1e-3f));
        }

        /// <summary>
        /// Subdividing must produce strictly more geometry, and must keep every vertex on
        /// the sphere - the midpoints are renormalized, which is what makes the subdivided
        /// solid approximate a sphere rather than bulge flat.
        /// </summary>
        [Test]
        public void Icosahedron_SubdivisionAddsGeometryAndStaysOnTheSphere()
        {
            Mesh coarse = Track(IcosahedronBuilder.Build(1f, 0));
            Mesh fine = Track(IcosahedronBuilder.Build(1f, 1));

            Assert.That(TriangleCount(fine), Is.GreaterThan(TriangleCount(coarse)));

            foreach (Vector3 v in fine.vertices)
                Assert.That(v.magnitude, Is.EqualTo(1f).Within(1e-3f));
        }

        // ========================================================= sanity net

        /// <summary>
        /// A cheap guard over the remaining public builders: whatever the geometry, a
        /// built mesh must have vertices, an index count that is a multiple of three, and
        /// no NaN. That is enough to catch a divide-by-zero or an unfilled buffer without
        /// asserting counts this suite has not derived.
        /// </summary>
        [Test]
        public void PublicBuilders_ProduceWellFormedMeshes()
        {
            var meshes = new Dictionary<string, Mesh>
            {
                { "plane", Track(PlaneBuilder.Build(1f, 1f, 2, 2)) },
                { "sphere", Track(SphereBuilder.Build(1f, 16, 12)) },
                { "icosahedron", Track(IcosahedronBuilder.Build(1f, 0)) },
                { "octahedron", Track(OctahedronBuilder.Build(1f, 0)) },
                { "cylinder", Track(CylinderBuilder.Build(1f, 1f, 2f, 16, 1, false)) },
                { "cylinder-open", Track(CylinderBuilder.Build(1f, 1f, 2f, 16, 1, true)) },
                { "ring", Track(RingBuilder.Build(0.5f, 1f, 16, 1)) },
                { "torus", Track(TorusBuilder.Build(1f, 0.25f, 16, 16)) },
            };

            foreach (KeyValuePair<string, Mesh> entry in meshes)
            {
                Mesh mesh = entry.Value;
                Assert.That(mesh, Is.Not.Null, entry.Key);
                Assert.That(mesh.vertexCount, Is.GreaterThan(0), entry.Key);
                Assert.That(mesh.triangles.Length % 3, Is.EqualTo(0), entry.Key);
                Assert.That(mesh.triangles.Length, Is.GreaterThan(0), entry.Key);

                foreach (Vector3 v in mesh.vertices)
                {
                    Assert.That(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z),
                        Is.False, $"{entry.Key} produced a NaN vertex");
                    Assert.That(float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z),
                        Is.False, $"{entry.Key} produced an infinite vertex");
                }

                int max = mesh.triangles.Max();
                Assert.That(max, Is.LessThan(mesh.vertexCount),
                    $"{entry.Key} has an index out of range of its vertex buffer");
            }
        }

        /// <summary>
        /// Capping a cylinder must add geometry relative to the open-ended form.
        /// </summary>
        [Test]
        public void Cylinder_ClosedHasMoreTrianglesThanOpenEnded()
        {
            Mesh closed = Track(CylinderBuilder.Build(1f, 1f, 2f, 16, 1, false));
            Mesh open = Track(CylinderBuilder.Build(1f, 1f, 2f, 16, 1, true));

            Assert.That(TriangleCount(closed), Is.GreaterThan(TriangleCount(open)));
        }
    }
}
