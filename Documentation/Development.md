# Development Notes

## Android Debug Flow
Tested on Android 10 (API 29).

1. Create a Unity project.
1. Add the ARENA-Unity package.
1. Create a meaningful Package Name like `com.company.appname` in project settings: `Edit > Project Settings > Player > Package Name`.
1. Install a good device debugging package to your project like [LunarConsole](https://assetstore.unity.com/packages/tools/gui/lunar-mobile-console-free-82881).
1. Switch platform to `Android` and `Build and Run` the app to generate the proper Android app data files folder.
1. **[Daily Temporary]**: Switch platform to `PC, Mac & Linux Standalone`.
1. **[Daily Temporary]**: Click the `Play` button to update the MQTT Token for the desktop.
1. **[Daily Temporary]**: Run the bash script `./Tests/Auth/android-add-auth.sh com.company.appname` to copy the desktop MQTT auth token to the Android app file storage.
1. Switch platform to `Android` and `Build and Run` the app.
1. The library will use a local file if it exists for auth at: `/storage/emulated/0/Android/data/com.company.appname/files/.arena_mqtt_auth`.
1. **[Eventually]**: To use the default Android-only auth flow, remove this local token from the device with `./Tests/Auth/android-remove-auth.sh com.company.appname`.
