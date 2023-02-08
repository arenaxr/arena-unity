# ArenaHeadless
A sample application to integrate ARENA and manage objects from a headless server application:
- Login to the ARENA scene
- Modify some objects
- Quit

## Option 1 (from build):
1. Follow [this](https://docs.arenaxr.org/content/unity/build.html) to setup the build enviornment. Make sure the required shaders are included!
2. Add the `HeadlessCLI.cs` script to a Game Object. Feel free to edit it.
3. Build your application. File -> Build Settings -> Build -> choose name and path.
4.

If MacOS:
```shell
cd <path to built executable>
open <executable name>.app --args <namespace> <scenename>
```
If Linux:
```shell
cd <path to built executable>
./<executable name>.x86_64 <namespace> <scenename>
```

## Option 2 (from headless editor):
MacOS:
```shell
cd <this ArenaHeadless project root>
/Applications/Unity/Hub/Editor/2019.2.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -logFile - -executeMethod ArenaHeadless.Play
```
