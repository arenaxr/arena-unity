/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

// Compile-time Unity version check.
// This file lives in its own assembly (conix.arena.unity.VersionCheck)
// with zero dependencies, so it compiles even when KTX/glTFast fail
// on older Unity versions. It provides a clear, actionable error.

#if !UNITY_6000_0_OR_NEWER
#error ARENA for Unity 1.8+ requires Unity 6 (6000.0) or newer. Your Unity version is too old. Please upgrade your Unity Editor, or use arena-unity 1.7.x for Unity 2022.3 support. See https://github.com/arenaxr/arena-unity
#endif
