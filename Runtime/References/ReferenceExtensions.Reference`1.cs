using System;
using System.Threading;
using NUnit.Framework;

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
            Tasks.UniTask<T>
#else
            Tasks.Task<T>
#endif
            LoadAsync<T>(this in Reference<T> reference, IProgress<float> progress = null, CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            if (CheckDirectReference(reference, out var result))
#if UNITASK
                return new Tasks.UniTask<T>(result);
#else
                return Task.FromResult(result);
#endif
            
            var assetProvider = AssetSystem.GetAssetProvider(reference.AssetGuid);
            Assert.IsNotNull(assetProvider, "No supported asset provider");
            return assetProvider.LoadAsync<T>(reference.AssetGuid, reference.SubAssetName, progress, cancellationToken);
        }
        
        public static void Release<T>(
            this in Reference<T> reference,
            T obj) where T : UnityEngine.Object
        {
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