using BagOfTricks.Favourites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BagOfTricks.Favourites {
    public static class FavouritesFactory {
        private static FavouritesManager abilities;
        private static FavouritesManager buffs;
        private static FavouritesManager feats;
        private static FavouritesManager functions;
        private static FavouritesManager items;
        private static FavouritesManager units;

        private static List<FavouritesManager> managers = new List<FavouritesManager>();
        private const string favouritesFolder = "Favourites";

        public static string GetFavouritesFolder => favouritesFolder;
        public static FavouritesManager GetFavouriteAbilities => abilities;
        public static FavouritesManager GetFavouriteBuffs => buffs;
        public static FavouritesManager GetFavouriteFeats => feats;
        public static FavouritesManager GetFavouriteFunctions => functions;
        public static FavouritesManager GetFavouriteItems => items;
        public static FavouritesManager GetFavouriteUnits => units;

        public static void DeserializeFavourites() {
            foreach (FavouritesManager manager in managers) {
                manager.Deserialize();
            }
        }

        public static void SerializeFavourites() {
            foreach (FavouritesManager manager in managers) {
                manager.Serialize();
            }
        }

        public static void Init(string modEntryPath) {
            managers.Clear();
            abilities = new FavouritesManager(modEntryPath, favouritesFolder, FavouriteXMLFile.Abilities);
            managers.Add(abilities);
            buffs = new FavouritesManager(modEntryPath, favouritesFolder, FavouriteXMLFile.Buffs);
            managers.Add(buffs);
            feats = new FavouritesManager(modEntryPath, favouritesFolder, FavouriteXMLFile.Feats);
            managers.Add(feats);
            functions = new FavouritesManager(modEntryPath, favouritesFolder, FavouriteXMLFile.Functions);
            managers.Add(functions);
            items = new FavouritesManager(modEntryPath, favouritesFolder, FavouriteXMLFile.Items);
            managers.Add(items);
            units = new FavouritesManager(modEntryPath, favouritesFolder, FavouriteXMLFile.Units);
            managers.Add(units);
        }
    }
}
