using UnityEngine;
using System.Collections;
using ArenaUnity;
using ArenaUnity.Schemas;
using System.Text.RegularExpressions;

namespace ArenaUnity.Components
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
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

        protected void OnValidate()
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

        public abstract void UpdateObject();
    }
}
