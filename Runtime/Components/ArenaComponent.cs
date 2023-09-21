/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using UnityEngine;

namespace ArenaUnity.Components
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ArenaObject))]
    public abstract class ArenaComponent : MonoBehaviour
	{
        internal bool apply = false;
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
    }
}
