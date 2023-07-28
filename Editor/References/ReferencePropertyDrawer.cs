// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember

using System.Reflection;
using UnityEngine.Assertions;

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
        
        protected override string GetCodeString(string guid, string subAsset)
        {
            Assert.IsFalse(string.IsNullOrEmpty(guid));
            
            return $"new Reference(\"{guid}\"{(string.IsNullOrEmpty(subAsset) ? string.Empty : ", \"{subAsset}\"")})";
        }
    }
}