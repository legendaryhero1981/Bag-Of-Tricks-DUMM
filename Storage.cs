using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Globalmap;
using Kingmaker.UnitLogic;
using TMPro;
using UnityEngine;

namespace BagOfTricks
{
    public static class Storage
    {
        public static Settings settings;

        public static string[] mainToolbarStrings =
        {
            RichText.MainCategoryFormat(Strings.GetText("mainCategory_FavouriteFunctions")),
            RichText.MainCategoryFormat(Strings.GetText("mainCategory_Cheats")),
            RichText.MainCategoryFormat(Strings.GetText("mainCategory_Mods")),
            RichText.MainCategoryFormat(Strings.GetText("mainCategory_Tools")),
            RichText.MainCategoryFormat(Strings.GetText("mainCategory_Settings"))
        };


        public static bool settingsWarning = false;

        public static readonly string gamerVersionAtCreation = "2.0.5c";

        public static readonly string gameHistoryLogPrefix = "[BagOfTricks] ";

        public static readonly string assetBundlesFolder = "AssetBundles";
        public static readonly string charactersImportFolder = "Characters";
        public static readonly string favouritesFolder = "Favourites";
        public static readonly string itemSetsFolder = "ItemSets";
        public static readonly string localisationFolder = "Localisation";
        public static readonly string savesFolder = "Saves";
        public static readonly string taxCollectorFolder = "TaxCollector";
        public static readonly string modifiedBlueprintsFolder = "ModifiedBlueprints";
        public static readonly string exportFolder = "Export";


        public static string currentItemSearch = "";

        public static List<string> itemMultipleGuid = new List<string>();

        public static readonly string scribeScrollBlueprintPrefix = "#ScribeScroll";
        public static readonly string craftMagicItemsBlueprintPrefix = "#CraftMagicItems";


        public static readonly List<string> validItemTypes = new List<string>
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

        public static List<string> validItemTypesFiltered = new List<string>
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

        public static readonly string isTrueString = "<color=green><b>✔</b></color>";
        public static readonly string isFalseString = "<color=red><b>✖</b></color>";

        public static readonly string favouriteTrueString = "<color=red><b>❤️</b></color>";
        public static readonly string favouriteFalseString = "<color=grey><b>❤️</b></color>";

        public static readonly string upArrow = "<color=grey><b>▲</b></color>";
        public static readonly string downArrow = "<color=grey><b>▼</b></color>";

        public static bool itemFavouritesLoad = true;
        public static List<string> itemFavouritesGuids = new List<string>();
        public static List<string> itemFavouriteNames = new List<string>();
        public static List<bool> toggleItemFavouriteDescription = new List<bool>();

        public static List<BlueprintScriptableObject> blueprintList = new List<BlueprintScriptableObject>();

        public static string[] filesCsv = { };
        public static List<bool> togglePreviewCsv = new List<bool>();
        public static List<string> previewStringsCsv = new List<string>();

        public static string[] filesTxt = { };
        public static List<bool> togglePreviewTxt = new List<bool>();
        public static List<string> previewStringsTxt = new List<string>();

        public static List<string> validItemObjectNames = new List<string>();
        public static List<string> validItemNames = new List<string>();
        public static List<string> validItemDescriptions = new List<string>();
        public static List<string> validItemFlavorTexts = new List<string>();
        public static List<string> validItemGuids = new List<string>();
        public static List<bool> toggleItemSearchDescription = new List<bool>();

        public static List<string> resultItemNames = new List<string>();
        public static List<string> resultItemGuids = new List<string>();

        public static string fadeToBlackState = Strings.GetText("misc_Enable");

        public static float flyingHeightSlider = 0f;

        public static string notificationEkunQ2rewardArmor = Strings.GetText("error_EkunQ2");
        public static string errorString = "";

        public static string currentBuffSearch = "";

        public static readonly List<string> validBuffTypes = new List<string>
        {
            "BlueprintBuff"
        };

        public static List<string> buffResultNames = new List<string>();
        public static List<string> buffResultGuids = new List<string>();
        public static List<bool> buffToggleResultDescription = new List<bool>();
        public static List<string> buffResultDescriptions = new List<string>();

        public static bool buffFavouritesLoad = true;
        public static List<string> buffFavouritesGuids = new List<string>();
        public static List<string> buffFavouriteNames = new List<string>();
        public static List<bool> buffToggleFavouriteDescription = new List<bool>();
        public static List<string> buffFavouritesDescriptions = new List<string>();

        public static List<string> buffValidObjectNames = new List<string>();
        public static List<string> buffValidNames = new List<string>();
        public static List<string> buffValidDescriptions = new List<string>();
        public static List<string> buffValidComponents = new List<string>();
        public static List<string> buffValidGuids = new List<string>();

        public static bool buffAllLoad = true;
        public static List<string> buffAllObjectNames = new List<string>();
        public static List<string> buffAllNames = new List<string>();
        public static List<string> buffAllGuids = new List<string>();
        public static List<string> buffAllDescriptions = new List<string>();
        public static UnitEntityData buffAllUnitEntityData = null;

        public static List<string> buffControllableCharacterNamesList = new List<string>();
        public static int buffSelectedControllableCharacterIndex = 0;
        public static List<UnitEntityData> buffPartyMembers = new List<UnitEntityData>();
        public static List<string> buffAllUnitsNamesList = new List<string>();
        public static List<UnitEntityData> buffAllUnits = new List<UnitEntityData>();
        public static bool reloadPartyStats = true;
        public static bool reloadPartyBuffs = true;


        public static List<UnitEntityData> statsPartyMembers = new List<UnitEntityData>();
        public static List<string> statsControllableCharacterNamesList = new List<string>();
        public static int statsSelectedControllableCharacterIndex = 0;
        public static int statsSelectedControllableCharacterIndexOld = 0;

        public static string[] individualSkillsArray =
        {
            Strings.GetText("charStat_Athletics"), Strings.GetText("charStat_KnowledgeArcana"),
            Strings.GetText("charStat_KnowledgeWorld"), Strings.GetText("charStat_LoreNature"),
            Strings.GetText("charStat_LoreReligion"), Strings.GetText("charStat_Mobility"),
            Strings.GetText("charStat_Perception"), Strings.GetText("charStat_Persuasion"),
            Strings.GetText("charStat_Stealth"), Strings.GetText("charStat_Thievery"),
            Strings.GetText("charStat_UseMagicDevice"), Strings.GetText("charStat_Bluff"),
            Strings.GetText("charStat_Diplomacy"), Strings.GetText("charStat_Intimidate")
        };

        public static string[] individualSavesArray =
        {
            Strings.GetText("charStat_Fortitude"), Strings.GetText("charStat_Reflex"), Strings.GetText("charStat_Will")
        };

        public static Dictionary<string, StatType> statsAttributesDict = new Dictionary<string, StatType>
        {
            {Strings.GetText("charStat_Strength"), StatType.Strength},
            {Strings.GetText("charStat_Dexterity"), StatType.Dexterity},
            {Strings.GetText("charStat_Constitution"), StatType.Constitution},
            {Strings.GetText("charStat_Intelligence"), StatType.Intelligence},
            {Strings.GetText("charStat_Wisdom"), StatType.Wisdom},
            {Strings.GetText("charStat_Charisma"), StatType.Charisma}
        };

        public static Dictionary<string, StatType> statsSkillsDict = new Dictionary<string, StatType>
        {
            {individualSkillsArray[0], StatType.SkillAthletics},
            {individualSkillsArray[1], StatType.SkillKnowledgeArcana},
            {individualSkillsArray[2], StatType.SkillKnowledgeWorld},
            {individualSkillsArray[3], StatType.SkillLoreNature},
            {individualSkillsArray[4], StatType.SkillLoreReligion},
            {individualSkillsArray[5], StatType.SkillMobility},
            {individualSkillsArray[6], StatType.SkillPerception},
            {individualSkillsArray[7], StatType.SkillPersuasion},
            {individualSkillsArray[8], StatType.SkillStealth},
            {individualSkillsArray[9], StatType.SkillThievery},
            {individualSkillsArray[10], StatType.SkillUseMagicDevice}
        };

        public static Dictionary<string, StatType> statsSocialSkillsDict = new Dictionary<string, StatType>
        {
            {individualSkillsArray[11], StatType.CheckBluff},
            {individualSkillsArray[12], StatType.CheckDiplomacy},
            {individualSkillsArray[13], StatType.CheckIntimidate}
        };

        public static Dictionary<string, StatType> statsSavesDict = new Dictionary<string, StatType>
        {
            {individualSavesArray[0], StatType.SaveFortitude},
            {individualSavesArray[1], StatType.SaveReflex},
            {individualSavesArray[2], StatType.SaveWill}
        };

        public static Dictionary<string, StatType> statsCombatDict = new Dictionary<string, StatType>
        {
            {Strings.GetText("charStat_Initiative"), StatType.Initiative},
            {Strings.GetText("charStat_BaseAttackBonus"), StatType.BaseAttackBonus},
            {Strings.GetText("charStat_AdditionalAttackBonus"), StatType.AdditionalAttackBonus},
            {Strings.GetText("charStat_NumberOfAttacksOfOpportunity"), StatType.AttackOfOpportunityCount},
            {Strings.GetText("charStat_SneakAttack"), StatType.SneakAttack},
            {Strings.GetText("charStat_AdditionalDamage"), StatType.AdditionalDamage},
            {Strings.GetText("charStat_AdditionalCombatManeuverBonus"), StatType.AdditionalCMB},
            {Strings.GetText("charStat_AdditionalCombatManeuverDefense"), StatType.AdditionalCMD},
            {Strings.GetText("charStat_ArmourClass"), StatType.AC},
            {Strings.GetText("charStat_Reach"), StatType.Reach},
            {Strings.GetText("charStat_HitPoints"), StatType.HitPoints},
            {Strings.GetText("charStat_Speed"), StatType.Speed}
        };

        public static List<UnitEntityData> enemyUnits = new List<UnitEntityData>();
        public static List<string> enemyUnitsNamesList = new List<string>();
        public static int enemyUnitIndex = 0;

        public static GlobalMapLocation lastHoveredLocation = null;

        public static string encounterError = "";

        public static List<string> blueprintRandomCombatEncounterGuids = new List<string>();

        public static string modEntryPath = "";
        public static string modAssemblyName = "";


        public static List<string> itemFavourites = new List<string>();
        public static readonly string favouritesItemsFile = "Items.xml";

        public static List<string> buffFavourites = new List<string>();
        public static readonly string favouritesBuffsFile = "Buffs.xml";

        public static bool globalChanged = false;
        public static string fovValue = "1";
        public static string globalString = Strings.GetText("button_SetGlobalMapZoomLevel");

        public static List<UnitEntityData> partyMembersFocusUnits = new List<UnitEntityData>();

        public static CultureInfo cultureEN = new CultureInfo("en");

        public static readonly string taxCollectorFile = "Save.xml";

        public static string[] alignmentsArrayKingdom =
        {
            Strings.GetText("arrayItem_Alignments_LawfulGood"), Strings.GetText("arrayItem_Alignments_NeutralGood"),
            Strings.GetText("arrayItem_Alignments_ChaoticGood"), Strings.GetText("arrayItem_Alignments_LawfulNeutral"),
            Strings.GetText("arrayItem_Alignments_TrueNeutral"), Strings.GetText("arrayItem_Alignments_ChaoticNeutral"),
            Strings.GetText("arrayItem_Alignments_LawfulEvil"), Strings.GetText("arrayItem_Alignments_NeutralEvil"),
            Strings.GetText("arrayItem_Alignments_ChaoticEvil"), Strings.GetText("arrayItem_Alignments_NoChange")
        };

        public static string[] alignmentsArrayStats =
        {
            Strings.GetText("arrayItem_Alignments_LawfulGood"), Strings.GetText("arrayItem_Alignments_NeutralGood"),
            Strings.GetText("arrayItem_Alignments_ChaoticGood"), Strings.GetText("arrayItem_Alignments_LawfulNeutral"),
            Strings.GetText("arrayItem_Alignments_TrueNeutral"), Strings.GetText("arrayItem_Alignments_ChaoticNeutral"),
            Strings.GetText("arrayItem_Alignments_LawfulEvil"), Strings.GetText("arrayItem_Alignments_NeutralEvil"),
            Strings.GetText("arrayItem_Alignments_ChaoticEvil"), Strings.GetText("arrayItem_Alignments_Good"),
            Strings.GetText("arrayItem_Alignments_Evil"), Strings.GetText("arrayItem_Alignments_Lawful"),
            Strings.GetText("arrayItem_Alignments_Chaotic")
        };

        public static string[] alignmentsArrayKingdomStats =
        {
            Strings.GetText("arrayItem_Alignments_LawfulGood"), Strings.GetText("arrayItem_Alignments_NeutralGood"),
            Strings.GetText("arrayItem_Alignments_ChaoticGood"), Strings.GetText("arrayItem_Alignments_LawfulNeutral"),
            Strings.GetText("arrayItem_Alignments_TrueNeutral"), Strings.GetText("arrayItem_Alignments_ChaoticNeutral"),
            Strings.GetText("arrayItem_Alignments_LawfulEvil"), Strings.GetText("arrayItem_Alignments_NeutralEvil"),
            Strings.GetText("arrayItem_Alignments_ChaoticEvil")
        };


        public static int statsSelectedAlignmentIndex = 0;

        public static string[] takeXArray =
        {
            Strings.GetText("arrayItem_takeX_Off"), Strings.GetText("arrayItem_takeX_Take10"),
            Strings.GetText("arrayItem_takeX_Take20"), Strings.GetText("arrayItem_takeX_TakeCustomValue")
        };

        public static List<string> menuItemFavourites = new List<string>();
        public static readonly string favouritesMenuItemsFile = "MenuItems.xml";

        public static string[] localisationsXML = { };
        public static List<string> localisationsXMLFiles = new List<string>();
        public static int selectedLocalisationOld = -1;

        public static float defaultVendorSellPriceMultiplier = 0.25f;

        public static int partySelectedSizeIndex = 0;

        public static string[] charSizeArray =
        {
            Strings.GetText("arrayItem_Size_Fine"), Strings.GetText("arrayItem_Size_Diminutive"),
            Strings.GetText("arrayItem_Size_Tiny"), Strings.GetText("arrayItem_Size_Small"),
            Strings.GetText("arrayItem_Size_Medium"), Strings.GetText("arrayItem_Size_Large"),
            Strings.GetText("arrayItem_Size_Huge"), Strings.GetText("arrayItem_Size_Gargantuan"),
            Strings.GetText("arrayItem_Size_Colossal")
        };

        public static int enemiesSelectedSizeIndex = 0;

        public static bool reloadPartyFeats = true;
        public static List<UnitEntityData> featsAllUnits = new List<UnitEntityData>();
        public static List<string> featsAllUnitsNamesList = new List<string>();
        public static List<string> featsControllableCharacterNamesList = new List<string>();
        public static int featsSelectedControllableCharacterIndex = 0;
        public static List<UnitEntityData> featsPartyMembers = new List<UnitEntityData>();
        public static bool featFavouritesLoad = true;
        public static int featsParamIndex = 0;

        public static string[] featsParamArray =
        {
            Strings.GetText("misc_None"), Strings.GetText("arrayItem_FeatParam_FencingGrace"),
            Strings.GetText("arrayItem_FeatParam_ImprovedCritical"),
            Strings.GetText("arrayItem_FeatParam_SlashingGrace"),
            Strings.GetText("arrayItem_FeatParam_SwordSaintChosenWeapon"),
            Strings.GetText("arrayItem_FeatParam_WeaponFocus"),
            Strings.GetText("arrayItem_FeatParam_WeaponFocusGreater"),
            Strings.GetText("arrayItem_FeatParam_WeaponMastery"),
            Strings.GetText("arrayItem_FeatParam_WeaponSpecialization"),
            Strings.GetText("arrayItem_FeatParam_WeaponSpecializationGreater"),
            Strings.GetText("arrayItem_FeatParam_SpellFocus"), Strings.GetText("arrayItem_FeatParam_GreaterSpellFocus")
        };

        public static List<string> featFavourites = new List<string>();
        public static readonly string favouritesFeatsFile = "Feats.xml";
        public static List<string> featFavouritesGuids = new List<string>();
        public static List<string> featFavouriteNames = new List<string>();
        public static List<bool> featToggleFavouriteDescription = new List<bool>();
        public static List<string> featFavouritesDescriptions = new List<string>();

        public static string currentFeatSearch = "";

        public static readonly List<string> validFeatTypes = new List<string>
        {
            "BlueprintFeature"
        };

        public static List<string> featResultNames = new List<string>();
        public static List<string> featResultGuids = new List<string>();
        public static List<bool> featToggleResultDescription = new List<bool>();
        public static List<string> featResultDescriptions = new List<string>();

        public static List<string> featValidObjectNames = new List<string>();
        public static List<string> featValidNames = new List<string>();
        public static List<string> featValidDescriptions = new List<string>();
        public static List<string> featValidGuids = new List<string>();

        public static bool featAllLoad = true;
        public static List<string> featAllObjectNames = new List<string>();
        public static List<string> featAllNames = new List<string>();
        public static List<int> featAllRanks = new List<int>();
        public static List<string> featAllGuids = new List<string>();
        public static List<string> featAllDescriptions = new List<string>();
        public static UnitEntityData featAllUnitEntityData = null;

        public static string assetBunldeFlags = "flags";
        public static AssetBundle flagsBundle;
        public static List<Texture2D> flagsTextures = new List<Texture2D>();
        public static List<GUIContent> localeGrid = new List<GUIContent>();
        public static bool loadOnce = true;

        public static Rect ummRect = new Rect();
        public static float ummWidth = 960f;
        public static Vector2[] ummScrollPosition;
        public static int ummTabId = 0;
        public static float setCharLevel = 1f;

        public static bool romanceCounterLoad = true;
        public static BlueprintRomanceCounter[] blueprintRomanceCounters;
        public static bool romanceCounterLoadExperimental = true;


        public static string[] neverRollXArray =
        {
            Strings.GetText("arrayItem_NeverRollX_Everyone"), Strings.GetText("arrayItem_NeverRollX_OnlyParty"),
            Strings.GetText("arrayItem_NeverRollX_OnlyEnemies")
        };

        public static string[] unitEntityDataSelectionGridArray =
        {
            Strings.GetText("arrayItem_NeverRollX_Everyone"), Strings.GetText("arrayItem_NeverRollX_OnlyParty"),
            Strings.GetText("arrayItem_NeverRollX_OnlyMainChar"), Strings.GetText("arrayItem_NeverRollX_OnlyEnemies")
        };

        public static List<string> togglesFavourites = new List<string>();
        public static readonly string favouritesTogglesFileOld = "Toggles.xml";
        public static readonly string favouritesTogglesFile = "Functions.xml";
        public static bool togglesFavouritesLoad = true;

        public static UnitEntityData unitLockPick;

        public static int lockPicks = 5;
        public static int lockPicksCreationDC = 10;
        public static bool checkLockPick = false;
        public static TextMeshProUGUI lockPicksNow = null;

        public static bool showModMenu = false;

        public static int statsSelectedClassIndex = 0;
        public static List<string> classNames = new List<string>();
        public static List<ClassData> classData = new List<ClassData>();
        public static float classLevelSlider = 0;

        public static string[] unitEntityDataArray =
        {
            Strings.GetText("arrayItem_UnityEntityData_Party"),
            Strings.GetText("arrayItem_UnityEntityData_ControllableCharacters"),
            Strings.GetText("arrayItem_UnityEntityData_ActiveCompanions"),
            Strings.GetText("arrayItem_UnityEntityData_AllCharacters"),
            Strings.GetText("arrayItem_UnityEntityData_Mercenaries"), Strings.GetText("arrayItem_UnityEntityData_Pets"),
            Strings.GetText("arrayItem_UnityEntityData_Enemies")
        };

        public static string[] unitEntityDataArrayNoEnemies =
            unitEntityDataArray.Take(unitEntityDataArray.Count() - 1).ToArray();

        public static int xpFilterUnitEntityDataIndex = 0;
        public static List<UnitEntityData> xpUnitEntityData;


        public static int statsFilterUnitEntityDataIndex = 0;
        public static int statsFilterUnitEntityDataIndexOld = 0;
        public static List<UnitEntityData> statsUnitEntityData;

        public static int featsFilterUnitEntityDataIndex = 0;
        public static int featsFilterUnitEntityDataIndexOld = 0;
        public static List<UnitEntityData> featsUnitEntityData;

        public static int buffsFilterUnitEntityDataIndex = 0;
        public static int buffsFilterUnitEntityDataIndexOld = 0;
        public static List<UnitEntityData> buffsUnitEntityData;

        public static int addAbilitiesFilterUnitEntityDataIndex = 0;
        public static int addAbilitiesFilterUnitEntityDataIndexOld = 0;
        public static List<UnitEntityData> addAbilitiesUnitEntityData;
        public static bool reloadPartyAddAbilities = true;
        public static int addAbilitiesSelectedControllableCharacterIndex = 0;
        public static List<string> addAbilitiesAllUnitsNamesList = new List<string>();

        public static List<UnitEntityData> addAbilitiesAllUnits = new List<UnitEntityData>();
        public static bool addAbilitiesAllLoad = true;
        public static UnitEntityData addAbilitiesAllUnitEntityData = null;
        public static List<string> addAbilitiesAllNames = new List<string>();
        public static List<string> addAbilitiesAllObjectNames = new List<string>();
        public static List<string> addAbilitiesAllGuids = new List<string>();
        public static List<string> addAbilitiesAllDescriptions = new List<string>();

        public static bool addAbilitiesFavouritesLoad = true;
        public static List<string> abilitiesFavourites = new List<string>();
        public static readonly string favouritesAbilitiesFile = "Abilities.xml";
        public static List<string> abilitiesFavouritesGuids = new List<string>();
        public static List<string> abilitiesFavouritesNames = new List<string>();
        public static List<bool> abilitiesToggleFavouriteDescription = new List<bool>();
        public static List<string> abilitiesFavouritesDescriptions = new List<string>();

        public static readonly List<string> validAbilityTypes = new List<string>
        {
            "BlueprintAbility"
        };

        public static List<string> abilityResultNames = new List<string>();
        public static List<string> abilityResultGuids = new List<string>();
        public static List<string> abilityResultDescriptions = new List<string>();
        public static List<string> abilityResultTypes = new List<string>();


        public static List<string> abilityValidObjectNames = new List<string>();
        public static List<string> abilityValidNames = new List<string>();
        public static List<string> abilityValidDescriptions = new List<string>();
        public static List<string> abilityValidGuids = new List<string>();
        public static string currentAbilitySearch = "";


        public static List<bool> abilityToggleResultDescription = new List<bool>();

        public static List<string> animationsControllableCharacterNamesList = new List<string>();
        public static int animationsSelectedControllableCharacterIndex = 0;
        public static List<UnitEntityData> animationsPartyMembers = new List<UnitEntityData>();
        public static bool reloadPartyAnimations = true;

        public static string partyCustomName = string.Empty;

        public static int spellSpellbooksFilterUnitEntityDataIndex = 0;
        public static int spellSpellbooksFilterUnitEntityDataIndexOld = 0;
        public static List<UnitEntityData> spellSpellbooksUnitEntityData;
        public static bool reloadPartySpellSpellbookChars = true;
        public static int spellSpellbooksSelectedControllableCharacterIndex = 0;
        public static int spellSpellbooksSelectedControllableCharacterIndexOld = 0;
        public static List<string> spellSpellbooksAllUnitsNamesList = new List<string>();
        public static List<UnitEntityData> spellSpellbooksAllUnits = new List<UnitEntityData>();

        public static bool reloadPartySpellSpellbookSpellbooks = true;
        public static List<string> spellSpellbooksSavedSpellbooksNames = new List<string>();
        public static List<Spellbook> spellSpellbooksSavedSpellbooks = new List<Spellbook>();
        public static int spellSpellbooksSavedSpellbooksCount = 0;
        public static int spellSpellbooksSelectedSavedSpellbooksIndex = 0;
        public static int spellSpellbooksSelectedSavedSpellbooksIndexOld = 0;

        public static bool spellSpellbooksShowKnownSpells = false;

        public static bool spellSpellbooksShowLearnableSpells = false;
        public static bool reloadSpellSpellbooksLearnableSpells = true;
        public static int spellSpellbooksSpellLevelListIndex = 0;
        public static int spellSpellbooksSpellLevelListIndexOld = 0;

        public static List<string> spellSpellbooksSavedSpellbooksLevels = new List<string>();
        public static List<bool> spellSpellbooksSavedSpellbooksDescription = new List<bool>();

        public static bool reloadSpellSpellbooksLearnableSpellsList = true;
        public static bool spellSpellbooksLearnableSpellsListShowKnown = false;

        public static List<SpellLevelList> spellSpellbooksSpellLevelLists = new List<SpellLevelList>();
        public static bool spellSpellbooksShowAddAbilitiesAsSpells = false;

        public static int spellSpellbooksSpellLevelAbilitiesIndex = 0;
        public static int spellSpellbooksSpellLevelAbilitiesIndexOld = 0;

        public static float healthPercentageSlider = 1f;

        public static string[] encumbranceArray =
        {
            Strings.GetText("encumbrance_Light"), Strings.GetText("encumbrance_Medium"),
            Strings.GetText("encumbrance_Heavy"), Strings.GetText("encumbrance_Overload")
        };

        public static bool hideVersionMismatch = false;
        public static bool showAchievementsMenu = false;

        public static bool showInventory = false;
        public static List<string> inventoryItems = new List<string>();
        public static List<int> inventoryItemsCount = new List<int>();

        public static string[] inventoryItemTypesArray =
        {
            RichText.Bold(Strings.GetText("misc_All")), Strings.GetText("label_Armours"),
            Strings.GetText("label_Belts"), Strings.GetText("label_Footwear"), Strings.GetText("label_Gloves"),
            Strings.GetText("label_Headwear"), Strings.GetText("label_Neckwear"), Strings.GetText("label_NonUsable"),
            Strings.GetText("misc_Other"), Strings.GetText("label_Rings"), Strings.GetText("label_Shields"),
            Strings.GetText("label_ShoulderItems"), Strings.GetText("label_UsableItems"),
            Strings.GetText("label_Weapons"), Strings.GetText("label_WristItems")
        };


        public static string[] weatherArray =
        {
            Strings.GetText("arrayItem_Weather_Normal"), Strings.GetText("arrayItem_Weather_Rain"),
            Strings.GetText("arrayItem_Weather_Snow")
        };

        public static bool hudHidden = false;

        public static float weatherIntensity = 0.5f;

        public static string[] diceTypes = {"D0", "D1", "D2", "D3", "D4", "D6", "D8", "D10", "D12", "D20", "D100"};

        public static List<string> spiderGuids = new List<string>
        {
            "ae2806b1e73ed7b4e9e9ae966de4dad6", //Corrupted_BloomInfusedSpiderMatriarch
            "d0e28afa4e4c0994cb6deae66612445a", //CR1_GiantSpiderMite
            "c4b33e5fd3d3a6f46b2aade647b0bf25", //CR1_GiantSpiderStandard
            "a28e944558ed5b64790c3701e8c89d75", //CR1_SpiderSwarm
            "e9c1c68972cc4904dacdf2df9acf6730", //CR2_GiantSpiderAdvanced
            "18a3ceeb3fb44f24ea6d3035a5f05a8c", //CR2_GiantSpiderMiteAdvanced
            "ba9451623f3f13742a8bd12da4822d2b", //CR4_GiantSpider
            "da2f152d19ce4d54e8c17da91f01fabd", //CR5_QuickSpiderSwarm
            "a813d907bc55e734584d99a038c9211e", //CR6_BloomInfusedSpider
            "51c66b0783a748c4b9538f0f0678c4d7", //CR7_GiantSpiderDoombringing
            "07467e9a29a215346ab66fec7963eb62", //CR7_GiantSpiderQuickling
            "63897b4df57da2f4396ca8a6f34723e7", //CR11_BloomInfusedSpiderMatriarch
            "ed734a6c822bdac448b98abfd7c03814", //GiantSpider_Cutscene
            "4622aca7715007147b26b7fc26db3df8", //GiantSpiderBloomInfused
            "5bb1781abca825f49a53869ff79a2c6f", //GiantSpiderMiniboss
            "9e120b5e0ad3c794491c049aa24b9fde", //GiantSpiderSummoned
            "f2327e24765fb6342975b6216bfb307b", //SpiderSwarmSummoned
            "36fe14f64d7746f429d30f9dd7b2e652", //SycamoreBoss_GiantSpider
            "af6700830836d9643939e1a1801a65af", //TrollLairFW_Zzamas
            "254091b112392c04db701331fbea3b8f" //Zzamas
        };

        public static string kingdomCustomName = string.Empty;

        public static string summonDurationMultiplierValue = "1";
        public static float finalSummonDurationMultiplierValue = 1f;


        public static string buffDurationMultiplierValue = "1";
        public static float finalBuffDurationMultiplierValue = 1f;
    }
}