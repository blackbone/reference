using System;
using UnityEngine;
using UnityEngine.Serialization;
using Newtonsoft.Json;

namespace References
{
    /// <summary>
    /// Reference to any possible asset in project.
    /// </summary>
    [Serializable]
    public struct Reference
    {
        internal static class Names
        {
            public static string Guid => nameof(guid);
            public static string SubAsset => nameof(subAsset);
            public static string DirectReference => nameof(directReference);
        }
        
        [JsonProperty] [SerializeField] private string guid;
        [JsonProperty] [SerializeField] private string subAsset;
        [JsonIgnore] [SerializeField] private UnityEngine.Object directReference;

        [JsonIgnore] internal string AssetGuid => guid;
        [JsonIgnore] internal string SubAssetName => subAsset;
        [JsonIgnore] internal UnityEngine.Object Asset => directReference;
        
        /// <summary>
        /// Is reference valid. Checking reference consistency but not checking integrity.
        /// </summary>
        /// <returns> True if reference has valid data. </returns>
        public readonly bool IsValid()
            => !string.IsNullOrEmpty(guid) && Guid.TryParse(guid, out _) || directReference != null;
        
        /// <summary>
        /// Show string representation.
        /// </summary>
        /// <returns></returns>
        public readonly override string ToString() => $"{guid}[{subAsset}]({(directReference != null ? "direct" : "indirect")}";

    }

}