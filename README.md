# ARENA-unity
Unity C# library for editing scenes and creating applications for the ARENA.

<img alt="" src="Documentation/arena-unity-demo.gif">

## Library Usage:
1. Open a new or existing Unity project. **Unity 2019.4+ supported.**
1. Open `Edit > Project Settings > Player > Other Settings`.
1. Change `Scripted Define Symbols` to include: `SSL`.
1. Change `Api Compatibility Level` to: `.NET 4.x`.
1. Open `Window > Package Manager` and `+ > Add package from git URL...`, use this link:
    ```
    https://github.com/conix-center/ARENA-unity.git#0.0.11
    ```
1. Create an empty GameObject to use as ARENA client root, rename it to something meaningful, like: `ARENA`.
1. Select the `ARENA` GameObject and press `Add Component` to add the `ArenaClient` script.
1. Modify the the inspector variables for the `ArenaClient` script to change **host, scene, namespace** as you wish.
1. Press **Play**.
1. The auth flow will open a web browser page for you to login, if you haven't yet.

## Documentation
- [Runtime Usage](https://arena.conix.io/content/unity/runtime)
- [Developers Notes](https://arena.conix.io/content/unity/developers)
- [Changelog](https://github.com/conix-center/ARENA-unity/blob/main/CHANGELOG.md)
- [Open Source](https://github.com/conix-center/ARENA-unity/blob/main/Third%20Party%20Notices.md) used and inspired from
