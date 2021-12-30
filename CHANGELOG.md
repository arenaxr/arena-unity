# Changelog

ARENA-unity notable changes. Started 2021-11-30 (version 0.0.1).

## [0.0.5] - 2021-12-29
### Added
- Allow `.gltf` GLTF load.
- Saves all GLTF files locally in project.
- GLTF import progress dialog.
- Ability to browse GLTF components in project `/Assets` folder.
- Support for lights, transparency, opacity.
- Delete confirmations.
- Ability to select an ARENA camera view to mimic.
- `ARENA | Signout` menu option.

### Changed
- Update `GTLFUtility` to https://github.com/Siccity/GLTFUtility/commit/0392488470b79e74b88676e023653de4ada63194.
- Fixed GTLF to Unity transforms.
- Round floats to ARENA granularity.
- Minor fixes and improved error reporting.

## [0.0.4] - 2021-12-07
### Changed
- Reduced frequency of GitHub version checking.

## [0.0.3] - 2021-12-06
### Added
- Version upgrade reminder direct from latest GitHub release tag.

## [0.0.2] - 2021-12-06
### Added
- Allow `.glb` GLTF load.
- ARENA cameras added to displays.
- Consistent naming`GameObject.name` == `object_id`
- Allow renaming of ARENA objects.
- Option to prevent logging of non-persist objects.
- Package version check in Editor.
- Fixed Unity-side object publish.

## [0.0.1] - 2021-11-30
### Added
- M2MqttUnity for ARENA TLS auth.
- Google auth for account ACL links.
- Load objects from persistance database.
- Publish transform changes as updates to ARENA.
- Handle updates from subscription updates to ARENA.
- Converted to Unity Package Manager format.
