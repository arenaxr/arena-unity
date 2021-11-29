# ARENA-csharp
Unity C# library for creating applications for the ARENA.
An early version was based on Olivia Lynn's demo: https://github.com/OliviaLynn/Unity-UDP-ARENA-Demo.
**This is a Work-In-Progress.**

## Documentation
- ARENA Auth/Messaging: https://arena.conix.io
- Google Auth: https://googleapis.dev/dotnet/Google.Apis.Auth/latest/api/Google.Apis.Auth.html
- Paho MQTT: https://m2mqtt.wordpress.com/m2mqtt_doc

## Library Usage:
1. Open a new or existing Unity project.
1. Modify the `Project Settings | Player | Other Settings`.
1. Change `Scripted Define Symbols` to include: `SSL`.
1. Change `Api Compatibility Level` to: `.NET 4.x`.
1. Open the `Window | Package Manager` and `+ | Add package from git URL...`, use this link: `https://github.com/conix-center/ARENA-csharp.git`.
1. Create an empty GameObject to use as ARENA client root, rename it to something meaningful, like: `ARENA`.
1. Select the `ARENA` GameObject and press `Add Component` to add the `ArenaClient` script.
1. Modify the the inspector variables for the `ArenaClient` script to change host, scene, namespace as you wish.
1. Press **Play**.
1. The auth flow will open a web browser page for you to login, if you haven't yet.

## During Runtime (Play)
1. If objects are stored in the ARENA Persistence Database, they will be child objects of the `ARENA` GameObject.
1. You may create or change an object and if it is a child of the `ARENA` GameObject, its properties will be published to the ARENA Persistence Database.
1. Incoming authorized messages may also add/change/remove your ARENA Unity objects.

## Architecture
- The `.NET 4.x` API level is required since ARENA JSON payloads are fluid, and we cannot keep up with schema serialization definitions by developers and users. So we use the `dynamic` object instantiations offered in the .Net 4 API to test for JSON attributes at runtime.
- **ArenaClient** is a Singleton class, meant to be instantiated only once to control the auth and MQTT communication flow.
- **ArenaObject** is a class for each GameObjects to publish to the ARENA, accessing the publish and subscribe MQTT methods through **ArenaClient.Instance**. `ArenaClient` will manage attaching `ArenaObject` to Unity GameObjects for you.

## Deprecated Support Notes
1. Determine when/how these steps are still needed (TODO mwfarb):
1. Add NuGet community package manager to Unity runtime: https://github.com/GlitchEnzo/NuGetForUnity
1. Use Nuget manager to install: https://www.nuget.org/packages/Google.Apis.Auth
1. Use Nuget manager to install: https://www.nuget.org/packages/Google.Apis.Oauth2.v2
1. Unity doesn't load the platform builds of the Paho MQTT .NET (https://github.com/eclipse/paho.mqtt.m2mqtt) package (https://www.nuget.org/packages/M2Mqtt) yet so we're rolling our own for now.
1. Copy the modified M2MqttUnity/Assets/M2Mqtt project (based on https://github.com/gpvigano/M2MqttUnity) from this repo to the project's Assets folder.

## Proposed TODO List:
- Determine right platforms to support, UI required
- Add a smooth disconnect/logout experience
- Move code to a Unity-specific repo, C# is too general a repo name
- Expand the object properties that can be synchronized between Unity-ARENA
- Use case discovery to determine the right level of Unity-ARENA interaction
