namespace References
{
    using UnityEngine;

    public static partial class ReferenceExtensions
    {
        private static bool CheckDirectReference(Reference reference, out Object result)
        {
            result = reference.Asset;
            return result != null;
        }

        private static bool CheckDirectReference<T>(Reference<T> reference, out T result) where T : Object
        {
            if (reference.Asset == null)
            {
                result = null;
                return false;
            }
            
            var assetType = reference.Asset.GetType();
            var requiredType      = typeof(T);

            if (typeof(Component).IsAssignableFrom(requiredType))
            {
                switch (reference.Asset)
                {
                    case GameObject gameObject:
                        result = gameObject.GetComponent<T>();
                        return true;
                    case Component:
                        result = reference.Asset;
                        return true;
                    default:
                        result = null;
                        return false;
                }
            }

            if (requiredType.IsAssignableFrom(assetType))
            {
                result = reference.Asset;
                return true;
            }

            result = null;
            return false;
        }
    }
}
