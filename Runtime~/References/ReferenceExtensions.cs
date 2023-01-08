namespace References
{
    using System;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.SceneManagement;

#if UNITASK
    using Cysharp.Threading.Tasks;
    using Task = Cysharp.Threading.Tasks.UniTask;
    using TaskScene = Cysharp.Threading.Tasks.UniTask<UnityEngine.SceneManagement.Scene>;
    using TaskGameObject = Cysharp.Threading.Tasks.UniTask<UnityEngine.GameObject>;
#else
    using Tasks = System.Threading.Tasks;
    using Task = System.Threading.Tasks.Task;
    using TaskScene = System.Threading.Tasks.Task<UnityEngine.SceneManagement.Scene>;
    using TaskGameObject = System.Threading.Tasks.Task<UnityEngine.GameObject>;
#endif

    public static class ReferenceExtensions
    {
        public static
#if UNITASK
            UniTask<UnityEngine.Object>
#else
            Tasks.Task<UnityEngine.Object>
#endif
            LoadAsync(this in Reference reference, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (CheckDirectReference(reference, out var result))
#if UNITASK
                return new UniTask<UnityEngine.Object>(result);
#else
                return Task.FromResult(result);
#endif

            var assetProvider = AssetService.GetAssetProvider(reference);
            Assert.IsNotNull(assetProvider, "No supported asset provider");
            return assetProvider.LoadAsync<UnityEngine.Object>(reference, progress, cancellationToken);
        }
        
        public static
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            LoadAsync<T>(this in Reference<T> reference, IProgress<float> progress = null, CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            if (CheckDirectReference(reference, out var result))
#if UNITASK
                return new UniTask<T>(result);
#else
                return Task.FromResult(result);
#endif
            
            var assetProvider = AssetService.GetAssetProvider(reference);
            Assert.IsNotNull(assetProvider, "No supported asset provider");
            return assetProvider.LoadAsync<T>(reference, progress, cancellationToken);
        }

        public static TaskGameObject InstantiateAsync(this in Reference<GameObject> reference, Transform parent = null, bool worldPositionStays = true, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (CheckDirectReference(reference, out var result))
            {
                var instance = UnityEngine.Object.Instantiate(result, parent);
#if UNITASK
                return new TaskGameObject(instance);
#else
                return Task.FromResult(instance);
#endif
            }
            
            var assetProvider = AssetService.GetAssetProvider(reference);
            Assert.IsNotNull(assetProvider, "No supported asset provider");
            return assetProvider.InstantiateAsync(reference, parent, worldPositionStays, progress, cancellationToken);
        }

        public static
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            InstantiateAsync<T>(this in Reference<T> reference, Transform parent = null, bool worldPositionStays = true, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : Component
        {
            if (CheckDirectReference(reference, out var result))
            {
                var instance = UnityEngine.Object.Instantiate(result, parent, worldPositionStays);
#if UNITASK
                return new UniTask<T>(instance);
#else
                return Task.FromResult(instance);
#endif
            }
            
            
            var assetProvider = AssetService.GetAssetProvider(reference);
            Assert.IsNotNull(assetProvider, "No supported asset provider");
            return assetProvider.InstantiateAsync<T>(reference, parent, worldPositionStays, progress, cancellationToken);
        }

        public static TaskScene LoadSceneAsync(this in ReferenceScene reference,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single, IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            var assetProvider = AssetService.GetAssetProvider(reference);
            Assert.IsNotNull(assetProvider, "No active asset service");
            return assetProvider.LoadSceneAsync(reference, loadSceneMode, progress, cancellationToken);
        }

        private static bool CheckDirectReference(Reference reference, out UnityEngine.Object result)
        {
            result = reference.Asset;
            return result != null;
        }

        private static bool CheckDirectReference<T>(Reference<T> reference, out T result) where T : UnityEngine.Object
        {
            if (reference.Asset == null)
            {
                result = null;
                return false;
            }
            
            var assetType = reference.Asset.GetType();
            var requiredType      = typeof(T);

            if (typeof(Component).IsAssignableFrom(requiredType))
            {
                switch (reference.Asset)
                {
                    case GameObject gameObject:
                        result = gameObject.GetComponent<T>();
                        return true;
                    case Component:
                        result = reference.Asset;
                        return true;
                    default:
                        result = null;
                        return false;
                }
            }

            if (requiredType.IsAssignableFrom(assetType))
            {
                result = reference.Asset;
                return true;
            }

            result = null;
            return false;
        }
    }
}
