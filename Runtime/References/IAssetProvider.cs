using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace References
{
#if UNITASK
    using Cysharp.Threading.Tasks;
#else
    using System.Threading.Tasks;
#endif

    
    public interface IAssetProvider : IDisposable
    {
        public byte Priority { get; }
        public bool CanProvideAsset(in string guid, in string subAsset = null);

        public        
#if UNITASK
        UniTask<Scene>
#else
        Task<Scene>
#endif
        LoadSceneAsync(
            string guid,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single, IProgress<float> progress = null,
            CancellationToken cancellationToken = default);

        void Release(UnityEngine.Object guid);
        
#if UNITASK
        UniTask<T>
#else
        Task<T>
#endif
        LoadAsync<T>(
            string guid,
            string subAsset,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object;
        
#if UNITASK
        UniTask<T>
#else
        Task<T>
#endif
        InstantiateAsync<T>(
            string guid,
            string subAsset,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object;
        
#if UNITASK
        UniTask<GameObject>
#else
        Task<GameObject>
#endif
            InstantiateAsync(
                string guid,
                string subAsset,
                Transform parent = null,
                bool worldPositionStays = false,
                IProgress<float> progress = null,
                CancellationToken cancellationToken = default);
        
#if UNITASK
        UniTask<T>
#else
        Task<T>
#endif
            InstantiateAsync<T>(
                string guid,
                string subAsset,
                Transform parent = null,
                bool worldPositionStays = false,
                IProgress<float> progress = null,
                CancellationToken cancellationToken = default)
                where T : Component;
    }
}