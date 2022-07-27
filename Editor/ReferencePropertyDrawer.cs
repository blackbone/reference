namespace References.Editor
{
    using System;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(Reference))]
    public sealed class ReferencePropertyDrawer : ReferenceDrawer
    {
        protected override Type TypeRestriction => typeof(UnityEngine.Object);
    }
}