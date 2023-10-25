using UnityEngine;
using UnityEngine.AddressableAssets;

namespace References.Addressables
{
    public static class AssetReferenceExtensions
    {
        public static Reference AsReference(this AssetReference assetReference)
            => new(assetReference.AssetGUID, assetReference.SubObjectName);
        public static Reference<T> AsReference<T>(this AssetReferenceT<T> assetReference) where T : Object
            => new(assetReference.AssetGUID, assetReference.SubObjectName);
        public static Reference<GameObject> AsReference(this AssetReferenceGameObject assetReference)
            => new(assetReference.AssetGUID, assetReference.SubObjectName);
        public static Reference<Sprite> AsReference(this AssetReferenceSprite assetReference)
            => new(assetReference.AssetGUID, assetReference.SubObjectName);
        public static Reference<Texture> AsReference(this AssetReferenceTexture assetReference)
            => new(assetReference.AssetGUID, assetReference.SubObjectName);
        public static Reference<Sprite> AsReference(this AssetReferenceAtlasedSprite assetReference)
            => new(assetReference.AssetGUID, assetReference.SubObjectName);
        public static Reference<Texture2D> AsReference(this AssetReferenceTexture2D assetReference)
            => new(assetReference.AssetGUID, assetReference.SubObjectName);
        public static Reference<Texture3D> AsReference(this AssetReferenceTexture3D assetReference)
            => new(assetReference.AssetGUID, assetReference.SubObjectName);
        public static ReferenceScene AsSceneReference(this AssetReference assetReference)
            => new(assetReference.AssetGUID);
    }
}