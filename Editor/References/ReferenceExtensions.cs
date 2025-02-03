using UnityEngine;
using UnityEditor;

namespace References.Editor
{
    public static class ReferenceExtensions
    {
        public static string GetEditorAssetPath<T>(this Reference<T> reference) where T : Object
            => AssetDatabase.GUIDToAssetPath(reference.AssetGuid);

        public static bool TryGetEditorAsset<T>(this Reference<T> reference, out T result) where T : Object
        {
            var assetPath = GetEditorAssetPath(reference);
            if (string.IsNullOrEmpty(assetPath))
            {
                result = null;
                return false;
            }

            var editorAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            var assetType = editorAsset.GetType();
            var requiredType = typeof(T);

            if (typeof(Component).IsAssignableFrom(requiredType))
            {
                switch (reference.Asset)
                {
                    case GameObject gameObject:
                        result = gameObject.GetComponent<T>();
                        return true;
                    case Component:
                        result = reference.Asset;
                        return true;
                    default:
                        result = null;
                        return false;
                }
            }

            if (requiredType.IsAssignableFrom(assetType))
            {
                result = reference.Asset;
                return true;
            }

            result = null;
            return false;
        }
    }
}
