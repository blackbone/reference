// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember

using UnityEngine.Assertions;

namespace References.Editor
{
    using System;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ReferenceScene))]
    public sealed class ReferenceScenePropertyDrawer : ReferenceDrawer
    {
        protected override Type TypeRestriction => typeof(SceneAsset);
        protected override bool CanReferSubAssets => false;
        protected override bool CanBeDirect => false;
        
        protected override string GetCodeString(string guid, string subAsset)
        {
            Assert.IsFalse(string.IsNullOrEmpty(guid));
            
            return $"new Reference(\"{guid}\")";
        }
    }
}