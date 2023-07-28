using System;
using UnityEngine.Assertions;
using UnityEngine;

namespace References
{
    internal sealed class InstantiatedResource : MonoBehaviour
    {
        internal GameObject Original;
        internal Action<GameObject> ReleaseCallback;
        
        private void Awake() => hideFlags = HideFlags.NotEditable | HideFlags.DontSave;

        private void OnDestroy()
        {
            Assert.IsNotNull(Original);
            Assert.IsNotNull(ReleaseCallback);
            ReleaseCallback(Original);
        }
    }
}