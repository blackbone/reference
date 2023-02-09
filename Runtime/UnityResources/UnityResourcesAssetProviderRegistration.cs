using UnityEngine;

namespace References.UnityResources
{
    public static class UnityResourcesAssetProviderRegistration
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Register()
        {
            AssetSystem.RegisterAssetProvider<UnityResourcesAssetProvider>();
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            Application.quitting -= OnApplicationQuit;
            AssetSystem.UnregisterAssetProvider<UnityResourcesAssetProvider>();
        }
    }
}