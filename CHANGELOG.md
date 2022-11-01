# Changelog

ARENA-unity notable changes. Started 2021-11-30 (version 0.0.1).

## [0.5.0](https://github.com/arenaxr/arena-unity/compare/v0.4.0...v0.5.0) (2022-11-01)


### Features

* **auth:** add auth state to hierarchy window ([#47](https://github.com/arenaxr/arena-unity/issues/47)) ([834574f](https://github.com/arenaxr/arena-unity/commit/834574f455772cb8e0b6b98672b69da80e132c1d))
* **auth:** show object perms in console and inspector ([f2d5a52](https://github.com/arenaxr/arena-unity/commit/f2d5a52f1cd6ec112701995ba828718f21eaaca6))
* **avatar:** render hands from remote immersive vr ([#48](https://github.com/arenaxr/arena-unity/issues/48)) ([11aeb7c](https://github.com/arenaxr/arena-unity/commit/11aeb7c0cb27b4555cb0466bd8ed45801b66ae0e))
* **text:** support unity text objects port to arena ([b9673da](https://github.com/arenaxr/arena-unity/commit/b9673da1eb40289ca2d64982e99e38b0d446e105))


### Bug Fixes

* **auth:** corrected namespace conflict ([d7603b9](https://github.com/arenaxr/arena-unity/commit/d7603b93c0b3e8e9c3f9eb7a16c54e92f22f2d21))
* **auth:** fix object perms in console during building ([2ed1b4e](https://github.com/arenaxr/arena-unity/commit/2ed1b4ef29c20f2c027d29f27c1c441bff71b2e6))
* **import:** fixed random progress meters hanging around ([0eb602a](https://github.com/arenaxr/arena-unity/commit/0eb602a686b926ff5436574402a1fdc15411e487))
* **mqtt:** updated publish rate to 10 Hz for transform changes ([5912fd6](https://github.com/arenaxr/arena-unity/commit/5912fd6f85fced50ad2b28a00959104a77328e3c))
* **object:** fix serialization create flag to allow object copy ([958f84c](https://github.com/arenaxr/arena-unity/commit/958f84caaf7cf4a0e2b6d22f18f93cf12095540b))
* protect against bad formatted assets ([b2e8e32](https://github.com/arenaxr/arena-unity/commit/b2e8e327bd9e0a36ed53bd0b756483b88a51d209))
* updated missing package version number v0.4.1 ([6d02f93](https://github.com/arenaxr/arena-unity/commit/6d02f935743f6dd97ceae010de1054bbdcf0fd22))

## [0.4.0](https://github.com/arenaxr/arena-unity/compare/v0.3.1...v0.4.0) (2022-10-17)


### Features

* **avatar:** renderCameras should also not attach Camera component ([64e7124](https://github.com/arenaxr/arena-unity/commit/64e71243296c2653f2693921f42027e2f923314c))
* **thickline:** render arena thickline ([e73a7ac](https://github.com/arenaxr/arena-unity/commit/e73a7ac96761a38b04f027a6fe2e7147b62efd6e))
* **ttl:** add ttl timer for short-term objects ([85adec9](https://github.com/arenaxr/arena-unity/commit/85adec9a4bc95a493e7f7566f3e6f27eb2875cb3))
* **videosphere:** use sphere as a videosphere placeholder ([cdae1c4](https://github.com/arenaxr/arena-unity/commit/cdae1c4e2d9cb412608a89c414f36367f3601c09))


### Bug Fixes

* **all:** rework live updates to handle parent/text/models better ([#45](https://github.com/arenaxr/arena-unity/issues/45)) ([4f8aea3](https://github.com/arenaxr/arena-unity/commit/4f8aea3567af7325ddfecbb9ee15524bd94cfc7e))
* **camera:** do not render on default display 1 ([49a3495](https://github.com/arenaxr/arena-unity/commit/49a34951d77d7a8f7653c21d997403e9f4da9ed9))
* **gltf-model:** allow spaces in model urls ([773e18a](https://github.com/arenaxr/arena-unity/commit/773e18ad51f5e41826510059bb6ac96ab464e228))
* **gltf:** allow builds to run legacy animation clips ([775d899](https://github.com/arenaxr/arena-unity/commit/775d899d6970d0ef28dfaf5514302ee14bd6f0da))
* hot patch: external realtime updates disabled ([ed76ff4](https://github.com/arenaxr/arena-unity/commit/ed76ff4d44998c17d9507645b682c9a3b4154de3))
* **thickline:** fix z position of thickline nodes ([5d1233c](https://github.com/arenaxr/arena-unity/commit/5d1233ceb8f37da1deecb0140a0001f3a1162ba0))

## [0.3.1](https://github.com/arenaxr/arena-unity/compare/v0.3.0...v0.3.1) (2022-10-03)


### Bug Fixes

* **avatar:** fix local coordinates to parent ([b5bbee0](https://github.com/arenaxr/arena-unity/commit/b5bbee09b337ec98c252413c0dac5ee60cfd468a))
* **mac:** remove xcode-specific build from non-macs ([e1912d9](https://github.com/arenaxr/arena-unity/commit/e1912d947af6d1910822fde957c2e049a9c35cf6))
* **packages:** upgrade newtonsoft-json and google.auth ([b932987](https://github.com/arenaxr/arena-unity/commit/b93298731f92147411d529707ddff9365bafd343))
* **shader:** check for required shaders in project ([7100bb2](https://github.com/arenaxr/arena-unity/commit/7100bb2c18f9c0d8ea25103870863e35fd5d5b39))
* **text:** fixed text occlusion with textmeshpro ([#41](https://github.com/arenaxr/arena-unity/issues/41)) ([68dd09c](https://github.com/arenaxr/arena-unity/commit/68dd09c95ff13faa59b4a0f136cdd0adc5f1d790))

## [0.3.0](https://github.com/arenaxr/arena-unity/compare/v0.2.0...v0.3.0) (2022-09-16)


### Features

* **avatar:** add displayName to other user's avatars ([283269d](https://github.com/arenaxr/arena-unity/commit/283269d94e0d3dd2d9adfc4719fbb628ce0b05e9))
* **avatar:** add flag to disable render remote cameras in the scene ([b53ead3](https://github.com/arenaxr/arena-unity/commit/b53ead372c78c13d1a2a6ec16c69573ecb98c8f4))
* **avatar:** render other user's avatars; fix redundant asset downloads ([fd295d2](https://github.com/arenaxr/arena-unity/commit/fd295d2090c7898d7399ffd3048fd4b636f51380))
* **camera:** allow multiple cameras with ArenaCamera component ([f65c546](https://github.com/arenaxr/arena-unity/commit/f65c546c759509e1fd6bb2a050973156eb434689))
* **text:** support importing arena a-text ([e104326](https://github.com/arenaxr/arena-unity/commit/e10432640744544038c92f32d10164f0a49845e7))


### Bug Fixes

* **auth:** throw an error when restricted users use more than one camera ([a70c9e9](https://github.com/arenaxr/arena-unity/commit/a70c9e9078218ec41def92667e53118bbea195c3))
* **avatar:** correctly handle local/remote cam deletes ([5d94f4b](https://github.com/arenaxr/arena-unity/commit/5d94f4bba6a68eabf87df86e84ccbb839fc1c146))
* **avatar:** fix inconsistant head model positions ([367b178](https://github.com/arenaxr/arena-unity/commit/367b17880adedb4b359966f6d2741e1e116f4f82))
* **avatar:** use correct static default avatar model ([c4361d7](https://github.com/arenaxr/arena-unity/commit/c4361d7a4ab64a1ae52b2ca918f6e8fdc8dcc3c4))
* prevent accidental overwrite of custom namespace ([9e28938](https://github.com/arenaxr/arena-unity/commit/9e28938891d6ce4520c22dfc3acfc5c18718a8f0))

## [0.2.0](https://github.com/arenaxr/arena-unity/compare/v0.1.0...v0.2.0) (2022-09-13)


### Features

* **auth:** provide simple/scene-specific signin options ([3d22bc3](https://github.com/arenaxr/arena-unity/commit/3d22bc3b586f9bfff1f95894bf1638c7c4355b46))
* **camera:** publish main/user-selected camera avatar, manage shutdown/lwt deletes ([#37](https://github.com/arenaxr/arena-unity/issues/37)) ([0d1c15c](https://github.com/arenaxr/arena-unity/commit/0d1c15c0c6317ec458655da05ee9f6062a2458e9))


### Bug Fixes

* **auth:** remove client id from scene topic, ATM anonymous user has no client id rights ([232845c](https://github.com/arenaxr/arena-unity/commit/232845c4954411477fc30e3ed61baa34c9faf86c))
* **auth:** resolve publish object/camera topics with/wo client id ([61b02d7](https://github.com/arenaxr/arena-unity/commit/61b02d734b01a7ccd447d04792aab76181ae8e87))
* **avatar:** don't recreate the unity client camera ([d89e2fa](https://github.com/arenaxr/arena-unity/commit/d89e2fafb2f13fb7b61f3d39a8e4e3e500724ad1))
* **editor:** give arena object json more room to edit ([4f8b090](https://github.com/arenaxr/arena-unity/commit/4f8b090a5f85efa7e778ff6277c88de43431aa2b))

## [0.1.0](https://github.com/arenaxr/arena-unity/compare/v0.0.14...v0.1.0) (2022-09-09)


### Features

* **auth:** add anonymous login type ([a497327](https://github.com/arenaxr/arena-unity/commit/a4973276b159ae9b99884f3c69fe17de717ab79f))
* **auth:** migrate auth flow to ArenaMqttClient ([4c00864](https://github.com/arenaxr/arena-unity/commit/4c008649560d3910d2f6d9d0b9101c254263639f))
* major refactor mqtt, ArenaClient => ArenaClientScene ([6709cc2](https://github.com/arenaxr/arena-unity/commit/6709cc25927346c9591446c487172bf24cf853b6))
* **mqtt:** add custom event publish ([0e07bb9](https://github.com/arenaxr/arena-unity/commit/0e07bb95279388c0e811775c1378cd6eae43278c))
* **mqtt:** allow custom publish/subcribe ([6a75797](https://github.com/arenaxr/arena-unity/commit/6a75797ba228a53bfba6aa6e197691d0c72c534a))
* **objects:** color arena objects that have scripts attached ([9b5044e](https://github.com/arenaxr/arena-unity/commit/9b5044eeac3ba3c4ce38b82276e96d81d4debf69))
* **objects:** ensure local arena objects take priority ([e288c5e](https://github.com/arenaxr/arena-unity/commit/e288c5e6d10ef89bea1654182e0e2970519b75e5))
* **objects:** prevent arena objects of the same name at start ([a6afe98](https://github.com/arenaxr/arena-unity/commit/a6afe98ca0a58d43b40325e5d3be67f2c7398e3b))
* **scene:** place arena objects in scene, not ArenaClient children ([a8f65ed](https://github.com/arenaxr/arena-unity/commit/a8f65edbc8870baa803ae47329bc9fb697cd28f4))


### Bug Fixes

* **menu:** auto-renamed arena context menu publish topic ([04ae465](https://github.com/arenaxr/arena-unity/commit/04ae4654288fca6525504b214ce477d06ab09319))
* **naming:** allow html5 non-whitespace object ids ([4087a86](https://github.com/arenaxr/arena-unity/commit/4087a8652a7a4311910afc3ce7b94b0c6080c187))
* **objects:** allow menu arena object create with mqtt only ([71f1f25](https://github.com/arenaxr/arena-unity/commit/71f1f2560f4654cf2b4f24491fa269e31e4aa6a7))
* **objects:** remove problimatic auto-rename ([65a2512](https://github.com/arenaxr/arena-unity/commit/65a2512ea135cb752d9c2cb7bc3f9c830edf27f1))
* **primitive:** match more primitives to a-frame defaults ([42e57ce](https://github.com/arenaxr/arena-unity/commit/42e57cee336b756c20257e783cf136ea86d4a4e3))

## [0.0.14](https://github.com/conix-center/ARENA-unity/compare/0.0.13...v0.0.14) (2022-07-14)


### Bug Fixes

* **editor:** improve arena object inspector color light theme ([213f263](https://github.com/conix-center/ARENA-unity/commit/213f2636fe7ad876bb98bf2b7300fd0ee869d53c))
* spelling ([c8d364f](https://github.com/conix-center/ARENA-unity/commit/c8d364fcae08dfd6100ef622ab6f8cabbef02743))

## [0.0.13] - 2022-02-28
### Added
- Mobile logging of persistence download objects.
- iOS builds enable file-sharing post-build.
### Changed
- Reworked `ArenaClientScene` APIs for improved connect/disconnect/status.
- Made `ArenaClientScene` permission variables read-only.
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
- Expand and focus `ArenaClientScene` component and objects list on start.
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
- Signout button on ArenaClientScene component.
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
- Load objects from persistence database.
- Publish transform changes as updates to ARENA.
- Handle updates from subscription updates to ARENA.
- Converted to Unity Package Manager format.
