#!/bin/bash
# Helper script to transfer desktop mqtt auth token to android package files

hostNameDefault="arenaxr.org"

command -v adb >/dev/null 2>&1 || { echo >&2 "adb is not installed.  Aborting."; exit 1; }

packageName=$1
hostName="${2:-$hostNameDefault}"
adb shell 'echo $EXTERNAL_STORAGE; echo $?'
adb shell 'echo $INTERNAL_STORAGE; echo $?'
packageDir="/storage/emulated/0/Android/data/$packageName/files/"

adb devices
adb shell 'mkdir -m 644 $packageDir; echo $?'
adb push ~/.arena/unity/$hostName/s/.arena_mqtt_auth $packageDir
