# ARENA-csharp
C# library for creating applications for the ARENA. **This is a Work-In-Progress.**

The SyncTest project is a test to get all of the auth flow running in Unity to publish object from Unity to the ARENA. Based on Olivia Lynn's demo: https://github.com/OliviaLynn/Unity-UDP-ARENA-Demo.

Ultimately it would be good to create NuGet package for Unity projects to consume.

## Proposed TODO List:
- Determine if NuGet is the right package manager for this, ideas: https://medium.com/runic-software/simple-guide-to-unity-package-management-4aea43d1baf7
- Determine right level of logging
- Determine right platforms to support, UI required
- Add a smooth disconnect/logout experience
- Move code to a Unity-specific repo, C# is too general a repo name
- Expand the object properties that can be synchronized between Unity-ARENA
- Use case discovery to determine the right level of Unity-ARENA interaction

## Rough build steps used to build SyncTest:
1. Create an empty SyncTest Unity project.
1. Add NuGet community package manager to Unity runtime: https://github.com/GlitchEnzo/NuGetForUnity
1. Use Nuget manager to install: https://www.nuget.org/packages/Google.Apis.Auth
1. Use Nuget manager to install: https://www.nuget.org/packages/Google.Apis.Oauth2.v2
1. Unity doesn't load the platform builds of the Paho MQTT .NET (https://github.com/eclipse/paho.mqtt.m2mqtt) package (https://www.nuget.org/packages/M2Mqtt) yet so we're rolling our own for now.
1. Copy the modified M2MQTTUnity project from this repo to the project's Assets folder.
1. Modify the project's build settings to include the Scripted Define Symbols: SSL.
1. Attach the component script ArenaClient to the Camera or any empty GameObject.
1. Attach the component script ArenaObject to any object you want to publish to the ARENA.
1. Modify the the inspector variables for the ArenaClient script to change host, scene, namespace.
1. Press Play.
1. The auth flow will open a web browser page for you to login.
1. You can also attach the ArenaObject script to any object during Play.

## Architecture
- **ArenaClient** is a Singleton class, meat to be instantiated only once to control the auth and MQTT communication flow.
- **ArenaObject** is a class for each object to publish to the ARENA, accessing the publish and subscribe MQTT methods through **ArenaClient.Instance**.
