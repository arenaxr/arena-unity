using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace ArenaUnity
{
    // Tagging a class with the EditorTool attribute and no target type registers a global tool. Global tools are valid for any selection, and are accessible through the top left toolbar in the editor.
    [EditorTool("ARENA Mesh Tool")]
    class ArenaMeshEditorTool : EditorTool
    {
        // Serialize this value to set a default value in the Inspector.
        [SerializeField]
        Texture2D m_ToolIcon;
        GUIContent m_IconContent;

        void OnEnable()
        {
            m_ToolIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/io.conix.arena.unity/Editor/Resources/Images/xr-logo.png", typeof(Texture2D));
            m_IconContent = new GUIContent()
            {
                image = m_ToolIcon,
                text = "ARENA Mesh Tool",
                tooltip = "ARENA Mesh Tool"
            };
        }

        public override GUIContent toolbarIcon
        {
            get { return m_IconContent; }
        }

        // This is called for each window that your tool is active in. Put the functionality of your tool here.
        public override void OnToolGUI(EditorWindow window)
        {
            // must be an arena object
            GameObject go = Selection.activeGameObject;
            if (go == null) return;
            ArenaObject aobj = go.GetComponent<ArenaObject>();
            if (aobj == null) return;
            ArenaMesh am = aobj.GetComponent<ArenaMesh>();
            if (am == null) return;
            switch (am.GetType().ToString())
            {
                case "ArenaUnity.ArenaMeshCube":
                    HandleSizeCube(aobj, go.GetComponent<ArenaMeshCube>());
                    break;
                case "ArenaUnity.ArenaMeshCone":
                    HandleSizeCone(aobj, go.GetComponent<ArenaMeshCone>());
                    break;
                case "ArenaUnity.ArenaMeshCapsule":
                    HandleSizeCapsule(aobj, go.GetComponent<ArenaMeshCapsule>());
                    break;
                case "ArenaUnity.ArenaMeshCylinder":
                    HandleSizeCylinder(aobj, go.GetComponent<ArenaMeshCylinder>());
                    break;
                case "ArenaUnity.ArenaMeshIcosahedron":
                    HandleSizeIcosahedron(aobj, go.GetComponent<ArenaMeshIcosahedron>());
                    break;
                case "ArenaUnity.ArenaMeshOctahedron":
                    HandleSizeOctahedron(aobj, go.GetComponent<ArenaMeshOctahedron>());
                    break;
                case "ArenaUnity.ArenaMeshPlane":
                    HandleSizePlane(aobj, go.GetComponent<ArenaMeshPlane>());
                    break;
                case "ArenaUnity.ArenaMeshSphere":
                    HandleSizeSphere(aobj, go.GetComponent<ArenaMeshSphere>());
                    break;
                case "ArenaUnity.ArenaMeshCircle":
                    HandleSizeCircle(aobj, go.GetComponent<ArenaMeshCircle>());
                    break;
                case "ArenaUnity.ArenaMeshRing":
                    HandleSizeRing(aobj, go.GetComponent<ArenaMeshRing>());
                    break;
                case "ArenaUnity.ArenaMeshTorus":
                    HandleSizeTorus(aobj, go.GetComponent<ArenaMeshTorus>());
                    break;
                case "ArenaUnity.ArenaMeshTorusKnot":
                    HandleSizeTorusKnot(aobj, go.GetComponent<ArenaMeshTorusKnot>());
                    break;
                case "ArenaUnity.ArenaMeshTriangle":
                    HandleSizeTriangle(aobj, go.GetComponent<ArenaMeshTriangle>());
                    break;
            }
        }

        private static void HandleSizeCube(ArenaObject aobj, ArenaMeshCube cube)
        {
            float size = HandleUtility.GetHandleSize(cube.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float width = cube.width;
            float height = cube.height;
            float depth = cube.depth;
            Handles.Label(cube.transform.position + Vector3.up + Vector3.forward * size, $"W {cube.width}\nH {cube.height}\nD {cube.depth}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                width = Handles.ScaleSlider(cube.width, cube.transform.position, cube.transform.right, cube.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.green))
            {
                height = Handles.ScaleSlider(cube.height, cube.transform.position, cube.transform.up, cube.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                depth = Handles.ScaleSlider(cube.depth, cube.transform.position, cube.transform.forward, cube.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Cube");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshCube>();
                    amesh.width = width;
                    amesh.height = height;
                    amesh.depth = depth;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizePlane(ArenaObject aobj, ArenaMeshPlane plane)
        {
            float size = HandleUtility.GetHandleSize(plane.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float width = plane.width;
            float height = plane.height;
            Handles.Label(plane.transform.position + Vector3.up + Vector3.forward * size, $"W {plane.width}\nH {plane.height}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                width = Handles.ScaleSlider(plane.width, plane.transform.position, plane.transform.right, plane.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                height = Handles.ScaleSlider(plane.height, plane.transform.position, plane.transform.up, plane.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Plane");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshPlane>();
                    amesh.width = width;
                    amesh.height = height;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeCapsule(ArenaObject aobj, ArenaMeshCapsule capsule)
        {
            float size = HandleUtility.GetHandleSize(capsule.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = capsule.radius;
            float height = capsule.height;
            Handles.Label(capsule.transform.position + Vector3.up + Vector3.forward * size, $"R {capsule.radius}\nH {capsule.height}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(capsule.radius, capsule.transform.position, capsule.transform.right, capsule.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.green))
            {
                height = Handles.ScaleSlider(capsule.height, capsule.transform.position, capsule.transform.up, capsule.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Capsule");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshCapsule>();
                    amesh.radius = radius;
                    amesh.height = height;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeCylinder(ArenaObject aobj, ArenaMeshCylinder cylinder)
        {
            float size = HandleUtility.GetHandleSize(cylinder.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = cylinder.radius;
            float height = cylinder.height;
            Handles.Label(cylinder.transform.position + Vector3.up + Vector3.forward * size, $"R {cylinder.radius}\nH {cylinder.height}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(cylinder.radius, cylinder.transform.position, cylinder.transform.right, cylinder.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.green))
            {
                height = Handles.ScaleSlider(cylinder.height, cylinder.transform.position, cylinder.transform.up, cylinder.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Cylinder");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshCylinder>();
                    amesh.radius = radius;
                    amesh.height = height;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeCone(ArenaObject aobj, ArenaMeshCone cone)
        {
            float size = HandleUtility.GetHandleSize(cone.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = cone.radius;
            float height = cone.height;
            Handles.Label(cone.transform.position + Vector3.up + Vector3.forward * size, $"R {cone.radius}\nH {cone.height}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(cone.radius, cone.transform.position, cone.transform.right, cone.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.green))
            {
                height = Handles.ScaleSlider(cone.height, cone.transform.position, cone.transform.up, cone.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Cone");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshCone>();
                    amesh.radius = radius;
                    amesh.height = height;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeSphere(ArenaObject aobj, ArenaMeshSphere sphere)
        {
            float size = HandleUtility.GetHandleSize(sphere.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = sphere.radius;
            Handles.Label(sphere.transform.position + Vector3.up + Vector3.forward * size, $"R {sphere.radius}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(sphere.radius, sphere.transform.position, sphere.transform.right, sphere.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Sphere");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshSphere>();
                    amesh.radius = radius;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeIcosahedron(ArenaObject aobj, ArenaMeshIcosahedron icosahedron)
        {
            float size = HandleUtility.GetHandleSize(icosahedron.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = icosahedron.radius;
            Handles.Label(icosahedron.transform.position + Vector3.up + Vector3.forward * size, $"R {icosahedron.radius}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(icosahedron.radius, icosahedron.transform.position, icosahedron.transform.right, icosahedron.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Icosahedron");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshIcosahedron>();
                    amesh.radius = radius;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeOctahedron(ArenaObject aobj, ArenaMeshOctahedron octahedron)
        {
            float size = HandleUtility.GetHandleSize(octahedron.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = octahedron.radius;
            Handles.Label(octahedron.transform.position + Vector3.up + Vector3.forward * size, $"R {octahedron.radius}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(octahedron.radius, octahedron.transform.position, octahedron.transform.right, octahedron.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Octahedron");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshOctahedron>();
                    amesh.radius = radius;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeRing(ArenaObject aobj, ArenaMeshRing ring)
        {
            float size = HandleUtility.GetHandleSize(ring.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float outerRadius = ring.outerRadius;
            float innerRadius = ring.innerRadius;
            float thetaLength = ring.thetaLength;
            Handles.Label(ring.transform.position + Vector3.up + Vector3.forward * size, $"OR {ring.outerRadius}\nIR {ring.innerRadius}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                outerRadius = Handles.ScaleSlider(ring.outerRadius, ring.transform.position, ring.transform.right, ring.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                innerRadius = Handles.ScaleSlider(ring.innerRadius, ring.transform.position, ring.transform.right, ring.transform.rotation, size / 2, snap);
            }
            //using (new Handles.DrawingScope(Color.yellow))
            //{
            //    float thetaDeg = ring.thetaLength * 180 / Mathf.PI;
            //    thetaDeg = Handles.FreeRotateHandle(Quaternion.Euler(0, thetaDeg, 0), ring.transform.position, size).eulerAngles.y;
            //    //thetaDeg = Handles.RotationHandle(Quaternion.Euler(0, thetaDeg, 0), ring.transform.position).eulerAngles.y;
            //    thetaLength = thetaDeg * 180 / Mathf.PI;
            //    Handles.DrawSolidArc(ring.transform.position, ring.transform.forward, -ring.transform.right, thetaLength, size / 2);
            //    //myObj.shieldArea = (float)Handles.ScaleValueHandle(myObj.shieldArea, myObj.transform.position + myObj.transform.forward * myObj.shieldArea, myObj.transform.rotation, 1, Handles.ConeHandleCap, 1);
            //}
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Ring");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshRing>();
                    amesh.outerRadius = outerRadius;
                    amesh.innerRadius = innerRadius;
                    //amesh.thetaLength = thetaLength;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeCircle(ArenaObject aobj, ArenaMeshCircle circle)
        {
            float size = HandleUtility.GetHandleSize(circle.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = circle.radius;
            Handles.Label(circle.transform.position + Vector3.up + Vector3.forward * size, $"R {circle.radius}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(circle.radius, circle.transform.position, circle.transform.right, circle.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Circle");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshCircle>();
                    amesh.radius = radius;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeTorus(ArenaObject aobj, ArenaMeshTorus torus)
        {
            float size = HandleUtility.GetHandleSize(torus.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = torus.radius;
            float thickness = torus.thickness;
            Handles.Label(torus.transform.position + Vector3.up + Vector3.forward * size, $"R {torus.radius}\nT {torus.thickness}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(torus.radius, torus.transform.position, torus.transform.right, torus.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                thickness = Handles.ScaleSlider(torus.thickness, torus.transform.position, torus.transform.right, torus.transform.rotation, size / 2, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Torus");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshTorus>();
                    amesh.radius = radius;
                    amesh.thickness = thickness;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeTorusKnot(ArenaObject aobj, ArenaMeshTorusKnot torusKnot)
        {
            float size = HandleUtility.GetHandleSize(torusKnot.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = torusKnot.radius;
            float thickness = torusKnot.thickness;
            Handles.Label(torusKnot.transform.position + Vector3.up + Vector3.forward * size, $"R {torusKnot.radius}\nT {torusKnot.thickness}");
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(torusKnot.radius, torusKnot.transform.position, torusKnot.transform.right, torusKnot.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                thickness = Handles.ScaleSlider(torusKnot.thickness, torusKnot.transform.position, torusKnot.transform.right, torusKnot.transform.rotation, size / 2, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena TorusKnot");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshTorusKnot>();
                    amesh.radius = radius;
                    amesh.thickness = thickness;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeTriangle(ArenaObject aobj, ArenaMeshTriangle triangle)
        {
            float size = HandleUtility.GetHandleSize(triangle.transform.position) * .5f;
            Vector3 snap = new Vector3(0.5f, 0.5f);
            Vector3 handleDirection = Vector3.forward;

            EditorGUI.BeginChangeCheck();
            Vector3 vertexA = triangle.vertexA;
            Vector3 vertexB = triangle.vertexB;
            Vector3 vertexC = triangle.vertexC;
            Handles.Label(triangle.vertexA + Vector3.up * 1f, $"A {triangle.vertexA}\nB {triangle.vertexB}\nC {triangle.vertexC}");
            using (new Handles.DrawingScope(Color.red))
            {
                Handles.DrawWireDisc(triangle.transform.position, handleDirection, size / 10f);
            }
            using (new Handles.DrawingScope(Color.green))
            {
                //Handles.CapFunction capFunction = (id, position, rotation, size, type) => Handles.CircleHandleCap(
                //  id,
                //  triangle.transform.position + vertexA,
                //  triangle.transform.rotation * Quaternion.LookRotation(handleDirection),
                //  size,
                //  EventType.Repaint);
                //vertexA = Handles.Slider2D(0, triangle.transform.position, vertexA, handleDirection, Vector3.right, Vector3.up, size, capFunction, snap);
                vertexA = Handles.Slider2D(triangle.transform.position + vertexA, handleDirection, Vector3.right, Vector3.up, size, Handles.CircleHandleCap, snap);
            }
            using (new Handles.DrawingScope(Color.magenta))
            {
                vertexB = Handles.Slider2D(triangle.transform.position + vertexB, handleDirection, Vector3.right, Vector3.up, size, Handles.CircleHandleCap, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                vertexC = Handles.Slider2D(triangle.transform.position + vertexC, handleDirection, Vector3.right, Vector3.up, size, Handles.CircleHandleCap, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Triangle");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshTriangle>();
                    amesh.vertexA = vertexA - triangle.transform.localPosition;
                    amesh.vertexB = vertexB - triangle.transform.localPosition;
                    amesh.vertexC = vertexC - triangle.transform.localPosition;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

    }
}
