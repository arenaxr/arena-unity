# ARENA Unity Components Prioritization

Based on a review of the unimplemented components (`// TODO: Implement this component if needed`) inside `Runtime/Components`, here is a recommended prioritization list. This list is organized by how quickly and effectively they can be implemented using **existing native Unity libraries**.

## Priority 1: The "Low-Hanging Fruit" (Native Unity Equivalents)
These components map almost 1:1 to built-in Unity components and require very little custom logic.

* **`ArenaSound`**: Maps directly to Unity's `AudioSource`. Properties like `volume`, `loop`, `autoplay`, and `positional` (`spatialBlend`) are trivial to assign. The only minor work is loading an AudioClip from a URL.
* **`ArenaShadow`**: Maps natively to Unity's `MeshRenderer.shadowCastingMode` and `receiveShadows`. This is extremely easy to toggle.
* **`ArenaGltfModelLod`**: Unity has a native `LODGroup` component built exactly for this purpose. You just need to instantiate the models and assign them to the LOD threshold array.
* **`ArenaGotoUrl` & `ArenaGotoLandmark`**: `GotoUrl` is a one-liner: `Application.OpenURL(url)`. `GotoLandmark` is a simple `Transform.SetPositionAndRotation` for the main camera rig.
* **Physics (`ArenaStaticBody`, `ArenaDynamicBody`, `ArenaImpulse`)**:
  * *Note: `static-body`, `dynamic-body`, and `impulse` have been deprecated in favor of `physx-*` style properties (e.g., `physx-body`, `physx-material`, `physx-joint`).*
  * However, mapping them to Unity remains identical to Unity's Physics system (`Rigidbody` and `Collider`).
  * `StaticBody` / `physx-body (static)` = `Collider` with no Rigidbody.
  * `DynamicBody` / `physx-body (dynamic)` = `Rigidbody` (handling mass, drag, etc.).
  * `Impulse` = `Rigidbody.AddForce(..., ForceMode.Impulse)`.
* **`ArenaBoxCollisionListener`**: Very simple to hook up using Unity's `MonoBehaviour.OnCollisionEnter`.
* **`ArenaGltfMorph`**: Maps natively to `SkinnedMeshRenderer.SetBlendShapeWeight`.

## Priority 2: Material & Scene Rendering (Medium)
These use native Unity features but require more specific property mapping or Unity package configurations.

* **`ArenaMaterialExtras`**: Adding normal maps, metallic/roughness tweaks, and texture offsets. These map perfectly to properties on Unity's Standard or URP shaders (e.g., `_Metallic`, `_BumpMap`).
* **`ArenaSceneRendererSettings` / `ArenaScenePostProcessing`**: Unity has a robust Post-Processing logic (or URP Volume Profiles) that handles Bloom, SSAO, etc. It requires instantiating a Volume Profile at runtime and toggling its overrides.
* **`ArenaSceneEnvPresets`**: Can be mapped to assigning predefined Unity Skybox materials and tweaking the `RenderSettings.ambientLight` or Directional Light angles.

## Priority 3: UI & Complex Integrations (Harder)
These require building custom layout engines or integrating external 3rd-party packages.

* **ARENA UI (`ArenaWireArenauiCard`, `ArenaWireArenauiButtonPanel`, `ArenaWireArenauiPrompt`, `ArenaTextinput`)**: Unity's Canvas and `TextMeshPro` are powerful, but dynamically generating 3D UI panels that perfectly match A-Frame HTML/CSS sizing/flexbox logic is challenging and requires a lot of layout prefab work.
* **`ArenaVideoControl` / `ArenaJitsiVideo`**: While Unity has a `VideoPlayer`, streaming and syncing WebRTC (like Jitsi) natively inside Unity is historically tricky and usually requires heavy external plugins (like Unity Render Streaming or WebRTC packages).
* **`ArenaWirePcdModel` & `ArenaWireGaussianSplatting`**: Unity has no native support for point clouds or splatting. You would need to pull in external repositories (like Keijiro Takahashi's PLY or Splatting repos) and wire up custom shaders.
* **`ArenaWireUrdfModel`**: Requires integrating a heavy external URDF parser (like the Unity Robotics Hub URDF Importer).

---
### Summary Recommendation
Starting with **Sound, Shadows, Physics (Bodies), and GotoURL/Landmark** will yield the most "bang for your buck" in terms of feature parity with A-Frame, as almost every Unity developer knows exactly how `AudioSource` and `Rigidbody` work out of the box!
