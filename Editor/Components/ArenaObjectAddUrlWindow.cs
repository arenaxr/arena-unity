/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ArenaUnity.Editor.Components
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

                var client = ArenaClientScene.Instance;
                if (client.arenaObjs.ContainsKey(object_id))
                    object_id = $"{object_id}-{UnityEngine.Random.Range(0, 1000000)}";
                ArenaObjectJson msg = new ArenaObjectJson
                {
                    object_id = object_id,
                    action = "create",
                    type = "object",
                    persist = true,
                };
                Quaternion rotOut = Quaternion.identity;
                ArenaDataJson data = new ArenaDataJson
                {
                    object_type = object_type,
                    Url = object_url,
                    Rotation = ArenaUnity.ToArenaRotationQuat(rotOut), // always send quaternions over the wire
                    Position = ArenaUnity.ToArenaPosition(cameraPoint),
                };
                msg.data = data;
                string payload = JsonConvert.SerializeObject(msg);
                client.PublishObject(msg.object_id, payload);
                Close();
            }

            if (GUILayout.Button("Cancel"))
                Close();
        }
    }
}
