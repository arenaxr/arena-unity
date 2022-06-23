#!/bin/bash
# Helper script to transfer desktop mqtt auth token to android package files

hostNameDefault="mqtt.arenaxr.org"
command -v adb >/dev/null 2>&1 || { echo >&2 "adb is not installed.  Aborting."; exit 1; }
adb devices

packageName=$1
hostName="${2:-$hostNameDefault}"
packageDir="/storage/emulated/0/Android/data/$packageName/files/"

# adb shell 'mkdir -m 644 $packageDir; echo $?'
adb push ~/.arena/unity/$hostName/s/.arena_mqtt_auth $packageDir
