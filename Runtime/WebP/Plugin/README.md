# WebP Unity Native Plugin

The `webp-unity` native plugin is a compiled version of
[libwebp](https://chromium.googlesource.com/webm/libwebp) (BSD-3-Clause)
that enables WebP texture decoding for glTF models loaded via glTFast.

The plugin name `webp-unity` matches the convention used in glTFast's
official `DocExamples/WebP.cs` reference implementation.

## Pre-built binaries (CI)

Binaries are built automatically by the
[`build-webp-plugin.yaml`](/.github/workflows/build-webp-plugin.yaml)
GitHub Actions workflow from the upstream libwebp source.

| Platform | Architecture | File | Unity .meta |
|---|---|---|---|
| **Android (Quest)** | ARM64 (v8a) | `Plugin/Android/arm64-v8a/libwebp-unity.so` | ✅ |
| Linux (Editor + Standalone) | x86-64 | `Plugin/Linux/x86_64/libwebp-unity.so` | ✅ |
| macOS (Editor + Standalone) | Universal (arm64 + x86_64) | `Plugin/macOS/webp-unity.bundle` | ✅ |
| Windows (Editor + Standalone) | x86-64 | `Plugin/Windows/x86_64/webp-unity.dll` | ✅ |

### Rebuilding binaries

Trigger the workflow manually via
**Actions → Build WebP Native Plugins → Run workflow**.

## Building manually

```bash
git clone https://chromium.googlesource.com/webm/libwebp
cd libwebp && mkdir build && cd build
cmake -DBUILD_SHARED_LIBS=ON -DWEBP_BUILD_EXTRAS=OFF \
      -DWEBP_BUILD_ANIM_UTILS=OFF -DWEBP_BUILD_CWEBP=OFF \
      -DWEBP_BUILD_DWEBP=OFF -DWEBP_BUILD_GIF2WEBP=OFF \
      -DWEBP_BUILD_IMG2WEBP=OFF -DWEBP_BUILD_VWEBP=OFF \
      -DWEBP_BUILD_WEBPINFO=OFF -DWEBP_BUILD_WEBPMUX=OFF \
      -DCMAKE_BUILD_TYPE=Release ..
cmake --build .
```

Rename the output to `webp-unity` (e.g. `libwebp-unity.so` on Linux)
and place it in the correct platform sub-directory.
