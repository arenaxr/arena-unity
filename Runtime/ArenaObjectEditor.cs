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

            // add any animation buttons
            if (script.animations != null)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
                foreach (string animation in script.animations)
                {
                    Animation anim = script.GetComponentInChildren<Animation>(true);
                    GUILayout.BeginHorizontal("Box");
                    GUILayout.Label(animation);
                    if (GUILayout.Button($"Play", GUILayout.Width(40)))
                    {
                        anim.Play(animation);
                    }
                    if (GUILayout.Button($"Stop", GUILayout.Width(40)))
                    {
                        anim.Stop(animation);
                    }
                    if (GUILayout.Button($"Rewind", GUILayout.Width(60)))
                    {
                        anim.Rewind(animation);
                    }
                    GUILayout.EndHorizontal();
                }
            }


        }
    }
}
