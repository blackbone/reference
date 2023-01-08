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
        protected abstract bool CanReferSubAssets { get; }
        protected abstract bool CanBeDirect { get; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // not array element - draw label
            var propertyPath = property.propertyPath;
            if (!propertyPath.EndsWith(']'))
                position = EditorGUI.PrefixLabel(position, label);
            else
            {
                var lastIndex = propertyPath.LastIndexOf('[') + 1;

                EditorGUI.LabelField(position, $"{propertyPath.Substring(lastIndex, propertyPath.Length - lastIndex - 1)}.");
                position.x += 30;
                position.width -= 30;
            }

            SerializedProperty guidProperty;
            SerializedProperty subAssetProperty = null;
            SerializedProperty directReferenceProperty = null;

            string guid = null;
            string subAsset = null;
            UnityEngine.Object directReference = null;
            var needChangeLink = false;
            var isDirectlyLinked = false;
            
            guidProperty = property.FindPropertyRelative(Reference.Names.Guid);
            guid = guidProperty.stringValue;

            if (CanReferSubAssets)
            {
                subAssetProperty = property.FindPropertyRelative(Reference.Names.SubAsset);
                subAsset = subAssetProperty.stringValue;
            }

            if (CanBeDirect)
            {
                directReferenceProperty = property.FindPropertyRelative(Reference.Names.DirectReference);
                directReference = directReferenceProperty.objectReferenceValue;
                isDirectlyLinked = directReference != null;
            }
            
            var currentAsset = GetEditorAsset(guid, CanReferSubAssets ? subAsset : null);

            // drawing
            if (CanBeDirect) position.width -= 30;

            var newAsset = EditorGUI.ObjectField(position, currentAsset, TypeRestriction, false);
            
            if (CanBeDirect)
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
                if (CanBeDirect) directReferenceProperty.objectReferenceValue = isDirectlyLinked ? null : newAsset;
                // ReSharper restore PossibleNullReferenceException
                
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.UpdateIfRequiredOrScript();
                return;
            }

            string newGuid;
            string newSubAsset;

            if (newAsset == null)
            {
                newGuid = null;
                newSubAsset = null;
            }
            else
            {
                if (newAsset is Component component)
                {
                    newSubAsset = null;

                    if (TypeRestriction.IsInstanceOfType(component))
                        newAsset = component;
                }
                else
                    newSubAsset = AssetDatabase.IsMainAsset(newAsset) ? null : newAsset.name;

                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newAsset, out newGuid, out long _))
                {
                    Debug.LogError("WTF? Something went wrong!");
                    return;
                }
            }

            if (newGuid == guid && subAsset == newSubAsset)
            {
                Debug.LogError("WTF? Asset changed but guid and instance id not!");
                return;
            }
            
            guidProperty.stringValue = newGuid;
            // ReSharper disable PossibleNullReferenceException
            if (CanReferSubAssets) subAssetProperty.stringValue = newSubAsset;
            if (CanBeDirect) directReferenceProperty.objectReferenceValue = isDirectlyLinked ? newAsset : null;
            // ReSharper restore PossibleNullReferenceException
            
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.UpdateIfRequiredOrScript();
        }

        private UnityEngine.Object GetEditorAsset(string guid, string subAssetName)
        {
            if (string.IsNullOrEmpty(guid))
                return null;
            
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = string.IsNullOrEmpty(subAssetName)
                ? AssetDatabase.LoadMainAssetAtPath(path)
                : AssetDatabase.LoadAllAssetRepresentationsAtPath(path).FirstOrDefault(a => a.name == subAssetName);
            
            if (asset is SceneAsset)
                return asset;

            if (asset is GameObject gameObject)
            {
                if (typeof(Component).IsAssignableFrom(TypeRestriction))
                {
                    asset = gameObject.GetComponent(TypeRestriction);
                }
            }

            return TypeRestriction.IsInstanceOfType(asset) ? asset : null;
        }
    }
}