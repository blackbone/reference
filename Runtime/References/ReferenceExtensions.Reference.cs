using System;
using System.Threading;
using NUnit.Framework;

namespace References
{
#if UNITASK
    using TaskObject = Cysharp.Threading.Tasks.UniTask<UnityEngine.Object>;
#else
    using TaskObject = System.Threading.Tasks.Task<UnityEngine.Object>;
#endif
    
    public static partial class ReferenceExtensions
    {
        public static TaskObject LoadAsync(
            this in Reference reference,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (!reference.IsValid())
                throw new Exception("Reference is not valid!");

            if (CheckDirectReference(reference, out var result))
#if UNITASK
                return new TaskObject(result);
#else
                return Task.FromResult(result);
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