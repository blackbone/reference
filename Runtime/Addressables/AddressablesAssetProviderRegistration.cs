using UnityEngine;

namespace References.Addressables
{
    internal static class AddressablesAssetProviderRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Register()
        {
            Debug.Log($"Registering {nameof(AddressablesAssetProvider)}");
            AssetSystem.RegisterAssetProvider<AddressablesAssetProvider>();
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            Application.quitting -= OnApplicationQuit;
            AssetSystem.UnregisterAssetProvider<AddressablesAssetProvider>();
        }
    }
}