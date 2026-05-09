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
    public class ArenaSceneEnvPresets : ArenaComponent
    {
        // ARENA env-presets component unity conversion status:
        // DONE: active
        // TODO: dressing
        // TODO: dressingAmount
        // TODO: dressingColor
        // TODO: dressingOnPlayArea
        // TODO: dressingScale
        // TODO: dressingUniformScale
        // TODO: dressingVariance
        // TODO: flatShading
        // DONE: fog
        // TODO: grid
        // TODO: gridColor
        // TODO: ground
        // DONE: groundColor
        // TODO: groundColor2
        // DONE: groundScale
        // TODO: groundTexture
        // TODO: groundYScale
        // TODO: hideInAR
        // DONE: horizonColor
        // DONE: lighting
        // DONE: lightPosition
        // TODO: playArea
        // TODO: preset
        // TODO: seed
        // DONE: shadow
        // DONE: shadowSize
        // DONE: skyColor
        // DONE: skyType
        public ArenaEnvPresetsJson json = new ArenaEnvPresetsJson();

        private GameObject envRoot;
        private GameObject groundPlane;

        protected override void ApplyRender()
        {
            if (envRoot == null)
            {
                if (gameObject.name == "env")
                {
                    envRoot = gameObject;
                }
                else
                {
                    Transform existingEnv = transform.Find("env");
                    if (existingEnv != null)
                    {
                        envRoot = existingEnv.gameObject;
                    }
                    else
                    {
                        GameObject rootEnv = GameObject.Find("/env");
                        if (rootEnv != null && rootEnv != this.gameObject)
                        {
                            envRoot = rootEnv;
                            envRoot.transform.SetParent(this.transform, false);
                            
                            // Clean up the initial default preset component to avoid duplicate render loops
                            var rootPresets = envRoot.GetComponent<ArenaSceneEnvPresets>();
                            if (rootPresets != null)
                                Destroy(rootPresets);
                        }
                        else
                        {
                            envRoot = new GameObject("env");
                            envRoot.transform.SetParent(this.transform, false);
                        }
                    }
                }
            }

            if (!json.Active)
            {
                if (groundPlane == null && envRoot != null)
                {
                    var existingGround = envRoot.transform.Find("Environment Ground Plane");
                    if (existingGround != null)
                        groundPlane = existingGround.gameObject;
                }

                if (groundPlane != null) groundPlane.SetActive(false);
                RenderSettings.fog = false;
                if (RenderSettings.sun != null && envRoot != null && RenderSettings.sun.transform.parent == envRoot.transform) 
                    RenderSettings.sun.enabled = false;
                if (Camera.main != null)
                {
                    RenderSettings.skybox = null;
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = Color.black;
                }
                return;
            }

            GenerateEnvironment(envRoot, ref groundPlane, json);
        }

        public static void GenerateEnvironment(GameObject envRoot, ref GameObject groundPlane, ArenaEnvPresetsJson json)
        {
            if (Camera.main != null)
            {
                // Sky
                if (json.SkyType == ArenaEnvPresetsJson.SkyTypeType.Color || json.SkyType == ArenaEnvPresetsJson.SkyTypeType.Gradient)
                {
                    RenderSettings.skybox = null;
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    if (ColorUtility.TryParseHtmlString(json.SkyColor, out Color sColor))
                        Camera.main.backgroundColor = sColor;
                }
                else if (json.SkyType == ArenaEnvPresetsJson.SkyTypeType.Atmosphere)
                {
                    Camera.main.clearFlags = CameraClearFlags.Skybox;
                    // Attempt to load default skybox material if null
                    if (RenderSettings.skybox == null)
                    {
                        Material defaultSky = Resources.GetBuiltinResource<Material>("Default-Skybox.mat");
                        if (defaultSky != null) RenderSettings.skybox = defaultSky;
                    }
                }
                else if (json.SkyType == ArenaEnvPresetsJson.SkyTypeType.None)
                {
                    RenderSettings.skybox = null;
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = Color.black;
                }
            }

            // Lighting
            Light mainLight = null;
            if (RenderSettings.sun != null)
            {
                mainLight = RenderSettings.sun;
            }
            else
            {
                // Find existing directional light or create one
                var lights = FindObjectsOfType<Light>();
                foreach (var l in lights)
                    if (l.type == LightType.Directional && l.transform.parent == envRoot.transform) { mainLight = l; break; }

                if (mainLight == null)
                {
                    var lightObj = new GameObject("Environment Directional Light");
                    lightObj.transform.SetParent(envRoot.transform);
                    mainLight = lightObj.AddComponent<Light>();
                    mainLight.type = LightType.Directional;
                    RenderSettings.sun = mainLight;
                }
            }

            if (json.Lighting == ArenaEnvPresetsJson.LightingType.None)
            {
                if (mainLight != null) mainLight.enabled = false;
            }
            else if (mainLight != null)
            {
                mainLight.enabled = true;
                if (json.Lighting == ArenaEnvPresetsJson.LightingType.Point)
                    mainLight.type = LightType.Point;
                else
                    mainLight.type = LightType.Directional;

                mainLight.transform.position = ArenaUnity.ToUnityPosition(json.LightPosition);
                if (mainLight.type == LightType.Directional)
                {
                    mainLight.transform.LookAt(Vector3.zero); // Aim directional light at origin
                    RenderSettings.sun = mainLight; // Link to skybox sun
                }

                mainLight.shadows = json.Shadow ? LightShadows.Soft : LightShadows.None;

                // Replicate A-Frame hemilight + sunlight intensity
                Vector3 sunPos = mainLight.transform.position.normalized;
                float intensity = 1.884f;
                Color hemiSkyCol = Color.white;

                if (json.SkyType != ArenaEnvPresetsJson.SkyTypeType.Atmosphere)
                {
                    if (ColorUtility.TryParseHtmlString(json.SkyColor, out Color skyC))
                    {
                        hemiSkyCol = new Color(
                            (skyC.r + 1.0f) / 2.0f,
                            (skyC.g + 1.0f) / 2.0f,
                            (skyC.b + 1.0f) / 2.0f
                        );
                    }
                }
                else
                {
                    ColorUtility.TryParseHtmlString("#CEE4F0", out hemiSkyCol);
                    // Dim light for night/sunset based on height
                    intensity = 0.314f + (sunPos.y * 1.57f);
                    intensity = Mathf.Max(0.01f, intensity); // clamp to prevent negative
                }

                mainLight.intensity = intensity;

                // Map A-Frame hemilight to Unity ambient trilight
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                RenderSettings.ambientIntensity = intensity;
                RenderSettings.ambientSkyColor = hemiSkyCol;

                if (ColorUtility.TryParseHtmlString(json.GroundColor, out Color gColor))
                    RenderSettings.ambientGroundColor = gColor;

                if (ColorUtility.TryParseHtmlString(json.HorizonColor, out Color hColor))
                    RenderSettings.ambientEquatorColor = hColor;
            }

            // Fog
            if (json.Fog > 0f)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = 1f;
                // A-Frame environment-component uses STAGE_SIZE = 200, far = (1.01 - fog) * STAGE_SIZE * 2
                RenderSettings.fogEndDistance = (1.01f - json.Fog) * 400f;

                if (ColorUtility.TryParseHtmlString(json.HorizonColor, out Color fColor))
                    RenderSettings.fogColor = fColor;
            }
            else
            {
                RenderSettings.fog = false;
            }

            // Basic Ground Plane
            if (groundPlane == null)
            {
                var existingGround = envRoot.transform.Find("Environment Ground Plane");
                if (existingGround != null)
                {
                    groundPlane = existingGround.gameObject;
                }
            }

            if (json.Ground != ArenaEnvPresetsJson.GroundType.None)
            {
                if (groundPlane == null)
                {
                    groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    groundPlane.name = "Environment Ground Plane";
                    groundPlane.transform.SetParent(envRoot.transform, false);
                }
                groundPlane.SetActive(true);

                float scaleX = json.GroundScale.X == 1f ? 200f : (float)json.GroundScale.X;
                float scaleZ = json.GroundScale.Z == 1f ? 200f : (float)json.GroundScale.Z;
                groundPlane.transform.localScale = new Vector3(scaleX / 10f, 1f, scaleZ / 10f); // Unity plane is 10x10 by default
                groundPlane.transform.localPosition = new Vector3(0, 0, 0); // Use origin to match A-Frame default

                var renderer = groundPlane.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = new Material(ArenaUnity.GetLitShader());
                    // A-Frame environment ground doesn't have gloss/smoothness by default
                    if (renderer.sharedMaterial.HasProperty("_Glossiness"))
                        renderer.sharedMaterial.SetFloat("_Glossiness", 0f);
                    if (renderer.sharedMaterial.HasProperty("_Smoothness"))
                        renderer.sharedMaterial.SetFloat("_Smoothness", 0f);

                    if (ColorUtility.TryParseHtmlString(json.GroundColor, out Color gColor))
                    {
                        if (renderer.sharedMaterial.HasProperty(ArenaUnity.ColorPropertyName))
                            renderer.sharedMaterial.SetColor(ArenaUnity.ColorPropertyName, gColor);
                    }
                }
            }
            else if (groundPlane != null)
            {
                groundPlane.SetActive(false);
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
