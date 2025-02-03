#if ADDRESSABLES
namespace References.Addressables
{
    using System;
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

    public sealed class AddressablesAssetProvider : IAssetProvider
    {
#if !UNITASK
        private static int _mainThreadId;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
#if !UNITASK
            // capture default(unity) sync-context.
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
#endif
        }

        private CancellationTokenSource initializationCts;
        private bool initialized;
        public AddressablesAssetProvider()
        {
            initializationCts = new CancellationTokenSource();
            Initialize(null, initializationCts.Token)
                .ContinueWith(() =>
                {
                    initialized = true;
                    initializationCts.Dispose();
                    initializationCts = null;
                }).Forget();
        }

        public async Task Initialize(IProgress<float> progress, CancellationToken cancellationToken)
        {
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

        public void Dispose()
        {
            if (initializationCts != null)
            {
                initializationCts.Cancel();
                initializationCts.Dispose();
                initializationCts = null;
            }
            Addressables.ResourceManager.Dispose();
        }

        public byte Priority => 0; // lowest priority

        public bool CanProvideAsset(in string guid, in string subAsset = null)
        {
            return true; // for now, will try check later
        }

        public void Release(UnityEngine.Object obj)
        {
            Addressables.Release(obj);
        }

        public async TaskScene LoadSceneAsync(string guid, LoadSceneMode loadSceneMode = LoadSceneMode.Single, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            Debug.Log($"[Addressables Provider] start loading {guid}({loadSceneMode}) scene...");
            var op = Addressables.LoadSceneAsync(guid, loadSceneMode, false);
            await op.ToUniTask(progress, cancellationToken: cancellationToken);
            Debug.Log($"[Addressables Provider] loadinged {guid}({loadSceneMode}) scene");
            if (op.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Unable to load scene from location {guid}:\r\n{op.OperationException}");
                return default;
            }

            Debug.Log($"[Addressables Provider] start activating {guid}({loadSceneMode}) scene...");
            await op.Result.ActivateAsync().ToUniTask(cancellationToken: cancellationToken);
            Debug.Log($"[Addressables Provider] activated {guid}({loadSceneMode}) scene");
            return op.Result.Scene;
        }

        public async
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            LoadAsync<T>(string guid, string subAsset, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var key = string.IsNullOrEmpty(subAsset) ? guid : $"{guid}[{subAsset}]";
            var type = typeof(T);
            if (typeof(Component).IsAssignableFrom(type))
            {
                var go = await Addressables.LoadAssetAsync<GameObject>(key).ToUniTask(progress, cancellationToken: cancellationToken);
                return go.GetComponent<T>();
            }

            return await Addressables.LoadAssetAsync<T>(key).ToUniTask(progress, cancellationToken: cancellationToken);
        }

        public async
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            InstantiateAsync<T>(string guid, string subAsset, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            throw new NotImplementedException("Implement non game object instantiation");
        }

        public async TaskGameObject InstantiateAsync(string guid, string subAsset, Transform parent = null, bool worldPositionStays = false, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            var key = string.IsNullOrEmpty(subAsset) ? guid : $"{guid}[{subAsset}]";
            var op = Addressables.InstantiateAsync(key, parent);
            await op.ToUniTask(progress, cancellationToken: cancellationToken);
            if (op.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Unable to instantiate asset of type (GameObject) from location {key}:\r\n{op.OperationException}");
                return default;
            }

            return op.Result;
        }

        public async
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            InstantiateAsync<T>(string guid, string subAsset, Transform parent = null, bool worldPositionStays = false, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : Component
        {
            var type = typeof(T);
            var op = Addressables.InstantiateAsync(guid, parent, worldPositionStays);
            await op.ToUniTask(progress, cancellationToken: cancellationToken);
            if (op.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Unable to instantiate asset of type ({type.FullName}) from location {guid}:\r\n{op.OperationException}");
                return default;
            }

            if (typeof(Component).IsAssignableFrom(type)) return op.Result.GetComponent<T>();

            Debug.LogError($"Unable to instantiate asset of type ({type.FullName}) from location {guid}:\r\n{type.FullName} is not a Component or GameObject.");
            return default;
        }

        private static async Task SwitchToMainThread(CancellationToken cancellationToken = default)
        {
#if UNITASK
            await Task.SwitchToMainThread(cancellationToken);
#else
            await new MainThreadAwaitable(cancellationToken);
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
