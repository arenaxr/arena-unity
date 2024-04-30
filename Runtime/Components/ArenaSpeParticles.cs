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
    public class ArenaSpeParticles : ArenaComponent
    {
        // ARENA spe-particles component unity conversion status:
        // TODO: acceleration
        // TODO: accelerationDistribution
        // TODO: accelerationSpread
        // TODO: activeMultiplier
        // TODO: affectedByFog
        // TODO: alphaTest
        // TODO: angle
        // TODO: angleSpread
        // TODO: blending
        // TODO: color
        // TODO: colorSpread
        // TODO: depthTest
        // TODO: depthWrite
        // TODO: direction
        // TODO: distribution
        // TODO: drag
        // TODO: dragSpread
        // TODO: duration
        // TODO: emitterScale
        // TODO: enableInEditor
        // TODO: enabled
        // TODO: frustumCulled
        // TODO: hasPerspective
        // TODO: maxAge
        // TODO: maxAgeSpread
        // TODO: opacity
        // TODO: opacitySpread
        // TODO: particleCount
        // TODO: positionDistribution
        // TODO: positionOffset
        // TODO: positionSpread
        // TODO: radius
        // TODO: radiusScale
        // TODO: randomizeAcceleration
        // TODO: randomizeAngle
        // TODO: randomizeColor
        // TODO: randomizeDrag
        // TODO: randomizeOpacity
        // TODO: randomizePosition
        // TODO: randomizeRotation
        // TODO: randomizeSize
        // TODO: randomizeVelocity
        // TODO: relative
        // TODO: rotation
        // TODO: rotationAxis
        // TODO: rotationAxisSpread
        // TODO: rotationSpread
        // TODO: rotationStatic
        // TODO: size
        // TODO: sizeSpread
        // TODO: texture
        // TODO: textureFrameCount
        // TODO: textureFrameLoop
        // TODO: textureFrames
        // TODO: useTransparency
        // TODO: velocity
        // TODO: velocityDistribution
        // TODO: velocitySpread
        // TODO: wiggle
        // TODO: wiggleSpread

        public ArenaSpeParticlesJson json = new ArenaSpeParticlesJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.
        }

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{{\"{json.componentName}\":{newJson}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
