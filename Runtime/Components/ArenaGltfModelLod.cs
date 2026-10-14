/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity.Components
{
    public class ArenaGltfModelLod : ArenaComponent
    {
        // ARENA gltf-model-lod component unity conversion status:
        // DONE: detailedUrl
        // DONE: detailedDistance
        // DONE: updateRate
        // TODO: retainCache

        public ArenaGltfModelLodJson json = new ArenaGltfModelLodJson();

        private GameObject detailedModelObj;
        private Coroutine lodCoroutine;
        private bool isDetailedLoaded = false;

        protected override void ApplyRender()
        {
            if (string.IsNullOrEmpty(json.DetailedUrl)) return;

            if (ArenaClientScene.Instance != null)
            {
                string assetPath = ArenaClientScene.Instance.checkLocalAsset(json.DetailedUrl);
                if (assetPath != null)
                {
                    if (detailedModelObj == null)
                    {
                        detailedModelObj = new GameObject("DetailedModelLOD");
                        detailedModelObj.transform.SetParent(gameObject.transform, false);
                        // Start hidden
                        detailedModelObj.SetActive(false);
                        ArenaWireGltfModel.AttachGltf(assetPath, detailedModelObj);
                        isDetailedLoaded = true;
                    }

                    if (lodCoroutine != null) StopCoroutine(lodCoroutine);
                    if (gameObject.activeInHierarchy)
                        lodCoroutine = StartCoroutine(CheckDistanceRoutine());
                }
                else
                {
                    ArenaClientScene.Instance.RegisterAssetCallback(json.DetailedUrl, () => { apply = true; });
                }
            }
        }

        private System.Collections.IEnumerator CheckDistanceRoutine()
        {
            while (true)
            {
                if (Camera.main != null && isDetailedLoaded)
                {
                    float dist = Vector3.Distance(Camera.main.transform.position, transform.position);
                    bool useDetailed = dist <= json.DetailedDistance;
                    
                    if (detailedModelObj != null && detailedModelObj.activeSelf != useDetailed)
                    {
                        detailedModelObj.SetActive(useDetailed);
                        
                        // Toggle base model(s) by disabling any child that isn't the detailed model
                        // and isn't another ARENA object.
                        foreach (Transform child in transform)
                        {
                            if (child.gameObject != detailedModelObj && child.GetComponent<ArenaObject>() == null)
                            {
                                child.gameObject.SetActive(!useDetailed);
                            }
                        }
                    }
                }
                float waitTime = json.UpdateRate / 1000f;
                if (waitTime <= 0) waitTime = 0.333f;
                yield return new WaitForSeconds(waitTime);
            }
        }

        private void OnDisable()
        {
            if (lodCoroutine != null)
            {
                StopCoroutine(lodCoroutine);
                lodCoroutine = null;
            }
        }

        private void OnEnable()
        {
            if (isDetailedLoaded)
            {
                if (lodCoroutine != null) StopCoroutine(lodCoroutine);
                lodCoroutine = StartCoroutine(CheckDistanceRoutine());
            }
        }

        private void OnDestroy()
        {
            if (lodCoroutine != null) StopCoroutine(lodCoroutine);
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
