using System.Dynamic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaObjectAddUrlWindow : EditorWindow
    {
        private MenuCommand menuCommand;
        private string object_type;
        private string object_id;
        private string object_url;

        internal void Init(string object_type, MenuCommand menuCommand)
        {
            this.object_type = object_type;
            this.menuCommand = menuCommand;
        }

        protected void OnGUI()
        {
            object_id = EditorGUILayout.TextField($"{object_type} object-id:", object_id);
            object_url = EditorGUILayout.TextField($"{object_type} url:", object_url);

            if (GUILayout.Button($"Create new {object_type}"))
            {
                dynamic msg = new ExpandoObject();
                msg.object_id = object_id;
                msg.action = "create";
                msg.type = "object";
                msg.persist = true;
                dynamic data = new ExpandoObject();
                data.object_type = object_type;
                data.url = object_url;
                msg.data = data;
                string payload = JsonConvert.SerializeObject(msg);

                if (ArenaClient.Instance != null)
                {
                    ArenaClient.Instance.Publish(object_id, payload); // remote
                    ArenaClient.Instance.ProcessMessage(payload, menuCommand); // local
                }
                else
                    Debug.LogError($"Failed to create object '{object_id}', press Play before creating an ARENA {object_type}.");
                Close();
            }

            if (GUILayout.Button("Abort"))
                Close();
        }
    }
}
