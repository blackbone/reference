#if ADDRESSABLES
namespace Assets
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.SceneManagement;

#if UNITASK
    using Cysharp.Threading.Tasks;
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
    
    public sealed class AddressablesAssetsService : IAssetsService
    {
#if !UNITASK
        private static int _mainThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            // capture default(unity) sync-context.
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }
#endif
        
        public void Dispose()
        {
            AssetServiceInternal.Current = null;
            Addressables.ResourceManager.Dispose();
        }

        async Task IAssetsService.Initialize(IProgress<float> progress, CancellationToken cancellationToken)
        {
            AssetServiceInternal.Current = this;

            var checkProgress = CreateProgress(v => progress?.Report(v * .1f));

            var catalogUpdates = await Addressables.CheckForCatalogUpdates().ToUniTask(checkProgress, cancellationToken: cancellationToken);

            await SwitchToMainThread(cancellationToken);
            await Yield(cancellationToken);

            var updateProgress = CreateProgress(v => progress?.Report(.1f + v * .9f));

            if (catalogUpdates.Count > 0)
                await Addressables.UpdateCatalogs(true, catalogUpdates)
                    .ToUniTask(updateProgress, cancellationToken: cancellationToken);

            updateProgress.Report(1f);
        }

        async TaskScene IAssetsService.LoadSceneAsync(string key, LoadSceneMode loadSceneMode,
            IProgress<float> progress, CancellationToken cancellationToken)
        {
            var op = Addressables.LoadSceneAsync(key, loadSceneMode, false);
            await op.ToUniTask(progress, cancellationToken: cancellationToken);
            if (op.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Unable to load scene from location {key}:\r\n{op.OperationException}");
                return default;
            }
            
            await op.Result.ActivateAsync().ToUniTask(cancellationToken: cancellationToken);
            return op.Result.Scene;
        }

        async
#if UNITASK
            Tasks.UniTask<T>
#else
            Tasks.Task<T>
#endif
            IAssetsService.LoadAsync<T>(string key, IProgress<float> progress, CancellationToken cancellationToken)
        {
            var type = typeof(T);
            if (typeof(Component).IsAssignableFrom(type))
            {
                var go = await Addressables.LoadAssetAsync<GameObject>(key).ToUniTask(progress, cancellationToken: cancellationToken);
                return go.GetComponent<T>();
            }
            
            return await Addressables.LoadAssetAsync<T>(key).ToUniTask(progress, cancellationToken: cancellationToken);
        }

        async
#if UNITASK
            Tasks.UniTask<IList<T>>
#else
            Tasks.Task<IList<T>>
#endif
            IAssetsService.LoadAllAsync<T>(string key, IProgress<float> progress, CancellationToken cancellationToken)
        {
            var result = new List<T>();
            await Addressables.LoadAssetsAsync(key, (T o) => result.Add(o), true)
                .ToUniTask(progress, cancellationToken: cancellationToken);
            return result;
        }

        void IAssetsService.Release(params UnityEngine.Object[] objs)
        {
            foreach (var o in objs)
                Addressables.Release(o);
        }

        async
#if UNITASK
            Tasks.UniTask<T>
#else
            Tasks.Task<T>
#endif
            IAssetsService.InstantiateAsync<T>(string key, Transform parent, bool worldPositionStays, IProgress<float> progress, CancellationToken cancellationToken)
        {
            var type = typeof(T);
            var op = Addressables.InstantiateAsync(key, parent, worldPositionStays);
            await op.ToUniTask(progress, cancellationToken: cancellationToken);
            if (op.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Unable to instantiate asset of type ({type.FullName}) from location {key}:\r\n{op.OperationException}");
                return default;
            }
            
            if (typeof(Component).IsAssignableFrom(type)) return op.Result.GetComponent<T>();
            
            Debug.LogError($"Unable to instantiate asset of type ({type.FullName}) from location {key}:\r\n{type.FullName} is not a Component or GameObject.");
            return default;
        }

        async TaskGameObject IAssetsService.InstantiateAsync(string key, Transform parent, IProgress<float> progress, CancellationToken cancellationToken)
        {
            var op = Addressables.InstantiateAsync(key, parent);
            await op.ToUniTask(progress, cancellationToken: cancellationToken);
            if (op.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Unable to instantiate asset of type (GameObject) from location {key}:\r\n{op.OperationException}");
                return default;
            }

            return op.Result;
        }

        void IAssetsService.ReleaseInstance<T>(T instance)
        {
            switch (instance)
            {
                case GameObject gameObject:
                    Addressables.ReleaseInstance(gameObject);
                    return;
                case Component component:
                    Addressables.ReleaseInstance(component.gameObject);
                    return;
                default:
                    throw new InvalidOperationException("Releasing non game object instaces is not allowed.");
            }
        }

        private static async Task SwitchToMainThread(CancellationToken cancellationToken = default)
        {
#if UNITASK
            await Task.SwitchToMainThread(cancellationToken);
#else
            await new MainThreadAwaitable(cancellationToken);
#endif
        }
        
        private static async Task Yield(CancellationToken cancellationToken = default)
        {
#if UNITASK
            await Task.Yield(cancellationToken);
#else
            await Task.Yield();
#endif
        }

        private static IProgress<float> CreateProgress(Action<float> report)
        {
#if UNITASK
            return Progress.Create(report);
#else
            return new Progress<float>(report);
#endif
        }
        
#if !UNITASK
        private readonly struct MainThreadAwaitable
        {
            private readonly CancellationToken _cancellationToken;

            public MainThreadAwaitable(CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
            }

            public Awaiter GetAwaiter() => new Awaiter(_cancellationToken);
            
            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                private readonly CancellationToken _cancellationToken;

                public Awaiter( CancellationToken cancellationToken)
                {
                    _cancellationToken = cancellationToken;
                }

                public bool IsCompleted
                {
                    get
                    {
                        var currentThreadId = Thread.CurrentThread.ManagedThreadId;
                        return _mainThreadId == currentThreadId;  // true : run immediate, false : register continuation.
                    }
                }

                public void GetResult() { _cancellationToken.ThrowIfCancellationRequested(); }

                public void OnCompleted(Action continuation)
                    => continuation();

                public void UnsafeOnCompleted(Action continuation)
                    => continuation();
            }
        }
#endif
    }
}

#endif