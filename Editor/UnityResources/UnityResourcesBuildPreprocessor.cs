using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using References.UnityResources;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor.UnityResources
{
    internal sealed class UnityResourcesBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -999;

        public void OnPreprocessBuild(BuildReport report) => GenerateResourceMap();
       
        [MenuItem("Tools/foo")]
        private static void GenerateResourceMap()
        {
            using var fs = new FileStream(UnityResourcesAssetProvider.ResourceMapPath, FileMode.Create);
            using var bw = new BinaryWriter(fs, Encoding.UTF8);

            foreach (var ((guid, iid), resourcePath) in GetGuidToResourcePathMapping())
            {
                bw.Write(guid);
                bw.Write(iid);
                bw.Write(resourcePath);
            }
        }

        private static IReadOnlyDictionary<(string, int), string> GetGuidToResourcePathMapping()
        {
            const string resourcesFolderPattern = "/Resources/";
            
            var allResources = Resources.LoadAll(string.Empty);
            var allPaths = allResources.Select(AssetDatabase.GetAssetPath).ToArray();
            var allProjectPaths = allPaths.Where(path => path.StartsWith("Assets/")).ToArray();
            var allNormalizedPaths = allProjectPaths.Select(path => path[(path.LastIndexOf(resourcesFolderPattern, StringComparison.Ordinal) + resourcesFolderPattern.Length)..])
                                                    .Select(path => path.Substring(0, path.LastIndexOf(".", StringComparison.Ordinal)))
                                                    .Distinct()
                                                    .ToArray();
            var pathToObjects = allNormalizedPaths.Select(path => (path, Resources.LoadAll(path))).ToList();

            foreach (var editorScene in EditorBuildSettings.scenes)
            {
                var path = editorScene.path;
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                pathToObjects.Add((sceneAsset.name, new UnityEngine.Object[] {sceneAsset}));
            }

            var result = new Dictionary<(string, int), string>();
            foreach (var (path, objects) in pathToObjects)
            {
                foreach (var obj in objects)
                {
                    var type = obj.GetType();
                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long _))
                    {
                        Debug.LogError($"Failed to process object for mapping {obj}");
                        continue;
                    }

                    var iid = obj.GetInstanceID();
                    if (!result.TryAdd((guid, iid), path))
                    {
                        Debug.LogError($"failed to add {obj} to mapping because of duplicate guid allocated by {result[(guid, iid)]} => ({guid}, {iid.ToString()}, {type.FullName})");
                        continue;
                    }
                }
            }

            return result;
        }
    }
}