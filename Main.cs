using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

using Harmony12;

using Kingmaker;
using Kingmaker.Assets.UI;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Tasks;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.ServiceWindow.LocalMap;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

using UnityEngine;
using UnityEngine.SceneManagement;

using GL = UnityEngine.GUILayout;
using ModEntry = UnityModManagerNet.UnityModManager.ModEntry;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    internal static class Main
    {
        /// <summary>
        /// [1.14.0.7] Mod对象实例，来源于DUMM
        /// </summary>
        internal static ModEntry ModEntry { get; private set; }
        /// <summary>
        /// [1.14.1.8] 窗口宽度
        /// </summary>
        internal static GUILayoutOption WindowWidth => GL.MaxWidth(UnityModManager.UI.WindowSize.x - 20f);
        /// <summary>
        /// [1.14.1.8] 需要排除的GUID
        /// </summary>
        internal const string ExcludeGuid = "c5d4962385e0e9c439ab935d83361947";
        /// <summary>
        /// [1.14.1.8] 查询结果数量限制
        /// </summary>
        internal const int QueryLimit = 100;

        public static bool Enabled;

        public static unsafe float* CameraScrollSpeed = null;
        public static LocalMap LocalMap = null;
        public static bool RotationChanged = false;

        public static Settings Settings;
        public static TaxCollectorSettings TaxSettings = new TaxCollectorSettings();
        public static SaveData SaveData = new SaveData();
        public static ModEntry.ModLogger ModLogger;
        public static ModEntryCheck ScaleXp;
        public static ModEntryCheck CraftMagicItems;
        public static bool VersionMismatch = false;
        public static Vector2 ScrollPosition;

        public static UICanvas MainCanvas = new UICanvas("BoT_MainCanvas");
        public static UITMPText SceneAreaInfo = new UITMPText(MainCanvas.baseGameObject, "BoT_SceneAndAreaInfo");
        public static UITMPText ObjectInfo = new UITMPText(MainCanvas.baseGameObject, "BoT_SceneAndAreaInfo");

        static bool Load(ModEntry modEntry)
        {
            ObjectInfo.Size(25);
            ObjectInfo.Off();

            SceneAreaInfo.Size(25);
            SceneAreaInfo.Off();

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            ModLogger = modEntry.Logger;

            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            Storage.modEntryPath = modEntry.Path;

            if (File.Exists(modEntry.Path + "debug"))
            {
                Settings.settingShowDebugInfo = true;
            }

            if (File.Exists(modEntry.Path + "devtool"))
            {
                Settings.toggleDevTools = Storage.isTrueString;
            }

            if (File.Exists(modEntry.Path + "dictxml"))
            {
                SerializeFallback();
            }

            Logger.Clear();

            try
            {
                CreateDirectories();

            }
            catch (Exception e)
            {

                ModLogger.Log(e.ToString());
            }

            ScaleXp = new ModEntryCheck("ScaleXP");
            CraftMagicItems = new ModEntryCheck("CraftMagicItems");

            Storage.flagsBundle = AssetBundle.LoadFromFile(Storage.modEntryPath + Storage.assetBundlesFolder + "\\" + Storage.assetBunldeFlags);

            Storage.modAssemblyName = modEntry.Info.AssemblyName;

            if (Strings.ToBool(Settings.toggleEnableTaxCollector))
            {
                TaxSettings = TaxCollector.Deserialize(Storage.modEntryPath + Storage.taxCollectorFolder + "\\" + Storage.taxCollectorFile);
                TaxCollector.saveTimeGame = TaxSettings.saveTime;
            }

            if (Storage.gamerVersionAtCreation != GameVersion.GetVersion())
            {
                ModLogger.Log(Strings.GetText("warning_GameVersion") + $": { GameVersion.GetVersion()}");
                ModLogger.Log(Strings.GetText("warning_GameVersion") + " " + Strings.Parenthesis(Strings.GetText("warning_Mod")) + $": {Storage.gamerVersionAtCreation}");
                VersionMismatch = true;
            }

            LoadFavourites();
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);

            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                ModLogger.Log(e.ToString());
                harmony.UnpatchAll(modEntry.Info.Id);
                return false;
            }

            Settings.heavyEncumbranceMultiplier = Mathf.Clamp(Settings.heavyEncumbranceMultiplier, 1f, 100f);
            Settings.customHeavyEncumbranceMultiplier = Settings.finalCustomHeavyEncumbranceMultiplier.ToString();
            Settings.fatigueHoursModifierMultiplier = Mathf.Clamp(Settings.fatigueHoursModifierMultiplier, 0.1f, 30f);
            Settings.customFatigueHoursModifierMultiplier = Settings.finalCustomFatigueHoursModifierMultiplier.ToString();
            Settings.experienceMultiplier = Mathf.Clamp(Settings.experienceMultiplier, 0.1f, 10f);
            Settings.customExperienceMultiplier = Settings.finalCustomExperienceMultiplier.ToString();
            Settings.moneyMultiplier = Mathf.Clamp(Settings.moneyMultiplier, 0.1f, 10f);
            Settings.customMoneyMultiplier = Settings.finalCustomMoneyMultiplier.ToString();
            Settings.spellsPerDayMultiplier = Mathf.Clamp(Settings.spellsPerDayMultiplier, 0.1f, 4f);
            Settings.customSpellsPerDayMultiplier = Settings.finalCustomspellsPerDayMultiplier.ToString();
            Settings.featMultiplierString = Settings.featMultiplier.ToString();
            Settings.selectedKingdomAlignmentString = Settings.selectedKingdomAlignment.ToString();
            Settings.selectedKingdomAlignmentTranslated = IntToAlignment(Settings.selectedKingdomAlignment);
            Settings.travelSpeedMultiplier = Mathf.Clamp(Settings.travelSpeedMultiplier, 0.1f, 100f);
            Settings.travelSpeedMultiplierString = Settings.travelSpeedMultiplier.ToString();
            Settings.characterCreationAbilityPointsMax = Math.Max(18, Settings.characterCreationAbilityPointsMax);
            Settings.characterCreationAbilityPointsMaxString = Settings.characterCreationAbilityPointsMax.ToString();
            Settings.characterCreationAbilityPointsMin = Math.Max(0, Settings.characterCreationAbilityPointsMin);
            Settings.characterCreationAbilityPointsMinString = Settings.characterCreationAbilityPointsMin.ToString();
            Settings.characterCreationAbilityPointsPlayer = Math.Max(0, Settings.characterCreationAbilityPointsPlayer);
            Settings.characterCreationAbilityPointsPlayerString = Settings.characterCreationAbilityPointsPlayer.ToString();
            Settings.characterCreationAbilityPointsMerc = Math.Max(0, Settings.characterCreationAbilityPointsMerc);
            Settings.characterCreationAbilityPointsMercString = Settings.characterCreationAbilityPointsMerc.ToString();
            Settings.companionCostMultiplier = Math.Max(0, Settings.companionCostMultiplier);
            Settings.companionCostMultiplierString = Settings.companionCostMultiplier.ToString();
            Settings.partyStatMultiplier = Mathf.Clamp(Settings.partyStatMultiplier, 0.1f, 10f);
            Settings.enemyStatMultiplier = Mathf.Clamp(Settings.enemyStatMultiplier, 0.1f, 10f);
            Settings.randomEncounterSettingFloatAmountLimited = Mathf.Clamp(Settings.randomEncounterSettingFloatAmountLimited, 0f, 1f);
            Settings.takeXCustom = Mathf.Clamp(Settings.takeXCustom, 1f, 20f);
            Settings.kingdomBuildingTimeModifier = Mathf.Clamp(Settings.kingdomBuildingTimeModifier, -10f, 10f);
            Settings.vendorSellPriceMultiplier = Settings.finalVendorSellPriceMultiplier.ToString();
            Settings.sillyBloodChance = Mathf.Clamp(Settings.sillyBloodChance, 0f, 1f);
            Settings.debugTimeMultiplier = Mathf.Clamp(Settings.debugTimeMultiplier, 0.1f, 30f);
            Settings.customDebugTimeMultiplier = Settings.finalCustomDebugTimeMultiplier.ToString();
            Settings.repeatableLockPickingWeariness = Settings.finalRepeatableLockPickingWeariness.ToString();
            Settings.artisanMasterpieceChance = Mathf.Clamp(Settings.artisanMasterpieceChance, 0f, 1f);
            Settings.unityModManagerButtonIndex = Mathf.Clamp(Settings.unityModManagerButtonIndex, 0f, 6f);

            Storage.flyingHeightSlider = Mathf.Clamp(Storage.flyingHeightSlider, -10f, 100f);
            Storage.itemFavouritesGuids = Storage.itemFavourites;
            Storage.buffFavouritesGuids = Storage.buffFavourites;
            Storage.featFavouritesGuids = Storage.featFavourites;
            Storage.abilitiesFavouritesGuids = Storage.abilitiesFavourites;

            if (!Settings.settingKeepGuids)
            {
                Settings.itemGuid = "";
            }

            try
            {
                if (File.Exists(Storage.modEntryPath + Storage.localisationFolder + "\\" + Settings.selectedLocalisationName + ".xml"))
                {
                    Strings.temp = Strings.XMLtoDict(Settings.selectedLocalisationName);
                    if (Strings.temp.Count == MenuText.fallback.Count && Strings.temp.Keys == MenuText.fallback.Keys)
                    {
                        Strings.current = Strings.temp;
                    }
                    else
                    {
                        Strings.current = Strings.temp.Union(MenuText.fallback).GroupBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
                    }
                    Strings.RefreshStrings();
                    Storage.mainToolbarStrings = new string[] { RichText.MainCategoryFormat(Strings.GetText("mainCategory_FavouriteFunctions")), RichText.MainCategoryFormat(Strings.GetText("mainCategory_Cheats")), RichText.MainCategoryFormat(Strings.GetText("mainCategory_Mods")), RichText.MainCategoryFormat(Strings.GetText("mainCategory_Tools")), RichText.MainCategoryFormat(Strings.GetText("mainCategory_Settings")) };
                    Settings.filterButtonText = Strings.GetText("misc_Enable");
                    Settings.currentSceneString = Strings.GetText("misc_Display");
                    Settings.showAllBuffs = Strings.GetText("misc_Display");
                    Settings.showAllFeats = Strings.GetText("misc_Display");
                    Settings.showAllAbilities = Strings.GetText("misc_Display");
                }
                Storage.localisationsXMLFiles.Clear();
                Storage.localisationsXML = Directory.GetFiles(Storage.modEntryPath + Storage.localisationFolder, "*.xml");
                foreach (var s in Storage.localisationsXML)
                {
                    Storage.localisationsXMLFiles.Add(Path.GetFileNameWithoutExtension(s));
                }

                if (Storage.localisationsXMLFiles.Count - 1 < Settings.selectedLocalisation)
                {
                    Settings.selectedLocalisation = 0;
                }
                if (Storage.localisationsXMLFiles.Contains(Settings.selectedLocalisationName))
                {
                    Settings.selectedLocalisation = Storage.localisationsXMLFiles.FindIndex(a => a.Contains(Settings.selectedLocalisationName));
                }
                SortLocalisationsAndFlags();

                Storage.selectedLocalisationOld = Settings.selectedLocalisation;

                if (Settings.settingSearchForCsv)
                {
                    Storage.filesCsv = Directory.GetFiles(modEntry.Path + Storage.itemSetsFolder, "*.csv");
                    Array.Sort<string>(Storage.filesCsv);
                }
                if (Settings.settingSearchForTxt)
                {
                    Storage.filesTxt = Directory.GetFiles(modEntry.Path + Storage.itemSetsFolder, "*.txt");
                    Array.Sort<string>(Storage.filesTxt);
                }
            }
            catch (IOException exception)
            {
                ModLogger.Log(exception.ToString());
            }

            if (!Settings.settingKeepCategories)
            {
                Settings.showCarryMoreCategory = false;
                Settings.showRestLessCategory = false;
                Settings.showMoneyCategory = false;
                Settings.showExperienceCategory = false;
                Settings.showKingdomCategory = false;
                Settings.showItemsCategory = false;
                Settings.showSpellCategory = false;
                Settings.showBuffsCategory = false;
                Settings.showPartyStatisticsCategory = false;
                Settings.showEnemyStatisticsCategory = false;
                Settings.showFeatsCategory = false;
                Settings.showAddAbilitiesCategory = false;
                Settings.showSpellSpellbooksCategory = false;
                Settings.showTravelAndMapCategory = false;
                Settings.showDiceRollCategory = false;
                Settings.showFogOfWarCategory = false;
                Settings.showMiscCategory = false;
                Settings.showKeyBindingsCategory = false;
                Settings.showTaxCollectorCategory = false;
                Settings.showBeneathTheStolenLandsCategory = false;
            }

            if (!Settings.settingKeepSubMenus)
            {
                Settings.showItemInfo = false;
                Settings.showItemSets = false;
                Settings.multipleItems = false;
                Settings.moneyAmount = "1";
                Settings.finalMoneyAmount = 1;
                Settings.experienceAmount = "1";
                Settings.finalExperienceAmount = 1;
                Settings.buildPointAmount = "1";
                Settings.finalBuildPointAmount = 1;
                Settings.itemAmount = "1";
                Settings.finalItemAmount = 1;
                Settings.itemSearch = "";
                Settings.itemAmount = "1";
                Settings.finalItemAmount = 1;
                Settings.toggleFilterWeapons = Storage.isTrueString;
                Settings.toggleFilterShields = Storage.isTrueString;
                Settings.toggleFilterArmours = Storage.isTrueString;
                Settings.toggleFilterRings = Storage.isTrueString;
                Settings.toggleFilterBelts = Storage.isTrueString;
                Settings.toggleFilterFootwear = Storage.isTrueString;
                Settings.toggleFilterGloves = Storage.isTrueString;
                Settings.toggleFilterHeadwear = Storage.isTrueString;
                Settings.toggleFilterNeckItems = Storage.isTrueString;
                Settings.toggleFilterShoulderItems = Storage.isTrueString;
                Settings.toggleFilterWristItems = Storage.isTrueString;
                Settings.toggleFilterKeys = Storage.isTrueString;
                Settings.toggleFilterMiscellaneousItems = Storage.isTrueString;
                Settings.toggleFilterNotes = Storage.isTrueString;
                Settings.toggleFilterUsableItems = Storage.isTrueString;
                Settings.showFilters = false;
                Settings.filterButtonText = Strings.GetText("misc_Enable");
                Settings.toggleSearchOnlyCraftMagicItems = Storage.isFalseString;
                Settings.toggleSearchByItemObjectName = Storage.isTrueString;
                Settings.toggleSearchByItemName = Storage.isTrueString;
                Settings.toggleSearchByItemDescription = Storage.isFalseString;
                Settings.toggleSearchByItemFlavorText = Storage.isFalseString;
                Settings.showItemFavourites = false;
                Settings.addItemIdentified = false;
                Settings.showExperimentalCategory = false;
                Settings.flyingHeight = "0.1";
                Settings.finalflyingHeight = 0.1f;
                Settings.flyingHeightUseSlider = false;
                Settings.gameConstsAmount = "1";
                Settings.finalGameConstsAmount = 1;
                Settings.showAllBuffs = Strings.GetText("misc_Display");
                Settings.showBuffFavourites = false;
                Settings.buffSearch = "";
                Settings.toggleSearchByBuffObjectName = Storage.isTrueString;
                Settings.toggleSearchByBuffName = Storage.isTrueString;
                Settings.toggleSearchByBuffDescription = Storage.isFalseString;
                Settings.toggleSearchByBuffComponents = Storage.isFalseString;
                Settings.toggleBuffsShowEnemies = Storage.isFalseString;
                Settings.partyStatsAmount = "10";
                Settings.partyFinalStatsAmount = 10;
                Settings.partyStatMultiplier = 1f;
                Settings.toggleShowOnlyClassSkills = Storage.isFalseString;
                Settings.enemyStatsAmount = "10";
                Settings.enemyFinalStatsAmount = 10;
                Settings.enemyStatMultiplier = 1f;
                Settings.toggleForcedEncounterIsHard = Storage.isFalseString;
                Settings.forcedEncounterCR = "1";
                Settings.forcedEncounterFinalCR = 1;
                Settings.forcedEncounterSelectedAvoidance = 0;
                Settings.forcedEncounterSelectedBlueprintMode = 0;
                Settings.forcedEncounterGuid = "";
                Settings.randomEncounterSettingsShow = false;
                Settings.randomEncounterSettingFloatAmountLimited = 0.1f;
                Settings.randomEncounterSettingFloatAmount = "1";
                Settings.randomEncounterSettingFloatAmountFinal = 1f;
                Settings.randomEncounterSettingIntAmount = "1";
                Settings.randomEncounterSettingIntAmountFinal = 1;
                Settings.kingdomBuildingTimeModifier = 0f;
                Settings.showSoundsCategory = false;
                Settings.showLanguageMenu = false;
                Settings.sillyBloodChance = 0.1f;
                Settings.showFeatsFavourites = false;
                Settings.featSearch = "";
                Settings.toggleSearchByFeatObjectName = Storage.isTrueString;
                Settings.toggleSearchByFeatName = Storage.isTrueString;
                Settings.toggleSearchByFeatDescription = Storage.isFalseString;
                Settings.toggleFeatsShowEnemies = Storage.isFalseString;
                Settings.showAllFeats = Strings.GetText("misc_Display");
                Settings.showRomanceCounters = false;
                Settings.showRomanceCountersSpoilers = false;
                Settings.featsParamShowAllWeapons = false;
                Settings.showUpgradSettlementsAndBuildings = false;
                Settings.showRomanceCountersExperimental = false;
                Settings.romanceCounterSetValue = "1";
                Settings.finalRomanceCounterSetValue = 1;
                Settings.damageDealtMultipliersValue = "1";
                Settings.finalDamageDealtMultipliersValue = 1;
                Settings.toggleShowClassDataOptions = Storage.isFalseString;
                Settings.showAllAbilities = Strings.GetText("misc_Display");
                Settings.showAbilitiesFavourites = false;
                Settings.abilitySearch = "";
                Settings.toggleSearchByAbilityObjectName = Storage.isTrueString;
                Settings.toggleSearchByAbilityName = Storage.isTrueString;
                Settings.toggleSearchByAbilityDescription = Storage.isFalseString;
                Settings.showAnimationsCategory = false;
            }

            if (Settings.gameConstsMinUnitSpeedMps != -1)
            {
                var minUnitSpeedMps = typeof(GameConsts).GetField("MinUnitSpeedMps");
                minUnitSpeedMps.SetValue(null, Settings.gameConstsMinUnitSpeedMps);
            }
            if (Settings.gameConstsMinWeaponRange != -1)
            {
                var minWeaponRange = typeof(GameConsts).GetField("MinWeaponRange");
                minWeaponRange.SetValue(null, Settings.gameConstsMinWeaponRange.Feet());
            }
            if (Settings.gameConstsMinWeaponRange != -1)
            {
                var stealthDcIncrement = typeof(GameConsts).GetField("StealthDCIncrement");
                stealthDcIncrement.SetValue(null, Settings.gameConstsMinWeaponRange.Feet());
            }

            if (Strings.ToBool(Settings.toggleUberLogger))
            {
                UberLogger.Logger.Enabled = true;
                if (Strings.ToBool(Settings.toggleUberLoggerForward))
                {
                    UberLogger.Logger.ForwardMessages = true;
                }
            }

            if (!BuildModeUtility.IsDevelopment && Strings.ToBool(Settings.toggleCombatDifficultyMessage))
            {
                EventBus.Subscribe((object)new CombatDifficultyMessage());
            }

            if (Settings.settingShowDebugInfo)
            {
                LogModInfoLoad();
            }

            if (Common.IsEarlierVersion(modEntry.Info.Version))
            {
                UpdateSettings();
                Settings.modVersion = modEntry.Info.Version;
            }

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGui;
            modEntry.OnSaveGUI = OnSaveGui;

            ModEntry = modEntry;

            return true;
        }

        static bool OnToggle(ModEntry modEntry, bool value)
        {
            Enabled = value;

            return true;
        }

        static void OnGui(ModEntry modEntry)
        {
            if (Storage.settingsWarning)
            {
                MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_SettingsXML_0")));

                MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_SettingsXML_1")));

                GL.Space(10);

                if (GL.Button(RichText.Bold(Strings.GetText("button_Confirm")), GL.ExpandWidth(false)))
                {
                    Storage.settingsWarning = false;
                }
            }
            else if (!Settings.firstLaunch && !Storage.settingsWarning)
            {
                if (Game.Instance.Player.ControllableCharacters.Any())
                {
                    if (VersionMismatch && !Storage.hideVersionMismatch)
                    {
                        Menu.VersionMismatch();
                    }

                    GL.Space(10);

                    Settings.mainToolbarIndex = GL.Toolbar(Settings.mainToolbarIndex, Storage.mainToolbarStrings, WindowWidth);

                    if (Strings.ToBool(Settings.toggleBoTScrollBar))
                    {
                        ScrollPosition = GL.BeginScrollView(ScrollPosition, GL.Width(Storage.ummWidth - 24f));
                    }
                    switch (Settings.mainToolbarIndex)
                    {
                        case 0:
                            Menu.FavouriteFunctions();
                            break;
                        case 1:
                            Menu.Cheat();
                            break;
                        case 2:
                            Menu.Mods();
                            break;
                        case 3:
                            Menu.Tools();
                            break;
                        case 4:
                            Menu.Settings();
                            break;
                    }

                    if (Strings.ToBool(Settings.toggleBoTScrollBar))
                    {
                        GL.EndScrollView();
                    }
                }
                else
                {
                    if (VersionMismatch && !Storage.hideVersionMismatch)
                    {
                        Menu.VersionMismatch();
                    }
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("message_NotInGame"));
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    if (Settings.currentSceneString == Strings.GetText("misc_Hide"))
                    {
                        var currenctSceneName = SceneManager.GetActiveScene().name;
                        GL.Label(Strings.GetText("label_CurrentScene") + $": { currenctSceneName}");
                    }
                    GL.EndHorizontal();
                }
            }
            else if (Settings.firstLaunch && SceneManager.GetActiveScene().name != "Start" && !Storage.settingsWarning)
            {
                if (VersionMismatch && !Storage.hideVersionMismatch)
                {
                    Menu.VersionMismatch();
                }
                if (Storage.loadOnce)
                {
                    SortLocalisationsAndFlags(true);
                    Storage.loadOnce = false;
                }

                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_SelectLanguage") + ":"));
                GL.BeginHorizontal();
                if (GL.Button(Strings.GetText("button_LoadRefreshAvailableLanguages"), GL.ExpandWidth(false)))
                {
                    try
                    {
                        Storage.localisationsXMLFiles.Clear();
                        Storage.localisationsXML = Directory.GetFiles(modEntry.Path + Storage.localisationFolder, "*.xml");
                        foreach (var s in Storage.localisationsXML)
                        {
                            Storage.localisationsXMLFiles.Add(Path.GetFileNameWithoutExtension(s));
                        }
                        SortLocalisationsAndFlags();
                    }
                    catch (IOException exception)
                    {
                        ModLogger.Log(exception.ToString());
                    }

                    if (Storage.localisationsXMLFiles.Count - 1 < Settings.selectedLocalisation)
                    {
                        Settings.selectedLocalisation = 0;
                    }
                }
                GL.EndHorizontal();
                GL.Space(10);
                GL.BeginHorizontal();
                Settings.selectedLocalisation = GL.SelectionGrid(Settings.selectedLocalisation, Storage.localeGrid.ToArray(), 1, GL.ExpandWidth(false));
                GL.EndHorizontal();
                if (Settings.selectedLocalisation != Storage.selectedLocalisationOld)
                {
                    Storage.selectedLocalisationOld = Settings.selectedLocalisation;
                    Settings.selectedLocalisationName = Path.GetFileNameWithoutExtension(Storage.localisationsXMLFiles.ToArray()[Settings.selectedLocalisation]);
                    if (File.Exists(Storage.modEntryPath + Storage.localisationFolder + "\\" + Settings.selectedLocalisationName + ".xml"))
                    {
                        Strings.temp = Strings.XMLtoDict(Settings.selectedLocalisationName);
                        if (Strings.temp.Count == MenuText.fallback.Count && Strings.temp.Keys == MenuText.fallback.Keys)
                        {
                            Strings.current = Strings.temp;
                        }
                        else
                        {
                            Strings.current = Strings.temp.Union(MenuText.fallback).GroupBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
                        }
                        Strings.RefreshStrings();
                        Storage.mainToolbarStrings = new string[] { RichText.MainCategoryFormat(Strings.GetText("mainCategory_FavouriteFunctions")), RichText.MainCategoryFormat(Strings.GetText("mainCategory_Cheats")), RichText.MainCategoryFormat(Strings.GetText("mainCategory_Mods")), RichText.MainCategoryFormat(Strings.GetText("mainCategory_Tools")), RichText.MainCategoryFormat(Strings.GetText("mainCategory_Settings")) };
                        Settings.filterButtonText = Strings.GetText("misc_Enable");
                        Settings.currentSceneString = Strings.GetText("misc_Display");
                        Settings.showAllBuffs = Strings.GetText("misc_Display");
                        Settings.showAllFeats = Strings.GetText("misc_Display");
                        Settings.showAllAbilities = Strings.GetText("misc_Display");

                    }
                }

                GL.Space(10);

                if (GL.Button(RichText.Bold(Strings.GetText("button_Confirm")), GL.ExpandWidth(false)))
                {
                    Settings.firstLaunch = false;
                }
            }
            else
            {
                if (VersionMismatch && !Storage.hideVersionMismatch)
                {
                    Menu.VersionMismatch();
                }
                MenuTools.SingleLineLabel(Strings.GetText("message_GameLoading"));
            }

            if (Settings.toggleShowTooltips == Storage.isTrueString)
            {
                MenuTools.Tooltips();
            }
        }

        public static List<string> BlueprintsByTypes(string[] validTypes)
        {
            return ResourcesLibrary.LibraryObject.GetAllBlueprints().FindAll(e => validTypes.Contains(e.GetType().Name)).Select(e => e.AssetGuid).ToList();
        }

        public static void CreateFilteredItemSet(string labelString, string[] type)
        {
            if (!GL.Button(labelString, GL.ExpandWidth(false))) return;
            ModEntry.OnModActions.Push(m => BlueprintsByTypes(type).FindAll(e => Utilities.GetBlueprintByGuid<BlueprintItem>(e) != null && e != ExcludeGuid).ForEach(e => MenuTools.AddSingleItemAmount(e, 1, Settings.addItemIdentified)));
        }

        public static void GetCustomItemSets(string[] files, List<string> previewStrings, List<bool> togglePreview)
        {
            for (var i = 0; i < files.Length; i++)
            {
                if (previewStrings.Count == 0)
                    for (var j = 0; j < files.Length; j++)
                        previewStrings.Add("");
                GL.BeginVertical("box");
                GL.BeginHorizontal();
                var lines = File.ReadAllLines(files[i]);
                togglePreview.Add(false);

                if (GL.Button(Strings.GetText("misc_Add") + $" {Path.GetFileNameWithoutExtension(files[i])}", GL.ExpandWidth(false)))
                    ModEntry.OnModActions.Push(m =>
                    {
                        for (var j = 0; j < lines.Length; j++)
                        {
                            lines[j] = lines[j].Trim();
                            if (lines[j].Contains(",") && lines[j].IndexOf(",", StringComparison.Ordinal) == lines[j].LastIndexOf(",", StringComparison.Ordinal))
                            {
                                var splitLine = lines[j].Split(new[] { "," }, StringSplitOptions.None);
                                splitLine[0] = splitLine[0].Trim();
                                splitLine[1] = splitLine[1].Trim();
                                if (ExcludeGuid == splitLine[0]) continue;
                                if ("" == splitLine[1])
                                    MenuTools.AddSingleItemAmount(splitLine[0], 1, Settings.addItemIdentified);
                                else if ("0" != splitLine[1] && int.TryParse(splitLine[1], out var number))
                                    MenuTools.AddSingleItemAmount(splitLine[0], number, Settings.addItemIdentified);
                            }
                            else if (ExcludeGuid != lines[j])
                                MenuTools.AddSingleItemAmount(lines[j], 1, Settings.addItemIdentified);
                        }
                    });

                if (GL.Button(Strings.GetText("button_Preview"), GL.ExpandWidth(false)))
                {
                    if (!togglePreview[i])
                    {
                        togglePreview[i] = true;
                        for (var j = 0; j < lines.Length && QueryLimit > j; j++)
                        {
                            lines[j] = lines[j].Trim();
                            if (lines[j].Contains(",") && lines[j].IndexOf(",", StringComparison.Ordinal) ==
                                lines[j].LastIndexOf(",", StringComparison.Ordinal))
                            {
                                var splitLine = lines[j].Split(new[] { "," }, StringSplitOptions.None);
                                splitLine[0] = splitLine[0].Trim();
                                splitLine[1] = splitLine[1].Trim();
                                if (ExcludeGuid == splitLine[0]) continue;
                                var itemByGuid = Utilities.GetBlueprintByGuid<BlueprintItem>(splitLine[0]);
                                if (null == itemByGuid) continue;
                                if ("" == splitLine[1])
                                {
                                    if (j == 0)
                                        previewStrings[i] += "1X " + itemByGuid.Name;
                                    else
                                        previewStrings[i] += ", " + "1X " + itemByGuid.Name;
                                }
                                else if ("0" != splitLine[1] && int.TryParse(splitLine[1], out _))
                                {
                                    if (j == 0)
                                        previewStrings[i] += splitLine[1] + "X " + itemByGuid.Name;
                                    else
                                        previewStrings[i] += ", " + splitLine[1] + "X " + itemByGuid.Name;
                                }
                            }
                            else if (ExcludeGuid != lines[j])
                            {
                                var itemByGuid = Utilities.GetBlueprintByGuid<BlueprintItem>(lines[j]);
                                if (null == itemByGuid) continue;
                                if (j == 0)
                                    previewStrings[i] = previewStrings[i] + itemByGuid.Name;
                                else
                                    previewStrings[i] = previewStrings[i] + ", " + itemByGuid.Name;
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
                    ModEntry.OnModActions.Push(m =>
                    {
                        for (var j = 0; j < lines.Length; j++)
                        {
                            lines[j] = lines[j].Trim();
                            if (lines[j].Contains(",") && lines[j].IndexOf(",", StringComparison.Ordinal) ==
                                lines[j].LastIndexOf(",", StringComparison.Ordinal))
                            {
                                var splitLine = lines[j].Split(new[] { "," }, StringSplitOptions.None);
                                splitLine[0] = splitLine[0].Trim();
                                splitLine[1] = splitLine[1].Trim();
                                if (ExcludeGuid == splitLine[0]) continue;
                                var itemByGuid = Utilities.GetBlueprintByGuid<BlueprintItem>(splitLine[0]);
                                if (null == itemByGuid || Storage.itemFavourites.Contains(splitLine[0])) continue;
                                if ("" == splitLine[1] || "0" != splitLine[1] && int.TryParse(splitLine[1], out _))
                                    Storage.itemFavourites.Add(splitLine[0]);
                            }
                            else if (ExcludeGuid != lines[j] && !Storage.itemFavourites.Contains(lines[j]) &&
                                     null != Utilities.GetBlueprintByGuid<BlueprintItem>(lines[j]))
                                Storage.itemFavourites.Add(lines[j]);
                        }

                        RefreshItemFavourites();
                    });
                if (GL.Button(Strings.GetText("button_RemoveFromFavourites"), GL.ExpandWidth(false)))
                    ModEntry.OnModActions.Push(m =>
                    {
                        for (var j = 0; j < lines.Length; j++)
                        {
                            lines[j] = lines[j].Trim();
                            if (lines[j].Contains(",") && lines[j].IndexOf(",", StringComparison.Ordinal) ==
                                lines[j].LastIndexOf(",", StringComparison.Ordinal))
                            {
                                var splitLine = lines[j].Split(new[] { "," }, StringSplitOptions.None);
                                splitLine[0] = splitLine[0].Trim();
                                splitLine[1] = splitLine[1].Trim();
                                if (ExcludeGuid == splitLine[0]) continue;
                                var itemByGuid = Utilities.GetBlueprintByGuid<BlueprintItem>(splitLine[0]);
                                if (null == itemByGuid || !Storage.itemFavourites.Contains(splitLine[0])) continue;
                                if ("" == splitLine[1] || "0" != splitLine[1] && int.TryParse(splitLine[1], out _))
                                    Storage.itemFavourites.Remove(splitLine[0]);
                            }
                            else if (ExcludeGuid != lines[j] && Storage.itemFavourites.Contains(lines[j]) &&
                                     null != Utilities.GetBlueprintByGuid<BlueprintItem>(lines[j]))
                                Storage.itemFavourites.Remove(lines[j]);
                        }

                        RefreshItemFavourites();
                    });
                GL.EndHorizontal();
                GL.EndVertical();

                if (!togglePreview[i]) continue;
                MenuTools.SingleLineLabel(previewStrings[i] != "" ? previewStrings[i] : Strings.GetText("message_NoValidEntriesFound"));
            }
        }

        public static void SearchValidItems(List<string> validItemTypesList)
        {
            Storage.validItemObjectNames.Clear();
            Storage.validItemNames.Clear();
            Storage.validItemDescriptions.Clear();
            Storage.validItemFlavorTexts.Clear();
            Storage.validItemGuids.Clear();
            Storage.resultItemNames.Clear();
            Storage.resultItemGuids.Clear();
            Storage.toggleItemSearchDescription.Clear();

            if (string.IsNullOrEmpty(Settings.itemSearch)) return;
            Storage.currentItemSearch = Settings.itemSearch;
            Storage.blueprintList = ResourcesLibrary.LibraryObject.GetAllBlueprints();
            foreach (var bso in Storage.blueprintList)
            {
                if (!validItemTypesList.Contains(bso.GetType().Name)) continue;
                var bpItem = bso as BlueprintItem;
                if (bpItem.name == "EkunQ2_rewardArmor") continue;
                if (Settings.toggleSearchOnlyCraftMagicItems == Storage.isTrueString)
                {
                    if (!bpItem.AssetGuid.Contains(Storage.scribeScrollBlueprintPrefix) && !bpItem.AssetGuid.Contains(Storage.craftMagicItemsBlueprintPrefix)) continue;
                    Storage.validItemObjectNames.Add(bpItem.name);
                    Storage.validItemNames.Add(bpItem.Name);
                    Storage.validItemDescriptions.Add(bpItem.Description);
                    Storage.validItemFlavorTexts.Add(bpItem.FlavorText);
                    Storage.validItemGuids.Add(bpItem.AssetGuid);
                }
                else
                {
                    Storage.validItemObjectNames.Add(bpItem.name);
                    Storage.validItemNames.Add(bpItem.Name);
                    Storage.validItemDescriptions.Add(bpItem.Description);
                    Storage.validItemFlavorTexts.Add(bpItem.FlavorText);
                    Storage.validItemGuids.Add(bpItem.AssetGuid);
                }
            }

            for (var i = 0; i < Storage.validItemObjectNames.Count; i++)
            {
                var stringItemName = Storage.validItemNames[i];
                var stringGuid = Storage.validItemGuids[i];
                if (Storage.validItemObjectNames[i].Contains(Settings.itemSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByItemObjectName == Storage.isTrueString)
                    {
                        if (!Storage.resultItemGuids.Contains(stringGuid))
                        {
                            Storage.resultItemGuids.Add(stringGuid);
                            Storage.resultItemNames.Add(stringItemName);
                        }
                    }
                }
                if (Storage.validItemNames[i].Contains(Settings.itemSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByItemName == Storage.isTrueString)
                    {
                        if (!Storage.resultItemGuids.Contains(stringGuid))
                        {
                            Storage.resultItemGuids.Add(stringGuid);
                            Storage.resultItemNames.Add(stringItemName);
                        }
                    }
                }
                if (Storage.validItemDescriptions[i].Contains(Settings.itemSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByItemDescription == Storage.isTrueString)
                    {
                        if (!Storage.resultItemGuids.Contains(stringGuid))
                        {
                            Storage.resultItemGuids.Add(stringGuid);
                            Storage.resultItemNames.Add(stringItemName);
                        }
                    }
                }
                if (Storage.validItemFlavorTexts[i].Contains(Settings.itemSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByItemFlavorText == Storage.isTrueString)
                    {
                        if (!Storage.resultItemGuids.Contains(stringGuid))
                        {
                            Storage.resultItemGuids.Add(stringGuid);
                            Storage.resultItemNames.Add(stringItemName);
                        }
                    }
                }

            }
        }

        public static void ResetFilteredItems()
        {
            Storage.validItemTypesFiltered.Clear();
            Storage.validItemTypesFiltered = new List<string>
                                    {
                                        "BlueprintItemWeapon",
                                        "BlueprintItemShield",
                                        "BlueprintItemArmor",
                                        "BlueprintItemEquipmentRing",
                                        "BlueprintItemEquipmentBelt",
                                        "BlueprintItemEquipmentFeet",
                                        "BlueprintItemEquipmentGloves",
                                        "BlueprintItemEquipmentHead",
                                        "BlueprintItemEquipmentNeck",
                                        "BlueprintItemEquipmentShoulders",
                                        "BlueprintItemEquipmentWrist",
                                        "BlueprintItemEquipmentHandSimple",
                                        "BlueprintItemKey",
                                        "BlueprintItem",
                                        "BlueprintItemNote",
                                        "BlueprintItemEquipmentUsable",
                                        "BlueprintItemEquipmentSimple"
                                    };
        }

        public static void RefreshItemFavourites()
        {
            Common.SerializeListString(Storage.itemFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesItemsFile);

            Storage.itemFavouritesGuids = Storage.itemFavourites;
            Storage.itemFavouriteNames.Clear();
            Storage.toggleItemFavouriteDescription.Clear();
            for (var i = 0; i < Storage.itemFavouritesGuids.Count; i++)
            {
                Storage.itemFavouriteNames.Add(Utilities.GetBlueprintByGuid<BlueprintItem>(Storage.itemFavourites[i]).Name);
            }
        }

        public static void SearchValidBuffs(List<string> validBuffTypesList)
        {
            Storage.buffValidObjectNames.Clear();
            Storage.buffValidNames.Clear();
            Storage.buffValidDescriptions.Clear();
            Storage.buffValidComponents.Clear();
            Storage.buffValidGuids.Clear();
            Storage.buffResultNames.Clear();
            Storage.buffResultGuids.Clear();
            Storage.buffResultDescriptions.Clear();

            if (string.IsNullOrEmpty(Settings.buffSearch)) return;
            Storage.currentBuffSearch = Settings.buffSearch;
            Storage.blueprintList = ResourcesLibrary.LibraryObject.GetAllBlueprints();

            foreach (var bso in Storage.blueprintList)
            {
                if (!validBuffTypesList.Contains(bso.GetType().Name)) continue;
                var bpBuff = bso as BlueprintBuff;
                Storage.buffValidObjectNames.Add(bpBuff.name);
                Storage.buffValidNames.Add(bpBuff.Name);
                Storage.buffValidDescriptions.Add(GetBuffDescription(bso));
                foreach (var bc in bpBuff.CollectComponents())
                    Storage.buffValidComponents.Add(bc.name);
                Storage.buffValidGuids.Add(bpBuff.AssetGuid);
            }

            for (var i = 0; i < Storage.buffValidObjectNames.Count; i++)
            {
                var stringBuffName = Storage.buffValidNames[i];
                var stringBuffObjectNames = Storage.buffValidObjectNames[i];
                var stringBuffGuid = Storage.buffValidGuids[i];
                if (Storage.buffValidObjectNames[i].Contains(Settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByBuffObjectName == Storage.isTrueString)
                    {
                        if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                        {
                            Storage.buffResultGuids.Add(stringBuffGuid);
                            Storage.buffResultNames.Add(!string.IsNullOrEmpty(stringBuffName)
                                ? stringBuffName
                                : stringBuffObjectNames);
                            Storage.buffResultDescriptions.Add(GetBuffDescription(Utilities.GetBlueprintByGuid<BlueprintBuff>(stringBuffGuid)));
                        }
                    }
                }
                if (Storage.buffValidNames[i].Contains(Settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByBuffName == Storage.isTrueString)
                    {
                        if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                        {
                            Storage.buffResultGuids.Add(stringBuffGuid);
                            Storage.buffResultNames.Add(!string.IsNullOrEmpty(stringBuffName)
                                ? stringBuffName
                                : stringBuffObjectNames);
                            Storage.buffResultDescriptions.Add(GetBuffDescription(Utilities.GetBlueprintByGuid<BlueprintBuff>(stringBuffGuid)));
                        }
                    }
                }
                if (Storage.buffValidDescriptions[i].Contains(Settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByBuffDescription == Storage.isTrueString)
                    {
                        if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                        {
                            Storage.buffResultGuids.Add(stringBuffGuid);
                            Storage.buffResultNames.Add(!string.IsNullOrEmpty(stringBuffName)
                                ? stringBuffName
                                : stringBuffObjectNames);
                            Storage.buffResultDescriptions.Add(GetBuffDescription(Utilities.GetBlueprintByGuid<BlueprintBuff>(stringBuffGuid)));
                        }
                    }
                }
                if (Storage.buffValidComponents[i].Contains(Settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByBuffComponents == Storage.isTrueString)
                    {
                        if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                        {
                            Storage.buffResultGuids.Add(stringBuffGuid);
                            Storage.buffResultNames.Add(!string.IsNullOrEmpty(stringBuffName)
                                ? stringBuffName
                                : stringBuffObjectNames);
                            Storage.buffResultDescriptions.Add(GetBuffDescription(Utilities.GetBlueprintByGuid<BlueprintBuff>(stringBuffGuid)));
                        }
                    }
                }
            }
        }

        public static string GetBuffDescription(BlueprintScriptableObject bpObejct)
        {
            var context = new MechanicsContext((UnitEntityData)null, Game.Instance.Player.MainCharacter.Value.Descriptor, bpObejct, (MechanicsContext)null, (TargetWrapper)null);
            return context.SelectUIData(UIDataType.Description)?.Description;
        }

        public static void RefreshBuffFavourites()
        {
            Common.SerializeListString(Storage.buffFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesBuffsFile);

            Storage.buffFavouritesGuids = Storage.buffFavourites;
            Storage.buffFavouriteNames.Clear();
            Storage.buffFavouritesDescriptions.Clear();
            for (var i = 0; i < Storage.buffFavouritesGuids.Count; i++)
            {
                var stringBuffName = Utilities.GetBlueprintByGuid<BlueprintBuff>(Storage.buffFavourites[i]).Name;
                if (stringBuffName != "" && stringBuffName != null)
                {
                    Storage.buffFavouriteNames.Add(stringBuffName);
                }
                else
                {
                    var stringBuffObjectNames = Utilities.GetBlueprintByGuid<BlueprintBuff>(Storage.buffFavourites[i]).name;
                    Storage.buffFavouriteNames.Add(stringBuffObjectNames);
                }
                Storage.buffFavouritesDescriptions.Add(GetBuffDescription(Utilities.GetBlueprintByGuid<BlueprintBuff>(Storage.buffFavourites[i])));
            }
        }

        public static void RefreshAllBuffs(UnitEntityData unitEntityData)
        {
            Storage.buffAllNames.Clear();
            Storage.buffAllObjectNames.Clear();
            Storage.buffAllGuids.Clear();
            Storage.buffAllDescriptions.Clear();

            for (var i = 0; i < unitEntityData.Buffs.Count; i++)
            {
                Storage.buffAllNames.Add(unitEntityData.Buffs[i].Name);
                Storage.buffAllObjectNames.Add(unitEntityData.Buffs[i].Blueprint.name);
                Storage.buffAllGuids.Add(unitEntityData.Buffs[i].Blueprint.AssetGuid);
                var context = new MechanicsContext((UnitEntityData)null, Game.Instance.Player.MainCharacter.Value.Descriptor, (BlueprintScriptableObject)Utilities.GetBlueprintByGuid<BlueprintBuff>(unitEntityData.Buffs[i].Blueprint.AssetGuid), (MechanicsContext)null, (TargetWrapper)null);
                Storage.buffAllDescriptions.Add(context.SelectUIData(UIDataType.Description)?.Description);
            }
        }

        static void SearchValidRandomEncounter(List<string> validRandomEncounters)
        {
            Storage.buffValidObjectNames.Clear();
            Storage.buffValidNames.Clear();
            Storage.buffValidDescriptions.Clear();
            Storage.buffValidComponents.Clear();
            Storage.buffValidGuids.Clear();

            Storage.buffResultNames.Clear();
            Storage.buffResultGuids.Clear();
            Storage.buffResultDescriptions.Clear();

            if (Settings.buffSearch != "")
            {
                Storage.currentBuffSearch = Settings.buffSearch;
                Storage.blueprintList = ResourcesLibrary.LibraryObject.GetAllBlueprints();
                for (var i = 0; i < Storage.blueprintList.Count; i++)
                {
                    var bpObejct = Storage.blueprintList[i];
                    if (validRandomEncounters.Contains(bpObejct.GetType().Name))
                    {
                        var bpBuff = bpObejct as BlueprintBuff;

                        Storage.buffValidObjectNames.Add(bpBuff.name);
                        Storage.buffValidNames.Add(bpBuff.Name);
                        Storage.buffValidDescriptions.Add(GetBuffDescription(bpObejct));

                        foreach (var bc in bpBuff.CollectComponents())
                        {
                            Storage.buffValidComponents.Add(bc.name);
                        }

                        Storage.buffValidGuids.Add(bpBuff.AssetGuid);

                    }

                }

                for (var i = 0; i < Storage.buffValidObjectNames.Count; i++)
                {
                    var stringBuffName = Storage.buffValidNames[i];
                    var stringBuffObjectNames = Storage.buffValidObjectNames[i];
                    var stringBuffGuid = Storage.buffValidGuids[i];
                    if (Storage.buffValidObjectNames[i].Contains(Settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Settings.toggleSearchByBuffObjectName == Storage.isTrueString)
                        {
                            if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                            {
                                Storage.buffResultGuids.Add(stringBuffGuid);

                                if (stringBuffName != "" && stringBuffName != null)
                                {
                                    Storage.buffResultNames.Add(stringBuffName);
                                }
                                else
                                {
                                    Storage.buffResultNames.Add(stringBuffObjectNames);
                                }

                                Storage.buffResultDescriptions.Add(GetBuffDescription(Utilities.GetBlueprintByGuid<BlueprintBuff>(stringBuffGuid)));
                            }
                        }
                    }
                    if (Storage.buffValidNames[i].Contains(Settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Settings.toggleSearchByBuffName == Storage.isTrueString)
                        {
                            if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                            {
                                if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                                {
                                    Storage.buffResultGuids.Add(stringBuffGuid);

                                    if (stringBuffName != "" && stringBuffName != null)
                                    {
                                        Storage.buffResultNames.Add(stringBuffName);
                                    }
                                    else
                                    {
                                        Storage.buffResultNames.Add(stringBuffObjectNames);
                                    }

                                    Storage.buffResultDescriptions.Add(GetBuffDescription(Utilities.GetBlueprintByGuid<BlueprintBuff>(stringBuffGuid)));
                                }
                            }
                        }
                    }
                    if (Storage.buffValidDescriptions[i].Contains(Settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Settings.toggleSearchByBuffDescription == Storage.isTrueString)
                        {
                            if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                            {
                                if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                                {
                                    Storage.buffResultGuids.Add(stringBuffGuid);

                                    if (stringBuffName != "" && stringBuffName != null)
                                    {
                                        Storage.buffResultNames.Add(stringBuffName);
                                    }
                                    else
                                    {
                                        Storage.buffResultNames.Add(stringBuffObjectNames);
                                    }

                                    Storage.buffResultDescriptions.Add(GetBuffDescription(Utilities.GetBlueprintByGuid<BlueprintBuff>(stringBuffGuid)));
                                }
                            }
                        }
                    }
                    if (Storage.buffValidComponents[i].Contains(Settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Settings.toggleSearchByBuffComponents == Storage.isTrueString)
                        {
                            if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                            {
                                if (!Storage.buffResultGuids.Contains(stringBuffGuid))
                                {
                                    Storage.buffResultGuids.Add(stringBuffGuid);

                                    if (stringBuffName != "" && stringBuffName != null)
                                    {
                                        Storage.buffResultNames.Add(stringBuffName);
                                    }
                                    else
                                    {
                                        Storage.buffResultNames.Add(stringBuffObjectNames);
                                    }

                                    Storage.buffResultDescriptions.Add(GetBuffDescription(Utilities.GetBlueprintByGuid<BlueprintBuff>(stringBuffGuid)));
                                }
                            }
                        }
                    }

                }
            }
        }

        public static void StartEncounter(BlueprintRandomEncounter blueprint, int cr, Vector3? position, bool isHard, bool isCamp)
        {
            Storage.encounterError = "";


            var state = Game.Instance.Player.GlobalMap;
            var settings = Game.Instance.BlueprintRoot.RE;
            state.NextEncounterRollMiles = state.MilesTravelled + settings.SafeMilesAfterEncounter;
            var randomEncounterData = new RandomEncounterData(blueprint, position)
            {
                CR = cr
            };
            if (isCamp)
                randomEncounterData.AvoidanceCheckResult = RandomEncounterAvoidanceCheckResult.Fail;
            if (blueprint.AvoidType == EncounterAvoidType.SkillCheck)
            {
                switch (Settings.forcedEncounterSelectedAvoidance)
                {
                    case 0:
                        var rule = Rulebook.Trigger<RulePartySkillCheck>(new RulePartySkillCheck(blueprint.AvoidSkill, blueprint.AvoidDC));
                        if (rule == null || rule.IsPassed)
                        {
                            randomEncounterData.AvoidanceCheckResult = RandomEncounterAvoidanceCheckResult.Success;
                        }
                        else
                        {
                            randomEncounterData.AvoidanceCheckResult = rule.DifficultyClass - rule.RollResult < settings.RandomEncounterAvoidanceFailMargin ? RandomEncounterAvoidanceCheckResult.Fail : RandomEncounterAvoidanceCheckResult.CriticalFail;
                        }
                        break;
                    case 1:
                        randomEncounterData.AvoidanceCheckResult = RandomEncounterAvoidanceCheckResult.Success;
                        break;
                    case 2:
                        randomEncounterData.AvoidanceCheckResult = RandomEncounterAvoidanceCheckResult.Fail;
                        break;
                    case 3:
                        randomEncounterData.AvoidanceCheckResult = RandomEncounterAvoidanceCheckResult.CriticalFail;
                        break;
                }

            }
            if (blueprint.IsRandomizedCombat)
            {
                randomEncounterData.SpawnersGroups = RandomEncounterUnitSelector.Select(blueprint, cr);
                if (!randomEncounterData.SpawnersGroups.All<SpawnersGroupData>((Func<SpawnersGroupData, bool>)(g => g.Spawners.All<RandomSpawnerData>((Func<RandomSpawnerData, bool>)(s => (bool)((UnityEngine.Object)s.Unit))))))
                {
                    var errorCantStartEncounter = $"Can't start encounter: {blueprint} CR {cr} -  try a different CR.";
                    ModLogger.Log(errorCantStartEncounter);
                    Storage.encounterError = errorCantStartEncounter;
                    return;
                }
            }
            state.StartEncounter(randomEncounterData);
            EventBus.RaiseEvent<IRandomEncounterHandler>((Action<IRandomEncounterHandler>)(h => h.OnRandomEncounterStarted(state.CurrentEncounterData)));
            GlobalMapRules.Instance.SpawnEncounterPawn(randomEncounterData, true);
            if (isCamp)
            {
                Game.Instance.Player.REManager.OnCampEncounterStarted();
                UnityModManager.UI.Instance.ToggleWindow();
            }
            else
            {
                Game.Instance.Player.REManager.OnGlobalMapEncounterStarted(blueprint, isHard);
                UnityModManager.UI.Instance.ToggleWindow();
            }
        }

        static void LoadFavourites()
        {
            if (File.Exists(Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesItemsFile))
            {
                Common.DeserializeListString(Storage.itemFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesItemsFile);
            }
            if (File.Exists(Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesBuffsFile))
            {
                Common.DeserializeListString(Storage.buffFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesBuffsFile);
            }
            if (File.Exists(Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesFeatsFile))
            {
                Common.DeserializeListString(Storage.featFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesFeatsFile);
            }
            if (File.Exists(Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesAbilitiesFile))
            {
                Common.DeserializeListString(Storage.abilitiesFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesAbilitiesFile);
            }
            if (File.Exists(Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFile))
            {
                Common.DeserializeListString(Storage.togglesFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFile);
            }
        }

        public static void GetEnemyUnits(List<UnitEntityData> enemyUnits)
        {
            using (var units = Game.Instance.State.Units.GetEnumerator())
            {
                while (units.MoveNext())
                {
                    UnitEntityData unit;
                    if ((unit = units.Current) != null && !unit.IsPlayerFaction && (unit.IsInGame && unit.IsRevealed) && (!unit.Descriptor.State.IsFinallyDead && unit.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot.PlayerFaction)))
                    {
                        enemyUnits.Add(unit);
                    }
                }
            }

            if (Storage.enemyUnitIndex != 0 && enemyUnits.Count - 1 < Storage.enemyUnitIndex)
            {
                Storage.enemyUnitIndex = enemyUnits.Count - 1;
            }

            if (enemyUnits != Storage.enemyUnits)
            {
                Storage.enemyUnits = enemyUnits;
                Storage.enemyUnitsNamesList.Clear();
                foreach (var controllableCharacter in enemyUnits)
                {
                    Storage.enemyUnitsNamesList.Add(controllableCharacter.CharacterName);
                }
            }
        }

        static void LogModInfoLoad()
        {
            ModLogger.Log("Current Culture: " + CultureInfo.CurrentCulture.ToString());
            ModLogger.Log("Game Version: " + GameVersion.GetVersion());
            ModLogger.Log($"Game Version (Mod): {Storage.gamerVersionAtCreation}");
            ModLogger.Log($"Mod Version: {new ModEntryCheck("BagOfTricks").Version()}");
            ModLogger.Log($"Mod Version (Settings.xml): {Settings.modVersion}");
            ModLogger.Log("Mod Assembly Name: " + Storage.modAssemblyName);
            ModLogger.Log("Mod Assembly MD5: " + ModMd5(Storage.modEntryPath + "\\" + Storage.modAssemblyName));
            ModLogger.Log("ItemSets Folder: " + Storage.modEntryPath + Storage.itemSetsFolder);
            ModLogger.Log("Favourites Folder: " + Storage.modEntryPath + Storage.favouritesFolder);
            ModLogger.Log("Characters Folder: " + Storage.modEntryPath + Storage.charactersImportFolder);
            ModLogger.Log("Tax Collector Folder: " + Storage.modEntryPath + Storage.taxCollectorFolder);
            ModLogger.Log("Modified Blueprints Folder: " + Storage.modEntryPath + Storage.modifiedBlueprintsFolder);
            ModLogger.Log("ScaleXP Installed: " + ScaleXp.IsInstalled());
            ModLogger.Log("CraftMagicItems Installed: " + CraftMagicItems.IsInstalled());
            ModLogger.Log("Localisation Folder: " + Storage.modEntryPath + Storage.localisationFolder);


            foreach (var s in Storage.localisationsXML)
            {
                ModLogger.Log("Localisations: " + s);
            }

            ModLogger.Log("AssetBundles Folder: " + Storage.modEntryPath + Storage.assetBundlesFolder);

            foreach (var s in Storage.flagsBundle.GetAllAssetNames())
            {
                ModLogger.Log("Flag Assets: " + s);
            }
        }

        static string ModMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static void CheckRandomEncounterSettings()
        {
            if (Settings.toggleRandomEncountersEnabled == Storage.isFalseString)
            {
                Game.Instance.BlueprintRoot.RE.EncountersEnabled = false;
            }
            if (Settings.toggleRandomEncountersEnabled == Storage.isTrueString)
            {
                Game.Instance.BlueprintRoot.RE.EncountersEnabled = true;
            }
            if (Settings.randomEncounterChanceOnGlobalMap != Defaults.randomEncounterChanceOnGlobalMap)
            {
                Game.Instance.BlueprintRoot.RE.ChanceOnGlobalMap = Settings.randomEncounterChanceOnGlobalMap;
            }
            if (Settings.randomEncounterChanceOnCamp != Defaults.randomEncounterChanceOnCamp)
            {
                Game.Instance.BlueprintRoot.RE.ChanceOnCamp = Settings.randomEncounterChanceOnCamp;
            }
            if (Settings.randomEncounterChanceOnCampSecondTime != Defaults.randomEncounterChanceOnCampSecondTime)
            {
                Game.Instance.BlueprintRoot.RE.ChanceOnCampSecondTime = Settings.randomEncounterChanceOnCampSecondTime;
            }
            if (Settings.randomEncounterHardEncounterChance != Defaults.randomEncounterHardEncounterChance)
            {
                Game.Instance.BlueprintRoot.RE.HardEncounterChance = Settings.randomEncounterHardEncounterChance;
            }
            if (Settings.randomEncounterHardEncounterMaxChance != Defaults.randomEncounterHardEncounterMaxChance)
            {
                Game.Instance.BlueprintRoot.RE.HardEncounterMaxChance = Settings.randomEncounterHardEncounterMaxChance;
            }
            if (Settings.randomEncounterHardEncounterChanceIncrease != Defaults.randomEncounterHardEncounterChanceIncrease)
            {
                Game.Instance.BlueprintRoot.RE.HardEncounterChanceIncrease = Settings.randomEncounterHardEncounterChanceIncrease;
            }
            if (Settings.randomEncounterStalkerAmbushChance != Defaults.randomEncounterStalkerAmbushChance)
            {
                Game.Instance.BlueprintRoot.RE.StalkerAmbushChance = Settings.randomEncounterStalkerAmbushChance;
            }
            if (Settings.randomEncounterRollMiles != Defaults.randomEncounterRollMiles)
            {
                Game.Instance.BlueprintRoot.RE.RollMiles = Settings.randomEncounterRollMiles;
            }
            if (Settings.randomEncounterSafeMilesAfterEncounter != Defaults.randomEncounterSafeMilesAfterEncounter)
            {
                Game.Instance.BlueprintRoot.RE.SafeMilesAfterEncounter = Settings.randomEncounterSafeMilesAfterEncounter;
            }
            if (Settings.randomEncounterDefaultSafeZoneSize != Defaults.randomEncounterDefaultSafeZoneSize)
            {
                Game.Instance.BlueprintRoot.RE.DefaultSafeZoneSize = Settings.randomEncounterDefaultSafeZoneSize;
            }
            if (Settings.randomEncounterEncounterPawnOffset != Defaults.randomEncounterEncounterPawnOffset)
            {
                Game.Instance.BlueprintRoot.RE.EncounterPawnOffset = Settings.randomEncounterEncounterPawnOffset;
            }
            if (Settings.randomEncounterEncounterPawnDistanceFromLocation != Defaults.randomEncounterEncounterPawnDistanceFromLocation)
            {
                Game.Instance.BlueprintRoot.RE.EncounterPawnDistanceFromLocation = Settings.randomEncounterEncounterPawnDistanceFromLocation;
            }
            if (Settings.randomEncounterRollMiles != Defaults.randomEncounterRollMiles)
            {
                Game.Instance.BlueprintRoot.RE.RollMiles = Settings.randomEncounterRollMiles;
            }
            if (Settings.randomEncounterEncounterMinBonusCR != Defaults.randomEncounterEncounterMinBonusCR)
            {
                Game.Instance.BlueprintRoot.RE.EncounterMinBonusCR = Settings.randomEncounterEncounterMinBonusCR;
            }
            if (Settings.randomEncounterEncounterMaxBonusCR != Defaults.randomEncounterEncounterMaxBonusCR)
            {
                Game.Instance.BlueprintRoot.RE.EncounterMaxBonusCR = Settings.randomEncounterEncounterMaxBonusCR;
            }
            if (Settings.randomEncounterHardEncounterBonusCR != Defaults.randomEncounterHardEncounterBonusCR)
            {
                Game.Instance.BlueprintRoot.RE.HardEncounterBonusCR = Settings.randomEncounterHardEncounterBonusCR;
            }
        }

        public static void ReloadPartyState()
        {
            Storage.reloadPartyStats = true;
            Storage.reloadPartyBuffs = true;
            Storage.reloadPartyFeats = true;
            Storage.reloadPartyAddAbilities = true;
            Storage.reloadPartyAnimations = true;
            Storage.reloadPartySpellSpellbookChars = true;
            Storage.reloadSpellSpellbooksLearnableSpells = true;
            Storage.reloadSpellSpellbooksLearnableSpellsList = true;
        }

        public static EventResult.MarginType GetOverrideMargin(KingdomEvent instance)
        {
            var associatedTask = instance.AssociatedTask;

            var leaderType = LeaderType.None;
            if (associatedTask != null)
            {
                try
                {
                    leaderType = associatedTask.AssignedLeader.Type;
                }
                catch (Exception e)
                {
                    ModLogger.Log(e.ToString());
                    ModLogger.Log("associatedTask: " + associatedTask.ToString());
                    ModLogger.Log("instance.FullName: " + instance.FullName);
                    ModLogger.Log("instance.EventBlueprint.AssetGuid: " + instance.EventBlueprint.AssetGuid);
                }
            }
            var autoResolveType = instance.EventBlueprint.AutoResolveResult;
            var hasSolutions = instance.EventBlueprint.Solutions.HasSolutions;
            if (!hasSolutions)
            {
                return autoResolveType;
            }
            var canSolve = instance.EventBlueprint.Solutions.CanSolve(leaderType);
            if (!canSolve)
            {
                return autoResolveType;
            }
            var possibleResults = instance.EventBlueprint.Solutions.GetResolutions(leaderType).
                                                 Where(r => !string.IsNullOrEmpty(r.LocalizedDescription)).
                                                 ToArray();
            if (possibleResults.Any(r => r.Margin == EventResult.MarginType.GreatSuccess))
            {
                return EventResult.MarginType.GreatSuccess;
            }
            if (possibleResults.Any(r => r.Margin == EventResult.MarginType.Success))
            {
                return EventResult.MarginType.Success;
            }
            return autoResolveType;
        }

        public static AlignmentMaskType GetAlignment(string alignmentString, AlignmentMaskType alignment)
        {
            switch (alignmentString)
            {
                case "lawfulgood":
                    alignment = AlignmentMaskType.LawfulGood;
                    break;
                case "neutralgood":
                    alignment = AlignmentMaskType.NeutralGood;
                    break;
                case "chaoticgood":
                    alignment = AlignmentMaskType.ChaoticGood;
                    break;
                case "lawfulneutral":
                    alignment = AlignmentMaskType.LawfulNeutral;
                    break;
                case "trueneutral":
                    alignment = AlignmentMaskType.TrueNeutral;
                    break;
                case "chaoticneutral":
                    alignment = AlignmentMaskType.ChaoticNeutral;
                    break;
                case "lawfulevil":
                    alignment = AlignmentMaskType.LawfulEvil;
                    break;
                case "neutralevil":
                    alignment = AlignmentMaskType.NeutralEvil;
                    break;
                case "chaoticevil":
                    alignment = AlignmentMaskType.ChaoticEvil;
                    break;
            }
            return alignment;
        }

        public static void SerializeFallback()
        {
            var serializer = new XmlSerializer(typeof(Strings.Localisation[]), new XmlRootAttribute() { ElementName = "resources" });
            using (var stream = File.Create(Storage.modEntryPath + Storage.localisationFolder + "\\" + "fallback.xml"))
            {
                serializer.Serialize(stream, MenuText.fallback.Select(kv => new Strings.Localisation() { key = kv.Key, value = kv.Value }).ToArray());
            }
        }

        public static void RefreshFeatFavourites()
        {
            Common.SerializeListString(Storage.featFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesFeatsFile);

            Storage.featFavouritesGuids = Storage.featFavourites;
            Storage.featFavouriteNames.Clear();
            Storage.featFavouritesDescriptions.Clear();
            for (var i = 0; i < Storage.featFavouritesGuids.Count; i++)
            {
                var stringFeatName = Utilities.GetBlueprintByGuid<BlueprintFeature>(Storage.featFavourites[i]).Name;
                if (stringFeatName != "" && stringFeatName != null)
                {
                    Storage.featFavouriteNames.Add(stringFeatName);
                }
                else
                {
                    var stringFeatObjectNames = Utilities.GetBlueprintByGuid<BlueprintFeature>(Storage.featFavourites[i]).name;
                    Storage.featFavouriteNames.Add(stringFeatObjectNames);
                }
                Storage.featFavouritesDescriptions.Add(GetFeatDescription(Utilities.GetBlueprintByGuid<BlueprintFeature>(Storage.featFavourites[i])));
            }
        }

        public static string GetFeatDescription(BlueprintScriptableObject bpObejct)
        {
            var context = new MechanicsContext((UnitEntityData)null, Game.Instance.Player.MainCharacter.Value.Descriptor, bpObejct, (MechanicsContext)null, (TargetWrapper)null);
            return context.SelectUIData(UIDataType.Description)?.Description;
        }

        public static void SearchValidFeats(List<string> validFeatTypesList)
        {
            Storage.featValidObjectNames.Clear();
            Storage.featValidNames.Clear();
            Storage.featValidDescriptions.Clear();
            Storage.featValidGuids.Clear();
            Storage.featResultNames.Clear();
            Storage.featResultGuids.Clear();
            Storage.featResultDescriptions.Clear();

            if (string.IsNullOrEmpty(Settings.featSearch)) return;
            Storage.currentFeatSearch = Settings.featSearch;
            Storage.blueprintList = ResourcesLibrary.LibraryObject.GetAllBlueprints();

            foreach (var bso in Storage.blueprintList)
            {
                if (validFeatTypesList.Contains(bso.GetType().Name))
                {
                    var bpFeat = bso as BlueprintFeature;
                    Storage.featValidObjectNames.Add(bpFeat.name);
                    Storage.featValidNames.Add(bpFeat.Name);
                    Storage.featValidDescriptions.Add(GetFeatDescription(bso));
                    Storage.featValidGuids.Add(bpFeat.AssetGuid);
                }
            }

            for (var i = 0; i < Storage.featValidObjectNames.Count; i++)
            {
                var stringFeatName = Storage.featValidNames[i];
                var stringFeatObjectNames = Storage.featValidObjectNames[i];
                var stringFeatGuid = Storage.featValidGuids[i];
                if (Storage.featValidObjectNames[i].Contains(Settings.featSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByFeatObjectName == Storage.isTrueString)
                    {
                        if (!Storage.featResultGuids.Contains(stringFeatGuid))
                        {
                            Storage.featResultGuids.Add(stringFeatGuid);
                            Storage.featResultNames.Add(!string.IsNullOrEmpty(stringFeatName)
                                ? stringFeatName
                                : stringFeatObjectNames);
                            Storage.featResultDescriptions.Add(GetFeatDescription(Utilities.GetBlueprintByGuid<BlueprintFeature>(stringFeatGuid)));
                        }
                    }
                }
                if (Storage.featValidNames[i].Contains(Settings.featSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByFeatName == Storage.isTrueString)
                    {
                        if (!Storage.featResultGuids.Contains(stringFeatGuid))
                        {
                            Storage.featResultGuids.Add(stringFeatGuid);
                            Storage.featResultNames.Add(!string.IsNullOrEmpty(stringFeatName)
                                ? stringFeatName
                                : stringFeatObjectNames);
                            Storage.featResultDescriptions.Add(GetFeatDescription(Utilities.GetBlueprintByGuid<BlueprintFeature>(stringFeatGuid)));
                        }
                    }
                }
                if (Storage.featValidDescriptions[i].Contains(Settings.featSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByFeatDescription == Storage.isTrueString)
                    {
                        if (!Storage.featResultGuids.Contains(stringFeatGuid))
                        {
                            Storage.featResultGuids.Add(stringFeatGuid);
                            Storage.featResultNames.Add(!string.IsNullOrEmpty(stringFeatName)
                                ? stringFeatName
                                : stringFeatObjectNames);
                            Storage.featResultDescriptions.Add(GetFeatDescription(Utilities.GetBlueprintByGuid<BlueprintFeature>(stringFeatGuid)));
                        }
                    }
                }
            }
        }

        public static void RefreshAllFeats(UnitEntityData unitEntityData)
        {
            Storage.featAllNames.Clear();
            Storage.featAllObjectNames.Clear();
            Storage.featAllRanks.Clear();
            Storage.featAllGuids.Clear();
            Storage.featAllDescriptions.Clear();

            for (var i = 0; i < unitEntityData.Descriptor.Progression.Features.Count; i++)
            {
                Storage.featAllNames.Add(unitEntityData.Descriptor.Progression.Features[i].Name);
                Storage.featAllObjectNames.Add(unitEntityData.Descriptor.Progression.Features[i].Blueprint.name);
                Storage.featAllRanks.Add(unitEntityData.Descriptor.Progression.Features[i].GetRank());
                Storage.featAllGuids.Add(unitEntityData.Descriptor.Progression.Features[i].Blueprint.AssetGuid);
                Storage.featAllDescriptions.Add(GetFeatDescription(unitEntityData.Descriptor.Progression.Features[i].Blueprint));
            }
        }

        public static void SortLocalisationsAndFlags(bool setSelection = false)
        {
            Storage.localeGrid.Clear();
            for (var i = 0; i < Storage.localisationsXMLFiles.Count; i++)
            {
                var locale = "";

                if (File.Exists(Storage.modEntryPath + Storage.localisationFolder + "\\" + Storage.localisationsXMLFiles[i] + ".xml"))
                {
                    try
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(Storage.modEntryPath + Storage.localisationFolder + "\\" + Storage.localisationsXMLFiles[i] + ".xml");
                        locale = (xmlDoc.SelectSingleNode("resources/Localisation/@value").Value);
                    }
                    catch (Exception e)
                    {
                        ModLogger.Log(e.ToString());
                    }
                }
                try
                {
                    switch (locale)
                    {
                        case "deDE":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXMLFiles[i], (Texture)Storage.flagsBundle.LoadAsset("deDE")));
                            break;
                        case "enGB":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXMLFiles[i], (Texture)Storage.flagsBundle.LoadAsset("enGB")));
                            break;
                        case "esES":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXMLFiles[i], (Texture)Storage.flagsBundle.LoadAsset("esES")));
                            break;
                        case "frFR":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXMLFiles[i], (Texture)Storage.flagsBundle.LoadAsset("frFR")));
                            break;
                        case "itIT":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXMLFiles[i], (Texture)Storage.flagsBundle.LoadAsset("itIT")));
                            break;
                        case "ruRU":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXMLFiles[i], (Texture)Storage.flagsBundle.LoadAsset("ruRU")));
                            break;
                        case "zhCN":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXMLFiles[i], (Texture)Storage.flagsBundle.LoadAsset("zhCN")));
                            break;
                        default:
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXMLFiles[i], (Texture)Storage.flagsBundle.LoadAsset("empty")));
                            break;
                    }
                }
                catch (Exception e)
                {

                    ModLogger.Log(e.ToString());
                }
                if (setSelection && String.Equals(LocalizationManager.CurrentLocale.ToString(), locale, StringComparison.OrdinalIgnoreCase))
                {
                    Settings.selectedLocalisation = i;
                }
            }
        }

        public static void UpdateSettings()
        {
            if (Common.CompareVersionStrings(Settings.modVersion, "1.9.0") == -1)
            {
                if (File.Exists(Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFileOld))
                {
                    try
                    {
                        File.Move(Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFileOld, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFile);
                        ModLogger.Log($"{Storage.favouritesTogglesFileOld} {Strings.GetText("message_RenamedTo")} {Storage.favouritesTogglesFile}.");

                    }
                    catch (Exception e)
                    {

                        ModLogger.Log(e.ToString());
                    }
                }
            }
            if (!Settings.cheatsCategories.Contains("BeneathTheStolenLands"))
            {
                var temp = Settings.cheatsCategories.ToList();
                temp.Add("BeneathTheStolenLands");
                Settings.cheatsCategories = temp.ToArray();
                ModLogger.Log("cheatsCategories\n+BeneathTheStolenLands");
            }
        }

        public static void RefreshTogglesFavourites()
        {
            Common.SerializeListString(Storage.togglesFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFile);
        }

        public static void ToggleActiveWarning(ref string toggleMain, ref string toggleOther, string buttonToggle)
        {
            if (toggleMain == Storage.isTrueString && toggleOther == Storage.isTrueString)
            {
                MenuTools.SingleLineLabel(RichText.Bold("'" + Strings.GetText(buttonToggle) + "' " + Strings.GetText("warning_AlsoEnabled")));
            }
        }

        public static void CreateDirectories()
        {
            Directory.CreateDirectory(Storage.modEntryPath + Storage.assetBundlesFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.charactersImportFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.favouritesFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.itemSetsFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.localisationFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.taxCollectorFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.modifiedBlueprintsFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.savesFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.exportFolder);

        }

        public static string IntToAlignment(int i)
        {
            var alignment = "none";
            switch (i)
            {
                case 0:
                    alignment = "lawfulgood";
                    break;
                case 1:
                    alignment = "neutralgood";
                    break;
                case 2:
                    alignment = "chaoticgood";
                    break;
                case 3:
                    alignment = "lawfulneutral";
                    break;
                case 4:
                    alignment = "trueneutral";
                    break;
                case 5:
                    alignment = "chaoticneutral";
                    break;
                case 6:
                    alignment = "lawfulevil";
                    break;
                case 7:
                    alignment = "neutralevil";
                    break;
                case 8:
                    alignment = "chaoticevil";
                    break;
            }
            return alignment;
        }

        public static void GetClassNames(UnitProgressionData prog)
        {
            if (!Storage.classNames.Any() || !Storage.classData.Any())
            {
                Storage.classNames.Clear();
                Storage.classData.Clear();
                var bcc = ResourcesLibrary.GetBlueprints<BlueprintCharacterClass>();
                foreach (var b in bcc)
                {
                    if (b.AssetGuid == "42a455d9ec1ad924d889272429eb8391" && !Common.DLCTieflings())
                    {

                    }
                    else
                    {
                        var cd = new ClassData(b);
                        var name = "";
                        if (!b.LocalizedName.IsEmpty() && b.LocalizedName != null)
                        {
                            name = b.LocalizedName;
                        }
                        else if (b.Name != "" && b.Name != null)
                        {
                            name = b.Name;
                        }
                        else
                        {
                            name = b.name;
                        }

                        if (prog.GetClassLevel(cd.CharacterClass) > 0)
                        {
                            name = name + $" ({prog.GetClassLevel(cd.CharacterClass)})";
                        }
                        Storage.classNames.Add(name);
                        Storage.classData.Add(cd);
                    }

                }
            }

        }

        public static class HyperTheurgeHelper
        {
            public static void SorcererSupreme()
            {
                var allCharacters = Game.Instance.Player.AllCharacters;
                foreach (var u in allCharacters)
                {
                    var books = u.Descriptor.Progression.Classes.Select((c) => u.Descriptor.GetSpellbook(c.CharacterClass)).Where((s) => s != null);
                    foreach (var sourceSpellbook in books)
                    {
                        for (var level = sourceSpellbook.FirstSpellbookLevel; level <= sourceSpellbook.LastSpellbookLevel; ++level)
                        {
                            foreach (var destinationSpellbook in books.Where((b) => b != sourceSpellbook))
                            {
                                if (level >= destinationSpellbook.FirstSpellbookLevel && level < destinationSpellbook.LastSpellbookLevel)
                                {
                                    foreach (var knownSpell in sourceSpellbook.GetKnownSpells(level))
                                    {
                                        var flag = !destinationSpellbook.IsKnown(knownSpell.Blueprint);
                                        var flag2 = true;// !destinationSpellbook.Blueprint.SpellList.Contains(knownSpell.Blueprint);
                                        if (flag && flag2)
                                        {
                                            destinationSpellbook.AddKnown(level, knownSpell.Blueprint, isCopy: true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public static void SorcererSupreme2()
            {
                var allCharacters = Game.Instance.Player.AllCharacters;
                var casterClasses = Game.Instance.BlueprintRoot.Progression.CharacterClasses.Where((c => c.Spellbook != null));
                var casterSpellsAll = casterClasses.SelectMany((c) =>
                {
                    var sb = c.Archetypes.Where((a) => a.ReplaceSpellbook != null).Select((a) => a.ReplaceSpellbook).ToList();
                    sb.Add(c.Spellbook);
                    return sb;
                }).SelectMany((b) => b.SpellList.SpellsByLevel).SelectMany((sll) => sll.Spells.Select((s) => new Tuple<int, BlueprintAbility>(sll.SpellLevel, s))).Distinct();
                foreach (var u in allCharacters)
                {
                    var books = u.Descriptor.Progression.Classes.Select((c) => u.Descriptor.GetSpellbook(c.CharacterClass)).Where((s) => s != null && s.Blueprint.IsArcane && s.Blueprint.Spontaneous && s.LastSpellbookLevel == 9);
                    foreach (var destinationSpellbook in books)
                    {
                        foreach (var sourceTuple in casterSpellsAll)
                        {
                            var level = sourceTuple.Item1;
                            var knownSpell = sourceTuple.Item2;

                            if (level >= destinationSpellbook.FirstSpellbookLevel && level <= destinationSpellbook.LastSpellbookLevel)
                            {
                                var flag = !destinationSpellbook.IsKnown(knownSpell);
                                if (flag)
                                {
                                    destinationSpellbook.AddKnown(level, knownSpell, isCopy: true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void RefreshAllAbilities(UnitEntityData unitEntityData)
        {
            Storage.addAbilitiesAllNames.Clear();
            Storage.addAbilitiesAllObjectNames.Clear();
            Storage.addAbilitiesAllGuids.Clear();
            Storage.addAbilitiesAllDescriptions.Clear();

            var test = new BlueprintAbility();

            for (var i = 0; i < unitEntityData.Descriptor.Abilities.Count; i++)
            {
                Storage.addAbilitiesAllNames.Add(unitEntityData.Descriptor.Abilities[i].Name);
                Storage.addAbilitiesAllObjectNames.Add(unitEntityData.Descriptor.Abilities[i].Blueprint.name);
                Storage.addAbilitiesAllGuids.Add(unitEntityData.Descriptor.Abilities[i].Blueprint.AssetGuid);
                Storage.addAbilitiesAllDescriptions.Add(GetFeatDescription(unitEntityData.Descriptor.Abilities[i].Blueprint));
            }
        }

        public static void RefreshAbilityFavourites()
        {
            Common.SerializeListString(Storage.abilitiesFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesAbilitiesFile);

            Storage.abilitiesFavouritesGuids = Storage.abilitiesFavourites;
            Storage.abilitiesFavouritesNames.Clear();
            Storage.abilitiesFavouritesDescriptions.Clear();
            for (var i = 0; i < Storage.abilitiesFavouritesGuids.Count; i++)
            {
                var stringAbilityName = Utilities.GetBlueprintByGuid<BlueprintAbility>(Storage.abilitiesFavourites[i]).Name;
                if (stringAbilityName != "" && stringAbilityName != null)
                {
                    Storage.abilitiesFavouritesNames.Add(stringAbilityName);
                }
                else
                {
                    var stringAbilityObjectNames = Utilities.GetBlueprintByGuid<BlueprintAbility>(Storage.abilitiesFavourites[i]).name;
                    Storage.abilitiesFavouritesNames.Add(stringAbilityObjectNames);
                }
                Storage.abilitiesFavouritesDescriptions.Add(Utilities.GetBlueprintByGuid<BlueprintAbility>(Storage.abilitiesFavourites[i]).Description);
            }
        }

        public static void SearchValidAbilities(List<string> validAbilityTypesList)
        {
            Storage.abilityValidObjectNames.Clear();
            Storage.abilityValidNames.Clear();
            Storage.abilityValidDescriptions.Clear();
            Storage.abilityValidGuids.Clear();
            Storage.abilityResultNames.Clear();
            Storage.abilityResultGuids.Clear();
            Storage.abilityResultDescriptions.Clear();
            Storage.abilityResultTypes.Clear();

            if (string.IsNullOrEmpty(Settings.abilitySearch)) return;
            Storage.currentAbilitySearch = Settings.abilitySearch;
            Storage.blueprintList = ResourcesLibrary.LibraryObject.GetAllBlueprints();
            foreach (var bso in Storage.blueprintList)
            {
                if (validAbilityTypesList.Contains(bso.GetType().Name))
                {
                    var bpAbility = bso as BlueprintAbility;
                    Storage.abilityValidObjectNames.Add(bpAbility.name);
                    Storage.abilityValidNames.Add(bpAbility.Name);
                    Storage.abilityValidDescriptions.Add(bpAbility.Description);
                    Storage.abilityValidGuids.Add(bpAbility.AssetGuid);
                }
            }

            for (var i = 0; i < Storage.abilityValidObjectNames.Count; i++)
            {
                var stringAblilityName = Storage.abilityValidNames[i];
                var stringAbilityObjectNames = Storage.abilityValidObjectNames[i];
                var stringAbilityGuid = Storage.abilityValidGuids[i];
                if (Storage.abilityValidObjectNames[i].Contains(Settings.abilitySearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByAbilityObjectName == Storage.isTrueString)
                    {
                        if (!Storage.abilityResultGuids.Contains(stringAbilityGuid))
                        {
                            Storage.abilityResultGuids.Add(stringAbilityGuid);
                            Storage.abilityResultNames.Add(!string.IsNullOrEmpty(stringAblilityName)
                                ? stringAblilityName
                                : stringAbilityObjectNames);
                            Storage.abilityResultDescriptions.Add(Utilities.GetBlueprintByGuid<BlueprintAbility>(stringAbilityGuid).Description);
                            Storage.abilityResultTypes.Add(Utilities.GetBlueprintByGuid<BlueprintAbility>(stringAbilityGuid).Type.ToString());
                        }
                    }
                }
                if (Storage.abilityValidNames[i].Contains(Settings.abilitySearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByAbilityName == Storage.isTrueString)
                    {
                        if (!Storage.abilityResultGuids.Contains(stringAbilityGuid))
                        {
                            Storage.abilityResultGuids.Add(stringAbilityGuid);
                            Storage.abilityResultNames.Add(!string.IsNullOrEmpty(stringAblilityName)
                                ? stringAblilityName
                                : stringAbilityObjectNames);
                            Storage.abilityResultDescriptions.Add(Utilities.GetBlueprintByGuid<BlueprintAbility>(stringAbilityGuid).Description);
                            Storage.abilityResultTypes.Add(Utilities.GetBlueprintByGuid<BlueprintAbility>(stringAbilityGuid).Type.ToString());
                        }
                    }
                }
                if (Storage.abilityValidDescriptions[i].Contains(Settings.abilitySearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Settings.toggleSearchByAbilityDescription == Storage.isTrueString)
                    {
                        if (!Storage.abilityResultGuids.Contains(stringAbilityGuid))
                        {
                            Storage.abilityResultGuids.Add(stringAbilityGuid);
                            Storage.abilityResultNames.Add(!string.IsNullOrEmpty(stringAblilityName)
                                ? stringAblilityName
                                : stringAbilityObjectNames);
                            Storage.abilityResultDescriptions.Add(Utilities.GetBlueprintByGuid<BlueprintAbility>(stringAbilityGuid).Description);
                            Storage.abilityResultTypes.Add(Utilities.GetBlueprintByGuid<BlueprintAbility>(stringAbilityGuid).Type.ToString());
                        }
                    }
                }
            }
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source?.IndexOf(value, comparisonType) >= 0;
        }

        static void OnSaveGui(ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }
    }
}