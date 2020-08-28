using BagOfTricks.Favourites;
using BagOfTricks.ModUI;
using BagOfTricks.Utils;
using BagOfTricks.Utils.Kingmaker;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Rest.Cooking;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Kingdom.Tasks;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks {
    public static class ActionKey {

        public static Settings settings = Main.settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;

        public static readonly List<BlueprintScriptableObject> sBlueprintInfo = new List<BlueprintScriptableObject>();



        private static readonly string[] mainExperimentalArray = { Strings.GetText("misc_None"), Strings.GetText("arrayItem_ActionKeyMain_GetInfo"), Strings.GetText("arrayItem_ActionKeyKill_Kill"), Strings.GetText("arrayItem_ActionKeyMain_ResurrectAndFullyRestore"), Strings.GetText("arrayItem_ActionKeyMain_BuffFromFavourites"), Strings.GetText("arrayItem_ActionKeyMain_EditStats"), Strings.GetText("label_TeleportUnit"), Strings.GetText("arrayItem_ActionKeyMain_SpawnUnit"), Strings.GetText("arrayItem_ActionKeyMain_RotateUnit"), Strings.GetText("header_Animations"), Strings.GetText("arrayItem_ActionKeyMain_SpawnCritters"), Strings.GetText("arrayItem_ActionKeyMain_MakeControllable"), Strings.GetText("arrayItem_ActionKeyMain_AddToParty"), Strings.GetText("arrayItem_ActionKeyMain_RecreateUnitDescriptor") };
        private static readonly string[] mainArray = { Strings.GetText("misc_None"), Strings.GetText("arrayItem_ActionKeyMain_GetInfo"), Strings.GetText("arrayItem_ActionKeyKill_Kill"), Strings.GetText("arrayItem_ActionKeyMain_ResurrectAndFullyRestore"), Strings.GetText("arrayItem_ActionKeyMain_BuffFromFavourites"), Strings.GetText("arrayItem_ActionKeyMain_EditStats"), Strings.GetText("label_TeleportUnit"), Strings.GetText("arrayItem_ActionKeyMain_SpawnUnit"), Strings.GetText("arrayItem_ActionKeyMain_RotateUnit"), Strings.GetText("header_Animations") };
        private static readonly string[] experimentalKillArray = { Strings.GetText("arrayItem_ActionKeyKill_Kill"), Strings.GetText("arrayItem_ActionKeyKill_ForceKill") };
        private static UnitEntityData unit = null;

        public static UnitEntityData editUnit = null;
        public static string editUnitStatsAmount = "10";
        public static int editUnitFinalStatsAmount = 10;
        public static int editUnitSelectedSizeIndex = 0;

        public static UnitEntityData teleportUnit = null;
        public static UnitEntityData rotateUnit = null;

        public static int banidtCrIndex = 0;
        public static string[] numberArray0t7 = { "0", "1", "2", "3", "4", "5", "6", "7" };
        public static string[] banditsGuids = { "6b8a83ef580c62341b674a5abf3afced", "b191e1a8f45ab2e438865988f48ab399", "de468d0b47f87644a3408642fe0876c", "435e3d847f566e5479acd4de2642ba31", "4956f420a728baa4d911281ed81e8ce4", "b68ce570b3bf13743a401356fda68c79", "179944f7553cafa468c6f548effbbf71", "341a00b2bbf4be9498049c62e6cd7456" };

        public static List<UnitAnimationType> animationTypes = new List<UnitAnimationType>();
        public static List<string> animationTypesNames = new List<string>();
        public static int animationTypesIndex = 0;

        public static bool load = true;

        public static void Functions(int index) {

            switch (index) {
                case 0:
                    break;
                case 1:
                    GameInfo();
                    break;
                case 2:
                    KillFunctions();
                    break;
                case 3:
                    UnitEntityDataUtils.ResurrectAndFullRestore(Common.GetUnitUnderMouse());
                    break;
                case 4:
                    UnitEntityDataUtils.Buff(Common.GetUnitUnderMouse(), FavouritesFactory.GetFavouriteBuffs.FavouritesList[Main.settings.actionKeyBuffIndex]);
                    break;
                case 5:
                    editUnit = Common.GetUnitUnderMouse();
                    break;
                case 6:
                    teleportUnit = Common.GetUnitUnderMouse();
                    if (teleportUnit != null && Strings.ToBool(Main.settings.toggleAddToLog)) {
                        Common.AddLogEntry(Strings.GetText("label_TeleportUnit") + $": {teleportUnit.CharacterName}", Color.black);
                    }
                    break;
                case 7:
                    if (Strings.ToBool(settings.toggleSpawnEnemiesFromUnitFavourites)) {
                        try {
                            Vector3 pos = Common.MousePositionLocalMap();
                            float x = 0.0f;
                            float z = 0.0f;
                            foreach (string guid in SpawnUnits.GetStoredGUIDs) {
                                Vector3 finalPos = new Vector3(pos.x + 1.5f * x, pos.y, pos.z + 1.5f * z);
                                SpawnUnits.UnitSpawner(finalPos, guid);
                                x++;
                                if (x > 10f) {
                                    x = 0.0f;
                                    z++;
                                }
                            }

                        }
                        catch (Exception e) {
                            modLogger.Log(e.ToString());
                        }
                    }
                    else if (settings.actionKeySpawnRandomEnemy && Strings.ToBool(settings.toggleActionKeyExperimental)) {
                        try {
                            Common.SpawnHostileUnit(Common.MousePositionLocalMap(), ResourcesLibrary.GetBlueprints<BlueprintUnit>().RandomElement());

                        }
                        catch (Exception e) {
                            modLogger.Log(e.ToString());
                        }
                    }
                    else {
                        Common.SpawnHostileUnit(Common.MousePositionLocalMap(), banditsGuids[banidtCrIndex]);
                    }
                    break;
                case 8:
                    rotateUnit = Common.GetUnitUnderMouse();
                    if (rotateUnit != null && Strings.ToBool(Main.settings.toggleAddToLog)) {
                        Common.AddLogEntry(Strings.GetText("arrayItem_ActionKeyMain_RotateUnit") + $": {rotateUnit.CharacterName}", Color.black);
                    }
                    break;
                case 9:
                    Common.GetUnitUnderMouse().View.AnimationManager.Execute(animationTypes[animationTypesIndex]);
                    break;
                case 10:
                    FxHelper.SpawnFxOnPoint(BlueprintRoot.Instance.Cheats.SillyCheatBlood, Common.MousePositionLocalMap());
                    break;
                case 11:
                    UnitEntityDataUtils.Charm(Common.GetUnitUnderMouse());
                    break;
                case 12:
                    UnitEntityDataUtils.AddToParty(Common.GetUnitUnderMouse());
                    break;
                case 13:
                    Common.GetUnitUnderMouse().Descriptor.Recreate = true;
                    break;
            }
        }
        public static void KillFunctions() {
            switch (Main.settings.actionKeyKillIndex) {
                case 0:
                    UnitEntityDataUtils.Kill(Common.GetUnitUnderMouse());
                    break;
                case 1:
                    UnitEntityDataUtils.ForceKill(Common.GetUnitUnderMouse());
                    break;
            }
        }

        public static void ActionKeyEditStatsGui(UnitEntityData unit) {
            GL.Space(10);
            GL.BeginHorizontal();
            ActionKey.editUnitSelectedSizeIndex = GL.SelectionGrid(ActionKey.editUnitSelectedSizeIndex, Storage.charSizeArray, 4);
            GL.EndHorizontal();

            GL.Space(10);
            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_SetSizeTo") + $" {Storage.charSizeArray[ActionKey.editUnitSelectedSizeIndex]}", GL.ExpandWidth(false))) {
                unit.Descriptor.State.Size = (Size)ActionKey.editUnitSelectedSizeIndex;
            }
            GL.EndHorizontal();
            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_SetToOriginalSize") + $" ({unit.Descriptor.OriginalSize})", GL.ExpandWidth(false))) {
                unit.Descriptor.State.Size = unit.Descriptor.OriginalSize;
            }
            GL.EndHorizontal();
            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentSize") + ": " + unit.Descriptor.State.Size);
            GL.Space(10);

            GL.BeginHorizontal();
            if (unit.Descriptor.HPLeft > 0) {
                if (GL.Button(Strings.GetText("button_Kill"), GL.ExpandWidth(false))) {
                    UnitEntityDataUtils.Kill(unit);
                }
                if (GL.Button(Strings.GetText("button_Panic"), GL.ExpandWidth(false))) {
                    unit.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>("cf0e277e6b785f449bbaf4e993b556e0"), (MechanicsContext)null, new FeatureParam());
                }
                if (GL.Button(Strings.GetText("button_Freeze"), GL.ExpandWidth(false))) {
                    unit.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>("af1e2d232ebbb334aaf25e2a46a92591"), (MechanicsContext)null, new FeatureParam());
                }
                if (GL.Button(Strings.GetText("button_MakeCower"), GL.ExpandWidth(false))) {
                    unit.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>("6062e3a8206a4284d867cbb7120dc091"), (MechanicsContext)null, new FeatureParam());
                }
                if (GL.Button(Strings.GetText("button_SetOnFire"), GL.ExpandWidth(false))) {
                    unit.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>("315acb0b29671f74c8c7cc062b23b9d6"), (MechanicsContext)null, new FeatureParam());
                }
            }
            GL.EndHorizontal();

            GL.BeginHorizontal();
            editUnitStatsAmount = GL.TextField(editUnitStatsAmount, 10, GL.Width(85f));
            MenuTools.SettingParse(ref editUnitStatsAmount, ref editUnitFinalStatsAmount);
            GL.EndHorizontal();

            CharacterStats charStats = unit.Descriptor.Stats;
            MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_AttributesBaseValues")));
            foreach (KeyValuePair<string, StatType> entry in Storage.statsAttributesDict) {
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, ActionKey.editUnitFinalStatsAmount);
            }
            MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_SkillsRanks")));
            foreach (KeyValuePair<string, StatType> entry in Storage.statsSkillsDict) {
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, ActionKey.editUnitFinalStatsAmount);
            }
            MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_SocialSkillsBaseValues")));
            foreach (KeyValuePair<string, StatType> entry in Storage.statsSocialSkillsDict) {
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, ActionKey.editUnitFinalStatsAmount);
            }
            MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_StatsSaves")));
            foreach (KeyValuePair<string, StatType> entry in Storage.statsSavesDict) {
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, ActionKey.editUnitFinalStatsAmount);
            }
            MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_StatsCombat")));
            foreach (KeyValuePair<string, StatType> entry in Storage.statsCombatDict) {
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, ActionKey.editUnitFinalStatsAmount);
            }
        }


        public static void MainToggle(int startIndex = 0) {
            if (GL.Button(MenuTools.TextWithTooltip("misc_Enable", "tooltip_ActionKey", $"{settings.toggleEnableActionKey}" + " ", ""), GL.ExpandWidth(false))) {
                if (settings.toggleEnableActionKey == Storage.isFalseString) {
                    settings.toggleEnableActionKey = Storage.isTrueString;
                    settings.actionKeyIndex = startIndex;
                    settings.actionKeyKillIndex = 0;
                }
                else if (settings.toggleEnableActionKey == Storage.isTrueString) {
                    settings.toggleEnableActionKey = Storage.isFalseString;
                    settings.actionKeyIndex = 0;
                    settings.actionKeyKillIndex = 0;
                }
            }
        }

        public static void RenderMenu() {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichTextUtils.MainCategoryFormat(Strings.GetText("label_ActionKey")));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton("ActionKeyRender");
            GL.EndHorizontal();

            GL.BeginHorizontal();
            MainToggle();
            GL.EndHorizontal();

            if (settings.toggleEnableActionKey == Storage.isTrueString) {
                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_ActionKey") + ": ", GL.ExpandWidth(false));
                Keys.SetKeyBinding(ref settings.actionKey);
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                if (GL.Button(MenuTools.TextWithTooltip("label_ActionKeyEnableExperimental", "tooltip_ActionKeyEnableExperimental", $"{settings.toggleActionKeyExperimental}" + " ", ""), GL.ExpandWidth(false))) {
                    if (settings.toggleActionKeyExperimental == Storage.isFalseString) {
                        settings.toggleActionKeyExperimental = Storage.isTrueString;
                        settings.actionKeyIndex = 0;
                        settings.actionKeyKillIndex = 0;
                    }
                    else if (settings.toggleActionKeyExperimental == Storage.isTrueString) {
                        settings.toggleActionKeyExperimental = Storage.isFalseString;
                        settings.actionKeyIndex = 0;
                        settings.actionKeyKillIndex = 0;
                    }
                }
                GL.EndHorizontal();

                MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("warning_ActionKeyExperimentalMode")));

                GL.BeginHorizontal();
                if (!Strings.ToBool(settings.toggleActionKeyExperimental)) {
                    settings.actionKeyIndex = GL.SelectionGrid(settings.actionKeyIndex, mainArray, 3);
                }
                else {
                    settings.actionKeyIndex = GL.SelectionGrid(settings.actionKeyIndex, mainExperimentalArray, 3);
                }
                GL.EndHorizontal();

                GL.Space(10);

                switch (settings.actionKeyIndex) {
                    case 1:
                        MenuTools.ToggleButtonActionAtOn(ref settings.toggleActionKeyLogInfo, new Action(LoggerUtils.InitBagOfTrickLogger), "buttonToggle_LogInfoToFile", "tooltip_LogInfoToFile");
                        MenuTools.ToggleButton(ref settings.toggleActionKeyShowUnitInfoBox, "buttonToggle_ShowUnitInfoBox", "tooltip_ShowUnitInfoBox");

                        break;
                    case 2:
                        if (Strings.ToBool(settings.toggleActionKeyExperimental)) {
                            GL.Space(10);
                            GL.BeginHorizontal();
                            settings.actionKeyKillIndex = GL.SelectionGrid(settings.actionKeyKillIndex, experimentalKillArray, 3);
                            GL.EndHorizontal();
                        }
                        break;
                    case 4:
                        if (!FavouritesFactory.GetFavouriteBuffs.FavouritesList.Any()) {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                        }
                        else {
                            if (Storage.buffFavouritesLoad == true) {
                                Main.RefreshBuffFavourites();
                                Storage.buffFavouritesLoad = false;
                            }

                            GL.Space(10);
                            GL.BeginHorizontal();
                            settings.actionKeyBuffIndex = GL.SelectionGrid(settings.actionKeyBuffIndex, Storage.buffFavouritesNames.ToArray(), 2);
                            GL.EndHorizontal();

                        }
                        break;
                    case 5:
                        if (editUnit != null && editUnit.IsInGame && !editUnit.Descriptor.State.IsFinallyDead) {
                            ActionKeyEditStatsGui(editUnit);
                        }
                        else {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitSelected"));
                        }
                        break;
                    case 6:
                        if (teleportUnit != null && teleportUnit.IsInGame) {
                            MenuTools.SingleLineLabel(Strings.GetText("label_TeleportUnit") + $": {teleportUnit.CharacterName}");
                        }
                        else {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitSelected"));
                        }
                        break;
                    case 7:

                        MenuTools.ToggleButton(ref settings.toggleSpawnEnemiesFromUnitFavourites, "buttonToggle_ActionKeySpawnUnitsFromUnitFavourites", "tooltip_ActionKeySpawnUnitsFromUnitFavourites", nameof(settings.toggleSpawnEnemiesFromUnitFavourites));
                        if (Strings.ToBool(settings.toggleSpawnEnemiesFromUnitFavourites)) {
                            SpawnUnits.FavouritesMenu();
                        }

                        if (Strings.ToBool(settings.toggleActionKeyExperimental)) {
                            GL.Space(10);

                            settings.actionKeySpawnRandomEnemy = GL.Toggle(settings.actionKeySpawnRandomEnemy, " " + Strings.GetText("toggle_SpawnRandomEnemy"), GL.ExpandWidth(false));
                        }
                        if (Strings.ToBool(settings.toggleSpawnEnemiesFromUnitFavourites) && settings.actionKeySpawnRandomEnemy) {
                            MenuTools.SingleLineLabel(RichTextUtils.BoldRedFormat(Strings.GetText("warning_SpawnRandomHostileUnit_ActionKeySpawnEnemiesFromUnitFavourites")));
                        }

                        GL.Space(10);
                        if (!Strings.ToBool(settings.toggleSpawnEnemiesFromUnitFavourites)) {
                            MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("label_SpawnHostileBandits1")));
                        }
                        else {
                            MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("label_SpawnHostileBandits1")) + " " + Strings.Parenthesis(RichTextUtils.BoldRedFormat(Strings.GetText("label_SpawnHostileBandits2"))));
                        }
                        MenuTools.SingleLineLabel(Strings.GetText("label_ChallengeRating") + " " + Strings.Parenthesis(Strings.GetText("misc_Bandit")));
                        GL.BeginHorizontal();
                        banidtCrIndex = GL.SelectionGrid(banidtCrIndex, numberArray0t7, 8);
                        GL.EndHorizontal();


                        break;
                    case 8:
                        if (rotateUnit != null && rotateUnit.IsInGame) {
                            MenuTools.SingleLineLabel(Strings.GetText("arrayItem_ActionKeyMain_RotateUnit") + $": {rotateUnit.CharacterName}");

                        }
                        else {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitSelected"));
                        }
                        break;
                    case 9:
                        if (load) {
                            animationTypes.Clear();
                            animationTypesNames.Clear();
                            foreach (UnitAnimationType animation in (UnitAnimationType[])Enum.GetValues(typeof(UnitAnimationType))) {
                                animationTypes.Add(animation);
                                animationTypesNames.Add(animation.ToString());

                            }
                            load = false;
                        }
                        GL.BeginHorizontal();
                        animationTypesIndex = GL.SelectionGrid(animationTypesIndex, animationTypesNames.ToArray(), 3);
                        GL.EndHorizontal();
                        break;
                    case 13:
                        MenuTools.SingleLineLabel(Strings.GetText("message_RecreateUnitDescriptor"));
                        break;
                }
            }
            GL.EndVertical();
        }

        public static void GameInfo() {
            sBlueprintInfo.Clear();
            AddGameInfo();
        }

        public static void AddGameInfo() {
            CollectGameInfo();
            if (sBlueprintInfo.Count == 0)
                return;


            BlueprintScriptableObject blueprint = sBlueprintInfo[0];

            string blueprintName = Utilities.GetBlueprintName(blueprint);
            string assetGuid = blueprint.AssetGuid;


            if (Strings.ToBool(Main.settings.toggleActionKeyLogInfo)) {
                Main.botLoggerLog.Log(blueprintName);
                Main.botLoggerLog.Log(assetGuid);
            }

            if (Main.settings.toggleAddToLog == Storage.isTrueString) {
                Common.AddLogEntry(blueprintName, Color.black);
                Common.AddLogEntry(assetGuid, Color.black);
            }
            else {
                Main.modLogger.Log(blueprintName);
                Main.modLogger.Log(assetGuid);

            }
            if (Main.settings.toggleActionKeyShowUnitInfoBox == Storage.isTrueString) {
                string message = Strings.GetText("label_AssetGuid") + ": " + unit.Blueprint.AssetGuid + "\n";
                message = message + Strings.GetText("label_BlueprintName") + ": " + unit.Blueprint.name + "\n";
                message = message + Strings.GetText("label_UnitName") + ": " + unit.Blueprint.CharacterName + "\n";
                message = message + Strings.GetText("label_ChallengeRating") + ": " + unit.Blueprint.CR + "\n";
                CharacterStats charStats = unit.Descriptor.Stats;
                foreach (KeyValuePair<string, StatType> entry in Storage.statsAttributesDict) {
                    message = message + $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})\n";
                }
                message = message + Strings.GetText("charStat_HitPoints") + ": " + unit.Descriptor.HPLeft + "/" + unit.Descriptor.MaxHP + "\n";
                message = message + $"{Strings.GetText("charStat_ArmourClass")}: {charStats.GetStat(StatType.AC).BaseValue} ({charStats.GetStat(StatType.AC).ModifiedValue})\n";
                message = message + $"{Strings.GetText("charStat_BaseAttackBonus")}: {charStats.GetStat(StatType.BaseAttackBonus).BaseValue} ({charStats.GetStat(StatType.BaseAttackBonus).ModifiedValue})\n";
                message = message + $"{Strings.GetText("charStat_Speed")}: {charStats.GetStat(StatType.Speed).BaseValue} ({charStats.GetStat(StatType.Speed).ModifiedValue})\n";
                message = message + $"{Strings.GetText("charStat_Reach")}: {charStats.GetStat(StatType.Reach).BaseValue} ({charStats.GetStat(StatType.Reach).ModifiedValue})\n";

                message = message + "\n" + Strings.GetText("label_AssetGuid") + ": " + unit.GetFirstWeapon().Blueprint.AssetGuid + "\n";
                message = message + Strings.GetText("label_BlueprintName") + ": " + unit.GetFirstWeapon().Blueprint.name + "\n";
                message = message + Strings.GetText("label_WeaponName") + ": " + unit.GetFirstWeapon().Name + "\n";
                message = message + Strings.GetText("label_WeaponDamage") + ": " + unit.GetFirstWeapon().Damage + "\n";

                UIUtility.ShowMessageBox(message, DialogMessageBoxBase.BoxType.Message, new Action<DialogMessageBoxBase.BoxButton>(Common.CloseMessageBox));
            }

        }

        public static void CollectGameInfo() {
            UnitEntityData unitUnderMouse = Common.GetUnitUnderMouse();

            if (Main.settings.toggleActionKeyShowUnitInfoBox == Storage.isTrueString && unitUnderMouse.Descriptor != null) {
                unit = unitUnderMouse;
            }

            BlueprintScriptableObject[] scriptableObjectArray = Tooltip();
            if (unitUnderMouse != null) {
                sBlueprintInfo.Add((BlueprintScriptableObject)unitUnderMouse.Blueprint);
            }
            else {
                if (scriptableObjectArray == null)
                    return;
                foreach (BlueprintScriptableObject scriptableObject in scriptableObjectArray)
                    sBlueprintInfo.Add(scriptableObject);
            }

        }

        public static BlueprintScriptableObject[] Tooltip() {
            TooltipData contextTooltipData = Game.Instance.UI.TooltipsController.ContextTooltipData;
            if (contextTooltipData == null)
                return (BlueprintScriptableObject[])null;
            ItemEntity itemEntity = contextTooltipData.Item;
            if (itemEntity != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) itemEntity.Blueprint
                };
            BlueprintFeatureBase feature = contextTooltipData.Feature;
            if (feature != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) feature
                };
            Ability ability = contextTooltipData.Ability;
            if (ability != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) ability.Blueprint
                };
            BlueprintFeatureSelection featureSelection = contextTooltipData.FeatureSelection;
            if (featureSelection != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) featureSelection
                };
            AbilityData abilityData = contextTooltipData.AbilityData;
            if (abilityData != (AbilityData)null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) abilityData.Blueprint
                };
            ActivatableAbility activatableAbility = contextTooltipData.ActivatableAbility;
            if (activatableAbility != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) activatableAbility.Blueprint
                };
            Buff buff = contextTooltipData.Buff;
            if (buff != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) buff.Blueprint
                };
            BlueprintAbility blueprintAbility = contextTooltipData.BlueprintAbility;
            if (blueprintAbility != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) blueprintAbility
                };
            BlueprintCookingRecipe recipe = contextTooltipData.Recipe;
            if (recipe != (UnityEngine.Object)null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) recipe
                };
            KingdomBuff kingdomBuff = contextTooltipData.KingdomBuff;
            if (kingdomBuff != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) kingdomBuff.Blueprint
                };
            UnitEntityData unit = contextTooltipData.Unit;
            if (unit != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) unit.Blueprint
                };
            BlueprintCharacterClass blueprintCharacterClass = contextTooltipData.Class;
            if (blueprintCharacterClass != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) blueprintCharacterClass
                };
            BlueprintRace race = contextTooltipData.Race;
            if (race != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) race
                };
            BlueprintSettlementBuilding settlementBuildingBp = contextTooltipData.SettlementBuildingBp;
            if (settlementBuildingBp != null)
                return new BlueprintScriptableObject[1]
                {
          (BlueprintScriptableObject) settlementBuildingBp
                };
            SettlementBuilding settlementBuilding = contextTooltipData.SettlementBuilding;
            if (settlementBuilding == null)
                return (BlueprintScriptableObject[])contextTooltipData.TutorialPage ?? (BlueprintScriptableObject[])null;
            return new BlueprintScriptableObject[1]
            {
        (BlueprintScriptableObject) settlementBuilding.Blueprint
            };
        }
    }
}
