using UnityEngine;

namespace References.Editor
{
    using System;
    using System.Collections;
    using System.Linq;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(Reference<>), true)]
    public sealed class ReferencePropertyDrawerGeneric : ReferencePropertyDrawer
    {
        private static readonly Type TypeIList = typeof(IList);
        
        protected override Type TypeRestriction
        {
            get
            {
                var type = fieldInfo.FieldType;
                
                // handle arrays
                if (type.IsArray)
                    type = type.GetElementType();
                
                // handle lists
                else if (TypeIList.IsAssignableFrom(type) && type.IsGenericType)
                    type = type.GetGenericArguments().FirstOrDefault();

                type = type?.GetGenericArguments().FirstOrDefault();
                
                if (type == null)
                    throw new NullReferenceException();
                
                return type;
            }
        }
    }
}