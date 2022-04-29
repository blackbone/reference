namespace Assets
{
    using System;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.SceneManagement;

#if UNITASK
    using Tasks = Cysharp.Threading.Tasks;
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
            Tasks.UniTask<UnityEngine.Object>
#else
            Tasks.Task<UnityEngine.Object>
#endif
            LoadAsync(this in Reference reference, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (CheckDirectReference(reference, out var result))
#if UNITASK
                return new Tasks.UniTask<UnityEngine.Object>(result);
#else
                return Task.FromResult(result);
#endif
            
            Assert.IsNotNull(AssetServiceInternal.Current, "No active asset service");
            return AssetServiceInternal.Current.LoadAsync<UnityEngine.Object>(reference.AssetGuid, progress, cancellationToken);
        }
        
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
            
            Assert.IsNotNull(AssetServiceInternal.Current, "No active asset service");
            return AssetServiceInternal.Current.LoadAsync<T>(reference.AssetGuid, progress, cancellationToken);
        }

        public static TaskGameObject InstantiateAsync(this in Reference<GameObject> reference, Transform parent = null, IProgress<float> progress = null, CancellationToken cancellationToken = default)
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
            
            Assert.IsNotNull(AssetServiceInternal.Current, "No active asset service");
            return AssetServiceInternal.Current.InstantiateAsync(reference.AssetGuid, parent, progress, cancellationToken);
        }

        public static
#if UNITASK
            Tasks.UniTask<T>
#else
            Tasks.Task<T>
#endif
            InstantiateAsync<T>(this in Reference<T> reference, Transform parent = null, bool worldPositionStays = true, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : Component
        {
            if (CheckDirectReference(reference, out var result))
            {
                var instance = UnityEngine.Object.Instantiate(result, parent, worldPositionStays);
#if UNITASK
                return new Tasks.UniTask<T>(instance);
#else
                return Task.FromResult(instance);
#endif
            }
            
            Assert.IsNotNull(AssetServiceInternal.Current, "No active asset service");
            return AssetServiceInternal.Current.InstantiateAsync<T>(reference.AssetGuid, parent, worldPositionStays, progress, cancellationToken);
        }

        public static TaskScene LoadSceneAsync(this in ReferenceScene reference,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single, IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            Assert.IsNotNull(AssetServiceInternal.Current, "No active asset service");
            return AssetServiceInternal.Current.LoadSceneAsync(reference.AssetGuid, loadSceneMode, progress, cancellationToken);
        }

        public static
#if UNITASK
            Tasks.UniTask<T>.Awaiter
#else
            System.Runtime.CompilerServices.TaskAwaiter<T>
#endif
            GetAwaiter<T>(this Reference<T> reference) where T : UnityEngine.Object
            => reference.LoadAsync().GetAwaiter();
        
        public static
#if UNITASK
            Tasks.UniTask<Scene>.Awaiter
#else
            System.Runtime.CompilerServices.TaskAwaiter<Scene>
#endif
            GetAwaiter(this ReferenceScene reference)
            => reference.LoadSceneAsync().GetAwaiter();
        
        public static
#if UNITASK
            Tasks.UniTask<UnityEngine.Object>.Awaiter
#else
            System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.Object>
#endif
            GetAwaiter(this Reference reference)
            => reference.LoadAsync().GetAwaiter();

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
