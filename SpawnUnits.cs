using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Kingmaker.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;

using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    // to-do: move buff, feats, items and abilities over to a similar structure
    public static class SpawnUnits
    {
        private static Settings settings = Main.settings;
        private static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;
        private static List<string> unitsFavourites = new List<string>();
        public static List<string> UnitsFavourites { get => unitsFavourites; set => unitsFavourites = value; }
        private static List<string> favouritesGuids = new List<string>();
        public static List<string> FavouritesGuids { get => favouritesGuids; set => favouritesGuids = value; }
        public static readonly string favouritesFile = "Units.xml";
        public static string UnitSetsFolder { get; } = "UnitSets";

        public static void RenderMenu()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showSpawnUnitsCategory = GL.Toggle(settings.showSpawnUnitsCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_SpawnUnits")), GL.ExpandWidth(false), GL.ExpandWidth(false));
            if (!settings.showSpawnUnitsCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("SpawnUnitsMenu");
                SearchMenu();
            }
            GL.EndVertical();
        }

        private static List<bool> toggleResultDescription = new List<bool>();

        private static void SearchMenu()
        {
            FavouritesMenu();
            CustomSetsMenu();
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.unitSearch = GL.TextField(settings.unitSearch, GL.Width(400f));
            GL.EndHorizontal();
            GL.BeginHorizontal();

            if (GL.Button(RichText.Bold(Strings.GetText("header_Search")), GL.Width(400f)))
            {
                SearchValidUnits(settings.unitSearch, settings.unitSearchFilters);
            }

            GL.EndHorizontal();
            GL.BeginHorizontal();
            GL.Label(Strings.GetText("label_SearchBy") + ": ", GL.ExpandWidth(false));
            MenuTools.FilterButton(ref settings.toggleSearchByUnitObjectName, "label_ObjectName", new Action<string, string>(SearchValidUnits), settings.unitSearch, settings.unitSearchFilters);
            MenuTools.FilterButton(ref settings.toggleSearchByUnitCharacterName, "label_UnitName", new Action<string, string>(SearchValidUnits), settings.unitSearch, settings.unitSearchFilters);
            MenuTools.FilterButton(ref settings.toggleSearchByUnitType, "label_UnitType", new Action<string, string>(SearchValidUnits), settings.unitSearch, settings.unitSearchFilters);
            GL.EndHorizontal();

            FilterBox("button_Filters", ref settings.unitSearchFilters, "label_UnitSearchFiltersInfo");
            StoredUnitsMenu();

            if (!searchResultGuiDs.Any())
            {
                MenuTools.SingleLineLabel(Strings.GetText("message_NoResult"));
            }
            else
            {
                if (searchResultGuiDs.Count > settings.finalResultLimit)
                {
                    MenuTools.SingleLineLabel($"{searchResultGuiDs.Count} " + Strings.GetText("label_Results"));
                    MenuTools.SingleLineLabel(Strings.GetText("label_TooManyResults_0") + $" { settings.finalResultLimit} " + Strings.GetText("label_TooManyResults_1"));
                    GL.Space(10);
                }
                else
                {
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_Results") + $" ({searchResultGuiDs.Count()}):"));
                    for (var i = 0; i < searchResultGuiDs.Count; i++)
                    {

                        GL.BeginHorizontal();
                        toggleResultDescription.Add(false);
                        toggleResultDescription[i] = GL.Toggle(toggleResultDescription[i], $"{Strings.CleanName(searchResultObjectNames[i])} ({searchResultCr[i]})", GL.ExpandWidth(false));

                        MenuTools.AddRemoveButtonShiftAdd(ref storedGuiDs, searchResultGuiDs[i], ref refreshStoredGuiDs, "<b>-</b>", "<b>+</b>", Strings.GetText("tooltip_AddMultipleToStorage"));
                        MenuTools.AddRemoveButton(ref unitsFavourites, searchResultGuiDs[i], ref favouritesLoad, Storage.favouriteTrueString, Storage.favouriteFalseString);
                        GL.EndHorizontal();
                        UnitDetails(toggleResultDescription[i], searchResultGuiDs[i]);
                    }

                    GL.BeginHorizontal();
                    if (GL.Button(Strings.GetText("label_AddResultsToStoredUnits") + $" ({searchResultGuiDs.Count()})", GL.ExpandWidth(false)))
                    {
                        foreach (var guid in searchResultGuiDs.Where(guid => !storedGuiDs.Contains(guid)))
                        {
                            storedGuiDs.Add(guid);
                        }

                        refreshStoredGuiDs = true;
                    }
                    GL.EndHorizontal();

                    var filename = "unit-";
                    var searchName = Regex.Replace(settings.unitSearch, @"[\\/:*?""<>|]", "");
                    if (!string.IsNullOrWhiteSpace(searchName))
                    {
                        filename += searchName;
                    }
                    else
                    {
                        filename += "search-results";
                    }
                    MenuTools.ExportCopyGuidsNamesButtons(searchResultGuiDs.ToArray(), searchResultObjectNames.ToArray(), filename);
                }
            }
            GL.EndVertical();
        }

        private static string[] unitSetsCsv = Array.Empty<string>();
        private static string[] unitSetsTxt = Array.Empty<string>();
        public static string[] UnitSetsCsv { get => unitSetsCsv; set => unitSetsCsv = value; }
        public static string[] UnitSetsTxt { get => unitSetsTxt; set => unitSetsTxt = value; }
        private static List<bool> toggleUnitsetsPreviewCsv = new List<bool>();
        private static List<string> previewUnitsetsStringsCsv = new List<string>();
        private static List<bool> toggleUnitseitsPreviewTxt = new List<bool>();
        private static List<string> previewUnitsetisStringsTxt = new List<string>();

        private static void CustomSetsMenu()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showUnitSets = GL.Toggle(settings.showUnitSets, RichText.Bold(Strings.GetText("headerOption_ShowUnitSets")), GL.ExpandWidth(false));
            GL.EndHorizontal();
            if (settings.showUnitSets == true)
            {
                StoredUnitsMenu();
                GL.BeginHorizontal();
                if (GL.Button(Strings.GetText("button_LoadRefresh"), GL.ExpandWidth(false)))
                {
                    LoadRefreshArraysFromFiles();
                }
                GL.EndHorizontal();

                if (settings.settingSearchForCsv)
                {
                    GL.Space(5);
                    GetCustomUnitSets(UnitSetsCsv, previewUnitsetsStringsCsv, toggleUnitsetsPreviewCsv);
                }

                if (settings.settingSearchForTxt)
                {
                    GL.Space(5);
                    GetCustomUnitSets(UnitSetsTxt, previewUnitsetisStringsTxt, toggleUnitseitsPreviewTxt);
                }
            }
            GL.EndVertical();
        }

        private static void LoadRefreshArraysFromFiles()
        {
            try
            {
                if (settings.settingSearchForCsv)
                {
                    Common.RefreshArrayFromFile(ref unitSetsCsv, SpawnUnits.UnitSetsFolder, "csv");
                }
                if (settings.settingSearchForTxt)
                {
                    Common.RefreshArrayFromFile(ref unitSetsTxt, SpawnUnits.UnitSetsFolder, "txt");
                }
            }
            catch (IOException exception)
            {
                modLogger.Log(exception.ToString());
            }
        }

        private static void GetCustomUnitSets(string[] files, List<string> previewStrings, List<bool> togglePreview)
        {
            for (var i = 0; i < files.Length; i++)
            {
                if (previewStrings.Count == 0)
                {
                    for (var ii = 0; ii < files.Length; ii++)
                    {
                        previewStrings.Add("");
                    }

                }
                GL.BeginVertical("box");
                GL.BeginHorizontal();
                var lines = Array.Empty<string>();
                try
                {
                    lines = File.ReadAllLines(files[i]);

                }
                catch (FileNotFoundException e)
                {
                    modLogger.Log(e.ToString());
                    LoadRefreshArraysFromFiles();
                    break;
                }
                togglePreview.Add(false);
                if (GL.Button(Strings.GetText("misc_Add") + $" {Path.GetFileNameWithoutExtension(files[i])}", GL.ExpandWidth(false)))
                {
                    for (var ii = 0; ii < lines.Length; ii++)
                    {
                        lines[ii] = lines[ii].Trim();
                        if (lines[ii].Contains(",") && lines[ii].IndexOf(",") == lines[ii].LastIndexOf(","))
                        {
                            var splitLine = lines[ii].Split(new string[] { "," }, StringSplitOptions.None);
                            splitLine[0] = splitLine[0].Trim();
                            splitLine[1] = splitLine[1].Trim();

                            if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && int.TryParse(splitLine[1], out var iTest) && splitLine[1] != "0")
                            {
                                for (var amount = 0; amount < int.Parse(splitLine[1]); amount++)
                                {
                                    storedGuiDs.Add(splitLine[0]);
                                    refreshStoredGuiDs = true;
                                }                                

                            }
                            else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && splitLine[1] == "")
                            {
                                storedGuiDs.Add(splitLine[0]);
                                refreshStoredGuiDs = true;
                            }
                        }
                        else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]) != null)
                        {
                            storedGuiDs.Add(lines[ii]);
                            refreshStoredGuiDs = true;
                        }

                    }
                }
                if (GL.Button(Strings.GetText("button_Preview"), GL.ExpandWidth(false)))
                {
                    if (!togglePreview[i])
                    {
                        togglePreview[i] = true;
                        for (var ii = 0; ii < lines.Length; ii++)
                        {
                            lines[ii] = lines[ii].Trim();

                            if (lines[ii].Contains(",") && lines[ii].IndexOf(",") == lines[ii].LastIndexOf(","))
                            {
                                var splitLine = lines[ii].Split(new string[] { "," }, StringSplitOptions.None);
                                splitLine[0] = splitLine[0].Trim();
                                splitLine[1] = splitLine[1].Trim();
                                var unitByGuid = Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]);

                                if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && int.TryParse(splitLine[1], out var iTest) && splitLine[1] != "0")
                                {
                                    if (ii == 0)
                                    {
                                        previewStrings[i] = previewStrings[i] + splitLine[1] + "x " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                    }
                                    else
                                    {
                                        previewStrings[i] = previewStrings[i] + ", " + splitLine[1] + "x " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                    }
                                }
                                else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && splitLine[1] == "")
                                {
                                    if (ii == 0)
                                    {
                                        previewStrings[i] = previewStrings[i] + "1x " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                    }
                                    else
                                    {
                                        previewStrings[i] = previewStrings[i] + ", " + "1x " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                    }
                                }
                            }
                            else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]) != null)
                            {
                                var unitByGuid = Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]);
                                if (ii == 0)
                                {
                                    previewStrings[i] = previewStrings[i] + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                }
                                else
                                {
                                    previewStrings[i] = previewStrings[i] + ", " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                }
                            }
                        }
                    }
                    else
                    {
                        togglePreview[i] = false;
                        previewStrings[i] = "";
                    }
                }
                GL.FlexibleSpace();
                if (GL.Button(Strings.GetText("button_AddToFavourites"), GL.ExpandWidth(false)))
                {
                    for (var ii = 0; ii < lines.Length; ii++)
                    {
                        lines[ii] = lines[ii].Trim();
                        if (lines[ii].Contains(",") && lines[ii].IndexOf(",") == lines[ii].LastIndexOf(","))
                        {
                            var splitLine = lines[ii].Split(new string[] { "," }, StringSplitOptions.None);
                            splitLine[0] = splitLine[0].Trim();
                            splitLine[1] = splitLine[1].Trim();

                            if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && int.TryParse(splitLine[1], out var iTest) && splitLine[1] != "0")
                            {
                                if (!UnitsFavourites.Contains(splitLine[0]))
                                {
                                    UnitsFavourites.Add(splitLine[0]);
                                }
                            }
                            else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && splitLine[1] == "")
                            {
                                if (!UnitsFavourites.Contains(splitLine[0]))
                                {
                                    UnitsFavourites.Add(splitLine[0]);
                                }
                            }
                        }
                        else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]) != null)
                        {
                            if (!UnitsFavourites.Contains(lines[ii]))
                            {
                                UnitsFavourites.Add(lines[ii]);
                            }
                        }
                    }
                    RefreshUnitFavourites();
                }
                if (GL.Button(Strings.GetText("button_RemoveFromFavourites"), GL.ExpandWidth(false)))
                {
                    for (var ii = 0; ii < lines.Length; ii++)
                    {
                        lines[ii] = lines[ii].Trim();
                        if (lines[ii].Contains(",") && lines[ii].IndexOf(",") == lines[ii].LastIndexOf(","))
                        {
                            var splitLine = lines[ii].Split(new string[] { "," }, StringSplitOptions.None);
                            splitLine[0] = splitLine[0].Trim();
                            splitLine[1] = splitLine[1].Trim();

                            if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && int.TryParse(splitLine[1], out var iTest) && splitLine[1] != "0")
                            {
                                if (UnitsFavourites.Contains(splitLine[0]))
                                {
                                    UnitsFavourites.Remove(splitLine[0]);

                                }
                            }
                            else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && splitLine[1] == "")
                            {
                                if (UnitsFavourites.Contains(splitLine[0]))
                                {
                                    UnitsFavourites.Remove(splitLine[0]);
                                }
                            }
                        }
                        else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]) != null)
                        {
                            if (UnitsFavourites.Contains(lines[ii]))
                            {
                                UnitsFavourites.Remove(lines[ii]);
                            }
                        }
                    }
                    RefreshUnitFavourites();
                }
                GL.EndHorizontal();
                GL.EndVertical();
                if (!togglePreview[i]) continue;
                MenuTools.SingleLineLabel(previewStrings[i] != ""
                    ? previewStrings[i]
                    : Strings.GetText("message_NoValidEntriesFound"));
            }
        }

        private static List<string> storedGuiDs = new List<string>();
        public static List<string> GetStoredGuiDs { get => storedGuiDs;}
        private static string storedUnitsLabel = "";
        private static bool refreshStoredGuiDs = true;
        private static string[] unitStateStrings = { Strings.GetText("misc_Friendly"), Strings.GetText("misc_Passive"), Strings.GetText("misc_Hostile") };
        private static SelectionGrid unitStateGrid = new SelectionGrid(unitStateStrings, 3);

        private static void StoredUnitsMenu(bool actionKeyMenu = false)
        {
            if (refreshStoredGuiDs)
            {
                storedUnitsLabel = RichText.Bold(Strings.GetText("label_StoredUnits") + $" ({storedGuiDs.Count()}): ");
                foreach (var nameCr in storedGuiDs.Select(s => $"{Strings.CleanName(Utilities.GetBlueprintByGuid<BlueprintUnit>(s).name)} ({Utilities.GetBlueprintByGuid<BlueprintUnit>(s).CR})"))
                {
                    storedUnitsLabel = storedUnitsLabel + nameCr + ", ";
                }
                storedUnitsLabel = storedUnitsLabel.TrimEnd(new char[] { ',', ' ' });
                refreshStoredGuiDs = false;
            }
            GL.BeginVertical("box");
            MenuTools.SingleLineLabel(storedUnitsLabel);
            if (storedGuiDs.Any())
            {
                GL.BeginHorizontal();

                if (GL.Button(Strings.GetText("label_ClearStoredUnits"), GL.ExpandWidth(false)))
                {
                    storedGuiDs.Clear();
                    refreshStoredGuiDs = true;
                }
                if (GL.Button(RichText.Bold(Strings.GetText("button_Spawn")), GL.ExpandWidth(false)))
                {
                    var playerPos = Game.Instance.Player.MainCharacter.Value.Position;
                    var angle = 0f;
                    foreach (var guid in GetStoredGuiDs)
                    {
                        var posXy = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)) * GetStoredGuiDs.Count() * 0.5f;
                        var finalPos = new Vector3(posXy.x + playerPos.x, playerPos.y, playerPos.z + posXy.y);
                        UnitSpawner(finalPos, guid);
                        angle++;
                    }
                }
                unitStateGrid.RenderNoGroup();
                GL.EndHorizontal();
                if (!actionKeyMenu)
                {
                    if(!Strings.ToBool(settings.toggleEnableActionKey))
                    {
                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_EnableActionKeyToSpawnHotkey") + ": ", GL.ExpandWidth(false));
                        ActionKey.MainToggle(7);
                        GL.EndHorizontal();
                    }
                    else if (Strings.ToBool(settings.toggleEnableActionKey))
                    {
                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        if (settings.actionKeyIndex != 7)
                        {
                            GL.Label(Strings.GetText("label_ActionKeyNotSetToSpawnUnits") + ": ", GL.ExpandWidth(false));
                            if(GL.Button(Strings.GetText("label_SetToSpawnUnits"),GL.ExpandWidth(false)))
                            {
                                settings.actionKeyIndex = 7;
                            }
                        }
                        else if(!Strings.ToBool(settings.toggleSpawnEnemiesFromUnitFavourites))
                        {
                            GL.Label(Strings.GetText("label_ActionKeyNotSetToSpawnUnitsFromUnitFavouritesStored") + ": ", GL.ExpandWidth(false));
                            MenuTools.ToggleButtonNoGroup(ref settings.toggleSpawnEnemiesFromUnitFavourites, "buttonToggle_ActionKeySpawnUnitsFromUnitFavourites", "tooltip_ActionKeySpawnUnitsFromUnitFavourites");
                        }
                        else
                        {
                            GL.Label(Strings.GetText("label_ActionKey") + ": ", GL.ExpandWidth(false));
                            MenuTools.SetKeyBinding(ref settings.actionKey);
                        }
                        GL.EndHorizontal();
                        GL.EndVertical();

                    }
                }
            }
            GL.EndVertical();
        }

        public static void UnitSpawner(Vector3 finalPos, string guid)
        {
            switch (unitStateGrid.selected)
            {
                case 0:
                    Common.SpawnFriendlyUnit(finalPos, guid);
                    break;
                case 1:
                    Common.SpawnPassiveUnit(finalPos, guid);
                    break;
                case 2:
                    Common.SpawnHostileUnit(finalPos, guid);
                    break;
                default:
                    modLogger.Log("unitStateGrid index not regcognised!");
                    break;
            }
        }

        private static bool favouritesLoad = true;                
        private static List<string> favouriteObjectNamesClean = new List<string>();
        private static List<int> favouriteCr = new List<int>();
        private static List<bool> toggleFavouriteDescription = new List<bool>();

        public static void FavouritesMenu(bool actionKeyMenu = false)
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showUnitsFavourites = GL.Toggle(settings.showUnitsFavourites, RichText.Bold(Strings.GetText("headerOption_ShowFavourites")) + " ", GL.ExpandWidth(false));
            GL.EndHorizontal();
            if (settings.showUnitsFavourites == true)
            {
                if (favouritesLoad == true)
                {
                    RefreshUnitFavourites();
                    favouritesLoad = false;
                }

                if (!unitsFavourites.Any())
                {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                }
                else
                {
                    StoredUnitsMenu(actionKeyMenu);

                    for (var i = 0; i < unitsFavourites.Count; i++)
                    {

                        GL.BeginHorizontal();

                        toggleFavouriteDescription.Add(false);
                        toggleFavouriteDescription[i] = GL.Toggle(toggleFavouriteDescription[i], $"{favouriteObjectNamesClean[i]} ({favouriteCr[i]})", GL.ExpandWidth(false));

                        MenuTools.AddRemoveButtonShiftAdd(ref storedGuiDs, favouritesGuids[i], ref refreshStoredGuiDs, "<b>-</b>", "<b>+</b>", Strings.GetText("tooltip_AddMultipleToStorage"));

                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                        {
                            favouritesGuids.Remove(favouritesGuids[i]);
                            favouritesLoad = true;
                        }
                        GL.EndHorizontal();

                        UnitDetails(toggleFavouriteDescription[i], favouritesGuids[i]);
                    }
                    
                    GL.Space(10);
                    GL.BeginHorizontal();
                    if (GL.Button(Strings.GetText("label_AddFavouritesToStoredUnits") + $" ({favouritesGuids.Count()})", GL.ExpandWidth(false)))
                    {
                        foreach (var guid in favouritesGuids.Where(guid => !storedGuiDs.Contains(guid)))
                        {
                            storedGuiDs.Add(guid);
                        }

                        refreshStoredGuiDs = true;
                    }
                    GL.EndHorizontal();

                    MenuTools.ExportCopyGuidsNamesButtons(favouritesGuids.ToArray(), favouriteObjectNamesClean.ToArray(), "unit-favourites");
                }

                if (unitsFavourites != favouritesGuids)
                {
                    unitsFavourites = favouritesGuids;
                }
            }
            GL.EndVertical();
        }

        private static void RefreshUnitFavourites()
        {
            Common.SerializeListString(unitsFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + favouritesFile);

            favouritesGuids = unitsFavourites;
            favouriteObjectNamesClean.Clear();
            favouriteCr.Clear();
            toggleFavouriteDescription.Clear();
            for (var i = 0; i < favouritesGuids.Count; i++)
            {
                var stringUnitObjectName = Utilities.GetBlueprintByGuid<BlueprintUnit>(unitsFavourites[i]).name;
                favouriteObjectNamesClean.Add(Strings.CleanName(stringUnitObjectName));
                var intUnitCr = Utilities.GetBlueprintByGuid<BlueprintUnit>(unitsFavourites[i]).CR;
                favouriteCr.Add(intUnitCr);
            }
        }

        private static bool filterError = false;
        private static SimpleShowHideButton hideFilterButton = new SimpleShowHideButton(true);

        private static void FilterBox(string label, ref string textFieldString, string message, float width = 500f)
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();            
            GL.Label(Strings.GetText(label) + ": ", GL.ExpandWidth(false));
            textFieldString = GL.TextField(textFieldString, GL.Width(width));
            hideFilterButton.Render();
            GL.EndHorizontal();

            if (hideFilterButton.GetButtonText == Strings.GetText("misc_Hide"))
            {
                MenuTools.SingleLineLabelGt(message);
                if (filterError)
                {
                    MenuTools.SingleLineLabel(RichText.BoldRedFormat(Strings.GetText("warning_FilterPatternError1")));
                    MenuTools.SingleLineLabel(RichText.BoldRedFormat(Strings.GetText("warning_FilterPatternError2")));

                }
            }

            GL.EndVertical();
        }

        private static List<string> searchResultGuiDs = new List<string>();
        private static List<string> searchResultObjectNames = new List<string>();
        private static List<int> searchResultCr = new List<int>();

        private static void SearchValidUnits(string search, string filters)
        {
            filterError = false;
            searchResultGuiDs.Clear();
            searchResultObjectNames.Clear();
            searchResultCr.Clear();
            toggleResultDescription.Clear();
            var blueprintUnits = ResourcesLibrary.GetBlueprints<BlueprintUnit>();
            foreach (var unit in blueprintUnits)
            {
                var unitAdded = false;
                var guid = unit.AssetGuid;
                if (Strings.ToBool(settings.toggleSearchByUnitObjectName))
                {
                    if(unit.name != null)
                    {
                        if(unit.name.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!searchResultGuiDs.Contains(guid))
                            {
                                AddResult(unit);
                                unitAdded = true;
                            }
                        }
                    }
                }
                else if (Strings.ToBool(settings.toggleSearchByUnitCharacterName))
                {
                    if (unit.CharacterName != null)
                    {
                        if (unit.CharacterName.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!searchResultGuiDs.Contains(guid))
                            {
                                AddResult(unit);
                                unitAdded = true;
                            }
                        }
                    }
                }
                else if(Strings.ToBool(settings.toggleSearchByUnitType))
                {
                    if (unit.Type?.Name != null)
                    {
                        if (unit.Type.Name.ToString().Contains(search, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!searchResultGuiDs.Contains(guid))
                            {
                                AddResult(unit);
                                unitAdded = true;
                            }
                        }
                    }
                }
                if(!string.IsNullOrEmpty(filters) && !filterError && unitAdded)
                {
                    FilterBoxParse(filters, unit);
                }
            }
        }

        private static void FilterBoxParse(string filters, BlueprintUnit unit)
        {
            // filters: cr(i|i,i,i|i-i|<i|>i), type(s), race(s), gender('m'|'f')
            filters = filters.ToLower();
            filters = Regex.Replace(filters, @"s", "");
            var correctFilter = new Regex(@"^[A-Za-z0-9,:<>;!-]*$");
            if (correctFilter.IsMatch(filters))
            {               
                var filterArray = filters.Split(';');
                
                foreach (var filter in filterArray)
                {
                    var filterValue = filter.Split(':');

                    if (filterValue.Length != 2)
                    {
                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown filter format: {filter}");
                        filterError = true;
                        return;
                    }
                    else
                    {
                        var type = filterValue[0];
                        var value = filterValue[1];
                        var not = false;
                        if(type.Remove(1) == "!")
                        {
                            not = true;
                            type = type.Remove(0,1);
                        }

                        if (type == "cr")
                        {
                            if (value.Contains(","))
                            {
                                var multipleCRs = value.Split(',');
                                var crsValue = new List<int>();
                                foreach (var cr in multipleCRs)
                                {
                                    if (int.TryParse(cr, out var crOut))
                                    {
                                        crsValue.Add(crOut);
                                    }
                                    else
                                    {
                                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {cr}");
                                        filterError = true;
                                        return;
                                    }
                                }
                                var check = !crsValue.Contains(unit.CR);
                                if (not)
                                {
                                    check = !check;
                                }

                                if (!check) continue;
                                RemoveResult(unit);
                                return;
                            }
                            else if (value.Contains("-"))
                            {
                                var fromTo = value.Split('-');
                                if (fromTo.Length != 2)
                                {
                                    modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown value format (expected i-i): {value}");
                                    filterError = true;
                                    return;
                                }
                                else
                                {
                                    int from;
                                    int to;
                                    if (int.TryParse(fromTo[0], out var fromOut))
                                    {
                                        from = fromOut;
                                        if (int.TryParse(fromTo[1], out var toOut))
                                        {
                                            to = toOut;
                                            var check = unit.CR < from || unit.CR > to;
                                            if (not)
                                            {
                                                check = !check;
                                            }

                                            if (!check) continue;
                                            RemoveResult(unit);
                                            return;
                                        }
                                        else
                                        {
                                            modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {fromTo[1]}");
                                            filterError = true;
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {fromTo[0]}");
                                        filterError = true;
                                        return;
                                    }
                                }
                            }
                            else if (value.Contains("<"))
                            {
                                var lesser = value.Split('<');
                                if (!string.IsNullOrEmpty(lesser[0]))
                                {
                                    modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown value format (expected <i): {value}");
                                    filterError = true;
                                    return;
                                }
                                else
                                {
                                    if (int.TryParse(lesser[1], out var lesserOut))
                                    {
                                        var check = unit.CR >= lesserOut;
                                        if (not)
                                        {
                                            check = !check;
                                        }

                                        if (!check) continue;
                                        RemoveResult(unit);
                                        return;
                                    }
                                    else
                                    {
                                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {lesser[1]}");
                                        filterError = true;
                                        return;
                                    }
                                }
                            }
                            else if (value.Contains(">"))
                            {
                                var geater = value.Split('>');
                                if (!string.IsNullOrEmpty(geater[0]))
                                {
                                    modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown value format (expected >i): {value}");
                                    filterError = true;
                                    return;
                                }
                                else
                                {
                                    if (int.TryParse(geater[1], out var greaterOut))
                                    {
                                        var check = unit.CR <= greaterOut;
                                        if (not)
                                        {
                                            check = !check;
                                        }

                                        if (!check) continue;
                                        RemoveResult(unit);
                                        return;
                                    }
                                    else
                                    {
                                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {geater[1]}");
                                        filterError = true;
                                        return;
                                    }
                                }
                            }
                            else if (int.TryParse(value, out var singleCr))
                            {
                                var check = unit.CR != singleCr;
                                if (not)
                                {
                                    check = !check;
                                }

                                if (!check) continue;
                                RemoveResult(unit);
                                return;
                            }
                            else
                            {
                                modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown cr format: {value}");
                                filterError = true;
                                return;
                            }

                        }
                        else if (type == "type")
                        {
                            if (unit.Type?.Name == null)
                            {
                                RemoveResult(unit);
                                return;
                            }
                            else if(value.Contains(","))
                            {
                                var types = value.Split(',');
                                var check = !types.Any(s => unit.Type.Name.ToString().Contains(s, StringComparison.CurrentCultureIgnoreCase));
                                if(not)
                                {
                                    check = !check;
                                }

                                if (!check) continue;
                                RemoveResult(unit);
                                return;
                            }
                            else
                            {
                                var check = !unit.Type.Name.ToString().Contains(value, StringComparison.CurrentCultureIgnoreCase);
                                if (not)
                                {
                                    check = !check;
                                }

                                if (!check) continue;
                                RemoveResult(unit);
                                return;
                            }
                        }
                        else if (type == "race")
                        {
                            if (unit.Race?.Name == null)
                            {
                                RemoveResult(unit);
                                return;
                            }
                            else if (value.Contains(","))
                            {
                                var races = value.Split(',');

                                var check = !races.Any(s => unit.Race.Name.Contains(s, StringComparison.CurrentCultureIgnoreCase));
                                if (not)
                                {
                                    check = !check;
                                }

                                if (!check) continue;
                                RemoveResult(unit);
                                return;
                            }
                            else
                            {
                                var check = !unit.Race.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase);
                                if (not)
                                {
                                    check = !check;
                                }

                                if (!check) continue;
                                RemoveResult(unit);
                                return;
                            }
                        }
                        else if (type == "gender")
                        {
                            switch (value)
                            {
                                case "f":
                                {
                                    var check = unit.Gender != Gender.Female;
                                    if (not)
                                    {
                                        check = !check;
                                    }
                                    if (check)
                                    {
                                        RemoveResult(unit);
                                        return;
                                    }

                                    break;
                                }
                                case "m":
                                {
                                    var check = unit.Gender != Gender.Male;
                                    if (not)
                                    {
                                        check = !check;
                                    }
                                    if (check)
                                    {
                                        RemoveResult(unit);
                                        return;
                                    }

                                    break;
                                }
                                default:
                                    modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError] {filters}" + $"\n[FilterError] Neither m nor f: {value}");
                                    filterError = true;
                                    return;
                            }
                        }
                        else
                        {
                            modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError] {filters}" + $"\n[FilterError] Unknown filter: {type}");
                            filterError = true;
                            return;
                        }
                    }
                }
            }
            else
            {
                modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError] {filters}" + "\n[FilterError Does not match: ^[A-Za-z0-9,:<>;-]*$");
                filterError = true;
                return;
            }
        }

        private static void AddResult(BlueprintUnit unit)
        {
            searchResultGuiDs.Add(unit.AssetGuid);
            searchResultObjectNames.Add(unit.name ?? "No Object Name!");
            searchResultCr.Add(unit.CR);
        }

        private static void RemoveResult(BlueprintUnit unit)
        {
            var i = searchResultGuiDs.IndexOf(unit.AssetGuid);
            searchResultGuiDs.RemoveAt(i);
            searchResultObjectNames.RemoveAt(i);
            searchResultCr.RemoveAt(i);
        }

        private static void UnitDetails(bool toggle, string unitGuid)
        {
            if (!toggle) return;
            GL.BeginVertical("box");

            var unitByGuid = Utilities.GetBlueprintByGuid<BlueprintUnit>(unitGuid);
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_UnitName") + ": ") + $"{unitByGuid.CharacterName}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_UnitGUID") + ": ") + $"{unitByGuid.AssetGuid}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectName") + ": ") + $"{unitByGuid.name}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectNameClean") + ": ") + $"{Strings.CleanName(unitByGuid.name)}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ChallengeRating") + ": ") + $"{unitByGuid.CR}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_UnitType") + ": ") + $"{unitByGuid.Type?.Name}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_Race") + ": ") + $"{unitByGuid.Race?.Name}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_Gender") + ": ") + $"{unitByGuid.Gender}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("charStat_Strength") + ": ") + $"{unitByGuid.Strength}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("charStat_Dexterity") + ": ") + $"{unitByGuid.Dexterity}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("charStat_Constitution") + ": ") + $"{unitByGuid.Constitution}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("charStat_Intelligence") + ": ") + $"{unitByGuid.Intelligence}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("charStat_Wisdom") + ": ") + $"{unitByGuid.Wisdom}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("charStat_Charisma") + ": ") + $"{unitByGuid.Charisma}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("charStat_Speed") + ": ") + $"{unitByGuid.Speed}");
            MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("charStat_BaseAttackBonus") + ": ") + $"{unitByGuid.BaseAttackBonus}");            
                                             
            MenuTools.CopyExportButtons("button_ExportUnitInfo", unitByGuid.name + ".txt", UnitInfo(unitByGuid),  "label_CopyUnitInformationToClipboard");
            GL.EndVertical();
        }

        private static string[] UnitInfo(BlueprintUnit unit)
        {
            return new string[]
            {
                Strings.GetText("label_UnitName") + ": " + $"{unit.CharacterName}",
                Strings.GetText("label_UnitGUID") + ": " + $"{unit.AssetGuid}",
                Strings.GetText("label_ObjectName") + ": " + $"{unit.name}",
                Strings.GetText("label_ObjectNameClean") + ": " + $"{Strings.CleanName(unit.name)}",
                Strings.GetText("label_ChallengeRating") + ": " + $"{unit.CR}",
                Strings.GetText("label_UnitType") + ": " + $"{unit.Type?.Name}",
                Strings.GetText("label_Race") + ": " + $"{unit.Race?.Name}",
                Strings.GetText("label_Gender") + ": " + $"{unit.Gender}",
                Strings.GetText("charStat_Strength") + ": " + $"{unit.Strength}",
                Strings.GetText("charStat_Dexterity") + ": " + $"{unit.Dexterity}",
                Strings.GetText("charStat_Constitution") + ": " + $"{unit.Constitution}",
                Strings.GetText("charStat_Intelligence") + ": " + $"{unit.Intelligence}",
                Strings.GetText("charStat_Wisdom") + ": " + $"{unit.Wisdom}",
                Strings.GetText("charStat_Charisma") + ": " + $"{unit.Charisma}",
                Strings.GetText("charStat_Speed") + ": " + $"{unit.Speed}",
                Strings.GetText("charStat_BaseAttackBonus") + ": " + $"{unit.BaseAttackBonus}",
            };
        }
    }
}
