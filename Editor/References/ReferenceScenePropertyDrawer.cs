// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember

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
    }
}