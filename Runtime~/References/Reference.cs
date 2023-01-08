using UnityEditor;
using UnityEngine.Serialization;

namespace References
{
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Structure that holds reference to some asset in project. Asset is referenced directly or by GUID.
    /// </summary>
    [Serializable]
    public struct Reference : IReference
    {
        internal static class Names
        {
            public static string AssetGuid => nameof(assetGuid);
            public static string InstanceId => nameof(instanceId);
            public static string Asset => nameof(asset);
        }

        /// <summary>
        /// Default value of reference. Used to be compared as invalid reference.
        /// </summary>
        public static Reference Default = default;

        [SerializeField] private string assetGuid;
        [SerializeField] private int instanceId;
        [SerializeField] private UnityEngine.Object asset;

        /// <summary>
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's asset GUID. </param>
        public Reference(string assetGuid, int instanceId = 0)
        {
            this.assetGuid = assetGuid;
            this.instanceId = 0;
            this.asset = null;
        }

        /// <summary>
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's asset GUID. </param>
        /// <param name="asset"> Direct reference to asset. </param>
        public Reference(string assetGuid, UnityEngine.Object asset, int instanceId = 0)
        {
            this.assetGuid = assetGuid;
            this.instanceId = instanceId;
            this.asset = asset;
        }

        /// <summary>
        /// Unity's asset GUID.
        /// </summary>
        public readonly string AssetGuid => assetGuid;

        public readonly int InstanceId => instanceId;

        /// <summary>
        /// Direct reference to asset.
        /// </summary>
        internal readonly UnityEngine.Object Asset => asset;

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

        /// <summary>
        /// Is reference valid. Checking reference consistency but not checking integrity.
        /// </summary>
        /// <returns> True if reference has valid data. </returns>
        public bool IsValid() => !string.IsNullOrEmpty(assetGuid) && Guid.TryParse(assetGuid, out _);
    }

    /// <summary>
    /// Structure that holds reference to scene in project. Scene asset is referenced directly or by GUID.
    /// </summary>
    [Serializable]
    public struct ReferenceScene : IReference
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
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's scene GUID. </param>
        /// <param name="respurcePath"> Unity's scene name. </param>
        public ReferenceScene(string assetGuid, string respurcePath)
        {
            this.assetGuid = assetGuid;
        }

        /// <summary>
        /// Unity's asset GUID.
        /// </summary>
        public readonly string AssetGuid => assetGuid;
        
        /// <summary>
        /// Instance id is not suitable for scenes because there's no such.
        /// Returns 0.s
        /// </summary>
        readonly int IReference.InstanceId => 0;

        /// <summary>
        /// Is assigned guid is a valid GUID. This not guarantee that asset exists or will be accessible.
        /// </summary>
        public readonly bool HasValidAssetGuid => Guid.TryParse(AssetGuid, out _);

        public static bool operator ==(ReferenceScene x, Reference y)
            => x.AssetGuid == y.AssetGuid && y.InstanceId == 0;

        public static bool operator ==(ReferenceScene x, ReferenceScene y)
            => x.AssetGuid == y.AssetGuid;

        public static bool operator !=(ReferenceScene x, Reference y)
            => x.AssetGuid != y.AssetGuid || y.InstanceId != 0;

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
            => string.IsNullOrWhiteSpace(AssetGuid) ? default : new Reference(AssetGuid, 0);

        /// <summary>
        /// Converts scene reference to generic version. (this is a point to submit)
        /// </summary>
        /// <typeparam name="TReference">Generic type definition to interpret asset as.</typeparam>
        /// <returns> Generic version of reference. </returns>
        public readonly Reference<TReference> ToReference<TReference>() where TReference : UnityEngine.Object
            => string.IsNullOrWhiteSpace(AssetGuid) ? default : new Reference<TReference>(AssetGuid);

        public readonly override string ToString()
            => AssetGuid;

        /// <summary>
        /// Is reference valid. Checking reference consistency but not checking integrity.
        /// </summary>
        /// <returns> True if reference has valid data. </returns>
        public bool IsValid()
            => !string.IsNullOrEmpty(assetGuid) && Guid.TryParse(assetGuid, out _);
    }

    /// <summary>
    /// Generic version of reference. T parameter is used to interpret asset.
    /// </summary>
    /// <typeparam name="T"> Generic type restiction for asset. </typeparam>
    [Serializable]
    public struct Reference<T> : IReference where T : UnityEngine.Object
    {
        /// <summary>
        /// Default value of reference. Used to be compared as invalid reference.
        /// </summary>
        public static Reference<T> Default = new Reference<T>(null);

        [SerializeField] private string assetGuid;
        [SerializeField] private int instanceId;
        [SerializeField] private T asset;

        /// <summary>
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's scene GUID. </param>
        public Reference(string assetGuid, int instanceId = 0)
        {
            this.assetGuid = assetGuid;
            this.instanceId = instanceId;
            asset = null;
        }

        /// <summary>
        /// Initializes new reference instance.
        /// </summary>
        /// <param name="assetGuid"> Unity's asset GUID. </param>
        /// <param name="asset"> Direct reference to asset. </param>
        public Reference(string assetGuid, T asset, int instanceId = 0)
        {
            this.assetGuid = assetGuid;
            this.instanceId = instanceId;
            this.asset = asset;
        }

        /// <summary>
        /// Unity's asset GUID.
        /// </summary>
        public readonly string AssetGuid => assetGuid;

        public readonly int InstanceId => instanceId;

        /// <summary>
        /// Direct reference to asset.
        /// </summary>
        internal readonly T Asset => asset;

        /// <summary>
        /// Is assigned guid is a valid GUID. This not guarantee that asset exists or will be accessible.
        /// </summary>
        public readonly bool HasValidAssetGuid => Guid.TryParse(AssetGuid, out _);

        public static bool operator ==(Reference<T> x, Reference y)
            => x.AssetGuid == y.AssetGuid && x.instanceId == y.InstanceId;

        public static bool operator ==(Reference<T> x, Reference<T> y)
            => x.AssetGuid == y.AssetGuid && x.instanceId == y.InstanceId;

        public static bool operator !=(Reference<T> x, Reference y)
            => x.AssetGuid != y.AssetGuid || x.instanceId != y.InstanceId;

        public static bool operator !=(Reference<T> x, Reference<T> y)
            => x.AssetGuid != y.AssetGuid || x.instanceId != y.InstanceId;

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
            => string.IsNullOrWhiteSpace(AssetGuid) ? default : new Reference(AssetGuid, InstanceId);

        /// <summary>
        /// Converts scene reference to generic version. (this is a point to submit)
        /// </summary>
        /// <typeparam name="TRefence">Generic type definition to interpret asset as.</typeparam>
        /// <returns> Generic version of reference. </returns>
        public readonly Reference<TRefence> ToReference<TRefence>() where TRefence : UnityEngine.Object
            => string.IsNullOrWhiteSpace(AssetGuid) ? default : new Reference<TRefence>(AssetGuid, InstanceId);

        public readonly override string ToString()
            => AssetGuid;

        /// <summary>
        /// Is reference valid. Checking reference consistency but not checking integrity.
        /// </summary>
        /// <returns> True if reference has valid data. </returns>
        public bool IsValid()
            => !string.IsNullOrEmpty(assetGuid) && Guid.TryParse(assetGuid, out _);
    }
}
