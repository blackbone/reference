// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming

namespace References.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public abstract partial class ReferenceDrawer : PropertyDrawer
    {
        public override bool CanCacheInspectorGUI(SerializedProperty property) => false;

        protected abstract Type   TypeRestriction        { get; }

        protected abstract bool IsDirectLinked(SerializedProperty property);
        protected abstract void SetDirectLink(SerializedProperty  property, UnityEngine.Object value);

        protected abstract bool Validate(UnityEngine.Object asset, bool isLinked, ref Rect validationRect, ref Rect position);

        protected abstract void DrawValidationControl(Rect validationRect, bool isLinked, string assetGuid, UnityEngine.Object asset);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);
            
            // the common part
            var assetGuidProperty = property.FindPropertyRelative("assetGuid");
            var assetGuid = assetGuidProperty.stringValue;
            var validAsset     = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetGuid), TypeRestriction);
            var isLinked  = IsDirectLinked(property);

            var validationRect  = Rect.zero;
            var addressableRect = Rect.zero;

            var isValid = Validate(validAsset, isLinked, ref validationRect, ref position);
#if ADDRESSABLES
            if (!isLinked) ModifyAddressableRect(validAsset, ref addressableRect, ref position);
#endif

            var linkRect = new Rect(position.x + position.width - 23, position.y, 23, position.height);
            position.width -= 23;
            
            var newValue = EditorGUI.ObjectField(position, validAsset, TypeRestriction, false);

            var iconContent = isLinked
                ? EditorGUIUtility.IconContent("icons/packagemanager/dark/link.png", "|Linked. Will be loaded by direct link.")
                : EditorGUIUtility.IconContent("icons/unlinked.png", "|Unlinked. Will be loaded through Asset Service.");

            if (GUI.Button(linkRect, iconContent, EditorStyles.toolbarButton))
            {
                SetDirectLink(property, isLinked ? null : validAsset);
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            
#if ADDRESSABLES
            if (!isLinked) DrawAddressablesControl(addressableRect, assetGuid, validAsset);
#endif
            if (!isValid) DrawValidationControl(validationRect, isLinked, assetGuid, validAsset);

            if (newValue == validAsset)
                return;

            if (newValue == null)
            {
                assetGuidProperty.stringValue      = string.Empty;
                SetDirectLink(property, null);
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                return;
            }

            if (!AssetDatabaseUtility.TryGetAssetGuid(newValue, out var guid))
                Debug.LogError("ERROR");

            assetGuidProperty.stringValue = guid;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}