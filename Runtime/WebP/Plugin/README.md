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

**Important:** the plugin must be a single *self-contained* shared library.
Since libwebp v1.3, a shared (`BUILD_SHARED_LIBS=ON`) build produces a
`libwebp` that dynamically depends on a separate `libsharpyuv` library.
Renaming that output and shipping it alone causes `DllNotFoundException`
at runtime (dlopen fails to resolve `libsharpyuv`). Instead, build the
static libraries and link them into one shared library:

```bash
git clone --branch v1.5.0 https://chromium.googlesource.com/webm/libwebp
cd libwebp
cmake -B build -DBUILD_SHARED_LIBS=OFF \
      -DCMAKE_POSITION_INDEPENDENT_CODE=ON \
      -DWEBP_BUILD_EXTRAS=OFF \
      -DWEBP_BUILD_ANIM_UTILS=OFF -DWEBP_BUILD_CWEBP=OFF \
      -DWEBP_BUILD_DWEBP=OFF -DWEBP_BUILD_GIF2WEBP=OFF \
      -DWEBP_BUILD_IMG2WEBP=OFF -DWEBP_BUILD_VWEBP=OFF \
      -DWEBP_BUILD_WEBPINFO=OFF -DWEBP_BUILD_WEBPMUX=OFF \
      -DCMAKE_BUILD_TYPE=Release
cmake --build build --parallel

# Linux / Android (use the NDK clang for Android):
cc -shared -o libwebp-unity.so -Wl,-soname,libwebp-unity.so \
   -Wl,--whole-archive build/libwebp.a -Wl,--no-whole-archive build/libsharpyuv.a -lm

# macOS (build arm64 and x86_64 separately — a fat static build breaks
# libwebp's SIMD flag detection — then lipo the two dylibs together):
cc -dynamiclib -arch arm64 -o webp-unity-arm64.dylib \
   -Wl,-force_load,build-arm64/libwebp.a build-arm64/libsharpyuv.a \
   -install_name @rpath/webp-unity.bundle
lipo -create webp-unity-arm64.dylib webp-unity-x86_64.dylib -output webp-unity.bundle

# Windows (from an MSVC developer prompt):
link /DLL /OUT:webp-unity.dll Release\libwebp.lib Release\libsharpyuv.lib ^
   /EXPORT:WebPGetInfo /EXPORT:WebPDecodeRGBAInto
```

Verify before shipping — the binary must have **no** dynamic dependency on
`libwebp` or `libsharpyuv` (`otool -L` on macOS, `readelf -d` on Linux/Android,
`dumpbin /dependents` on Windows) and must export `WebPGetInfo` and
`WebPDecodeRGBAInto`. Place the result in the correct platform sub-directory.
