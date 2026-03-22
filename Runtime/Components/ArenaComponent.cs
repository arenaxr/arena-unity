/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArenaUnity.Components
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ArenaObject))]
    public abstract class ArenaComponent : MonoBehaviour
	{
        public bool apply = false;
        internal bool scriptLoaded = false;
        internal string updatedJson = null;

        protected virtual void Start()
        {
            apply = true;
        }

        protected virtual void Update()
        {
            if (apply)
            {
                ApplyRender();
                apply = false;
            }
        }

        protected virtual void OnValidate()
        {
            if (!scriptLoaded)
            {
                scriptLoaded = true;
            }
            else
            {   // do not publish update on script load
                UpdateObject();
            }
        }

        /// <summary>
        /// Implement the updates from remote Arena Json to local Unity Objects
        /// </summary>
        protected abstract void ApplyRender();

        /// <summary>
        /// Implement the updates from local Unity Objects to remote Arena Json
        /// </summary>
        public abstract void UpdateObject();

        protected void PublishIfChanged(string attributeName, string newJson)
        {
            newJson = StripEmptyStrings(newJson);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{{\"{attributeName}\":{InjectNulls(newJson)}}}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }

        protected void PublishIfChanged(string newJson)
        {
            newJson = StripEmptyStrings(newJson);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{InjectNulls(newJson)}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }

        private string StripEmptyStrings(string json)
        {
            JObject obj = JObject.Parse(json);
            bool stripped = false;
            System.Collections.Generic.List<string> propsToRemove = null;
            foreach (var prop in obj.Properties())
            {
                if (prop.Value.Type == JTokenType.String && prop.Value.ToString() == "")
                {
                    if (propsToRemove == null) propsToRemove = new System.Collections.Generic.List<string>();
                    propsToRemove.Add(prop.Name);
                    stripped = true;
                }
            }
            if (stripped)
            {
                foreach (var name in propsToRemove)
                {
                    obj.Remove(name);
                }
                return obj.ToString(Formatting.None);
            }
            return json;
        }

        private string InjectNulls(string newJson)
        {
            if (updatedJson != null && updatedJson != newJson)
            {
                JObject oldObj = JObject.Parse(updatedJson);
                JObject newObj = JObject.Parse(newJson);
                bool injectedNulls = false;
                foreach (var prop in oldObj.Properties())
                {
                    if (newObj.Property(prop.Name) == null)
                    {
                        newObj.Add(prop.Name, null);
                        injectedNulls = true;
                    }
                }
                if (injectedNulls)
                {
                    return newObj.ToString(Formatting.None);
                }
            }
            return newJson;
        }
    }
}
