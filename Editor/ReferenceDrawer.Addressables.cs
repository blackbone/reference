#if ADDRESSABLES
namespace Assets.Editor
{
    using System.Diagnostics;
    using UnityEditor;
    using UnityEngine;

    public abstract partial class ReferenceDrawer
    {
        [Conditional("ADDRESSABLES")]
        private void ModifyAddressableRect(Object asset, ref Rect addressableRect, ref Rect objectFieldRect)
        {
            if (asset != null)
            {
                addressableRect       =  new Rect(objectFieldRect.x + objectFieldRect.width - 23, objectFieldRect.y, 23, objectFieldRect.height);
                objectFieldRect.width -= 23;
            }
            else
                addressableRect = Rect.zero;
        }

        [Conditional("ADDRESSABLES")]
        private void DrawAddressablesControl(Rect rect, string assetGuid, Object asset)
        {
            if (asset != null)
            {
                var isAddressable = AddressableUtility.TryGetAssetEntry(assetGuid, out var assetEntry, out var isImplicitlyAdded);
                var iconContent = isAddressable
                    ? isImplicitlyAdded
                        ? EditorGUIUtility.IconContent("icons/processed/d_prefabvariant icon.asset", $"|Is implicit addressable ({assetEntry.address})")
                        : EditorGUIUtility.IconContent("icons/processed/d_prefabmodel icon.asset", $"|Is addressable ({assetEntry.address})")
                    : EditorGUIUtility.IconContent("icons/collabconflict.png", $"|Not addressable! Link or add to addressables by click.");
            
                if (GUI.Button(rect, iconContent, EditorStyles.toolbarButton))
                {
                    if (!isAddressable || isImplicitlyAdded) AddressableUtility.AddToAddressables(asset, null, AssetDatabase.GetAssetPath(asset));
                    else AddressableUtility.RemoveFromAddressables(asset);
                }
            }
        }
    }
}
#endif
