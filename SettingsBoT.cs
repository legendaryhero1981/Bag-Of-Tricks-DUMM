using System;
using System.IO;
using System.Linq;

using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    public static class SettingsBoT
    {
        public static Settings settings = Main.Settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.ModLogger;

        public static void Update()
        {
            CheckNewCheatCategoryElement("SpawnUnitsMenu");
            CheckNewCheatCategoryElement("BeneathTheStolenLands");

            if (CompareVersionStrings(settings.modVersion, "1.9.0") == -1)
            {
                if (File.Exists(Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFileOld))
                {
                    try
                    {
                        File.Move(Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFileOld, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFile);
                        modLogger.Log($"{Storage.favouritesTogglesFileOld} {Strings.GetText("message_RenamedTo")} {Storage.favouritesTogglesFile}.");
                    }
                    catch (Exception e)
                    {
                        modLogger.Log(e.ToString());
                    }
                }
            }

            if (CompareVersionStrings(settings.modVersion, "1.14.2") != -1) return;
            if (Storage.togglesFavourites.Contains("toggleArmourChecksPenalty0,buttonToggle_ArmourChecksPenalty0,tooltip_ArmourChecksPenalty0"))
            {
                modLogger.Log("toggleArmourChecksPenalty0 - favourites update start");
                modLogger.Log($"--- {Storage.togglesFavourites.Count}");
                Storage.togglesFavourites.RemoveAll(IsToggleArmourChecksPenalty0);
                modLogger.Log($"--- {Storage.togglesFavourites.Count}");
                Storage.togglesFavourites.Add("ArmourChecksPenalty0");
                modLogger.Log($"--- {Storage.togglesFavourites.Count}");
                modLogger.Log("toggleArmourChecksPenalty0 - favourites update end");
            }

            if (!Storage.togglesFavourites.Contains("toggleArcaneSpellFailureRoll,buttonToggle_NoArcaneSpellFailure,tooltip_NoArcaneSpellFailure")) return;
            modLogger.Log("toggleArcaneSpellFailureRoll - favourites update start");
            modLogger.Log($"--- {Storage.togglesFavourites.Count}");
            Storage.togglesFavourites.RemoveAll(IsToggleArcaneSpellFailureRoll);
            modLogger.Log($"--- {Storage.togglesFavourites.Count}");
            Storage.togglesFavourites.Add("ArcaneSpellFailureRoll");
            modLogger.Log($"--- {Storage.togglesFavourites.Count}");
            modLogger.Log("toggleArcaneSpellFailureRoll - favourites update end");
        }

        public static bool IsEarlierVersion(string modEntryVersion)
        {
            return CompareVersionStrings(settings.modVersion, modEntryVersion) == -1;
        }
        private static int CompareVersionStrings(string v1, string v2)
        {
            return new Version(v1).CompareTo(new Version(v2));
        }

        private static void CheckNewCheatCategoryElement(string newCategory)
        {
            if (settings.cheatsCategories.Contains(newCategory)) return;
            var temp = settings.cheatsCategories.ToList();
            temp.Add(newCategory);
            settings.cheatsCategories = temp.ToArray();
            modLogger.Log("cheatsCategories\n+" + newCategory);
        }

        private static bool IsToggleArmourChecksPenalty0(string s)
        {
            return s == "toggleArmourChecksPenalty0,buttonToggle_ArmourChecksPenalty0,tooltip_ArmourChecksPenalty0";
        }

        private static bool IsToggleArcaneSpellFailureRoll(string s)
        {
            return s == "toggleArcaneSpellFailureRoll,buttonToggle_NoArcaneSpellFailure,tooltip_NoArcaneSpellFailure";
        }
    }
}
