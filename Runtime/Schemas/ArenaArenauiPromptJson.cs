/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

// CAUTION: This file is autogenerated from https://github.com/arenaxr/arena-schemas. Changes made here may be overwritten.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ArenaUnity.Schemas
{
    /// <summary>
    /// ARENAUI element which displays prompt with button actions.
    /// </summary>
    [Serializable]
    public class ArenaArenauiPromptJson
    {
        public const string componentName = "arenaui-prompt";

        // arenaui-prompt member-fields

        private static string defTitle = "Prompt";
        [JsonProperty(PropertyName = "title")]
        [Tooltip("Title")]
        public string Title = defTitle;
        public bool ShouldSerializeTitle()
        {
            return true; // required in json schema 
        }

        private static string defDescription = "This is a prompt. Please confirm or cancel.";
        [JsonProperty(PropertyName = "description")]
        [Tooltip("Description")]
        public string Description = defDescription;
        public bool ShouldSerializeDescription()
        {
            if (_token != null && _token.SelectToken("description") != null) return true;
            return (Description != defDescription);
        }

        private static string[] defButtons = ['Confirm', 'Cancel'];
        [JsonProperty(PropertyName = "buttons")]
        [Tooltip("Buttons")]
        public string[] Buttons = defButtons;
        public bool ShouldSerializeButtons()
        {
            return true; // required in json schema 
        }

        private static float defWidth = 1.5f;
        [JsonProperty(PropertyName = "width")]
        [Tooltip("Override width")]
        public float Width = defWidth;
        public bool ShouldSerializeWidth()
        {
            return true; // required in json schema 
        }

        public enum FontType
        {
            [EnumMember(Value = "Roboto")]
            Roboto,
            [EnumMember(Value = "Roboto-Mono")]
            RobotoMono,
        }
        private static FontType defFont = FontType.Roboto;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "font")]
        [Tooltip("Font to use for button text")]
        public FontType Font = defFont;
        public bool ShouldSerializeFont()
        {
            if (_token != null && _token.SelectToken("font") != null) return true;
            return (Font != defFont);
        }

        private static string defCollisionListener = "";
        [JsonProperty(PropertyName = "collision-listener")]
        [Tooltip("Name of the collision-listener, default can be empty string. Collisions trigger click events")]
        public string CollisionListener = defCollisionListener;
        public bool ShouldSerializeCollisionListener()
        {
            if (_token != null && _token.SelectToken("collision-listener") != null) return true;
            return (CollisionListener != defCollisionListener);
        }

        private static bool defHideOnEnterAr = true;
        [JsonProperty(PropertyName = "hide-on-enter-ar")]
        [Tooltip("Hide object when entering AR. Remove component to *not* hide")]
        public bool HideOnEnterAr = defHideOnEnterAr;
        public bool ShouldSerializeHideOnEnterAr()
        {
            if (_token != null && _token.SelectToken("hide-on-enter-ar") != null) return true;
            return (HideOnEnterAr != defHideOnEnterAr);
        }

        private static bool defHideOnEnterVr = true;
        [JsonProperty(PropertyName = "hide-on-enter-vr")]
        [Tooltip("Hide object when entering VR. Remove component to *not* hide")]
        public bool HideOnEnterVr = defHideOnEnterVr;
        public bool ShouldSerializeHideOnEnterVr()
        {
            if (_token != null && _token.SelectToken("hide-on-enter-vr") != null) return true;
            return (HideOnEnterVr != defHideOnEnterVr);
        }

        private static bool defShowOnEnterAr = true;
        [JsonProperty(PropertyName = "show-on-enter-ar")]
        [Tooltip("Show object when entering AR. Hidden otherwise")]
        public bool ShowOnEnterAr = defShowOnEnterAr;
        public bool ShouldSerializeShowOnEnterAr()
        {
            if (_token != null && _token.SelectToken("show-on-enter-ar") != null) return true;
            return (ShowOnEnterAr != defShowOnEnterAr);
        }

        private static bool defShowOnEnterVr = true;
        [JsonProperty(PropertyName = "show-on-enter-vr")]
        [Tooltip("Show object when entering VR. Hidden otherwise")]
        public bool ShowOnEnterVr = defShowOnEnterVr;
        public bool ShouldSerializeShowOnEnterVr()
        {
            if (_token != null && _token.SelectToken("show-on-enter-vr") != null) return true;
            return (ShowOnEnterVr != defShowOnEnterVr);
        }

        private static string defUrl = "";
        [JsonProperty(PropertyName = "url")]
        [Tooltip("Model URL. Store files paths under 'store/users/<username>' (e.g. store/users/wiselab/models/factory_robot_arm/scene.gltf); to use CDN, prefix with `https://arena-cdn.conix.io/` (e.g. https://arena-cdn.conix.io/store/users/wiselab/models/factory_robot_arm/scene.gltf)")]
        public string Url = defUrl;
        public bool ShouldSerializeUrl()
        {
            if (_token != null && _token.SelectToken("url") != null) return true;
            return (Url != defUrl);
        }

        private static bool defScreenshareable = true;
        [JsonProperty(PropertyName = "screenshareable")]
        [Tooltip("Whether or not a user can screenshare on an object")]
        public bool Screenshareable = defScreenshareable;
        public bool ShouldSerializeScreenshareable()
        {
            if (_token != null && _token.SelectToken("screenshareable") != null) return true;
            return (Screenshareable != defScreenshareable);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaArenauiPromptJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaArenauiPromptJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaArenauiPromptJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}