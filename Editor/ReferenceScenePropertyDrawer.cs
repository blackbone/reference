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
        protected override Type TypeRestriction => typeof(SceneAsset);

        protected override bool IsDirectLinked(SerializedProperty property)
            => !string.IsNullOrEmpty(property.FindPropertyRelative("sceneName").stringValue);

        protected override void SetDirectLink(SerializedProperty property, UnityEngine.Object value)
            => property.FindPropertyRelative("sceneName").stringValue = (value as SceneAsset)?.name;

        protected override bool Validate(UnityEngine.Object asset, bool isLinked, ref Rect validationRect, ref Rect position)
        {
            if (asset == null) return true;
            var isInEditorBuildList = EditorBuildSettings.scenes.Any(s => s.path == AssetDatabase.GetAssetPath(asset));
            if (isInEditorBuildList == isLinked) return true;
            
            validationRect =  new Rect(position.x + position.width - 23, position.y, 23, position.height);
            position.width -= 23;
            return false;
        }

        protected override void DrawValidationControl(Rect rect, bool isLinked, string assetGuid,
            UnityEngine.Object                             asset)
        {
            var isInEditorBuildList = EditorBuildSettings.scenes.Any(s => s.path == AssetDatabase.GetAssetPath(asset));

            GUIContent iconContent;

            switch (isLinked)
            {
                case true when !isInEditorBuildList:
                    iconContent = EditorGUIUtility.IconContent("icons/collabconflict.png",
                                                               "|Direct linked Scene not in Editor Scene List! Add Scene to Editor Scene List.");
                    break;
                case false when isInEditorBuildList:
                    iconContent = EditorGUIUtility.IconContent("icons/collabconflict.png",
                                                               "|Not linked Scene in Editor Scene List! Remove from scene list.");
                    break;
                default:
                    return;
            }

            if (!GUI.Button(rect, iconContent, EditorStyles.toolbarButton)) return;

            var list = EditorBuildSettings.scenes.ToList();
            if (isInEditorBuildList)
            {
                list.RemoveAll(s => s.path == AssetDatabase.GetAssetPath(asset));
                EditorBuildSettings.scenes = list.ToArray();
                return;
            }

            list.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(asset), true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}