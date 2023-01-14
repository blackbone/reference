using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace References
{
    internal static class AssetSystem
    {
        private static bool isInitializedAndNotDisposed;
        
        private static readonly List<IAssetProvider> AssetProviders = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanges;
            
            void OnPlayModeChanges(UnityEditor.PlayModeStateChange playModeStateChange)
            {
                switch (playModeStateChange)
                {
                    case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                        Dispose();
                        break;
                    case UnityEditor.PlayModeStateChange.EnteredEditMode:
                    case UnityEditor.PlayModeStateChange.ExitingEditMode:
                    case UnityEditor.PlayModeStateChange.EnteredPlayMode:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(playModeStateChange), playModeStateChange, null);
                }
            }
#else
            Application.quitting += Dispose;
#endif
            isInitializedAndNotDisposed = true;
        }

        private static void Dispose()
        {
#if UNITY_EDITOR
#else
            Application.quitting -= Dispose;
#endif
            foreach (var assetProvider in AssetProviders)
                assetProvider.Dispose();
            
            AssetProviders.Clear();
            
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

        public static bool RegisterAssetProvider(in IAssetProvider assetProvider)
        {
            Assert.IsTrue(isInitializedAndNotDisposed, "Assets System IAssetProvider can be only registered when initialized.");
            
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