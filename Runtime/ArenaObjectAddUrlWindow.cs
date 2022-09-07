/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System;
using System.Dynamic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity
{
#if UNITY_EDITOR
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
                if (ArenaClientScene.Instance == null)
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

                // Set a position in front of the camera
                float distance = 2f;
                Camera cam = Camera.current ?? Camera.main;
                Vector3 cameraPoint = cam.transform.position + cam.transform.forward * distance;

                dynamic msg = new ExpandoObject();
                msg.object_id = Regex.Replace(object_id, ArenaUnity.regexObjId, ArenaUnity.replaceCharObjId);
                if (ArenaClientScene.Instance.arenaObjs.ContainsKey(msg.object_id))
                    msg.object_id = $"{msg.object_id}-{UnityEngine.Random.Range(0, 1000000)}";
                msg.action = "create";
                msg.type = "object";
                msg.persist = true;
                dynamic data = new ExpandoObject();
                data.object_type = object_type;
                data.url = object_url;
                Quaternion rotOut = object_type == "gltf-model" ? ArenaUnity.UnityToGltfRotationQuat(Quaternion.identity) : Quaternion.identity;
                data.rotation = ArenaUnity.ToArenaRotationEuler(rotOut.eulerAngles);
                data.position = ArenaUnity.ToArenaPosition(cameraPoint);
                msg.data = data;
                string payload = JsonConvert.SerializeObject(msg);
                ArenaClientScene.Instance.PublishObject(msg.object_id, payload); // remote
                ArenaClientScene.Instance.ProcessMessage(payload, menuCommand); // local
                Close();
            }

            if (GUILayout.Button("Cancel"))
                Close();
        }
    }
#endif
}
