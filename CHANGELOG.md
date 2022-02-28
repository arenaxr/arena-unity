# Changelog

ARENA-unity notable changes. Started 2021-11-30 (version 0.0.1).

## [0.0.13] - 2022-02-28
### Added
- Mobile logging of persistence download objects.
- iOS builds enable file-sharing post-build.
### Changed
- Reworked `ArenaClient` APIs for improved connect/disconnect/status.
- Made `ArenaClient` permission variables read-only.
- Mobile builds no longer automatically connect in Play mode.

## [0.0.12] - 2022-02-15
### Added
- Local MQTT token auth option.
- UI/console MQTT expiration clock.
- Added meshes for dodecahedron, tetrahedron, capsule, triangle.
- Added triangle Mesh Tool editor.
### Changed
- Removed Editor features from mobile Runtime.
- Fixed application paths for Windows.

## [0.0.11] - 2022-01-21
### Added
- `ARENA Mesh Tool` to add 3D control handles for primitive meshes.
- `ArenaMesh` components for all primitives, to edit render dimensions.
- Automatic publish updates for all primitive mesh changes.
- Animation test `Play/Stop/Rewind` buttons for all GLTF animations.
- Clickable scene URL on `ArenaCLient` component.
### Changed
- Improved error handling for missing GTLF sub-files.
- Fixed cone mesh origin.
- Resolved Unity Quad/Plane/Capsule primitives.

## [0.0.10] - 2022-01-14
### Added
- Prevent inadvertent edits of GLTF sub-components with `Static`.
- Improved accuracy of `Json Data` in realtime.
- Highlight ARENA objects in `Hierarchy View` in green.
- Expand and focus `ArenaClient` component and objects list on start.
- Made `Json Data` validated on realtime edits, publish with `Publish Json Data` button.
- Support loading material textures.
- Support separate `Scale` vs `Renderer` dimensions via manual `Mesh` calculations.
- Support geometries: `Ring, Torus, Plane, Icosahedron, Octahedron, Cone`.
- Added geometries to `GameObject > ARENA`.
- Support GLTF Load on Demand (LOD), loading both models.
### Changed
- Import only visual ARENA assets, no audio or video.
- Check for `GameObject > ARENA` menu item name conflicts.
- Prevent TRS publish of objects without TRS (`scene-options, program`).

## [0.0.9] - 2022-01-06
### Added
- Live import of models/images from MQTT messages.
- `GameObject > ARENA` menu items to quickly add models/images by URL.
- Allow `Undo` of ARENA objects, handled by delete system.
- Support cast/receive shadows.
### Changed
- Fixed unparenting publish.

## [0.0.8] - 2022-01-04
### Added
- Consolidated delete conformation.
- Transform-only object updates.
- Sort ARENA Components to top of inspector.
- Signout button on ArenaClient component.
### Changed
- Fixed inaccurate merge of json data.
- Minor updates to rename, parenting.

## [0.0.7] - 2022-01-03
### Added
- Import images as sprites, stored in project `/Assets` folder.
- Accurate download progress dialog.
- Manual publish button for objects.
### Changed
- Allowed legacy color import.
- Fixed runtime transparency/opacity.
- Fixed parent/child position.
- Minor updates to ambient lights, shaders.

## [0.0.6] - 2021-12-30
### Changed
- Fixed samples folder missing meta files.

## [0.0.5] - 2021-12-29
### Added
- Allow `.gltf` GLTF load.
- Saves all GLTF files locally in project.
- GLTF import progress dialog.
- Ability to browse GLTF components in project `/Assets` folder.
- Support for lights, transparency, opacity.
- Delete confirmations.
- Ability to select an ARENA camera view to mimic.
- `ARENA > Signout` menu option.
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
