using UnityEngine;

namespace References.UnityResources
{
    internal static class UnityResourcesAssetProviderRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Register()
        {
            Debug.Log($"Registering {nameof(UnityResourcesAssetProvider)}");
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