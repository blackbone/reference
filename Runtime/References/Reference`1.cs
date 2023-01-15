using System;
using UnityEngine;
using Newtonsoft.Json;

namespace References
{
    /// <summary>
    /// Typed reference for assets.
    /// </summary>
    [Serializable]
    public struct Reference<T> where T : UnityEngine.Object
    {
        [JsonProperty] [SerializeField] private string guid;
        [JsonProperty] [SerializeField] private string subAsset;
        [JsonIgnore] [SerializeField] private T directReference;
        
        [JsonIgnore] internal string AssetGuid => guid;
        [JsonIgnore] internal string SubAssetName => subAsset;
        [JsonIgnore] internal T Asset => directReference;
        
        /// <summary>
        /// Is reference valid. Checking reference consistency but not checking integrity.
        /// </summary>
        /// <returns> True if reference has valid data. </returns>
        public readonly bool IsValid() => !string.IsNullOrEmpty(guid) && Guid.TryParse(guid, out _);
        
        /// <summary>
        /// Show string representation.
        /// </summary>
        /// <returns></returns>
        public readonly override string ToString() => $"{guid}[{subAsset}]({(directReference != null ? "direct" : "indirect")}";
    }
}