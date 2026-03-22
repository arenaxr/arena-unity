/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Burst;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArenaUnity
{
    [BurstCompile]
    public class ArenaWireObjModel : ArenaComponent
    {
        // ARENA obj-model component unity conversion status:
        // DONE: obj
        // TODO: mtl

        public ArenaObjModelJson json = new ArenaObjModelJson();

        protected override void ApplyRender()
        {
#if UNITY_EDITOR
            if (ArenaClientScene.Instance == null) return;
            var objpath = ArenaClientScene.Instance.checkLocalAsset(json.Obj);
            if (objpath == null)
            {
                ArenaClientScene.Instance.RegisterAssetCallback(json.Obj, () => { apply = true; });
                return;
            }
            
            if (!string.IsNullOrEmpty(json.Mtl))
            {
                var mtlpath = ArenaClientScene.Instance.checkLocalAsset(json.Mtl);
                if (mtlpath == null)
                {
                    ArenaClientScene.Instance.RegisterAssetCallback(json.Mtl, () => { apply = true; });
                    return;
                }
            }

            if (objpath != null)
            {
                GameObject objpre = (GameObject)AssetDatabase.LoadAssetAtPath(objpath, typeof(GameObject));
                if (objpre != null)
                {
                    GameObject sobj = Instantiate(objpre);
                    sobj.transform.SetParent(transform, false);
                    sobj.transform.localRotation = ArenaUnity.GltfToUnityRotationQuat(sobj.transform.localRotation);
                    // mtl is already downloaded if we reach here
                }
                else
                {
                    Debug.LogError($"Unable to load '{objpath}'");
                }
            }

#else
            Debug.LogWarning($"ObjModel object '{json.Obj}' is Editor only, not yet implemented in Runtime mode.");
#endif
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
