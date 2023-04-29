#if ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace References.Editor
{
    internal static class AddressableUtility
    {
        private static readonly Type ComponentType = typeof(Component);

        private static IEnumerable<AddressableAssetEntry> AssetEntires => AssetGroups.SelectMany(group => group.entries);

        private static IEnumerable<AddressableAssetGroup> AssetGroups => AssetSettings.groups
                                                                                      .Where(group => group != null && group.HasSchema<BundledAssetGroupSchema>());

        private static AddressableAssetSettings AssetSettings => AddressableAssetSettingsDefaultObject.Settings;

        public static void CreateOrMoveAssetEntry(string assetGuid)
        {
            var assetSettings = AssetSettings;
            assetSettings.CreateOrMoveEntry(assetGuid, assetSettings.DefaultGroup);
        }

        public static IEnumerable<AddressableAssetEntry> GetAssetEntries()
        {
            return AssetEntires;
        }

        public static IEnumerable<AddressableAssetEntry> GetAssetEntries<T>()
            where T : class
        {
            var type = typeof(T);
            foreach (var assetEntry in AssetEntires)
            {
                var validAsset = assetEntry.MainAsset switch
                                 {
                                     T _ => true,
                                     GameObject gameObject => type.IsSubclassOf(ComponentType) && gameObject.TryGetComponent<T>(out _),
                                     _ => false
                                 };

                if (validAsset) yield return assetEntry;
            }
        }

        public static bool IsValidAsset(UnityEngine.Object asset)
        {
            return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset));
        }

        public static bool IsValidAsset<T>(UnityEngine.Object asset)
            where T : class
        {
            var type = typeof(T);
            var result = IsValidAsset(asset);

            switch (asset)
            {
                case GameObject gameObject:
                    result &= type == typeof(GameObject) ||
                              (type.IsSubclassOf(ComponentType) &&
                               gameObject.TryGetComponent<T>(out _));
                    break;
                default:
                    result &= asset is T;
                    break;
            }

            return result;
        }

        public static bool TryGetAsset(string assetGuid, out UnityEngine.Object asset, out bool isImplicitlyAdded)
        {
            if (!TryGetAssetEntry(assetGuid, out var assetEntry, out isImplicitlyAdded))
            {
                asset = default;
                isImplicitlyAdded = false;
                return false;
            }

            asset = assetEntry.MainAsset;
            return true;
        }

        public static bool TryGetAsset<T>(string assetGuid, out T asset)
            where T : class
        {
            if (!TryGetAsset(assetGuid, out var baseAsset, out _))
            {
                asset = default;
                return false;
            }

            switch (baseAsset)
            {
                case T derivedAsset:
                {
                    asset = derivedAsset;

                    return true;
                }
                case GameObject gameObject:
                {
                    if (gameObject.TryGetComponent<T>(out var derivedAsset))
                    {
                        asset = derivedAsset;

                        return true;
                    }

                    break;
                }
            }

            asset = default;

            return false;
        }

        public static bool TryGetAssetEntry(UnityEngine.Object asset, out AddressableAssetEntry assetEntry, out bool isImplicitlyAdded)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long _))
                return TryGetAssetEntry(guid, out assetEntry, out isImplicitlyAdded);

            assetEntry = null;
            isImplicitlyAdded = false;
            return false;
        }

        public static bool TryGetAssetEntry(string assetGuid, out AddressableAssetEntry assetEntry, out bool isImplicitlyAdded)
        {
            if (assetGuid != null)
                foreach (var assetGroup in AssetGroups)
                {
                    assetEntry = assetGroup.GetAssetEntry(assetGuid, true);

                    if (assetEntry == null)
                        continue;

                    var explicitEntry = assetGroup.GetAssetEntry(assetGuid, false);
                    isImplicitlyAdded = explicitEntry == null;
                    return true;
                }

            assetEntry = default;

            isImplicitlyAdded = false;
            return false;
        }

        public static void AddToAddressables(UnityEngine.Object targetObject, AddressableAssetGroup assetGroup, string address)
        {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(targetObject, out var guid, out long _))
                throw new Exception();

            if (assetGroup == null)
                assetGroup = AssetSettings.DefaultGroup;

            var entry = AssetSettings.CreateOrMoveEntry(guid, assetGroup);

            if (string.IsNullOrWhiteSpace(address))
                address = AssetDatabase.GetAssetPath(targetObject);

            entry.SetAddress(address);
        }

        public static void RemoveFromAddressables(UnityEngine.Object targetObject)
        {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(targetObject, out var guid, out long _))
                throw new Exception();

            if (!TryGetAssetEntry(guid, out var entry, out _))
                return;

            AssetSettings.RemoveAssetEntry(entry.guid);
        }
    }
}
#endif