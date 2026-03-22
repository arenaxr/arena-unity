# arena-unity
Unity C# library for editing scenes and creating applications for the ARENA.

<img alt="" src="Documentation~/arena-unity-demo.gif">

## Installation

1. Open a new or existing Unity project. **Unity 2022.3+ supported.**
2. `Edit > Project Settings > Player > PC, Mac & Linux Standalone > Resolution and Presentation > Resolution`:
    * `Run In Background` set to `true`.
3. Open `Window > Package Manager` and click `+ > Add package from git URL...`. 
4. Enter `https://github.com/arenaxr/arena-unity.git` and click **Add**.

## User Quick Start

1. Create an empty GameObject in your scene (e.g., name it `ARENA`).
2. Select the `ARENA` GameObject and press **Add Component** to attach the `ArenaClientScene` script.
3. Modify the inspector variables for the `ArenaClientScene` script (e.g., Host Address, Namespace Name, Scene Name).
4. Press **Play** in the Unity Editor. The auth flow will open a browser to log in.
5. Persistent ARENA objects will automatically load as child GameObjects. Any modified properties will sync remotely.

> **Note:** For advanced usage like exporting GLTF models or using the ARENA Mesh Tool, see the [ARENA Unity Editor Documentation](https://docs.arenaxr.org/content/unity/editor).

## Developer Quick Start

You can programmatically connect and interact with an ARENA scene using the `ArenaClientScene` singleton.

```csharp
using ArenaUnity;
using UnityEngine;

public class ArenaDemo : MonoBehaviour
{
    private ArenaClientScene scene;

    void Start()
    {
        scene = ArenaClientScene.Instance;
        scene.hostAddress = "arenaxr.org";
        scene.namespaceName = "public";
        scene.sceneName = "example";
        
        StartCoroutine(scene.ConnectArena());
    }

    void Update()
    {
        if (scene.mqttClientConnected)
        {
            // Connected! You can now publish objects or listen to messages.
        }
    }
}
```

> **Note:** For detailed API examples, custom message processing, and comprehensive component references, see the [ARENA Unity Runtime Documentation](https://docs.arenaxr.org/content/unity/runtime) and [Requirements Architecture](REQUIREMENTS.md).

## Library Reference

* [Requirements & Architecture](REQUIREMENTS.md) (Detailed component mappings and source index)
* [Components Priority](COMPONENTS_PRIORITY.md)
* [Contributing](CONTRIBUTING.md)
* [Release Checklist](RELEASE.md)
* [Changelog](CHANGELOG.md)
* [Open Source](Third%20Party%20Notices.md) used and inspired from

## Alternate Server Usage

In addition to the above, you may deploy your own ARENA webserver if you do not wish to use our test deployment server at [arenaxr.org](https://arenaxr.org). You can follow our setup guidelines in [arenaxr/arena-services-docker](https://github.com/arenaxr/arena-services-docker) to run your own server.
