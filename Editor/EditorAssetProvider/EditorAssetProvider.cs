using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace References.EditorAssetProvider
{
#if UNITASK
#else
    using Tasks = System.Threading.Tasks;
#endif

    [Preserve]
    internal sealed class EditorAssetProvider : IAssetProvider
    {
        private readonly Dictionary<UnityEngine.Object, ulong> objectCounters = new();
        private bool isDisposed;
        private uint loads = 0;
        private uint releases = 0;

        public byte Priority => 255; // editor assets 

        public void Dispose()
        {
            ThrowIfDisposed();

            string color;
            if (objectCounters == null || objectCounters.Count == 0)
                color = ColorUtility.ToHtmlStringRGBA(Color.green);
            else if (objectCounters.All(kv => kv.Value == 0))
                color = ColorUtility.ToHtmlStringRGBA(Color.yellow);
            else 
                color = ColorUtility.ToHtmlStringRGBA(Color.red);
            
            var report = new StringBuilder($"<color=#{color}>Editor Asset usage Report</color> ({loads.ToString()} / {releases.ToString()}):\n");

            if (objectCounters == null || objectCounters.Count == 0)
            {
                report.AppendLine($"<color=#{color}>All clear!</color>");
            }
            else if (objectCounters.All(kv => kv.Value == 0))
            {
                report.AppendLine($"<color=#{color}>No references, but objects not cleared:</color>");
                foreach (var obj in objectCounters.Keys)
                    report.AppendLine($"    {obj.name}{obj}");
            }
            else
            {
                report.AppendLine($"<color=#{color}>Some objects not cleared:</color>");
                foreach (var kv in objectCounters)
                    report.AppendLine($"    {kv.Key.name}(View: {kv.Key}): {kv.Value.ToString()} references not cleared.");
            }

            Debug.Log(report.ToString());
            objectCounters?.Clear();
            Resources.UnloadUnusedAssets();
            isDisposed = true;
        }

        public bool CanProvideAsset(in string guid, in string subAsset = null)
        {
            ThrowIfDisposed();

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
                return false;

            if (string.IsNullOrEmpty(subAsset))
                return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null;

            var subAssetName = subAsset;
            return AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath).Any(asset => asset.name == subAssetName);
        }

        public async
#if UNITASK
            UniTask<Scene>
#else
            Tasks.Task<Scene>
#endif
            LoadSceneAsync(string guid, LoadSceneMode loadSceneMode = LoadSceneMode.Single, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
                throw new Exception($"Can't find asset with guid {guid} in project.");

            var op = EditorSceneManager.LoadSceneAsyncInPlayMode(assetPath, new LoadSceneParameters(loadSceneMode));
            await op.ToUniTask(progress, cancellationToken: cancellationToken);
            return SceneManager.GetSceneByPath(assetPath);
        }

        public async
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            LoadAsync<T>(string guid, string subAsset, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            ThrowIfDisposed();

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
                throw new Exception($"Can't find asset with guid {guid} in project.");

            T result;
            if (string.IsNullOrEmpty(subAsset))
            {
                result = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (result == null)
                    throw new Exception($"Failed to load asset of type {typeof(T).FullName} with guid {guid} at path {assetPath}");
            }
            else
            {
                result = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath)
                                      .OfType<T>()
                                      .FirstOrDefault(asset => asset.name == subAsset);
                if (result == null)
                    throw new Exception($"Failed to load asset of type {typeof(T).FullName} with guid {guid}[{subAsset}] at path {assetPath}");
            }

            objectCounters.TryGetValue(result, out var count);
            objectCounters[result] = count + 1;
            loads++;
            return result;
        }

        public async
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            InstantiateAsync<T>(string guid, string subAsset, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            ThrowIfDisposed();

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

        public async
#if UNITASK
            UniTask<GameObject>
#else
            Tasks.Task<GameObject>
#endif
            InstantiateAsync(string guid, string subAsset, Transform parent = null, bool worldPositionStays = false, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

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

        public void Release(UnityEngine.Object obj)
        {
            ThrowIfDisposed();

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

        public async
#if UNITASK
            UniTask<T>
#else
            Tasks.Task<T>
#endif
            InstantiateAsync<T>(string guid, string subAsset, Transform parent = null, bool worldPositionStays = false, IProgress<float> progress = null, CancellationToken cancellationToken = default) where T : Component
        {
            ThrowIfDisposed();

            return await InstantiateComponentAsync(typeof(T), guid, subAsset, parent, worldPositionStays, progress, cancellationToken) as T;
        }

        private void ThrowIfDisposed()
        {
            if (!isDisposed)
                return;

            throw new ObjectDisposedException($"{this} has been already disposed");
        }

        private void ReleaseInternal(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                Debug.LogError("Releasing \"null\" object. Potentially it was released previously.");
                return;
            }

            if (!objectCounters.TryGetValue(obj, out var counter))
            {
                Debug.LogWarning($"Releasing object {obj} with ref count is zero - it's possible leak or object disposition after cleaning context!");
                return;
            }

            --counter;
            releases++;
            if (counter > 0)
            {
                objectCounters[obj] = counter;
                return;
            }

            objectCounters.Remove(obj);
            switch (obj)
            {
                case GameObject:
                case Component:
                case AssetBundle:
                    Resources.UnloadUnusedAssets();
                    break;
                default:
                    Resources.UnloadAsset(obj);
                    break;
            }
        }

        private async
#if UNITASK
            UniTask<UnityEngine.Object>
#else
            Tasks.Task<UnityEngine.Object>
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
    }
}