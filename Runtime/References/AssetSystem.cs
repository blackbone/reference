using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace References
{
    public static class AssetSystem
    {
        private static bool _isInitializedAndNotDisposed;

        private static readonly List<Type> AssetProviderTypes = new();
        private static IAssetProvider[] _assetProviders;

        public static void Initialize()
        {
            Assert.IsFalse(_isInitializedAndNotDisposed, "Already initialized!");
            
            _assetProviders = AssetProviderTypes
                              .Select(Activator.CreateInstance)
                              .Cast<IAssetProvider>()
                              .OrderByDescending(assetProvider => assetProvider.Priority)
                              .ToArray();

            _isInitializedAndNotDisposed = true;
        }

        public static void Dispose()
        {
            foreach (var assetProvider in _assetProviders)
                assetProvider.Dispose();

            _assetProviders = null;
            _isInitializedAndNotDisposed = false;
        }

        internal static IAssetProvider GetAssetProvider(in string guid)
        {
            Assert.IsTrue(_isInitializedAndNotDisposed, "Assets System is disposed or not initialized yet.");

            foreach (var assetProvider in _assetProviders)
                if (assetProvider.CanProvideAsset(guid))
                    return assetProvider;

            return null;
        }

        public static void RegisterAssetProvider<T>() where T : IAssetProvider, new()
        {
            Assert.IsFalse(_isInitializedAndNotDisposed, "Assets System IAssetProvider can be only registered when not initialized.");

            var assetProviderType = typeof(T);
            if (AssetProviderTypes.Contains(assetProviderType))
            {
                Debug.LogError($"Asset Provider type {assetProviderType.FullName} already registered.");
                return;
            }

            AssetProviderTypes.Add(assetProviderType);
        }

        public static void UnregisterAssetProvider<T>()
        {
            var assetProviderType = typeof(T);
            if (!AssetProviderTypes.Contains(assetProviderType))
            {
                Debug.LogError($"Asset Provider type {assetProviderType.FullName} is not registered.");
                return;
            }

            AssetProviderTypes.Remove(assetProviderType);
        }
    }
}