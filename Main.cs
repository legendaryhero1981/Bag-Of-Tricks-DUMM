using BagOfTricks.Utils;
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
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
        /// <summary>
        /// [1.14.5.13] 显示控件的文本提示
        /// </summary>
        internal static void ShowTooltip() => UnityModManager.UI.ShowTooltip();

        public static bool enabled;

        public static unsafe float* cameraScrollSpeed = null;
        public static LocalMap localMap = null;
        public static bool rotationChanged = false;

        public static Settings settings;
        public static TaxCollectorSettings taxSettings = new TaxCollectorSettings();
        public static SaveData saveData = new SaveData();
        public static ModEntry.ModLogger modLogger;
        public static ModEntryCheck scaleXp;
        public static ModEntryCheck craftMagicItems;
        public static ModEntryCheck advancedMartialArts;
        public static ModEntryCheck callOfTheWild;
        public static ModEntryCheck eldritchArcana;
        public static bool versionMismatch = false;
        public static Vector2 scrollPosition;

        public static UICanvas mainCanvas = new UICanvas("BoT_MainCanvas");
        public static UITMPText sceneAreaInfo = new UITMPText(mainCanvas.baseGameObject, "BoT_SceneAndAreaInfo");
        public static UITMPText objectInfo = new UITMPText(mainCanvas.baseGameObject, "BoT_SceneAndAreaInfo");

        public static Logger botLoggerLog;
        public static Logger battleLoggerLog;
        public static HtmlLogger battleLoggerHtml;

        internal static bool Load(ModEntry modEntry)
        {
            objectInfo.Size(25);
            objectInfo.Off();

            sceneAreaInfo.Size(25);
            sceneAreaInfo.Off();

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            modLogger = modEntry.Logger;

            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            Storage.modEntryPath = modEntry.Path;

            if (File.Exists(modEntry.Path + "debug"))
            {
                settings.settingShowDebugInfo = true;
                settings.toggleAutomaticallyLoadLastSave = Storage.isTrueString;
            }

            if (File.Exists(modEntry.Path + "devtools"))
            {
                settings.toggleDevTools = Storage.isTrueString;
            }

            if (File.Exists(modEntry.Path + "fastload"))
            {
                settings.toggleAutomaticallyLoadLastSave = Storage.isTrueString;
            }

            try
            {
                CreateDirectories();
            }
            catch (Exception e)
            {
                modLogger.Log(e.ToString());
            }

            if (File.Exists(modEntry.Path + "dictxml"))
            {
                SerializeFallback();
            }

            if (Strings.ToBool(settings.toggleActionKeyLogInfo) || (Strings.ToBool(settings.toggleCreateBattleLogFile) && Strings.ToBool(settings.toggleCreateBattleLogFileBotLog)))
            {
                LoggerUtils.InitBagOfTrickLogger();
            }

            if (Strings.ToBool(settings.toggleCreateBattleLogFile))
            {

                if (Strings.ToBool(settings.toggleCreateBattleLogFileLog))
                {
                    LoggerUtils.InitBattleLogDefaultLogger();
                }

                if (Strings.ToBool(settings.toggleCreateBattleLogFileHtml))
                {
                    LoggerUtils.InitBattleLogHtmlLogger();
                }
            }

            scaleXp = new ModEntryCheck("ScaleXP");
            craftMagicItems = new ModEntryCheck("CraftMagicItems");
            advancedMartialArts = new ModEntryCheck("AdvancedMartialArts");
            callOfTheWild = new ModEntryCheck("CallOfTheWild");
            eldritchArcana = new ModEntryCheck("EldritchArcana");

            Storage.flagsBundle = AssetBundle.LoadFromFile(Storage.modEntryPath + Storage.assetBundlesFolder + "\\" + Storage.assetBunldeFlags);

            Storage.modAssemblyName = modEntry.Info.AssemblyName;

            if (Strings.ToBool(settings.toggleEnableTaxCollector))
            {
                taxSettings = TaxCollector.Deserialize(Storage.modEntryPath + Storage.taxCollectorFolder + "\\" + Storage.taxCollectorFile);
                TaxCollector.saveTimeGame = taxSettings.saveTime;
            }

            if (Storage.gamerVersionAtCreation != GameVersion.GetVersion())
            {
                modLogger.Log(Strings.GetText("warning_GameVersion") + $": { GameVersion.GetVersion()}");
                modLogger.Log(Strings.GetText("warning_GameVersion") + " " + Strings.Parenthesis(Strings.GetText("warning_Mod")) + $": {Storage.gamerVersionAtCreation}");
                versionMismatch = true;
            }

            LoadFavourites();
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);

            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                modLogger.Log(e.ToString());
                harmony.UnpatchAll(modEntry.Info.Id);
                return false;
            }

            settings.heavyEncumbranceMultiplier = Mathf.Clamp(settings.heavyEncumbranceMultiplier, 1f, 100f);
            settings.customHeavyEncumbranceMultiplier = settings.finalCustomHeavyEncumbranceMultiplier.ToString();
            settings.fatigueHoursModifierMultiplier = Mathf.Clamp(settings.fatigueHoursModifierMultiplier, 0.1f, 30f);
            settings.customFatigueHoursModifierMultiplier = settings.finalCustomFatigueHoursModifierMultiplier.ToString();
            settings.experienceMultiplier = Mathf.Clamp(settings.experienceMultiplier, 0.1f, 10f);
            settings.customExperienceMultiplier = settings.finalCustomExperienceMultiplier.ToString();
            settings.moneyMultiplier = Mathf.Clamp(settings.moneyMultiplier, 0.1f, 10f);
            settings.customMoneyMultiplier = settings.finalCustomMoneyMultiplier.ToString();
            settings.spellsPerDayMultiplier = Mathf.Clamp(settings.spellsPerDayMultiplier, 0.1f, 4f);
            settings.customSpellsPerDayMultiplier = settings.finalCustomspellsPerDayMultiplier.ToString();
            settings.featMultiplierString = settings.featMultiplier.ToString();
            settings.selectedKingdomAlignmentString = settings.selectedKingdomAlignment.ToString();
            settings.selectedKingdomAlignmentTranslated = IntToAlignment(settings.selectedKingdomAlignment);
            settings.travelSpeedMultiplier = Mathf.Clamp(settings.travelSpeedMultiplier, 0.1f, 100f);
            settings.travelSpeedMultiplierString = settings.travelSpeedMultiplier.ToString();
            settings.characterCreationAbilityPointsMax = Math.Max(18, settings.characterCreationAbilityPointsMax);
            settings.characterCreationAbilityPointsMaxString = settings.characterCreationAbilityPointsMax.ToString();
            settings.characterCreationAbilityPointsMin = Math.Max(0, settings.characterCreationAbilityPointsMin);
            settings.characterCreationAbilityPointsMinString = settings.characterCreationAbilityPointsMin.ToString();
            settings.characterCreationAbilityPointsPlayer = Math.Max(0, settings.characterCreationAbilityPointsPlayer);
            settings.characterCreationAbilityPointsPlayerString = settings.characterCreationAbilityPointsPlayer.ToString();
            settings.characterCreationAbilityPointsMerc = Math.Max(0, settings.characterCreationAbilityPointsMerc);
            settings.characterCreationAbilityPointsMercString = settings.characterCreationAbilityPointsMerc.ToString();
            settings.companionCostMultiplier = Math.Max(0, settings.companionCostMultiplier);
            settings.companionCostMultiplierString = settings.companionCostMultiplier.ToString();
            settings.partyStatMultiplier = Mathf.Clamp(settings.partyStatMultiplier, 0.1f, 10f);
            settings.enemyStatMultiplier = Mathf.Clamp(settings.enemyStatMultiplier, 0.1f, 10f);
            settings.randomEncounterSettingFloatAmountLimited = Mathf.Clamp(settings.randomEncounterSettingFloatAmountLimited, 0f, 1f);
            settings.takeXCustom = Mathf.Clamp(settings.takeXCustom, 1f, 20f);
            settings.kingdomBuildingTimeModifier = Mathf.Clamp(settings.kingdomBuildingTimeModifier, -10f, 10f);
            settings.vendorSellPriceMultiplier = settings.finalVendorSellPriceMultiplier.ToString();
            settings.sillyBloodChance = Mathf.Clamp(settings.sillyBloodChance, 0f, 1f);
            settings.debugTimeMultiplier = Mathf.Clamp(settings.debugTimeMultiplier, 0.1f, 30f);
            settings.customDebugTimeMultiplier = settings.finalCustomDebugTimeMultiplier.ToString();
            settings.repeatableLockPickingWeariness = settings.finalRepeatableLockPickingWeariness.ToString();
            settings.artisanMasterpieceChance = Mathf.Clamp(settings.artisanMasterpieceChance, 0f, 1f);
            settings.unityModManagerButtonIndex = Mathf.Clamp(settings.unityModManagerButtonIndex, 0f, 6f);

            Storage.flyingHeightSlider = Mathf.Clamp(Storage.flyingHeightSlider, -10f, 100f);
            Storage.itemFavouritesGuids = Storage.itemFavourites;
            Storage.buffFavouritesGuids = Storage.buffFavourites;
            Storage.featFavouritesGuids = Storage.featFavourites;
            Storage.abilitiesFavouritesGuids = Storage.abilitiesFavourites;

            SpawnUnits.FavouritesGuids = SpawnUnits.UnitsFavourites;

            if (!settings.settingKeepGuids)
            {
                settings.itemGuid = "";
            }

            try
            {
                if (File.Exists(Storage.modEntryPath + Storage.localisationFolder + "\\" + settings.selectedLocalisationName + ".xml"))
                {
                    Strings.temp = Strings.XmLtoDict(settings.selectedLocalisationName);
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
                    settings.filterButtonText = Strings.GetText("misc_Enable");
                    settings.currentSceneString = Strings.GetText("misc_Display");
                    settings.showAllBuffs = Strings.GetText("misc_Display");
                    settings.showAllFeats = Strings.GetText("misc_Display");
                    settings.showAllAbilities = Strings.GetText("misc_Display");
                }
                Storage.localisationsXmlFiles.Clear();
                Storage.localisationsXml = Directory.GetFiles(Storage.modEntryPath + Storage.localisationFolder, "*.xml");
                foreach (var s in Storage.localisationsXml)
                {
                    Storage.localisationsXmlFiles.Add(Path.GetFileNameWithoutExtension(s));
                }

                if (Storage.localisationsXmlFiles.Count - 1 < settings.selectedLocalisation)
                {
                    settings.selectedLocalisation = 0;
                }
                if (Storage.localisationsXmlFiles.Contains(settings.selectedLocalisationName))
                {
                    settings.selectedLocalisation = Storage.localisationsXmlFiles.FindIndex(a => a.Contains(settings.selectedLocalisationName));
                }
                SortLocalisationsAndFlags();

                Storage.selectedLocalisationOld = settings.selectedLocalisation;

                if (settings.settingSearchForCsv)
                {
                    Common.RefreshArrayFromFile(ref Storage.itemSetsCsv, Storage.itemSetsFolder, "csv");
                    string[] temp = SpawnUnits.UnitSetsCsv;
                    Common.RefreshArrayFromFile(ref temp, SpawnUnits.UnitSetsFolder, "csv");
                    SpawnUnits.UnitSetsCsv = temp;
                }
                if (settings.settingSearchForTxt)
                {
                    Common.RefreshArrayFromFile(ref Storage.itemSetsTxt, Storage.itemSetsFolder, "txt");
                    string[] temp = SpawnUnits.UnitSetsTxt;
                    Common.RefreshArrayFromFile(ref temp, SpawnUnits.UnitSetsFolder, "txt");
                    SpawnUnits.UnitSetsTxt = temp;
                }
            }
            catch (IOException exception)
            {
                modLogger.Log(exception.ToString());
            }

            if (!settings.settingKeepCategories)
            {
                settings.showCarryMoreCategory = false;
                settings.showRestLessCategory = false;
                settings.showMoneyCategory = false;
                settings.showExperienceCategory = false;
                settings.showKingdomCategory = false;
                settings.showItemsCategory = false;
                settings.showSpellCategory = false;
                settings.showBuffsCategory = false;
                settings.showPartyStatisticsCategory = false;
                settings.showEnemyStatisticsCategory = false;
                settings.showFeatsCategory = false;
                settings.showAddAbilitiesCategory = false;
                settings.showSpellSpellbooksCategory = false;
                settings.showTravelAndMapCategory = false;
                settings.showDiceRollCategory = false;
                settings.showFogOfWarCategory = false;
                settings.showMiscCategory = false;
                settings.showKeyBindingsCategory = false;
                settings.showTaxCollectorCategory = false;
                settings.showBeneathTheStolenLandsCategory = false;
                settings.showSpawnUnitsCategory = false;
            }

            if (!settings.settingKeepSubMenus)
            {
                settings.showItemInfo = false;
                settings.showItemSets = false;
                settings.multipleItems = false;
                settings.moneyAmount = "1";
                settings.finalMoneyAmount = 1;
                settings.experienceAmount = "1";
                settings.finalExperienceAmount = 1;
                settings.buildPointAmount = "1";
                settings.finalBuildPointAmount = 1;
                settings.itemAmount = "1";
                settings.finalItemAmount = 1;
                settings.itemSearch = "";
                settings.itemAmount = "1";
                settings.finalItemAmount = 1;
                settings.toggleFilterWeapons = Storage.isTrueString;
                settings.toggleFilterShields = Storage.isTrueString;
                settings.toggleFilterArmours = Storage.isTrueString;
                settings.toggleFilterRings = Storage.isTrueString;
                settings.toggleFilterBelts = Storage.isTrueString;
                settings.toggleFilterFootwear = Storage.isTrueString;
                settings.toggleFilterGloves = Storage.isTrueString;
                settings.toggleFilterHeadwear = Storage.isTrueString;
                settings.toggleFilterNeckItems = Storage.isTrueString;
                settings.toggleFilterShoulderItems = Storage.isTrueString;
                settings.toggleFilterWristItems = Storage.isTrueString;
                settings.toggleFilterKeys = Storage.isTrueString;
                settings.toggleFilterMiscellaneousItems = Storage.isTrueString;
                settings.toggleFilterNotes = Storage.isTrueString;
                settings.toggleFilterUsableItems = Storage.isTrueString;
                settings.showFilters = false;
                settings.filterButtonText = Strings.GetText("misc_Enable");
                settings.toggleSearchOnlyCraftMagicItems = Storage.isFalseString;
                settings.toggleSearchByItemObjectName = Storage.isTrueString;
                settings.toggleSearchByItemName = Storage.isTrueString;
                settings.toggleSearchByItemDescription = Storage.isFalseString;
                settings.toggleSearchByItemFlavorText = Storage.isFalseString;
                settings.showItemFavourites = false;
                settings.addItemIdentified = false;
                settings.showExperimentalCategory = false;
                settings.flyingHeight = "0.1";
                settings.finalflyingHeight = 0.1f;
                settings.flyingHeightUseSlider = false;
                settings.gameConstsAmount = "1";
                settings.finalGameConstsAmount = 1;
                settings.showAllBuffs = Strings.GetText("misc_Display");
                settings.showBuffFavourites = false;
                settings.buffSearch = "";
                settings.toggleSearchByBuffObjectName = Storage.isTrueString;
                settings.toggleSearchByBuffName = Storage.isTrueString;
                settings.toggleSearchByBuffDescription = Storage.isFalseString;
                settings.toggleSearchByBuffComponents = Storage.isFalseString;
                settings.toggleBuffsShowEnemies = Storage.isFalseString;
                settings.partyStatsAmount = "10";
                settings.partyFinalStatsAmount = 10;
                settings.partyStatMultiplier = 1f;
                settings.toggleShowOnlyClassSkills = Storage.isFalseString;
                settings.enemyStatsAmount = "10";
                settings.enemyFinalStatsAmount = 10;
                settings.enemyStatMultiplier = 1f;
                settings.toggleForcedEncounterIsHard = Storage.isFalseString;
                settings.forcedEncounterCr = "1";
                settings.forcedEncounterFinalCr = 1;
                settings.forcedEncounterSelectedAvoidance = 0;
                settings.forcedEncounterSelectedBlueprintMode = 0;
                settings.forcedEncounterGuid = "";
                settings.randomEncounterSettingsShow = false;
                settings.randomEncounterSettingFloatAmountLimited = 0.1f;
                settings.randomEncounterSettingFloatAmount = "1";
                settings.randomEncounterSettingFloatAmountFinal = 1f;
                settings.randomEncounterSettingIntAmount = "1";
                settings.randomEncounterSettingIntAmountFinal = 1;
                settings.kingdomBuildingTimeModifier = 0f;
                settings.showSoundsCategory = false;
                settings.showLanguageMenu = false;
                settings.sillyBloodChance = 0.1f;
                settings.showFeatsFavourites = false;
                settings.featSearch = "";
                settings.toggleSearchByFeatObjectName = Storage.isTrueString;
                settings.toggleSearchByFeatName = Storage.isTrueString;
                settings.toggleSearchByFeatDescription = Storage.isFalseString;
                settings.toggleFeatsShowEnemies = Storage.isFalseString;
                settings.showAllFeats = Strings.GetText("misc_Display");
                settings.showRomanceCounters = false;
                settings.showRomanceCountersSpoilers = false;
                settings.featsParamShowAllWeapons = false;
                settings.showUpgradSettlementsAndBuildings = false;
                settings.showRomanceCountersExperimental = false;
                settings.romanceCounterSetValue = "1";
                settings.finalRomanceCounterSetValue = 1;
                settings.damageDealtMultipliersValue = "1";
                settings.finalDamageDealtMultipliersValue = 1;
                settings.toggleShowClassDataOptions = Storage.isFalseString;
                settings.showAllAbilities = Strings.GetText("misc_Display");
                settings.showAbilitiesFavourites = false;
                settings.abilitySearch = "";
                settings.toggleSearchByAbilityObjectName = Storage.isTrueString;
                settings.toggleSearchByAbilityName = Storage.isTrueString;
                settings.toggleSearchByAbilityDescription = Storage.isFalseString;
                settings.showAnimationsCategory = false;
                settings.unitSearch = "";
                settings.unitSearchFilters = "";
                settings.toggleSearchByUnitObjectName = Storage.isTrueString;
                settings.toggleSearchByUnitCharacterName = Storage.isTrueString;
                settings.toggleSearchByUnitType = Storage.isFalseString;
                settings.showUnitsFavourites = false;
                settings.showUnitSets = false;
            }

            if (settings.gameConstsMinUnitSpeedMps != -1)
            {
                var minUnitSpeedMps = typeof(GameConsts).GetField("MinUnitSpeedMps");
                minUnitSpeedMps.SetValue(null, settings.gameConstsMinUnitSpeedMps);
            }
            if (settings.gameConstsMinWeaponRange != -1)
            {
                var minWeaponRange = typeof(GameConsts).GetField("MinWeaponRange");
                minWeaponRange.SetValue(null, settings.gameConstsMinWeaponRange.Feet());
            }
            if (settings.gameConstsMinWeaponRange != -1)
            {
                var stealthDcIncrement = typeof(GameConsts).GetField("StealthDCIncrement");
                stealthDcIncrement.SetValue(null, settings.gameConstsMinWeaponRange.Feet());
            }

            if (Strings.ToBool(settings.toggleUberLogger))
            {
                UberLogger.Logger.Enabled = true;
                if (Strings.ToBool(settings.toggleUberLoggerForward))
                {
                    UberLogger.Logger.ForwardMessages = true;
                }
            }

            if (!BuildModeUtility.IsDevelopment && Strings.ToBool(settings.toggleCombatDifficultyMessage))
            {
                EventBus.Subscribe(new CombatDifficultyMessage());
            }

            if (settings.settingShowDebugInfo)
            {
                LogModInfoLoad();
            }

            if (SettingsBoT.IsEarlierVersion(modEntry.Info.Version))
            {
                SettingsBoT.Update();
                settings.modVersion = modEntry.Info.Version;
            }

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGui;
            modEntry.OnSaveGUI = OnSaveGui;

            ModEntry = modEntry;

            return true;
        }

        static bool OnToggle(ModEntry modEntry, bool value)
        {
            enabled = value;

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
            else if (!settings.firstLaunch && !Storage.settingsWarning)
            {
                if (Game.Instance.Player.ControllableCharacters.Any())
                {
                    if (versionMismatch && !Storage.hideVersionMismatch)
                    {
                        Menu.VersionMismatch();
                    }

                    GL.Space(10);

                    settings.mainToolbarIndex = GL.Toolbar(settings.mainToolbarIndex, Storage.mainToolbarStrings, WindowWidth);

                    if (Strings.ToBool(settings.toggleBoTScrollBar))
                    {
                        scrollPosition = GL.BeginScrollView(scrollPosition, GL.Width(Storage.ummWidth - 24f));
                    }
                    switch (settings.mainToolbarIndex)
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

                    if (Strings.ToBool(settings.toggleBoTScrollBar))
                    {
                        GL.EndScrollView();
                    }
                }
                else
                {
                    if (versionMismatch && !Storage.hideVersionMismatch)
                    {
                        Menu.VersionMismatch();
                    }
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("message_NotInGame"));
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    if (settings.currentSceneString == Strings.GetText("misc_Hide"))
                    {
                        var currenctSceneName = SceneManager.GetActiveScene().name;
                        GL.Label(Strings.GetText("label_CurrentScene") + $": { currenctSceneName}");
                    }
                    GL.EndHorizontal();
                }
            }
            else if (settings.firstLaunch && SceneManager.GetActiveScene().name != "Start" && !Storage.settingsWarning)
            {
                if (versionMismatch && !Storage.hideVersionMismatch)
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
                        Storage.localisationsXmlFiles.Clear();
                        Storage.localisationsXml = Directory.GetFiles(modEntry.Path + Storage.localisationFolder, "*.xml");
                        foreach (var s in Storage.localisationsXml)
                        {
                            Storage.localisationsXmlFiles.Add(Path.GetFileNameWithoutExtension(s));
                        }
                        SortLocalisationsAndFlags();
                    }
                    catch (IOException exception)
                    {
                        modLogger.Log(exception.ToString());
                    }

                    if (Storage.localisationsXmlFiles.Count - 1 < settings.selectedLocalisation)
                    {
                        settings.selectedLocalisation = 0;
                    }
                }
                GL.EndHorizontal();
                GL.Space(10);
                GL.BeginHorizontal();
                settings.selectedLocalisation = GL.SelectionGrid(settings.selectedLocalisation, Storage.localeGrid.ToArray(), 1, GL.ExpandWidth(false));
                GL.EndHorizontal();
                if (settings.selectedLocalisation != Storage.selectedLocalisationOld)
                {
                    Storage.selectedLocalisationOld = settings.selectedLocalisation;
                    settings.selectedLocalisationName = Path.GetFileNameWithoutExtension(Storage.localisationsXmlFiles.ToArray()[settings.selectedLocalisation]);
                    if (File.Exists(Storage.modEntryPath + Storage.localisationFolder + "\\" + settings.selectedLocalisationName + ".xml"))
                    {
                        Strings.temp = Strings.XmLtoDict(settings.selectedLocalisationName);
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
                        settings.filterButtonText = Strings.GetText("misc_Enable");
                        settings.currentSceneString = Strings.GetText("misc_Display");
                        settings.showAllBuffs = Strings.GetText("misc_Display");
                        settings.showAllFeats = Strings.GetText("misc_Display");
                        settings.showAllAbilities = Strings.GetText("misc_Display");

                    }
                }

                GL.Space(10);

                if (GL.Button(RichText.Bold(Strings.GetText("button_Confirm")), GL.ExpandWidth(false)))
                {
                    settings.firstLaunch = false;
                }
            }
            else
            {
                if (versionMismatch && !Storage.hideVersionMismatch)
                {
                    Menu.VersionMismatch();
                }
                MenuTools.SingleLineLabel(Strings.GetText("message_GameLoading"));
            }

            if (settings.toggleShowTooltips == Storage.isTrueString) ShowTooltip();
        }

        public static List<string> BlueprintsByTypes(string[] validTypes)
        {
            return ResourcesLibrary.LibraryObject.GetAllBlueprints().AsParallel().WithMergeOptions(ParallelMergeOptions.FullyBuffered)
                .Where(e => validTypes.Contains(e.GetType().Name))
                .Select(e => e.AssetGuid).ToList();
        }

        public static void CreateFilteredItemSet(string labelString, string[] types)
        {
            if (!GL.Button(labelString, GL.ExpandWidth(false))) return;
            ModEntry.OnModActions.Push(m =>
                BlueprintsByTypes(types).AsParallel().WithMergeOptions(ParallelMergeOptions.FullyBuffered)
                    .Where(e => ExcludeGuid != e && null != Utilities.GetBlueprintByGuid<BlueprintItem>(e))
                    .ForEach(item => MenuTools.AddSingleItemAmount(item, 1, settings.addItemIdentified)
            ));
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
                var lines = Array.Empty<string>();
                try
                {
                    lines = File.ReadAllLines(files[i]);

                }
                catch (FileNotFoundException e)
                {
                    modLogger.Log(e.ToString());
                }
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
                                    MenuTools.AddSingleItemAmount(splitLine[0], 1, settings.addItemIdentified);
                                else if ("0" != splitLine[1] && int.TryParse(splitLine[1], out var number))
                                    MenuTools.AddSingleItemAmount(splitLine[0], number, settings.addItemIdentified);
                            }
                            else if (ExcludeGuid != lines[j])
                                MenuTools.AddSingleItemAmount(lines[j], 1, settings.addItemIdentified);
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

            if (string.IsNullOrEmpty(settings.itemSearch)) return;
            Storage.currentItemSearch = settings.itemSearch;
            Storage.blueprintList = ResourcesLibrary.LibraryObject.GetAllBlueprints();
            foreach (var bso in Storage.blueprintList)
            {
                if (!validItemTypesList.Contains(bso.GetType().Name)) continue;
                var bpItem = bso as BlueprintItem;
                if (bpItem.name == "EkunQ2_rewardArmor") continue;
                if (settings.toggleSearchOnlyCraftMagicItems == Storage.isTrueString)
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
                if (Storage.validItemObjectNames[i].Contains(settings.itemSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByItemObjectName == Storage.isTrueString)
                    {
                        if (!Storage.resultItemGuids.Contains(stringGuid))
                        {
                            Storage.resultItemGuids.Add(stringGuid);
                            Storage.resultItemNames.Add(stringItemName);
                        }
                    }
                }
                if (Storage.validItemNames[i].Contains(settings.itemSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByItemName == Storage.isTrueString)
                    {
                        if (!Storage.resultItemGuids.Contains(stringGuid))
                        {
                            Storage.resultItemGuids.Add(stringGuid);
                            Storage.resultItemNames.Add(stringItemName);
                        }
                    }
                }
                if (Storage.validItemDescriptions[i].Contains(settings.itemSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByItemDescription == Storage.isTrueString)
                    {
                        if (!Storage.resultItemGuids.Contains(stringGuid))
                        {
                            Storage.resultItemGuids.Add(stringGuid);
                            Storage.resultItemNames.Add(stringItemName);
                        }
                    }
                }
                if (Storage.validItemFlavorTexts[i].Contains(settings.itemSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByItemFlavorText == Storage.isTrueString)
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
            Storage.itemFavouritesNames.Clear();
            Storage.toggleItemFavouriteDescription.Clear();
            for (var i = 0; i < Storage.itemFavouritesGuids.Count; i++)
            {
                Storage.itemFavouritesNames.Add(Utilities.GetBlueprintByGuid<BlueprintItem>(Storage.itemFavourites[i]).Name);
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

            if (string.IsNullOrEmpty(settings.buffSearch)) return;
            Storage.currentBuffSearch = settings.buffSearch;
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
                if (Storage.buffValidObjectNames[i].Contains(settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByBuffObjectName == Storage.isTrueString)
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
                if (Storage.buffValidNames[i].Contains(settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByBuffName == Storage.isTrueString)
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
                if (Storage.buffValidDescriptions[i].Contains(settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByBuffDescription == Storage.isTrueString)
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
                if (Storage.buffValidComponents[i].Contains(settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByBuffComponents == Storage.isTrueString)
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
            Storage.buffFavouritesNames.Clear();
            Storage.buffFavouritesDescriptions.Clear();
            for (var i = 0; i < Storage.buffFavouritesGuids.Count; i++)
            {
                var stringBuffName = Utilities.GetBlueprintByGuid<BlueprintBuff>(Storage.buffFavourites[i]).Name;
                if (stringBuffName != "" && stringBuffName != null)
                {
                    Storage.buffFavouritesNames.Add(stringBuffName);
                }
                else
                {
                    var stringBuffObjectNames = Utilities.GetBlueprintByGuid<BlueprintBuff>(Storage.buffFavourites[i]).name;
                    Storage.buffFavouritesNames.Add(stringBuffObjectNames);
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

            if (settings.buffSearch != "")
            {
                Storage.currentBuffSearch = settings.buffSearch;
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
                    if (Storage.buffValidObjectNames[i].Contains(settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (settings.toggleSearchByBuffObjectName == Storage.isTrueString)
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
                    if (Storage.buffValidNames[i].Contains(settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (settings.toggleSearchByBuffName == Storage.isTrueString)
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
                    if (Storage.buffValidDescriptions[i].Contains(settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (settings.toggleSearchByBuffDescription == Storage.isTrueString)
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
                    if (Storage.buffValidComponents[i].Contains(settings.buffSearch, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (settings.toggleSearchByBuffComponents == Storage.isTrueString)
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
            var re = Game.Instance.BlueprintRoot.RE;
            state.NextEncounterRollMiles = state.MilesTravelled + re.SafeMilesAfterEncounter;
            var randomEncounterData = new RandomEncounterData(blueprint, position)
            {
                CR = cr
            };
            if (isCamp)
                randomEncounterData.AvoidanceCheckResult = RandomEncounterAvoidanceCheckResult.Fail;
            if (blueprint.AvoidType == EncounterAvoidType.SkillCheck)
            {
                switch (Main.settings.forcedEncounterSelectedAvoidance)
                {
                    case 0:
                        var rule = Rulebook.Trigger<RulePartySkillCheck>(new RulePartySkillCheck(blueprint.AvoidSkill, blueprint.AvoidDC));
                        if (rule == null || rule.IsPassed)
                        {
                            randomEncounterData.AvoidanceCheckResult = RandomEncounterAvoidanceCheckResult.Success;
                        }
                        else
                        {
                            randomEncounterData.AvoidanceCheckResult = rule.DifficultyClass - rule.RollResult < re.RandomEncounterAvoidanceFailMargin ? RandomEncounterAvoidanceCheckResult.Fail : RandomEncounterAvoidanceCheckResult.CriticalFail;
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
                if (!randomEncounterData.SpawnersGroups.All<SpawnersGroupData>((Func<SpawnersGroupData, bool>)(g => g.Spawners.All<RandomSpawnerData>((Func<RandomSpawnerData, bool>)(s => (bool)((SerializedScriptableObject)s.Unit))))))
                {
                    UberDebug.LogWarningChannel("RE", string.Format("Can't start encounter: {0} CR{1}", (object)blueprint, (object)cr), (object[])Array.Empty<object>());
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
            if (File.Exists(Storage.modEntryPath + Storage.favouritesFolder + "\\" + SpawnUnits.favouritesFile))
            {
                Common.DeserializeListString(SpawnUnits.UnitsFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + SpawnUnits.favouritesFile);
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
            modLogger.Log("Current Culture: " + CultureInfo.CurrentCulture.ToString());
            modLogger.Log("Game Version: " + GameVersion.GetVersion());
            modLogger.Log($"Game Version (Mod): {Storage.gamerVersionAtCreation}");
            modLogger.Log($"Mod Version: {new ModEntryCheck("BagOfTricks").Version()}");
            modLogger.Log($"Mod Version (Settings.xml): {settings.modVersion}");
            modLogger.Log("Mod Assembly Name: " + Storage.modAssemblyName);
            modLogger.Log("Mod Assembly MD5: " + ModMd5(Storage.modEntryPath + "\\" + Storage.modAssemblyName));
            modLogger.Log("ItemSets Folder: " + Storage.modEntryPath + Storage.itemSetsFolder);
            modLogger.Log("Favourites Folder: " + Storage.modEntryPath + Storage.favouritesFolder);
            modLogger.Log("Characters Folder: " + Storage.modEntryPath + Storage.charactersImportFolder);
            modLogger.Log("Tax Collector Folder: " + Storage.modEntryPath + Storage.taxCollectorFolder);
            modLogger.Log("Modified Blueprints Folder: " + Storage.modEntryPath + Storage.modifiedBlueprintsFolder);
            modLogger.Log("ScaleXP Installed: " + scaleXp.IsInstalled());
            modLogger.Log("CraftMagicItems Installed: " + craftMagicItems.IsInstalled());
            modLogger.Log("AdvancedMartialArts Installed: " + advancedMartialArts.IsInstalled());
            modLogger.Log("CallOfTheWild Installed: " + callOfTheWild.IsInstalled());
            modLogger.Log("EldritchArcana Installed: " + eldritchArcana.IsInstalled());
            modLogger.Log("Localisation Folder: " + Storage.modEntryPath + Storage.localisationFolder);


            foreach (var s in Storage.localisationsXml)
            {
                modLogger.Log("Localisations: " + s);
            }

            modLogger.Log("AssetBundles Folder: " + Storage.modEntryPath + Storage.assetBundlesFolder);

            foreach (var s in Storage.flagsBundle.GetAllAssetNames())
            {
                modLogger.Log("Flag Assets: " + s);
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
            if (Strings.ToBool(settings.toggleEnableRandomEncounterSettings))
            {
                if (settings.toggleRandomEncountersEnabled == Storage.isFalseString)
                {
                    Game.Instance.BlueprintRoot.RE.EncountersEnabled = false;
                }

                if (settings.toggleRandomEncountersEnabled == Storage.isTrueString)
                {
                    Game.Instance.BlueprintRoot.RE.EncountersEnabled = true;
                }

                if (settings.randomEncounterChanceOnGlobalMap != Defaults.randomEncounterChanceOnGlobalMap)
                {
                    Game.Instance.BlueprintRoot.RE.ChanceOnGlobalMap = settings.randomEncounterChanceOnGlobalMap;
                }

                if (settings.randomEncounterChanceOnCamp != Defaults.randomEncounterChanceOnCamp)
                {
                    Game.Instance.BlueprintRoot.RE.ChanceOnCamp = settings.randomEncounterChanceOnCamp;
                }

                if (settings.randomEncounterChanceOnCampSecondTime != Defaults.randomEncounterChanceOnCampSecondTime)
                {
                    Game.Instance.BlueprintRoot.RE.ChanceOnCampSecondTime =
                        settings.randomEncounterChanceOnCampSecondTime;
                }

                if (settings.randomEncounterHardEncounterChance != Defaults.randomEncounterHardEncounterChance)
                {
                    Game.Instance.BlueprintRoot.RE.HardEncounterChance = settings.randomEncounterHardEncounterChance;
                }

                if (settings.randomEncounterHardEncounterMaxChance != Defaults.randomEncounterHardEncounterMaxChance)
                {
                    Game.Instance.BlueprintRoot.RE.HardEncounterMaxChance =
                        settings.randomEncounterHardEncounterMaxChance;
                }

                if (settings.randomEncounterHardEncounterChanceIncrease !=
                    Defaults.randomEncounterHardEncounterChanceIncrease)
                {
                    Game.Instance.BlueprintRoot.RE.HardEncounterChanceIncrease =
                        settings.randomEncounterHardEncounterChanceIncrease;
                }

                if (settings.randomEncounterStalkerAmbushChance != Defaults.randomEncounterStalkerAmbushChance)
                {
                    Game.Instance.BlueprintRoot.RE.StalkerAmbushChance = settings.randomEncounterStalkerAmbushChance;
                }

                if (settings.randomEncounterRollMiles != Defaults.randomEncounterRollMiles)
                {
                    Game.Instance.BlueprintRoot.RE.RollMiles = settings.randomEncounterRollMiles;
                }

                if (settings.randomEncounterSafeMilesAfterEncounter != Defaults.randomEncounterSafeMilesAfterEncounter)
                {
                    Game.Instance.BlueprintRoot.RE.SafeMilesAfterEncounter =
                        settings.randomEncounterSafeMilesAfterEncounter;
                }

                if (settings.randomEncounterDefaultSafeZoneSize != Defaults.randomEncounterDefaultSafeZoneSize)
                {
                    Game.Instance.BlueprintRoot.RE.DefaultSafeZoneSize = settings.randomEncounterDefaultSafeZoneSize;
                }

                if (settings.randomEncounterEncounterPawnOffset != Defaults.randomEncounterEncounterPawnOffset)
                {
                    Game.Instance.BlueprintRoot.RE.EncounterPawnOffset = settings.randomEncounterEncounterPawnOffset;
                }

                if (settings.randomEncounterEncounterPawnDistanceFromLocation !=
                    Defaults.randomEncounterEncounterPawnDistanceFromLocation)
                {
                    Game.Instance.BlueprintRoot.RE.EncounterPawnDistanceFromLocation =
                        settings.randomEncounterEncounterPawnDistanceFromLocation;
                }

                if (settings.randomEncounterRollMiles != Defaults.randomEncounterRollMiles)
                {
                    Game.Instance.BlueprintRoot.RE.RollMiles = settings.randomEncounterRollMiles;
                }

                if (settings.randomEncounterRandomEncounterAvoidanceFailMargin !=
                    Defaults.randomEncounterRandomEncounterAvoidanceFailMargin)
                {
                    Game.Instance.BlueprintRoot.RE.RandomEncounterAvoidanceFailMargin =
                        settings.randomEncounterRandomEncounterAvoidanceFailMargin;
                }

                if (settings.randomEncounterEncounterMinBonusCr != Defaults.randomEncounterEncounterMinBonusCr)
                {
                    Game.Instance.BlueprintRoot.RE.EncounterMinBonusCR = settings.randomEncounterEncounterMinBonusCr;
                }

                if (settings.randomEncounterEncounterMaxBonusCr != Defaults.randomEncounterEncounterMaxBonusCr)
                {
                    Game.Instance.BlueprintRoot.RE.EncounterMaxBonusCR = settings.randomEncounterEncounterMaxBonusCr;
                }

                if (settings.randomEncounterHardEncounterBonusCr != Defaults.randomEncounterHardEncounterBonusCr)
                {
                    Game.Instance.BlueprintRoot.RE.HardEncounterBonusCR = settings.randomEncounterHardEncounterBonusCr;
                }
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
                    modLogger.Log(e.ToString());
                    modLogger.Log("associatedTask: " + associatedTask.ToString());
                    modLogger.Log("instance.FullName: " + instance.FullName);
                    modLogger.Log("instance.EventBlueprint.AssetGuid: " + instance.EventBlueprint.AssetGuid);
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
            var serializer = new XmlSerializer(typeof(Localisation[]), new XmlRootAttribute() { ElementName = "resources" });
            using (var stream = File.Create(Storage.modEntryPath + Storage.localisationFolder + "\\" + "fallback.xml"))
            {
                serializer.Serialize(stream, MenuText.fallback.Select(kv => new Localisation() { key = kv.Key, value = kv.Value }).ToArray());
            }
        }

        public static void RefreshFeatFavourites()
        {
            Common.SerializeListString(Storage.featFavourites, Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesFeatsFile);

            Storage.featFavouritesGuids = Storage.featFavourites;
            Storage.featFavouritesNames.Clear();
            Storage.featFavouritesDescriptions.Clear();
            for (var i = 0; i < Storage.featFavouritesGuids.Count; i++)
            {
                var stringFeatName = Utilities.GetBlueprintByGuid<BlueprintFeature>(Storage.featFavourites[i]).Name;
                if (stringFeatName != "" && stringFeatName != null)
                {
                    Storage.featFavouritesNames.Add(stringFeatName);
                }
                else
                {
                    var stringFeatObjectNames = Utilities.GetBlueprintByGuid<BlueprintFeature>(Storage.featFavourites[i]).name;
                    Storage.featFavouritesNames.Add(stringFeatObjectNames);
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

            if (string.IsNullOrEmpty(settings.featSearch)) return;
            Storage.currentFeatSearch = settings.featSearch;
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
                if (Storage.featValidObjectNames[i].Contains(settings.featSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByFeatObjectName == Storage.isTrueString)
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
                if (Storage.featValidNames[i].Contains(settings.featSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByFeatName == Storage.isTrueString)
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
                if (Storage.featValidDescriptions[i].Contains(settings.featSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByFeatDescription == Storage.isTrueString)
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
            for (var i = 0; i < Storage.localisationsXmlFiles.Count; i++)
            {
                var locale = "";

                if (File.Exists(Storage.modEntryPath + Storage.localisationFolder + "\\" + Storage.localisationsXmlFiles[i] + ".xml"))
                {
                    try
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(Storage.modEntryPath + Storage.localisationFolder + "\\" + Storage.localisationsXmlFiles[i] + ".xml");
                        locale = (xmlDoc.SelectSingleNode("resources/Localisation/@value").Value);
                    }
                    catch (Exception e)
                    {
                        modLogger.Log(e.ToString());
                    }
                }
                try
                {
                    switch (locale)
                    {
                        case "deDE":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXmlFiles[i], (Texture)Storage.flagsBundle.LoadAsset("deDE")));
                            break;
                        case "enGB":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXmlFiles[i], (Texture)Storage.flagsBundle.LoadAsset("enGB")));
                            break;
                        case "esES":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXmlFiles[i], (Texture)Storage.flagsBundle.LoadAsset("esES")));
                            break;
                        case "frFR":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXmlFiles[i], (Texture)Storage.flagsBundle.LoadAsset("frFR")));
                            break;
                        case "itIT":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXmlFiles[i], (Texture)Storage.flagsBundle.LoadAsset("itIT")));
                            break;
                        case "ruRU":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXmlFiles[i], (Texture)Storage.flagsBundle.LoadAsset("ruRU")));
                            break;
                        case "zhCN":
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXmlFiles[i], (Texture)Storage.flagsBundle.LoadAsset("zhCN")));
                            break;
                        default:
                            Storage.localeGrid.Add(new GUIContent(" " + Storage.localisationsXmlFiles[i], (Texture)Storage.flagsBundle.LoadAsset("empty")));
                            break;
                    }
                }
                catch (Exception e)
                {

                    modLogger.Log(e.ToString());
                }
                if (setSelection && String.Equals(LocalizationManager.CurrentLocale.ToString(), locale, StringComparison.OrdinalIgnoreCase))
                {
                    settings.selectedLocalisation = i;
                }
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
            Directory.CreateDirectory(Storage.modEntryPath + Storage.exportFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.favouritesFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.itemSetsFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.localisationFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.modifiedBlueprintsFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.savesFolder);
            Directory.CreateDirectory(Storage.modEntryPath + Storage.taxCollectorFolder);
            Directory.CreateDirectory(Storage.modEntryPath + SpawnUnits.UnitSetsFolder);
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
            if (Storage.classNames.Any() && Storage.classData.Any()) return;
            Storage.classNames.Clear();
            Storage.classData.Clear();
            var bcc = ResourcesLibrary.GetBlueprints<BlueprintCharacterClass>();
            foreach (var b in bcc)
            {
                if (b.AssetGuid == "42a455d9ec1ad924d889272429eb8391" && !Common.DLCTieflings()) continue;
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

            if (string.IsNullOrEmpty(settings.abilitySearch)) return;
            Storage.currentAbilitySearch = settings.abilitySearch;
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

                if (Storage.abilityValidObjectNames[i].Contains(settings.abilitySearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByAbilityObjectName == Storage.isTrueString)
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
                if (Storage.abilityValidNames[i].Contains(settings.abilitySearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByAbilityName == Storage.isTrueString)
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
                if (Storage.abilityValidDescriptions[i].Contains(settings.abilitySearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (settings.toggleSearchByAbilityDescription == Storage.isTrueString)
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
            settings.Save(modEntry);
        }
    }
}