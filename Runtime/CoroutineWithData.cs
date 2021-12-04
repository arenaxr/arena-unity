/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021, The CONIX Research Center. All rights reserved.
 */

using System.Collections;
using UnityEngine;

namespace ArenaUnity
{
    public class CoroutineWithData
    {
        public Coroutine coroutine { get; private set; }
        public object result;
        private IEnumerator target;
        public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
        {
            this.target = target;
            this.coroutine = owner.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }
        }
    }
}
