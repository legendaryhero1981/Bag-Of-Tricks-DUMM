using BagOfTricks.Utils.Mods;
using System;

namespace BagOfTricks.Favourites {
    public class FavouritesModdedGuidManager : FavouritesGuidManager {
        public FavouritesModdedGuidManager(string modEntryPath, string favouritesFolder, FavouriteXMLFile xml) : base(modEntryPath, favouritesFolder, xml) {
        }

        public override bool IsValidGuid(string stringGuid) {
            if (!String.IsNullOrWhiteSpace(stringGuid)) {
                if (CraftMagicItemsUtils.IsModGuid(stringGuid)) {
                    CraftMagicItemsUtils.RemoveGuidSuffix(ref stringGuid);
                }
                return base.IsValidGuid(stringGuid);
            }
            return false;
        }
    }
}
