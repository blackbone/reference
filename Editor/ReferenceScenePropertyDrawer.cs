namespace Assets.Editor
{
    using System;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ReferenceScene))]
    public sealed class ReferenceScenePropertyDrawer : ReferenceDrawer
    {
        protected override Type TypeRestriction => typeof(SceneAsset);
    }
}