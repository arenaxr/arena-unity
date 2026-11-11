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
    public class ArenaPhysxJointDriver : ArenaComponent
    {
        // ARENA physx-joint-driver component unity conversion status:
        // TODO: angularVelocity
        // DONE: axes
        // DONE: damping
        // DONE: forceLimit
        // DONE: linearVelocity
        // DONE: lockOtherAxes
        // DONE: slerpRotation
        // DONE: stiffness
        // DONE: useAcceleration

        public ArenaPhysxJointDriverJson json = new ArenaPhysxJointDriverJson();

        protected override void ApplyRender()
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            // drivers only decorate the D6 joint made by physx-joint, they never make one,
            // so wait out of order arrivals, ArenaPhysxJoint re-applies us once its joint exists
            ConfigurableJoint d6 = gameObject.GetComponent<ConfigurableJoint>();
            if (d6 == null) return;

            bool driveX = false, driveY = false, driveZ = false, driveTwist = false, driveSwing = false;
            if (json.Axes != null)
            {
                foreach (string axis in json.Axes)
                {
                    switch (axis)
                    {
                        case "x": driveX = true; break;
                        case "y": driveY = true; break;
                        case "z": driveZ = true; break;
                        case "twist": driveTwist = true; break;
                        case "swing": driveSwing = true; break;
                    }
                }
            }

            JointDrive drive = new JointDrive
            {
                positionSpring = json.Stiffness,
                positionDamper = json.Damping,
                maximumForce = json.ForceLimit,
                useAcceleration = json.UseAcceleration
            };

            if (driveX) d6.xDrive = drive;
            if (driveY) d6.yDrive = drive;
            if (driveZ) d6.zDrive = drive;

            d6.rotationDriveMode = json.SlerpRotation ? RotationDriveMode.Slerp : RotationDriveMode.XYAndZ;
            if (json.SlerpRotation)
            {
                if (driveTwist || driveSwing) d6.slerpDrive = drive;
            }
            else
            {
                if (driveTwist) d6.angularXDrive = drive;
                if (driveSwing) d6.angularYZDrive = drive;
            }

            if (json.LinearVelocity != null)
            {
                d6.targetVelocity = ArenaUnity.ToUnityPosition(json.LinearVelocity);
            }

            if (json.LockOtherAxes)
            {   // lock every axis this driver does not drive
                if (!driveX) d6.xMotion = ConfigurableJointMotion.Locked;
                if (!driveY) d6.yMotion = ConfigurableJointMotion.Locked;
                if (!driveZ) d6.zMotion = ConfigurableJointMotion.Locked;
                if (!driveTwist) d6.angularXMotion = ConfigurableJointMotion.Locked;
                if (!driveSwing)
                {
                    d6.angularYMotion = ConfigurableJointMotion.Locked;
                    d6.angularZMotion = ConfigurableJointMotion.Locked;
                }
            }

            // TODO: angularVelocity, unity targetAngularVelocity is radians/sec in the joint frame,
            // the A-Frame to Unity handedness flip for an angular rate is unverified, left unmapped
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
