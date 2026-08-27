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
    public class ArenaPhysxJointConstraint : ArenaComponent
    {
        // ARENA physx-joint-constraint component unity conversion status:
        // DONE: angularLimit
        // DONE: constrainedAxes
        // DONE: damping
        // DONE: freeAxes
        // DONE: limitCone
        // DONE: linearLimit
        // DONE: lockedAxes
        // DONE: restitution
        // DONE: stiffness
        // DONE: twistLimit

        public ArenaPhysxJointConstraintJson json = new ArenaPhysxJointConstraintJson();

        protected override void ApplyRender()
        {
            if (!ArenaSceneOptions.PhysicsEnabled) return;

            // constraints only decorate the joint made by physx-joint, they never make one,
            // so wait out of order arrivals, ArenaPhysxJoint re-applies us once its joint exists
            HingeJoint revolute = gameObject.GetComponent<HingeJoint>();
            ConfigurableJoint d6 = gameObject.GetComponent<ConfigurableJoint>();
            if (revolute == null && d6 == null) return;

            if (revolute != null && json.AngularLimit != null)
            {   // [Revolute] hinge low/high angle in degrees
                JointLimits limits = revolute.limits;
                limits.min = json.AngularLimit.X;
                limits.max = json.AngularLimit.Y;
                limits.bounciness = json.Restitution;
                revolute.limits = limits;
                revolute.useLimits = true;
            }

            if (d6 == null) return;

            // [D6] axis lists decide which motions are locked, limited, or free
            ApplyAxesMotion(d6, json.LockedAxes, ConfigurableJointMotion.Locked);
            ApplyAxesMotion(d6, json.ConstrainedAxes, ConfigurableJointMotion.Limited);
            ApplyAxesMotion(d6, json.FreeAxes, ConfigurableJointMotion.Free);

            if (json.LinearLimit != null)
            {   // [D6, Prismatic] unity linear limits are one symmetric distance, use the larger magnitude
                SoftJointLimit linearLimit = d6.linearLimit;
                linearLimit.limit = Mathf.Max(Mathf.Abs(json.LinearLimit.X), Mathf.Abs(json.LinearLimit.Y));
                linearLimit.bounciness = json.Restitution;
                d6.linearLimit = linearLimit;
            }

            if (json.TwistLimit != null)
            {   // [D6] twist is rotation about the joint's primary (x) axis
                SoftJointLimit lowTwist = d6.lowAngularXLimit;
                lowTwist.limit = json.TwistLimit.X;
                lowTwist.bounciness = json.Restitution;
                d6.lowAngularXLimit = lowTwist;
                SoftJointLimit highTwist = d6.highAngularXLimit;
                highTwist.limit = json.TwistLimit.Y;
                highTwist.bounciness = json.Restitution;
                d6.highAngularXLimit = highTwist;
            }

            if (json.LimitCone != null)
            {   // [D6] swing cone is the pair of y, z angular limits
                SoftJointLimit swingY = d6.angularYLimit;
                swingY.limit = json.LimitCone.X;
                swingY.bounciness = json.Restitution;
                d6.angularYLimit = swingY;
                SoftJointLimit swingZ = d6.angularZLimit;
                swingZ.limit = json.LimitCone.Y;
                swingZ.bounciness = json.Restitution;
                d6.angularZLimit = swingZ;
            }

            if (json.Stiffness > 0f)
            {   // [All] stiffness greater than 0 makes this a soft constraint, spring damped
                SoftJointLimitSpring linearSpring = d6.linearLimitSpring;
                linearSpring.spring = json.Stiffness;
                linearSpring.damper = json.Damping;
                d6.linearLimitSpring = linearSpring;
                SoftJointLimitSpring twistSpring = d6.angularXLimitSpring;
                twistSpring.spring = json.Stiffness;
                twistSpring.damper = json.Damping;
                d6.angularXLimitSpring = twistSpring;
                SoftJointLimitSpring swingSpring = d6.angularYZLimitSpring;
                swingSpring.spring = json.Stiffness;
                swingSpring.damper = json.Damping;
                d6.angularYZLimitSpring = swingSpring;
            }
        }

        private void ApplyAxesMotion(ConfigurableJoint d6, string[] axes, ConfigurableJointMotion motion)
        {
            if (axes == null) return;

            foreach (string axis in axes)
            {
                switch (axis)
                {
                    case "x": d6.xMotion = motion; break;
                    case "y": d6.yMotion = motion; break;
                    case "z": d6.zMotion = motion; break;
                    case "twist": d6.angularXMotion = motion; break;
                    case "swing":
                        d6.angularYMotion = motion;
                        d6.angularZMotion = motion;
                        break;
                }
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
