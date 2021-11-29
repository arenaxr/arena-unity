# ARENA-csharp
C# library for creating applications for the ARENA. **This is a Work-In-Progress.**

The SyncTest project is a test to get all of the auth flow running in Unity to publish objects from Unity to the ARENA. Based on Olivia Lynn's demo: https://github.com/OliviaLynn/Unity-UDP-ARENA-Demo.

It would be good to create a public package for Unity projects to consume.

## Documentation
- ARENA Auth/Messaging: https://arena.conix.io
- Google Auth: https://googleapis.dev/dotnet/Google.Apis.Auth/latest/api/Google.Apis.Auth.html
- Paho MQTT: https://m2mqtt.wordpress.com/m2mqtt_doc

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
1. Copy the modified M2MqttUnity/Assets/M2Mqtt project (based on https://github.com/gpvigano/M2MqttUnity) from this repo to the project's Assets folder.
1. Modify the Project Settings, section Player, Scripted Define Symbols to include: SSL.
1. Modify the Project Settings, section Player, Api Compatibility Level to: .NET 4.x.
1. Attach the component script ArenaClient to the Camera or any empty GameObject, once is enough.
1. Attach the component script ArenaObject to any object you want to publish to the ARENA.
1. Modify the the inspector variables for the ArenaClient script to change host, scene, namespace.
1. Press Play.
1. The auth flow will open a web browser page for you to login.
1. You can also attach the ArenaObject script to any object during Play.

## Architecture
- **ArenaClient** is a Singleton class, meant to be instantiated only once to control the auth and MQTT communication flow.
- **ArenaObject** is a class for each object to publish to the ARENA, accessing the publish and subscribe MQTT methods through **ArenaClient.Instance**.
