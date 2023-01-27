using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

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

        public ReferenceScene(string guid)
        {
            this.guid = guid;
        }

        /// <summary>
        /// Is reference valid. Checking reference consistency but not checking integrity.
        /// </summary>
        /// <returns> True if reference has valid data. </returns>
        public readonly bool IsValid()
            => !string.IsNullOrEmpty(guid) && Guid.TryParse(guid, out _);
        
        /// <summary>
        /// Show string representation.
        /// </summary>
        /// <returns></returns>
        public readonly override string ToString() => $"{guid}";
        
        // reference generalization
        public static implicit operator Reference(in ReferenceScene reference)
            => new(reference.guid);

        // this will be invalid in most cases but can be useful for generalizing access
        public static implicit operator ReferenceScene(in Reference reference)
        {
            // anyway there's safety guards
            Assert.IsTrue(string.IsNullOrEmpty(reference.SubAsset), "Reference to sub asset casted to scene reference");
            Assert.IsNull(reference.Asset, "Direct reference to asset casted to scene reference.");
            
            return new ReferenceScene(reference.AssetGuid);
        }
    }
}