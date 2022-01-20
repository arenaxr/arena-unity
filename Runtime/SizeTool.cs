using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace ArenaUnity
{
    // Tagging a class with the EditorTool attribute and no target type registers a global tool. Global tools are valid for any selection, and are accessible through the top left toolbar in the editor.
    [EditorTool("ARENA Size Tool")]
    [RequireComponent(typeof(ArenaObject))]
    class SizeTool : EditorTool
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
                text = "ARENA Size Tool",
                tooltip = "ARENA Size Tool"
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
            ArenaMeshCube cube = go.GetComponent<ArenaMeshCube>();
            if (cube != null)
            {
                float size = HandleUtility.GetHandleSize(cube.transform.position) * 1f;
                float snap = 0.5f;

                EditorGUI.BeginChangeCheck();

                float width = cube.width;
                float height = cube.height;
                float depth = cube.depth;
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
                    //Undo.RecordObjects(Selection.transforms, "Size Arena Object");
                    foreach (var o in Selection.gameObjects)
                    {
                        var arena = o.GetComponent<ArenaMeshCube>();
                        arena.width = width;
                        arena.height = height;
                        arena.depth = depth;
                        arena.rebuild = true;
                    }
                }

            }


        }
    }
}
