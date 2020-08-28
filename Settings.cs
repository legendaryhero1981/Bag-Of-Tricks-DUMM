using System.Collections.Generic;

using UnityEngine;

using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    public class Settings : UnityModManager.ModSettings
    {
        public string modVersion = "0.0.0";

        public bool firstLaunch = true;

        public bool showCarryMoreCategory = false;
        public float heavyEncumbranceMultiplier = 1f;
        public string customHeavyEncumbranceMultiplier = "1";
        public float finalCustomHeavyEncumbranceMultiplier = 1f;
        public bool useCustomHeavyEncumbranceMultiplier = false;

        public bool showRestLessCategory = false;
        public float fatigueHoursModifierMultiplier = 1f;
        public string customFatigueHoursModifierMultiplier = "1";
        public float finalCustomFatigueHoursModifierMultiplier = 1f;
        public bool useCustomFatigueHoursModifierMultiplier = false;

        public string toggleNoRationsRequired = Storage.isFalseString;

        public float moneyMultiplier = 1f;
        public string customMoneyMultiplier = "1";
        public float finalCustomMoneyMultiplier = 1f;
        public bool useCustomMoneyMultiplier = false;

        public bool showMoneyCategory = false;
        public string moneyAmount = "1";
        public int finalMoneyAmount = 1;


        public string toggleExperienceMultiplier = Storage.isFalseString;
        public float experienceMultiplier = 1f;
        public string customExperienceMultiplier = "1";
        public float finalCustomExperienceMultiplier = 1f;
        public bool useCustomExperienceMultiplier = false;

        public bool showExperienceCategory = false;
        public string experienceAmount = "1";
        public int finalExperienceAmount = 1;

        public bool showItemsCategory = false;
        public string itemGuid = "";
        public string itemAmount = "1";
        public int finalItemAmount = 1;
        public bool showItemInfo = false;
        public bool addItemIdentified = false;

        public string itemSearch = "";


        public string toggleSearchByItemObjectName = Storage.isTrueString;
        public string toggleSearchByItemName = Storage.isTrueString;
        public string toggleSearchByItemDescription = Storage.isFalseString;
        public string toggleSearchByItemFlavorText = Storage.isFalseString;

        public bool showFilters = false;
        public string filterButtonText = Strings.GetText("misc_Enable");
        public string toggleFilterWeapons = Storage.isTrueString;
        public string toggleFilterShields = Storage.isTrueString;
        public string toggleFilterArmours = Storage.isTrueString;
        public string toggleFilterRings = Storage.isTrueString;
        public string toggleFilterBelts = Storage.isTrueString;
        public string toggleFilterFootwear = Storage.isTrueString;
        public string toggleFilterGloves = Storage.isTrueString;
        public string toggleFilterHeadwear = Storage.isTrueString;
        public string toggleFilterNeckItems = Storage.isTrueString;
        public string toggleFilterShoulderItems = Storage.isTrueString;

        public string toggleFilterWristItems = Storage.isTrueString;

        //public static string filterHandSimple = Storage.hideString;
        public string toggleFilterKeys = Storage.isTrueString;
        public string toggleFilterMiscellaneousItems = Storage.isTrueString;
        public string toggleFilterNotes = Storage.isTrueString;

        public string toggleFilterUsableItems = Storage.isTrueString;
        //public static string filterSimpleItems = Storage.hideString;

        public bool multipleItems = false;

        public bool showItemSets = false;

        public bool showItemFavourites = false;

        public bool showMiscCategory = false;

        public string resultLimit = "100";
        public int finalResultLimit = 100;
        public bool settingKeepGuids = false;
        public bool settingSearchForCsv = true;
        public bool settingSearchForTxt = true;
        public bool settingKeepCategories = false;
        public bool settingKeepSubMenus = false;
        public bool settingShowDebugInfo = false;
        public bool settingCreateBackupBeforeImport = true;

        public string toggleFogOfWarVisuals = Storage.isTrueString;

        public bool showExperimentalCategory = false;
        public string toggleExperimentalIUnderstand = Storage.isFalseString;
        public string flyingHeight = "0.1";
        public float finalflyingHeight = 0.1f;
        public bool flyingHeightUseSlider = false;

        public bool showDiceRollCategory = false;
        public string toggleArcaneSpellFailureRoll = Storage.isFalseString;
        public string toggleArcaneSpellFailureRollOutOfCombatOnly = Storage.isFalseString;
        public string toggleRoll20Initiative = Storage.isFalseString;
        public int roll20InitiativeIndex = 0;
        public string togglePartyAlwaysRoll20 = Storage.isFalseString;
        public KeyCode togglePartyAlwaysRoll20Key = KeyCode.Insert;
        public string toggleEnablePartyAlwaysRoll20Hotkey = Storage.isFalseString;
        public string toggleEnemiesAlwaysRoll1 = Storage.isFalseString;
        public string toggleEveryoneExceptPlayerFactionRolls1 = Storage.isFalseString;

        public int takeXIndex = 0;
        public float takeXCustom = 1f;
        public string toggleTakeXSkillsOnly = Storage.isFalseString;

        public bool showSpellCategory = false;
        public string toggleUnlimitedCasting = Storage.isFalseString;
        public float spellsPerDayMultiplier = 1f;
        public string customSpellsPerDayMultiplier = "1";
        public float finalCustomspellsPerDayMultiplier = 1f;
        public bool useCustomspellsPerDayMultiplier = false;

        public string currentSceneString = Strings.GetText("misc_Display");

        public bool showKingdomCategory = false;
        public string buildPointAmount = "1";
        public int finalBuildPointAmount = 1;
        public string kingdomStatsAmount = "1";
        public int kingdomFinalStatsAmount = 1;
        public float kingdomBuildingTimeModifier = 0f;


        public string gameConstsAmount = "1";
        public int finalGameConstsAmount = 1;
        public int gameConstsMinWeaponRange = -1;
        public int gameConstsMinUnitSpeedMps = -1;
        public int gameConstsStealthDcIncrement = -1;

        public bool showBuffsCategory = false;
        public string showAllBuffs = Strings.GetText("misc_Display");
        public bool showBuffFavourites = false;
        public string buffSearch = "";
        public string toggleSearchByBuffObjectName = Storage.isTrueString;
        public string toggleSearchByBuffName = Storage.isTrueString;
        public string toggleSearchByBuffDescription = Storage.isFalseString;
        public string toggleSearchByBuffComponents = Storage.isFalseString;
        public List<string> buffFavourites = new List<string>();
        public string toggleBuffsShowEnemies = Storage.isFalseString;

        public bool showPartyStatisticsCategory = false;
        public string partyStatsAmount = "10";
        public int partyFinalStatsAmount = 10;
        public float partyStatMultiplier = 1f;

        public bool showEnemyStatisticsCategory = false;
        public string enemyStatsAmount = "10";
        public int enemyFinalStatsAmount = 10;
        public float enemyStatMultiplier = 1f;

        public int featMultiplier = 1;
        public string featMultiplierString = "1";
        public int selectedKingdomAlignment = 9;
        public string selectedKingdomAlignmentString = "9";
        public string selectedKingdomAlignmentTranslated = "none";
        public string toggleFfAoe = Storage.isFalseString;
        public string toggleFfAny = Storage.isFalseString;
        public string toggleMetamagic = Storage.isFalseString;
        public string toggleMaterialComponent = Storage.isFalseString;
        public string toggleSpontaneousCopyScrolls = Storage.isFalseString;
        public float companionCostMultiplier = 1;
        public string companionCostMultiplierString = "1";
        public string toggleNoInactiveCamp = Storage.isFalseString;
        public string toggleInfiniteAbilities = Storage.isFalseString;
        public string toggleInstantEvent = Storage.isFalseString;
        public string travelSpeedMultiplierString = "1.0";
        public string characterCreationAbilityPointsMaxString = "18";
        public string characterCreationAbilityPointsPlayerString = "25";
        public string characterCreationAbilityPointsMercString = "20";
        public string toggleInstantCooldown = Storage.isFalseString;
        public string toggleUndetectableStealth = Storage.isFalseString;
        public string toggleInfiniteSkillpoints = Storage.isFalseString;
        public float travelSpeedMultiplier = 1.0f;
        public int characterCreationAbilityPointsMax = 18;
        public string toggleKingdomEventResultSuccess = Storage.isFalseString;
        public string toggleEquipmentRestrictions = Storage.isFalseString;
        public string toggleDialogRestrictions = Storage.isFalseString;
        public string toggleSettlementRestrictions = Storage.isFalseString;
        public string toggleInfiniteItems = Storage.isFalseString;
        public string toggleMoveSpeedAsOne = Storage.isFalseString;
        public int characterCreationAbilityPointsPlayer = 25;
        public int characterCreationAbilityPointsMerc = 20;
        public string toggleIgnoreClassAlignment = Storage.isFalseString;
        public int addCasterLevel = 1;
        public string addCasterLevelString = "1";

        public string toggleSkipSpellSelection = Storage.isFalseString;

        public bool showTravelAndMapCategory = false;
        public string toggleEnableAvoidanceSuccess = Storage.isFalseString;
        public string toggleEnableTeleport = Storage.isFalseString;

        public string forcedEncounterGuid = "";
        public string forcedEncounterCr = "1";
        public int forcedEncounterFinalCr = 1;
        public string toggleForcedEncounterIsHard = Storage.isFalseString;
        public int forcedEncounterSelectedAvoidance = 0;
        public int forcedEncounterSelectedBlueprintMode = 0;

        public string toggleRandomEncountersEnabled = Storage.isTrueString;
        public bool randomEncounterSettingsShow = false;

        public float randomEncounterSettingFloatAmountLimited = 0.1f;
        public string randomEncounterSettingFloatAmount = "1";
        public float randomEncounterSettingFloatAmountFinal = 1f;
        public string randomEncounterSettingIntAmount = "1";
        public int randomEncounterSettingIntAmountFinal = 1;

        public float randomEncounterChanceOnGlobalMap = Defaults.randomEncounterChanceOnGlobalMap;
        public float randomEncounterChanceOnCamp = Defaults.randomEncounterChanceOnCamp;
        public float randomEncounterChanceOnCampSecondTime = Defaults.randomEncounterChanceOnCampSecondTime;
        public float randomEncounterHardEncounterChance = Defaults.randomEncounterHardEncounterChance;
        public float randomEncounterHardEncounterMaxChance = Defaults.randomEncounterHardEncounterMaxChance;
        public float randomEncounterHardEncounterChanceIncrease = Defaults.randomEncounterHardEncounterChanceIncrease;
        public float randomEncounterRollMiles = Defaults.randomEncounterRollMiles;
        public float randomEncounterSafeMilesAfterEncounter = Defaults.randomEncounterSafeMilesAfterEncounter;
        public int randomEncounterEncounterMinBonusCr = Defaults.randomEncounterEncounterMinBonusCr;
        public int randomEncounterEncounterMaxBonusCr = Defaults.randomEncounterEncounterMaxBonusCr;
        public int randomEncounterHardEncounterBonusCr = Defaults.randomEncounterHardEncounterBonusCr;
        public float randomEncounterDefaultSafeZoneSize = Defaults.randomEncounterDefaultSafeZoneSize;

        public int randomEncounterRandomEncounterAvoidanceFailMargin =
            Defaults.randomEncounterRandomEncounterAvoidanceFailMargin;

        public float randomEncounterEncounterPawnOffset = Defaults.randomEncounterEncounterPawnOffset;

        public float randomEncounterEncounterPawnDistanceFromLocation =
            Defaults.randomEncounterEncounterPawnDistanceFromLocation;

        public float randomEncounterStalkerAmbushChance = Defaults.randomEncounterStalkerAmbushChance;


        public Vector3? forcedEncounterPostion = new Vector3(1, 1, 1);

        public bool showKeyBindingsCategory = false;
        public KeyCode teleportKey = KeyCode.Plus;

        public float defaultRotation = -400f;
        public float cameraRotation = 0f;
        public float cameraTurnRate = 1f;
        public KeyCode cameraTurnLeft = KeyCode.Comma;
        public KeyCode cameraTurnRight = KeyCode.Period;
        public KeyCode cameraReset = KeyCode.Slash;
        public string toggleEnableCameraRotation = Storage.isFalseString;
        public string toggleEnableCameraScrollSpeed = Storage.isFalseString;
        public string toggleEnableCameraZoom = Storage.isFalseString;
        public float finalFovValue = 1f;
        public string savedFovMin = Defaults.fovMin;
        public string savedFovMax = Defaults.fovMax;
        public string savedFovGlobalMap = Defaults.fovGlobalMap;

        public KeyCode focusCameraKey = KeyCode.Delete;
        public string toggleEnableFocusCamera = Storage.isFalseString;
        public string toggleEnableFocusCameraSelectedUnit = Storage.isFalseString;
        public bool focusCameraToggle = false;
        public KeyCode focusCylceKey = KeyCode.End;
        public int partyMembersFocusPositionCounter = 0;
        public string toggleCameraBounds = Storage.isFalseString;
        public string toggleCameraAllowScroll = Storage.isFalseString;

        public string toggleEnableTaxCollector = Storage.isTrueString;
        public bool showTaxCollectorCategory = false;

        public string toggleAddToLog = Storage.isTrueString;

        public bool showSoundsCategory = false;

        public bool showFogOfWarCategory = false;
        public string toggleOverwriteFogOfWar = Storage.isFalseString;
        public string toggleFogOfWarFull = Storage.isTrueString;
        public bool toggleFogOfWarBool = true;

        public int selectedLocalisation = 0;
        public string selectedLocalisationName = "English";

        public bool showLanguageMenu = false;

        public KeyCode resetCutsceneLockKey = KeyCode.Home;

        public string toggleEnableResetCutsceneLockHotkey = Storage.isFalseString;

        public string vendorSellPriceMultiplier = "0.25";
        public float finalVendorSellPriceMultiplier = 0.25f;
        public string toggleVendorSellPriceMultiplier = Storage.isFalseString;
        public string toggleVendorsSellFor0 = Storage.isFalseString;
        public string toggleVendorsBuyFor0 = Storage.isFalseString;

        public float sillyBloodChance = Defaults.sillyBloddChance;
        public float sillyBloodChanceSaved = Defaults.sillyBloddChance;

        public string toggleIgnoreForbiddenFeatures = Storage.isFalseString;

        public string toggleSearchOnlyCraftMagicItems = Storage.isFalseString;

        public string toggleShowOnlyClassSkills = Storage.isFalseString;

        public KeyCode actionKey = KeyCode.Pause;
        public string toggleEnableActionKey = Storage.isFalseString;
        public string toggleActionKeyExperimental = Storage.isFalseString;
        public int actionKeyIndex = 0;
        public int actionKeyKillIndex = 0;
        public int actionKeyBuffIndex = 0;
        public string toggleActionKeyLogInfo = Storage.isFalseString;
        public string toggleActionKeyShowUnitInfoBox = Storage.isFalseString;
        public bool actionKeySpawnRandomEnemy = false;

        public bool showFeatsCategory = false;
        public string toggleFeatsShowEnemies = Storage.isFalseString;
        public bool showFeatsFavourites = false;

        public string featSearch = "";
        public string toggleSearchByFeatObjectName = Storage.isTrueString;
        public string toggleSearchByFeatName = Storage.isTrueString;
        public string toggleSearchByFeatDescription = Storage.isFalseString;
        public string showAllFeats = Strings.GetText("misc_Display");

        public bool featsParamShowAllWeapons = false;

        public string toggleShowTooltips = Storage.isTrueString;

        public bool showRomanceCounters = false;
        public bool showRomanceCountersSpoilers = false;

        public string toggleNoLevelUpRestirctions = Storage.isFalseString;

        public string toggleRestoreSpellsAbilitiesAfterCombat = Storage.isFalseString;

        public string toggleNeverRoll1 = Storage.isFalseString;
        public int neverRoll1Index = 0;
        public string toggleNeverRoll20 = Storage.isFalseString;
        public int neverRoll20Index = 0;

        public string toggleInstantRestAfterCombat = Storage.isFalseString;
        public string toggleFullHitdiceEachLevel = Storage.isFalseString;
        public string toggleRollHitDiceEachLevel = Storage.isFalseString;

        public string toggleRollWithAdvantage = Storage.isFalseString;
        public int rollWithAdvantageIndex = 0;

        public string toggleRollWithDisadvantage = Storage.isFalseString;
        public int rollWithDisadvantageeIndex = 0;

        public string toggleIgnoreFeaturePrerequisites = Storage.isFalseString;
        public string toggleIgnoreFeatureListPrerequisites = Storage.isFalseString;
        public string toggleFeaturesIgnorePrerequisites = Storage.isFalseString;

        public float debugTimeMultiplier = Defaults.debugTimeScale;
        public string customDebugTimeMultiplier = "1";
        public float finalCustomDebugTimeMultiplier = 1f;
        public bool useCustomDebugTimeMultiplier = false;

        public bool showUpgradSettlementsAndBuildings = false;

        public string toggleStopGameTime = Storage.isFalseString;

        public string toggleNoDamageFromEnemies = Storage.isFalseString;
        public string togglePartyOneHitKills = Storage.isFalseString;
        public string toggleDamageDealtMultipliers = Storage.isFalseString;
        public string togglePartyDamageDealtMultiplier = Storage.isFalseString;
        public float partyDamageDealtMultiplier = 1f;
        public string toggleEnemiesDamageDealtMultiplier = Storage.isFalseString;
        public float enemiesDamageDealtMultiplier = 1f;

        public string damageDealtMultipliersValue = "1";
        public float finalDamageDealtMultipliersValue = 1;


        public bool showRomanceCountersExperimental = false;
        public string romanceCounterSetValue = "1";
        public float finalRomanceCounterSetValue = 1f;

        public string toggleAllDoorContainersUnlocked = Storage.isFalseString;
        public string toggleRepeatableLockPicking = Storage.isFalseString;
        public string toggleRepeatableLockPickingWeariness = Storage.isFalseString;

        public string repeatableLockPickingWeariness = "1";
        public float finalRepeatableLockPickingWeariness = 1f;
        public string toggleRepeatableLockPickingLockPicks = Storage.isFalseString;
        public string toggleRepeatableLockPickingLockPicksInventoryText = Storage.isTrueString;

        public float artisanMasterpieceChance = Defaults.artisanMasterpieceChance;

        public string toggleNoIngredientsRequired = Storage.isFalseString;

        public string toggleUnityModManagerButton = Storage.isTrueString;
        public float unityModManagerButtonIndex = 1f;

        public string toggleScaleInventoryMoney = Storage.isFalseString;

        public string toggleShowClassDataOptions = Storage.isFalseString;

        public string toggleNoBurnKineticist = Storage.isFalseString;
        public string toggleMaximiseAcceptedBurn = Storage.isFalseString;

        public bool showAddAbilitiesCategory = false;
        public string showAllAbilities = Strings.GetText("misc_Display");
        public bool showAbilitiesFavourites = false;
        public string abilitySearch = "";
        public string toggleSearchByAbilityObjectName = Storage.isTrueString;
        public string toggleSearchByAbilityName = Storage.isTrueString;
        public string toggleSearchByAbilityDescription = Storage.isFalseString;

        public string toggleMainCharacterRoll20 = Storage.isFalseString;

        public string toggleShowPetInventory = Storage.isFalseString;

        public bool showAnimationsCategory = false;

        public string toggleAnimationCloseUmm = Storage.isFalseString;

        public bool showSpellSpellbooksCategory = false;

        public string toggleInstantCooldownMainChar = Storage.isFalseString;

        public string toggleDexBonusLimit99 = Storage.isFalseString;

        public string togglePassSkillChecksIndividual = Storage.isFalseString;
        public string togglePassSkillChecksIndividualDc99 = Storage.isFalseString;
        public int indexPassSkillChecksIndividual = 0;

        public string[] togglePassSkillChecksIndividualArray =
        {
            Storage.isTrueString, Storage.isTrueString, Storage.isTrueString, Storage.isTrueString,
            Storage.isTrueString, Storage.isTrueString, Storage.isTrueString, Storage.isTrueString,
            Storage.isTrueString, Storage.isTrueString, Storage.isTrueString, Storage.isTrueString,
            Storage.isTrueString, Storage.isTrueString
        };
        /*
        00 = StatType.SkillAthletics
        01 = StatType.SkillKnowledgeArcana
        02 = StatType.SkillKnowledgeWorld
        03 = StatType.SkillLoreNature
        04 = StatType.SkillLoreReligion
        05 = StatType.SkillMobility
        06 = StatType.SkillPerception
        07 = StatType.SkillPersuasion
        08 = StatType.SkillStealth
        09 = StatType.SkillThievery
        10 = StatType.SkillUseMagicDevice
        11 = StatType.CheckBluff
        12 = StatType.CheckDiplomacy
        13 = StatType.CheckIntimidate
        */

        public string toggleItemsWeighZero = Storage.isFalseString;

        public string toggleExtraAttacksParty = Storage.isFalseString;
        public int extraAttacksPartyPrimaryHand = 0;
        public int extraAttacksPartySecondaryHand = 0;

        public string toggleAlignmentFix = Storage.isFalseString;
        public string toggleSpellbookAbilityAlignmentChecks = Storage.isFalseString;
        public string toggleSetSpeedOnSummon = Storage.isFalseString;
        public int setSpeedOnSummonValue = 30;

        public string toggleNoResourcesClaimCost = Storage.isFalseString;
        public string toggleFreezeTimedQuestAt90Days = Storage.isFalseString;
        public string togglePreventQuestFailure = Storage.isFalseString;
        public string toggleAlwaysSucceedCastingDefensively = Storage.isFalseString;
        public string toggleAlwaysSucceedConcentration = Storage.isFalseString;
        public string toggleSortSpellbooksAlphabetically = Storage.isFalseString;
        public string toggleSortSpellsAlphabetically = Storage.isFalseString;
        public string toggleAllHitsAreCritical = Storage.isFalseString;

        public string togglePassSavingThrowIndividual = Storage.isFalseString;
        public int indexPassSavingThrowIndividuall = 0;

        public string[] togglePassSavingThrowIndividualArray =
            {Storage.isTrueString, Storage.isTrueString, Storage.isTrueString};
        /*
        00 = StatType.SaveFortitude
        01 = StatType.SaveReflex
        02 = StatType.SaveWill
        */

        public string toggleNoAttacksOfOpportunity = Storage.isFalseString;
        public int indexNoAttacksOfOpportunity = 0;

        public string toggleCookingAndHuntingInDungeons = Storage.isFalseString;
        public string toggleArmourChecksPenalty0 = Storage.isFalseString;
        public string toggleArmourChecksPenalty0OutOfCombatOnly = Storage.isFalseString;


        public int mainToolbarIndex = 1;

        public string characterCreationAbilityPointsMinString = "7";
        public int characterCreationAbilityPointsMin = 7;

        public string[] cheatsCategories =
        {
            "CharacterCreationProgressionExperience",
            "Encumbrance",
            "RestRations",
            "Money",
            "PartyOptions",
            "EnemyStats",
            "ItemsEquipment",
            "Buffs",
            "Feats",
            "Abilities",
            "SpellsAndSpellbooks",
            "SpellAbilityOptions",
            "MapTravel",
            "Kingdom",
            "DiceRolls",
            "SpawnUnitsMenu",
            "BeneathTheStolenLands",
            "Misc"
        };

        public bool editFavouriteFunctionsPosition = false;

        public string toggleSetEncumbrance = Storage.isFalseString;
        public int setEncumbrancIndex = 0;
        public string toggleInstantPartyChange = Storage.isFalseString;
        public string toggleDevTools = Storage.isFalseString;
        public string toggleDevToolsLogToUmm = Storage.isFalseString;

        public string togglePartyMovementSpeedMultiplier = Storage.isFalseString;
        public float partyMovementSpeedMultiplierValue = 1;

        public string hiddenGameVersion = "";
        public string toggleBoTScrollBar = Storage.isFalseString;
        public string toggleHudToggle = Storage.isFalseString;
        public KeyCode hudToggleKey = KeyCode.Semicolon;

        public string toggleItemModding = Storage.isFalseString;

        public string toggleNoNegativeLevels = Storage.isFalseString;

        public string togglAutoEquipConsumables = Storage.isFalseString; // rename to toggleAutoEquipConsumables

        public string toggleIgnorePrerequisites = Storage.isFalseString;
        public string toggleIgnoreCasterTypeSpellLevel = Storage.isFalseString;
        public string toggleIgnoreForbiddenArchetype = Storage.isFalseString;
        public string toggleIgnorePrerequisiteStatValue = Storage.isFalseString;

        public string toggleSpiderBegone = Storage.isFalseString;

        public string toggleNoTempHpKineticist = Storage.isFalseString;

        public string toggleShowAreaName = Storage.isFalseString;

        public string toggleRestoreItemChargesAfterCombat = Storage.isFalseString;
        public string toggleReverseCasterAlignmentChecks = Storage.isFalseString;
        public string togglePreventAlignmentChanges = Storage.isFalseString;
        public string toggleDisplayObjectInfo = Storage.isFalseString;

        public string toggleSummonDurationMultiplier = Storage.isFalseString;
        public int indexSummonDurationMultiplier = 0;
        public float finalSummonDurationMultiplierValue = 1f;

        public string toggleSetSummonLevelTo20 = Storage.isFalseString;
        public int indexSetSummonLevelTo20 = 0;

        public string toggleBuffDurationMultiplier = Storage.isFalseString;
        public int indexBuffDurationMultiplier = 0;
        public float finalBuffDurationMultiplierValue = 1f;

        public bool showBeneathTheStolenLandsCategory = false;

        public string toggleUberLogger = Storage.isFalseString;
        public string toggleUberLoggerForward = Storage.isFalseString;
        public string toggleUberLoggerForwardPrefix = Storage.isFalseString;

        public string toggleExportToModFolder = Storage.isFalseString;
        public string toggleCombatDifficultyMessage = Storage.isFalseString;

        public string toggleMakeSummmonsControllable = Storage.isFalseString;
        public string toggleDisableWarpaintedSkullAbilityForSummonedBarbarians = Storage.isFalseString;

        public string toggleRemoveSummonsGlow = Storage.isFalseString;

        public string toggleSpellAbilityRangeMultiplier = Storage.isFalseString;
        public float spellAbilityRangeMultiplier = 1f;
        public float customSpellAbilityRangeMultiplier = 1f;
        public bool useCustomSpellAbilityRangeMultiplier = false;

        public string toggleTabletopSpellAbilityRange = Storage.isFalseString;

        public bool showSpawnUnitsCategory = false;
        public string unitSearch = "";
        public string toggleSearchByUnitObjectName = Storage.isTrueString;
        public string toggleSearchByUnitCharacterName = Storage.isTrueString;
        public string toggleSearchByUnitType = Storage.isFalseString;
        public string unitSearchFilters = "";
        public bool showUnitsFavourites = false;
        public string toggleSpawnEnemiesFromUnitFavourites = Storage.isFalseString;

        public string toggleAutomaticallyLoadLastSave = Storage.isFalseString;
        public bool showUnitSets = false;

        public string toggleCustomSpellAbilityRange = Storage.isFalseString;

        public int customSpellAbilityRangeClose = 30;
        public int customSpellAbilityRangeMedium = 40;
        public int customSpellAbilityRangeLong = 50;


        public string toggleEnemyBaseHitPointsMultiplier = Storage.isFalseString;
        public float enemyBaseHitPointsMultiplier = 1f;
        public float customEnemyBaseHitPointsMultiplier = 1f;
        public bool useCustomEnemyBaseHitPointsMultiplier = false;
        public string toggleEnemyBaseHitPointsMultiplierCampEncounter = Storage.isTrueString;
        public string toggleEnemyBaseHitPointsMultiplierUnitSpawner = Storage.isTrueString;

        public string toggleSetTargetFrameRate = Storage.isFalseString;
        public int targetFrameRate = 0;

        public string toggleCreateBattleLogFile = Storage.isFalseString;
        public string toggleCreateBattleLogFileLog = Storage.isTrueString;
        public string toggleCreateBattleLogFileHtml = Storage.isFalseString;
        public string toggleCreateBattleLogFileBotLog = Storage.isFalseString;

        public string toggleRespecCostMultiplier = Storage.isFalseString;
        public string repecCostMultiplierString = "1";
        public float repecCostMultiplier = 1f;

        public string toggleAllowCampingEverywhere = Storage.isFalseString;
        public string toggleCustomTakeXAsMin = Storage.isFalseString;

        public string toggleEnableRandomEncounterSettings = Storage.isFalseString;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}