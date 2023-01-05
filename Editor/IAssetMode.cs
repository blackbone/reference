using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace References.Editor
{
    public interface IAssetMode
    {
        public bool Supports(Type type);
        bool Matches(SerializedProperty serializedProperty);
        void Apply(SerializedProperty serializedProperty);
        void Reset(SerializedProperty serializedProperty);
        
        private static readonly UnknownAssetMode UnknownAssetMode = new();
        public static IAssetMode GetAssetMode(Type propertyType, SerializedProperty serializedProperty)
        {
            return GetSupportedAssetModes(propertyType)
                .FirstOrDefault(mode => mode.Matches(serializedProperty)) ?? UnknownAssetMode;
        }

        private static readonly Dictionary<Type, IAssetMode[]> SupportedAssetModes = new();

        public static IReadOnlyList<IAssetMode> GetSupportedAssetModes(Type propertyType)
        {
            if (SupportedAssetModes.TryGetValue(propertyType, out var supportedModes))
                return supportedModes;

            supportedModes = TypeCache.GetTypesDerivedFrom<IAssetMode>()
                                      .Except(new[] { typeof(UnknownAssetMode) })
                                      .Select(type => Activator.CreateInstance(type) as IAssetMode)
                                      .Where(mode => mode.Supports(propertyType))
                                      .ToArray();

            SupportedAssetModes[propertyType] = supportedModes;

            return Array.Empty<IAssetMode>();
        }
    }

    public sealed class UnknownAssetMode : IAssetMode
    {
        public bool Supports(Type type) => true;

        public bool Matches(SerializedProperty serializedProperty) => true;
        public void Apply(SerializedProperty serializedProperty)
        {
        }

        public void Reset(SerializedProperty serializedProperty)
        {
        }
    }
    
    public sealed class DirectLinkAssetMode : IAssetMode
    {
        public bool Supports(Type type)
        {
            // non references
            if (!typeof(IReference).IsAssignableFrom(type))
                return false;
            
            // scene references don't support direct links, only resources
            if (type == typeof(ReferenceScene))
                return false;
            
            // others supports all
            return true;
        }

        public bool Matches(SerializedProperty serializedProperty)
        {
            throw new NotImplementedException();
        }

        public void Apply(SerializedProperty serializedProperty)
        {
        }

        public void Reset(SerializedProperty serializedProperty)
        {
        }
    }
    
    public sealed class ResourceAssetMode : IAssetMode
    {
        public bool Supports(Type type)
        {
            // non references
            if (!typeof(IReference).IsAssignableFrom(type))
                return false;

            // everything can be loaded from resources
            return true;
        }

        public bool Matches(SerializedProperty serializedProperty)
        {
            return true;
        }

        public void Apply(SerializedProperty serializedProperty)
        {
        }

        public void Reset(SerializedProperty serializedProperty)
        {
        }
    }
}