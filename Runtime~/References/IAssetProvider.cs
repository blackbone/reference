using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace References
{
#if UNITASK
    using Tasks = Cysharp.Threading.Tasks;
    using Task = UniTask;
    using TaskScene = UniTask<Scene>;
    using TaskGameObject = UniTask<GameObject>;

#else
    using Tasks = System.Threading.Tasks;
    using Task = System.Threading.Tasks.Task;
    using TaskScene = System.Threading.Tasks.Task<UnityEngine.SceneManagement.Scene>;
    using TaskGameObject = System.Threading.Tasks.Task<UnityEngine.GameObject>;
#endif
    
    public interface IAssetProvider
    {
        int Priority { get; }
        
        bool CanProvide(in IReference reference);
        
        public Task Initialize(IProgress<float> progress, CancellationToken cancellationToken = default);

        public void Release(params UnityEngine.Object[] objs);

        public TaskScene LoadSceneAsync(ReferenceScene reference, LoadSceneMode loadSceneMode = LoadSceneMode.Single, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        public
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            LoadAsync<T>(IReference reference, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object;

        public

#if UNITASK
            UniTask<IList<T>>
#else
            Tasks.Task<IList<T>>
#endif
            LoadAllAsync<T>(IReference reference, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object;

        public
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            InstantiateAsync<T>(IReference reference, Transform parent = null, bool worldPositionStays = true, IProgress<float> progress = null, CancellationToken cancellationToken = default)
            where T : Component;
        
        public TaskGameObject InstantiateAsync(IReference reference, Transform parent = null, bool worldPositionStays = true, IProgress<float> progress = null, CancellationToken cancellationToken = default);

        public void ReleaseInstance<T>(T instance) where T : UnityEngine.Object;
    }
}