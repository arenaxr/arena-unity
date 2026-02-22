/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArenaUnity
{
    public class ArenaWireLight : ArenaComponent
    {
        // ARENA light component unity conversion status:
        // DONE: angle
        // DONE: castShadow
        // DONE: color
        // TODO: decay
        // DONE: distance
        // TODO: envMap
        // DONE: groundColor
        // DONE: intensity
        // TODO: light
        // DONE: penumbra
        // DONE: shadowBias
        // TODO: shadowCameraBottom
        // TODO: shadowCameraFar
        // TODO: shadowCameraFov
        // TODO: shadowCameraLeft
        // DONE: shadowCameraNear
        // TODO: shadowCameraRight
        // TODO: shadowCameraTop
        // TODO: shadowCameraVisible
        // TODO: shadowMapHeight
        // TODO: shadowMapWidth
        // TODO: shadowRadius
        // DONE: target
        // DONE: type

        public ArenaLightJson json = new ArenaLightJson();

        protected override void ApplyRender()
        {
            if (json.Type == ArenaLightJson.TypeType.Ambient || json.Type == ArenaLightJson.TypeType.Hemisphere)
            {
                if (json.Type == ArenaLightJson.TypeType.Hemisphere)
                {
                    RenderSettings.ambientMode = AmbientMode.Trilight;
                    if (json.Color != null)
                        RenderSettings.ambientSkyColor = ArenaUnity.ToUnityColor(json.Color);
                    if (json.GroundColor != null)
                        RenderSettings.ambientGroundColor = ArenaUnity.ToUnityColor(json.GroundColor);
                }
                else
                {
                    RenderSettings.ambientMode = AmbientMode.Flat;
                    if (json.Color != null)
                        RenderSettings.ambientLight = ArenaUnity.ToUnityColor(json.Color);
                }
                RenderSettings.ambientIntensity = json.Intensity;
            }
            else
            {
                Light light = gameObject.GetComponent<Light>();
                if (light == null)
                    light = gameObject.AddComponent<Light>();
                switch (json.Type)
                {
                    case ArenaLightJson.TypeType.Directional:
                        light.type = LightType.Directional;
                        gameObject.transform.LookAt(new Vector3(0,0,0));
                        break;
                    case ArenaLightJson.TypeType.Point:
                        light.type = LightType.Point;
                        light.range = json.Distance;
                        // Unity's light.bounceIntensity could arguably be tied to decay but let's just observe standard decay mappings
                        // Light component does not have a direct exact analog for threejs 'decay' without custom curves
                        break;
                    case ArenaLightJson.TypeType.Spot:
                        light.type = LightType.Spot;
                        light.range = json.Distance;
                        light.spotAngle = json.Angle;
                        light.innerSpotAngle = json.Angle * (1f - json.Penumbra);
                        break;
                }
                light.intensity = json.Intensity;
                if (json.Color != null)
                    light.color = ArenaUnity.ToUnityColor(json.Color);

                light.shadows = !json.CastShadow ? LightShadows.None : LightShadows.Soft;
                if (json.CastShadow)
                {
                    light.shadowBias = json.ShadowBias;
                    // Unity handles shadow map resolution and frustum slightly differently, often globally or per pipeline.
                    // But we can map shadow map width and height hints to custom resolution where applicable, though standard Light doesn't expose it directly except via LightShadows enum.
                    light.shadowNearPlane = json.ShadowCameraNear;
                    // Note: shadowMapWidth, shadowMapHeight, shadowCameraFar, shadowCameraFov etc. might not have direct simple mappings in the base Light component.
                }

                if (!string.IsNullOrEmpty(json.Target))
                {
                    // If target exists, point the light towards the target GameObject
                    GameObject targetObj = GameObject.Find(json.Target);
                    if (targetObj != null)
                    {
                        light.transform.LookAt(targetObj.transform);
                    }
                }

                if (!string.IsNullOrEmpty(json.EnvMap))
                {
                    // Loading an environment map usually involves downloading a cubemap and applying it to RenderSettings.customReflection
                    // That process requires async downloading via ArenaClientScene or similar, so we'll just log it for now as a TODO.
                    Debug.LogWarning("ArenaWireLight: envMap setting is noted but dynamically loading cubemaps at runtime is not fully implemented yet.");
                }
            }
        }

        // light
        public static JObject ToArenaLight(GameObject gobj)
        {
            // TODO: translate from RenderSettings.ambientMode, may need centralized one-time publish
            var data = new ArenaLightJson();
            Light light = gobj.GetComponent<Light>();
            if (light != null)
            {
                switch (light.type)
                {
                    case LightType.Directional:
                        data.Type = ArenaLightJson.TypeType.Directional;
                        break;
                    case LightType.Point:
                        data.Type = ArenaLightJson.TypeType.Point;
                        data.Distance = ArenaUnity.ArenaFloat(light.range);
                        break;
                    case LightType.Spot:
                        data.Type = ArenaLightJson.TypeType.Spot;
                        data.Distance = ArenaUnity.ArenaFloat(light.range);
                        data.Angle = ArenaUnity.ArenaFloat(light.spotAngle);
                        data.Penumbra = ArenaUnity.ArenaFloat(1f - (light.innerSpotAngle / light.spotAngle));
                        break;
                }
                data.Intensity = ArenaUnity.ArenaFloat(light.intensity);
                data.Color = ArenaUnity.ToArenaColor(light.color);
                data.CastShadow = light.shadows != LightShadows.None;
                data.ShadowBias = ArenaUnity.ArenaFloat(light.shadowBias);
                data.ShadowCameraNear = ArenaUnity.ArenaFloat(light.shadowNearPlane);
            }

            return data != null ? JObject.FromObject(data) : null;
        }

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{newJson}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
