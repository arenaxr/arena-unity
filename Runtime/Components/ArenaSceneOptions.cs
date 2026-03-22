/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ArenaUnity.Components
{
    public class ArenaSceneOptions : ArenaComponent
    {
        // ARENA scene-options component unity conversion status:
        // TODO: clickableOnlyEvents
        // TODO: distanceModel
        // TODO: sceneHeadModels
        // TODO: jitsiHost
        // TODO: maxAVDist
        // TODO: navMesh
        // TODO: networkedLocationSolver
        // TODO: privateScene
        // TODO: refDistance
        // TODO: rolloffFactor
        // TODO: screenshare
        // TODO: videoFrustumCulling
        // TODO: videoDistanceConstraints
        // TODO: videoDefaultResolutionConstraint
        // TODO: volume
        // DONE: physics
        // TODO: ar-hit-test
        // TODO: openvps
        // TODO: originMarker

        public ArenaSceneOptionsJson json = new ArenaSceneOptionsJson();
        private GameObject groundPlaneObj;

        // TODO: This should should be a property of the ArenaPhysicsJson sub component
        public static bool PhysicsEnabled
        {
            get
            {
                if (ArenaClientScene.Instance != null && ArenaClientScene.Instance.sceneOptions?.Physics != null)
                {
                    try
                    {
                        JObject physicsObj = JObject.FromObject(ArenaClientScene.Instance.sceneOptions.Physics);
                        if (physicsObj.TryGetValue("enabled", out JToken enabledToken))
                        {
                            return enabledToken.Value<bool>();
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore parse errors here, defaults to false
                    }
                }
                return false;
            }
        }

        protected override void ApplyRender()
        {
            if (ArenaClientScene.Instance != null)
                ArenaClientScene.Instance.sceneOptions = json;

            // Handle default physx ground plane integration
            if (PhysicsEnabled)
            {
                // A-Frame physx adds a default ground plane unless disabled.
                // We recreate this default ground plane here so objects have somewhere to fall.
                if (groundPlaneObj == null)
                {
                    groundPlaneObj = new GameObject("ArenaPhysxDefaultGroundPlane");
                    groundPlaneObj.transform.SetParent(transform, false);
                    groundPlaneObj.transform.localPosition = new Vector3(0, -0.05f, 0); // slightly below 0 so 0-positioned cubes sit flush

                    BoxCollider bc = groundPlaneObj.AddComponent<BoxCollider>();
                    bc.size = new Vector3(100f, 0.1f, 100f);

                    // Note: it is a static collider by default since it has no Rigidbody.
                }
            }
            else
            {
                // If physics is disabled or removed, cleanup default ground plane.
                if (groundPlaneObj != null)
                {
                    Destroy(groundPlaneObj);
                    groundPlaneObj = null;
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
