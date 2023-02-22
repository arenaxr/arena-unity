/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

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
    /// A-Frame Environment presets. More properties at <a href='https://github.com/supermedium/aframe-environment-component'>https://github.com/supermedium/aframe-environment-component</a>
    /// </summary>
    [Serializable]
    public class ArenaEnvironmentPresetsJson
    {
        public const string componentName = "environment-presets";

        // environment-presets member-fields

        private static bool defActive = true;
        [JsonProperty(PropertyName = "active")]
        [Tooltip("Show/hides the environment presets component. Use this instead of using the visible attribute.")]
        public bool Active = defActive;
        public bool ShouldSerializeActive()
        {
            return true; // required in json schema 
        }

        public enum DressingType
        {
            [EnumMember(Value = "apparatus")]
            Apparatus,
            [EnumMember(Value = "arches")]
            Arches,
            [EnumMember(Value = "cubes")]
            Cubes,
            [EnumMember(Value = "cylinders")]
            Cylinders,
            [EnumMember(Value = "hexagons")]
            Hexagons,
            [EnumMember(Value = "mushrooms")]
            Mushrooms,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "pyramids")]
            Pyramids,
            [EnumMember(Value = "stones")]
            Stones,
            [EnumMember(Value = "torii")]
            Torii,
            [EnumMember(Value = "towers")]
            Towers,
            [EnumMember(Value = "trees")]
            Trees,
        }
        private static DressingType defDressing = DressingType.None;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "dressing")]
        [Tooltip("Dressing is the term we use here for the set of additional objects that are put on the ground for decoration.")]
        public DressingType Dressing = defDressing;
        public bool ShouldSerializeDressing()
        {
            if (_token != null && _token.SelectToken("dressing") != null) return true;
            return (Dressing != defDressing);
        }

        private static float defDressingAmount = 10f;
        [JsonProperty(PropertyName = "dressingAmount")]
        [Tooltip("Number of objects used for dressing")]
        public float DressingAmount = defDressingAmount;
        public bool ShouldSerializeDressingAmount()
        {
            if (_token != null && _token.SelectToken("dressingAmount") != null) return true;
            return (DressingAmount != defDressingAmount);
        }

        private static string defDressingColor = "#795449";
        [JsonProperty(PropertyName = "dressingColor")]
        [Tooltip("Base color of dressing objects.")]
        public string DressingColor = defDressingColor;
        public bool ShouldSerializeDressingColor()
        {
            if (_token != null && _token.SelectToken("dressingColor") != null) return true;
            return (DressingColor != defDressingColor);
        }

        private static float defDressingOnPlayArea = 0f;
        [JsonProperty(PropertyName = "dressingOnPlayArea")]
        [Tooltip("Amount of dressing on play area.")]
        public float DressingOnPlayArea = defDressingOnPlayArea;
        public bool ShouldSerializeDressingOnPlayArea()
        {
            if (_token != null && _token.SelectToken("dressingOnPlayArea") != null) return true;
            return (DressingOnPlayArea != defDressingOnPlayArea);
        }

        private static float defDressingScale = 5f;
        [JsonProperty(PropertyName = "dressingScale")]
        [Tooltip("Height (in meters) of dressing objects.")]
        public float DressingScale = defDressingScale;
        public bool ShouldSerializeDressingScale()
        {
            if (_token != null && _token.SelectToken("dressingScale") != null) return true;
            return (DressingScale != defDressingScale);
        }

        private static bool defDressingUniformScale = true;
        [JsonProperty(PropertyName = "dressingUniformScale")]
        [Tooltip("If false, a different value is used for each coordinate x, y, z in the random variance of size.")]
        public bool DressingUniformScale = defDressingUniformScale;
        public bool ShouldSerializeDressingUniformScale()
        {
            if (_token != null && _token.SelectToken("dressingUniformScale") != null) return true;
            return (DressingUniformScale != defDressingUniformScale);
        }

        private static object defDressingVariance = JsonConvert.DeserializeObject("{'x': 1, 'y': 1, 'z': 1}");
        [JsonProperty(PropertyName = "dressingVariance")]
        [Tooltip("Vector3")]
        public object DressingVariance = defDressingVariance;
        public bool ShouldSerializeDressingVariance()
        {
            if (_token != null && _token.SelectToken("dressingVariance") != null) return true;
            return (DressingVariance != defDressingVariance);
        }

        private static bool defFlatShading = false;
        [JsonProperty(PropertyName = "flatShading")]
        [Tooltip("Whether to show everything smoothed (false) or polygonal (true).")]
        public bool FlatShading = defFlatShading;
        public bool ShouldSerializeFlatShading()
        {
            if (_token != null && _token.SelectToken("flatShading") != null) return true;
            return (FlatShading != defFlatShading);
        }

        private static float defFog = 0f;
        [JsonProperty(PropertyName = "fog")]
        [Tooltip("Amount of fog (0 = none, 1 = full fog). The color is estimated automatically.")]
        public float Fog = defFog;
        public bool ShouldSerializeFog()
        {
            if (_token != null && _token.SelectToken("fog") != null) return true;
            return (Fog != defFog);
        }

        public enum GridType
        {
            [EnumMember(Value = "1x1")]
            Onex1,
            [EnumMember(Value = "2x2")]
            Twox2,
            [EnumMember(Value = "crosses")]
            Crosses,
            [EnumMember(Value = "dots")]
            Dots,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "xlines")]
            Xlines,
            [EnumMember(Value = "ylines")]
            Ylines,
        }
        private static GridType defGrid = GridType.None;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "grid")]
        [Tooltip("1x1 and 2x2 are rectangular grids of 1 and 2 meters side, respectively.")]
        public GridType Grid = defGrid;
        public bool ShouldSerializeGrid()
        {
            if (_token != null && _token.SelectToken("grid") != null) return true;
            return (Grid != defGrid);
        }

        private static string defGridColor = "#ccc";
        [JsonProperty(PropertyName = "gridColor")]
        [Tooltip("Color of the grid.")]
        public string GridColor = defGridColor;
        public bool ShouldSerializeGridColor()
        {
            if (_token != null && _token.SelectToken("gridColor") != null) return true;
            return (GridColor != defGridColor);
        }

        public enum GroundType
        {
            [EnumMember(Value = "canyon")]
            Canyon,
            [EnumMember(Value = "flat")]
            Flat,
            [EnumMember(Value = "hills")]
            Hills,
            [EnumMember(Value = "noise")]
            Noise,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "spikes")]
            Spikes,
        }
        private static GroundType defGround = GroundType.Hills;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "ground")]
        [Tooltip("Orography style.")]
        public GroundType Ground = defGround;
        public bool ShouldSerializeGround()
        {
            if (_token != null && _token.SelectToken("ground") != null) return true;
            return (Ground != defGround);
        }

        private static string defGroundColor = "#553e35";
        [JsonProperty(PropertyName = "groundColor")]
        [Tooltip("Main color of the ground.")]
        public string GroundColor = defGroundColor;
        public bool ShouldSerializeGroundColor()
        {
            if (_token != null && _token.SelectToken("groundColor") != null) return true;
            return (GroundColor != defGroundColor);
        }

        private static string defGroundColor2 = "#694439";
        [JsonProperty(PropertyName = "groundColor2")]
        [Tooltip("Secondary color of the ground. Used for textures, ignored if groundTexture is none.")]
        public string GroundColor2 = defGroundColor2;
        public bool ShouldSerializeGroundColor2()
        {
            if (_token != null && _token.SelectToken("groundColor2") != null) return true;
            return (GroundColor2 != defGroundColor2);
        }

        private static object defGroundScale = JsonConvert.DeserializeObject("{'x': 1, 'y': 1, 'z': 1}");
        [JsonProperty(PropertyName = "groundScale")]
        [Tooltip("Vector3")]
        public object GroundScale = defGroundScale;
        public bool ShouldSerializeGroundScale()
        {
            if (_token != null && _token.SelectToken("groundScale") != null) return true;
            return (GroundScale != defGroundScale);
        }

        public enum GroundTextureType
        {
            [EnumMember(Value = "checkerboard")]
            Checkerboard,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "squares")]
            Squares,
            [EnumMember(Value = "walkernoise")]
            Walkernoise,
        }
        private static GroundTextureType defGroundTexture = GroundTextureType.None;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "groundTexture")]
        [Tooltip("Texture applied to the ground.")]
        public GroundTextureType GroundTexture = defGroundTexture;
        public bool ShouldSerializeGroundTexture()
        {
            if (_token != null && _token.SelectToken("groundTexture") != null) return true;
            return (GroundTexture != defGroundTexture);
        }

        private static float defGroundYScale = 3f;
        [JsonProperty(PropertyName = "groundYScale")]
        [Tooltip("Maximum height (in meters) of ground's features (hills, mountains, peaks..).")]
        public float GroundYScale = defGroundYScale;
        public bool ShouldSerializeGroundYScale()
        {
            if (_token != null && _token.SelectToken("groundYScale") != null) return true;
            return (GroundYScale != defGroundYScale);
        }

        private static bool defHideInAR = true;
        [JsonProperty(PropertyName = "hideInAR")]
        [Tooltip("If true, hide the environment when entering AR.")]
        public bool HideInAR = defHideInAR;
        public bool ShouldSerializeHideInAR()
        {
            if (_token != null && _token.SelectToken("hideInAR") != null) return true;
            return (HideInAR != defHideInAR);
        }

        private static string defHorizonColor = "#ffa500";
        [JsonProperty(PropertyName = "horizonColor")]
        [Tooltip("Horizon Color")]
        public string HorizonColor = defHorizonColor;
        public bool ShouldSerializeHorizonColor()
        {
            if (_token != null && _token.SelectToken("horizonColor") != null) return true;
            return (HorizonColor != defHorizonColor);
        }

        public enum LightingType
        {
            [EnumMember(Value = "distant")]
            Distant,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "point")]
            Point,
        }
        private static LightingType defLighting = LightingType.Distant;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "lighting")]
        [Tooltip("A hemisphere light and a key light (directional or point) are added to the scene automatically when using the component. Use none if you don't want this automatic lighting set being added. The color and intensity are estimated automatically.")]
        public LightingType Lighting = defLighting;
        public bool ShouldSerializeLighting()
        {
            if (_token != null && _token.SelectToken("lighting") != null) return true;
            return (Lighting != defLighting);
        }

        private static object defLightPosition = JsonConvert.DeserializeObject("{'x': 0, 'y': 1, 'z': -0.2}");
        [JsonProperty(PropertyName = "lightPosition")]
        [Tooltip("Vector3")]
        public object LightPosition = defLightPosition;
        public bool ShouldSerializeLightPosition()
        {
            if (_token != null && _token.SelectToken("lightPosition") != null) return true;
            return (LightPosition != defLightPosition);
        }

        private static float defPlayArea = 1f;
        [JsonProperty(PropertyName = "playArea")]
        [Tooltip("Radius of the area in the center reserved for the player and the gameplay. The ground is flat in there and no objects are placed inside.")]
        public float PlayArea = defPlayArea;
        public bool ShouldSerializePlayArea()
        {
            if (_token != null && _token.SelectToken("playArea") != null) return true;
            return (PlayArea != defPlayArea);
        }

        public enum PresetType
        {
            [EnumMember(Value = "arches")]
            Arches,
            [EnumMember(Value = "checkerboard")]
            Checkerboard,
            [EnumMember(Value = "contact")]
            Contact,
            [EnumMember(Value = "default")]
            Default,
            [EnumMember(Value = "dream")]
            Dream,
            [EnumMember(Value = "egypt")]
            Egypt,
            [EnumMember(Value = "forest")]
            Forest,
            [EnumMember(Value = "goaland")]
            Goaland,
            [EnumMember(Value = "goldmine")]
            Goldmine,
            [EnumMember(Value = "japan")]
            Japan,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "osiris")]
            Osiris,
            [EnumMember(Value = "poison")]
            Poison,
            [EnumMember(Value = "starry")]
            Starry,
            [EnumMember(Value = "threetowers")]
            Threetowers,
            [EnumMember(Value = "tron")]
            Tron,
            [EnumMember(Value = "volcano")]
            Volcano,
            [EnumMember(Value = "yavapai")]
            Yavapai,
        }
        private static PresetType defPreset = PresetType.Default;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "preset")]
        [Tooltip("An A-frame preset environment.")]
        public PresetType Preset = defPreset;
        public bool ShouldSerializePreset()
        {
            return true; // required in json schema 
        }

        private static float defSeed = 1f;
        [JsonProperty(PropertyName = "seed")]
        [Tooltip("Seed for randomization. If you don't like the layout of the elements, try another value for the seed.")]
        public float Seed = defSeed;
        public bool ShouldSerializeSeed()
        {
            if (_token != null && _token.SelectToken("seed") != null) return true;
            return (Seed != defSeed);
        }

        private static bool defShadow = false;
        [JsonProperty(PropertyName = "shadow")]
        [Tooltip("Shadows on/off. Sky light casts shadows on the ground of all those objects with shadow component applied")]
        public bool Shadow = defShadow;
        public bool ShouldSerializeShadow()
        {
            if (_token != null && _token.SelectToken("shadow") != null) return true;
            return (Shadow != defShadow);
        }

        private static float defShadowSize = 10f;
        [JsonProperty(PropertyName = "shadowSize")]
        [Tooltip("Size of the shadow, if applied")]
        public float ShadowSize = defShadowSize;
        public bool ShouldSerializeShadowSize()
        {
            if (_token != null && _token.SelectToken("shadowSize") != null) return true;
            return (ShadowSize != defShadowSize);
        }

        private static string defSkyColor = "#ffa500";
        [JsonProperty(PropertyName = "skyColor")]
        [Tooltip("Sky Color")]
        public string SkyColor = defSkyColor;
        public bool ShouldSerializeSkyColor()
        {
            if (_token != null && _token.SelectToken("skyColor") != null) return true;
            return (SkyColor != defSkyColor);
        }

        public enum SkyTypeType
        {
            [EnumMember(Value = "atmosphere")]
            Atmosphere,
            [EnumMember(Value = "color")]
            Color,
            [EnumMember(Value = "gradient")]
            Gradient,
            [EnumMember(Value = "none")]
            None,
        }
        private static SkyTypeType defSkyType = SkyTypeType.Color;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "skyType")]
        [Tooltip("A sky type")]
        public SkyTypeType SkyType = defSkyType;
        public bool ShouldSerializeSkyType()
        {
            if (_token != null && _token.SelectToken("skyType") != null) return true;
            return (SkyType != defSkyType);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaEnvironmentPresetsJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            return JsonConvert.DeserializeObject<ArenaEnvironmentPresetsJson>(Regex.Unescape(jsonString));
        }
    }
}
