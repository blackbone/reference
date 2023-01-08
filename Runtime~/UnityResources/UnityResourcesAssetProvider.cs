using System.IO;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine.Scripting;

namespace References.UnityResources
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.SceneManagement;

#if UNITASK
    using Cysharp.Threading.Tasks;
    using Task = Cysharp.Threading.Tasks.UniTask;
    using TaskScene = Cysharp.Threading.Tasks.UniTask<UnityEngine.SceneManagement.Scene>;
    using TaskGameObject = Cysharp.Threading.Tasks.UniTask<UnityEngine.GameObject>;
#else
    using System.Runtime.CompilerServices;
    using Tasks = System.Threading.Tasks;
    using Task = System.Threading.Tasks.Task;
    using TaskScene = System.Threading.Tasks.Task<UnityEngine.SceneManagement.Scene>;
    using TaskGameObject = System.Threading.Tasks.Task<UnityEngine.GameObject>;
#endif
    
    [Preserve]
    internal sealed class UnityResourcesAssetProvider : IAssetProvider
    {
        internal const string ResourceMapPath = "Assets/Resources/resource_map.bytes";

        public int Priority => 1000;

        private readonly Dictionary<int, ulong> assetCounters = new();
        private readonly Dictionary<string, ulong> counters = new();
        private readonly Dictionary<string, string> knownResourceMap = new();
        private readonly Dictionary<int, string> knownAssetToResourceMap = new();

        public bool CanProvide(in IReference reference)
        {
            if (!knownResourceMap.TryGetValue(reference.AssetGuid, out var resourcePath))
                return false;

            if (reference is ReferenceScene)
                return SceneManager.GetSceneByName(resourcePath).IsValid();
            
            return Resources.InstanceIDToObject(reference.InstanceId) != null;
        }
        
        public async UniTask<Scene> LoadSceneAsync(ReferenceScene reference, LoadSceneMode loadSceneMode = LoadSceneMode.Single, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (!knownResourceMap.TryGetValue(reference.AssetGuid, out var resourcePath))
                throw new InvalidDataException();

            await SceneManager.LoadSceneAsync(resourcePath, loadSceneMode).ToUniTask(progress, cancellationToken: cancellationToken);
            return SceneManager.GetSceneByName(resourcePath);
        }

        public async UniTask Initialize(IProgress<float> progress, CancellationToken cancellationToken = default)
        {
            var asset = await Resources.LoadAsync<TextAsset>(ResourceMapPath).ToUniTask(cancellationToken: cancellationToken) as TextAsset;
            if (asset == null) throw new Exception("Can't load asset");

            using var ms = new MemoryStream(asset.bytes);
            using var br = new BinaryReader(ms, Encoding.UTF8);

            while (ms.CanRead)
            {
                var guid = br.ReadString();
                var instanceId = br.ReadInt32();
                var resourcePath = br.ReadString();

                knownResourceMap[guid] = resourcePath;
                knownAssetToResourceMap[instanceId] = resourcePath;
            }
        }

        private void Release(UnityEngine.Object obj)
        {
            if (!assetCounters.TryGetValue(obj, out var count))
            {
                Debug.LogError("this object is not tracked so can't be unloaded.");
                return;
            }

            assetCounters[obj] = count - 1;
            if (assetCounters[obj] != 0)
                return;

            objectToResourcePath.Remove(obj, out var key);
            resourcePathToObject.Remove(key);
            
            Resources.UnloadAsset(obj);
            counters.Remove(obj);
        }

        public void Release(params UnityEngine.Object[] objs)
        {
            foreach (var obj in objs)
                Release(obj);
        }

        public async UniTask<T> LoadAsync<T>(IReference reference, IProgress<float> progress = null, CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            Assert.IsNotNull(knownResourceMap, $"{GetType().Name} is not initialized yet.");
            
            var type = typeof(T);
            Type loadType = null;
            
            if (typeof(Component).IsAssignableFrom(type))
                loadType = typeof(GameObject);

            if (!knownAssetToResourceMap.TryGetValue(reference.InstanceId, out var resourcePath))
                throw new InvalidDataException($"Can't find mapping for {reference.AssetGuid}[{reference.InstanceId}");

            var op = Resources.LoadAsync(resourcePath, loadType ?? type);
            await op.ToUniTask(progress: progress, cancellationToken: cancellationToken);
            if (op.asset == null)
            {
                Debug.LogError($"Unable to load asset of type ({(loadType ?? type).Name}) from location {reference}");
                return default;
            }

            // asset loaded at this point, so need to add it to counters
            counters.TryGetValue(op.asset, out var refCount);
            counters[op.asset] = refCount + 1;
            resourcePathToObject[resourcePath] = op.asset;
            objectToResourcePath[op.asset] = resourcePath;
            
            T result;
            if (loadType == typeof(GameObject))
            {
                var go = op.asset as GameObject;
                if (go == null)
                {
                    Debug.LogError($"Unable to load asset of type (GameObject) from location {reference}");
                    Release(op.asset);
                    return default;
                }

                result = go.GetComponent<T>();
            }
            else
            {
                result = op.asset as T;
            }

            if (result != null)
                return result;
            
            Debug.LogError($"Unable to load asset of type (GameObject) from location {reference}");
            Release(op.asset);
            return default;
        }

        public async UniTask<IList<T>> LoadAllAsync<T>(IReference reference, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var type = typeof(T);
            Type loadType = null;
            
            if (typeof(Component).IsAssignableFrom(type))
                loadType = typeof(GameObject);

            if (!knownResourceMap.TryGetValue(reference.AssetGuid, out var resourcePath))
                throw new InvalidDataException($"Can't find mapping for {reference.AssetGuid}[{reference.InstanceId}");
            
            // TODO [Dmitrii Osipov] 
            var op = Resources.LoadAsync(resourcePath, loadType ?? type);
            await op.ToUniTask(progress: progress, cancellationToken: cancellationToken);
            if (op.asset == null)
            {
                Debug.LogError($"Unable to load asset of type ({(loadType ?? type).Name}) from location {reference}");
                return default;
            }
            
            // TODO [Dmitrii Osipov] think how to manage multiple objects in same location
            throw new NotImplementedException();
        }

        public async UniTask<T> InstantiateAsync<T>(IReference reference, Transform parent = null, bool worldPositionStays = true, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : Component
        {
            var component = await LoadAsync<T>(reference, progress, cancellationToken);

            var instance = UnityEngine.Object.Instantiate(component, parent, worldPositionStays);
            var instantiatedResource = instance.gameObject.AddComponent<InstantiatedResource>();
            instantiatedResource.Original = component.gameObject;
            instantiatedResource.ReleaseCallback = ReleaseInstance;
            
            return instance;
        }

        public async TaskGameObject InstantiateAsync(IReference reference, Transform parent = null, bool worldPositionStays = true, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            var gameObject = await LoadAsync<GameObject>(reference, progress, cancellationToken);

            var instance = UnityEngine.Object.Instantiate(gameObject, parent, worldPositionStays);
            var instantiatedResource = instance.AddComponent<InstantiatedResource>();
            instantiatedResource.Original = gameObject;
            instantiatedResource.ReleaseCallback = ReleaseInstance;

            return instance;
        }

        public void ReleaseInstance<T>(T instance) where T : UnityEngine.Object
        {
            switch (instance)
            {
                case GameObject gameObject:
                    Release(gameObject);
                    return;
                case Component component:
                    Release(component.gameObject);
                    return;
                default:
                    throw new InvalidOperationException("Releasing non game object instances is not allowed.");
            }
        }
    }
}