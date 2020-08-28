using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BagOfTricks.Extensions;

using UnityModManager = UnityModManagerNet.UnityModManager;
using BagOfTricks.Favourites;

namespace BagOfTricks {
    public static class SettingsBoT {
        private static Settings settings = Main.settings;
        private static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;

        private static string favouritesToggleXML = Favourites.FavouriteXMLFile.Toggles.GetDescription();
        private static FavouritesManager functionsManager = FavouritesFactory.GetFavouriteFunctions;


        public static void Update() {

            CheckNewCheatCategoryElement("SpawnUnitsMenu");
            CheckNewCheatCategoryElement("BeneathTheStolenLands");

            if (CompareVersionStrings(settings.modVersion, "1.9.0") == -1) {
                if (File.Exists(Storage.modEntryPath + Storage.favouritesFolder + "\\" + favouritesToggleXML)) {
                    try {
                        File.Move(Storage.modEntryPath + Storage.favouritesFolder + "\\" + favouritesToggleXML, functionsManager.GetPath);
                        modLogger.Log($"{favouritesToggleXML} {Strings.GetText("message_RenamedTo")} {functionsManager.GetFileName}.");

                    }
                    catch (Exception e) {

                        modLogger.Log(e.ToString());
                    }
                }
            }
            if (CompareVersionStrings(settings.modVersion, "1.14.2") == -1) {
                if (functionsManager.FavouritesList.Contains("toggleArmourChecksPenalty0,buttonToggle_ArmourChecksPenalty0,tooltip_ArmourChecksPenalty0")) {
                    modLogger.Log("toggleArmourChecksPenalty0 - favourites update start");
                    modLogger.Log($"--- {functionsManager.FavouritesList.Count}");
                    functionsManager.FavouritesList.RemoveAll(IsToggleArmourChecksPenalty0);
                    modLogger.Log($"--- {functionsManager.FavouritesList.Count}");
                    functionsManager.FavouritesList.Add("ArmourChecksPenalty0");
                    modLogger.Log($"--- {functionsManager.FavouritesList.Count}");
                    modLogger.Log("toggleArmourChecksPenalty0 - favourites update end");
                }
                if (functionsManager.FavouritesList.Contains("toggleArcaneSpellFailureRoll,buttonToggle_NoArcaneSpellFailure,tooltip_NoArcaneSpellFailure")) {
                    modLogger.Log("toggleArcaneSpellFailureRoll - favourites update start");
                    modLogger.Log($"--- {functionsManager.FavouritesList.Count}");
                    functionsManager.FavouritesList.RemoveAll(IsToggleArcaneSpellFailureRoll);
                    modLogger.Log($"--- {functionsManager.FavouritesList.Count}");
                    functionsManager.FavouritesList.Add("ArcaneSpellFailureRoll");
                    modLogger.Log($"--- {functionsManager.FavouritesList.Count}");
                    modLogger.Log("toggleArcaneSpellFailureRoll - favourites update end");
                }
            }
        }

        public static bool IsEarlierVersion(string modEntryVersion) {
            if (CompareVersionStrings(settings.modVersion, modEntryVersion) == -1) {
                return true;
            }
            else {
                return false;
            }
        }
        private static int CompareVersionStrings(string v1, string v2) {
            return new Version(v1).CompareTo(new Version(v2));
        }


        private static void CheckNewCheatCategoryElement(string newCategory) {
            if (!settings.cheatsCategories.Contains(newCategory)) {
                List<string> temp = settings.cheatsCategories.ToList();
                temp.Add(newCategory);
                settings.cheatsCategories = temp.ToArray();
                modLogger.Log("cheatsCategories\n+" + newCategory);
            }
        }

        private static bool IsToggleArmourChecksPenalty0(string s) {
            if (s == "toggleArmourChecksPenalty0,buttonToggle_ArmourChecksPenalty0,tooltip_ArmourChecksPenalty0") {
                return true;
            }
            return false;
        }
        private static bool IsToggleArcaneSpellFailureRoll(string s) {
            if (s == "toggleArcaneSpellFailureRoll,buttonToggle_NoArcaneSpellFailure,tooltip_NoArcaneSpellFailure") {
                return true;
            }
            return false;
        }
    }
}
