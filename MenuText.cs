using System.Collections.Generic;

namespace BagOfTricks
{
    internal class MenuText
    {
        public static Dictionary<string, string> fallback = new Dictionary<string, string>
        {
            /*
            Pathfinder Kingmaker locales:
            enGB
            ruRU
            deDE
            frFR
            itIT
            esES
            zhCN
            */
            //locale start
            {"locale", "fallback"},
            {"contributors", "-"},
            {"contributors_text", "Contributors for the selected language"},
            //locale end

            //mainCategory start
            {"mainCategory_XP", "Character Creation, Progression &  Experience"},
            {"mainCategory_Encumbrance", "Encumbrance & Carrying Capacity"},
            {"mainCategory_Rest", "Rest & Rations"},
            {"mainCategory_Money", "Cost & Money"},
            {"mainCategory_PartyStats", "Party Options"},
            {"mainCategory_EnemyStats", "Enemy Statistics"},
            {"mainCategory_Items", "Items & Equipment"},
            {"mainCategory_Buffs", "Buffs"},
            {"mainCategory_SpellAbilityOptions", "Spell & Ability Options"},
            {"mainCategory_MapTravel", "Map & Travel"},
            {"mainCategory_Kingdom", "Kingdom"},
            {"mainCategory_Dice", "Dice Rolls"},
            {"mainCategory_CameraTools", "Camera Tools"},
            {"mainCategory_DevTools", "Development Tools"},
            {"mainCategory_BlueprintModding", "Blueprint Modding"},
            {"mainCategory_Misc", "Misc"},
            {"mainCategory_KeyBindings", "Key Bindings"},
            {"mainCategory_TaxCollector", "Tax Collector"},
            {"mainCategory_Feats", "Feats"},
            {"mainCategory_AddAbilities", "Abilities"},
            {"mainCategory_SpellsAndSpellbooks", "Spells & Spellbooks"},
            {"mainCategory_BeneathTheStolenLands", "Beneath The Stolen Lands"},

            {"mainCategory_FavouriteFunctions", "Favourite Functions"},
            {"mainCategory_Cheats", "Cheats"},
            {"mainCategory_Mods", "Mods"},
            {"mainCategory_Tools", "Tools"},
            {"mainCategory_Settings", "Settings"},
            //mainCategory end

            //header start            
            {"header_UISoundTypes", "UI Sound Types"},
            {"header_TakeX", "Take X"},
            {"header_CC", "Character Creation"},
            {"header_Progression", "Progression"},
            {"header_Experience", "Experience"},
            {"header_Multiplier", "Multiplier"},
            {"header_UseCustomMultiplier", "Use Custom Multiplier "},
            {"header_CustomMultiplier", "Custom Multiplier"},
            {"header_RestLess", "Rest Less - Slower Weariness"},
            {"header_AttributesBaseValues", "Attributes: Base Values (Total Values)"},
            {"header_SkillsRanks", "Skills: Ranks (Total Values)"},
            {"header_SocialSkillsBaseValues", "Social Skills: Base Values (Modified Values)"},
            {"header_StatsSaves", "Saves: Class Bonuses (Total Values)"},
            {"header_StatsCombat", "Combat: Base Values (Modified Values)"},
            {"header_PartyMultipliers", "Party Multipliers"},
            {"header_Attributes", "Attributes"},
            {"header_Skills", "Skills"},
            {"header_SocialSkills", "Social Skills"},
            {"header_Saves", "Saves"},
            {"header_Combat", "Combat"},
            {"header_Rank", "Rank"},
            {"header_EnemyMultipliers", "Enemy Multipliers"},
            {"header_SearchAndAddItems", "Search & Add Items"},
            {"header_ItemSearch", "Item Search"},
            {"header_Search", "Search"},
            {"header_SpellsPerDay", "Spells Per Day / Memorised Spells"},
            {"header_RandomEncounterSettings", "Random Encounter Settings"},
            {"header_BuildingTimeModifier", "Building Time Modifier"},
            {"header_BuildPoints", "Build Points"},
            {"header_Statistics", "Statistics"},
            {"header_Experimental", "Experimental"},
            {"header_Sounds", "Sounds"},
            {"header_GameConstants", "Game Constants"},
            {"header_Language", "Language"},
            {"header_Level", "Level"},
            {"header_RomanceCounters", "Romance Counters"},
            {"header_Alignment", "Alignment"},
            {"header_Size", "Size"},
            {"header_TimeScale", "Time Scale"},
            {"header_Animations", "Play Animations"},
            {"header_CarryingCapacity", "Carrying Capacity"},
            {"header_About", "About"},
            {"header_KingdomAlignment", "Kingdom Alignment"},
            {"header_PartySpeed", "Kingdom Alignment"},
            {"header_Achievements", "Achievements"},
            //header end

            //label start
            {"label_CurrentMultiplier", "Current Multiplier"},
            {"label_CurrentMoney", "Current Money"},
            {"label_CurrentAlignment", "Current Alignment"},
            {"label_CurrentCasterLevel", "Current Caster Level"},
            {"label_Location", "Location"},
            {"label_InStealth", "in stealth"},
            {"label_ItemName", "Item Name"},
            {"label_ItemType", "Item Type"},
            {"label_ItemDescription", "Item Description"},
            {"label_ItemFlavourText", "Item Flavour Text"},
            {"label_ItemCost", "Item Cost"},
            {"label_ItemSellPrice", "Item Sell Price"},
            {"label_ItemWeight", "Item Weight"},
            {"label_ObjectName", "Object Name"},
            {"label_ObjectNameClean", "Object Name (cleaned)"},
            {"label_ItemSubtypeName", "Item Subtype Name"},
            {"label_ItemSubtypeDescription", "Item Subtype Description"},
            {"label_ItemGuid", "Item Guid"},
            {"label_ItemComment", "Item Comment"},
            {"label_ItemIcon", "Item Icon"},
            {"label_ItemIdentifyDC", "Item Identify DC"},
            {"label_ItemInventoryPutSound", "Item Inventory Put Sound"},
            {"label_ItemInventoryTakeSound", "Item Inventory Take Sound"},
            {"label_ItemIsStackable", "Item Is Actually Stackable"},
            {"label_ItemIsNotable", "Item Is Notable"},
            {"label_ItemMiscellaneousType", "Item Miscellaneous Type"},
            {"label_ItemNonIdentifiedDescription", "Item Non Identified Description"},
            {"label_ItemNonIdentifiedName", "Item Non Identified Name"},
            {"label_AssetGuid", "AssetGuid"},
            {"label_Amount", "Amount"},
            {"label_DisplayItemInformation", "Display Item Information"},
            {"label_Description", "Description"},
            {"label_FlavourText", "Flavour Text"},
            {"label_Armours", "Armours"},
            {"label_Belts", "Belts"},
            {"label_Footwear", "Footwear"},
            {"label_Gloves", "Gloves"},
            {"label_Headwear", "Headwear"},
            {"label_Keys", "Keys"},
            {"label_MiscellaneousItems", "Miscellaneous Items"},
            {"label_NeckItems", "Neck Items"},
            {"label_Neckwear", "Neckwear"},
            {"label_NonUsable", "Non Usable"},
            {"label_Notes", "Notes"},
            {"label_Rings", "Rings"},
            {"label_Shields", "Shields"},
            {"label_ShoulderItems", "Shoulder Items"},
            {"label_UsableItems", "Usable Items"},
            {"label_Weapons", "Weapons"},
            {"label_WristItems", "Wrist Items"},
            {"label_Results", "Results"},
            {"label_TooManyResults_0", "More than"},
            {"label_TooManyResults_1", "results - change the Search Result Limit or try and be more specific!"},
            {"label_NumberOfActiveBuffs", "Number Of Active Buffs"},
            {"label_CurrentScene", "Current Scene"},
            {"label_BuffName", "Buff Name"},
            {"label_BuffObjectName", "Buff Object Name"},
            {"label_BuffBlueprintName", "Buff Blueprint Name"},
            {"label_BuffBlueprintAssetGuid", "Buff Blueprint AssetGuid"},
            {"label_BuffDescription", "Buff Description"},
            {"label_Remove", "Remove"},
            {"label_RemoveAll", "Remove All"},
            {"label_SearchBy", "Search by"},
            {"label_BuffComponents", "Buff Components"},
            {"label_MilesTravelled", "Miles Travelled"},
            {"label_NextEncounterRollAt", "Next Encounter Roll At"},
            {"label_TeleportKey", "Teleport Key"},
            {"label_CurrentValue", "Current Value"},
            {
                "label_MilesUntilNextEncounterInfo",
                "This value is added to the current miles travelled in order to determine the next encounter roll."
            },
            {
                "label_BuildingTimeInfo",
                "Use negative values for a build time reduction - only applied to newly started constructions!"
            },
            {"label_CurrentBuildingTimeModifier", "Current Building Time Modifier"},
            {"label_CurrentBuildPoints", "Current Build Points"},
            {"label_CurrentBaseBuildPointIncome", "Current Base Build Point Income"},
            {"label_AlwaysRoll20ForIntiativeInfo", "affects friends and foes!"},
            {"label_CameraTurnRate", "Camera Turn Rate"},
            {"label_CurrentRotation", "Current Rotation"},
            {"label_CurrentDefaultRotation", "Current Default Rotation"},
            {"label_RotateLeftKey", "Rotate Left Key"},
            {"label_RotateRightKey", "Rotate Right Key"},
            {"label_ResetRotationKey", "Reset Rotation Key"},
            {"label_CurrentLowerZoomLimit", "Current Lower Zoom Limit"},
            {"label_CurrentUpperZoomLimit", "Current Upper Zoom Limit"},
            {"label_GlobalMapZoomLevelRestartButtonChange", "Global Map Zoom Level (after restart)"},
            {
                "label_GlobalMapZoomLevelRestart",
                "You have to restart the game for the changes to the Global Map Zoom Level to apply!"
            },
            {"label_FocusCameraKey", "Focus Camera Key"},
            {"label_CycleFocusKey", "Cycle Focus Key"},
            {"label_CurrentFocus", "Current Focus"},
            {"label_CurrentGameLocale", "Current Game Locale"},
            {"label_CurrentGameID", "Current Game ID"},
            {"label_Teleport", "Teleport"},
            {"label_RotateCameraLeft", "Rotate Camera Left"},
            {"label_RotateCameraRight", "Rotate Camera Right"},
            {"label_ResetCameraRotation", "Reset Camera Rotation"},
            {"label_FocusCamera", "Focus Camera"},
            {"label_CycleFocus", "Cycle Focus"},
            {"label_TogglePartyAlwaysRolls20", "Toggle Party Always Rolls 20"},
            {"label_CurrentSearchResultLimit", "Current Search Result Limit"},
            {"label_CurrentFlyingHeight", "Current Flying Height"},
            {"label_CurrentMinimalUnitSpeed", "Current Minimal Unit Speed"},
            {"label_CurrentMinimalWeaponRange", "Current Minimal Weapon Range"},
            {"label_CurrentStealthDCIncrement", "Current Stealth DC Increment"},
            {"label_ResetCutsceneLock", "Best used together with Focus Camera"},
            {"label_ResetCutsceneLockKey", "Reset Cutscene Lock Key"},
            {"label_CurrentXP", "Current XP"},
            {"label_CurrentLevel", "Current Level"},
            {"label_CurrentSize", "Current Size"},
            {
                "label_CraftMagicItemsSearchItemInfo",
                "Custom items from the Craft Magic Items mod have to be crafted at least once before they show up in the search!"
            },
            {"label_ActionKey", "Action Key"},
            {"label_ActionKeyEnableExperimental", "Enable Experimental Mode"},
            {"label_FeatDescription", "Feat Description"},
            {"label_FeatName", "Feat Name"},
            {"label_FeatObjectName", "Feat Object Name"},
            {"label_FeatBlueprintAssetGuid", "Feat Blueprint AssetGuid"},
            {"label_FeatRank", "Feat Rank"},
            {"label_SelectLanguage", "Please select your language"},
            {"label_ShowSpoilers", "Show Spoilers"},
            {"label_RomanceCounter", "Romance Counter"},
            {"label_CounterValue", "Counter Value"},
            {"label_LowerCutoff", "Lower Cutoff"},
            {"label_UpperCutoff", "Upper Cutoff"},
            {"label_ShowAllWeaponCategories", "Show All Weapon Categories"},
            {"label_ParametrizedFeats", "Parametrized Feats"},
            {
                "label_SearchInfoParametrizedFeats",
                "If you can't find a feat through the search try the Parametrized Feats option."
            },
            {"label_BlueprintName", "Blueprint Name"},
            {"label_UnitName", "Unit Name"},
            {"label_WeaponName", "Weapon Name"},
            {"label_WeaponDamage", "Weapon Damage"},
            {"label_ChallengeRating", "Challenge Rating"},
            {"label_EditingInExperimental", "The values can be edited in the Experimental sub-menu."},
            {"label_KingdomName", "Kingdom Name"},
            {"label_ArtisanMasterPieceChance", "Artisan Masterpiece Chance"},
            {"label_WearinessPerAttempt", "Hours Worth Of Weariness Per Attempt"},
            {"label_TeleportUnit", "Teleport Unit"},
            {"label_LockPicks", "Lock Picks"},
            {"label_CurrentRegionCR", "Current Region's CR"},
            {"label_UnityModManagerButtonIndex", "Unity Mod Manger Button Position"},
            {"label_AbilityDescription", "Ability Description"},
            {"label_AbilityName", "Ability Name"},
            {"label_AbilityObjectName", "Ability Object Name"},
            {"label_AbilityBlueprintAssetGuid", "Ability Blueprint AssetGuid"},
            {"label_CustomName", "Custom Name"},
            {"label_CurrentCustomName", "Current Custom Name"},
            {"label_CurrentCustomGender", "Current Custom Gender"},
            {"label_CurrentDefaultGender", "Current Default Gender"},
            {"label_BlueprintCharacterName", "Blueprint Character Name"},
            {"label_Spellbooks", "Spellbooks"},
            {"label_SpellListLevel", "Spell List Level"},
            {"label_SpellLevel", "Spell Level"},
            {"label_CurrentHitPoints", "Current Hit Points"},
            {"label_AdvanceGameTime", "Advance Game Time"},
            {"label_UI", "User Interface"},
            {
                "label_NoAttacksOfOpportunity",
                "Attacks of opportunity <b>targeting</b> the selection will be prevented."
            },
            {
                "label_SmartConsoleInfo",
                "Type <b>'help'</b> into the Smart Console to get a list of all console commands."
            },
            {"label_Weather", "Weather"},
            {"label_Intensity", "Intensity"},
            {
                "label_ItemModdingInfo",
                "JSON files named after the AssetGUID of the item you wish to edit have to be placed inside of the ModifiedBlueprints folder (e.g. 'efa6c2ee9e630384188a50b1ce6600fe.json'.\nEither remove values you don't want to change or set them to null.\nExample files for each supported category are included in the ModifiedBlueprints folder.\nThe AsserGUIDs can be acquired from spacehamster's JSON Blueprint Dump on github or you can directly edit most values through the item search or this menu.\n<b>Adding enchantments of the wrong type might cause display issues in the inventory!</b>"
            },
            {"label_CustomGender", "Custom Gender"},
            {"label_CurrentName", "Current Name"},
            {"label_CampingSpecialAbility", "Camping Special Ability"},
            {
                "label_SummonDurationMultiplier",
                "The duration of creatures summoned by the selection will be multiplied by the selected value."
            },
            {
                "label_BuffDurationMultiplier",
                "The duration of buffs cast by the selection will be multiplied by the selected value."
            },
            {"label_SetSummonLevelTo20", "Creatures summoned by the selection will always be level 20."},
            {"label_CrisisPoints", "Crisis Points"},
            {"label_CurrentCrisisPoints", "Current Crisis Points"},
            {"label_CombatDifficulty", "combat difficulty"},
            //label end 

            //message start
            {"message_NotInGame", "Load into the game!"},
            {"message_ExitCombat", "Exit Combat!"},
            {"message_NoEnemies", "There are no revealed enemies!"},
            {"message_NoKingdom", "No kingdom found!"},
            {"message_NoFavourites", "No favourites found!"},
            {"message_NoValidGuids", "Enter valid Guids!"},
            {"message_NoValidGuid", "Enter a valid Guid!"},
            {"message_NoResult", "No result!"},
            {"message_NoUnitSelected", "No unit selected!"},
            {"message_NoValidEntriesFound", "No valid entries found."},
            {"message_GameLoading", "Please wait while the game loads."},
            {"message_NoLockPicksLeft", "No lock picks left!"},
            {"message_LockPickSaved", "Lock pick saved!"},
            {"message_LockPickLost", "Lock pick lost!"},
            {"message_NoUnitFound", "No unit found - try a different setting!"},
            {"message_RenamedTo", "renamed to"},
            {"message_NoSpellbooksFound", "No spellbooks found!"},
            {"message_NoSpellsFound", "No spells found!"},
            {
                "message_AddAbilitiesAsSpells",
                "The abilities will be added to the current spellbook at the selected spell level."
            },
            {"message_RecreateUnitDescriptor", "Requires you to reload the area (e.g. leave and enter again)."},
            {"message_NoModItems", "No modified blueprints found!"},
            //message end

            //logMessage start
            {"logMessage_Received", "Received:"},
            {"logMessage_Removed", "Removed:"},
            {"logMessage_Gold", "Gold"},
            {"logMessage_Enabled", "Enabled"},
            {"logMessage_Disabled", "Disabled"},
            // { "logMessage_partyAlwaysRolls20", fallback["buttonToggle_partyAlwaysRolls20"]}, 
            // use buttonToggle_partyAlwaysRolls20
            //logMessage end

            //headerOption start
            {"headerOption_MaxAbilityScoreAtCC", "Maximal Base Ability Score At Character Creation"},
            {"headerOption_MinAbilityScoreAtCC", "Minimal Base Ability Score At Character Creation"},
            {"headerOption_AbilityPointsAtCCPlayer", "Ability Points At Character Creation (Player [Default: 25])"},
            {"headerOption_AbilityPointsAtCCmerc", "Ability Points At Character Creation (Mercenary [Default: 20])"},
            {"headerOption_AddCasterLevel", "Set Caster Level (Default: 1)"},
            {"headerOption_NoRationsRequired", "No Rations Required"},
            {"headerOption_MercenaryCostMultiplier", "Mercenary Cost Multiplier"},
            {"headerOption_ShowFavourites", "Show Favourites"},
            {"headerOption_ShowItemSets", "Show Item Sets"},
            {"headerOption_TravelSpeedMultiplier", "Travel Time Scale Multiplier"},
            {"headerOption_SettingsValue", "Settings Value"},
            {"headerOption_ChanceOnGlobalMap", "Chance On Global Map"},
            {"headerOption_ChanceWhenCamping", "Chance When Camping"},
            {"headerOption_ChanceWhenCampingAfterAttackDuringRest", "Chance When Camping After Attack During Rest"},
            {"headerOption_HardEncounterChance", "Hard Encounter Chance"},
            {"headerOption_HardEncounterMaxChance", "Hard Encounter Maximal Chance"},
            {"headerOption_HardEncounterChanceIncreaseIncrements", "Hard Encounter Chance Increase Increments"},
            {"headerOption_StalkerAmbushChances", "Stalker Ambush Chance"},
            {"headerOption_MilesUntilNextEncounterRoll", "Miles Until Next Encounter Roll"},
            {"headerOption_SafeMilesAfterEncounter", "Safe Miles After Encounter"},
            {"headerOption_SafeZoneSize", "Safe Zone Size"},
            {"headerOption_EncounterPawnOffset", "Encounter Pawn Offset"},
            {"headerOption_EncounterPawnDistanceFromLocation", "Encounter Pawn Distance From Location"},
            {"headerOption_AvoidanceFailMargin", "Avoidance Fail Margin"},
            {"headerOption_MinimalChallengeRatingBonus", "Minimal Challenge Rating Bonus"},
            {"headerOption_MaximalChallengeRatingBonus", "Maximal Challenge Rating Bonus"},
            {"headerOption_HardEncounterChallengeRatingBonus", "Hard Encounter Challenge Rating Bonus"},
            {"headerOption_AdjustBy", "Adjust By"},
            {"headerOption_OverrideKingdomAlignmentResult", "Override Kingdom Alignment Result"},
            {"headerOption_Community", "Community"}, //kingdomStats start
            {"headerOption_Loyalty", "Loyalty"},
            {"headerOption_Military", "Military"},
            {"headerOption_Economy", "Economy"},
            {"headerOption_Relations", "Relations"},
            {"headerOption_Divine", "Divine"},
            {"headerOption_Arcane", "Arcane"},
            {"headerOption_Stability", "Stability"},
            {"headerOption_Culture", "Culture"},
            {"headerOption_Espionage", "Espionage"}, //kingdomStats end
            {"headerOption_CameraScrollSpeed", "Camera Scroll Speed"},
            {"headerOption_SearchResultLimit", "Search Result Limit"},
            {"headerOption_FlyingHeight", "Flying Height"},
            {"headerOption_UseSlider", "Use Slider"},
            {"headerOption_VendorSellMultiplier", "Vendor Sell Multiplier"},
            {"headerOption_CrittersInsteadOfBloodChance", "Critters Instead Of Blood Chance"},
            //headerOption end

            //toggle start
            {"toggle_AddItemIdentified", "Add Items Identified"},
            {"toggle_AddMultipleItems", "Add Multiple Items"},
            {"toggle_SearchCsvForCustomItemSets", "Search .csv files for Custom Item sets"},
            {"toggle_SearchTxtForCustomItemSets", "Search .txt files for Custom Item sets"},
            {"toggle_RememberLastEnteredGuids", "Remember last entered Guid(s)"},
            {"toggle_RememberOpenCategories", "Remember open categories"},
            {"toggle_RememberOpenSubMenus", "Remember open sub-menus and search settings"},
            {"toggle_CreateCharStatsBackup", "Create backup before importing character statistics"},
            {"toggle_ShowDebugInfo", "Show debug information"},
            {"toggle_UpgradeSettlementsAndBuildings", "Upgrade Settlements & Buildings"},
            {"toggle_ShowKnownSpells", "Show Known Spells"},
            {"toggle_ShowAllLearnableSpells", "Show All Learnable Spells"},
            {"toggle_AddAbilitiesAsSpells", "Add Abilities/Spells From Favourites As Spells"},
            {"toggle_AddSpellsbooks", "Add Spellbooks"},
            {"toggle_MoveFavourites", "Rearrange Favourite Functions"},
            {"toggle_ShowInventory", "Show Inventory"},
            {"toggle_SpawnRandomEnemy", "Spawn Random Hostile Unit"},
            {"toggle_ShowModifiedItems", "Show Modified Blueprints"},
            {"toggle_ShowItemTypes", "Show & Edit Item Type Blueprints"},
            //toggles end

            //button start
            {"button_PartyExperience", "Party Experience"},
            //{ "button_Experience", "Experience" }, use header_Experience
            {"button_Add6Rations", "Add 6 camping supplies and rations"},
            {"button_Money", "Money"},
            {"button_SetAlignment", "Set Alignment To"},
            {"button_AlignmentCantBeSetTo", "Alignment Can't Be Set To"},
            {"button_AlignmentUseShiftInstead", "Use Shift Instead!"},
            {"button_ShiftAlignmentTowards", "Shift Alignment Towards"},
            {"button_Apply", "Apply"},
            {"button_SetTo", "Set To"},
            {"button_SetRankTo", "Set Rank To"},
            {"button_SetTo0", "Set To 0"},
            {"button_SetAllTo0", "Set All To 0"},
            {"button_PressKey", "Press Key"},
            {"button_ExportCharInfo", "Export Character Information (.txt)"},
            {"button_ImportStatsFrom", "Import Statistics From"},
            {"button_Kill", "Kill"},
            {"button_Panic", "Panic"},
            {"button_Freeze", "Freeze"},
            {"button_MakeCower", "Make Cower"},
            {"button_SetOnFire", "Set On Fire"},
            {"button_RevealFromStealth", "Reveal From Stealth"},
            {"button_KillAll", "Kill All"},
            {"button_PanicAll", "Panic All"},
            {"button_FreezeAll", "Freeze All"},
            {"button_MakeAllCower", "Make All Cower"},
            {"button_SetAllOnFire", "Set All On Fire"},
            {"button_RevealAllFromStealth", "Reveal All From Stealth"},
            {"button_AddGuid", "Add Guid"},
            {"button_SetGuid", "Set Guid"},
            {"button_Receive", "Receive"},
            {"button_ExportItemInfo", "Export Item Information (.txt)"},
            {"button_AddFavouritesToInventory", "Add Favourites To Inventory"},
            {"button_ExportFavouritesGuids", "Export Favourites' Guids"},
            {"button_ExportFavouritesNames", "Export Favourites' Names"},
            {"button_AddAllArmour", "Add All Armour"},
            {"button_AddAllBelts", "Add All Belts"},
            {"button_AddAllFootwear", "Add All Footwear"},
            {"button_AddAllGloves", "Add All Gloves"},
            {"button_AddAllHandSimple", "Add All Hand Simple (First World Lantern)"},
            {"button_AddAllHeadwear", "Add All Headwear"},
            {"button_AddAllKeys", "Add All Keys"},
            {"button_AddAllMiscellaneousItems", "Add All Miscellaneous Items"},
            {"button_AddAllArtifactsAndRelics", "Add All Artifacts And Relics"},
            {"button_AddAllNeckItems", "Add All Neck Items"},
            {"button_AddAllNotes", "Add All Notes"},
            {"button_AddAllRings", "Add All Rings"},
            {"button_AddAllShields", "Add All Shields"},
            {"button_AddAllShoulderItems", "Add All Shoulder Items"},
            {"button_AddAllUsableItems", "Add All Usable Items"},
            {"button_AddAllWeapons", "Add All Weapons"},
            {"button_AddAllWristItems", "Add All Wrist Items"},
            {"button_CustomSets", "Custom Sets"},
            {"button_LoadRefresh", "Load / Refresh"},
            {"button_LoadRefreshAvailableLanguages", "Load / Refresh Available Languages"},
            {"button_Filters", "Filters"},
            {"button_ResetFilters", "Reset Filters"},
            {"button_AddCurrentSearchResults", "Add Current Search Results"},
            {"button_ExportCurrentSearchResultGuids", "Export Current Search Result Guids"},
            {"button_ExportCurrentSearchResultNames", "Export Current Search Result Names"},
            {"button_RemoveAllBuffs", "Remove All Buffs"},
            {"button_RemoveAllNegativeEffects", "Remove All Negative Effects"},
            {"button_AllBuffs", "All Buffs"},
            {"button_AddToParty", "Add To Party"},
            {"button_AddFavouritesTo", "Add Favourites To"},
            {"button_Default", "Default"},
            {"button_BaseBPIncome", "Base BP Income"},
            {"button_SetLowerZoomLimit", "Set Lower Zoom Limit"},
            {"button_SetUpperZoomLimit", "Set Upper Zoom Limit"},
            {"button_SetGlobalMapZoomLevel", "Set Global Map Zoom Level"},
            {"button_CreateGameHistoryLog", "Create Game History Log"},
            {"button_DumpKingdomState", "Dump Kingdom State"},
            {"button_FeatSelectionMultiplier", "Feat Selection Multiplier"},
            {"button_SetToDefault", "Set To Default"},
            {"button_SorcererSupreme", "Sorcerer Supreme (mix and match spells)"},
            {"button_ResetToDefault", "Reset To Default"},
            {"button_MinimalUnitSpeed", "Minimal Unit Speed"},
            {"button_MinimalWeaponRange", "Minimal Weapon Range"},
            {"button_StealthDCIncrement", "Stealth DC Increment"},
            {"button_InstantRest", "Instant Rest"},
            {"button_RestoreSpellsAbilites", "Restore Spells & Abilities"},
            {"button_ResetCutsceneLock", "Reset Cutscene Lock"},
            {"button_ReadyToLevel", "Ready To Level Up!"},
            {"button_AddAllArmourAndWeaponProficiencies", "Add All Armour And Weapon Proficiencies To"},
            {"button_SetSizeTo", "Set Size To"},
            {"button_SetToOriginalSize", "Set To Original Size"},
            {"button_CreateFallbackXML", "Create Fallback XML"},
            {"button_TeleportPartyToPlayer", "Teleport Party To Player"},
            {"button_ChangePartyMembers", "Change Party Members (Only On Local Maps)"},
            {"button_ReloadArea", "Reload Area"},
            {"button_TeleportEveryoneToPlayer", "Teleport Everyone To Player"},
            {"button_PostPartyChange", "Activate New Party Members And Teleport To Player"},
            {"button_Preview", "Preview"},
            {"button_AddToFavourites", "Add To Favourites"},
            {"button_RemoveFromFavourites", "Remove From Favourites"},
            {"button_LevelUp", "Level Up"},
            {"button_Downgrade", "Downgrade"},
            {"button_MakeClassSkills", "Make Class Skill"},
            {"button_Confirm", "Confirm"},
            {"button_AllFeats", "All Feats"},
            {"button_ResetCharacterLevel", "Reset Character Level"},
            {"button_SetCharacterLevel", "Set Character Level To"},
            {"button_RestoreSpellsAbilitiesAfterCombat", "Restore Spells & Abilities After Combat"},
            {"button_RestoreItemChargesAfterCombat", "Restore Item Charges After Combat"},
            {"button_Upgrade", "Upgrade"},
            {"button_LearnAllCookingRecipes", "Learn All Cooking Recipes"},
            {"button_AllAbilities", "All Abilities"},
            {"button_SetPartysCurrentHitPointsTo", "Set Party's Current Hit Points To"},
            {"button_SetPrimaryHandAttacksTo", "Set Extra Primary Hand Attacks To"},
            {"button_SetSecondaryHandAttacksTo", "Set Extra Secondary Hand Attacks To"},
            {"button_SetSpeedOnSummon", "Set Creatures's Speed When Summoned To"},
            {"button_AdvanceGameTimeBy", "Advance Game Time By"},
            {"button_RemoveSpellbook", "Remove Spellbook"},
            {"button_AddCasterLevelTo", "Add Caster Level To"},
            {"button_RevealAllMapLocations", "All Map Locations"},
            {"button_RevealAllReachableMapLocations", "All Reachable Map Locations"},
            {"button_RevealAllMapLocationsXWaypointHidden", "All Map Locations Except Waypoints And Hidden Locations"},
            {"button_RevealAllRegularLocations", "All Regular Locations"},
            {"button_RevealAllLandmarks", "All Landmarks"},
            {"button_RevealAllWaypoints", "All Waypoints"},
            {"button_RevealAllHiddenLocations", "All Hidden Locations"},
            {"button_RevealAllSystemWaypoints", "All System Waypoints"},
            {"button_ChangeWeather", "Change Weather"},
            {"button_RemoveItemModification", "Remove Item Modification"},
            {"button_PatchManually", "Patch Manually"},
            {"button_RestoreAllItemCharges", "Restore All Item Charges"},
            {"button_RemoveEquippedItems", "Remove All Equipped Items"},
            //button end

            //buttonToggle start
            {"buttonToggle_PartyAlwaysRolls20", "Party Always Rolls 20"},
            {"buttonToggle_InfiniteSkillPoints", "Infinite Skill Points"},
            {"buttonToggle_IgnoreClassAlignment", "Ignore Class Alignment Restrictions"},
            {"buttonToggle_ExperienceMultiplier", "Experience Multiplier"},
            {"buttonToggle_UnlimitedItemCharges", "Unlimited Item Charges"},
            {"buttonToggle_IgnoreEquipmentRestrictions", "Ignore Equipment Restrictions"},
            {"buttonToggle_ShowEnemies", "Show Enemies"},
            {"buttonToggle_NoArcaneSpellFailure", "No Arcane Spell Failure"},
            {"buttonToggle_UnlimitedCasting", "Unlimited Casting"},
            {"buttonToggle_MetamagicZero", "Set Default Metamagic Cost To Zero"},
            {"buttonToggle_IgnoreMaterialComponents", "Ignore Material Components"},
            {"buttonToggle_SpontaneousCastersCopyScrolls", "Spontaneous Casters Copy Scrolls"},
            {"buttonToggle_UnlimitedAbilities", "Unlimited Abilities"},
            {"buttonToggle_MostlyUndetectableDuringStealth", "Mostly Undetectable During Stealth"},
            {"buttonToggle_NoFriendlyFireAOE", "No Friendly Fire AOE"},
            {"buttonToggle_NoFriendlyFireAny", "No Friendly Fire (Any)"},
            {"buttonToggle_NoCooldown", "No Cooldown"},
            {"buttonToggle_SetPartySpeedToFastestMember", "Set Party Speed To Fastest Member"},
            {"buttonToggle_PartyMovementSpeedMultiplier", "Party Movement Speed Multiplier"},
            {"buttonToggle_RevealMapLocations", "Reveal Map Locations"},
            {"buttonToggle_EnableTeleport", "Enable Teleport"},
            {"buttonToggle_RandomEncounterPossible", "Random Encounters Possible"},
            {"buttonToggle_AlwaysSucceedAtAvoidingEncounters", "Always Succeed At Avoiding Encounters"},
            {"buttonToggle_InstantKingdomEvents", "Instant Kingdom Events"},
            {"buttonToggle_ForceSuccessTriumphs", "Force Success/Triumph"},
            {"buttonToggle_IgnoreBuildingRestrictions", "Ignore Building Restrictions"},
            {"buttonToggle_EnableHotkey", "Enable Hotkey"},
            {"buttonToggle_EnemiesAlwaysRoll1", "Enemies Always Roll 1"},
            {"buttonToggle_AlwaysRoll20ForInitiative", "Always Roll 20 For Initiative"},
            {"buttonToggle_CameraRotation", "Camera Rotation"},
            {"buttonToggle_CameraScrollSpeed", "Camera Scroll Speed"},
            {"buttonToggle_CameraZoomUnlocked", "Camera Zoom Unlocked"},
            {"buttonToggle_EnableFocusCamera", "Enable Focus Camera"},
            {"buttonToggle_DisableCameraBounds", "Disable Camera Bounds"},
            {"buttonToggle_MoveCameraToPlayer", "Move Camera To Player"},
            {"buttonToggle_IgnoreDialogueRestrictions", "Ignore Dialogue Restrictions"},
            {"buttonToggle_ReverseCasterAlignmentChecks", "Reverse Caster Alignment Checks"},
            {"buttonToggle_PreventAlignmentChanges", "Prevent Alignment Changes"},
            {"buttonToggle_NoInactiveCampsite", "Don't Leave An Inactive Campsite"},
            {"buttonToggle_EnableDLCContent", "Enable DLC Content (only if it is available on your system!)"},
            {"buttonToggle_FogOfWar", "Fog Of War (Visuals Only)"},
            {"buttonToggle_FadeToBlack", "Fade To Black"},
            {"buttonToggle_ActiveSceneName", "Active Scene Name"},
            {"buttonToggle_LogMessages", "Log Messages"},
            {"buttonToggle_BoTScrollbar", "Scrollbar"},
            {"buttonToggle_FogOfWarFull", "Fog Of War"},
            {"buttonToggle_OverwriteFogOfWar", "Overwrite Fog Of War Settings"},
            {"buttonToggle_SkipSpellSelection", "Skip Spell Selection"},
            {"buttonToggle_VendorsSellFor0", "Sell For 0 To Vendors"},
            {"buttonToggle_VendorsBuyFor0", "Buy For 0 From Vendors"},
            {"buttonToggle_IgnoreForbiddenFeatures", "Ignore Forbidden Feats When Choosing A Class"},
            {"buttonToggle_SearchOnlyCraftMagicItems", "Search Only For Items Created By Craft Magic Items"},
            {"buttonToggle_EveryoneExceptPlayerFactionRolls1", "Everyone Except The Player Faction Always Rolls 1"},
            {"buttonToggle_experimentalIUnderstand", "I Understand"},
            {"buttonToggle_LogInfoToFile", "Log Info To File"},
            {"buttonToggle_ShowOnlyClassSkills", "Show Only Class Skills"},
            {"buttonToggle_ShowTooltips", "Show Tooltips"},
            {"buttonToggle_NoLevelUpRestirctions", "Always Be Able To Level Up"},
            {"buttonToggle_NeverRoll1", "Never Roll 1"},
            {"buttonToggle_NeverRoll20", "Never Roll 20"},
            {"buttonToggle_InstantRestAfterCombat", "Instant Rest After Combat"},
            {"buttonToggle_FullHitdiceEachLevel", "Add Full Hit Die Value On Level Up"},
            {"buttonToggle_RollHitDiceEachLevel", "Roll Hit Die Value On Level Up"},
            {"buttonToggle_RollWithAdvantage", "Roll With Advantage"},
            {"buttonToggle_RollWithDisadvantage", "Roll With Disadvantage"},
            {"buttonToggle_IgnorenFeaturePrerequisites", "Ignore Feat Prerequisites When Choosing A Class"},
            {"buttonToggle_IgnorenFeatureListPrerequisites", "Ignore Feat Prerequisites (List) When Choosing A Class"},
            {"buttonToggle_FeaturesIgnorePrerequisites", "Ignore Prerequisites When Choosing Feats"},
            {"buttonToggle_ShowUnitInfoBox", "Show Unit Info Box"},
            {"buttonToggle_StopGameTime", "Stop Passing Of Time"},
            {"buttonToggle_NoDamageFromEnemies", "Enemies Don't Deal Damage To The Party"},
            {"buttonToggle_PartyOneHitKills", "Party Members Kill Enemies In One Hit"},
            {"buttonToggle_AllDoorContainersUnlocked", "Unlock All Containers & Doors"},
            {"buttonToggle_RepeatableLockPicking", "Unlimited Lock Picking Attempts"},
            {"buttonToggle_RepeatableLockPickingWeariness", "Lock Picking Causes Weariness"},
            {"buttonToggle_NoIngredientsRequired", "Ignore Cooking Ingredient Requirements"},
            {"buttonToggle_RepeatableLockPickingLockPicks", "Use Lock Picks (Proof Of Concept!)"},
            {"buttonToggle_UnityModManagerButton", "Add Unity Mod Manager Button To ESC Menu"},
            {"buttonToggle_DamageDealtMultipliers", "Damage Dealt Multipliers"},
            {"buttonToggle_PartyDamageDealtMultiplier", "Damage Dealt By Party Multiplier"},
            {"buttonToggle_EnemiesDamageDealtMultiplier", "Damage Dealt By Enemies Multipliers"},
            {"buttonToggle_ScaleInventoryMoney", "Scale Money Display (Inventory Screen)"},
            {"buttonToggle_RepeatableLockPickingLockPicksInventoryText", "Show Lock Picks In Inventory"},
            {"buttonToggle_ShowClassDataOptions", "Class Data Options (Set Level)"},
            {"buttonToggle_NoBurnKineticist", "No Burn (Kineticist)"},
            {"buttonToggle_MaximiseAcceptedBurnKineticist", "Maximise Accepted Burn (Kineticist)"},
            {"buttonToggle_NoTempHPKineticist", "No Temporary Hit Point Reduction By Burn (Kineticist)"},
            {"buttonToggle_MainCharacterAlwaysRolls20", "Main Character Always Rolls 20"},
            {"buttonToggle_ShowPetInventory", "Show Pet Inventory"},
            {"buttonToggle_AnimationCloseUMM", "Close Unity Mod Manger Interface When Animations Plays"},
            {"buttonToggle_InstantCooldownMainChar", "Main Character Has No Cooldown"},
            {"buttonToggle_DexBonusLimit99", "Set Maximal Dexterity Bonus Limit To 99"},
            {"buttonToggle_PassSkillChecksIndividual", "Always Pass Skill Checks"},
            {"buttonToggle_PassSkillChecksIndividualDC99", "Set DC To -99"},
            {"buttonToggle_ItemsWeighZero", "No Item Weight"},
            {"buttonToggle_ExtraAttacksParty", "Extra Attacks Per Round"},
            {"buttonToggle_AlignmentFix", "Neutral Alignment Shifts Don't Cause Movement On The Law/Chaos Axis"},
            {"buttonToggle_SpellbookAbilityAlignmentChecks", "Ignore Spellbook And Ability Alignment Checks"},
            {"buttonToggle_SetSpeedOnSummon", "Set Creature's Speed When Summoned"},
            {"buttonToggle_EnableFocusCameraSelectedUnit", "Enable Focus On Selected Portrait"},
            {"buttonToggle_ClaimResources", "Claim All Revealed Resources In Your Lands"},
            {"buttonToggle_NoResourcesClaimCost", "No Resource Claim Cost"},
            {"buttonToggle_FreezeTimedQuestAt90Days", "Freeze Timed Quest At 90 Days Left"},
            {"buttonToggle_PreventQuestFailure", "Prevent Quest Failure"},
            {"buttonToggle_AlwaysSucceedCastingDefensively", "Never Fail Casting Defensively"},
            {"buttonToggle_AlwaysSucceedConcentration", "Never Fail Concentration Checks"},
            {"buttonToggle_SortSpellbooksAlphabetically", "Sort Spellbooks Alphabetically (Spellbook Screen)"},
            {"buttonToggle_SortSpellsAlphabetically", "Sort Spells Alphabetically (Spellbook Screen)"},
            {"buttonToggle_AllHitsAreCritical", "All Attacks Are Critical Hits"},
            {"buttonToggle_PassSavingThrowIndividual", "Always Pass Saving Throws"},
            {"buttonToggle_NoAttacksOfOpportunity", "No Attacks Of Opportunity"},
            {"buttonToggle_CookingAndHuntingInDungeons", "Allow Cooking And Hunting In Dungeons"},
            {"buttonToggle_ArmourChecksPenalty0", "Set Armour Check Penalty To 0"},
            {"buttonToggle_SetEncumbrance", "Set Encumbrance"},
            {"buttonToggle_InstantPartyChange", "Instantly Swap Party Members"},
            {"buttonToggle_LogToUMM", "Log To Unity Mod Manager"},
            {"buttonToggle_EnableToggleHUD", "Enable Toggle HUD"},
            {"buttonToggle_DisplayObjectInfo", "Display Object Info"},
            {"buttonToggle_ToggleHUD", "Toggle HUD"},
            {"buttonToggle_NoNegativeLevels", "No Negative Levels"},
            {"buttonToggle_AutoEquipConsumables", "Automatically Refill Consumables In Belt Solts"},
            {"buttonToggle_IgnorePrerequisites", "Ignore Class And Feat Restrictions"},
            {"buttonToggle_IgnoreCasterTypeSpellLevel", "Ignore Caster Type And Spell Level Restrictions"},
            {"buttonToggle_IgnoreForbiddenArchetype", "Ignore Forbidden Archetypes"},
            {"buttonToggle_IgnorePrerequisiteStatValue", "Ignore Required Stat Values"},
            {"buttonToggle_SpiderBegone", "Spiders Begone!"},
            {"buttonToggle_ShowAreaName", "Show Area Name"},
            {"buttonToggle_SummonDurationMultiplier", "Summon Duration Multiplier"},
            {"buttonToggle_BuffDurationMultiplier", "Buff Duration Multiplier"},
            {"buttonToggle_SetSummonLevelTo20", "Set Summon Level To 20"},
            {"buttonToggle_UberLogger", "Enable UberLogger"},
            {"buttonToggle_UberLoggerForward", "Enable UberLogger Forwarding To Unity"},
            {"buttonToggle_UberLoggerForwardPrefix", "Add Prefix To UberLogger Logs"},
            {"buttonToggle_ExportToModFolder", "Export To Mod Folder"},
            {"buttonToggle_ShowCombatDifficulty", "Show Combat Difficulty"},
            { "buttonToggle_OutOfCombatOnly", "Out Of Combat Only"},
            //buttonToggle end

            //arrayItem start
            {"arrayItem_Alignments_NoChange", "No Change"},
            {"arrayItem_Alignments_ChaoticEvil", "Chaotic Evil"},
            {"arrayItem_Alignments_NeutralEvil", "Neutral Evil"},
            {"arrayItem_Alignments_LawfulEvil", "Lawful Evil"},
            {"arrayItem_Alignments_ChaoticNeutral", "Chaotic Neutral"},
            {"arrayItem_Alignments_TrueNeutral", "True Neutral"},
            {"arrayItem_Alignments_LawfulNeutral", "Lawful Neutral"},
            {"arrayItem_Alignments_ChaoticGood", "Chaotic Good"},
            {"arrayItem_Alignments_NeutralGood", "Neutral Good"},
            {"arrayItem_Alignments_LawfulGood", "Lawful Good"},
            {"arrayItem_Alignments_Good", "Good"},
            {"arrayItem_Alignments_Evil", "Evil"},
            {"arrayItem_Alignments_Lawful", "Lawful"},
            {"arrayItem_Alignments_Chaotic", "Chaotic"},

            {"arrayItem_takeX_TakeCustomValue", "Take Custom Value"},
            {"arrayItem_takeX_Take20", "Take 20"},
            {"arrayItem_takeX_Take10", "Take 10"},
            {"arrayItem_takeX_Off", "Off"},

            {"arrayItem_Size_Fine", "Fine"},
            {"arrayItem_Size_Diminutive", "Diminutive"},
            {"arrayItem_Size_Tiny", "Tiny"},
            {"arrayItem_Size_Small", "Small"},
            {"arrayItem_Size_Medium", "Medium"},
            {"arrayItem_Size_Large", "Large"},
            {"arrayItem_Size_Huge", "Huge"},
            {"arrayItem_Size_Gargantuan", "Gargantuan"},
            {"arrayItem_Size_Colossal", "Colossal"},

            {"arrayItem_ActionKeyMain_None", "None"},
            {"arrayItem_ActionKeyMain_GetInfo", "Get Info"},
            {"arrayItem_ActionKeyMain_ResurrectAndFullyRestore", "Resurrect And Fully Restore"},
            {"arrayItem_ActionKeyMain_BuffFromFavourites", "Buff From Favourites"},
            {"arrayItem_ActionKeyMain_EditStats", "Edit Stats"},
            {"arrayItem_ActionKeyMain_MakeControllable", "Make Controllable"},
            {"arrayItem_ActionKeyMain_SpawnCritters", "Spawn Critters"},
            {"arrayItem_ActionKeyMain_AddToParty", "Add To Party"},
            {"arrayItem_ActionKeyMain_SpawnEnemy", "Spawn Enemy"},
            {"arrayItem_ActionKeyMain_RotateUnit", "Rotate Unit"},
            {"arrayItem_ActionKeyMain_RecreateUnitDescriptor", "Recreate Unit Descriptor"},


            {"arrayItem_ActionKeyKill_Kill", "Kill"},
            {"arrayItem_ActionKeyKill_ForceKill", "Force Kill"},

            {"arrayItem_FeatParam_FencingGrace", "Fencing Grace"},
            {"arrayItem_FeatParam_ImprovedCritical", "Improved Critical"},
            {"arrayItem_FeatParam_SlashingGrace", "Slashing Grace"},
            {"arrayItem_FeatParam_SwordSaintChosenWeapon", "Sword Saint Chosen Weapon"},
            {"arrayItem_FeatParam_WeaponFocus", "Weapon Focus"},
            {"arrayItem_FeatParam_WeaponFocusGreater", "Greater Weapon Focus"},
            {"arrayItem_FeatParam_WeaponMastery", "Weapon Mastery"},
            {"arrayItem_FeatParam_WeaponSpecialization", "Weapon Specialization"},
            {"arrayItem_FeatParam_WeaponSpecializationGreater", "Greater Weapon Specialization"},
            {"arrayItem_FeatParam_SpellFocus", "Spell Focus"},
            {"arrayItem_FeatParam_GreaterSpellFocus", "Greater Spell Focus"},

            {"arrayItem_NeverRollX_Everyone", "Everyone"},
            {"arrayItem_NeverRollX_OnlyParty", "Only The Party"},
            {"arrayItem_NeverRollX_OnlyEnemies", "Only Enemies"},
            {"arrayItem_NeverRollX_OnlyMainChar", "Only The Main Character"},

            {"arrayItem_UnityEntityData_Party", "Active Party"},
            {"arrayItem_UnityEntityData_ControllableCharacters", "Controllable Characters"},
            {"arrayItem_UnityEntityData_ActiveCompanions", "Active Companions"},
            {"arrayItem_UnityEntityData_AllCharacters", "All Characters"},
            {"arrayItem_UnityEntityData_Mercenaries", "Mercenaries (Custom Companions)"},
            {"arrayItem_UnityEntityData_Pets", "Pets"},
            {"arrayItem_UnityEntityData_Enemies", "Enemies"},


            {"arrayItem_Weather_Normal", "Normal"},
            {"arrayItem_Weather_Rain", "Rain"},
            {"arrayItem_Weather_Snow", "Snow"},
            //arrayItem end

            //charStat start
            {"charStat_Strength", "Strength"},
            {"charStat_Dexterity", "Dexterity"},
            {"charStat_Constitution", "Constitution"},
            {"charStat_Intelligence", "Intelligence"},
            {"charStat_Wisdom", "Wisdom"},
            {"charStat_Charisma", "Charisma"},

            {"charStat_Athletics", "Athletics"},
            {"charStat_KnowledgeArcana", "Knowledge (Arcana)"},
            {"charStat_KnowledgeWorld", "Knowledge (World)"},
            {"charStat_LoreNature", "Lore (Nature)"},
            {"charStat_LoreReligion", "Lore (Religion)"},
            {"charStat_Mobility", "Mobility"},
            {"charStat_Perception", "Perception"},
            {"charStat_Persuasion", "Persuasion"},
            {"charStat_Stealth", "Stealth"},
            {"charStat_Thievery", "Trickery"},
            {"charStat_UseMagicDevice", "Use Magic Device"},

            {"charStat_Bluff", "Bluff"},
            {"charStat_Diplomacy", "Diplomacy"},
            {"charStat_Intimidate", "Intimidate"},

            {"charStat_Fortitude", "Fortitude"},
            {"charStat_Reflex", "Reflex"},
            {"charStat_Will", "Will"},

            {"charStat_Initiative", "Initiative"},
            {"charStat_BaseAttackBonus", "Base Attack Bonus"},
            {"charStat_AdditionalAttackBonus", "Additional Attack Bonus"},
            {"charStat_NumberOfAttacksOfOpportunity", "Number Of Attacks Of Opportunity"},
            {"charStat_SneakAttack", "Sneak Attack"},
            {"charStat_AdditionalDamage", "Additional Damage"},
            {"charStat_AdditionalCombatManeuverBonus", "Additional Combat Maneuver Bonus"},
            {"charStat_AdditionalCombatManeuverDefense", "Additional Combat Maneuver Defenses"},
            {"charStat_ArmourClass", "Armour Class"},
            {"charStat_Reach", "Reach"},
            {"charStat_HitPoints", "Hit Points"},
            //{ "charStat_TemporaryHitPoints", "Temporary Hit Points"},
            {"charStat_Speed", "Speed"},
            //charStat end

            //warning start
            {"warning_Buffs", "Make sure to save before adding or removing unfamiliar buffs!"},
            {"warning_Experimental", "Make sure to save and create backups before using any of these options!"},
            {"warning_Alignment", "Be careful when changing alignment!"},
            {
                "warning_TakeXAndPartyRoll20",
                "Both Party Always Rolls 20 and Take X are active! Party Always Rolls 20  will overwrite Take X's results!"
            },
            {"warning_InfiniteSkillPoints", "Has to be disabled after spending the skill points in order to progress"},
            {"warning_UnresposiveAddingItems", "The game might become unresponsive while adding the items!"},
            {"warning_EkunQ2", "This item causes errors and thus can't be added."},
            {
                "warning_SpellsPerDay",
                "Be aware that too high values can break the Spellbook UI of prepared spellcasters!"
            },
            {"warning_ZoomLimits_0", "is larger than the Upper Zoom Limit"},
            {"warning_ZoomLimits_1", "Your current Lower Zoom Limit"},
            {"warning_ZoomLimits_2", "As a result the scroll directions when zooming will be flipped!"},
            {"warning_FeatSelectionMultiplier_0", "Can cause UI issues."},
            {
                "warning_FeatSelectionMultiplier_1",
                "In order to fix these you have to edit the party.json and remove any entries in m_selectionsByLevel entries beyond the first."
            },
            {
                "warning_FeatSelectionMultiplier_2",
                "You can find an in depth guide on how to fix it on the mod page under Resources."
            },
            {
                "warning_SorcererSupreme_0",
                "Can prevent low level characters from progressing past the spell selection."
            },
            {"warning_SorcererSupreme_1", "You can use the button below to progress past it temporarily."},
            {
                "warning_IgnoreClassAlignment",
                "Does not disable the alignment restrictions for abilities and feats!\nUse <b>Ignore Spellbook And Ability Alignment Checks</b> to get around the ability/spellbook limitations (-> Spell & Ability Options)"
            },
            {"warning_CustomGender", "Changing gender causes visual glitches!"},
            {
                "warning_XPAndScaleXPActive",
                "It is recommended to keep Bag Of Tricks' XP Multiplier off while using ScaleXP!"
            },
            {
                "warning_ActionKeyExperimentalMode",
                "Experimental Mode includes features that can break the game and cause crashes!"
            },
            {"warning_GameVersion", "Game Version"},
            {"warning_Mod", "Mod"},
            {"warning_Feats_0", "Make sure to save before adding or removing unfamiliar feats!"},
            {"warning_Feats_1", "Adding too many weapon proficiencies may cause UI issues!"},
            {
                "warning_toggleNoLevelUpRestirctions",
                "A character level above 20 will cause the spell book UI to break.\nYou can use the Reset Character Level option to make it usable again."
            },
            {
                "warning_SetCharacterLevel",
                "Your experience points will be set to the lowest value for the chosen level!"
            },
            {"warning_SettingsXML_0", "If you are using the mod for the first time you can ignore this message."},
            {
                "warning_SettingsXML_1",
                "Since there were major changes to the Settings.xml\nit is highly recommended to the delete the Settings.xml file\nin your 'Pathfinder Kingmaker\\Mods\\BagOfTricks' folder before continuing to use the mod!"
            },
            {"warning_NeverRoll1AndPartyRoll20", "Party Always Rolls 20 is active either disable it or Never Roll 1."},
            {
                "warning_NeverRoll20AndPartyRoll20",
                "Party Always Rolls 20 is active either disable it or Never Roll 20."
            },
            {"warning_AlsoEnabled", "is also enabled!"},
            {"warning_CurrentlyNotReversible", "Currently not reversible!"},
            {"warning_CreatedForVersion_0", "This mod was created for game version"},
            {"warning_CreatedForVersion_1", "you are using game version"},
            {
                "warning_CreatedForVersion_2",
                "Save often and check the mod page and forum for information regarding compatibility!"
            },
            {"warning_CantripsRemoval", "Even when removed cantrips will still remain useable!"},
            {"warning_NewSpellsAdded", "In order to use newly added spells you either have to rest or use"},
            {"warning_NewSpellsAddedAlt", "In order to use newly added spells you might either have to rest or use"},
            {
                "warning_UpgradeSetllement",
                "Upgrading settlements through this menu won't count towards quest goals!\nA downgrade option is available in the experimental section (-> Misc)."
            },
            {
                "warning_RandomEncounterPossible",
                "Turning off random encounters completely will cause you to miss certain encounters required for quests and thus break them!"
            },
            {
                "warning_StopGameTime",
                "Can cause issues with summons, buffs and debuffs!\nTo pause the kingdom timeline without any of these issues use the Kingdom Resolution mod."
            },
            {
                "warning_AddSpellsbooks",
                "The in-game Spellbook Screen can't display more than five spellbooks!\nSpellbooks added through this menu aren't fully functional (e.g. no cantrips)!"
            },
            {
                "warning_TakeX",
                "Combat related rolls made before combat has started are still affected by TakeX (e.g. an attack on an unsuspecting enemy)."
            },
            {
                "warning_UnlimitedCasting",
                "Unlimited Casting is known to crash/freeze the game after the final fight of this scene.\nThus it is highly recommended to deactivate it temporarily!"
            },
            {
                "warning_NoDamageFromEnemies",
                "If you haven't cleared all combat encounters in this area is highly recommended to temporarily disable 'Enemies Don't Deal Damage To The Party' in this area!\nOtherwise a companion quest might break!"
            },
            {"warning_PreventAlignmentChanges", "Prevent Alignment Changes is active!"},
            //warning end

            //tooltip start
            {
                "tooltip_BuffDurationMultiplier",
                "The duration of buffs cast by the selection will be multiplied by the selected value.\nNote that extremely high multipliers might prevent it from being applied to very long running buffs.\nIf that is the case you will be able to see an error in the log (System.OverflowException)."
            },

            {
                "tooltip_SetEncumbrance",
                "Sets the encumbrance for individual characters and the party as a whole to the chosen value."
            },

            {
                "tooltip_MaxAbilityScoreAtCC",
                "Use the input field to set the maximal ability score at character creation."
            },
            {
                "tooltip_MinAbilityScoreAtCC",
                "Use the input field to set the minimal ability score at character creation."
            },
            {
                "tooltip_AbilityPointsAtCCPlayer",
                "Use the input field to set the ability points at character creation for the player."
            },
            {
                "tooltip_AbilityPointsAtCCmerc",
                "﻿Use the input field to set the ability points at character creation for mercenaries."
            },
            {
                "tooltip_FullHitdiceEachLevel",
                "Removes the halving of the hit die value by the game when adding hit point on level up."
            },
            {
                "tooltip_RollHitDiceEachLevel",
                "Instead of adding half the hit die value every level up the added value is determined by a roll."
            },
            {"tooltip_AddCasterLevel", "﻿Use the input field to set the caster level."},
            {
                "tooltip_NoLevelUpRestirctions",
                "You can level up even when you don't meet the experience requirements or are already level 20.\nThe UI indicators won't immediately update to show that a level up is available/unavailable."
            },
            {"tooltip_InfiniteSkillPoints", "﻿Gain infinite skill points."},
            {
                "tooltip_IgnorePrerequisites",
                "﻿Ignore restrictions when picking classes and feats.\nIf you pick a Prestige Class at character creation you will start without equipment\nand might lack important Weapon Proficiencies!"
            },
            {"tooltip_IgnoreCasterTypeSpellLevel", "﻿Ignore caster type and spell level restrictions."},
            {"tooltip_IgnoreForbiddenArchetype", "﻿Ignore forbidden Archetypes."},
            {"tooltip_IgnorePrerequisiteStatValue", "﻿Ignore required stat values."},
            {"tooltip_IgnoreClassAlignment", "﻿Ignore alignment restrictions for classes."},
            {
                "tooltip_IgnoreForbiddenFeatures",
                "﻿Ignore forbidden feats when choosing a class e.g. Atheism when trying to become a Cleric."
            },
            {
                "tooltip_IgnorenFeaturePrerequisites",
                "Ignore feat prerequisites when choosing a class (e.g. allows to become a Cleric of Groetus).\nIf this option doesn't work try Ignore Feat Prerequisites (List) When Choosing A Class"
            },
            {
                "tooltip_IgnorenFeatureListPrerequisites",
                "﻿Ignore feat prerequisites from lists when choosing a class.\nIf this option doesn't work try Ignore Feat Prerequisites When Choosing A Class"
            },
            {"tooltip_FeaturesIgnorePrerequisites", "﻿Ignore prerequisites when choosing feats."},
            {"tooltip_ExperienceMultiplier", "Enables a multiplier for all gained experience points."},

            {"tooltip_Multiplier", "Use the slider to set a multiplier."},
            {"tooltip_CustomMultiplier", "Allows to set a custom multiplier."},
            {"tooltip_LearnAllCookingRecipes", "Teaches all cooking recipes to the party."},
            {"tooltip_NoIngredientsRequired", "Disables cooking ingredient requirements."},
            {"tooltip_InstantRest", "Applies all the effects of a rest immediately to the party."},
            {"tooltip_InstantRestAfterCombat", "Applies all the effects of a rest automatically after every combat."},
            {"tooltip_CookingAndHuntingInDungeons", "Allows to hunt and cook during rests in dungeons."},
            {"tooltip_NoRationsRequired", "Disables to need for rations when resting."},
            {"tooltip_Add6Rations", "Adds six rations to the party's inventory."},

            {
                "tooltip_MercenaryCostMultiplier",
                "Use the input field to set a multiplier for the cost for recruiting a mercenary."
            },
            {"tooltip_VendorSellMultiplier", "Enables a multiplier for goods sold to vendors."},
            {"tooltip_VendorsSellFor0", "All items sold are worth 0."},
            {"tooltip_VendorsBuyFor0", "All items bought are worth 0."},

            {
                "tooltip_RepeatableLockPicking",
                "Locks can be picked repeatedly.\nRequired for Lock Picking Causes Weariness and Use Lock Pick"
            },
            {
                "tooltip_RepeatableLockPickingWeariness",
                "Each attempt at picking a lock causes weariness.\nBelow the value can be customized (1.0 = 1 hour of weariness per attempt)."
            },
            {"tooltip_1EqualsHourWeariness", "1.0 = 1 hour of weariness per attempt."},
            {
                "tooltip_RepeatableLockPickingLockPicks",
                "This is a simple proof of concept of a lock pick system.\nYour party has a limited set of lock picks based on their Trickery.\nDuring rests (currently only camping) the party members skilled in Trickery will create new lock picks. The amount is based on skill rolls.\nIf you use this feature please provide feedback!"
            },
            {
                "tooltip_RepeatableLockPickingLockPicksInventoryText",
                "Display the amount of lock picks in the inventory screen.\nRequires to load a save or change the area in order to take effect."
            },


            {"tooltip_CustomName", "Requires to load a save or change the area in order to fully take effect."},
            {"tooltip_CustomGender", "Allows you to set a switch a character's gender.\nCauses visual glitches!"},
            {
                "tooltip_ShowClassDataOptions",
                "Allows to change the class level independent of leveling and to add normally unavailable classes.\nSave often and use with caution!"
            },
            {"tooltip_RemoveEquippedItems", "﻿Removes all equipped items from the selected character."},
            {
                "tooltip_ResetCharacterLevel",
                "﻿Resets the character level to match your current experience points (at most level 20)."
            },
            {"tooltip_SetCharacterLevel", "﻿Sets the character level to the chosen value."},
            {
                "tooltip_SetAlignment",
                "Set the selected character's alignment to the one chosen from the selection grid.\n\"You've performed a [x] action.\" will be displayed irregardless of the unit affected."
            },
            {
                "tooltip_ShiftAlignmentTowards",
                "Shifts the selected character's alignment towards the one chosen from the selection grid by the set value."
            },
            {"tooltip_SetSize", "Set the selected character's size to the one chosen from the selection grid."},
            {"tooltip_SetToOriginalSize", "Revert the selected character back to their original size."},
            {"tooltip_Statistics", "Use the input field or the +/- buttons to alter the statistics."},
            {"tooltip_ShowOnlyClassSkills", "Switch between displaying all skills or only class skills."},
            {"tooltip_PartyMultipliers", "Use the slider to set a multiplier for the chosen party statistics."},
            {
                "tooltip_ExportCharInfo",
                "Creates a .txt file with all the selected character's statistics in the Characters folder.\nThis file can be edited and imported."
            },
            {"tooltip_ImportStatsFrom", "Imports all the statistics for the selected character from a .txt file."},

            {"tooltip_ItemsWeighZero", "The weight of all items is set to 0."},
            {"tooltip_DexBonusLimit99", "Sets the maximal Dexterity Bonus limit of all armours to 99."},
            {"tooltip_ArmourChecksPenalty0", "Sets the Armour Check Penalty of all armours to 0."},
            {"tooltip_UnlimitedItemCharges", "Usable items have unlimited charges."},
            {
                "tooltip_IgnoreEquipmentRestrictions",
                "Disables equipment restrictions (Alignment, Class, Statistics and Proficiencies)."
            },
            {"tooltip_AddItemIdentified", "Items added through this menu will be added in an identified state."},
            {"tooltip_AssetGuid", "If you don't know an item's Guid you can use the search function below."},
            {"tooltip_AddMultipleItems", "Allows you to insert multiple Guid separated by commas."},

            {"tooltip_ShowEnemies", "Display enemies instead of party members."},

            {
                "tooltip_ShowAllWeaponCategories",
                "﻿Enabling this options will allow you to add versions of feats the game doesn't normally include e.g. Fencing Grace (Greatclub)."
            },

            {
                "tooltip_RestoreSpellsAbilites",
                "Restore your party's spells and abilities﻿.\nDoes not affect Burn - use Instant Rest instead."
            },
            {
                "tooltip_SpellbookAbilityAlignmentChecks",
                "Disable alignment checks for spellbooks access and when using ability\ne.g. allowing you to use Paladin abilities while being Chaotic Evil."
            },
            {"tooltip_NoArcaneSpellFailure", "﻿Disables arcane spell failure for your party."},
            {"tooltip_AlwaysSucceedCastingDefensively", "﻿Prevents party members from failing casting defensively."},
            {"tooltip_AlwaysSucceedConcentration", "Prevents party members from failing concentration checks."},
            {"tooltip_UnlimitedCasting", "﻿Unlimited spell casts for you party."},
            {"tooltip_MetamagicZero", "Sets the default Metamagic cost to zero."},
            {"tooltip_IgnoreMaterialComponents", "Disables material component requirements."},
            {"tooltip_SpontaneousCastersCopyScrolls", "﻿Allows spontaneous casters to copy spells from scrolls."},
            {"tooltip_UnlimitedAbilities", "Unlimited ability uses for you party."},
            {
                "tooltip_RestoreSpellsAbilitiesAfterCombat",
                "All spells and abilities are automatically restored after every combat."
            },
            {"tooltip_RestoreItemChargesAfterCombat", "All items are automatically recharged after every combat."},
            {"tooltip_MostlyUndetectableDuringStealth", "﻿Remain (mostly) undetectable while in stealth."},
            {"tooltip_NoFriendlyFireAOE", "Disables friendly fire when using area off effect spells or abilities."},
            {"tooltip_NoFriendlyFireAny", "Disables friendly fire of any type."},
            {"tooltip_NoCooldown", "Disables cooldowns (certain delays will still occur)."},
            {
                "tooltip_InstantCooldownMainChar",
                "Disables cooldowns only for the main character (certain delays will still occur)."
            },
            {"tooltip_NoBurnKineticist", "The Burn Cost is either completely removed or not applied."},
            {
                "tooltip_MaximiseAcceptedBurnKineticist",
                "Sets the accepted burn to its maximal value and keeps it at that value until deactivated.\nChanges to the burn maximum or party composition/new levels as Kineticist might require to toggle this option off and on or a rest."
            },
            {
                "tooltip_NoTempHPKineticist",
                "Removes the temporary hit point reduction caused by burn.\nRequires you to return to the main menu and reload your save game!"
            },
            {
                "tooltip_SpellsPerDay",
                "Use the slider or a custom multiplier to set the amount of spells per day / memorised spells."
            },

            {"tooltip_InstantPartyChange", "Swapping party members on the global map happens instantly."},
            {
                "tooltip_SetPartySpeedToFastestMember",
                "Sets the speed of the whole party to match the speed of the fastest party member."
            },
            {
                "tooltip_PartyMovementSpeedMultiplier",
                "Multiplies the movement speed of all party members by the set value."
            },
            {
                "tooltip_ClaimResources",
                "Claims all revealed resources in your lands.\nNote that the claims will still cost you BP!"
            },
            {"tooltip_NoResourcesClaimCost", "Sets the cost for claiming resources to 0."},
            {
                "tooltip_EnableTeleport",
                "Teleport the party to the current mouse position with a key press (on foot and on the global map)."
            },
            {"tooltip_TeleportPartyToPlayer", "Teleports the party to the player character."},
            {
                "tooltip_TravelSpeedMultiplier",
                "Use the input field to set a multiplier for the global map travel time scale."
            },

            {
                "tooltip_RandomEncounterSettings",
                "Use the toggles, slider and input files to configure random encounter to your liking."
            },
            {"tooltip_RandomEncounterPossible", "Disables random encounters on the global map."},
            {"tooltip_AlwaysSucceedAtAvoidingEncounters", "Avoiding random encounters will always be successful."},

            {"tooltip_UpgradeSettlementsAndBuildings", "Upgrade settlements and individual buildings."},
            {"tooltip_SetKingdomAlignment", "Set the kingdom's alignment to the one chosen from the selection grid."},
            {"tooltip_OverrideKingdomAlignmentResult", "﻿Overrides kingdom alignment result."},
            {"tooltip_InstantKingdomEvents", "﻿﻿Instantly complete kingdom events."},
            {"tooltip_ForceSuccessTriumphs", "﻿﻿﻿Force success/triumph for the result of kingdom events."},
            {"tooltip_IgnoreBuildingRestrictions", "﻿﻿﻿Ignore kingdom building restrictions."},

            {
                "tooltip_PassSkillChecksIndividual",
                "Allows you to pick individual skills which will always pass when checked."
            },
            {"tooltip_PassSkillChecksIndividualDC99", "Sets the DC of all skill checks to -99."},
            {
                "tooltip_PassSavingThrowIndividual",
                "Allows you to pick individual saving throws which will always pass when checked."
            },
            {"tooltip_AllHitsAreCritical", "All attack performed by the party result in critical hits."},
            {"tooltip_MainCharacterRolls20", "Every roll on a d20 by the main character will result in a natural 20."},
            {"tooltip_PartyAlwaysRolls20", "Every roll on a d20 by the party will result in a natural 20."},
            {"tooltip_EnablePartyAlwaysRoll20Hotkey", "Enable a hotkey to toggle Party Always Rolls 20."},
            {"tooltip_EnemiesAlwaysRoll1", "Every roll on a d20 by the enemies will result in a natural 1."},
            {
                "tooltip_EveryoneExceptPlayerFactionRolls1",
                "Every roll on a d20 by anyone except the members of the player faction will result in a natural 1."
            },
            {
                "tooltip_NeverRoll1",
                "Rolls of 1 on a d20 never occur.\nCan be set to affect everyone, only the party or only enemies."
            },
            {
                "tooltip_NeverRoll20",
                "Rolls of 20 on a d20 never occur.\nCan be set to affect everyone, only the party or only enemies."
            },
            {
                "tooltip_RollWithAdvantage",
                "Two d20s are rolled instead of one and the highest one is taken as the result.\nCan be set to affect everyone, only the party or only enemies."
            },
            {
                "tooltip_RollWithDisadvantage",
                "Two d20s are rolled instead of one and the lowest one is taken as the result.\nCan be set to affect everyone, only the party or only enemies."
            },
            {"tooltip_AlwaysRoll20ForInitiative", "﻿Everyone including your enemies rolls 20 for initiative."},
            {
                "tooltip_TakeX",
                "﻿﻿D20 rolls made out of combat will result in the set value i.e. 10, 20 or a custom one."
            },

            {"tooltip_CameraRotation", "Enables camera rotation."},
            {"tooltip_CameraScrollSpeed", "Enables the option to change the scroll speed of the camera."},
            {"tooltip_CameraZoomUnlocked", "Enables editing the camera zoom limits."},
            {
                "tooltip_EnableFocusCamera",
                "Enables focusing the camera on party members and cycling through them via hotkey."
            },
            {
                "tooltip_EnableFocusCameraSelectedUnit",
                "Selecting a character's portrait will cause the camera to focus on them."
            },
            {"tooltip_DisableCameraBounds", "Disables the camera bounds and allows you to free move the camera."},
            {"tooltip_MoveCameraToPlayer", "Centers the camera on the player character."},
            {
                "tooltip_ResetCutsceneLock",
                "﻿﻿Allows you to move during cutscenes.\nBest used together with Focus Camera to override the game's focus."
            },
            {"tooltip_EnableResetCutsceneLockHotkey", "Enables to toggle the Reset Cutscene Lock option via hotkey."},
            {"tooltip_AnimationCloseUMM", "Closes the Unity Mod Manager interface once a play button is pressed."},
            {"tooltip_ShowAreaName", "Displays the current area's name at the top of the screen."},
            {
                "tooltip_ShowCombatDifficulty",
                "Displays the combat difficulty and CR once combat starts.\nIf the development tools are enabled their combat difficulty display will be used instead.\nDisabling requires a restart to take effect!"
            },


            {
                "tooltip_UnityModManagerButton",
                "﻿﻿Adds a button which opens the Unity Mod Manager UI to the ESC menu.\nRequires to load a save or change the area in order to take effect."
            },
            {
                "tooltip_UnityModManagerButtonIndex",
                "Adjusts the position of the button (0 = bottom, 6 = top).\nRequires to load a save or change the area in order to take effect."
            },
            {
                "tooltip_AlignmentFix",
                "E.g. when being Lawful Neutral a Neutral Good alignment shift\nwill bring you towards Lawful Good instead of Neutral Good"
            },
            {
                "tooltip_AutoEquipConsumables",
                "The belt slots will automatically be refilled after an items is used.\nBut only if another item of the same type is available in the party's inventory.\nEquipping an item (e.g. Alchemist's Fire) multiple times in the belt while also having them on the quick/action bar \nwill cause 0 charge items to left behind if used from the belt."
            },
            {
                "tooltip_ShowPetInventory",
                "Allows to view a pet's inventory by selecting it and pressing the inventory key."
            },
            {
                "tooltip_ScaleInventoryMoney",
                "Dynamically scales the size of the money display to fit larger amounts properly."
            },
            {"tooltip_SortSpellbooksAlphabetically", "Sorts the spellbooks on the Spellbooks Screen alphabetically."},
            {"tooltip_SortSpellsAlphabetically", "Sorts the spells on the Spellbooks Screen alphabetically."},
            {
                "tooltip_NoNegativeLevels",
                "Prevents party members from gaining negative levels.\nThe game will still display level drain messages in the battle log."
            },

            {
                "tooltip_FreezeTimedQuestAt90Days",
                "Freezes the time left on timed quest at 90 days.\nOnce turned off timed quest will return to their normal state i.e. they will react as if time has progressed while the option was active.\nThus it is recommended to complete timed quest before turning it off or making sure that there is still time left."
            },
            {"tooltip_ActionKey", "﻿﻿The action key can be set to perform a range of actions via hotkey."},
            {"tooltip_NoAttacksOfOpportunity", "The selected group won't be the target of attacks of opportunity."},
            {"tooltip_SummonDurationMultiplier", "Multiplies the duration of summons by the chosen value."},
            {"tooltip_SetSummonLevelTo20", "Sets the level of all summons to 20."},
            {
                "tooltip_ExtraAttacksParty",
                "All party members gain the set number of extra main attacks for the primary and secondary hand."
            },
            {
                "tooltip_SetSpeedOnSummon",
                "The speed of all creatures summoned by the party will be set to the chosen value.\nDoes not apply to already summoned ones!"
            },
            {"tooltip_TimeScale", "Changes the time scale of the whole game."},
            {
                "tooltip_StopGameTime",
                "Stops the passing of time on local maps, on the global map and through actions (e.g. resting).\nQuest timers won't advance."
            },
            {"tooltip_NoDamageFromEnemies", "Enemies don't deal damage to the party."},
            {
                "tooltip_PartyOneHitKill",
                "Party members kill their target with a single blow (if they hit and are using the correct damage type)."
            },
            {
                "tooltip_DamageDealtMultipliers",
                "Enables the setting of multipliers to the damage dealt by the party and enemies."
            },
            {"tooltip_PartyDamageDealtMultiplier", "Enables damage dealt by the party multiplier."},
            {"tooltip_EnemiesDamageDealtMultiplier", "Enables damage dealt by enemies multiplier."},
            {
                "tooltip_AllDoorContainersUnlocked",
                "They will still be display as if they were locked, but any character will be able to open them.\nThis might grant you access to places you shouldn't be able to reach!"
            },
            {
                "tooltip_ActionKeyEnableExperimental",
                "Experimental Mode includes features that can break the game and cause crashes!"
            },
            {
                "tooltip_LogInfoToFile",
                "Enables logging the gathered information to the BagOfTricks.log file in the BagOfTricks folder."
            },
            {"tooltip_ShowUnitInfoBox", "Displays a info box with unit statistics."},
            {"tooltip_OverwriteFogOfWar", "Allows to completely disable fog of war or only its visuals."},
            {"tooltip_IgnoreDialogueRestrictions", "﻿﻿Removes dialogue restrictions."},
            {
                "tooltip_ReverseCasterAlignmentChecks",
                "﻿﻿Reverses caster alignment checks e.g. when summoning monsters with an evil character you will summon the good variant."
            },
            {"tooltip_PreventAlignmentChanges", "﻿﻿Prevents any changes made to a character's alignment."},
            {"tooltip_NoInactiveCampsite", "﻿Disables leaving an inactive campsite."},
            {
                "tooltip_SpiderBegone",
                "﻿Swaps the models of spiders with wolves and spider swarms with rat swarms.\nNote that this doesn't affect the small non-interactable spiders!\nA game restart/reload is highly recommended after changing the setting!"
            },

            {"tooltip_PreventQuestFailure", "﻿While active quests won't fail."},
            {
                "tooltip_DevTools",
                "Enables the built-in development tools (e.g. console) and adds new commands.\nRequires a restart to fully take effect."
            },
            {"tooltip_LogToUMM", "Writes all Smart Console logs to the Unity Mod Manger log."},
            {"tooltip_UberLogger", "Enables the built-in logger."},
            {"tooltip_UberLoggerForward", "Forwards UberLogger's messages to Unity's output log."},
            {"tooltip_UberLoggerForwardPrefix", "Adds [UberLogger] to the messages forwarded to Unity's output log."},


            {
                "tooltip_BlueprintModding",
                "Enables the modification of existing blueprints by loading files from the ModifiedBlueprints directory."
            },
            {"tooltip_PatchManually", "Applies the item modification immediately."},

            {"tooltip_ToggleHUD", "Enables toggling of the HUD via hotkey."},
            {"tooltip_DisplayObjectInfo", "Displays an object's name and GUID when hovering your cursor over it."},

            {
                "tooltip_TaxCollector",
                "The Tax Collector will collect build points and money (based on your character and kingdom stats) for you while you are gone."
            },
            {"tooltip_ShowTooltips", "﻿﻿Disables the display of tooltips by the mod."},
            {"tooltip_LogMessages", "﻿Disables in-game log messages created by the mod."},
            {
                "tooltip_ExportToModFolder",
                "﻿Files will be exported to \\Pathfinder Kingmaker\\Mods\\BagOfTricks\\Export."
            },
            {
                "tooltip_BoTScrollbar",
                "﻿Enables a scrollbar in the mod's menu allowing the category buttons to be visible at all time.\nDon't enable this if you have a lot of mods installed!"
            },
            {"tooltip_CreateFallbackXML", "Creates a .xml file of the fallback localisation."},
            {"tooltip_SnakeMode", "sssSsssSssssSSsssssSSSsssssSSssssSsssSsssave"},

            {"tooltip_SpellFavourites", "Spells are added to the Abilities Favourites"},

            {"tooltip_ArrowUp", "Click to move\nthe element up\nor shift-click\nto move it to\nthe top."},
            {"tooltip_ArrowDown", "Click to move\nthe element down\nor shift-click\nto move it to\nthe bottom."},

            { "tooltip_OutOfCombatOnly_ArmourChecksPenalty0", "Set Armour Check Penalty To 0 will only be active outside of combat encounters."},
            { "tooltip_OutOfCombatOnly_ArcaneSpellFailureRoll", "No Arcane Spell Failure will only be active outside of combat encounters."},
            //tooltip end                               


            //misc start
            {"misc_items", "items"},
            {"misc_times", "times"},
            {"misc_item", "item"},
            {"misc_time", "time"},
            {"misc_Enable", "Enable"},
            {"misc_Disable", "Disable"},
            {"misc_Hide", "Hide"},
            {"misc_Display", "Display"},
            {"misc_Add", "Add"},
            {"misc_To", "To"},
            {"misc_Play", "Play"},
            {"misc_Yes", "Yes"},
            {"misc_No", "No"},
            {"misc_Next", "Next"},
            {"misc_None", "None"},
            {"misc_Left", "left"},
            {"misc_UnityModManager", "Unity Mod Manager"},
            {"misc_Bandit", "Bandit"},
            {"misc_SnakeMode", "Snake Mode"},
            {"misc_BagOfSnakeModes", "Bag Of Snake Modes"},
            {"misc_Learn", "Learn"},
            {"misc_AtLevel", "At Level"},
            {"misc_AlwaysPass", "Always Pass"},
            {"misc_Checks", "Checks"},
            {"misc_Saves", "Saves"},
            {"misc_Days", "Days"},
            {"misc_Sell", "Sell"},
            {"misc_Unlock", "Unlock"},
            {"misc_UnlockAll", "Unlock All"},
            {"misc_By", "By"},
            {"misc_All", "All"},
            {"misc_Other", "Other"},
            {"misc_RequiresRestart", "Requires a restart to fully take effect."},
            {"misc_True", "True"},
            {"misc_False", "False"},
            {"misc_NumberOfRolls", "Number Of Rolls"},
            //misc end

            {"encumbrance_Light", "Light"},
            {"encumbrance_Medium", "Medium"},
            {"encumbrance_Heavy", "Heavy"},
            {"encumbrance_Overload", "Overload"},

            //error start
            {
                "error_EkunQ2",
                "c5d4962385e0e9c439ab935d83361947 (EkunQ2_rewardArmor) causes errors and thus won't be added."
            },
            {"error_NotFound", "not found!"},
            //error end

            //taxCollector start
            {"taxCollector_Lord", "Lord"},
            {"taxCollector_Lady", "Lady"},
            {"taxCollector_Excellence", "Excellence"},
            {"taxCollector_GreetingInitial_0_0", "Greetings my"},
            {"taxCollector_GreetingInitial_0_1", "Greetings your"},
            {"taxCollector_GreetingInitial_1", "Or do you wish to be addressed differently?"},
            {"taxCollector_GreetingInitial_2", "Please tell me, how I shall address you in the future?"},
            {"taxCollector_ButtonAddresseMe", "Address me as"},
            {"taxCollector_GreetingInitial_3", "Brilliant! Let's get started!"},

            {"taxCollector_GreetingReturn_0_0", "Welcome back"},
            {"taxCollector_GreetingReturn_0_1", "Welcome"},
            {"taxCollector_GreetingReturn_1_0", "What do you wish to do?"},

            {"taxCollector_ButtonCollectTaxes", "Collect Taxes"},
            {
                "taxCollector_NothingToCollect_0_0",
                "My apologies, the tax collectors haven't returned yet. Try again later!"
            },
            {"taxCollector_Money", "Money"},
            {"taxCollector_BuildPoints", "Build Points"},
            {"taxCollector_Collected_0_0", "Here you go"},

            {"taxCollector_EmployTaxCollector", "Employ Tax Collector"},
            {"taxCollector_Approach", "Approach"},
            //taxCollector end


            //about start
            {
                "about_KingmakerModsPW",
                "Includes code from KingmakerMods.pw - Copyright (c) 2018 fireundubh <fireundubh@gmail.com>, MIT License (MIT)"
            },
            //about end
        };
    }
}