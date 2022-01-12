using System;
using System.Dynamic;
using System.Text.RegularExpressions;
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
                if (ArenaClient.Instance == null)
                {
                    Debug.LogError($"Failed to create object '{object_id}', press Play before creating an ARENA {object_type}.");
                    return;
                }
                // validate uri
                if (!Uri.IsWellFormedUriString(object_url, UriKind.RelativeOrAbsolute))
                {
                    Debug.LogError($"Badly-formed Uri: '{object_url}'.");
                    return;
                }
                dynamic msg = new ExpandoObject();
                msg.object_id = Regex.Replace(object_id, ArenaUnity.regexArenaObjectId, "-"); ;
                msg.action = "create";
                msg.type = "object";
                msg.persist = true;
                dynamic data = new ExpandoObject();
                data.object_type = object_type;
                data.url = object_url;
                Quaternion rotOut = object_type == "gltf-model" ? ArenaUnity.UnityToGltfRotationQuat(Quaternion.identity) : Quaternion.identity;
                data.rotation = ArenaUnity.ToArenaRotationEuler(rotOut.eulerAngles);
                msg.data = data;
                string payload = JsonConvert.SerializeObject(msg);
                ArenaClient.Instance.Publish(object_id, payload); // remote
                ArenaClient.Instance.ProcessMessage(payload, menuCommand); // local
                Close();
            }

            if (GUILayout.Button("Cancel"))
                Close();
        }
    }
}
