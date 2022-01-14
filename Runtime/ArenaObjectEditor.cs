using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    [CustomEditor(typeof(ArenaObject))]
    public class ArenaObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ArenaObject script = (ArenaObject)target;

            // add button to publish unity changes
            GUI.enabled = script.messageType == "object";
            if (GUILayout.Button("Publish Unity Data"))
            {
                script.PublishCreateUpdate();
            }
            GUI.enabled = true;

            DrawDefaultInspector();

            // add button to publish manual json data changes if valid
            GUI.enabled = script.isJsonValidated;
            if (GUILayout.Button("Publish Json Data"))
            {
                script.PublishJsonData();
            }
            GUI.enabled = true;
        }
    }
}
