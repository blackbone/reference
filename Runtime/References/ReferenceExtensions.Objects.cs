using System;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace References
{
#if UNITASK
    using Tasks = Cysharp.Threading.Tasks;
    using TaskGameObject = Cysharp.Threading.Tasks.UniTask<GameObject>;
#else
    using Tasks = System.Threading.Tasks;
    using TaskGameObject = System.Threading.Tasks.Task<GameObject>;
#endif

    public static partial class ReferenceExtensions
    {
        public static
#if UNITASK
            Tasks.UniTask<T>
#else
            Tasks.Task<T>
#endif
            InstantiateAsync<T>(this in Reference<T> reference, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (!reference.IsValid())
                throw new Exception("Reference is not valid!");

            if (CheckDirectReference(reference, out var result))
            {
                var instance = UnityEngine.Object.Instantiate(result);
#if UNITASK
                return new Tasks.UniTask<T>(instance);
#else
                return Tasks.Task.FromResult(instance);
#endif
            }


            var assetProvider = AssetSystem.GetAssetProvider(reference.AssetGuid);
            Assert.IsNotNull(assetProvider, "No supported asset provider");
            return assetProvider.InstantiateAsync<T>(reference.AssetGuid, reference.SubAssetName, progress, cancellationToken);
        }
    }
}