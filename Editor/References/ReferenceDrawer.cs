// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming

namespace References.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public abstract class ReferenceDrawer : PropertyDrawer
    {
        private static readonly GUIContent LinkedContent = new(EditorGUIUtility.IconContent("d_Linked"))
        {
            tooltip = "Directly linked. Asset will be loaded immediately (and be in dependencies)."
        };
        private static readonly GUIContent NotLinkedContent = new(EditorGUIUtility.IconContent("d_Unlinked"))
        {
            tooltip = "Not directly linked. Asset will be loaded through asset provider (and will not be in dependencies)."
        };
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public override bool CanCacheInspectorGUI(SerializedProperty property) => false;

        protected abstract Type TypeRestriction { get; }
        protected abstract bool UseSubAssetIds { get; }
        protected abstract bool UseDirectLink { get; }

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
                position.width -= 30;
            }

            SerializedProperty guidProperty;
            SerializedProperty subAssetIdProperty = null;
            SerializedProperty directReferenceProperty = null;

            string guid = null;
            var subAssetId = 0L;
            UnityEngine.Object directReference = null;
            var needChangeLink = false;
            var isDirectlyLinked = false;
            
            guidProperty = property.FindPropertyRelative(Reference.Names.Guid);
            guid = guidProperty.stringValue;

            if (UseSubAssetIds)
            {
                subAssetIdProperty = property.FindPropertyRelative(Reference.Names.SubAssetId);
                subAssetId = subAssetIdProperty.longValue;
            }

            if (UseDirectLink)
            {
                directReferenceProperty = property.FindPropertyRelative(Reference.Names.DirectReference);
                directReference = directReferenceProperty.objectReferenceValue;
                isDirectlyLinked = directReference != null;
            }
            
            var currentAsset = GetEditorAsset(guid, UseSubAssetIds ? subAssetId : null);

            // drawing
            if (UseDirectLink) position.width -= 30;

            var newAsset = EditorGUI.ObjectField(position, currentAsset, TypeRestriction, false);
            
            if (UseDirectLink)
            {
                position.x += position.width;
                position.width = 30;

                var guiEnabled = GUI.enabled;
                GUI.enabled = currentAsset != null;
                var buttonContent = directReference != null ? LinkedContent : NotLinkedContent;
                needChangeLink = GUI.Button(position, buttonContent, EditorStyles.toolbarButton);
                GUI.enabled = guiEnabled;
            }
            
            if (!needChangeLink && newAsset == currentAsset)
            {
                return;
            }

            // applying changes
            if (needChangeLink)
            {
                // ReSharper disable PossibleNullReferenceException
                if (UseDirectLink) directReferenceProperty.objectReferenceValue = isDirectlyLinked ? null : newAsset;
                // ReSharper restore PossibleNullReferenceException
                
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.UpdateIfRequiredOrScript();
                return;
            }

            string newGuid;
            long newSubAssetId;

            if (newAsset == null)
            {
                newGuid = null;
                newSubAssetId = 0;
            }
            else
            {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newAsset, out newGuid, out newSubAssetId))
                {
                    Debug.LogError("WTF? Something went wrong!");
                    return;
                }
            }

            if (newGuid == guid && subAssetId == newSubAssetId)
            {
                Debug.LogError("WTF? Asset changed but guid and instance id not!");
                return;
            }
            
            guidProperty.stringValue = newGuid;
            // ReSharper disable PossibleNullReferenceException
            if (UseSubAssetIds) subAssetIdProperty.longValue = newSubAssetId;
            if (UseDirectLink) directReferenceProperty.objectReferenceValue = isDirectlyLinked ? newAsset : null;
            // ReSharper restore PossibleNullReferenceException
            
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.UpdateIfRequiredOrScript();
        }

        private static UnityEngine.Object GetEditorAsset(string guid, long? subAssetId)
        {
            if (string.IsNullOrEmpty(guid))
                return null;
            
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset is SceneAsset)
                return asset;

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long liif) && liif == subAssetId)
                return asset;
            
            foreach (var subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
            {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(subAsset, out _, out liif) || liif != subAssetId)
                    continue;

                return subAsset;
            }

            foreach (var subAsset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(subAsset, out _, out liif) || liif != subAssetId)
                    continue;

                return subAsset;
            }

            return null;
        }
    }
}