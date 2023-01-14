using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace References.UnityResources
{
    internal sealed class UnityResourcesAssetProvider : IAssetProvider
    {
        internal const string ResourceMapName = "resource_map";

        [RuntimeInitializeOnLoadMethod]
        private static void Register() => AssetSystem.RegisterAssetProvider(new UnityResourcesAssetProvider());

        private readonly Dictionary<string, string> guidToResourcePath = new();
        private readonly Dictionary<string, HashSet<string>> guidToSubAssets = new();
        private readonly Dictionary<string, ulong> counters = new();
        private readonly Dictionary<UnityEngine.Object, string> objectToResource = new();

        public int Priority => 1000;

        private UnityResourcesAssetProvider()
        {
            var resourcesMappingAsset = Resources.Load<TextAsset>(ResourceMapName);
            
            Assert.IsNotNull(resourcesMappingAsset);

            using var ms = new MemoryStream(resourcesMappingAsset.bytes);
            using var br = new BinaryReader(ms, Encoding.UTF8);
            
            while (ms.Position < ms.Length)
            {
                var guid = br.ReadString();
                
                var subAssetsCount = br.ReadInt32();
                if (subAssetsCount > 0)
                {
                    guidToSubAssets[guid] = new HashSet<string>(subAssetsCount);
                    for (var i = 0; i < subAssetsCount; i++)
                        guidToSubAssets[guid].Add(br.ReadString());
                }
                
                guidToResourcePath[guid] = br.ReadString();
                br.ReadString(); // "\r\n" line
            }
        }

        public bool CanProvideAsset(in string guid, in string subAsset = null)
        {
            if (string.IsNullOrWhiteSpace(subAsset))
                return guidToResourcePath.ContainsKey(guid);

            return guidToResourcePath.ContainsKey(guid)
                   && guidToSubAssets.TryGetValue(guid, out var subAssets)
                   && subAssets.Contains(subAsset);
        }

        public async
#if UNITASK
            UniTask<Scene>
#else
            Task<Scene>
#endif
            LoadSceneAsync(string guid, LoadSceneMode loadSceneMode = LoadSceneMode.Single, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (!guidToResourcePath.TryGetValue(guid, out var sceneName))
                throw new InvalidDataException();

            await SceneManager.LoadSceneAsync(sceneName, loadSceneMode).ToUniTask(progress, cancellationToken: cancellationToken);
            return SceneManager.GetSceneByName(sceneName);
        }

        public void Release(UnityEngine.Object obj)
        {
            switch (obj)
            {
                case Component component:
                    ReleaseInternal(component.gameObject);
                    return;
                default:
                    ReleaseInternal(obj);
                    return;
            }
        }

        private void ReleaseInternal(UnityEngine.Object obj)
        {
            if (!objectToResource.TryGetValue(obj, out var guid))
            {
                Debug.LogError($"Can't release object {obj} because it's not linked to any resource.");
                return;
            }

            if (!counters.TryGetValue(guid, out var counter))
            {
                Debug.LogError($"Can't release object {obj} because ref count is zero - it's possible leak!");
                return;
            }
            
            --counter;
            if (counter > 0)
            {
                counters[guid] = counter;
                return;
            }

            counters.Remove(guid);
            Resources.UnloadAsset(obj);
            Resources.UnloadUnusedAssets();
        }
        
        public async
#if UNITASK
            UniTask<T>
#else
            Task<T>
#endif
            LoadAsync<T>(string guid, string subAsset, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var type = typeof(T);
            Type loadType = null;
            
            if (typeof(Component).IsAssignableFrom(type))
                loadType = typeof(GameObject);

            if (!guidToResourcePath.TryGetValue(guid, out var resourcePath))
                throw new InvalidDataException($"Can't find mapping for {guid}");
            
            if (!string.IsNullOrEmpty(subAsset) && !(guidToSubAssets.TryGetValue(guid, out var subAssets) && !subAssets.Contains(subAsset)))
                throw new InvalidDataException($"Can't find mapping for {guid}[{subAsset}]");

            var op = Resources.LoadAsync(resourcePath, loadType ?? type);
            await op.ToUniTask(progress: progress, cancellationToken: cancellationToken);
            if (op.asset == null)
            {
                Debug.LogError($"Unable to load asset of type ({(loadType ?? type).Name}) from location {guid}[{subAsset}]");
                return default;
            }

            T result;
            if (loadType == typeof(GameObject))
            {
                var go = op.asset as GameObject;
                if (go == null)
                {
                    Debug.LogError($"Unable to load asset of type (GameObject) from location {guid}[{subAsset}]");
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
            {
                counters.TryGetValue(guid, out var counter);
                if (counter == 0)
                    objectToResource[result] = guid;
                counters[guid] = counter + 1;
                return result;
            }
            
            Debug.LogError($"Unable to load asset of type (GameObject) from location {guid}[{subAsset}]");
            Release(op.asset);
            return default;
        }

        public async
#if UNITASK
            UniTask<T>
#else
            Task<T>
#endif
            InstantiateAsync<T>(string guid, string subAsset, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (typeof(T) == typeof(GameObject))
                return await InstantiateAsync(guid, subAsset, progress: progress, cancellationToken: cancellationToken) as T;
            
            if (typeof(Component).IsAssignableFrom(typeof(T)))
                return await InstantiateComponentAsync(typeof(T), guid, subAsset, null, false, progress, cancellationToken) as T;
            
            const float loadWeight = .95f;

            IProgress<float> loadProgress = null;
            if (progress != null)
                loadProgress = new Progress<float>(v => progress.Report(loadWeight * v));

            var asset = await LoadAsync<T>(guid, subAsset, loadProgress, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            
            if (asset == null)
            {
                Debug.LogError("FFFUUU!!!");
                Release(asset);
                return default;
            }
            
            var instance = UnityEngine.Object.Instantiate(asset);
            progress?.Report(1f);
            return instance;
        }

        public async UniTask<GameObject> InstantiateAsync(string guid, string subAsset, Transform parent = null, bool worldPositionStays = false, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            const float loadWeight = .9f;
            const float instantiateWeight = 1f - loadWeight;

            IProgress<float> loadProgress = null;
            IProgress<float> instantiateProgress = null;
            if (progress != null)
            {
                loadProgress = new Progress<float>(v => progress.Report(loadWeight * v));
                instantiateProgress = new Progress<float>(v => progress.Report(loadWeight + instantiateWeight * v));
            }

            var asset = await LoadAsync<GameObject>(guid, subAsset, loadProgress, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            
            if (asset == null)
            {
                Debug.LogError("FFFUUU!!!");
                Release(asset);
                return default;
            }
            
            instantiateProgress?.Report(0);
            var instance = UnityEngine.Object.Instantiate(asset);
            instantiateProgress?.Report(.5f);
            var instantiatedResource = instance.AddComponent<InstantiatedResource>();
            instantiatedResource.Original = asset;
            instantiatedResource.ReleaseCallback = ReleaseInternal;
            
            progress?.Report(1f);
            return instance;
        }

        public async
#if UNITASK
            UniTask<T>
#else
            Task<T>
#endif
            InstantiateAsync<T>(string guid, string subAsset, Transform parent = null, bool worldPositionStays = false, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : Component
            => await InstantiateComponentAsync(typeof(T), guid, subAsset, parent, worldPositionStays, progress, cancellationToken) as T;

        private async
#if UNITASK
            UniTask<UnityEngine.Object>
#else
            Task<UnityEngine.Object>
#endif
            InstantiateComponentAsync(Type componentType, string guid, string subAsset, Transform parent = null, bool worldPositionStays = false, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            const float loadWeight = .9f;
            const float instantiateWeight = 1f - loadWeight;

            IProgress<float> loadProgress = null;
            IProgress<float> instantiateProgress = null;
            if (progress != null)
            {
                loadProgress = new Progress<float>(v => progress.Report(loadWeight * v));
                instantiateProgress = new Progress<float>(v => progress.Report(loadWeight + instantiateWeight * v));
            }

            var asset = await LoadAsync<GameObject>(guid, subAsset, loadProgress, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            
            if (asset == null)
            {
                Debug.LogError("FFFUUU!!! Can't load");
                Release(asset);
                return default;
            }
            
            if (asset.GetComponent(componentType) == null)
            {
                Debug.LogError("FFFUUU!!! no component");
                Release(asset);
                return default;
            }
            
            instantiateProgress?.Report(0);
            var instance = UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
            instantiateProgress?.Report(.5f);
            var instantiatedResource = instance.AddComponent<InstantiatedResource>();
            instantiatedResource.Original = asset;
            instantiatedResource.ReleaseCallback = ReleaseInternal;
            
            progress?.Report(1f);
            return instance.GetComponent(componentType);
        }

        public void Dispose()
        {
            foreach (var obj in objectToResource.Keys)
                Release(obj);
                
            this.counters.Clear();
            this.objectToResource.Clear();
            this.guidToSubAssets.Clear();
            this.guidToResourcePath.Clear();
        }
    }
}