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
    /// Opens an HTML prompt when clicked. Sends text input as an event on MQTT. Requires click-listener.
    /// </summary>
    [Serializable]
    public class ArenaTextinputJson
    {
        public const string componentName = "textinput";

        // textinput member-fields

        public enum OnType
        {
            [EnumMember(Value = "mousedown")]
            Mousedown,
            [EnumMember(Value = "mouseup")]
            Mouseup,
            [EnumMember(Value = "mouseenter")]
            Mouseenter,
            [EnumMember(Value = "mouseleave")]
            Mouseleave,
            [EnumMember(Value = "triggerdown")]
            Triggerdown,
            [EnumMember(Value = "triggerup")]
            Triggerup,
            [EnumMember(Value = "gripdown")]
            Gripdown,
            [EnumMember(Value = "gripup")]
            Gripup,
            [EnumMember(Value = "menudown")]
            Menudown,
            [EnumMember(Value = "menuup")]
            Menuup,
            [EnumMember(Value = "systemdown")]
            Systemdown,
            [EnumMember(Value = "systemup")]
            Systemup,
            [EnumMember(Value = "trackpaddown")]
            Trackpaddown,
            [EnumMember(Value = "trackpadup")]
            Trackpadup,
        }
        private static OnType defOn = OnType.Mousedown;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "on")]
        [Tooltip("A case-sensitive string representing the event type to listen for, e.g. 'mousedown', 'mouseup'. See https://developer.mozilla.org/en-US/docs/Web/Events")]
        public OnType On = defOn;
        public bool ShouldSerializeOn()
        {
            if (_token != null && _token.SelectToken("on") != null) return true;
            return (On != defOn);
        }

        private static string defTitle = "Text Input";
        [JsonProperty(PropertyName = "title")]
        [Tooltip("The prompt title")]
        public string Title = defTitle;
        public bool ShouldSerializeTitle()
        {
            if (_token != null && _token.SelectToken("title") != null) return true;
            return (Title != defTitle);
        }

        private static string defLabel = "Input text below (max is 140 characters)";
        [JsonProperty(PropertyName = "label")]
        [Tooltip("Text prompt label")]
        public string Label = defLabel;
        public bool ShouldSerializeLabel()
        {
            if (_token != null && _token.SelectToken("label") != null) return true;
            return (Label != defLabel);
        }

        private static string defPlaceholder = "Type here";
        [JsonProperty(PropertyName = "placeholder")]
        [Tooltip("Text input place holder")]
        public string Placeholder = defPlaceholder;
        public bool ShouldSerializePlaceholder()
        {
            if (_token != null && _token.SelectToken("placeholder") != null) return true;
            return (Placeholder != defPlaceholder);
        }

        // General json object management

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private static JToken _token;

        public string SaveToString()
        {
            return Regex.Unescape(JsonConvert.SerializeObject(this));
        }

        public static ArenaTextinputJson CreateFromJSON(string jsonString, JToken token)
        {
            _token = token; // save updated wire json
            ArenaTextinputJson json = null;
            try {
                json = JsonConvert.DeserializeObject<ArenaTextinputJson>(Regex.Unescape(jsonString));
            } catch (JsonReaderException e)
            {
                Debug.LogWarning($"{e.Message}: {jsonString}");
            }
            return json;
        }
    }
}