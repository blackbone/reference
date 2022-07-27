namespace References.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public abstract partial class ReferenceDrawer : PropertyDrawer
    {
        private SerializedProperty assetGuid;
        private SerializedProperty asset;
        
        protected abstract Type TypeRestriction { get; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            var assetGuid = (this.assetGuid ??= property.FindPropertyRelative(nameof(this.assetGuid))).stringValue;
            var linkedAsset    = (this.asset ??= property.FindPropertyRelative(nameof(this.asset))).objectReferenceValue;
            var isLinked = linkedAsset != null;
            var asset     = isLinked ? linkedAsset : AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetGuid), TypeRestriction);

            var addressableRect = Rect.zero;
#if ADDRESSABLES
            if (!isLinked) ModifyAddressableRect(asset, ref addressableRect, ref position);
#endif

            var linkRect = new Rect(position.x + position.width - 23, position.y, 23, position.height);
            position.width -= 23;
            
            var newValue = EditorGUI.ObjectField(position, asset, TypeRestriction, false);

            var iconContent = isLinked
                ? EditorGUIUtility.IconContent("icons/packagemanager/dark/link.png", "|Linked. Will be loaded by direct link.")
                : EditorGUIUtility.IconContent("icons/unlinked.png", "|Unlinked. Will be loaded through Asset Service.");

            if (GUI.Button(linkRect, iconContent, EditorStyles.toolbarButton))
            {
                this.asset.objectReferenceValue = isLinked ? null : asset;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            
#if ADDRESSABLES
            if(!isLinked)
                DrawAddressablesControl(addressableRect, assetGuid, asset);
#endif

            if (newValue == asset)
                return;

            if (newValue == null)
            {
                this.assetGuid.stringValue      = string.Empty;
                this.asset.objectReferenceValue = null;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                return;
            }

            if (!AssetDatabaseUtility.TryGetAssetGuid(newValue, out var guid))
                Debug.LogError("ERROR");

            this.assetGuid.stringValue = guid;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}