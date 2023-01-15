using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace References
{
    public static class AssetSystem
    {
        private static bool isInitializedAndNotDisposed;

        private static readonly List<Type> AssetProviderTypes = new();
        private static IAssetProvider[] AssetProviders;

        public static void Initialize()
        {
            Assert.IsFalse(isInitializedAndNotDisposed, "Already initialized!");

            AssetProviders = AssetProviderTypes
                             .Select(Activator.CreateInstance)
                             .Cast<IAssetProvider>()
                             .OrderBy(assetProvider => assetProvider.Priority)
                             .ToArray();
            
            isInitializedAndNotDisposed = true;
        }

        public static void Dispose()
        {
            foreach (var assetProvider in AssetProviders)
                assetProvider.Dispose();

            AssetProviders = null;
            isInitializedAndNotDisposed = false;
        }

        internal static IAssetProvider GetAssetProvider(in string guid)
        {
            Assert.IsTrue(isInitializedAndNotDisposed, "Assets System is disposed or not initialized yet.");
        
            foreach (var assetProvider in AssetProviders)
                if (assetProvider.CanProvideAsset(guid))
                    return assetProvider;

            return null;
        }

        public static void RegisterAssetProvider<T>() where T : IAssetProvider, new()
        {
            Assert.IsFalse(isInitializedAndNotDisposed, "Assets System IAssetProvider can be only registered when not initialized.");
            
            var assetProviderType = typeof(T);
            if (AssetProviderTypes.Contains(assetProviderType))
                throw new Exception();
            
            AssetProviderTypes.Add(assetProviderType);
        }
    }
}