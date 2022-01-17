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
                foreach (string animation in script.animations)
                {
                    Animation anim = script.GetComponentInChildren<Animation>(true);
                    if (GUILayout.Button($"Play {animation}"))
                    {
                        anim.Play(animation);
                    }
                    if (GUILayout.Button($"Stop {animation}"))
                    {
                        anim.Stop(animation);
                    }
                }
            }

        }
    }
}
