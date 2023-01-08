// using System;
// using JetBrains.Annotations;
// using UnityEditor;
//
// namespace References.Editor
// {
//     [UsedImplicitly]
//     internal class AddressableAssetMode : IAssetMode
//     {
//         public bool Supports(Type type)
//         {
//             if (!typeof(IReference).IsAssignableFrom(type))
//                 return false;
//
//             return true;
//         }
//
//         public bool Matches(SerializedProperty serializedProperty)
//         {
//             // is current mode is addressable determined by "current asset is in addressables"
//             return AddressableUtility.TryGetAsset(serializedProperty.FindPropertyRelative(Reference.Names.AssetGuid).stringValue, out Object obj);
//         }
//     }
// }