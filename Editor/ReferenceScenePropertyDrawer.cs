// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember

namespace References.Editor
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ReferenceScene))]
    public sealed class ReferenceScenePropertyDrawer : ReferenceDrawer
    {
        private SerializedProperty sceneName;

        protected override Type TypeRestriction => typeof(SceneAsset);

        protected override bool IsDirectLinked() => !string.IsNullOrEmpty(sceneName.stringValue);

        protected override void SetDirectLink(UnityEngine.Object value) => sceneName.stringValue = (value as SceneAsset)?.name;

        protected override bool Validate(UnityEngine.Object asset, bool isLinked, ref Rect validationRect, ref Rect position)
        {
            if (asset == null) return true;
            var isInEditorBuildList = EditorBuildSettings.scenes.Any(s => s.path == AssetDatabase.GetAssetPath(asset));
            if (isInEditorBuildList == isLinked) return true;
            
            validationRect =  new Rect(position.x + position.width - 23, position.y, 23, position.height);
            position.width -= 23;
            return false;
        }

        protected override void DrawValidationControl(Rect rect, bool isLinked, string assetGuid, UnityEngine.Object asset)
        {
            var isInEditorBuildList = EditorBuildSettings.scenes.Any(s => s.path == AssetDatabase.GetAssetPath(asset));
            
            var iconContent = isInEditorBuildList
                ? EditorGUIUtility.IconContent("icons/collabconflict.png", $"|Not linked Scene in Editor Scene List! Remove from scene list.")
                : EditorGUIUtility.IconContent("icons/collabconflict.png", $"|Direct linked Scene not in Editor Scene List! Add Scene to Editor Scene List.");
            
            if (GUI.Button(rect, iconContent, EditorStyles.toolbarButton))
            {
                if (isInEditorBuildList)
                {
                    var list = EditorBuildSettings.scenes.ToList();
                    list.RemoveAll(s => s.path == AssetDatabase.GetAssetPath(asset));
                    EditorBuildSettings.scenes = list.ToArray();
                }
                else
                {
                    var list = EditorBuildSettings.scenes.ToList();
                    list.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(asset), true));
                    EditorBuildSettings.scenes = list.ToArray();
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            sceneName = sceneName ??= property.FindPropertyRelative(nameof(sceneName));
            base.OnGUI(position, property, label);
        }
    }
}