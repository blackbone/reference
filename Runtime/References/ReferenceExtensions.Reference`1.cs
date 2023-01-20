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
            return assetProvider.LoadAsync<T>(reference.AssetGuid, reference.SubAssetName, progress, cancellationToken);
        }
        
        public static void Release<T>(
            this in Reference<T> reference,
            T obj) where T : UnityEngine.Object
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