using System;
using Newtonsoft.Json;
using UnityEngine;

namespace References
{
    /// <summary>
    /// Reference to scene.
    /// </summary>
    [Serializable]
    public struct ReferenceScene
    {
        [JsonProperty] [SerializeField] private string guid;
        
        internal string AssetGuid => guid;
        
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
        public readonly override string ToString() => $"{guid}";
    }
}