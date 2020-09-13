using BagOfTricks.Utils;
using UnityModManagerNet;

namespace BagOfTricks.Favourites {
    public class FavouritesGuidManager : FavouritesManager {
        private UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;

        public FavouritesGuidManager(string modEntryPath, string favouritesFolder, FavouriteXMLFile xml) : base(modEntryPath, favouritesFolder, xml) {
        }


        public void AddGuid(string stringGuid) {
            if (IsValidGuid(stringGuid)) {
                this.FavouritesList.Add(stringGuid);
            }
            else {
                modLogger.Log("[" + stringGuid + "] " + Strings.GetText("error_InvalidGUID"));
            }
        }

        public virtual bool IsValidGuid(string stringGuid) {
            return StringUtils.IsGUID(stringGuid);
        }
    }
}
