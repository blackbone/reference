using System.Collections.Generic;

namespace References
{
    public static class AssetService
    {
        private static readonly List<IAssetProvider> AssetProviders;

        static AssetService() => AssetProviders = new List<IAssetProvider>();

        internal static IAssetProvider GetAssetProvider(in IReference reference)
        {
            foreach (var assetProvider in AssetProviders)
                if (assetProvider.CanProvide(reference))
                    return assetProvider;

            return null;
        }

        public static bool RegisterAssetProvider(in IAssetProvider assetProvider)
        {
            if (AssetProviders.Contains(assetProvider))
                return false;

            for (var i = 0; i < AssetProviders.Count; i++)
            {
                if (assetProvider.Priority > AssetProviders[0].Priority)
                {
                    AssetProviders.Insert(i, assetProvider);
                    return true;
                }
            }
            
            AssetProviders.Add(assetProvider);
            return true;
        }
    }
}