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
    /// Define entities as a landmark; Landmarks appears in the landmark list and you can move (teleport) to them; You can define the behavior of the teleport: if you will be at a fixed or random distance, looking at the landmark, fixed offset or if it is constrained by a navmesh (when it exists)
    /// </summary>
    [Serializable]
    public class ArenaLandmarkJson
    {
        public const string componentName = "landmark";

        // landmark member-fields

        private static float defRandomRadiusMin = 0f;
        [JsonProperty(PropertyName = "randomRadiusMin")]
        [Tooltip("Minimum radius from the landmark to teleport to. (randomRadiusMax must > 0)")]
        public float RandomRadiusMin = defRandomRadiusMin;
        public bool ShouldSerializeRandomRadiusMin()
        {
            if (_token != null && _token.SelectToken("randomRadiusMin") != null) return true;
            return (RandomRadiusMin != defRandomRadiusMin);
        }

        private static float defRandomRadiusMax = 0f;
        [JsonProperty(PropertyName = "randomRadiusMax")]
        [Tooltip("Maximum radius from the landmark to teleport to.")]
        public float RandomRadiusMax = defRandomRadiusMax;
        public bool ShouldSerializeRandomRadiusMax()
        {
            if (_token != null && _token.SelectToken("randomRadiusMax") != null) return true;
            return (RandomRadiusMax != defRandomRadiusMax);
        }

        private static object defOffsetPosition = JsonConvert.DeserializeObject("{'x': 0, 'y': 1.6, 'z': 0}");
        [JsonProperty(PropertyName = "offsetPosition")]
        [Tooltip("Use as a static teleport x,y,z offset")]
        public object OffsetPosition = defOffsetPosition;
        public bool ShouldSerializeOffsetPosition()
        {
            if (_token != null && _token.SelectToken("offsetPosition") != null) return true;
            return (OffsetPosition != defOffsetPosition);
        }

        public enum ConstrainToNavMeshType
        {
            [EnumMember(Value = "false")]
            False,
            [EnumMember(Value = "any")]
            Any,
            [EnumMember(Value = "coplanar")]
            Coplanar,
        }
        private static ConstrainToNavMeshType defConstrainToNavMesh = ConstrainToNavMeshType.False;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "constrainToNavMesh")]
        [Tooltip("Teleports should snap to navmesh. Valid values: 'false', 'any', 'coplanar'")]
        public ConstrainToNavMeshType ConstrainToNavMesh = defConstrainToNavMesh;
        public bool ShouldSerializeConstrainToNavMesh()
        {
            if (_token != null && _token.SelectToken("constrainToNavMesh") != null) return true;
            return (ConstrainToNavMesh != defConstrainToNavMesh);
        }

        private static bool defStartingPosition = false;
        [JsonProperty(PropertyName = "startingPosition")]
        [Tooltip("Set to true to use this landmark as a scene start (spawn) position. If several landmarks with startingPosition=true exist in a scene, one will be randomly selected.")]
        public bool StartingPosition = defStartingPosition;
        public bool ShouldSerializeStartingPosition()
        {
            if (_token != null && _token.SelectToken("startingPosition") != null) return true;
            return (StartingPosition != defStartingPosition);
        }

        private static bool defLookAtLandmark = true;
        [JsonProperty(PropertyName = "lookAtLandmark")]
        [Tooltip("Set to true to make users face the landmark when teleported to it.")]
        public bool LookAtLandmark = defLookAtLandmark;
        public bool ShouldSerializeLookAtLandmark()
        {
            if (_token != null && _token.SelectToken("lookAtLandmark") != null) return true;
            return (LookAtLandmark != defLookAtLandmark);
        }

        private static string defLabel = "";
        [JsonProperty(PropertyName = "label")]
        [Tooltip("Landmark description to display in the landmark list")]
        public string Label = defLabel;
        public bool ShouldSerializeLabel()
        {
            return true; // required in json schema 
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaLandmarkJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaLandmarkJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaLandmarkJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}
