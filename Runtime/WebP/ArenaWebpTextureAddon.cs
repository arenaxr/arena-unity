/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 *
 * Adapted from glTFast DocExamples/WebpTextureAddon.cs (Apache-2.0)
 * https://github.com/Unity-Technologies/com.unity.cloud.gltfast
 *
 * Provides EXT_texture_webp support for ARENA glTF model loading.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Addons;
using GLTFast.Schema;
using Unity.Collections;
using UnityEngine;

namespace ArenaUnity
{
    /// <summary>
    /// glTFast ImportAddon that registers WebP texture decoding for glTF models
    /// using the EXT_texture_webp extension.
    /// </summary>
    class ArenaWebpTextureAddon : ImportAddon<ArenaWebpTextureAddonInstance> { }

    /// <summary>
    /// ImportAddon instance that handles WebP texture loading via libwebp P/Invoke.
    /// Implements ITextureImageLoader to intercept glTFast's texture pipeline.
    /// </summary>
    class ArenaWebpTextureAddonInstance : ImportAddonInstance, ITextureImageLoader
    {
        /// <inheritdoc />
        public override void Inject(GltfImportBase gltfImport)
        {
#if NEWTONSOFT_JSON
            if (gltfImport is not GLTFast.Newtonsoft.GltfImport)
                return;

            gltfImport.AddImportAddonInstance(this);
#else
            Debug.LogError("[ArenaWebP] WebP texture addon requires the Newtonsoft.Json package.");
#endif
        }

        /// <inheritdoc />
        public override bool SupportsGltfExtension(string extensionName)
        {
            return extensionName == "EXT_texture_webp";
        }

        /// <summary>
        /// Checks if this loader can handle the given texture via EXT_texture_webp extension.
        /// </summary>
        public bool IsAbleToLoad(TextureBase texture, out int imageIndex)
        {
#if NEWTONSOFT_JSON
            if (texture is GLTFast.Newtonsoft.Schema.Texture { extensions: not null } t
                && t.extensions.TryGetValue<TextureWebpExtension>(
                    "EXT_texture_webp", out var ext))
            {
                imageIndex = ext.source;
                return true;
            }
#endif
            imageIndex = -1;
            return false;
        }

        /// <summary>
        /// Content-based detection: checks for RIFF/WEBP magic bytes.
        /// </summary>
        public bool IsAbleToLoad(ReadOnlySpan<byte> data)
        {
            return ImageFormatDetection.IsWebP(data);
        }

        /// <summary>
        /// Decodes WebP image data into a Texture2D using the native libwebp plugin.
        /// </summary>
        public async Task<ImageResult> LoadImage(
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken
            )
        {
            var texture = await ArenaWebpDecoder.Decode(data, linear, readable, cancellationToken);
            return new ImageResult(texture, true);
        }

        /// <inheritdoc />
        public override void Dispose() { }

        /// <inheritdoc />
        public override void Inject(IInstantiator instantiator) { }
    }

    /// <summary>
    /// Deserialization target for the EXT_texture_webp glTF extension.
    /// </summary>
    [Serializable]
    struct TextureWebpExtension
    {
        public int source;
    }
}
