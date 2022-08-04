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
        protected override Type TypeRestriction => typeof(UnityEngine.Object);
        
        protected override bool IsDirectLinked(SerializedProperty property)
            => property.FindPropertyRelative("asset").objectReferenceValue != null;

        protected override void SetDirectLink(SerializedProperty property, UnityEngine.Object value)
            => property.FindPropertyRelative("asset").objectReferenceValue = value;
        
        protected override bool Validate(UnityEngine.Object validAsset, bool isLinked, ref Rect validationRect, ref Rect position)
            => true;

        protected override void DrawValidationControl(Rect validationRect, bool isLinked, string assetGuid, UnityEngine.Object validAsset)
        {
             // no op
        }
    }
}