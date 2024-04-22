using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArenaUnity;

public class HeadlessCLI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR
        // The Editor will automatically start connecting to the scene, so we
        // put this code in a !UNITY_EDITOR block.

        ArenaClientScene scene = ArenaClientScene.Instance;

        // Grab CLI arguments (change this to add additional arguments if
        // needed)
        string[] arguments = Environment.GetCommandLineArgs();
        if (arguments.Length >= 3)
        {
            string namespaceName = arguments[1];
            string sceneName = arguments[2];

            scene.authType = ArenaMqttClient.Auth.Anonymous;

            // Set namespace and scenename to arguments
            scene.namespaceName = namespaceName;
            scene.sceneName = sceneName;

            Debug.Log($"Connecting to {namespaceName}/{sceneName}...");
        }

        // Actually connect to the scene, with the set namespace and scenename
        StartCoroutine(scene.ConnectArena());
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}
