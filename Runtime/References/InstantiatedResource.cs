using System;
using NUnit.Framework;
using UnityEngine;

namespace References
{
    internal sealed class InstantiatedResource : MonoBehaviour
    {
        internal GameObject Original;
        internal Action<GameObject> ReleaseCallback;
        public InstantiatedResource() => hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
        private void OnDestroy()
        {
            Assert.IsNotNull(Original);
            Assert.IsNotNull(ReleaseCallback);
            ReleaseCallback(Original);
        }
    }
}