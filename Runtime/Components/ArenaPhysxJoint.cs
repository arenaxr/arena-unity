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
    public class ArenaPhysxJoint : ArenaComponent
    {
        // ARENA physx-joint component unity conversion status:
        // DONE: breakForce
        // DONE: collideWithTarget
        // TODO: projectionTolerance
        // TODO: removeElOnBreak
        // TODO: softFixed
        // DONE: target
        // DONE: type

        public ArenaPhysxJointJson json = new ArenaPhysxJointJson();

        protected override void ApplyRender()
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            Joint joint = null;

            switch (json.Type)
            {
                case ArenaPhysxJointJson.TypeType.Fixed:
                    joint = gameObject.GetComponent<FixedJoint>();
                    if (joint == null) joint = gameObject.AddComponent<FixedJoint>();
                    break;
                case ArenaPhysxJointJson.TypeType.Revolute:
                    joint = gameObject.GetComponent<HingeJoint>();
                    if (joint == null) joint = gameObject.AddComponent<HingeJoint>();
                    break;
                case ArenaPhysxJointJson.TypeType.Spherical:
                    joint = gameObject.GetComponent<CharacterJoint>();
                    if (joint == null) joint = gameObject.AddComponent<CharacterJoint>();
                    break;
                case ArenaPhysxJointJson.TypeType.Prismatic:
                case ArenaPhysxJointJson.TypeType.D6:
                    joint = gameObject.GetComponent<ConfigurableJoint>();
                    if (joint == null) joint = gameObject.AddComponent<ConfigurableJoint>();
                    break;
                default:
                    joint = gameObject.GetComponent<ConfigurableJoint>();
                    if (joint == null) joint = gameObject.AddComponent<ConfigurableJoint>();
                    break;
            }

            if (json.BreakForce != null)
            {
                joint.breakForce = json.BreakForce.X >= 0 ? json.BreakForce.X : Mathf.Infinity;
                joint.breakTorque = json.BreakForce.Y >= 0 ? json.BreakForce.Y : Mathf.Infinity;
            }

            joint.enableCollision = json.CollideWithTarget;

            if (!string.IsNullOrEmpty(json.Target))
            {
                if (ArenaClientScene.Instance != null && ArenaClientScene.Instance.arenaObjs.TryGetValue(json.Target, out GameObject targetObj))
                {
                    Rigidbody targetRb = targetObj.GetComponent<Rigidbody>();
                    if (targetRb != null)
                    {
                        joint.connectedBody = targetRb;
                    }
                }
            }

            SetSuppressTransform(true);
        }

        private void SetSuppressTransform(bool suppress)
        {
            var aobj = GetComponent<ArenaObject>();
            if (aobj != null)
                aobj.suppressTransformPublish = suppress;
        }

        private void OnDisable()
        {
            SetSuppressTransform(false);
        }

        private void OnDestroy()
        {
            SetSuppressTransform(false);
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
