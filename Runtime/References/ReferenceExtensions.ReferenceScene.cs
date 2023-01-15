using System;
using System.Threading;
using NUnit.Framework;
using UnityEngine.SceneManagement;

namespace References
{
#if UNITASK
    using TaskScene = Cysharp.Threading.Tasks.UniTask<Scene>;
#else
    using TaskScene = System.Threading.Tasks.Task<Scene>;
#endif
    
    public static partial class ReferenceExtensions
    {
        public static TaskScene LoadSceneAsync(
            this in ReferenceScene reference,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single, 
            Progress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (!reference.IsValid())
                throw new Exception("Reference is not valid!");

            var assetProvider = AssetSystem.GetAssetProvider(reference.AssetGuid);
            Assert.IsNotNull(assetProvider, "No active asset service");
            return assetProvider.LoadSceneAsync(reference.AssetGuid, loadSceneMode, progress, cancellationToken);
        }

        public static void Release(
            this in ReferenceScene reference,
            in Scene scene)
        {
            if (!reference.IsValid())
                throw new Exception("Reference is not valid!");

            var assetProvider = AssetSystem.GetAssetProvider(reference.AssetGuid);
            Assert.IsNotNull(assetProvider, "No active asset service");
            // return assetProvider.ReleaseScene(scene);
        }
    }
}