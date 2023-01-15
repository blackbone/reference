// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember

using System.Reflection;
using UnityEngine;

namespace References.Editor
{
    using System;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(Reference), false)]
    public class ReferencePropertyDrawer : ReferenceDrawer
    {
        protected override Type TypeRestriction => typeof(UnityEngine.Object);
        protected override bool CanReferSubAssets => true;
        protected override bool CanBeDirect => fieldInfo.GetCustomAttribute(typeof(NoDirectAttribute)) == null;
    }
}