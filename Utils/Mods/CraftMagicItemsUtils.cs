using BagOfTricks.Extensions;

namespace BagOfTricks.Utils.Mods
{
    public static class CraftMagicItemsUtils {
        public static readonly string oldBlueprintSuffix = "#ScribeScroll";
        public static readonly string blueprintSuffix = "#CraftMagicItems";

        public static bool IsModGuid(string stringGuid) {
            return stringGuid.ContainsAny(oldBlueprintSuffix, blueprintSuffix);
        }

        public static void RemoveGuidSuffix(ref string stringGuid) {
            stringGuid = stringGuid.Split('#')[0];
        }
    }
}
