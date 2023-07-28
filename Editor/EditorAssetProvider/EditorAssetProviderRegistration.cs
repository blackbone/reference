using UnityEditor;
using UnityEngine;

namespace References.EditorAssetProvider
{
    internal static class EditorAssetProviderRegistration
    {
        private const string PrefsKey = nameof(EditorAssetProviderRegistration) + "Disabled";

        private static bool IsDisabled
        {
            get => EditorPrefs.GetBool(PrefsKey, false);
            set => EditorPrefs.SetBool(PrefsKey, value);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Register()
        {
            if (IsDisabled) // isDisabled
                return;
            
            Debug.Log($"Registering {nameof(EditorAssetProvider)}");
            AssetSystem.RegisterAssetProvider<EditorAssetProvider>();
            Application.quitting += OnApplicationQuit;
            
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
            {
                if (playModeStateChange != PlayModeStateChange.ExitingPlayMode) return;
                OnApplicationQuit();
            }
#endif
        }

        private static void OnApplicationQuit()
        {
            if (IsDisabled) // isDisabled
                return;
            
            Debug.Log($"Unregistering {nameof(EditorAssetProvider)}");
            Application.quitting -= OnApplicationQuit;
            AssetSystem.UnregisterAssetProvider<EditorAssetProvider>();
        }

        [MenuItem("Tools/Asset Providers/Editor/Enable")]
        private static void Enable() => IsDisabled = false;
        
        [MenuItem("Tools/Asset Providers/Editor/Enable", true)]
        private static bool EnableValidation() => IsDisabled;
        
        [MenuItem("Tools/Asset Providers/Editor/Disable")]
        private static void Disable() => IsDisabled = true;
        
        [MenuItem("Tools/Asset Providers/Editor/Disable", true)]
        private static bool DisableValidation() => !IsDisabled;
    }
}