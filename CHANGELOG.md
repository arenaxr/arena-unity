# Changelog

ARENA-unity notable changes. Started 2021-11-30 (version 0.0.1).

## [0.10.0](https://github.com/arenaxr/arena-unity/compare/v0.9.3...v0.10.0) (2023-09-25)


### Features

* **json:** use full arena json schema, refactor for ArenaComponent base ([#78](https://github.com/arenaxr/arena-unity/issues/78)) ([6ca1490](https://github.com/arenaxr/arena-unity/commit/6ca14905fbc3350e066c5716ecb958b331690994))


### Bug Fixes

* add network latency stats ([95c7ab6](https://github.com/arenaxr/arena-unity/commit/95c7ab677e6fdaeca4cb37530dfb95a8cc15178a))
* **json:** add partially autogen json schema for all wire properties ([#77](https://github.com/arenaxr/arena-unity/issues/77)) ([a6de2b2](https://github.com/arenaxr/arena-unity/commit/a6de2b2aa3e0053485917720c19db40634fd4105))
* **json:** update json schemas + allow array schemas ([c7397cf](https://github.com/arenaxr/arena-unity/commit/c7397cf6c00626e2a086ed8de0b48710a92bdba0))
* reorganize mesh components, deprecate prism ([80d713b](https://github.com/arenaxr/arena-unity/commit/80d713b8f7511436f27e5876778dbe77d83eb598))
* use arena defaults settings from host, separate broker/webhost ([#76](https://github.com/arenaxr/arena-unity/issues/76)) ([01c2393](https://github.com/arenaxr/arena-unity/commit/01c23936bd9ae7d82b2ef9a774e87ee8ed276248))

## [0.9.3](https://github.com/arenaxr/arena-unity/compare/v0.9.2...v0.9.3) (2023-06-06)


### Bug Fixes

* **renderer:** enable/diable renderer from data.visible ([c143444](https://github.com/arenaxr/arena-unity/commit/c143444978fe389a39ec923368a00b1e632face6))

## [0.9.2](https://github.com/arenaxr/arena-unity/compare/v0.9.1...v0.9.2) (2023-06-02)


### Bug Fixes

* **click-listener:** do not auto-add ArenaCamera component ([c2b3b81](https://github.com/arenaxr/arena-unity/commit/c2b3b81ac2c09062a3830571ec3b35285c51444d))
* **object:** check for existing arena object component ([b134689](https://github.com/arenaxr/arena-unity/commit/b134689fca066982170381bca65620a59d4815eb))
* **object:** recursively remove children if any parents are deleted ([d1d81f9](https://github.com/arenaxr/arena-unity/commit/d1d81f92bed972e9fec0c19fce8fc1df3f2f4e4a))
* **object:** use dict update rathar than add arenaObjs ([b20e82d](https://github.com/arenaxr/arena-unity/commit/b20e82ddf289e5b1239926a4a08a6870e52f4126))

## [0.9.1](https://github.com/arenaxr/arena-unity/compare/v0.9.0...v0.9.1) (2023-05-30)


### Bug Fixes

* **auth:** check for failed permissions from bad hostname ([3243872](https://github.com/arenaxr/arena-unity/commit/3243872d2cc7178af202b658e522bf94ae1c958a))
* **click-listener:** accept both legacy string or boolean values ([2b2efd8](https://github.com/arenaxr/arena-unity/commit/2b2efd8cc3ddc6f6e4334f98edf9b6c53b0c91d5))
* **gltf:** fixed runtime loading of multiple .gltf files and assets ([760f977](https://github.com/arenaxr/arena-unity/commit/760f97744d4fdc735fa4c7521d8aabab79cae0f7))
* **hand:** add drawControllerRay option, and fix hand model position ([#70](https://github.com/arenaxr/arena-unity/issues/70)) ([bd3b564](https://github.com/arenaxr/arena-unity/commit/bd3b5640fb2345fbf3a0772aa6de5774c5de33b8))

## [0.9.0](https://github.com/arenaxr/arena-unity/compare/v0.8.0...v0.9.0) (2023-03-07)


### Features

* **line/thickline:** support  object type, also LineRenderer to ([07d832f](https://github.com/arenaxr/arena-unity/commit/07d832f6de8eae2d304ab1943fd22d779b4a15c4))
* **mqtt:** add publish frequency override values per object/camera ([#66](https://github.com/arenaxr/arena-unity/issues/66)) ([370ebac](https://github.com/arenaxr/arena-unity/commit/370ebac6b71151ffdee4b0ea9ad3ec9a42d64e87))
* **thickline:** support prescreened lineWidthStyler algorithms ([f156394](https://github.com/arenaxr/arena-unity/commit/f1563948eda86b7cf4839b88db521d2996a322c9))


### Bug Fixes

* **laser:** add program choice to use laser line or thickline ([0b71b8a](https://github.com/arenaxr/arena-unity/commit/0b71b8aae2084427583db1b5bf0efd58f205f895))
* **material:** improved opacity irregularity in rendering ([1f02a4b](https://github.com/arenaxr/arena-unity/commit/1f02a4b604d8210dd851a0b1bcf361a9a87bd948))
* **material:** use Fade mode for opacity&lt;1 trans==true ([d104a39](https://github.com/arenaxr/arena-unity/commit/d104a39ebe1daa35756167387931d6bfc898e48f))
* **mqtt:** fixed timestamp format ([aec6434](https://github.com/arenaxr/arena-unity/commit/aec6434634450c2e31b0d23b09d0bf673508d207))
* **mqtt:** fixed UTC Zulu timestamp ([0ee16fe](https://github.com/arenaxr/arena-unity/commit/0ee16fe7c25e01afbd12c2fce22c9c20fc10bdce))

## [0.8.0](https://github.com/arenaxr/arena-unity/compare/v0.7.1...v0.8.0) (2023-03-02)


### Features

* **click-listener:** added ArenaClickListener component, added LaserPointer example ([#64](https://github.com/arenaxr/arena-unity/issues/64)) ([b06bd8c](https://github.com/arenaxr/arena-unity/commit/b06bd8cdf81d8beef37de9590fd38c2321371a91))
* **json:** add several json-serializable component classes ([#62](https://github.com/arenaxr/arena-unity/issues/62)) ([632121b](https://github.com/arenaxr/arena-unity/commit/632121b3edadebf049598571134306e9c40e35e1))
* **scene:** preserve ArenaClientScene name during runtime ([57cfdf5](https://github.com/arenaxr/arena-unity/commit/57cfdf532bcce82a0288fc9944ce386515c8b372))


### Bug Fixes

* add deserialization failure check ([12a2872](https://github.com/arenaxr/arena-unity/commit/12a2872e11a79cbbb1b5811fe3467721d5b1746a))
* **animation-mixer:** fixed repetitions default ([e7a412f](https://github.com/arenaxr/arena-unity/commit/e7a412f5cfab6ba8f8d193b3601083b239d31a77))
* **animation-mixer:** handle null property without crashing ([906f4b9](https://github.com/arenaxr/arena-unity/commit/906f4b96dadbc8c83e89446455554733db9dcd4d))
* **animation-mixer:** pub wire msg on edit, fix loop control, require clip ([e1bf687](https://github.com/arenaxr/arena-unity/commit/e1bf6877c6fa6cff1cee35827278687fd23ec734))
* **click-listener:** convert OnEventCallback to pass full msg json as string ([9879ce4](https://github.com/arenaxr/arena-unity/commit/9879ce43c7cef53781c588ca918d688e6b55e696))
* **click-listener:** fixed gltf model collision mesh ([917d597](https://github.com/arenaxr/arena-unity/commit/917d597591b117e88c32e42bc4c6804869552d6f))
* **click-listener:** simplify primitive mesh colliders as convex ([2439ab4](https://github.com/arenaxr/arena-unity/commit/2439ab44580aadc70a73da968be3760398c18e7f))
* **click-listener:** update collision mesh when mesh changes ([a2da15a](https://github.com/arenaxr/arena-unity/commit/a2da15aaa5283eff45b633c901447d90b2d27454))
* **events/persist:** fixed events publish topic and id, added persist complete event ([363c198](https://github.com/arenaxr/arena-unity/commit/363c19813ad2cb2d40defd16cb29b18a96d8e8f5))
* fixed runtime crash while checking permissions ([3a80c97](https://github.com/arenaxr/arena-unity/commit/3a80c975b7648d6b19f4c93ec289094b4120c5f5))
* **mesh:** fixed arena defaults for several geometries ([3bb06f2](https://github.com/arenaxr/arena-unity/commit/3bb06f22eb4f41d82febc5140fb0e9a43eb0f95e))
* **mqtt:** always process locally generated messages ([9266cf7](https://github.com/arenaxr/arena-unity/commit/9266cf7124e1cb5c936dcb443e4670d0c91443b0))

## [0.7.1](https://github.com/arenaxr/arena-unity/compare/v0.7.0...v0.7.1) (2023-02-16)


### Bug Fixes

* **animation-mixer:** avoid AssetDatabase for standalone builds ([8dd27e0](https://github.com/arenaxr/arena-unity/commit/8dd27e0d964033c1ac70f125d5b05b83a4952d91))
* **animation-mixer:** load clip list at gltf file read ([e0b840d](https://github.com/arenaxr/arena-unity/commit/e0b840d4bd68fd59c5d5f17b3a2a72bf6bd15013))
* **hand:** fixed dealyed load of controller models ([e3fc715](https://github.com/arenaxr/arena-unity/commit/e3fc715b1ae94daf4e06f663eb5399399849e43f))

## [0.7.0](https://github.com/arenaxr/arena-unity/compare/v0.6.3...v0.7.0) (2023-02-15)


### Features

* **animation-mixer:** support animation-mixer, test new json parser ([#59](https://github.com/arenaxr/arena-unity/issues/59)) ([09cc529](https://github.com/arenaxr/arena-unity/commit/09cc529f7b0f05aba608dc986c8a8d51440a813c))
* **samples:** add "headless" build option ([#56](https://github.com/arenaxr/arena-unity/issues/56)) ([b11b1aa](https://github.com/arenaxr/arena-unity/commit/b11b1aa4f4152f37cdb27e534cfe4d74d4596844))


### Bug Fixes

* **avatar:** simplified automated naming of exported cameras ([3ec7676](https://github.com/arenaxr/arena-unity/commit/3ec7676c965a6695029baea589b9841c0e9b1b0e))
* **object:** handle poorly formatted model/image content ([9719723](https://github.com/arenaxr/arena-unity/commit/9719723fcb4e0375e32e4fbbd7c5bf7d40c9fcc5))

## [0.6.3](https://github.com/arenaxr/arena-unity/compare/v0.6.2...v0.6.3) (2023-01-28)


### Bug Fixes

* **object:** alway convert rotation to quaternions for wire. Closes [#54](https://github.com/arenaxr/arena-unity/issues/54). ([0dad548](https://github.com/arenaxr/arena-unity/commit/0dad548fe464a991b74c2cf48e9f579b3dab8e30))

## [0.6.2](https://github.com/arenaxr/arena-unity/compare/v0.6.1...v0.6.2) (2023-01-26)


### Bug Fixes

* **hand:** apply proper transforms for hands the same as all gltf ([8a558ae](https://github.com/arenaxr/arena-unity/commit/8a558ae247fb00b0916c948b5f591b3a0abb5348))
* **hand:** locally remove matching hands when camera is deleted ([beff3f3](https://github.com/arenaxr/arena-unity/commit/beff3f37f7760fd857daba57bc8ae375cb26da73))
* **hand:** Use new a-frame hand model urls. Closes [#51](https://github.com/arenaxr/arena-unity/issues/51). ([7318d34](https://github.com/arenaxr/arena-unity/commit/7318d34553695dc2ba7e4dfb87658cd546577b25))

## [0.6.1](https://github.com/arenaxr/arena-unity/compare/v0.6.0...v0.6.1) (2022-12-22)

### Features

* **glTFUtility:** add support for URP/HDRP
* **object:** don't create (or download assests for) GameObjects that already exist in the Unity scene.

## [0.6.0](https://github.com/arenaxr/arena-unity/compare/v0.5.1...v0.6.0) (2022-11-21)

### Features

* **mqtt:** define all messages callback ([6b6c1d0](https://github.com/arenaxr/arena-unity/commit/6b6c1d02050be7e90012263baef3bdd0c26f8786))

## [0.5.1](https://github.com/arenaxr/arena-unity/compare/v0.5.0...v0.5.1) (2022-11-10)


### Bug Fixes

* **update:** allow update manager to handle leading v in version tag ([45b92af](https://github.com/arenaxr/arena-unity/commit/45b92af297aeb9c1b781fc2c97e25c87a1daa086))

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
