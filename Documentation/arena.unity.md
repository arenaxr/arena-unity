# ARENA-Unity Documentation

## During Runtime (Play)

1. If objects are stored in the ARENA Persistence Database, they will be child objects of the `ARENA` GameObject.
1. You may create or change an object and if it is a child of the `ARENA` GameObject, its properties will be published to the ARENA Persistence Database.
1. Incoming authorized messages may also add/change/remove your ARENA Unity objects.

![unity-desktop.png](unity-desktop.png)

## ArenaClient Script

name | type | default | description
-- | -- | -- | --
Script | ArenaClient | -- | The script instance to manage the MQTT runtime.
Broker Address | string | arenaxr.org | Host name of the ARENA MQTT broker
Namespace Name | string | null | Namespace (automated with username), but can be overridden
Scene Name | string | example | Name of the scene, without namespace ('example', not 'username/example'
Scene Url | string | null | Browser URL for the scene
Camera For Display | Camera | MainCamera | Cameras for Display 1
Camera Auto Sync | bool | false | Synchronize camera display to first ARENA user in the scene
Log Mqtt Objects | bool | false | Console log MQTT object messages
Log Mqtt Users | bool | false | Console log MQTT user messages
Log Mqtt Events | bool | false | Console log MQTT client event messages
Log Mqtt Non Persist | bool | false | Console log MQTT non-persist messages
Transform Publish Interval | int | 30 | Publish per frames frequency to publish detected transform changes (0 to stop)
Email | string | null | Authenticated user email account
Permissions | string | null | MQTT JWT Auth Payload and Claims

## ArenaObject Script

name | type | default | description
-- | -- | -- | --
Publish Object Update | button | -- | Manual button to publish an object update (transform changes will update automatically)
Script | ArenaObject | -- | The script instance to manage an ARENA object runtime.
Message Type | string | object | Message type in persistance storage schema
Persist | bool | true | Persist this object in the ARENA server database (default true = persist on server)
Json Data | string | null | ARENA JSON-encoded message (debug only for now)
