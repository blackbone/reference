using System.Collections.Generic;

namespace References
{
    internal static class AssetService
    {
        private static readonly List<IAssetProvider> AssetProviders = new();

        internal static IAssetProvider GetAssetProvider(in string guid)
        {
            foreach (var assetProvider in AssetProviders)
                if (assetProvider.CanProvideAsset(guid))
                    return assetProvider;

            return null;
        }

        public static bool RegisterAssetProvider(in IAssetProvider assetProvider)
        {
            if (AssetProviders.Contains(assetProvider))
                return false;

            for (var i = 0; i < AssetProviders.Count; i++)
            {
                if (assetProvider.Priority <= AssetProviders[0].Priority)
                    continue;
                
                AssetProviders.Insert(i, assetProvider);
                return true;
            }
            
            AssetProviders.Add(assetProvider);
            return true;
        }
    }
}