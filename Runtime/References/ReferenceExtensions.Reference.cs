using System;
using System.Threading;
using UnityEngine.Assertions;

namespace References
{
#if UNITASK
    using Tasks = Cysharp.Threading.Tasks;
#else
    using Tasks = System.Threading.Tasks;
#endif
    
    public static partial class ReferenceExtensions
    {
        public static 
#if UNITASK
            Tasks.UniTask<UnityEngine.Object>
#else
            Tasks.Task<UnityEngine.Object>
#endif
            LoadAsync(
            this in Reference reference,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (!reference.IsValid())
                throw new Exception("Reference is not valid!");

            if (CheckDirectReference(reference, out var result))
#if UNITASK
                return Tasks.UniTask.FromResult(result);
#else
                return Tasks.Task.FromResult(result);
#endif

            var assetProvider = AssetSystem.GetAssetProvider(reference.AssetGuid);
            Assert.IsNotNull(assetProvider, "No supported asset provider");
            return assetProvider.LoadAsync<UnityEngine.Object>(reference.AssetGuid, reference.SubAssetName, progress, cancellationToken);
        }

        public static void Release(
            this in Reference reference,
            UnityEngine.Object obj)
        {
            if (!reference.IsValid())
                throw new Exception("Reference is not valid!");

            if (CheckDirectReference(reference, out var result))
            {
                Assert.AreEqual(result, obj);
                return;
            }
            
            var assetProvider = AssetSystem.GetAssetProvider(reference.AssetGuid);
            Assert.IsNotNull(assetProvider, "No supported asset provider");
            assetProvider.Release(obj);
        }
    }
}