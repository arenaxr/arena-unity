/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.IO;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaWireImage : ArenaComponent
    {
        // ARENA image component unity conversion status:
        // DONE: url
        // DONE: height
        // TODO: segmentsHeight
        // TODO: segmentsWidth
        // DONE: width

        public ArenaImageJson json = new ArenaImageJson();

        protected override void ApplyRender()
        {
            var url = json.Url;
            string assetPath = null;
            if (url != null && ArenaClientScene.Instance != null)
            {
                assetPath = ArenaClientScene.Instance.checkLocalAsset(url);
                if (assetPath == null)
                {
                    ArenaClientScene.Instance.RegisterAssetCallback(url, () => { apply = true; });
                    return;
                }
            }
            AttachImage(assetPath, gameObject, json);
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }

        internal static void AttachImage(string assetPath, GameObject gobj, ArenaImageJson json = null)
        {
            Sprite sprite = null;
            if (assetPath != null)
            {
                sprite = LoadSpriteFromFile(assetPath);
            }

            if (sprite == null)
            {
                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f, 1, SpriteMeshType.FullRect);
            }

            SpriteRenderer spriteRenderer = gobj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gobj.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = sprite;
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;

            if (json != null)
            {
                spriteRenderer.size = new Vector2(json.Width, json.Height);
                // note: SpriteRenderer does not support segmentsWidth or segmentsHeight
            }
            else
            {
                spriteRenderer.size = Vector2.one;
            }
        }

        private static Sprite LoadSpriteFromFile(string assetPath)
        {
            if (assetPath == null) return null;
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Trilinear;
            var imgdata = File.ReadAllBytes(assetPath);
            tex.LoadImage(imgdata);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f, 1, SpriteMeshType.FullRect);
            return sprite;
        }
    }
}
