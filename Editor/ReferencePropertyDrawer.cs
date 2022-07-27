// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember

namespace References.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(Reference), false)]
    public class ReferencePropertyDrawer : ReferenceDrawer
    {
        private            SerializedProperty asset;
        
        protected override Type               TypeRestriction => typeof(UnityEngine.Object);
        
        protected override bool IsDirectLinked() => asset.objectReferenceValue != null;

        protected override void SetDirectLink(UnityEngine.Object value) => asset.objectReferenceValue = value;
        protected override bool Validate(UnityEngine.Object validAsset, bool isLinked, ref Rect validationRect, ref Rect position)
            => true;

        protected override void DrawValidationControl(Rect validationRect, bool isLinked, string assetGuid, UnityEngine.Object validAsset)
        {
             // no op
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            asset = asset ??= property.FindPropertyRelative(nameof(asset));
            base.OnGUI(position, property, label);
        }
    }
}