namespace Assets.Editor
{
    using System;
    using System.Linq;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(Reference<>), true)]
    public sealed class ReferencePropertyDrawerGeneric : ReferenceDrawer
    {
        protected override Type TypeRestriction => fieldInfo.FieldType.GetGenericArguments().FirstOrDefault();
    }
}