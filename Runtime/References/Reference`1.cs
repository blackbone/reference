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

        public Reference(string guid, string subAsset = null, T directReference = null)
        {
            this.guid = guid;
            this.subAsset = subAsset;
            this.directReference = directReference;
        }
        
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

        public static implicit operator Reference(in Reference<T> reference)
            => new(reference.guid, reference.subAsset, reference.directReference);
        
        public static implicit operator Reference<T>(in Reference reference)
            => new(reference.AssetGuid, reference.SubAsset, reference.Asset as T);
    }
}