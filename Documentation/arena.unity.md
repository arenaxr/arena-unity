# ARENA-Unity Documentation

## During Runtime (Play)

1. If objects are stored in the ARENA Persistence Database, they will be child objects of the `ARENA` GameObject.
1. You may create or change an object and if it is a child of the `ARENA` GameObject, its properties will be published to the ARENA Persistence Database.
1. Incoming authorized messages may also add/change/remove your ARENA Unity objects.

![unity-desktop.png](unity-desktop.png)

## ArenaClient Script

| type | name | default | description |
| -- | -- | -- | -- |
| string | Broker Address | "arenaxr.org" | Host name of the ARENA MQTT broker |
| string | Namespace Name | null | Namespace (automated with username), but can be overridden |
| string | Scene Name | "example" | Name of the scene, without namespace ('example', not 'username/example' |
| string | Scene Url | null | Browser URL for the scene |
| Camera | Camera For Display | MainCamera | Cameras for Display 1 |
| bool | Camera Auto Sync | false | Synchronize camera display to first ARENA user in the scene |
| bool | Log Mqtt Objects | false | Console log MQTT object messages |
| bool | Log Mqtt Users | false | Console log MQTT user messages |
| bool | Log Mqtt Events | false | Console log MQTT client event messages |
| bool | Log Mqtt Non Persist | false | Console log MQTT non-persist messages |
| int | Publish Interval | 30 | in publish per frames Frequency to publish detected changes by frames (0 to stop) |
| string | Email | null | Authenticated user email account |
| string | Permissions | null | MQTT JWT Auth Payload and Claims |
