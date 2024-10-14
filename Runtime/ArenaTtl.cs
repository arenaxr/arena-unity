using System;
using UnityEngine;

namespace ArenaUnity
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ArenaObject))]
    public class ArenaTtl : MonoBehaviour
    {
        long? expiration = null;

        private void Start()
        {
        }

        public void SetTtlDeleteTimer(float seconds)
        {
            expiration = DateTimeOffset.Now.ToUnixTimeMilliseconds() + (long)(seconds * 1000);
        }

        private void Update()
        {
            if (expiration != null && DateTimeOffset.Now.ToUnixTimeMilliseconds() > expiration)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.externalDelete = true;
                    Destroy(gameObject);
                }
            }
        }

    }
}
