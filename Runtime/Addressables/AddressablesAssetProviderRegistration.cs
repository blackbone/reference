using UnityEngine;
using UnityEngine.Scripting;

namespace References.Addressables
{
    [Preserve]
    public static class AddressablesAssetProviderRegistration
    {
        [Preserve]

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void Register()
        {
            Debug.Log($"Registering {nameof(AddressablesAssetProvider)}");
            AssetSystem.RegisterAssetProvider<AddressablesAssetProvider>();
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
            Debug.Log($"Unregistering {nameof(AddressablesAssetProvider)}");
            Application.quitting -= OnApplicationQuit;
            AssetSystem.UnregisterAssetProvider<AddressablesAssetProvider>();
        }
    }
}
