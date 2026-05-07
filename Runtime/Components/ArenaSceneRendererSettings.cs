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
    public class ArenaSceneRendererSettings: ArenaComponent
    {
        // ARENA renderer-settings component unity conversion status:
        // DONE: localClippingEnabled
        // DONE: outputColorSpace
        // DONE: physicallyCorrectLights
        // DONE: sortObjects

        public ArenaRendererSettingsJson json = new ArenaRendererSettingsJson();

        protected override void ApplyRender()
        {
            if (Camera.main != null)
            {
                if (json.SortObjects)
                {
                    Camera.main.opaqueSortMode = UnityEngine.Rendering.OpaqueSortMode.Default;
                    Camera.main.transparencySortMode = TransparencySortMode.Default;
                }
                else
                {
                    Camera.main.opaqueSortMode = UnityEngine.Rendering.OpaqueSortMode.NoDistanceSort;
                    Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
                    Camera.main.transparencySortAxis = Vector3.up; // arbitrary axis to disrupt standard sorting
                }
            }

            // outputColorSpace mapping warning
            bool isLinearUnity = QualitySettings.activeColorSpace == ColorSpace.Linear;
            bool isLinearArena = json.OutputColorSpace == ArenaRendererSettingsJson.OutputColorSpaceType.LinearSRGBColorSpace;
            if (isLinearUnity != isLinearArena && json.OutputColorSpace != ArenaRendererSettingsJson.OutputColorSpaceType.NoColorSpace)
            {
                Debug.LogWarning($"[ArenaSceneRendererSettings] ARENA requests outputColorSpace '{json.OutputColorSpace}' but Unity project is baked with '{QualitySettings.activeColorSpace}'. Cannot change at runtime.");
            }

            // localClippingEnabled warning
            if (json.LocalClippingEnabled)
            {
                Debug.LogWarning("[ArenaSceneRendererSettings] localClippingEnabled is true, but Unity lacks native global clipping plane support without custom shaders.");
            }

            // physicallyCorrectLights warning
            if (json.PhysicallyCorrectLights != UnityEngine.Rendering.GraphicsSettings.lightsUseLinearIntensity)
            {
                Debug.LogWarning($"[ArenaSceneRendererSettings] physicallyCorrectLights is {json.PhysicallyCorrectLights}, but Unity GraphicsSettings.lightsUseLinearIntensity is {UnityEngine.Rendering.GraphicsSettings.lightsUseLinearIntensity}. Cannot change at runtime.");
            }
        }

        public override void UpdateObject()
        {
            PublishIfChanged(json.attributeName, JsonConvert.SerializeObject(json));
        }
    }
}
