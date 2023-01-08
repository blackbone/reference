namespace References.UnityResources
{
    internal sealed class ResourceInfo
    {
        public readonly string Guid;
        public readonly string[] SubAssetNames;
        public readonly string ResourcePath;

        public ResourceInfo(string guid, string[] subAssetNames, string resourcePath)
        {
            Guid = guid;
            SubAssetNames = subAssetNames;
            ResourcePath = resourcePath;
        }
    }
}