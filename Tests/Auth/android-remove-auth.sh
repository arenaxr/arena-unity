#!/bin/bash
# Helper script to remove desktop mqtt auth token from android package files

hostNameDefault="arenaxr.org"
command -v adb >/dev/null 2>&1 || { echo >&2 "adb is not installed.  Aborting."; exit 1; }
adb devices

packageName=$1
packageDir="/storage/emulated/0/Android/data/$packageName/files/"

adb shell 'rm $packageDir/.arena_mqtt_auth; echo $?'
