using System;
using UnityEngine;

namespace References
{
    /// <summary>
    /// Typed reference for assets.
    /// </summary>
    [Serializable]
    public struct Reference<T> where T : UnityEngine.Object
    {
        [SerializeField] private string guid;
        [SerializeField] private string subAsset;
        [SerializeField] private T directReference;
        
        internal string AssetGuid => guid;
        internal string SubAssetName => subAsset;
        internal T Asset => directReference;
        
        /// <summary>
        /// Is reference valid. Checking reference consistency but not checking integrity.
        /// </summary>
        /// <returns> True if reference has valid data. </returns>
        public bool IsValid()
            => !string.IsNullOrEmpty(guid) && Guid.TryParse(guid, out _);
        
        /// <summary>
        /// Show string representation.
        /// </summary>
        /// <returns></returns>
        public readonly override string ToString() => $"{guid}[{subAsset}]({(directReference != null ? "direct" : "indirect")}";
    }
}