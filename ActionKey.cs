using System;
using System.Collections.Generic;
using System.Linq;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Particles;

using UnityEngine;

using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    public static class ActionKey
    {
        public static Settings settings = Main.Settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.ModLogger;

        public static readonly List<BlueprintScriptableObject> s_BlueprintInfo = new List<BlueprintScriptableObject>();


        private static readonly string[] mainExperimentalArray =
        {
            Strings.GetText("misc_None"), Strings.GetText("arrayItem_ActionKeyMain_GetInfo"),
            Strings.GetText("arrayItem_ActionKeyKill_Kill"),
            Strings.GetText("arrayItem_ActionKeyMain_ResurrectAndFullyRestore"),
            Strings.GetText("arrayItem_ActionKeyMain_BuffFromFavourites"),
            Strings.GetText("arrayItem_ActionKeyMain_EditStats"), Strings.GetText("label_TeleportUnit"),
            Strings.GetText("arrayItem_ActionKeyMain_SpawnEnemy"),
            Strings.GetText("arrayItem_ActionKeyMain_RotateUnit"), Strings.GetText("header_Animations"),
            Strings.GetText("arrayItem_ActionKeyMain_SpawnCritters"),
            Strings.GetText("arrayItem_ActionKeyMain_MakeControllable"),
            Strings.GetText("arrayItem_ActionKeyMain_AddToParty"),
            Strings.GetText("arrayItem_ActionKeyMain_RecreateUnitDescriptor")
        };

        private static readonly string[] mainArray =
        {
            Strings.GetText("misc_None"), Strings.GetText("arrayItem_ActionKeyMain_GetInfo"),
            Strings.GetText("arrayItem_ActionKeyKill_Kill"),
            Strings.GetText("arrayItem_ActionKeyMain_ResurrectAndFullyRestore"),
            Strings.GetText("arrayItem_ActionKeyMain_BuffFromFavourites"),
            Strings.GetText("arrayItem_ActionKeyMain_EditStats"), Strings.GetText("label_TeleportUnit"),
            Strings.GetText("arrayItem_ActionKeyMain_SpawnEnemy"),
            Strings.GetText("arrayItem_ActionKeyMain_RotateUnit"), Strings.GetText("header_Animations")
        };


        private static readonly string[] experimentalKillArray =
            {Strings.GetText("arrayItem_ActionKeyKill_Kill"), Strings.GetText("arrayItem_ActionKeyKill_ForceKill")};

        private static UnitEntityData unit = null;

        public static UnitEntityData editUnit = null;
        public static string editUnitStatsAmount = "10";
        public static int editUnitFinalStatsAmount = 10;
        public static int editUnitSelectedSizeIndex = 0;

        public static UnitEntityData teleportUnit = null;
        public static UnitEntityData rotateUnit = null;

        public static int banidtCrIndex = 0;
        public static string[] numberArray0t7 = {"0", "1", "2", "3", "4", "5", "6", "7"};

        public static string[] banditsGuids =
        {
            "6b8a83ef580c62341b674a5abf3afced", "b191e1a8f45ab2e438865988f48ab399", "de468d0b47f87644a3408642fe0876c",
            "435e3d847f566e5479acd4de2642ba31", "4956f420a728baa4d911281ed81e8ce4", "b68ce570b3bf13743a401356fda68c79",
            "179944f7553cafa468c6f548effbbf71", "341a00b2bbf4be9498049c62e6cd7456"
        };

        public static List<UnitAnimationType> animationTypes = new List<UnitAnimationType>();
        public static List<string> animationTypesNames = new List<string>();
        public static int animationTypesIndex = 0;

        public static bool load = true;

        public static void Functions(int index)
        {
            switch (index)
            {
                case 0:
                    break;
                case 1:
                    GameInfo();
                    break;
                case 2:
                    KillFunctions();
                    break;
                case 3:
                    Common.ResurrectAndFullRestore(Common.GetUnitUnderMouse());
                    break;
                case 4:
                    Common.Buff(Common.GetUnitUnderMouse(),
                        Storage.buffFavouritesGuids[Main.Settings.actionKeyBuffIndex]);
                    break;
                case 5:
                    editUnit = Common.GetUnitUnderMouse();
                    break;
                case 6:
                    teleportUnit = Common.GetUnitUnderMouse();
                    if (teleportUnit != null && Strings.ToBool(Main.Settings.toggleAddToLog))
                        Common.AddLogEntry(Strings.GetText("label_TeleportUnit") + $": {teleportUnit.CharacterName}",
                            Color.black);
                    break;
                case 7:
                    if (settings.actionKeySpawnRandomEnemy && Strings.ToBool(settings.toggleActionKeyExperimental))
                        try
                        {
                            Common.SpawnHostileUnit(Common.MousePositionLocalMap(),
                                ResourcesLibrary.GetBlueprints<BlueprintUnit>().RandomElement());
                        }
                        catch (Exception e)
                        {
                            modLogger.Log(e.ToString());
                        }
                    else
                        Common.SpawnHostileUnit(Common.MousePositionLocalMap(), banditsGuids[banidtCrIndex]);

                    break;
                case 8:
                    rotateUnit = Common.GetUnitUnderMouse();
                    if (rotateUnit != null && Strings.ToBool(Main.Settings.toggleAddToLog))
                        Common.AddLogEntry(
                            Strings.GetText("arrayItem_ActionKeyMain_RotateUnit") + $": {rotateUnit.CharacterName}",
                            Color.black);
                    break;
                case 9:
                    Common.GetUnitUnderMouse().View.AnimationManager.Execute(animationTypes[animationTypesIndex]);
                    break;
                case 10:
                    FxHelper.SpawnFxOnPoint(BlueprintRoot.Instance.Cheats.SillyCheatBlood,
                        Common.MousePositionLocalMap());
                    break;
                case 11:
                    Common.Charm(Common.GetUnitUnderMouse());
                    break;
                case 12:
                    Common.AddToParty(Common.GetUnitUnderMouse());
                    break;
                case 13:
                    Common.GetUnitUnderMouse().Descriptor.Recreate = true;
                    break;
            }
        }

        public static void KillFunctions()
        {
            switch (Main.Settings.actionKeyKillIndex)
            {
                case 0:
                    Common.Kill(Common.GetUnitUnderMouse());
                    break;
                case 1:
                    Common.ForceKill(Common.GetUnitUnderMouse());
                    break;
            }
        }

        public static void ActionKeyEditStatsGui(UnitEntityData unit)
        {
            GL.Space(10);
            GL.BeginHorizontal();
            editUnitSelectedSizeIndex = GL.SelectionGrid(editUnitSelectedSizeIndex, Storage.charSizeArray, 4);
            GL.EndHorizontal();

            GL.Space(10);
            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_SetSizeTo") + $" {Storage.charSizeArray[editUnitSelectedSizeIndex]}",
                GL.ExpandWidth(false))) unit.Descriptor.State.Size = (Size) editUnitSelectedSizeIndex;
            GL.EndHorizontal();
            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_SetToOriginalSize") + $" ({unit.Descriptor.OriginalSize})",
                GL.ExpandWidth(false))) unit.Descriptor.State.Size = unit.Descriptor.OriginalSize;
            GL.EndHorizontal();
            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentSize") + ": " + unit.Descriptor.State.Size);
            GL.Space(10);

            GL.BeginHorizontal();
            if (unit.Descriptor.HPLeft > 0)
            {
                if (GL.Button(Strings.GetText("button_Kill"), GL.ExpandWidth(false))) Common.Kill(unit);
                if (GL.Button(Strings.GetText("button_Panic"), GL.ExpandWidth(false)))
                    unit.Descriptor.AddFact(
                        (BlueprintUnitFact) Utilities.GetBlueprintByGuid<BlueprintBuff>(
                            "cf0e277e6b785f449bbaf4e993b556e0"), (MechanicsContext) null, new FeatureParam());
                if (GL.Button(Strings.GetText("button_Freeze"), GL.ExpandWidth(false)))
                    unit.Descriptor.AddFact(
                        (BlueprintUnitFact) Utilities.GetBlueprintByGuid<BlueprintBuff>(
                            "af1e2d232ebbb334aaf25e2a46a92591"), (MechanicsContext) null, new FeatureParam());
                if (GL.Button(Strings.GetText("button_MakeCower"), GL.ExpandWidth(false)))
                    unit.Descriptor.AddFact(
                        (BlueprintUnitFact) Utilities.GetBlueprintByGuid<BlueprintBuff>(
                            "6062e3a8206a4284d867cbb7120dc091"), (MechanicsContext) null, new FeatureParam());
                if (GL.Button(Strings.GetText("button_SetOnFire"), GL.ExpandWidth(false)))
                    unit.Descriptor.AddFact(
                        (BlueprintUnitFact) Utilities.GetBlueprintByGuid<BlueprintBuff>(
                            "315acb0b29671f74c8c7cc062b23b9d6"), (MechanicsContext) null, new FeatureParam());
            }

            GL.EndHorizontal();

            GL.BeginHorizontal();
            editUnitStatsAmount = GL.TextField(editUnitStatsAmount, 10, GL.Width(85f));

            editUnitStatsAmount = MenuTools.IntTestSettingStage1(editUnitStatsAmount);
            editUnitFinalStatsAmount = MenuTools.IntTestSettingStage2(editUnitStatsAmount, editUnitFinalStatsAmount);
            GL.EndHorizontal();

            var charStats = unit.Descriptor.Stats;
            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_AttributesBaseValues")));
            foreach (var entry in Storage.statsAttributesDict)
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, editUnitFinalStatsAmount);
            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_SkillsRanks")));
            foreach (var entry in Storage.statsSkillsDict)
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, editUnitFinalStatsAmount);
            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_SocialSkillsBaseValues")));
            foreach (var entry in Storage.statsSocialSkillsDict)
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, editUnitFinalStatsAmount);
            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_StatsSaves")));
            foreach (var entry in Storage.statsSavesDict)
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, editUnitFinalStatsAmount);
            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_StatsCombat")));
            foreach (var entry in Storage.statsCombatDict)
                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, editUnitFinalStatsAmount);
        }


        public static void RenderMenu()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.MainCategoryFormat(Strings.GetText("label_ActionKey")));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton("ActionKeyRender");
            GL.EndHorizontal();

            GL.BeginHorizontal();
            if (GL.Button(
                MenuTools.TextWithTooltip("misc_Enable", "tooltip_ActionKey", $"{settings.toggleEnableActionKey}" + " ",
                    ""), GL.ExpandWidth(false)))
            {
                if (settings.toggleEnableActionKey == Storage.isFalseString)
                {
                    settings.toggleEnableActionKey = Storage.isTrueString;
                    settings.actionKeyIndex = 0;
                    settings.actionKeyKillIndex = 0;
                }
                else if (settings.toggleEnableActionKey == Storage.isTrueString)
                {
                    settings.toggleEnableActionKey = Storage.isFalseString;
                    settings.actionKeyIndex = 0;
                    settings.actionKeyKillIndex = 0;
                }
            }

            GL.EndHorizontal();

            if (settings.toggleEnableActionKey == Storage.isTrueString)
            {
                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_ActionKey") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.actionKey);
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                if (GL.Button(
                    MenuTools.TextWithTooltip("label_ActionKeyEnableExperimental",
                        "tooltip_ActionKeyEnableExperimental", $"{settings.toggleActionKeyExperimental}" + " ", ""),
                    GL.ExpandWidth(false)))
                {
                    if (settings.toggleActionKeyExperimental == Storage.isFalseString)
                    {
                        settings.toggleActionKeyExperimental = Storage.isTrueString;
                        settings.actionKeyIndex = 0;
                        settings.actionKeyKillIndex = 0;
                    }
                    else if (settings.toggleActionKeyExperimental == Storage.isTrueString)
                    {
                        settings.toggleActionKeyExperimental = Storage.isFalseString;
                        settings.actionKeyIndex = 0;
                        settings.actionKeyKillIndex = 0;
                    }
                }

                GL.EndHorizontal();

                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("warning_ActionKeyExperimentalMode")));

                GL.BeginHorizontal();
                if(!Strings.ToBool(settings.toggleActionKeyExperimental))
                {
                    settings.actionKeyIndex = GL.SelectionGrid(settings.actionKeyIndex, mainArray, 3);
                }
                else
                {
                    settings.actionKeyIndex = GL.SelectionGrid(settings.actionKeyIndex, mainExperimentalArray, 3);
                }
                GL.EndHorizontal();

                GL.Space(10);

                switch (settings.actionKeyIndex)
                {
                    case 1:
                        MenuTools.ToggleButton(ref settings.toggleActionKeyLogInfo, "buttonToggle_LogInfoToFile",
                            "tooltip_LogInfoToFile");
                        MenuTools.ToggleButton(ref settings.toggleActionKeyShowUnitInfoBox,
                            "buttonToggle_ShowUnitInfoBox", "tooltip_ShowUnitInfoBox");

                        break;
                    case 2:
                        if (Strings.ToBool(settings.toggleActionKeyExperimental))
                        {
                            GL.Space(10);
                            GL.BeginHorizontal();
                            settings.actionKeyKillIndex =
                                GL.SelectionGrid(settings.actionKeyKillIndex, experimentalKillArray, 3);
                            GL.EndHorizontal();
                        }

                        break;
                    case 4:
                        if (!Storage.buffFavourites.Any())
                        {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                        }
                        else
                        {
                            if (Storage.buffFavouritesLoad == true)
                            {
                                Main.RefreshBuffFavourites();
                                Storage.buffFavouritesLoad = false;
                            }

                            GL.Space(10);
                            GL.BeginHorizontal();
                            settings.actionKeyBuffIndex = GL.SelectionGrid(settings.actionKeyBuffIndex,
                                Storage.buffFavouriteNames.ToArray(), 2);
                            GL.EndHorizontal();
                        }

                        if (Storage.buffFavourites != Storage.buffFavouritesGuids)
                            Storage.buffFavourites = Storage.buffFavouritesGuids;
                        break;
                    case 5:
                        if (editUnit != null && editUnit.IsInGame && !editUnit.Descriptor.State.IsFinallyDead)
                            ActionKeyEditStatsGui(editUnit);
                        else
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitSelected"));
                        break;
                    case 6:
                        if (teleportUnit != null && teleportUnit.IsInGame)
                            MenuTools.SingleLineLabel(Strings.GetText("label_TeleportUnit") +
                                                      $": {teleportUnit.CharacterName}");
                        else
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitSelected"));
                        break;
                    case 7:
                        if (Strings.ToBool(settings.toggleActionKeyExperimental))
                            settings.actionKeySpawnRandomEnemy = GL.Toggle(settings.actionKeySpawnRandomEnemy,
                                " " + Strings.GetText("toggle_SpawnRandomEnemy"), GL.ExpandWidth(false));

                        GL.Space(10);

                        MenuTools.SingleLineLabel(Strings.GetText("label_ChallengeRating") + " " +
                                                  Strings.Parenthesis(Strings.GetText("misc_Bandit")));
                        GL.BeginHorizontal();
                        banidtCrIndex = GL.SelectionGrid(banidtCrIndex, numberArray0t7, 8);
                        GL.EndHorizontal();

                        break;
                    case 8:
                        if (rotateUnit != null && rotateUnit.IsInGame)
                            MenuTools.SingleLineLabel(Strings.GetText("arrayItem_ActionKeyMain_RotateUnit") +
                                                      $": {rotateUnit.CharacterName}");
                        else
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitSelected"));
                        break;
                    case 9:
                        if (load)
                        {
                            animationTypes.Clear();
                            animationTypesNames.Clear();
                            foreach (var animation in (UnitAnimationType[]) Enum.GetValues(typeof(UnitAnimationType)))
                            {
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

        public static void GameInfo()
        {
            s_BlueprintInfo.Clear();
            AddGameInfo();
        }

        public static void AddGameInfo()
        {
            CollectGameInfo();
            if (s_BlueprintInfo.Count == 0)
                return;


            var blueprint = s_BlueprintInfo[0];

            var blueprintName = Utilities.GetBlueprintName(blueprint);
            var assetGuid = blueprint.AssetGuid;


            if (Main.Settings.toggleActionKeyLogInfo == Storage.isTrueString)
            {
                Logger.Log(blueprintName);
                Logger.Log(assetGuid);
            }

            if (Main.Settings.toggleAddToLog == Storage.isTrueString)
            {
                Common.AddLogEntry(blueprintName, Color.black);
                Common.AddLogEntry(assetGuid, Color.black);
            }
            else
            {
                Main.ModLogger.Log(blueprintName);
                Main.ModLogger.Log(assetGuid);
            }

            if (Main.Settings.toggleActionKeyShowUnitInfoBox == Storage.isTrueString)
            {
                var message = Strings.GetText("label_AssetGuid") + ": " + unit.Blueprint.AssetGuid + "\n";
                message = message + Strings.GetText("label_BlueprintName") + ": " + unit.Blueprint.name + "\n";
                message = message + Strings.GetText("label_UnitName") + ": " + unit.Blueprint.CharacterName + "\n";
                message = message + Strings.GetText("label_ChallengeRating") + ": " + unit.Blueprint.CR + "\n";
                var charStats = unit.Descriptor.Stats;
                foreach (var entry in Storage.statsAttributesDict)
                    message = message +
                              $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})\n";
                message = message + Strings.GetText("charStat_HitPoints") + ": " + unit.Descriptor.HPLeft + "/" +
                          unit.Descriptor.MaxHP + "\n";
                message = message +
                          $"{Strings.GetText("charStat_ArmourClass")}: {charStats.GetStat(StatType.AC).BaseValue} ({charStats.GetStat(StatType.AC).ModifiedValue})\n";
                message = message +
                          $"{Strings.GetText("charStat_BaseAttackBonus")}: {charStats.GetStat(StatType.BaseAttackBonus).BaseValue} ({charStats.GetStat(StatType.BaseAttackBonus).ModifiedValue})\n";
                message = message +
                          $"{Strings.GetText("charStat_Speed")}: {charStats.GetStat(StatType.Speed).BaseValue} ({charStats.GetStat(StatType.Speed).ModifiedValue})\n";
                message = message +
                          $"{Strings.GetText("charStat_Reach")}: {charStats.GetStat(StatType.Reach).BaseValue} ({charStats.GetStat(StatType.Reach).ModifiedValue})\n";

                message = message + "\n" + Strings.GetText("label_AssetGuid") + ": " +
                          unit.GetFirstWeapon().Blueprint.AssetGuid + "\n";
                message = message + Strings.GetText("label_BlueprintName") + ": " +
                          unit.GetFirstWeapon().Blueprint.name + "\n";
                message = message + Strings.GetText("label_WeaponName") + ": " + unit.GetFirstWeapon().Name + "\n";
                message = message + Strings.GetText("label_WeaponDamage") + ": " + unit.GetFirstWeapon().Damage + "\n";

                UIUtility.ShowMessageBox(message, DialogMessageBox.BoxType.Message,
                    new Action<DialogMessageBox.BoxButton>(Common.CloseMessageBox));
            }
        }

        public static void CollectGameInfo()
        {
            var unitUnderMouse = Common.GetUnitUnderMouse();

            if (Main.Settings.toggleActionKeyShowUnitInfoBox == Storage.isTrueString &&
                unitUnderMouse.Descriptor != null) unit = unitUnderMouse;

            var scriptableObjectArray = Tooltip();
            if (unitUnderMouse != null)
            {
                s_BlueprintInfo.Add((BlueprintScriptableObject) unitUnderMouse.Blueprint);
            }
            else
            {
                if (scriptableObjectArray == null)
                    return;
                foreach (var scriptableObject in scriptableObjectArray)
                    s_BlueprintInfo.Add(scriptableObject);
            }
        }

        public static BlueprintScriptableObject[] Tooltip()
        {
            var contextTooltipData = Game.Instance.UI.TooltipsController.ContextTooltipData;
            if (contextTooltipData == null)
                return (BlueprintScriptableObject[]) null;
            var itemEntity = contextTooltipData.Item;
            if (itemEntity != null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) itemEntity.Blueprint
                };
            var feature = contextTooltipData.Feature;
            if ((UnityEngine.Object) feature != (UnityEngine.Object) null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) feature
                };
            var ability = contextTooltipData.Ability;
            if (ability != null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) ability.Blueprint
                };
            var featureSelection = contextTooltipData.FeatureSelection;
            if ((UnityEngine.Object) featureSelection != (UnityEngine.Object) null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) featureSelection
                };
            var abilityData = contextTooltipData.AbilityData;
            if (abilityData != (AbilityData) null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) abilityData.Blueprint
                };
            var activatableAbility = contextTooltipData.ActivatableAbility;
            if (activatableAbility != null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) activatableAbility.Blueprint
                };
            var buff = contextTooltipData.Buff;
            if (buff != null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) buff.Blueprint
                };
            var blueprintAbility = contextTooltipData.BlueprintAbility;
            if ((UnityEngine.Object) blueprintAbility != (UnityEngine.Object) null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) blueprintAbility
                };
            var recipe = contextTooltipData.Recipe;
            if ((UnityEngine.Object) recipe != (UnityEngine.Object) null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) recipe
                };
            var kingdomBuff = contextTooltipData.KingdomBuff;
            if (kingdomBuff != null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) kingdomBuff.Blueprint
                };
            var unit = contextTooltipData.Unit;
            if (unit != null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) unit.Blueprint
                };
            var blueprintCharacterClass = contextTooltipData.Class;
            if ((UnityEngine.Object) blueprintCharacterClass != (UnityEngine.Object) null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) blueprintCharacterClass
                };
            var race = contextTooltipData.Race;
            if ((UnityEngine.Object) race != (UnityEngine.Object) null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) race
                };
            var settlementBuildingBp = contextTooltipData.SettlementBuildingBp;
            if ((UnityEngine.Object) settlementBuildingBp != (UnityEngine.Object) null)
                return new BlueprintScriptableObject[1]
                {
                    (BlueprintScriptableObject) settlementBuildingBp
                };
            var settlementBuilding = contextTooltipData.SettlementBuilding;
            if (settlementBuilding == null)
                return (BlueprintScriptableObject[]) contextTooltipData.TutorialPage ??
                       (BlueprintScriptableObject[]) null;
            return new BlueprintScriptableObject[1]
            {
                (BlueprintScriptableObject) settlementBuilding.Blueprint
            };
        }
    }
}