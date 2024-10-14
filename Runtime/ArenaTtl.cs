using System;
using UnityEngine;

namespace ArenaUnity
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ArenaObject))]
    public class ArenaTtl : MonoBehaviour
    {
        DateTime? expiration = null;

        private void Start()
        {
        }

        public void SetTtlDeleteTimer(float seconds)
        {
            DateTime now = DateTime.Now;
            int secOnly = (int)Math.Truncate(seconds);
            int msOnly = (int)Math.Truncate((seconds - secOnly) * 1000);
            TimeSpan time = new(0, 0, 0, secOnly, msOnly);
            expiration = now.Add(time);
        }

        private void Update()
        {
            Debug.Log($"ttl up: {expiration - DateTime.Now}");
            if (expiration != null && expiration > DateTime.Now)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                    aobj.externalDelete = true;

                Destroy(gameObject);
            }
        }

    }
}
