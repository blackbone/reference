// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming

using System.Linq;

namespace References.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public abstract class ReferenceDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public override bool CanCacheInspectorGUI(SerializedProperty property) => false;

        protected abstract Type TypeRestriction { get; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // not array element - draw label
            var path = property.propertyPath;
            if (!path.EndsWith(']'))
                position = EditorGUI.PrefixLabel(position, label);
            else
            {
                var lastIndex = path.LastIndexOf('[') + 1;

                EditorGUI.LabelField(position, $"{path.Substring(lastIndex, path.Length - lastIndex - 1)}.");
                position.x += 30;
            }
            
            var assetGuidProperty = property.FindPropertyRelative(Reference.Names.AssetGuid);
            var instanceIdProperty = property.FindPropertyRelative(Reference.Names.InstanceId);
            var assetGuid = assetGuidProperty.stringValue;
            var instanceId = instanceIdProperty.intValue;
            var currentAsset = GetEditorAsset(assetGuid, instanceId);
            
            var newAsset = EditorGUI.ObjectField(position, currentAsset, TypeRestriction, false);
            if (newAsset == currentAsset)
                return;
            
            if (newAsset == null)
            {
                assetGuidProperty.stringValue = null;
                instanceIdProperty.intValue = 0;
                return;
            }

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newAsset, out var newGuid, out long _);
            var newInstanceId = newAsset != null ? newAsset.GetInstanceID() : 0;

            if (newGuid == assetGuid && instanceId == newInstanceId)
                return;
                    
            assetGuidProperty.stringValue = newGuid;
            instanceIdProperty.intValue = newInstanceId;
        }

        private static UnityEngine.Object GetEditorAsset(string assetGuid, int instanceId)
        {
            if (string.IsNullOrEmpty(assetGuid))
                return null;
            
            var path = AssetDatabase.GUIDToAssetPath(assetGuid);
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            if (mainAsset.GetInstanceID() == instanceId)
                return mainAsset;

            var allAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            return allAssets.FirstOrDefault(asset => asset.GetInstanceID() == instanceId);
        }
    }
}