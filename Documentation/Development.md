# Development Notes

## Android Debug Flow
Tested on Android 10 (API 29).

1. Follow the [startup project setup](https://github.com/conix-center/ARENA-unity#library-usage).
1. `Edit > Project Settings > Player`: Create a meaningful Package Name like `com.companyname.appname`...
    - `Company Name`: `companyname` (sample)
    - `Product Name`: `appname` (sample)
    - `Android > Identification > Package Name`: `com.companyname.appname` (sample)
1. `Edit > Project Settings > Player > Android > Other Settings > Identification`:
    - `Minimum API Level`: at least API 24 (for XR/ARCore).
1. `Edit > Project Settings > Player > Android > Other Settings > Configuration:`:
    - `Api Compatibility Level` to: `.NET 4.x`.
    - `Install Location` to: `Automatic` or `Force Internal`.
    - `Internet Access` to: `Require`.
    - `Write Permission` to: `Internal`.
1. `Edit > Project Settings > Player > Android > Other Settings > Script Compilation`:
    - `Scripted Define Symbols` to include:
        - `SSL`
1. `Edit > Project Settings > Player > Graphics > Video`:
    - `Always Included Shaders` to include:
        - `Standard`
        - `Unlit/Color`
        - `GLTFUtility/Standard (Metallic)`
        - `GLTFUtility/Standard Transparent (Metallic)`
        - `GLTFUtility/Standard (Specular)`
        - `GLTFUtility/Standard Transparent (Specular)`
1. Install a good device debugging package to your project like [LunarConsole](https://assetstore.unity.com/packages/tools/gui/lunar-mobile-console-free-82881).
1. Switch platform to `Android` and `Build and Run` the app to generate the proper Android app data files folder.
1. **[Daily Temporary]**: Switch platform to `PC, Mac & Linux Standalone`.
1. **[Daily Temporary]**: Click the `Play` button to update the MQTT Token for the desktop.
1. **[Daily Temporary]**: Run the bash script `./Tests/Auth/android-add-auth.sh com.companyname.appname` to copy the desktop MQTT auth token to the Android app file storage.
1. Switch platform to `Android` and `Build and Run` the app.
1. The library will use a local file if it exists for auth at: `/storage/emulated/0/Android/data/com.companyname.appname/files/.arena_mqtt_auth`.
1. **[Eventually]**: To use the default Android-only auth flow, remove this local token from the device with `./Tests/Auth/android-remove-auth.sh com.companyname.appname`.
