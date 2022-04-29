namespace Assets
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
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

    public interface IAssetsService : IDisposable
    {
        public Task Initialize(IProgress<float> progress, CancellationToken cancellationToken = default);
        
        public void Release(params UnityEngine.Object[] objs);

        public TaskScene LoadSceneAsync(string key, LoadSceneMode loadSceneMode = LoadSceneMode.Single, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        public
#if UNITASK
            Tasks.UniTask<T>
#else
            Tasks.Task<T>
#endif
            LoadAsync<T>(string key, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        public
            
#if UNITASK
            Tasks.UniTask<IList<T>>
#else
            Tasks.Task<IList<T>>
#endif
            LoadAllAsync<T>(string key, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        public
#if UNITASK
            Tasks.UniTask<T>
#else
            Tasks.Task<T>
#endif
            InstantiateAsync<T>(string key, Transform parent = null, bool worldPositionStays = true, IProgress<float> progress = null, CancellationToken cancellationToken = default)
            where T : Component;

        public TaskGameObject InstantiateAsync(string key, Transform parent = null, IProgress<float> progress = null, CancellationToken cancellationToken = default);
        
        public void ReleaseInstance<T>(T instance) where T : UnityEngine.Object;
    }
}