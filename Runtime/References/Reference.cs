namespace References
{
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Structure that holds reference to some asset in project. Asset is referenced directly or by GUID.
    /// </summary>
    [Serializable]
    public struct Reference
    {
        /// <summary>
        /// Default value of reference. Used to be compared as invalid reference.
        /// </summary>
        public static Reference Default = default;

        [SerializeField] private string             assetGuid;
        [SerializeField] private UnityEngine.Object asset;

        /// <summary>
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's asset GUID. </param>
        public Reference(string assetGuid)
        {
            this.assetGuid = assetGuid;
            asset          = null;
        }

        /// <summary>
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's asset GUID. </param>
        /// <param name="asset"> Direct reference to asset. </param>
        public Reference(string assetGuid, UnityEngine.Object asset)
        {
            this.assetGuid = assetGuid;
            this.asset     = asset;
        }

        /// <summary>
        /// Unity's asset GUID.
        /// </summary>
        public readonly   string             AssetGuid => assetGuid;
        
        /// <summary>
        /// Direct reference to asset.
        /// </summary>
        internal readonly UnityEngine.Object Asset     => asset;
        
        /// <summary>
        /// Is assigned guid is a valid GUID. This not guarantee that asset exists or will be accessible.
        /// </summary>
        public readonly bool HasValidAssetGuid => Guid.TryParse(AssetGuid, out _);

        public static bool operator ==(Reference x, Reference y)
            => x.AssetGuid == y.AssetGuid;

        public static bool operator !=(Reference x, Reference y)
            => x.AssetGuid != y.AssetGuid;

        public static implicit operator string(Reference value)
            => value.AssetGuid;

        public readonly override bool Equals(object other)
            => other != null && other.GetHashCode() == GetHashCode();

        public readonly override int GetHashCode()
            => AssetGuid.GetHashCode();

        /// <summary>
        /// Converts abstract reference to generic version.
        /// </summary>
        /// <typeparam name="T">Generic type definition to interpret asset as.</typeparam>
        /// <returns> Generic version of reference. </returns>
        public readonly Reference<T> ToReference<T>() where T : UnityEngine.Object
            => string.IsNullOrWhiteSpace(AssetGuid) ? default : new Reference<T>(AssetGuid, asset as T);

        public readonly override string ToString()
            => AssetGuid;

        public bool IsValid() => !string.IsNullOrEmpty(assetGuid) && Guid.TryParse(assetGuid, out _);
    }

    /// <summary>
    /// Structure that holds reference to scene in project. Scene asset is referenced directly or by GUID.
    /// </summary>
    [Serializable]
    public struct ReferenceScene
    {
        /// <summary>
        /// Default value of reference. Used to be compared as invalid reference.
        /// </summary>
        public static ReferenceScene Default = new ReferenceScene(null);

        [SerializeField] private string assetGuid;

        /// <summary>
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's scene GUID. </param>
        public ReferenceScene(string assetGuid)
        {
            this.assetGuid = assetGuid;
        }
        /// <summary>
        /// Unity's asset GUID.
        /// </summary>
        public readonly string AssetGuid => assetGuid;
        
        /// <summary>
        /// Is assigned guid is a valid GUID. This not guarantee that asset exists or will be accessible.
        /// </summary>
        public readonly bool HasValidAssetGuid => Guid.TryParse(AssetGuid, out _);

        public static bool operator ==(ReferenceScene x, Reference y)
            => x.AssetGuid == y.AssetGuid;

        public static bool operator ==(ReferenceScene x, ReferenceScene y)
            => x.AssetGuid == y.AssetGuid;

        public static bool operator !=(ReferenceScene x, Reference y)
            => x.AssetGuid != y.AssetGuid;

        public static bool operator !=(ReferenceScene x, ReferenceScene y)
            => x.AssetGuid != y.AssetGuid;

        public static implicit operator Reference(ReferenceScene value)
            => value.ToReference();

        public static implicit operator string(ReferenceScene value)
            => value.AssetGuid;

        public readonly override bool Equals(object other)
            => other != null && other.GetHashCode() == GetHashCode();

        public readonly override int GetHashCode()
            => AssetGuid.GetHashCode();

        /// <summary>
        /// Converts scene reference to abstract reference.
        /// </summary>
        /// <returns> Abstract version of reference. </returns>
        public readonly Reference ToReference()
            => string.IsNullOrWhiteSpace(AssetGuid) ? default : new Reference(AssetGuid);

        /// <summary>
        /// Converts scene reference to generic version. (this is a point to submit)
        /// </summary>
        /// <typeparam name="TRefence">Generic type definition to interpret asset as.</typeparam>
        /// <returns> Generic version of reference. </returns>
        public readonly Reference<TReference> ToReference<TReference>() where TReference : UnityEngine.Object
            => string.IsNullOrWhiteSpace(AssetGuid) ? default : new Reference<TReference>(AssetGuid);

        public readonly override string ToString()
            => AssetGuid;

        public bool IsValid()
            => !string.IsNullOrEmpty(assetGuid) && Guid.TryParse(assetGuid, out _);
    }

    /// <summary>
    /// Generic version of reference. T parameter is used to interpret asset.
    /// </summary>
    /// <typeparam name="T"> Generic type restiction for asset. </typeparam>
    [Serializable]
    public struct Reference<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// Default value of reference. Used to be compared as invalid reference.
        /// </summary>
        public static Reference<T> Default = new Reference<T>(null);

        [SerializeField] private string assetGuid;
        [SerializeField] private T      asset;

        /// <summary>
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's scene GUID. </param>
        public Reference(string assetGuid)
        {
            this.assetGuid = assetGuid;
            asset          = null;
        }

        /// <summary>
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's asset GUID. </param>
        /// <param name="asset"> Direct reference to asset. </param>
        public Reference(string assetGuid, T asset)
        {
            this.assetGuid = assetGuid;
            this.asset     = asset;
        }

        /// <summary>
        /// Unity's asset GUID.
        /// </summary>
        public readonly   string AssetGuid => assetGuid;
        
        /// <summary>
        /// Direct reference to asset.
        /// </summary>
        internal readonly T      Asset     => asset;

        /// <summary>
        /// Is assigned guid is a valid GUID. This not guarantee that asset exists or will be accessible.
        /// </summary>
        public readonly bool HasValidAssetGuid => Guid.TryParse(AssetGuid, out _);

        public static bool operator ==(Reference<T> x, Reference y)
            => x.AssetGuid == y.AssetGuid;

        public static bool operator ==(Reference<T> x, Reference<T> y)
            => x.AssetGuid == y.AssetGuid;

        public static bool operator !=(Reference<T> x, Reference y)
            => x.AssetGuid != y.AssetGuid;

        public static bool operator !=(Reference<T> x, Reference<T> y)
            => x.AssetGuid != y.AssetGuid;

        public static implicit operator Reference(Reference<T> value)
            => value.ToReference();

        public static implicit operator string(Reference<T> value)
            => value.AssetGuid;

        public readonly override bool Equals(object other)
            => other != null && other.GetHashCode() == GetHashCode();

        public readonly override int GetHashCode()
            => AssetGuid.GetHashCode();

        /// <summary>
        /// Converts scene reference to abstract reference.
        /// </summary>
        /// <returns> Abstract version of reference. </returns>
        public readonly Reference ToReference()
            => string.IsNullOrWhiteSpace(AssetGuid) ? default : new Reference(AssetGuid);

        /// <summary>
        /// Converts scene reference to generic version. (this is a point to submit)
        /// </summary>
        /// <typeparam name="TRefence">Generic type definition to interpret asset as.</typeparam>
        /// <returns> Generic version of reference. </returns>
        public readonly Reference<TRefence> ToReference<TRefence>() where TRefence : UnityEngine.Object
            => string.IsNullOrWhiteSpace(AssetGuid) ? default : new Reference<TRefence>(AssetGuid);

        public readonly override string ToString()
            => AssetGuid;

        public bool IsValid()
            => !string.IsNullOrEmpty(assetGuid) && Guid.TryParse(assetGuid, out _);
    }
}
