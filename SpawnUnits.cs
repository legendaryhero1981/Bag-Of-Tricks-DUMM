using BagOfTricks.Favourites;
using BagOfTricks.ModUI;
using BagOfTricks.Utils;
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

namespace BagOfTricks {

    // to-do: move buff, feats, items and abilities over to a similar structure
    public static class SpawnUnits {
        private static Settings settings = Main.settings;
        private static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;
        private static List<string> unitsFavourites = FavouritesFactory.GetFavouriteUnits.FavouritesList;

        public static string UnitSetsFolder { get; } = "UnitSets";

        public static void RenderMenu() {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showSpawnUnitsCategory = GL.Toggle(settings.showSpawnUnitsCategory, RichTextUtils.MainCategoryFormat(Strings.GetText("mainCategory_SpawnUnits")), GL.ExpandWidth(false));
            if (!settings.showSpawnUnitsCategory) {
                GL.EndHorizontal();
            }
            else {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("SpawnUnitsMenu");
                SearchMenu();

            }
            GL.EndVertical();
        }

        private static List<bool> toggleResultDescription = new List<bool>();
        private static void SearchMenu() {
            FavouritesMenu();

            CustomSetsMenu();

            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.unitSearch = GL.TextField(settings.unitSearch, GL.Width(320f));
            GL.EndHorizontal();


            GL.BeginHorizontal();
            if (GL.Button(RichTextUtils.Bold(Strings.GetText("header_Search")), GL.ExpandWidth(false))) {
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

            if (!searchResultGUIDs.Any()) {
                MenuTools.SingleLineLabel(Strings.GetText("message_NoResult"));
            }
            else {
                if (searchResultGUIDs.Count > settings.finalResultLimit) {
                    MenuTools.SingleLineLabel($"{searchResultGUIDs.Count} " + Strings.GetText("label_Results"));
                    MenuTools.SingleLineLabel(Strings.GetText("label_TooManyResults_0") + $" { settings.finalResultLimit} " + Strings.GetText("label_TooManyResults_1"));
                    GL.Space(10);
                }
                else {

                    MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("label_Results") + $" ({searchResultGUIDs.Count()}):"));
                    for (int i = 0; i < searchResultGUIDs.Count; i++) {

                        GL.BeginHorizontal();
                        toggleResultDescription.Add(false);
                        toggleResultDescription[i] = GL.Toggle(toggleResultDescription[i], "", GL.ExpandWidth(false));
                        GL.Label($"{Strings.CleanName(searchResultObjectNames[i])} ({searchResultCR[i]})");

                        MenuTools.AddRemoveButtonShiftAdd(ref storedGUIDs, searchResultGUIDs[i], ref refreshStoredGUIDs, "<b>-</b>", "<b>+</b>", Strings.GetText("tooltip_AddMultipleToStorage"));
                        MenuTools.AddRemoveButton(ref unitsFavourites, searchResultGUIDs[i], ref favouritesLoad, Storage.favouriteTrueString, Storage.favouriteFalseString);
                        GL.EndHorizontal();
                        UnitDetails(toggleResultDescription[i], searchResultGUIDs[i]);
                    }

                    GL.BeginHorizontal();
                    if (GL.Button(Strings.GetText("label_AddResultsToStoredUnits") + $" ({searchResultGUIDs.Count()})", GL.ExpandWidth(false))) {
                        foreach (string guid in searchResultGUIDs) {
                            if (!storedGUIDs.Contains(guid)) {
                                storedGUIDs.Add(guid);
                            }
                        }
                        refreshStoredGUIDs = true;
                    }
                    GL.EndHorizontal();

                    string filename = "unit-";
                    string searchName = Regex.Replace(settings.unitSearch, @"[\\/:*?""<>|]", "");
                    if (!String.IsNullOrWhiteSpace(searchName)) {
                        filename = filename + searchName;
                    }
                    else {
                        filename = filename + "search-results";
                    }
                    MenuTools.ExportCopyGuidsNamesButtons(searchResultGUIDs.ToArray(), searchResultObjectNames.ToArray(), filename);
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
        private static void CustomSetsMenu() {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showUnitSets =  GL.Toggle(settings.showUnitSets, RichTextUtils.Bold(Strings.GetText("headerOption_ShowUnitSets")), GL.ExpandWidth(false));
            GL.EndHorizontal();
            if (settings.showUnitSets == true) {
                StoredUnitsMenu();
                GL.BeginHorizontal();
                if (GL.Button(Strings.GetText("button_LoadRefresh"), GL.ExpandWidth(false))) {
                    LoadRefreshArraysFromFiles();
                }
                GL.EndHorizontal();

                if (settings.settingSearchForCsv) {
                    GL.Space(5);
                    GetCustomUnitSets(UnitSetsCsv, previewUnitsetsStringsCsv, toggleUnitsetsPreviewCsv);
                }

                if (settings.settingSearchForTxt) {
                    GL.Space(5);
                    GetCustomUnitSets(UnitSetsTxt, previewUnitsetisStringsTxt, toggleUnitseitsPreviewTxt);
                }
            }
            GL.EndVertical();
        }

        private static void LoadRefreshArraysFromFiles() {
            try {
                if (settings.settingSearchForCsv) {
                    Common.RefreshArrayFromFile(ref unitSetsCsv, SpawnUnits.UnitSetsFolder, "csv");
                }
                if (settings.settingSearchForTxt) {
                    Common.RefreshArrayFromFile(ref unitSetsTxt, SpawnUnits.UnitSetsFolder, "txt");
                }
            }
            catch (IOException exception) {
                modLogger.Log(exception.ToString());
            }
        }

        private static void GetCustomUnitSets(string[] files, List<string> previewStrings, List<bool> togglePreview) {
            for (int i = 0; i < files.Length; i++) {
                if (previewStrings.Count == 0) {
                    for (int ii = 0; ii < files.Length; ii++) {
                        previewStrings.Add("");
                    }

                }
                GL.BeginVertical("box");
                GL.BeginHorizontal();
                string[] lines = Array.Empty<string>();
                try {
                    lines = File.ReadAllLines(files[i]);

                }
                catch (FileNotFoundException e) {
                    modLogger.Log(e.ToString());
                    LoadRefreshArraysFromFiles();
                    break;
                }
                togglePreview.Add(false);
                if (GL.Button(Strings.GetText("misc_Add") + $" {Path.GetFileNameWithoutExtension(files[i])}", GL.ExpandWidth(false))) {
                    for (int ii = 0; ii < lines.Length; ii++) {
                        lines[ii] = lines[ii].Trim();
                        if (lines[ii].Contains(",") && lines[ii].IndexOf(",") == lines[ii].LastIndexOf(",")) {
                            string[] splitLine = lines[ii].Split(new string[] { "," }, StringSplitOptions.None);
                            splitLine[0] = splitLine[0].Trim();
                            splitLine[1] = splitLine[1].Trim();

                            if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && int.TryParse(splitLine[1], out int iTest) && splitLine[1] != "0") {
                                for (int amount = 0; amount < int.Parse(splitLine[1]); amount++) {
                                    storedGUIDs.Add(splitLine[0]);
                                    refreshStoredGUIDs = true;
                                }

                            }
                            else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && splitLine[1] == "") {
                                storedGUIDs.Add(splitLine[0]);
                                refreshStoredGUIDs = true;
                            }
                        }
                        else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]) != null) {
                            storedGUIDs.Add(lines[ii]);
                            refreshStoredGUIDs = true;
                        }

                    }
                }
                if (GL.Button(Strings.GetText("button_Preview"), GL.ExpandWidth(false))) {
                    if (!togglePreview[i]) {
                        togglePreview[i] = true;
                        for (int ii = 0; ii < lines.Length; ii++) {
                            lines[ii] = lines[ii].Trim();

                            if (lines[ii].Contains(",") && lines[ii].IndexOf(",") == lines[ii].LastIndexOf(",")) {
                                string[] splitLine = lines[ii].Split(new string[] { "," }, StringSplitOptions.None);
                                splitLine[0] = splitLine[0].Trim();
                                splitLine[1] = splitLine[1].Trim();
                                BlueprintUnit unitByGuid = Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]);

                                if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && int.TryParse(splitLine[1], out int iTest) && splitLine[1] != "0") {
                                    if (ii == 0) {
                                        previewStrings[i] = previewStrings[i] + splitLine[1] + "x " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                    }
                                    else {
                                        previewStrings[i] = previewStrings[i] + ", " + splitLine[1] + "x " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                    }
                                }
                                else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && splitLine[1] == "") {
                                    if (ii == 0) {
                                        previewStrings[i] = previewStrings[i] + "1x " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                    }
                                    else {
                                        previewStrings[i] = previewStrings[i] + ", " + "1x " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                    }
                                }
                            }
                            else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]) != null) {
                                BlueprintUnit unitByGuid = Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]);
                                if (ii == 0) {
                                    previewStrings[i] = previewStrings[i] + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                }
                                else {
                                    previewStrings[i] = previewStrings[i] + ", " + $"{Strings.CleanName(unitByGuid.name)} ({unitByGuid.CR})";
                                }
                            }
                        }
                    }
                    else {
                        togglePreview[i] = false;
                        previewStrings[i] = "";
                    }
                }
                GL.FlexibleSpace();
                if (GL.Button(Strings.GetText("button_AddToFavourites"), GL.ExpandWidth(false))) {
                    for (int ii = 0; ii < lines.Length; ii++) {
                        lines[ii] = lines[ii].Trim();
                        if (lines[ii].Contains(",") && lines[ii].IndexOf(",") == lines[ii].LastIndexOf(",")) {
                            string[] splitLine = lines[ii].Split(new string[] { "," }, StringSplitOptions.None);
                            splitLine[0] = splitLine[0].Trim();
                            splitLine[1] = splitLine[1].Trim();

                            if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && int.TryParse(splitLine[1], out int iTest) && splitLine[1] != "0") {
                                if (!unitsFavourites.Contains(splitLine[0])) {
                                    unitsFavourites.Add(splitLine[0]);
                                }
                            }
                            else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && splitLine[1] == "") {
                                if (!unitsFavourites.Contains(splitLine[0])) {
                                    unitsFavourites.Add(splitLine[0]);
                                }
                            }
                        }
                        else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]) != null) {
                            if (!unitsFavourites.Contains(lines[ii])) {
                                unitsFavourites.Add(lines[ii]);
                            }
                        }
                    }
                    RefreshUnitFavourites();
                }
                if (GL.Button(Strings.GetText("button_RemoveFromFavourites"), GL.ExpandWidth(false))) {
                    for (int ii = 0; ii < lines.Length; ii++) {
                        lines[ii] = lines[ii].Trim();
                        if (lines[ii].Contains(",") && lines[ii].IndexOf(",") == lines[ii].LastIndexOf(",")) {
                            string[] splitLine = lines[ii].Split(new string[] { "," }, StringSplitOptions.None);
                            splitLine[0] = splitLine[0].Trim();
                            splitLine[1] = splitLine[1].Trim();

                            if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && int.TryParse(splitLine[1], out int iTest) && splitLine[1] != "0") {
                                if (unitsFavourites.Contains(splitLine[0])) {
                                    unitsFavourites.Remove(splitLine[0]);

                                }
                            }
                            else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(splitLine[0]) != null && splitLine[1] == "") {
                                if (unitsFavourites.Contains(splitLine[0])) {
                                    unitsFavourites.Remove(splitLine[0]);
                                }
                            }
                        }
                        else if (Utilities.GetBlueprintByGuid<BlueprintUnit>(lines[ii]) != null) {
                            if (unitsFavourites.Contains(lines[ii])) {
                                unitsFavourites.Remove(lines[ii]);
                            }
                        }
                    }
                    RefreshUnitFavourites();
                }
                GL.EndHorizontal();
                GL.EndVertical();
                if (togglePreview[i]) {
                    if (previewStrings[i] != "") {
                        MenuTools.SingleLineLabel(previewStrings[i]);
                    }
                    else {
                        MenuTools.SingleLineLabel(Strings.GetText("message_NoValidEntriesFound"));
                    }
                }
            }
        }

        private static List<string> storedGUIDs = new List<string>();
        public static List<string> GetStoredGUIDs { get => storedGUIDs; }
        private static string storedUnitsLabel = "";
        private static bool refreshStoredGUIDs = true;
        private static string[] unitStateStrings = { Strings.GetText("misc_Friendly"), Strings.GetText("misc_Passive"), Strings.GetText("misc_Hostile") };
        private static SelectionGrid unitStateGrid = new SelectionGrid(unitStateStrings, 3);
        private static void StoredUnitsMenu(bool actionKeyMenu = false) {
            if (refreshStoredGUIDs) {
                storedUnitsLabel = RichTextUtils.Bold(Strings.GetText("label_StoredUnits") + $" ({storedGUIDs.Count()}): ");
                foreach (string s in storedGUIDs) {
                    BlueprintUnit blueprintUnit = Utilities.GetBlueprintByGuid<BlueprintUnit>(s);
                    if (blueprintUnit != null) {
                        string nameCR = $"{Strings.CleanName(Utilities.GetBlueprintByGuid<BlueprintUnit>(s).name)} ({Utilities.GetBlueprintByGuid<BlueprintUnit>(s).CR})";
                        storedUnitsLabel = storedUnitsLabel + nameCR + ", ";
                    }
                    else {
                        storedUnitsLabel = s + " " + Strings.GetText("error_NotFound") + ", ";
                    }
                }
                storedUnitsLabel = storedUnitsLabel.TrimEnd(new char[] { ',', ' ' });
                refreshStoredGUIDs = false;
            }
            GL.BeginVertical("box");
            MenuTools.SingleLineLabel(storedUnitsLabel);
            if (storedGUIDs.Any()) {
                GL.BeginHorizontal();

                if (GL.Button(Strings.GetText("label_ClearStoredUnits"), GL.ExpandWidth(false))) {
                    storedGUIDs.Clear();
                    refreshStoredGUIDs = true;
                }
                if (GL.Button(RichTextUtils.Bold(Strings.GetText("button_Spawn")), GL.ExpandWidth(false))) {
                    Vector3 playerPos = Game.Instance.Player.MainCharacter.Value.Position;
                    float angle = 0f;
                    foreach (string guid in GetStoredGUIDs) {
                        Vector2 posXY = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)) * GetStoredGUIDs.Count() * 0.5f;
                        Vector3 finalPos = new Vector3(posXY.x + playerPos.x, playerPos.y, playerPos.z + posXY.y);
                        UnitSpawner(finalPos, guid);
                        angle++;
                    }
                }
                unitStateGrid.RenderNoGroup();
                GL.EndHorizontal();
                if (!actionKeyMenu) {
                    if (!StringUtils.ToToggleBool(settings.toggleEnableActionKey)) {
                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_EnableActionKeyToSpawnHotkey") + ": ", GL.ExpandWidth(false));
                        ActionKey.MainToggle(7);
                        GL.EndHorizontal();
                    }
                    else if (StringUtils.ToToggleBool(settings.toggleEnableActionKey)) {
                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        if (settings.actionKeyIndex != 7) {
                            GL.Label(Strings.GetText("label_ActionKeyNotSetToSpawnUnits") + ": ", GL.ExpandWidth(false));
                            if (GL.Button(Strings.GetText("label_SetToSpawnUnits"), GL.ExpandWidth(false))) {
                                settings.actionKeyIndex = 7;
                            }
                        }
                        else if (!StringUtils.ToToggleBool(settings.toggleSpawnEnemiesFromUnitFavourites)) {
                            GL.Label(Strings.GetText("label_ActionKeyNotSetToSpawnUnitsFromUnitFavouritesStored") + ": ", GL.ExpandWidth(false));
                            MenuTools.ToggleButtonNoGroup(ref settings.toggleSpawnEnemiesFromUnitFavourites, "buttonToggle_ActionKeySpawnUnitsFromUnitFavourites", "tooltip_ActionKeySpawnUnitsFromUnitFavourites");
                        }
                        else {
                            GL.Label(Strings.GetText("label_ActionKey") + ": ", GL.ExpandWidth(false));
                            Keys.SetKeyBinding(ref settings.actionKey);
                        }
                        GL.EndHorizontal();
                        GL.EndVertical();

                    }

                }
            }
            GL.EndVertical();
        }

        public static void UnitSpawner(Vector3 finalPos, string guid) {
            if (unitStateGrid.selected == 0) {
                Common.SpawnFriendlyUnit(finalPos, guid);

            }
            else if (unitStateGrid.selected == 1) {
                Common.SpawnPassiveUnit(finalPos, guid);

            }
            else if (unitStateGrid.selected == 2) {
                Common.SpawnHostileUnit(finalPos, guid);
            }
            else {
                modLogger.Log("unitStateGrid index not recognised!");
            }
        }

        private static bool favouritesLoad = true;
        private static List<String> favouriteObjectNamesClean = new List<string>();
        private static List<int> favouriteCR = new List<int>();
        private static List<bool> toggleFavouriteDescription = new List<bool>();
        public static void FavouritesMenu(bool actionKeyMenu = false) {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showUnitsFavourites = GL.Toggle(settings.showUnitsFavourites, RichTextUtils.Bold(Strings.GetText("headerOption_ShowFavourites")), GL.ExpandWidth(false));
            GL.EndHorizontal();
            if (settings.showUnitsFavourites == true) {

                if (favouritesLoad == true) {
                    RefreshUnitFavourites();
                    favouritesLoad = false;
                }

                if (!unitsFavourites.Any()) {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                }
                else {
                    StoredUnitsMenu(actionKeyMenu);

                    for (int i = 0; i < unitsFavourites.Count; i++) {

                        GL.BeginHorizontal();

                        toggleFavouriteDescription.Add(false);
                        toggleFavouriteDescription[i] = GL.Toggle(toggleFavouriteDescription[i], RichTextUtils.Bold($"{favouriteObjectNamesClean[i]} ({favouriteCR[i]})"), GL.ExpandWidth(false));

                        MenuTools.AddRemoveButtonShiftAdd(ref storedGUIDs, unitsFavourites[i], ref refreshStoredGUIDs, "<b>-</b>", "<b>+</b>", Strings.GetText("tooltip_AddMultipleToStorage"));

                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false))) {
                            unitsFavourites.Remove(unitsFavourites[i]);
                            RefreshUnitFavourites();
                            break;
                        }
                        GL.EndHorizontal();


                        UnitDetails(toggleFavouriteDescription[i], unitsFavourites[i]);
                    }

                    GL.Space(10);
                    GL.BeginHorizontal();
                    if (GL.Button(Strings.GetText("label_AddFavouritesToStoredUnits") + $" ({unitsFavourites.Count()})", GL.ExpandWidth(false))) {
                        foreach (string guid in unitsFavourites) {
                            if (!storedGUIDs.Contains(guid)) {
                                storedGUIDs.Add(guid);
                            }
                        }
                        refreshStoredGUIDs = true;
                    }
                    GL.EndHorizontal();

                    MenuTools.ExportCopyGuidsNamesButtons(unitsFavourites.ToArray(), favouriteObjectNamesClean.ToArray(), "unit-favourites");
                }
            }
            GL.EndVertical();
        }
        private static void RefreshUnitFavourites() {
            FavouritesFactory.GetFavouriteUnits.Serialize();

            favouriteObjectNamesClean.Clear();
            favouriteCR.Clear();
            toggleFavouriteDescription.Clear();
            for (int i = 0; i < unitsFavourites.Count; i++) {
                BlueprintUnit blueprintUnit = Utilities.GetBlueprintByGuid<BlueprintUnit>(unitsFavourites[i]);
                if (blueprintUnit != null) {
                    string stringUnitObjectName = blueprintUnit.name;
                    favouriteObjectNamesClean.Add(Strings.CleanName(stringUnitObjectName));
                    int intUnitCR = blueprintUnit.CR;
                    favouriteCR.Add(intUnitCR);
                }
                else {
                    favouriteObjectNamesClean.Add(unitsFavourites[i] + " " + Strings.GetText("error_NotFound"));
                    favouriteCR.Add(-1);
                }
            }
        }

        private static bool filterError = false;
        private static SimpleShowHideButton hideFilterButton = new SimpleShowHideButton(true);
        private static void FilterBox(string label, ref string textFieldString, string message, float width = 500f) {
            GL.BeginVertical("box");

            GL.BeginHorizontal();
            GL.Label(Strings.GetText(label) + ": ", GL.ExpandWidth(false));
            textFieldString = GL.TextField(textFieldString, GL.Width(width));
            hideFilterButton.Render();
            GL.EndHorizontal();
            if (hideFilterButton.GetButtonText == Strings.GetText("misc_Hide")) {
                MenuTools.SingleLineLabelGt(message);
                if (filterError) {
                    MenuTools.SingleLineLabel(RichTextUtils.BoldRedFormat(Strings.GetText("warning_FilterPatternError1")));
                    MenuTools.SingleLineLabel(RichTextUtils.BoldRedFormat(Strings.GetText("warning_FilterPatternError2")));

                }
            }

            GL.EndVertical();
        }

        private static List<String> searchResultGUIDs = new List<string>();
        private static List<String> searchResultObjectNames = new List<string>();
        private static List<int> searchResultCR = new List<int>();

        private static void SearchValidUnits(string search, string filters) {
            filterError = false;
            searchResultGUIDs.Clear();
            searchResultObjectNames.Clear();
            searchResultCR.Clear();
            toggleResultDescription.Clear();
            IEnumerable<BlueprintUnit> blueprintUnits = ResourcesLibrary.GetBlueprints<BlueprintUnit>();
            foreach (BlueprintUnit unit in blueprintUnits) {
                bool unitAdded = false;
                string guid = unit.AssetGuid;
                if (StringUtils.ToToggleBool(settings.toggleSearchByUnitObjectName)) {
                    if (unit.name != null) {
                        if (unit.name.Contains(search, StringComparison.CurrentCultureIgnoreCase)) {
                            if (!searchResultGUIDs.Contains(guid)) {
                                AddResult(unit);
                                unitAdded = true;
                            }
                        }
                    }
                }
                else if (StringUtils.ToToggleBool(settings.toggleSearchByUnitCharacterName)) {
                    if (unit.CharacterName != null) {
                        if (unit.CharacterName.Contains(search, StringComparison.CurrentCultureIgnoreCase)) {
                            if (!searchResultGUIDs.Contains(guid)) {
                                AddResult(unit);
                                unitAdded = true;
                            }
                        }
                    }
                }
                else if (StringUtils.ToToggleBool(settings.toggleSearchByUnitType)) {
                    if (unit.Type?.Name != null) {
                        if (unit.Type.Name.ToString().Contains(search, StringComparison.CurrentCultureIgnoreCase)) {
                            if (!searchResultGUIDs.Contains(guid)) {
                                AddResult(unit);
                                unitAdded = true;
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(filters) && !filterError && unitAdded) {
                    FilterBoxParse(filters, unit);
                }
            }
        }

        private static void FilterBoxParse(string filters, BlueprintUnit unit) {
            // filters: cr(i|i,i,i|i-i|<i|>i), type(s), race(s), gender('m'|'f')
            filters = filters.ToLower();
            filters = Regex.Replace(filters, @"s", "");
            Regex correctFilter = new Regex(@"^[A-Za-z0-9,:<>;!-]*$");
            if (correctFilter.IsMatch(filters)) {

                string[] filterArray = filters.Split(';');

                foreach (string filter in filterArray) {
                    string[] filterValue = filter.Split(':');

                    if (filterValue.Length != 2) {
                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown filter format: {filter}");
                        filterError = true;
                        return;
                    }
                    else {
                        string type = filterValue[0];
                        string value = filterValue[1];
                        bool not = false;
                        if (type.Remove(1) == "!") {
                            not = true;
                            type = type.Remove(0, 1);
                        }

                        if (type == "cr") {
                            if (value.Contains(",")) {
                                string[] multipleCRs = value.Split(',');
                                List<int> crsValue = new List<int>();
                                foreach (string cr in multipleCRs) {
                                    if (int.TryParse(cr, out int crOut)) {
                                        crsValue.Add(crOut);
                                    }
                                    else {
                                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {cr}");
                                        filterError = true;
                                        return;
                                    }
                                }
                                bool check = !crsValue.Contains(unit.CR);
                                if (not) {
                                    check = !check;
                                }
                                if (check) {
                                    RemoveResult(unit);
                                    return;
                                }
                            }
                            else if (value.Contains("-")) {
                                string[] fromTo = value.Split('-');
                                if (fromTo.Length != 2) {
                                    modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown value format (expected i-i): {value}");
                                    filterError = true;
                                    return;
                                }
                                else {
                                    int from;
                                    int to;
                                    if (int.TryParse(fromTo[0], out int fromOut)) {
                                        from = fromOut;
                                        if (int.TryParse(fromTo[1], out int toOut)) {
                                            to = toOut;
                                            bool check = unit.CR < from || unit.CR > to;
                                            if (not) {
                                                check = !check;
                                            }
                                            if (check) {
                                                RemoveResult(unit);
                                                return;
                                            }
                                        }
                                        else {
                                            modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {fromTo[1]}");
                                            filterError = true;
                                            return;
                                        }
                                    }
                                    else {
                                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {fromTo[0]}");
                                        filterError = true;
                                        return;
                                    }
                                }
                            }
                            else if (value.Contains("<")) {
                                string[] lesser = value.Split('<');
                                if (!string.IsNullOrEmpty(lesser[0])) {
                                    modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown value format (expected <i): {value}");
                                    filterError = true;
                                    return;
                                }
                                else {
                                    if (int.TryParse(lesser[1], out int lesserOut)) {
                                        bool check = unit.CR >= lesserOut;
                                        if (not) {
                                            check = !check;
                                        }
                                        if (check) {
                                            RemoveResult(unit);
                                            return;
                                        }
                                    }
                                    else {
                                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {lesser[1]}");
                                        filterError = true;
                                        return;
                                    }
                                }
                            }
                            else if (value.Contains(">")) {
                                string[] geater = value.Split('>');
                                if (!string.IsNullOrEmpty(geater[0])) {
                                    modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown value format (expected >i): {value}");
                                    filterError = true;
                                    return;
                                }
                                else {
                                    if (int.TryParse(geater[1], out int greaterOut)) {
                                        bool check = unit.CR <= greaterOut;
                                        if (not) {
                                            check = !check;
                                        }
                                        if (check) {
                                            RemoveResult(unit);
                                            return;
                                        }
                                    }
                                    else {
                                        modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unable to parse value (expected integer): {geater[1]}");
                                        filterError = true;
                                        return;
                                    }
                                }
                            }
                            else if (int.TryParse(value, out int singleCR)) {
                                bool check = unit.CR != singleCR;
                                if (not) {
                                    check = !check;
                                }
                                if (check) {
                                    RemoveResult(unit);
                                    return;
                                }
                            }
                            else {
                                modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError]{filters}" + $"\n[FilterError] Unknown cr format: {value}");
                                filterError = true;
                                return;
                            }

                        }
                        else if (type == "type") {
                            if (unit.Type?.Name == null) {
                                RemoveResult(unit);
                                return;
                            }
                            else if (value.Contains(",")) {
                                string[] types = value.Split(',');
                                bool check = !types.Any(s => unit.Type.Name.ToString().Contains(s, StringComparison.CurrentCultureIgnoreCase));
                                if (not) {
                                    check = !check;
                                }
                                if (check) {
                                    RemoveResult(unit);
                                    return;
                                }
                            }
                            else {
                                bool check = !unit.Type.Name.ToString().Contains(value, StringComparison.CurrentCultureIgnoreCase);
                                if (not) {
                                    check = !check;
                                }
                                if (check) {
                                    RemoveResult(unit);
                                    return;
                                }
                            }
                        }
                        else if (type == "race") {
                            if (unit.Race?.Name == null) {
                                RemoveResult(unit);
                                return;
                            }
                            else if (value.Contains(",")) {
                                string[] races = value.Split(',');

                                bool check = !races.Any(s => unit.Race.Name.Contains(s, StringComparison.CurrentCultureIgnoreCase));
                                if (not) {
                                    check = !check;
                                }
                                if (check) {
                                    RemoveResult(unit);
                                    return;
                                }
                            }
                            else {
                                bool check = !unit.Race.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase);
                                if (not) {
                                    check = !check;
                                }
                                if (check) {
                                    RemoveResult(unit);
                                    return;
                                }
                            }
                        }
                        else if (type == "gender") {
                            if (value == "f") {
                                bool check = unit.Gender != Gender.Female;
                                if (not) {
                                    check = !check;
                                }
                                if (check) {
                                    RemoveResult(unit);
                                    return;
                                }
                            }
                            else if (value == "m") {
                                bool check = unit.Gender != Gender.Male;
                                if (not) {
                                    check = !check;
                                }
                                if (check) {
                                    RemoveResult(unit);
                                    return;
                                }
                            }
                            else {
                                modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError] {filters}" + $"\n[FilterError] Neither m nor f: {value}");
                                filterError = true;
                                return;
                            }
                        }
                        else {
                            modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError] {filters}" + $"\n[FilterError] Unknown filter: {type}");
                            filterError = true;
                            return;
                        }
                    }
                }
            }
            else {
                modLogger.Log($"[FilterError] {Strings.GetText("warning_FilterPatternError1")}" + $"\n[FilterError] {filters}" + "\n[FilterError Does not match: ^[A-Za-z0-9,:<>;-]*$");
                filterError = true;
                return;
            }
        }

        private static void AddResult(BlueprintUnit unit) {
            searchResultGUIDs.Add(unit.AssetGuid);

            if (unit.name != null) {
                searchResultObjectNames.Add(unit.name);
            }
            else {
                searchResultObjectNames.Add("No Object Name!");
            }

            searchResultCR.Add(unit.CR);
        }

        private static void RemoveResult(BlueprintUnit unit) {
            int i = searchResultGUIDs.IndexOf(unit.AssetGuid);
            searchResultGUIDs.RemoveAt(i);
            searchResultObjectNames.RemoveAt(i);
            searchResultCR.RemoveAt(i);
        }

        private static void UnitDetails(bool toggle, string unitGuid) {
            if (toggle) {
                GL.BeginVertical("box");

                BlueprintUnit unitByGuid = Utilities.GetBlueprintByGuid<BlueprintUnit>(unitGuid);
                if (unitByGuid != null) {
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_UnitName") + ": ") + $"{unitByGuid.CharacterName}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_UnitGUID") + ": ") + $"{unitByGuid.AssetGuid}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_ObjectName") + ": ") + $"{unitByGuid.name}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_ObjectNameClean") + ": ") + $"{Strings.CleanName(unitByGuid.name)}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_ChallengeRating") + ": ") + $"{unitByGuid.CR}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_UnitType") + ": ") + $"{unitByGuid.Type?.Name}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_Race") + ": ") + $"{unitByGuid.Race?.Name}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_Gender") + ": ") + $"{unitByGuid.Gender}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("charStat_Strength") + ": ") + $"{unitByGuid.Strength}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("charStat_Dexterity") + ": ") + $"{unitByGuid.Dexterity}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("charStat_Constitution") + ": ") + $"{unitByGuid.Constitution}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("charStat_Intelligence") + ": ") + $"{unitByGuid.Intelligence}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("charStat_Wisdom") + ": ") + $"{unitByGuid.Wisdom}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("charStat_Charisma") + ": ") + $"{unitByGuid.Charisma}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("charStat_Speed") + ": ") + $"{unitByGuid.Speed}");
                    MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("charStat_BaseAttackBonus") + ": ") + $"{unitByGuid.BaseAttackBonus}");
                    MenuTools.CopyExportButtons("button_ExportUnitInfo", unitByGuid.name + ".txt", UnitInfo(unitByGuid), "label_CopyUnitInformationToClipboard");
                }
                else {
                    MenuTools.SingleLineLabel(unitGuid + " " + Strings.GetText("error_NotFound"));
                }

                GL.EndVertical();
            }
        }

        private static string[] UnitInfo(BlueprintUnit unit) {
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
