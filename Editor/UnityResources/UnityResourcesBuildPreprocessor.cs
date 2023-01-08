using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Pool;

namespace References.UnityResources.Editor
{
    internal sealed class UnityResourcesBuildPreprocessor : IPreprocessBuildWithReport
    {
        private const string ResourceMapPath = "Assets/Resources/" + UnityResourcesAssetProvider.ResourceMapName + ".bytes";
        private const string PlayModeGenerateToggle = "Assets/References/Generate Resource Map on Play Mode";
        private static bool isEnabled;

        static UnityResourcesBuildPreprocessor()
        {
            isEnabled = EditorPrefs.GetBool(nameof(PlayModeGenerateToggle));
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!isEnabled) return;
            if (state != PlayModeStateChange.ExitingEditMode) return;
            
            GenerateResourceMap();
        }

        public int callbackOrder => -999;

        public void OnPreprocessBuild(BuildReport report) => GenerateResourceMap();
        
        [MenuItem(PlayModeGenerateToggle, priority = 314)]
        private static void EnableEditorMapGeneration()
        {
            isEnabled = !isEnabled;
            Menu.SetChecked(PlayModeGenerateToggle, isEnabled);
        }
        
        [MenuItem(PlayModeGenerateToggle, priority = 314, validate = true)]
        private static bool EnableEditorMapGenerationValidate()
        {
            Menu.SetChecked(PlayModeGenerateToggle, isEnabled);
            return true;
        }

        [MenuItem("Assets/References/Generate Resource Map", priority = 314)]
        private static void GenerateResourceMap()
        {
            using var fs = new FileStream(ResourceMapPath, FileMode.Create);
            using var bw = new BinaryWriter(fs, Encoding.UTF8);

            foreach (var resourceInfo in GetResourceRepresentations())
            {
                bw.Write(resourceInfo.Guid);
                
                if (resourceInfo.SubAssetNames != null)
                {
                    bw.Write(resourceInfo.SubAssetNames.Length);
                    foreach (var subAssetName in resourceInfo.SubAssetNames)
                        bw.Write(subAssetName);
                }
                else
                    bw.Write(0);

                bw.Write(resourceInfo.ResourcePath);
                bw.Write("\r\n");
            }
            
            bw.Flush();

            AssetDatabase.ImportAsset(ResourceMapPath);
        }

        private static IEnumerable<ResourceInfo> GetResourceRepresentations()
        {
            // add all included scene mapping (guid -> scene name)
            foreach (var editorScene in EditorBuildSettings.scenes.Where(si => si.enabled))
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(editorScene.path);
                var guid = AssetDatabase.AssetPathToGUID(editorScene.path);
                yield return new ResourceInfo(guid, null, sceneAsset.name);
            }
            
            // add all contents for Resources folders
            const string resourcesFolderPattern = "/Resources/";

            var allResources = Resources.LoadAll(string.Empty);
            var allPaths = allResources.Select(AssetDatabase.GetAssetPath).ToArray();
            var allProjectPaths = allPaths.Where(path => path.StartsWith("Assets/")).ToArray();

            HashSetPool<string>.Get(out var uniqueResourcePaths);
            foreach (var projectPath in allProjectPaths)
            {
                var resourcePath = projectPath[(projectPath.LastIndexOf(resourcesFolderPattern, StringComparison.Ordinal) + resourcesFolderPattern.Length)..];
                resourcePath = resourcePath[..resourcePath.LastIndexOf(".", StringComparison.Ordinal)];
                if (!uniqueResourcePaths.Add(resourcePath))
                    Debug.LogError($"Resource path \"{resourcePath}\" is used multiple times across project. Runtime collisions possible (and reference can point not what you expect).");
                
                var guid = AssetDatabase.AssetPathToGUID(projectPath);
                var subObjectNames = AssetDatabase.LoadAllAssetRepresentationsAtPath(projectPath).Select(o => o.name).ToArray();
                
                yield return new ResourceInfo(guid, subObjectNames.Length > 0 ? subObjectNames : null, resourcePath);
            }
        }
    }
}