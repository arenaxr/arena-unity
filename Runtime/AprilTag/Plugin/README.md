# ArenaAprilTag Native Plugin

The `ArenaAprilTag` native plugin is a compiled version of the
[AprilTag](https://github.com/AprilRobotics/apriltag) library (BSD-2-Clause)
built with the **tag36h11** family enabled — the family used by
[ARENA](https://github.com/arenaxr/arena-web-core) for physical scene-origin localization.

## Included pre-built binaries

| Platform | Architecture | File |
|---|---|---|
| Linux (Editor + Standalone) | x86-64 | `Plugin/Linux/x86_64/libArenaAprilTag.so` |

## Building for other platforms

Clone the upstream AprilTag library and compile a shared library that exports
`tag36h11_create` / `tag36h11_destroy`.

```bash
git clone https://github.com/AprilRobotics/apriltag.git
cd apriltag && mkdir build && cd build
cmake -DBUILD_SHARED_LIBS=ON -DBUILD_EXAMPLES=OFF -DBUILD_PYTHON_WRAPPER=OFF \
      -DCMAKE_BUILD_TYPE=Release ..
cmake --build .
```

Then place the resulting library in the correct sub-directory and add a Unity `.meta`
file (use the Linux `.meta` as a template, adjusting platform settings):

| Platform | Destination | File name |
|---|---|---|
| Windows x86-64 | `Plugin/Windows/x86_64/` | `ArenaAprilTag.dll` |
| macOS (universal) | `Plugin/macOS/` | `ArenaAprilTag.bundle` |
| Android arm64 | `Plugin/Android/` | `libArenaAprilTag.so` |
| iOS arm64 | `Plugin/iOS/` | `libArenaAprilTag.a` (static) |

> **iOS note:** For iOS the library must be built as a static archive and
> `Config.cs` already handles this via the `__Internal` entry point.
