using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public static class AssetDatabaseUtility
    {
        public static bool TryGetAssetGuid(Object asset, out string assetGuid)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                assetGuid = string.Empty;
                return false;
            }

            assetGuid = AssetDatabase.AssetPathToGUID(path);
            return !string.IsNullOrEmpty(assetGuid);
        }

        public static bool Exists(Object asset) => AssetDatabase.IsMainAsset(asset) ||
                                                   AssetDatabase.IsNativeAsset(asset) ||
                                                   AssetDatabase.IsSubAsset(asset);

        public static bool TryGetAsset<T>(string assetGuid, out T asset) where T : Object
        {
            if (string.IsNullOrEmpty(assetGuid))
            {
                asset = null;
                return false;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrEmpty(assetPath))
            {
                asset = null;
                return false;
            }

            asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return asset != null;
        }
    }
}
