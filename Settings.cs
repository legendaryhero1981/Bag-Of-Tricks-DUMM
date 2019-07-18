using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;

namespace BagOfTricks
{
    public class Settings : UnityModManager.ModSettings
    {
        public string abilitySearch = "";

        public KeyCode actionKey = KeyCode.Pause;
        public int actionKeyBuffIndex = 0;
        public int actionKeyIndex = 0;
        public int actionKeyKillIndex = 0;
        public bool actionKeySpawnRandomEnemy = false;
        public int addCasterLevel = 1;
        public string addCasterLevelString = "1";
        public bool addItemIdentified = false;

        public float artisanMasterpieceChance = Defaults.artisanMasterpieceChance;
        public List<string> buffFavourites = new List<string>();
        public string buffSearch = "";
        public string buildPointAmount = "1";
        public KeyCode cameraReset = KeyCode.Slash;
        public float cameraRotation = 0f;
        public KeyCode cameraTurnLeft = KeyCode.Comma;
        public float cameraTurnRate = 1f;
        public KeyCode cameraTurnRight = KeyCode.Period;
        public int characterCreationAbilityPointsMax = 18;
        public string characterCreationAbilityPointsMaxString = "18";
        public int characterCreationAbilityPointsMerc = 20;
        public string characterCreationAbilityPointsMercString = "20";
        public int characterCreationAbilityPointsMin = 7;

        public string characterCreationAbilityPointsMinString = "7";
        public int characterCreationAbilityPointsPlayer = 25;
        public string characterCreationAbilityPointsPlayerString = "25";

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
            "BeneathTheStolenLands",
            "Misc"
        };

        public int companionCostMultiplier = 1;
        public string companionCostMultiplierString = "1";

        public string currentSceneString = Strings.GetText("misc_Display");
        public string customDebugTimeMultiplier = "1";
        public string customExperienceMultiplier = "1";
        public string customFatigueHoursModifierMultiplier = "1";
        public string customHeavyEncumbranceMultiplier = "1";
        public string customMoneyMultiplier = "1";
        public string customSpellsPerDayMultiplier = "1";

        public string damageDealtMultipliersValue = "1";

        public float debugTimeMultiplier = Defaults.debugTimeScale;

        public float defaultRotation = -400f;

        public bool editFavouriteFunctionsPosition = false;
        public float enemiesDamageDealtMultiplier = 1f;
        public int enemyFinalStatsAmount = 10;
        public float enemyStatMultiplier = 1f;
        public string enemyStatsAmount = "10";
        public string experienceAmount = "1";
        public float experienceMultiplier = 1f;
        public int extraAttacksPartyPrimaryHand = 0;
        public int extraAttacksPartySecondaryHand = 0;
        public float fatigueHoursModifierMultiplier = 1f;

        public int featMultiplier = 1;
        public string featMultiplierString = "1";

        public string featSearch = "";

        public bool featsParamShowAllWeapons = false;
        public string filterButtonText = Strings.GetText("misc_Enable");
        public float finalBuffDurationMultiplierValue = 1f;
        public int finalBuildPointAmount = 1;
        public float finalCustomDebugTimeMultiplier = 1f;
        public float finalCustomExperienceMultiplier = 1f;
        public float finalCustomFatigueHoursModifierMultiplier = 1f;
        public float finalCustomHeavyEncumbranceMultiplier = 1f;
        public float finalCustomMoneyMultiplier = 1f;
        public float finalCustomspellsPerDayMultiplier = 1f;
        public float finalDamageDealtMultipliersValue = 1;
        public int finalExperienceAmount = 1;
        public float finalflyingHeight = 0.1f;
        public float finalFovValue = 1f;
        public int finalGameConstsAmount = 1;
        public int finalItemAmount = 1;
        public int finalMoneyAmount = 1;
        public float finalRepeatableLockPickingWeariness = 1f;
        public int finalResultLimit = 100;
        public float finalRomanceCounterSetValue = 1f;
        public float finalSummonDurationMultiplierValue = 1f;
        public float finalVendorSellPriceMultiplier = 0.25f;

        public bool firstLaunch = true;
        public string flyingHeight = "0.1";
        public bool flyingHeightUseSlider = false;

        public KeyCode focusCameraKey = KeyCode.Delete;
        public bool focusCameraToggle = false;
        public KeyCode focusCylceKey = KeyCode.End;
        public string forcedEncounterCR = "1";
        public int forcedEncounterFinalCR = 1;

        public string forcedEncounterGuid = "";


        public Vector3? forcedEncounterPostion = new Vector3(1, 1, 1);
        public int forcedEncounterSelectedAvoidance = 0;
        public int forcedEncounterSelectedBlueprintMode = 0;


        public string gameConstsAmount = "1";
        public int gameConstsMinUnitSpeedMps = -1;
        public int gameConstsMinWeaponRange = -1;
        public int gameConstsStealthDCIncrement = -1;
        public float heavyEncumbranceMultiplier = 1f;

        public string hiddenGameVersion = "";
        public KeyCode hudToggleKey = KeyCode.Semicolon;
        public int indexBuffDurationMultiplier = 0;
        public int indexNoAttacksOfOpportunity = 0;
        public int indexPassSavingThrowIndividuall = 0;
        public int indexPassSkillChecksIndividual = 0;
        public int indexSetSummonLevelTo20 = 0;
        public int indexSummonDurationMultiplier = 0;
        public string itemAmount = "1";
        public string itemGuid = "";

        public string itemSearch = "";
        public float kingdomBuildingTimeModifier = 0f;
        public int kingdomFinalStatsAmount = 1;
        public string kingdomStatsAmount = "1";

        public int mainToolbarIndex = 1;
        public string modVersion = "0.0.0";
        public string moneyAmount = "1";

        public float moneyMultiplier = 1f;
        //public static string filterSimpleItems = Storage.hideString;

        public bool multipleItems = false;
        public int neverRoll1Index = 0;
        public int neverRoll20Index = 0;
        public float partyDamageDealtMultiplier = 1f;
        public int partyFinalStatsAmount = 10;
        public int partyMembersFocusPositionCounter = 0;
        public float partyMovementSpeedMultiplierValue = 1;
        public float partyStatMultiplier = 1f;
        public string partyStatsAmount = "10";
        public float randomEncounterChanceOnCamp = Defaults.randomEncounterChanceOnCamp;
        public float randomEncounterChanceOnCampSecondTime = Defaults.randomEncounterChanceOnCampSecondTime;

        public float randomEncounterChanceOnGlobalMap = Defaults.randomEncounterChanceOnGlobalMap;
        public float randomEncounterDefaultSafeZoneSize = Defaults.randomEncounterDefaultSafeZoneSize;
        public int randomEncounterEncounterMaxBonusCR = Defaults.randomEncounterEncounterMaxBonusCR;
        public int randomEncounterEncounterMinBonusCR = Defaults.randomEncounterEncounterMinBonusCR;

        public float randomEncounterEncounterPawnDistanceFromLocation =
            Defaults.randomEncounterEncounterPawnDistanceFromLocation;

        public float randomEncounterEncounterPawnOffset = Defaults.randomEncounterEncounterPawnOffset;
        public int randomEncounterHardEncounterBonusCR = Defaults.randomEncounterHardEncounterBonusCR;
        public float randomEncounterHardEncounterChance = Defaults.randomEncounterHardEncounterChance;
        public float randomEncounterHardEncounterChanceIncrease = Defaults.randomEncounterHardEncounterChanceIncrease;
        public float randomEncounterHardEncounterMaxChance = Defaults.randomEncounterHardEncounterMaxChance;

        public int randomEncounterRandomEncounterAvoidanceFailMargin =
            Defaults.randomEncounterRandomEncounterAvoidanceFailMargin;

        public float randomEncounterRollMiles = Defaults.randomEncounterRollMiles;
        public float randomEncounterSafeMilesAfterEncounter = Defaults.randomEncounterSafeMilesAfterEncounter;
        public string randomEncounterSettingFloatAmount = "1";
        public float randomEncounterSettingFloatAmountFinal = 1f;

        public float randomEncounterSettingFloatAmountLimited = 0.1f;
        public string randomEncounterSettingIntAmount = "1";
        public int randomEncounterSettingIntAmountFinal = 1;
        public bool randomEncounterSettingsShow = false;
        public float randomEncounterStalkerAmbushChance = Defaults.randomEncounterStalkerAmbushChance;

        public string repeatableLockPickingWeariness = "1";

        public KeyCode resetCutsceneLockKey = KeyCode.Home;

        public string resultLimit = "100";
        public int rollWithAdvantageIndex = 0;
        public int rollWithDisadvantageeIndex = 0;
        public string romanceCounterSetValue = "1";
        public string savedFovGlobalMap = Defaults.fovGlobalMap;
        public string savedFovMax = Defaults.fovMax;
        public string savedFovMin = Defaults.fovMin;
        public int selectedKingdomAlignment = 9;
        public string selectedKingdomAlignmentString = "9";
        public string selectedKingdomAlignmentTranslated = "none";

        public int selectedLocalisation = 0;
        public string selectedLocalisationName = "English";
        public int setEncumbrancIndex = 0;
        public int setSpeedOnSummonValue = 30;
        public bool settingCreateBackupBeforeImport = true;
        public bool settingKeepCategories = false;
        public bool settingKeepGuids = false;
        public bool settingKeepSubMenus = false;
        public bool settingSearchForCsv = true;
        public bool settingSearchForTxt = true;
        public bool settingShowDebugInfo = false;
        public bool showAbilitiesFavourites = false;

        public bool showAddAbilitiesCategory = false;
        public string showAllAbilities = Strings.GetText("misc_Display");
        public string showAllBuffs = Strings.GetText("misc_Display");
        public string showAllFeats = Strings.GetText("misc_Display");

        public bool showAnimationsCategory = false;

        public bool showBeneathTheStolenLandsCategory = false;
        public bool showBuffFavourites = false;

        public bool showBuffsCategory = false;

        public bool showCarryMoreCategory = false;

        public bool showDiceRollCategory = false;

        public bool showEnemyStatisticsCategory = false;

        public bool showExperienceCategory = false;

        public bool showExperimentalCategory = false;

        public bool showFeatsCategory = false;
        public bool showFeatsFavourites = false;

        public bool showFilters = false;

        public bool showFogOfWarCategory = false;

        public bool showItemFavourites = false;
        public bool showItemInfo = false;

        public bool showItemsCategory = false;

        public bool showItemSets = false;

        public bool showKeyBindingsCategory = false;

        public bool showKingdomCategory = false;

        public bool showLanguageMenu = false;

        public bool showMiscCategory = false;

        public bool showMoneyCategory = false;

        public bool showPartyStatisticsCategory = false;

        public bool showRestLessCategory = false;

        public bool showRomanceCounters = false;


        public bool showRomanceCountersExperimental = false;
        public bool showRomanceCountersSpoilers = false;

        public bool showSoundsCategory = false;

        public bool showSpellCategory = false;

        public bool showSpellSpellbooksCategory = false;
        public bool showTaxCollectorCategory = false;

        public bool showTravelAndMapCategory = false;

        public bool showUpgradSettlementsAndBuildings = false;

        public float sillyBloodChance = Defaults.sillyBloddChance;
        public float sillyBloodChanceSaved = Defaults.sillyBloddChance;
        public float spellsPerDayMultiplier = 1f;
        public float takeXCustom = 1f;

        public int takeXIndex = 0;
        public KeyCode teleportKey = KeyCode.Plus;

        public string togglAutoEquipConsumables = Storage.isFalseString; // rename to toggleAutoEquipConsumables
        public string toggleActionKeyExperimental = Storage.isFalseString;
        public string toggleActionKeyLogInfo = Storage.isFalseString;
        public string toggleActionKeyShowUnitInfoBox = Storage.isFalseString;

        public string toggleAddToLog = Storage.isTrueString;

        public string toggleAlignmentFix = Storage.isFalseString;

        public string toggleAllDoorContainersUnlocked = Storage.isFalseString;
        public string toggleAllHitsAreCritical = Storage.isFalseString;
        public string toggleAlwaysSucceedCastingDefensively = Storage.isFalseString;
        public string toggleAlwaysSucceedConcentration = Storage.isFalseString;

        public string toggleAnimationCloseUMM = Storage.isFalseString;
        public string toggleArcaneSpellFailureRoll = Storage.isFalseString;
        public string toggleArmourChecksPenalty0 = Storage.isFalseString;
        public string toggleBoTScrollBar = Storage.isFalseString;

        public string toggleBuffDurationMultiplier = Storage.isFalseString;
        public string toggleBuffsShowEnemies = Storage.isFalseString;
        public string toggleCameraAllowScroll = Storage.isFalseString;
        public string toggleCameraBounds = Storage.isFalseString;
        public string toggleCombatDifficultyMessage = Storage.isFalseString;

        public string toggleCookingAndHuntingInDungeons = Storage.isFalseString;
        public string toggleDamageDealtMultipliers = Storage.isFalseString;
        public string toggleDevTools = Storage.isFalseString;
        public string toggleDevToolsLogToUMM = Storage.isFalseString;

        public string toggleDexBonusLimit99 = Storage.isFalseString;
        public string toggleDialogRestrictions = Storage.isFalseString;
        public string toggleDisplayObjectInfo = Storage.isFalseString;
        public string toggleEnableActionKey = Storage.isFalseString;
        public string toggleEnableAvoidanceSuccess = Storage.isFalseString;
        public string toggleEnableCameraRotation = Storage.isFalseString;
        public string toggleEnableCameraScrollSpeed = Storage.isFalseString;
        public string toggleEnableCameraZoom = Storage.isFalseString;
        public string toggleEnableFocusCamera = Storage.isFalseString;
        public string toggleEnableFocusCameraSelectedUnit = Storage.isFalseString;
        public string toggleEnablePartyAlwaysRoll20Hotkey = Storage.isFalseString;

        public string toggleEnableResetCutsceneLockHotkey = Storage.isFalseString;

        public string toggleEnableTaxCollector = Storage.isTrueString;
        public string toggleEnableTeleport = Storage.isFalseString;
        public string toggleEnemiesAlwaysRoll1 = Storage.isFalseString;
        public string toggleEnemiesDamageDealtMultiplier = Storage.isFalseString;
        public string toggleEquipmentRestrictions = Storage.isFalseString;
        public string toggleEveryoneExceptPlayerFactionRolls1 = Storage.isFalseString;


        public string toggleExperienceMultiplier = Storage.isFalseString;
        public string toggleExperimentalIUnderstand = Storage.isFalseString;

        public string toggleExportToModFolder = Storage.isFalseString;

        public string toggleExtraAttacksParty = Storage.isFalseString;
        public string toggleFeatsShowEnemies = Storage.isFalseString;
        public string toggleFeaturesIgnorePrerequisites = Storage.isFalseString;
        public string toggleFfAny = Storage.isFalseString;
        public string toggleFfAOE = Storage.isFalseString;
        public string toggleFilterArmours = Storage.isTrueString;
        public string toggleFilterBelts = Storage.isTrueString;
        public string toggleFilterFootwear = Storage.isTrueString;
        public string toggleFilterGloves = Storage.isTrueString;

        public string toggleFilterHeadwear = Storage.isTrueString;

        //public static string filterHandSimple = Storage.hideString;
        public string toggleFilterKeys = Storage.isTrueString;
        public string toggleFilterMiscellaneousItems = Storage.isTrueString;
        public string toggleFilterNeckItems = Storage.isTrueString;
        public string toggleFilterNotes = Storage.isTrueString;
        public string toggleFilterRings = Storage.isTrueString;
        public string toggleFilterShields = Storage.isTrueString;
        public string toggleFilterShoulderItems = Storage.isTrueString;
        public string toggleFilterUsableItems = Storage.isTrueString;
        public string toggleFilterWeapons = Storage.isTrueString;
        public string toggleFilterWristItems = Storage.isTrueString;
        public bool toggleFogOfWarBool = true;
        public bool toggleFogOfWarBoolDefault = true;
        public string toggleFogOfWarFull = Storage.isTrueString;

        public string toggleFogOfWarVisuals = Storage.isTrueString;
        public string toggleForcedEncounterIsHard = Storage.isFalseString;
        public string toggleFreezeTimedQuestAt90Days = Storage.isFalseString;
        public string toggleFullHitdiceEachLevel = Storage.isFalseString;
        public string toggleHUDToggle = Storage.isFalseString;
        public string toggleIgnoreCasterTypeSpellLevel = Storage.isFalseString;
        public string toggleIgnoreClassAlignment = Storage.isFalseString;
        public string toggleIgnoreFeatureListPrerequisites = Storage.isFalseString;

        public string toggleIgnoreFeaturePrerequisites = Storage.isFalseString;
        public string toggleIgnoreForbiddenArchetype = Storage.isFalseString;

        public string toggleIgnoreForbiddenFeatures = Storage.isFalseString;

        public string toggleIgnorePrerequisites = Storage.isFalseString;
        public string toggleIgnorePrerequisiteStatValue = Storage.isFalseString;
        public string toggleInfiniteAbilities = Storage.isFalseString;
        public string toggleInfiniteItems = Storage.isFalseString;
        public string toggleInfiniteSkillpoints = Storage.isFalseString;
        public string toggleInstantCooldown = Storage.isFalseString;

        public string toggleInstantCooldownMainChar = Storage.isFalseString;
        public string toggleInstantEvent = Storage.isFalseString;
        public string toggleInstantPartyChange = Storage.isFalseString;

        public string toggleInstantRestAfterCombat = Storage.isFalseString;

        public string toggleItemModding = Storage.isFalseString;
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
        public string toggleKingdomEventResultSuccess = Storage.isFalseString;

        public string toggleMainCharacterRoll20 = Storage.isFalseString;
        public string toggleMaterialComponent = Storage.isFalseString;
        public string toggleMaximiseAcceptedBurn = Storage.isFalseString;
        public string toggleMetamagic = Storage.isFalseString;
        public string toggleMoveSpeedAsOne = Storage.isFalseString;

        public string toggleNeverRoll1 = Storage.isFalseString;

        public string toggleNeverRoll20 = Storage.isFalseString;
        /*
        00 = StatType.SaveFortitude
        01 = StatType.SaveReflex
        02 = StatType.SaveWill
        */

        public string toggleNoAttacksOfOpportunity = Storage.isFalseString;

        public string toggleNoBurnKineticist = Storage.isFalseString;

        public string toggleNoDamageFromEnemies = Storage.isFalseString;
        public string toggleNoInactiveCamp = Storage.isFalseString;

        public string toggleNoIngredientsRequired = Storage.isFalseString;

        public string toggleNoLevelUpRestirctions = Storage.isFalseString;

        public string toggleNoNegativeLevels = Storage.isFalseString;

        public string toggleNoRationsRequired = Storage.isFalseString;

        public string toggleNoResourcesClaimCost = Storage.isFalseString;

        public string toggleNoTempHPKineticist = Storage.isFalseString;
        public string toggleOverwriteFogOfWar = Storage.isFalseString;
        public string togglePartyAlwaysRoll20 = Storage.isFalseString;
        public KeyCode togglePartyAlwaysRoll20Key = KeyCode.Insert;
        public string togglePartyDamageDealtMultiplier = Storage.isFalseString;

        public string togglePartyMovementSpeedMultiplier = Storage.isFalseString;
        public string togglePartyOneHitKills = Storage.isFalseString;

        public string togglePassSavingThrowIndividual = Storage.isFalseString;

        public string[] togglePassSavingThrowIndividualArray =
            {Storage.isTrueString, Storage.isTrueString, Storage.isTrueString};

        public string togglePassSkillChecksIndividual = Storage.isFalseString;

        public string[] togglePassSkillChecksIndividualArray =
        {
            Storage.isTrueString, Storage.isTrueString, Storage.isTrueString, Storage.isTrueString,
            Storage.isTrueString, Storage.isTrueString, Storage.isTrueString, Storage.isTrueString,
            Storage.isTrueString, Storage.isTrueString, Storage.isTrueString, Storage.isTrueString,
            Storage.isTrueString, Storage.isTrueString
        };

        public string togglePassSkillChecksIndividualDC99 = Storage.isFalseString;
        public string togglePreventAlignmentChanges = Storage.isFalseString;
        public string togglePreventQuestFailure = Storage.isFalseString;

        public string toggleRandomEncountersEnabled = Storage.isTrueString;
        public string toggleRepeatableLockPicking = Storage.isFalseString;
        public string toggleRepeatableLockPickingLockPicks = Storage.isFalseString;
        public string toggleRepeatableLockPickingLockPicksInventoryText = Storage.isTrueString;
        public string toggleRepeatableLockPickingWeariness = Storage.isFalseString;

        public string toggleRestoreItemChargesAfterCombat = Storage.isFalseString;

        public string toggleRestoreSpellsAbilitiesAfterCombat = Storage.isFalseString;
        public string toggleReverseCasterAlignmentChecks = Storage.isFalseString;
        public string toggleRoll20Initiative = Storage.isFalseString;
        public string toggleRollHitDiceEachLevel = Storage.isFalseString;

        public string toggleRollWithAdvantage = Storage.isFalseString;

        public string toggleRollWithDisadvantage = Storage.isFalseString;

        public string toggleScaleInventoryMoney = Storage.isFalseString;
        public string toggleSearchByAbilityDescription = Storage.isFalseString;
        public string toggleSearchByAbilityName = Storage.isTrueString;
        public string toggleSearchByAbilityObjectName = Storage.isTrueString;
        public string toggleSearchByBuffComponents = Storage.isFalseString;
        public string toggleSearchByBuffDescription = Storage.isFalseString;
        public string toggleSearchByBuffName = Storage.isTrueString;
        public string toggleSearchByBuffObjectName = Storage.isTrueString;
        public string toggleSearchByFeatDescription = Storage.isFalseString;
        public string toggleSearchByFeatName = Storage.isTrueString;
        public string toggleSearchByFeatObjectName = Storage.isTrueString;
        public string toggleSearchByItemDescription = Storage.isFalseString;
        public string toggleSearchByItemFlavorText = Storage.isFalseString;
        public string toggleSearchByItemName = Storage.isTrueString;


        public string toggleSearchByItemObjectName = Storage.isTrueString;

        public string toggleSearchOnlyCraftMagicItems = Storage.isFalseString;

        public string toggleSetEncumbrance = Storage.isFalseString;
        public string toggleSetSpeedOnSummon = Storage.isFalseString;

        public string toggleSetSummonLevelTo20 = Storage.isFalseString;
        public string toggleSettlementRestrictions = Storage.isFalseString;

        public string toggleShowAreaName = Storage.isFalseString;

        public string toggleShowClassDataOptions = Storage.isFalseString;

        public string toggleShowOnlyClassSkills = Storage.isFalseString;

        public string toggleShowPetInventory = Storage.isFalseString;

        public string toggleShowTooltips = Storage.isTrueString;

        public string toggleSkipSpellSelection = Storage.isFalseString;
        public string toggleSortSpellbooksAlphabetically = Storage.isFalseString;
        public string toggleSortSpellsAlphabetically = Storage.isFalseString;
        public string toggleSpellbookAbilityAlignmentChecks = Storage.isFalseString;

        public string toggleSpiderBegone = Storage.isFalseString;
        public string toggleSpontaneousCopyScrolls = Storage.isFalseString;

        public string toggleStopGameTime = Storage.isFalseString;

        public string toggleSummonDurationMultiplier = Storage.isFalseString;
        public string toggleTakeXSkillsOnly = Storage.isFalseString;

        public string toggleUberLogger = Storage.isFalseString;
        public string toggleUberLoggerForward = Storage.isFalseString;
        public string toggleUberLoggerForwardPrefix = Storage.isFalseString;
        public string toggleUndetectableStealth = Storage.isFalseString;

        public string toggleUnityModManagerButton = Storage.isTrueString;
        public string toggleUnlimitedCasting = Storage.isFalseString;
        public string toggleVendorsBuyFor0 = Storage.isFalseString;
        public string toggleVendorSellPriceMultiplier = Storage.isFalseString;
        public string toggleVendorsSellFor0 = Storage.isFalseString;
        public float travelSpeedMultiplier = 1.0f;
        public string travelSpeedMultiplierString = "1.0";
        public float unityModManagerButtonIndex = 1f;
        public bool useCustomDebugTimeMultiplier = false;
        public bool useCustomExperienceMultiplier = false;
        public bool useCustomFatigueHoursModifierMultiplier = false;
        public bool useCustomHeavyEncumbranceMultiplier = false;
        public bool useCustomMoneyMultiplier = false;
        public bool useCustomspellsPerDayMultiplier = false;

        public string vendorSellPriceMultiplier = "0.25";


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}