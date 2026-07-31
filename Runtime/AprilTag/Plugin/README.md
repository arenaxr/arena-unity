# ArenaAprilTag Native Plugin

The `ArenaAprilTag` native plugin is a compiled version of the
[AprilTag](https://github.com/AprilRobotics/apriltag) library (BSD-2-Clause)
built with the **tag36h11** family enabled — the family used by
[ARENA](https://github.com/arenaxr/arena-web-core) for physical scene-origin localization.

## Pre-built binaries (CI)

Binaries are built automatically by the
[`build-apriltag-plugin.yaml`](/.github/workflows/build-apriltag-plugin.yaml)
GitHub Actions workflow from the upstream AprilRobotics/apriltag source (v3.4.3).

| Platform | Architecture | File | Unity .meta |
|---|---|---|---|
| **Android (Quest)** | ARM64 (v8a) | `Plugin/Android/arm64-v8a/libArenaAprilTag.so` | ✅ |
| Linux (Editor + Standalone) | x86-64 | `Plugin/Linux/x86_64/libArenaAprilTag.so` | ✅ |
| macOS (Editor + Standalone) | Universal (arm64 + x86_64) | `Plugin/macOS/ArenaAprilTag.bundle` | ✅ |
| Windows (Editor + Standalone) | x86-64 | `Plugin/Windows/x86_64/ArenaAprilTag.dll` | ✅ |

### Rebuilding binaries

To rebuild all platform binaries, trigger the workflow manually via
**Actions → Build AprilTag Native Plugins → Run workflow**.

The workflow will:
1. Clone the upstream `AprilRobotics/apriltag` source at the pinned tag
2. Cross-compile for each platform using platform-specific toolchains (Android NDK, cmake, etc.)
3. Commit the resulting binaries back to the branch

## Building manually

Clone the upstream AprilTag library and compile a shared library that exports
`tag36h11_create` / `tag36h11_destroy`.

```bash
git clone https://github.com/AprilRobotics/apriltag.git
cd apriltag && mkdir build && cd build
cmake -DBUILD_SHARED_LIBS=ON -DBUILD_EXAMPLES=OFF -DBUILD_PYTHON_WRAPPER=OFF \
      -DCMAKE_BUILD_TYPE=Release ..
cmake --build .
```

For Android (Quest) cross-compilation:

```bash
cmake \
  -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK_ROOT/build/cmake/android.toolchain.cmake \
  -DANDROID_ABI=arm64-v8a \
  -DANDROID_PLATFORM=android-26 \
  -DBUILD_SHARED_LIBS=ON -DBUILD_EXAMPLES=OFF -DBUILD_PYTHON_WRAPPER=OFF \
  -DCMAKE_BUILD_TYPE=Release ..
cmake --build .
```

Place the resulting library in the correct sub-directory. Unity `.meta` files
are already provided for all platforms listed above.

> **iOS note:** For iOS the library must be built as a static archive and
> `Config.cs` already handles this via the `__Internal` entry point. iOS is not
> yet included in the CI workflow.
