using BagOfTricks.Utils;
using System.Collections.Generic;
using System.Linq;
using BagOfTricks.Extensions;
using System.IO;

namespace BagOfTricks.Favourites {
    public class FavouritesManager {
        private string path;
        private string fileName;
        private List<string> favouritesList;
        public string GetPath => path;
        public string GetFileName => fileName;
        public List<string> FavouritesList { get => favouritesList; set => favouritesList = value; }


        public FavouritesManager(string modEntryPath, string favouritesFolder, FavouriteXMLFile xml) {
            fileName = xml.GetDescription();
            path = modEntryPath + favouritesFolder + "\\" + fileName;
            FavouritesList = new List<string>();
        }


        private void DeleteFavouritesXML() {
            File.Delete(path);
        }

        public bool FavouritesXMLExists() {
            return File.Exists(path);
        }

        public void Serialize() {
            if (FavouritesList.Any()) {
                XMLUtils.SerializeListString(favouritesList, path);
            }
            else {
                if (FavouritesXMLExists()) {
                    DeleteFavouritesXML();
                }
            }

        }

        public void Deserialize() {
            if (FavouritesXMLExists()) {
                XMLUtils.DeserializeListString(favouritesList, path);
            }
        }
    }
}
