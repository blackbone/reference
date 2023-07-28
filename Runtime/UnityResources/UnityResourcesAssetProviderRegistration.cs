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
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playModeStateChange)
            {
                if (playModeStateChange != UnityEditor.PlayModeStateChange.ExitingPlayMode) return;
                OnApplicationQuit();
            }
#endif
        }

        private static void OnApplicationQuit()
        {
            Debug.Log($"Unregistering {nameof(UnityResourcesAssetProvider)}");
            Application.quitting -= OnApplicationQuit;
            AssetSystem.UnregisterAssetProvider<UnityResourcesAssetProvider>();
        }
    }
}