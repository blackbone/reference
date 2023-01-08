using System;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        [SerializeField] private string guid;
        [SerializeField] private string subAsset;
        [SerializeField] private UnityEngine.Object directReference;

        internal string AssetGuid => guid;
        internal string SubAssetName => subAsset;
        internal UnityEngine.Object Asset => directReference;
        
        /// <summary>
        /// Is reference valid. Checking reference consistency but not checking integrity.
        /// </summary>
        /// <returns> True if reference has valid data. </returns>
        public bool IsValid()
            => !string.IsNullOrEmpty(guid) && Guid.TryParse(guid, out _) || directReference != null;
        
        /// <summary>
        /// Show string representation.
        /// </summary>
        /// <returns></returns>
        public readonly override string ToString() => $"{guid}[{subAsset}]({(directReference != null ? "direct" : "indirect")}";

    }

}