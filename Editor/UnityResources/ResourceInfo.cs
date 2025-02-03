using System.Collections.Generic;
using System.Linq;

namespace References.UnityResources.Editor
{
    internal sealed class ResourceInfo
    {
        private readonly string Guid;
        private readonly string[] SubAssetNames;
        private readonly string ResourcePath;

        public ResourceInfo(string guid, string[] subAssetNames, string resourcePath)
        {
            Guid = guid;
            SubAssetNames = subAssetNames;
            ResourcePath = resourcePath;
        }

        public override string ToString()
        {
            const char split = '~';
            return string.Join(split, Enumerate().ToArray());

            IEnumerable<string> Enumerate()
            {
                yield return Guid;
                yield return ResourcePath;
                var count = SubAssetNames?.Length ?? 0;
                yield return count.ToString();
                for (var i = 0; i < count; i++)
                    yield return SubAssetNames?[i];
            }
        }
    }
}
