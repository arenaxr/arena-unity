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
            //ArenaObject aobj = aobj.
            //if (aobj == null) return;


            EditorGUI.BeginChangeCheck();

            Vector3 position = Tools.handlePosition;

            using (new Handles.DrawingScope(Color.green))
            {
                position = Handles.Slider(position, Vector3.right);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Vector3 delta = position - Tools.handlePosition;

                Undo.RecordObjects(Selection.transforms, "Move Platform");

                foreach (var transform in Selection.transforms)
                    transform.position += delta;
            }
        }
    }
}
