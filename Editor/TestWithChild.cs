using UnityEditor;
using UnityEngine;

namespace Packages.reference.Editor
{
    public class TestWithChild : ScriptableObject
    {
        [MenuItem("Test/Create Test Asset")]
        public static void CreateTestAsset()
        {
            const string path = "Assets/test.asset";
            
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                AssetDatabase.DeleteAsset(path);
            
            AssetDatabase.Refresh();
            
            AssetDatabase.StartAssetEditing();
            {
                var asset = CreateInstance<TestWithChild>();
                asset.name = "test2";
                var child1 = CreateInstance<TestWithChild>();
                var child2 = CreateInstance<TestWithChild>();
                var child3 = new Mesh();
                var child4 = new Mesh();
                var child5 = new Texture2D(64, 64);
                var child6 = new Texture2D(64, 64);
                child1.name = child2.name = child3.name = child4.name = child5.name = child6.name = "child";
                
                
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.AddObjectToAsset(child1, path);
                AssetDatabase.AddObjectToAsset(child2, path);
                AssetDatabase.AddObjectToAsset(child3, path);
                AssetDatabase.AddObjectToAsset(child4, path);
                AssetDatabase.AddObjectToAsset(child5, path);
                AssetDatabase.AddObjectToAsset(child6, path);
            }
            AssetDatabase.StopAssetEditing();
            
            AssetDatabase.Refresh();
        }
    }
}