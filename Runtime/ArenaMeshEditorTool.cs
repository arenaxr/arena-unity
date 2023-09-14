/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif
using UnityEngine;

namespace ArenaUnity
{
#if UNITY_EDITOR
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
            Debug.Log(am.GetType().ToString());
            switch (am.GetType().ToString())
            {
                case "ArenaUnity.ArenaMeshBox":
                    HandleSizeCube(aobj, go.GetComponent<ArenaMeshBox>());
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
                case "ArenaUnity.ArenaMeshTetrahedron":
                    HandleSizeTetrahedron(aobj, go.GetComponent<ArenaMeshTetrahedron>());
                    break;
                case "ArenaUnity.ArenaMeshDodecahedron":
                    HandleSizeDodecahedron(aobj, go.GetComponent<ArenaMeshDodecahedron>());
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

        private static void HandleSizeCube(ArenaObject aobj, ArenaMeshBox cube)
        {
            float size = HandleUtility.GetHandleSize(cube.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float width = cube.json.Width;
            float height = cube.json.Height;
            float depth = cube.json.Depth;
            using (new Handles.DrawingScope(Color.magenta))
            {
                width = Handles.ScaleSlider(cube.json.Width, cube.transform.position, cube.transform.right, cube.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.green))
            {
                height = Handles.ScaleSlider(cube.json.Height, cube.transform.position, cube.transform.up, cube.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                depth = Handles.ScaleSlider(cube.json.Depth, cube.transform.position, cube.transform.forward, cube.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Cube");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshBox>();
                    amesh.json.Width = width;
                    amesh.json.Height = height;
                    amesh.json.Depth = depth;
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
            float width = plane.json.Width;
            float height = plane.json.Height;
            using (new Handles.DrawingScope(Color.magenta))
            {
                width = Handles.ScaleSlider(plane.json.Width, plane.transform.position, plane.transform.right, plane.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                height = Handles.ScaleSlider(plane.json.Height, plane.transform.position, plane.transform.up, plane.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Plane");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshPlane>();
                    amesh.json.Width = width;
                    amesh.json.Height = height;
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
            float radius = capsule.json.Radius;
            float length = capsule.json.Length;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(capsule.json.Radius, capsule.transform.position, capsule.transform.right, capsule.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.green))
            {
                length = Handles.ScaleSlider(capsule.json.Length, capsule.transform.position, capsule.transform.up, capsule.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Capsule");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshCapsule>();
                    amesh.json.Radius = radius;
                    amesh.json.Length = length;
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
            float radius = cylinder.json.Radius;
            float height = cylinder.json.Height;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(cylinder.json.Radius, cylinder.transform.position, cylinder.transform.right, cylinder.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.green))
            {
                height = Handles.ScaleSlider(cylinder.json.Height, cylinder.transform.position, cylinder.transform.up, cylinder.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Cylinder");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshCylinder>();
                    amesh.json.Radius = radius;
                    amesh.json.Height = height;
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
            float radius = cone.json.RadiusBottom;
            float height = cone.json.Height;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(cone.json.RadiusBottom, cone.transform.position, cone.transform.right, cone.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.green))
            {
                height = Handles.ScaleSlider(cone.json.Height, cone.transform.position, cone.transform.up, cone.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Cone");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshCone>();
                    amesh.json.RadiusBottom = radius;
                    amesh.json.Height = height;
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
            float radius = sphere.json.Radius;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(sphere.json.Radius, sphere.transform.position, sphere.transform.right, sphere.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Sphere");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshSphere>();
                    amesh.json.Radius = radius;
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
            float radius = icosahedron.json.Radius;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(icosahedron.json.Radius, icosahedron.transform.position, icosahedron.transform.right, icosahedron.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Icosahedron");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshIcosahedron>();
                    amesh.json.Radius = radius;
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
            float radius = octahedron.json.Radius;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(octahedron.json.Radius, octahedron.transform.position, octahedron.transform.right, octahedron.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Octahedron");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshOctahedron>();
                    amesh.json.Radius = radius;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeDodecahedron(ArenaObject aobj, ArenaMeshDodecahedron dodecahedron)
        {
            float size = HandleUtility.GetHandleSize(dodecahedron.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = dodecahedron.json.Radius;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(dodecahedron.json.Radius, dodecahedron.transform.position, dodecahedron.transform.right, dodecahedron.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Dodecahedron");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshDodecahedron>();
                    amesh.json.Radius = radius;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

        private static void HandleSizeTetrahedron(ArenaObject aobj, ArenaMeshTetrahedron tetrahedron)
        {
            float size = HandleUtility.GetHandleSize(tetrahedron.transform.position) * 1f;
            float snap = 0.5f;

            EditorGUI.BeginChangeCheck();
            float radius = tetrahedron.json.Radius;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(tetrahedron.json.Radius, tetrahedron.transform.position, tetrahedron.transform.right, tetrahedron.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Tetrahedron");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshTetrahedron>();
                    amesh.json.Radius = radius;
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
            float outerRadius = ring.json.RadiusOuter;
            float innerRadius = ring.json.RadiusInner;
            float thetaLength = ring.json.ThetaLength;
            using (new Handles.DrawingScope(Color.magenta))
            {
                outerRadius = Handles.ScaleSlider(ring.json.RadiusOuter, ring.transform.position, ring.transform.right, ring.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                innerRadius = Handles.ScaleSlider(ring.json.RadiusInner, ring.transform.position, ring.transform.right, ring.transform.rotation, size / 2, snap);
            }
            //using (new Handles.DrawingScope(Color.yellow))
            //{
            //    float thetaDeg = ring.json.thetaLength * 180 / Mathf.PI;
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
                    amesh.json.RadiusOuter = outerRadius;
                    amesh.json.RadiusInner = innerRadius;
                    //amesh.json.thetaLength = thetaLength;
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
            float radius = circle.json.Radius;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(circle.json.Radius, circle.transform.position, circle.transform.right, circle.transform.rotation, size, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Circle");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshCircle>();
                    amesh.json.Radius = radius;
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
            float radius = torus.json.Radius;
            float radiusTubular = torus.json.RadiusTubular;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(torus.json.Radius, torus.transform.position, torus.transform.right, torus.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                radiusTubular = Handles.ScaleSlider(torus.json.RadiusTubular, torus.transform.position, torus.transform.right, torus.transform.rotation, size / 2, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena Torus");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshTorus>();
                    amesh.json.Radius = radius;
                    amesh.json.RadiusTubular = radiusTubular;
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
            float radius = torusKnot.json.Radius;
            float radiusTubular = torusKnot.json.RadiusTubular;
            using (new Handles.DrawingScope(Color.magenta))
            {
                radius = Handles.ScaleSlider(torusKnot.json.Radius, torusKnot.transform.position, torusKnot.transform.right, torusKnot.transform.rotation, size, snap);
            }
            using (new Handles.DrawingScope(Color.cyan))
            {
                radiusTubular = Handles.ScaleSlider(torusKnot.json.RadiusTubular, torusKnot.transform.position, torusKnot.transform.right, torusKnot.transform.rotation, size / 2, snap);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObjects(Selection.gameObjects, "Size Arena TorusKnot");
                foreach (var o in Selection.gameObjects)
                {
                    var amesh = o.GetComponent<ArenaMeshTorusKnot>();
                    amesh.json.Radius = radius;
                    amesh.json.RadiusTubular = radiusTubular;
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
            // TODO (mwfarb): update this conversion when Vector3 objects are in parity with our schema
            Vector3 vertexA = (Vector3)triangle.json.VertexA;
            Vector3 vertexB = (Vector3)triangle.json.VertexB;
            Vector3 vertexC = (Vector3)triangle.json.VertexC;
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
                    amesh.json.VertexA = vertexA - triangle.transform.localPosition;
                    amesh.json.VertexB = vertexB - triangle.transform.localPosition;
                    amesh.json.VertexC = vertexC - triangle.transform.localPosition;
                    amesh.build = true;
                    aobj.meshChanged = true;
                }
            }
        }

    }
#endif
}
