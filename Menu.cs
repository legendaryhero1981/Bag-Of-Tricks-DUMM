using Harmony12;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Rest.Cooking;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.FogOfWar;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.WeatherSystem;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.SceneManagement;

using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    public static class Menu
    {
        public static Settings settings = Main.Settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.ModLogger;

        public static void SpawnUnitsMenu()
        {
            SpawnUnits.RenderMenu();
        }

        public static void ClassData()
        {
            GL.BeginVertical("box");

            MenuTools.ToggleButton(ref settings.toggleShowClassDataOptions, "buttonToggle_ShowClassDataOptions",
                "tooltip_ShowClassDataOptions");

            if (Strings.ToBool(settings.toggleShowClassDataOptions))
            {
                GL.Space(10);

                MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_Experimental")));

                GL.BeginHorizontal();
                if (GL.Button(
                    RichText.Bold($"{settings.toggleExperimentalIUnderstand} " +
                                  Strings.GetText("buttonToggle_experimentalIUnderstand")), GL.ExpandWidth(false)))
                {
                    if (settings.toggleExperimentalIUnderstand == Storage.isFalseString)
                        settings.toggleExperimentalIUnderstand = Storage.isTrueString;
                    else if (settings.toggleExperimentalIUnderstand == Storage.isTrueString)
                        settings.toggleExperimentalIUnderstand = Storage.isFalseString;
                }

                GL.EndHorizontal();
                GL.Space(10);

                if (Strings.ToBool(settings.toggleExperimentalIUnderstand))
                {
                    if (Storage.statsSelectedControllableCharacterIndex !=
                        Storage.statsSelectedControllableCharacterIndexOld)
                    {
                        Storage.classNames.Clear();
                        Storage.classData.Clear();
                        Storage.statsSelectedControllableCharacterIndexOld =
                            Storage.statsSelectedControllableCharacterIndex;
                    }

                    GL.BeginHorizontal();
                    if (GL.Button(Strings.GetText("button_LoadRefresh"), GL.ExpandWidth(false)))
                    {
                        Storage.classNames.Clear();
                        Storage.classData.Clear();
                    }

                    GL.EndHorizontal();

                    GL.Space(10);

                    var prog = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                        .Progression;

                    Main.GetClassNames(prog);

                    GL.BeginHorizontal();
                    Storage.statsSelectedClassIndex = GL.SelectionGrid(Storage.statsSelectedClassIndex, Storage.classNames.ToArray(), 3);
                    GL.EndHorizontal();
                    var selectedClass = Storage.classData[Storage.statsSelectedClassIndex];

                    GL.Space(10);

                    GL.BeginHorizontal();
                    Storage.classLevelSlider = GL.HorizontalSlider(Storage.classLevelSlider, 0, 20f, GL.Width(200f));
                    GL.Label($" {Strings.GetText("header_Level")}: {Mathf.RoundToInt(Storage.classLevelSlider)}",
                        GL.ExpandWidth(false));
                    GL.EndHorizontal();


                    if (prog.GetClassLevel(selectedClass.CharacterClass) > 0)
                        MenuTools.SingleLineLabel(Strings.GetText("label_CurrentLevel") +
                                                  $": {prog.GetClassLevel(selectedClass.CharacterClass)}");
                    else
                        MenuTools.SingleLineLabel(Strings.GetText("label_CurrentLevel") + ": -");

                    if (GL.Button($"{Strings.GetText("button_SetTo")} {Mathf.RoundToInt(Storage.classLevelSlider)}",
                        GL.ExpandWidth(false)))
                    {
                        Storage.classNames.Clear();

                        if (prog.GetClassLevel(selectedClass.CharacterClass) > 0)
                        {
                            prog.GetClassData(selectedClass.CharacterClass).Level =
                                Mathf.RoundToInt(Storage.classLevelSlider);
                        }
                        else if (prog.GetClassLevel(selectedClass.CharacterClass) == 0)
                        {
                            selectedClass.Level = Mathf.RoundToInt(Storage.classLevelSlider);
                            prog.Classes.Add(selectedClass);
                        }
                    }
                }
            }

            GL.EndVertical();
        }

        public static void ChangeName()
        {
            GL.BeginVertical("box");
            MenuTools.SingleLineLabel(MenuTools.TextWithTooltip("label_CustomName", "tooltip_CustomName", true));
            GL.BeginHorizontal();
            Storage.partyCustomName = GL.TextField(Storage.partyCustomName, 36, GL.Width(300f));
            GL.EndHorizontal();

            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentCustomName") + ": " + Storage
                                          .statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                                          .CustomName);
            MenuTools.SingleLineLabel(Strings.GetText("label_BlueprintCharacterName") + ": " + Storage
                                          .statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                                          .Blueprint.CharacterName);


            if (GL.Button(
                new GUIContent(Strings.GetText("button_SetTo") + " " + Storage.partyCustomName,
                    Strings.GetText("tooltip_CustomName")), GL.ExpandWidth(false)))
            {
                Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.CustomName =
                    Storage.partyCustomName;
                Storage.reloadPartyStats = true;
            }

            if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
            {
                Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.CustomName =
                    string.Empty;
                Storage.reloadPartyStats = true;
            }

            GL.EndVertical();
        }

        public static void PlayAnimations()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showAnimationsCategory = GL.Toggle(settings.showAnimationsCategory, RichText.Bold(Strings.GetText("header_Animations")), GL.ExpandWidth(false));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(PlayAnimations));
            if (settings.showAnimationsCategory == true)
            {
                if (Storage.reloadPartyAnimations)
                {
                    Storage.animationsSelectedControllableCharacterIndex = 0;
                    Storage.animationsPartyMembers = Game.Instance.Player.ControllableCharacters;
                    Storage.animationsControllableCharacterNamesList.Clear();
                    foreach (var controllableCharacter in Storage.animationsPartyMembers)
                        Storage.animationsControllableCharacterNamesList.Add(controllableCharacter.CharacterName);
                    Storage.reloadPartyAnimations = false;
                }

                if (!Storage.reloadPartyAnimations)
                {
                    GL.BeginHorizontal();
                    Storage.animationsSelectedControllableCharacterIndex = GL.SelectionGrid(Storage.animationsSelectedControllableCharacterIndex, Storage.animationsControllableCharacterNamesList.ToArray(), 3);
                    GL.EndHorizontal();

                    GL.Space(10);

                    MenuTools.ToggleButton(ref settings.toggleAnimationCloseUMM, "buttonToggle_AnimationCloseUMM",
                        "tooltip_AnimationCloseUMM", nameof(settings.toggleAnimationCloseUMM));

                    GL.Space(10);

                    foreach (var animation in (UnitAnimationType[])Enum.GetValues(typeof(UnitAnimationType)))
                    {
                        GL.BeginHorizontal();
                        if (GL.Button(Strings.GetText("misc_Play") + " " + animation.ToString(), GL.ExpandWidth(false)))
                        {
                            Storage.animationsPartyMembers[Storage.animationsSelectedControllableCharacterIndex].View
                                .AnimationManager.Execute(animation);

                            if (Strings.ToBool(settings.toggleAnimationCloseUMM))
                                UnityModManager.UI.Instance.ToggleWindow();
                        }

                        GL.EndHorizontal();
                    }
                }
            }

            GL.EndVertical();
        }

        public static void CurrentHitPointsOptions()
        {
            GL.BeginVertical("box");
            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_CurrentHitPoints") + ": " +
                                                    Storage.statsPartyMembers[
                                                            Storage.statsSelectedControllableCharacterIndex].Descriptor
                                                        .HPLeft + "/" +
                                                    Storage.statsPartyMembers[
                                                            Storage.statsSelectedControllableCharacterIndex].Descriptor
                                                        .MaxHP + "\n"));
            GL.BeginHorizontal();
            Storage.healthPercentageSlider =
                GL.HorizontalSlider(Storage.healthPercentageSlider, 0.01f, 1f, GL.Width(300f));
            GL.Label($" {Storage.healthPercentageSlider:p0}", GL.ExpandWidth(false));
            GL.EndHorizontal();
            GL.Space(10);
            GL.BeginHorizontal();
            if (GL.Button(
                $"{Strings.GetText("button_SetTo")} {Storage.healthPercentageSlider:p0} ({Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName})",
                GL.ExpandWidth(false)))
                Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Damage = Mathf.RoundToInt(
                    Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.MaxHP -
                    Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.MaxHP *
                    Storage.healthPercentageSlider);
            GL.EndHorizontal();

            GL.Space(10);

            GL.BeginHorizontal();
            if (GL.Button(
                $"{Strings.GetText("button_SetPartysCurrentHitPointsTo")} {Storage.healthPercentageSlider:p0}",
                GL.ExpandWidth(false)))
                foreach (var unit in Storage.statsPartyMembers)
                    unit.Damage =
                        Mathf.RoundToInt(unit.Descriptor.MaxHP -
                                         unit.Descriptor.MaxHP * Storage.healthPercentageSlider);
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void DowngradeSettlement()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.Bold(Strings.GetText("label_DowngradeSettlements")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(DowngradeSettlement));
            if (KingdomState.Instance?.Regions != null && KingdomState.Instance != null)
                foreach (var region in KingdomState.Instance.Regions)
                    if (region.IsClaimed)
                        if (region.Settlement != null)
                        {
                            MenuTools.SingleLineLabel(RichText.Bold(
                                region.Blueprint.LocalizedName.ToString() + " - " + region.Settlement.Name));
                            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentLevel") + ": " +
                                                      region.Settlement.Level.ToString());
                            if (region.Settlement.Level == SettlementState.LevelType.City ||
                                region.Settlement.Level == SettlementState.LevelType.Town)
                                if (GL.Button(Strings.GetText("button_Downgrade"), GL.ExpandWidth(false)))
                                {
                                    if (region.Settlement.Level == SettlementState.LevelType.City)
                                    {
                                        var type = region.Settlement.GetType();
                                        var propertyInfo = type.GetProperty("Level");
                                        propertyInfo.SetValue(region.Settlement, SettlementState.LevelType.Town, null);
                                    }
                                    else if (region.Settlement.Level == SettlementState.LevelType.Town)
                                    {
                                        var type = region.Settlement.GetType();
                                        var propertyInfo = type.GetProperty("Level");
                                        propertyInfo.SetValue(region.Settlement, SettlementState.LevelType.Village,
                                            null);
                                    }
                                }
                        }

            GL.EndVertical();
        }

        public static SelectionGrid passSkillChecksIndividualGrid =
            new SelectionGrid(Storage.unitEntityDataSelectionGridArray, 4);

        public static void PassSkillChecksIndividual()
        {
            GL.BeginVertical("box");

            MenuTools.ToggleButtonFavouritesMenu(ref settings.togglePassSkillChecksIndividual, "buttonToggle_PassSkillChecksIndividual", "tooltip_PassSkillChecksIndividual", true);
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(PassSkillChecksIndividual));

            if (Strings.ToBool(settings.togglePassSkillChecksIndividual))
            {
                passSkillChecksIndividualGrid.Render(ref settings.indexPassSkillChecksIndividual);

                MenuTools.ToggleButton(ref settings.togglePassSkillChecksIndividualDC99,
                    "buttonToggle_PassSkillChecksIndividualDC99", "tooltip_PassSkillChecksIndividualDC99", true);

                for (var i = 0; i < Main.Settings.togglePassSkillChecksIndividualArray.Count(); i++)
                {
                    GL.BeginHorizontal();
                    if (GL.Button(
                        Main.Settings.togglePassSkillChecksIndividualArray[i] + " " +
                        Strings.GetText("misc_AlwaysPass") + " " + Storage.individualSkillsArray[i] + " " +
                        Strings.GetText("misc_Checks"), GL.ExpandWidth(false)))
                    {
                        if (Main.Settings.togglePassSkillChecksIndividualArray[i] == Storage.isFalseString)
                            Main.Settings.togglePassSkillChecksIndividualArray[i] = Storage.isTrueString;
                        else if (Main.Settings.togglePassSkillChecksIndividualArray[i] == Storage.isTrueString)
                            Main.Settings.togglePassSkillChecksIndividualArray[i] = Storage.isFalseString;
                    }

                    GL.EndHorizontal();
                }
            }

            GL.EndVertical();
        }

        public static void MiscExtras()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("misc_SnakeMode", "tooltip_SnakeMode", false), GL.ExpandWidth(false)))
            {
                using (EntityPoolEnumerator<UnitEntityData> units = Game.Instance.State.Units.GetEnumerator())
                {
                    while (units.MoveNext())
                    {
                        UnitEntityData unit;
                        if ((unit = units.Current) != null && unit.IsInGame && !unit.Descriptor.State.IsFinallyDead)
                        {
                            unit.View.AnimationManager.Execute(UnitAnimationType.Prone);
                        }
                    }
                }
            }
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(MiscExtras));
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("misc_BagOfSnakeModes", "tooltip_SnakeMode", false), GL.ExpandWidth(false)))
            {
                using (EntityPoolEnumerator<UnitEntityData> units = Game.Instance.State.Units.GetEnumerator())
                {
                    while (units.MoveNext())
                    {
                        UnitEntityData unit;
                        if ((unit = units.Current) != null && unit.IsInGame && !unit.Descriptor.State.IsFinallyDead)
                        {
                            unit.View.AnimationManager.Execute(UnitAnimationType.Prone);
                            FxHelper.SpawnFxOnPoint(BlueprintRoot.Instance.Cheats.SillyCheatBlood, unit.Position);
                        }
                    }
                }
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        private static TextFieldInt extraAttacksPartyTextField = new TextFieldInt();

        public static void ExtraAttacksParty()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleExtraAttacksParty, "buttonToggle_ExtraAttacksParty", "tooltip_ExtraAttacksParty");
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(ExtraAttacksParty));
            if (Strings.ToBool(settings.toggleExtraAttacksParty))
            {
                extraAttacksPartyTextField.RenderField();
                GL.Space(10);
                GL.BeginHorizontal();
                if (GL.Button(
                    Strings.GetText("button_SetPrimaryHandAttacksTo") + $" {extraAttacksPartyTextField.finalAmount}",
                    GL.ExpandWidth(false)))
                    settings.extraAttacksPartyPrimaryHand = extraAttacksPartyTextField.finalAmount;
                GL.EndHorizontal();
                MenuTools.SingleLineLabel(Strings.GetText("label_CurrentValue") +
                                          $": {settings.extraAttacksPartyPrimaryHand}");
                GL.BeginHorizontal();
                if (GL.Button(
                    Strings.GetText("button_SetSecondaryHandAttacksTo") + $" {extraAttacksPartyTextField.finalAmount}",
                    GL.ExpandWidth(false)))
                    settings.extraAttacksPartySecondaryHand = extraAttacksPartyTextField.finalAmount;
                GL.EndHorizontal();
                MenuTools.SingleLineLabel(Strings.GetText("label_CurrentValue") +
                                          $": {settings.extraAttacksPartySecondaryHand}");
            }

            GL.EndVertical();
        }

        private static TextFieldInt setSpeedOnSummonTextField = new TextFieldInt();

        public static void SetSpeedOnSummon()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleSetSpeedOnSummon, "buttonToggle_SetSpeedOnSummon", "tooltip_SetSpeedOnSummon");
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(SetSpeedOnSummon));
            if (Strings.ToBool(settings.toggleSetSpeedOnSummon))
            {
                setSpeedOnSummonTextField.RenderField();
                GL.Space(10);
                GL.BeginHorizontal();
                if (GL.Button(Strings.GetText("button_SetSpeedOnSummon") + $" {setSpeedOnSummonTextField.finalAmount}",
                    GL.ExpandWidth(false))) settings.setSpeedOnSummonValue = setSpeedOnSummonTextField.finalAmount;
                GL.EndHorizontal();
                MenuTools.SingleLineLabel(Strings.GetText("label_CurrentValue") +
                                          $": {settings.setSpeedOnSummonValue}");
            }

            GL.EndVertical();
        }

        public static void NoResourceClaimCost()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(
                MenuTools.TextWithTooltip("buttonToggle_NoResourcesClaimCost", "tooltip_NoResourcesClaimCost",
                    $"{settings.toggleNoResourcesClaimCost}" + " ", ""), GL.ExpandWidth(false)))
            {
                if (settings.toggleNoResourcesClaimCost == Storage.isFalseString)
                {
                    KingdomRoot.Instance.DefaultMapResourceCost = 0;
                    settings.toggleNoResourcesClaimCost = Storage.isTrueString;
                }
                else if (settings.toggleNoResourcesClaimCost == Storage.isTrueString)
                {
                    KingdomRoot.Instance.DefaultMapResourceCost = Defaults.defaultMapResourceCost;
                    settings.toggleNoResourcesClaimCost = Storage.isFalseString;
                }
            }
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(NoResourceClaimCost));
            GL.EndVertical();
        }

        private static TextFieldInt AdvanceGameTimeTextField = new TextFieldInt();

        public static void AdvanceGameTime()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.Bold(Strings.GetText("label_AdvanceGameTime")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(AdvanceGameTime));
            GL.Space(10);
            AdvanceGameTimeTextField.RenderField();
            GL.Space(10);
            GL.BeginHorizontal();
            if (GL.Button(
                Strings.GetText("button_AdvanceGameTimeBy") + $" {AdvanceGameTimeTextField.finalAmount} " +
                Strings.GetText("misc_Days"), GL.ExpandWidth(false)))
                Game.Instance.AdvanceGameTime(TimeSpan.FromDays(AdvanceGameTimeTextField.finalAmount));
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void UIOptions()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.MainCategoryFormat(Strings.GetText("label_UI")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(UIOptions));

            MenuTools.ToggleButton(ref settings.toggleShowPetInventory, "buttonToggle_ShowPetInventory",
                "tooltip_ShowPetInventory", nameof(settings.toggleShowPetInventory));
            MenuTools.ToggleButton(ref settings.toggleScaleInventoryMoney, "buttonToggle_ScaleInventoryMoney",
                "tooltip_ScaleInventoryMoney", nameof(settings.toggleScaleInventoryMoney));
            MenuTools.ToggleButton(ref settings.toggleSortSpellbooksAlphabetically,
                "buttonToggle_SortSpellbooksAlphabetically", "tooltip_SortSpellbooksAlphabetically",
                nameof(settings.toggleSortSpellbooksAlphabetically));
            MenuTools.ToggleButton(ref settings.toggleSortSpellsAlphabetically, "buttonToggle_SortSpellsAlphabetically",
                "tooltip_SortSpellsAlphabetically", nameof(settings.toggleSortSpellsAlphabetically));
            MenuTools.ToggleButton(ref settings.toggleUnityModManagerButton, "buttonToggle_UnityModManagerButton",
                "tooltip_UnityModManagerButton", nameof(settings.toggleUnityModManagerButton));
            if (Strings.ToBool(settings.toggleUnityModManagerButton))
            {
                MenuTools.SingleLineLabel(MenuTools.TextWithTooltip("label_UnityModManagerButtonIndex",
                    "tooltip_UnityModManagerButtonIndex", false));
                GL.BeginHorizontal();
                settings.unityModManagerButtonIndex =
                    GL.HorizontalSlider(settings.unityModManagerButtonIndex, 0f, 6f, GL.Width(210f));
                GL.Label($" {Mathf.RoundToInt(settings.unityModManagerButtonIndex)}", GL.ExpandWidth(false));
                GL.EndHorizontal();
            }

            MenuTools.ToggleButton(ref settings.toggleShowAreaName, "buttonToggle_ShowAreaName", "tooltip_ShowAreaName",
                nameof(settings.toggleShowAreaName));

            ShowCombatDifficulty();

            GL.EndVertical();
        }

        public static void ShowCombatDifficulty()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(
                MenuTools.TextWithTooltip("buttonToggle_ShowCombatDifficulty", "tooltip_ShowCombatDifficulty",
                    $"{settings.toggleCombatDifficultyMessage}" + " "), GL.ExpandWidth(false)))
            {
                if (settings.toggleCombatDifficultyMessage == Storage.isFalseString)
                {
                    EventBus.Subscribe(new CombatDifficultyMessage());
                    settings.toggleCombatDifficultyMessage = Storage.isTrueString;
                }
                else if (settings.toggleCombatDifficultyMessage == Storage.isTrueString)
                {
                    settings.toggleCombatDifficultyMessage = Storage.isFalseString;
                }
            }
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(ShowCombatDifficulty));
            GL.EndVertical();
        }

        public static void MiscMods()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.MainCategoryFormat(Strings.GetText("mainCategory_Misc")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(MiscMods));

            RepeatableLockPickingOptions();
            MenuTools.ToggleButton(ref settings.toggleRollHitDiceEachLevel, "buttonToggle_RollHitDiceEachLevel", "tooltip_RollHitDiceEachLevel", nameof(settings.toggleRollHitDiceEachLevel));
            Main.ToggleActiveWarning(ref settings.toggleRollHitDiceEachLevel, ref settings.toggleFullHitdiceEachLevel, "buttonToggle_FullHitdiceEachLevel");
            MenuTools.ToggleButton(ref settings.toggleAlignmentFix, "buttonToggle_AlignmentFix", "tooltip_AlignmentFix", nameof(settings.toggleAlignmentFix));
            MenuTools.ToggleButton(ref settings.togglAutoEquipConsumables, "buttonToggle_AutoEquipConsumables", "tooltip_AutoEquipConsumables", nameof(settings.togglAutoEquipConsumables));
            MenuTools.ToggleButton(ref settings.toggleNoInactiveCamp, "buttonToggle_NoInactiveCampsite", "tooltip_NoInactiveCampsite", nameof(settings.toggleNoInactiveCamp));
            MenuTools.ToggleButton(ref settings.toggleRemoveSummonsGlow, "buttonToggle_RemoveSummonsGlow", "tooltip_RemoveSummonsGlow", nameof(settings.toggleRemoveSummonsGlow));
            MenuTools.ToggleButton(ref settings.toggleSpiderBegone, "buttonToggle_SpiderBegone", "tooltip_SpiderBegone", nameof(settings.toggleSpiderBegone));
            GL.EndVertical();
        }

        public static string isEncounterChanceInCurrentRegionReduced = "";
        public static void RandomEncounterSettings()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.randomEncounterSettingsShow = GL.Toggle(settings.randomEncounterSettingsShow, new GUIContent(RichText.Bold(Strings.GetText("header_RandomEncounterSettings")), Strings.GetText("tooltip_RandomEncounterSettings")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(RandomEncounterSettings));
            if (settings.randomEncounterSettingsShow)
            {
                RandomEcnountersPossible();

                MenuTools.ToggleButton(ref settings.toggleEnableAvoidanceSuccess, "buttonToggle_AlwaysSucceedAtAvoidingEncounters", "tooltip_AlwaysSucceedAtAvoidingEncounters", nameof(settings.toggleEnableAvoidanceSuccess));

                GL.Space(10);

                GL.BeginVertical("box");
                if (KingdomState.Instance != null)
                {
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_RegionREInformation") + " " + Strings.Parenthesis(((IList<BlueprintRegion>)BlueprintRoot.Instance.Kingdom.Regions).FirstOrDefault<BlueprintRegion>((Func<BlueprintRegion, bool>)(r => r.Id == Game.Instance.Player.GlobalMap.CurrentRegion)).LocalizedName)));
                }
                else
                {
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_RegionREInformation")));
                }
                GL.Space(10);
                if (settings.settingShowDebugInfo && KingdomState.Instance != null)
                {
                    MenuTools.SingleLineLabel("REModifierClaimed: " + Game.Instance.Player.Kingdom.REModifierClaimed);
                    MenuTools.SingleLineLabel("REModifierUnclaimed: " + Game.Instance.Player.Kingdom.REModifierUnclaimed);
                    MenuTools.SingleLineLabel("REModifierUpgraded: " + Game.Instance.Player.Kingdom.REModifierUpgraded);
                    MenuTools.SingleLineLabel("IsEncounterChanceInCurrentRegionReduced: " + Game.Instance.Player.REManager.IsEncounterChanceInCurrentRegionReduced);
                    GL.Space(10);

                }
                isEncounterChanceInCurrentRegionReduced = Game.Instance.Player.REManager.IsEncounterChanceInCurrentRegionReduced ? Storage.isTrueString : Storage.isFalseString;
                MenuTools.SingleLineLabel(Strings.GetText("label_EncounterChanceReduced") + ": " + isEncounterChanceInCurrentRegionReduced);
                MenuTools.SingleLineLabel(Strings.GetText("headerOption_ChanceOnGlobalMap") + ": " + Game.Instance.Player.REManager.GlobalMapEncounterChance);
                MenuTools.SingleLineLabel(Strings.GetText("headerOption_HardEncounterChanceOnGlobalMap") + ": " + Game.Instance.Player.REManager.GlobalMapHardEncounterChance);
                MenuTools.SingleLineLabel(Strings.GetText("headerOption_ChanceWhenCamping") + ": " + Game.Instance.Player.REManager.CampEncounterChance);
                GL.EndVertical();
                GL.Space(10);
                MenuTools.SingleLineLabelGT("label_EncounterChanceInformation", true);


                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_SettingsValue") + ": ", GL.ExpandWidth(false));
                settings.randomEncounterSettingFloatAmountLimited =
                    GL.HorizontalSlider(settings.randomEncounterSettingFloatAmountLimited, 0f, 1f, GL.Width(400f));
                GL.Label($" {Math.Round(settings.randomEncounterSettingFloatAmountLimited, 2)}", GL.ExpandWidth(false));
                GL.EndHorizontal();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_ChanceOnGlobalMap") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.ChanceOnGlobalMap + ")", GL.ExpandWidth(true));
                if (GL.Button(
                    Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountLimited})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterChanceOnGlobalMap = settings.randomEncounterSettingFloatAmountLimited;
                    Game.Instance.BlueprintRoot.RE.ChanceOnGlobalMap = settings.randomEncounterChanceOnGlobalMap;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.ChanceOnGlobalMap = Defaults.randomEncounterChanceOnGlobalMap;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_ChanceWhenCamping") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " + Game.Instance.BlueprintRoot.RE.ChanceOnCamp +
                    ")", GL.ExpandWidth(true));
                if (GL.Button(
                    Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountLimited})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterChanceOnCamp = settings.randomEncounterSettingFloatAmountLimited;
                    Game.Instance.BlueprintRoot.RE.ChanceOnCamp = settings.randomEncounterChanceOnCamp;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.ChanceOnCamp = Defaults.randomEncounterChanceOnCamp;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_ChanceWhenCampingAfterAttackDuringRest") + " ",
                    GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.ChanceOnCampSecondTime + ")", GL.ExpandWidth(true));
                if (GL.Button(
                    Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountLimited})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterChanceOnCampSecondTime = settings.randomEncounterSettingFloatAmountLimited;
                    Game.Instance.BlueprintRoot.RE.ChanceOnCampSecondTime =
                        settings.randomEncounterChanceOnCampSecondTime;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.ChanceOnCampSecondTime =
                        Defaults.randomEncounterChanceOnCampSecondTime;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_HardEncounterChance") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.HardEncounterChance + ")", GL.ExpandWidth(true));
                if (GL.Button(
                    Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountLimited})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterHardEncounterChance = settings.randomEncounterSettingFloatAmountLimited;
                    Game.Instance.BlueprintRoot.RE.HardEncounterChance = settings.randomEncounterHardEncounterChance;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.HardEncounterChance = Defaults.randomEncounterHardEncounterChance;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_HardEncounterMaxChance") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.HardEncounterMaxChance + ")", GL.ExpandWidth(true));
                if (GL.Button(
                    Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountLimited})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterHardEncounterMaxChance = settings.randomEncounterSettingFloatAmountLimited;
                    Game.Instance.BlueprintRoot.RE.HardEncounterMaxChance =
                        settings.randomEncounterHardEncounterMaxChance;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.HardEncounterMaxChance =
                        Defaults.randomEncounterHardEncounterMaxChance;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_HardEncounterChanceIncreaseIncrements") + " ",
                    GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.HardEncounterChanceIncrease + ")", GL.ExpandWidth(true));
                if (GL.Button(
                    Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountLimited})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterHardEncounterChanceIncrease =
                        settings.randomEncounterSettingFloatAmountLimited;
                    Game.Instance.BlueprintRoot.RE.HardEncounterChanceIncrease =
                        settings.randomEncounterHardEncounterChanceIncrease;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.HardEncounterChanceIncrease =
                        Defaults.randomEncounterHardEncounterChanceIncrease;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_StalkerAmbushChances") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.StalkerAmbushChance + ")", GL.ExpandWidth(true));
                if (GL.Button(
                    Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountLimited})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterStalkerAmbushChance = settings.randomEncounterSettingFloatAmountLimited;
                    Game.Instance.BlueprintRoot.RE.StalkerAmbushChance = settings.randomEncounterStalkerAmbushChance;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.StalkerAmbushChance = Defaults.randomEncounterStalkerAmbushChance;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_SettingsValue") + ": ", GL.ExpandWidth(false));
                settings.randomEncounterSettingFloatAmount =
                    GL.TextField(settings.randomEncounterSettingFloatAmount, 6, GL.Width(100f));
                settings.randomEncounterSettingFloatAmount =
                    MenuTools.FloatTestSettingStage1(settings.randomEncounterSettingFloatAmount);
                settings.randomEncounterSettingFloatAmountFinal = MenuTools.FloatTestSettingStage2(
                    settings.randomEncounterSettingFloatAmount, settings.randomEncounterSettingFloatAmountFinal);
                GL.EndHorizontal();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_MilesUntilNextEncounterRoll") + " ", GL.ExpandWidth(false));

                GL.Label(
                    Strings.Parenthesis(Strings.GetText("label_CurrentValue") + ": " +
                                        Game.Instance.BlueprintRoot.RE.RollMiles), GL.ExpandWidth(true));
                if (GL.Button(Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountFinal})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterRollMiles = settings.randomEncounterSettingFloatAmountFinal;
                    Game.Instance.BlueprintRoot.RE.RollMiles = settings.randomEncounterRollMiles;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.RollMiles = Defaults.randomEncounterRollMiles;
                GL.EndHorizontal();
                MenuTools.SingleLineLabel(Strings.Parenthesis(Strings.GetText("label_MilesUntilNextEncounterInfo")));
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_SafeMilesAfterEncounter") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.SafeMilesAfterEncounter + ")", GL.ExpandWidth(true));
                if (GL.Button(Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountFinal})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterSafeMilesAfterEncounter = settings.randomEncounterSettingFloatAmountFinal;
                    Game.Instance.BlueprintRoot.RE.SafeMilesAfterEncounter =
                        settings.randomEncounterSafeMilesAfterEncounter;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.SafeMilesAfterEncounter =
                        Defaults.randomEncounterSafeMilesAfterEncounter;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_SafeZoneSize") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.DefaultSafeZoneSize + ")", GL.ExpandWidth(true));
                if (GL.Button(Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountFinal})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterDefaultSafeZoneSize = settings.randomEncounterSettingFloatAmountFinal;
                    Game.Instance.BlueprintRoot.RE.DefaultSafeZoneSize = settings.randomEncounterDefaultSafeZoneSize;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.DefaultSafeZoneSize = Defaults.randomEncounterDefaultSafeZoneSize;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_EncounterPawnOffset") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.EncounterPawnOffset + ")", GL.ExpandWidth(true));
                if (GL.Button(Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountFinal})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterEncounterPawnOffset = settings.randomEncounterSettingFloatAmountFinal;
                    Game.Instance.BlueprintRoot.RE.EncounterPawnOffset = settings.randomEncounterEncounterPawnOffset;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.EncounterPawnOffset = Defaults.randomEncounterEncounterPawnOffset;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_EncounterPawnDistanceFromLocation") + " ",
                    GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.EncounterPawnDistanceFromLocation + ")", GL.ExpandWidth(true));
                if (GL.Button(Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingFloatAmountFinal})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterEncounterPawnDistanceFromLocation =
                        settings.randomEncounterSettingFloatAmountFinal;
                    Game.Instance.BlueprintRoot.RE.EncounterPawnDistanceFromLocation =
                        settings.randomEncounterEncounterPawnDistanceFromLocation;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.EncounterPawnDistanceFromLocation =
                        Defaults.randomEncounterEncounterPawnDistanceFromLocation;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_SettingsValue") + ": ", GL.ExpandWidth(false));
                settings.randomEncounterSettingIntAmount =
                    GL.TextField(settings.randomEncounterSettingIntAmount, 6, GL.Width(100f));
                MenuTools.SettingParse(ref settings.randomEncounterSettingIntAmount,
                    ref settings.randomEncounterSettingIntAmountFinal);
                GL.EndHorizontal();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_AvoidanceFailMargin") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.RandomEncounterAvoidanceFailMargin + ")", GL.ExpandWidth(true));
                if (GL.Button(Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingIntAmountFinal})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterRandomEncounterAvoidanceFailMargin =
                        settings.randomEncounterSettingIntAmountFinal;
                    Game.Instance.BlueprintRoot.RE.RandomEncounterAvoidanceFailMargin =
                        settings.randomEncounterRandomEncounterAvoidanceFailMargin;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.RandomEncounterAvoidanceFailMargin =
                        Defaults.randomEncounterRandomEncounterAvoidanceFailMargin;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_MinimalChallengeRatingBonus") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.EncounterMinBonusCR + ")", GL.ExpandWidth(true));
                if (GL.Button(Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingIntAmountFinal})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterEncounterMinBonusCR = settings.randomEncounterSettingIntAmountFinal;
                    Game.Instance.BlueprintRoot.RE.EncounterMinBonusCR = settings.randomEncounterEncounterMinBonusCR;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.EncounterMinBonusCR = Defaults.randomEncounterEncounterMinBonusCR;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_MaximalChallengeRatingBonus") + " ", GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.EncounterMaxBonusCR + ")", GL.ExpandWidth(true));
                if (GL.Button(Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingIntAmountFinal})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterEncounterMaxBonusCR = settings.randomEncounterSettingIntAmountFinal;
                    Game.Instance.BlueprintRoot.RE.EncounterMaxBonusCR = settings.randomEncounterEncounterMaxBonusCR;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.EncounterMaxBonusCR = Defaults.randomEncounterEncounterMaxBonusCR;
                GL.EndHorizontal();
                GL.EndVertical();

                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_HardEncounterChallengeRatingBonus") + " ",
                    GL.ExpandWidth(false));

                GL.Label(
                    "(" + Strings.GetText("label_CurrentValue") + ": " +
                    Game.Instance.BlueprintRoot.RE.HardEncounterBonusCR + ")", GL.ExpandWidth(true));
                if (GL.Button(Strings.GetText("button_Apply") + $"({settings.randomEncounterSettingIntAmountFinal})",
                    GL.ExpandWidth(false)))
                {
                    settings.randomEncounterHardEncounterBonusCR = settings.randomEncounterSettingIntAmountFinal;
                    Game.Instance.BlueprintRoot.RE.HardEncounterBonusCR = settings.randomEncounterHardEncounterBonusCR;
                }

                if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    Game.Instance.BlueprintRoot.RE.HardEncounterBonusCR = Defaults.randomEncounterHardEncounterBonusCR;
                GL.EndHorizontal();
                GL.EndVertical();
            }
            GL.EndVertical();
        }

        public static void RandomEcnountersPossible()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.Bold(Strings.GetText("warning_RandomEncounterPossible")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(RandomEcnountersPossible));
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("buttonToggle_RandomEncounterPossible", "tooltip_RandomEncounterPossible", $"{settings.toggleRandomEncountersEnabled}" + " ", ""), GL.ExpandWidth(false)))

            {
                if (settings.toggleRandomEncountersEnabled == Storage.isFalseString)
                {
                    Game.Instance.BlueprintRoot.RE.EncountersEnabled = true;
                    settings.toggleRandomEncountersEnabled = Storage.isTrueString;
                }
                else if (settings.toggleRandomEncountersEnabled == Storage.isTrueString)
                {
                    Game.Instance.BlueprintRoot.RE.EncountersEnabled = false;
                    settings.toggleRandomEncountersEnabled = Storage.isFalseString;
                }
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static SelectionGrid passSavingThrowIndividualGrid = new SelectionGrid(Storage.unitEntityDataSelectionGridArray, 4);

        public static void PassSavingThrowIndividual()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.togglePassSavingThrowIndividual,
                "buttonToggle_PassSavingThrowIndividual", "tooltip_PassSavingThrowIndividual", true);
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(PassSkillChecksIndividual));
            GL.EndHorizontal();

            if (Strings.ToBool(settings.togglePassSavingThrowIndividual))
            {
                passSavingThrowIndividualGrid.Render(ref settings.indexPassSavingThrowIndividuall);

                for (var i = 0; i < Main.Settings.togglePassSavingThrowIndividualArray.Count(); i++)
                {
                    GL.BeginHorizontal();
                    if (GL.Button(
                        Main.Settings.togglePassSavingThrowIndividualArray[i] + " " +
                        Strings.GetText("misc_AlwaysPass") + " " + Storage.individualSavesArray[i] + " " +
                        Strings.GetText("misc_Saves"), GL.ExpandWidth(false)))
                    {
                        if (Main.Settings.togglePassSavingThrowIndividualArray[i] == Storage.isFalseString)
                            Main.Settings.togglePassSavingThrowIndividualArray[i] = Storage.isTrueString;
                        else if (Main.Settings.togglePassSavingThrowIndividualArray[i] == Storage.isTrueString)
                            Main.Settings.togglePassSavingThrowIndividualArray[i] = Storage.isFalseString;
                    }

                    GL.EndHorizontal();
                }
            }

            GL.EndVertical();
        }


        // to-do: simplify multiplier methods and add unitEntitiySelectionGrid to relevant options
        public static SelectionGrid BuffDurationMultiplierGrid =
            new SelectionGrid(Storage.unitEntityDataSelectionGridArray, 4);

        public static void BuffDurationMultiplier()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleBuffDurationMultiplier,
                "buttonToggle_BuffDurationMultiplier", "tooltip_BuffDurationMultiplier");
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(BuffDurationMultiplier));
            GL.EndHorizontal();

            if (Strings.ToBool(settings.toggleBuffDurationMultiplier))
            {
                MenuTools.SingleLineLabel(Strings.GetText("label_BuffDurationMultiplier"));
                BuffDurationMultiplierGrid.Render(ref settings.indexBuffDurationMultiplier);


                MenuTools.SettingsValueFloat(ref Storage.buffDurationMultiplierValue,
                    ref Storage.finalBuffDurationMultiplierValue);

                MenuTools.SingleLineLabel(Strings.GetText("label_CurrentMultiplier") +
                                          $": {settings.finalBuffDurationMultiplierValue}");
                GL.BeginHorizontal();
                if (GL.Button(
                    Strings.GetText("button_SetTo") + $" {Math.Round(Storage.finalBuffDurationMultiplierValue, 3)}",
                    GL.ExpandWidth(false)))
                    settings.finalBuffDurationMultiplierValue =
                        (float)Math.Round(Storage.finalBuffDurationMultiplierValue, 3);
                GL.EndHorizontal();
            }

            GL.EndVertical();
        }

        public static SelectionGrid summonDurationMultiplierGrid =
            new SelectionGrid(Storage.unitEntityDataSelectionGridArray, 4);

        public static void SummonDurationMultiplier()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleSummonDurationMultiplier,
                "buttonToggle_SummonDurationMultiplier", "tooltip_SummonDurationMultiplier");
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(SummonDurationMultiplier));
            GL.EndHorizontal();

            if (Strings.ToBool(settings.toggleSummonDurationMultiplier))
            {
                MenuTools.SingleLineLabel(Strings.GetText("label_SummonDurationMultiplier"));
                summonDurationMultiplierGrid.Render(ref settings.indexSummonDurationMultiplier);


                MenuTools.SettingsValueFloat(ref Storage.summonDurationMultiplierValue,
                    ref Storage.finalSummonDurationMultiplierValue);

                MenuTools.SingleLineLabel(Strings.GetText("label_CurrentMultiplier") +
                                          $": {settings.finalSummonDurationMultiplierValue}");
                GL.BeginHorizontal();
                if (GL.Button(
                    Strings.GetText("button_SetTo") + $" {Math.Round(Storage.finalSummonDurationMultiplierValue, 3)}",
                    GL.ExpandWidth(false)))
                    settings.finalSummonDurationMultiplierValue =
                        (float)Math.Round(Storage.finalSummonDurationMultiplierValue, 3);
                GL.EndHorizontal();
            }

            GL.EndVertical();
        }

        public static SelectionGrid setSummonLevelTo20Grid =
            new SelectionGrid(Storage.unitEntityDataSelectionGridArray, 4);

        public static void SetSummonLevelTo20()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleSetSummonLevelTo20,
                "buttonToggle_SetSummonLevelTo20", "tooltip_SetSummonLevelTo20");
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(SetSummonLevelTo20));
            GL.EndHorizontal();

            if (Strings.ToBool(settings.toggleSetSummonLevelTo20))
            {
                MenuTools.SingleLineLabel(Strings.GetText("label_SetSummonLevelTo20"));
                setSummonLevelTo20Grid.Render(ref settings.indexSetSummonLevelTo20);
            }

            GL.EndVertical();
        }

        public static SelectionGrid noAttacksOfOpportunityGrid =
            new SelectionGrid(Storage.unitEntityDataSelectionGridArray, 4);

        public static void NoAttacksOfOpportunity()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleNoAttacksOfOpportunity,
                "buttonToggle_NoAttacksOfOpportunity", "tooltip_NoAttacksOfOpportunity");
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(NoAttacksOfOpportunity));
            GL.EndHorizontal();


            if (Strings.ToBool(settings.toggleNoAttacksOfOpportunity))
            {
                MenuTools.SingleLineLabel(Strings.GetText("label_NoAttacksOfOpportunity"));
                noAttacksOfOpportunityGrid.Render(ref settings.indexNoAttacksOfOpportunity);
            }

            GL.EndVertical();
        }

        public static TextFieldInt setRankForceTextField = new TextFieldInt();

        public static void Feats()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showFeatsCategory = GL.Toggle(settings.showFeatsCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_Feats")), GL.ExpandWidth(false));
            if (!settings.showFeatsCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("Feats");

                GL.Space(10);

                MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_Feats_0")));
                MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_Feats_1")));

                GL.Space(10);

                GL.BeginHorizontal();
                Storage.featsFilterUnitEntityDataIndex = GL.SelectionGrid(Storage.featsFilterUnitEntityDataIndex, Storage.unitEntityDataArray, 3);
                GL.EndHorizontal();

                var player = Game.Instance.Player;

                switch (Storage.featsFilterUnitEntityDataIndex)
                {
                    case 0:
                        Storage.featsUnitEntityData = player.Party;
                        break;
                    case 1:
                        Storage.featsUnitEntityData = player.ControllableCharacters;
                        break;
                    case 2:
                        Storage.featsUnitEntityData = player.ActiveCompanions;
                        break;
                    case 3:
                        Storage.featsUnitEntityData = player.AllCharacters;
                        break;
                    case 4:
                        Storage.featsUnitEntityData = Common.GetCustomCompanions();
                        break;
                    case 5:
                        Storage.featsUnitEntityData = Common.GetPets();
                        break;
                    case 6:
                        Storage.featsUnitEntityData = Common.GetEnemies();
                        break;
                }

                if (Storage.featsFilterUnitEntityDataIndex != Storage.featsFilterUnitEntityDataIndexOld)
                {
                    Storage.reloadPartyFeats = true;
                    Storage.featsFilterUnitEntityDataIndexOld = Storage.featsFilterUnitEntityDataIndex;
                }

                if (Storage.featsUnitEntityData.Count != Storage.featsAllUnits.Count || Storage.reloadPartyFeats)
                {
                    Storage.featsSelectedControllableCharacterIndex = 0;
                    Storage.featsAllUnitsNamesList.Clear();
                    foreach (var controllableCharacter in Storage.featsUnitEntityData)
                        Storage.featsAllUnitsNamesList.Add(controllableCharacter.CharacterName);
                    Storage.featsAllUnits = Storage.featsUnitEntityData;
                    Storage.reloadPartyFeats = false;
                }

                if (Storage.featsUnitEntityData.Count - 1 < Storage.featsSelectedControllableCharacterIndex)
                    Storage.featsSelectedControllableCharacterIndex = Storage.featsUnitEntityData.Count - 1;

                if (Storage.featsUnitEntityData.Any())
                {
                    if (!Storage.reloadPartyFeats)
                    {
                        GL.Space(10);

                        GL.BeginHorizontal();
                        Storage.featsSelectedControllableCharacterIndex = GL.SelectionGrid(
                            Storage.featsSelectedControllableCharacterIndex, Storage.featsAllUnitsNamesList.ToArray(),
                            6);
                        GL.EndHorizontal();

                        GL.Space(10);

                        if (GL.Button(
                            $"{settings.showAllFeats} " + Strings.GetText("button_AllFeats") +
                            $" ({Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]})",
                            GL.ExpandWidth(false)))
                        {
                            if (settings.showAllFeats == Strings.GetText("misc_Hide"))
                            {
                                settings.showAllFeats = Strings.GetText("misc_Display");
                            }
                            else if (settings.showAllFeats == Strings.GetText("misc_Display"))
                            {
                                settings.showAllFeats = Strings.GetText("misc_Hide");
                                Storage.featAllLoad = true;
                            }
                            else
                            {
                                settings.showAllFeats = Strings.GetText("misc_Display");
                            }
                        }

                        if (settings.showAllFeats == Strings.GetText("misc_Hide"))
                        {
                            var unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                            if (unitEntityData != Storage.featAllUnitEntityData)
                            {
                                Storage.featAllUnitEntityData = unitEntityData;
                                Storage.featAllLoad = true;
                            }

                            if (Storage.featAllLoad == true)
                            {
                                Main.RefreshAllFeats(unitEntityData);
                                Storage.featAllLoad = false;
                            }

                            for (var i = 0; i < unitEntityData.Descriptor.Progression.Features.Count; i++)
                            {
                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label(Strings.GetText("label_FeatName") + $": {Storage.featAllNames[i]}");
                                GL.FlexibleSpace();
                                if (Storage.featFavourites.Contains(unitEntityData.Descriptor.Progression.Features[i]
                                    .Blueprint.AssetGuid))
                                {
                                    if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                    {
                                        Storage.featFavourites.Remove(unitEntityData.Descriptor.Progression.Features[i]
                                            .Blueprint.AssetGuid);
                                        Storage.featFavouritesLoad = true;
                                    }
                                }
                                else
                                {
                                    if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                                    {
                                        Storage.featFavourites.Add(unitEntityData.Descriptor.Progression.Features[i]
                                            .Blueprint.AssetGuid);
                                        Storage.featFavouritesLoad = true;
                                    }
                                }

                                GL.EndHorizontal();
                                MenuTools.SingleLineLabel(Strings.GetText("label_FeatObjectName") +
                                                          $": {Storage.featAllObjectNames[i]}");
                                GL.BeginVertical("box");
                                MenuTools.SingleLineLabel(Strings.GetText("label_FeatRank") + $": {Storage.featAllRanks[i]}");
                                setRankForceTextField.RenderField();
                                GL.BeginHorizontal();
                                if (GL.Button(
                                    Strings.GetText("button_SetRankTo") + $" {setRankForceTextField.finalAmount}",
                                    GL.ExpandWidth(false)))
                                {
                                    var feat = unitEntityData.Descriptor.Progression.Features[i] as Feature;
                                    feat.SetRankForce(setRankForceTextField.finalAmount);
                                    Storage.featAllLoad = true;
                                }

                                GL.EndHorizontal();
                                GL.EndVertical();
                                MenuTools.SingleLineLabel(Strings.GetText("label_FeatBlueprintAssetGuid") +
                                                          $": {Storage.featAllGuids[i]}");
                                MenuTools.SingleLineLabel(Strings.GetText("label_FeatDescription") +
                                                          $": {Storage.featAllDescriptions[i]}");

                                if (GL.Button(
                                    Strings.GetText("label_Remove") +
                                    $" {unitEntityData.Descriptor.Progression.Features[i].Blueprint.name}",
                                    GL.ExpandWidth(false)))
                                {
                                    unitEntityData.Descriptor.Progression.Features.RemoveFact(
                                        (BlueprintUnitFact)unitEntityData.Descriptor.Progression.Features[i]
                                            .Blueprint);
                                    Storage.featAllLoad = true;
                                }

                                GL.EndVertical();
                            }
                        }

                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        settings.showFeatsFavourites = GL.Toggle(settings.showFeatsFavourites, RichText.Bold(Strings.GetText("headerOption_ShowFavourites")), GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        if (settings.showFeatsFavourites == true)
                        {
                            if (Storage.featFavouritesLoad == true)
                            {
                                Main.RefreshFeatFavourites();
                                Storage.featFavouritesLoad = false;
                            }

                            if (!Storage.featFavourites.Any())
                            {
                                MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                            }
                            else
                            {
                                for (var i = 0; i < Storage.featFavourites.Count; i++)
                                {
                                    GL.BeginHorizontal();
                                    Storage.featToggleFavouriteDescription.Add(false);
                                    Storage.featToggleFavouriteDescription[i] = GL.Toggle(Storage.featToggleFavouriteDescription[i], $"{Storage.featFavouritesNames[i]}", GL.ExpandWidth(false));

                                    if (Main.CraftMagicItems.ModIsActive() && Storage.featFavourites[i].Contains(Storage.craftMagicItemsBlueprintPrefix))
                                        GL.Label(Strings.Parenthesis(Storage.craftMagicItemsBlueprintPrefix), GL.ExpandWidth(false));

                                    GL.FlexibleSpace();

                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + $" {Storage.featFavouritesNames[i]} " +
                                            Strings.GetText("misc_To") +
                                            $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(Utilities.GetBlueprintByGuid<BlueprintFeature>(Storage.featFavouritesGuids[i]), null, new FeatureParam());
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }

                                    if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                    {
                                        Storage.featFavouritesGuids.Remove(Storage.featFavouritesGuids[i]);
                                        Storage.featFavouritesLoad = true;
                                    }
                                    GL.EndHorizontal();

                                    FeatDetails(Storage.featToggleFavouriteDescription[i], Storage.featFavouritesGuids[i], Storage.featFavouritesDescriptions[i]);
                                }

                                GL.Space(10);

                                if (GL.Button(
                                    Strings.GetText("button_AddFavouritesTo") +
                                    $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                    GL.ExpandWidth(false)))
                                    for (var i = 0; i < Storage.featFavouritesGuids.Count; i++)
                                        if (Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                Storage.featFavouritesGuids[i]) != null)
                                        {
                                            var unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(Utilities.GetBlueprintByGuid<BlueprintFeature>(Storage.featFavouritesGuids[i]), null, new FeatureParam());
                                            Storage.featAllLoad = true;
                                        }

                                MenuTools.ExportCopyGuidsNamesButtons(Storage.featFavouritesGuids.ToArray(), Storage.featFavouritesNames.ToArray(), "feat-favourites");
                            }

                            if (Storage.featFavourites != Storage.featFavouritesGuids)
                                Storage.featFavourites = Storage.featFavouritesGuids;
                        }

                        GL.EndVertical();

                        GL.Space(10);

                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_ParametrizedFeats")));

                        GL.BeginHorizontal();
                        Storage.featsParamIndex = GL.SelectionGrid(Storage.featsParamIndex, Storage.featsParamArray, 3);
                        GL.EndHorizontal();

                        if (Storage.featsParamIndex == 1 || Storage.featsParamIndex == 3 ||
                            Storage.featsParamIndex == 4)
                        {
                            GL.BeginHorizontal();
                            settings.featsParamShowAllWeapons = GL.Toggle(settings.featsParamShowAllWeapons, new GUIContent(Strings.GetText("label_ShowAllWeaponCategories"), Strings.GetText("tooltip_ShowAllWeaponCategories")),
                                GL.ExpandWidth(false));
                            GL.EndHorizontal();
                        }

                        switch (Storage.featsParamIndex)
                        {
                            case 0:
                                break;
                            case 1:
                                foreach (var weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory)))
                                    if (settings.featsParamShowAllWeapons || weapon.HasSubCategory(WeaponSubCategory.OneHandedPiercing))
                                        try
                                        {
                                            if (GL.Button(
                                                Strings.GetText("misc_Add") + " " +
                                                Strings.GetText("arrayItem_FeatParam_FencingGrace") + " - " + weapon +
                                                " " + Strings.GetText("misc_To") +
                                                $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                                GL.ExpandWidth(false)))
                                            {
                                                var featureParam = new FeatureParam(weapon);
                                                var unitEntityData =
                                                    Storage.featsAllUnits[
                                                        Storage.featsSelectedControllableCharacterIndex];
                                                unitEntityData.Descriptor.AddFact(
                                                    Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                        "47b352ea0f73c354aba777945760b441"), null, featureParam);
                                                Storage.featAllLoad = true;
                                            }
                                        }
                                        catch (IndexOutOfRangeException e)
                                        {
                                            modLogger.Log(e.ToString());
                                            modLogger.Log("Forcing refresh");
                                            Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                        }
                                break;
                            case 2:
                                foreach (var weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory)))
                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + " " +
                                            Strings.GetText("arrayItem_FeatParam_ImprovedCritical") + " - " +
                                            weapon.ToString() + " " + Strings.GetText("misc_To") +
                                            $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var featureParam = new FeatureParam(weapon);
                                            var unitEntityData =
                                                Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                    "f4201c85a991369408740c6888362e20"), (MechanicsContext)null,
                                                featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }
                                break;
                            case 3:
                                foreach (var weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory)))
                                    if (settings.featsParamShowAllWeapons || weapon.HasSubCategory(WeaponSubCategory.OneHandedSlashing))
                                        try
                                        {
                                            if (GL.Button(
                                                Strings.GetText("misc_Add") + " " +
                                                Strings.GetText("arrayItem_FeatParam_SlashingGrace") + " - " +
                                                weapon + " " + Strings.GetText("misc_To") +
                                                $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                                GL.ExpandWidth(false)))
                                            {
                                                var featureParam = new FeatureParam(weapon);
                                                var unitEntityData =
                                                    Storage.featsAllUnits[
                                                        Storage.featsSelectedControllableCharacterIndex];
                                                unitEntityData.Descriptor.AddFact(
                                                    Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                        "697d64669eb2c0543abb9c9b07998a38"), null, featureParam);
                                                Storage.featAllLoad = true;
                                            }
                                        }
                                        catch (IndexOutOfRangeException e)
                                        {
                                            modLogger.Log(e.ToString());
                                            modLogger.Log("Forcing refresh");
                                            Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                        }
                                break;
                            case 4:
                                foreach (var weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory)))
                                    if (settings.featsParamShowAllWeapons || weapon.HasSubCategory(WeaponSubCategory.Melee))
                                        try
                                        {
                                            if (GL.Button(
                                                Strings.GetText("misc_Add") + " " +
                                                Strings.GetText("arrayItem_FeatParam_SwordSaintChosenWeapon") +
                                                " - " + weapon + " " + Strings.GetText("misc_To") +
                                                $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                                GL.ExpandWidth(false)))
                                            {
                                                var featureParam = new FeatureParam(weapon);
                                                var unitEntityData =
                                                    Storage.featsAllUnits[
                                                        Storage.featsSelectedControllableCharacterIndex];
                                                unitEntityData.Descriptor.AddFact(
                                                    Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                        "c0b4ec0175e3ff940a45fc21f318a39a"), null, featureParam);
                                                Storage.featAllLoad = true;
                                            }
                                        }
                                        catch (IndexOutOfRangeException e)
                                        {
                                            modLogger.Log(e.ToString());
                                            modLogger.Log("Forcing refresh");
                                            Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                        }
                                break;
                            case 5:
                                foreach (var weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory)))
                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + " " +
                                            Strings.GetText("arrayItem_FeatParam_WeaponFocus") + " - " +
                                            weapon.ToString() + " " + Strings.GetText("misc_To") +
                                            $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var featureParam = new FeatureParam(weapon);
                                            var unitEntityData =
                                                Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                    "1e1f627d26ad36f43bbd26cc2bf8ac7e"), (MechanicsContext)null,
                                                featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }

                                break;
                            case 6:
                                foreach (var weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory)))
                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + " " +
                                            Strings.GetText("arrayItem_FeatParam_WeaponFocusGreater") + " - " +
                                            weapon.ToString() + " " + Strings.GetText("misc_To") +
                                            $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var featureParam = new FeatureParam(weapon);
                                            var unitEntityData =
                                                Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                    "09c9e82965fb4334b984a1e9df3bd088"), (MechanicsContext)null,
                                                featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }

                                break;
                            case 7:
                                foreach (var weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory)))
                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + " " +
                                            Strings.GetText("arrayItem_FeatParam_WeaponMastery") + " - " +
                                            weapon.ToString() + " " + Strings.GetText("misc_To") +
                                            $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var featureParam = new FeatureParam(weapon);
                                            var unitEntityData =
                                                Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                    "38ae5ac04463a8947b7c06a6c72dd6bb"), (MechanicsContext)null,
                                                featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }

                                break;
                            case 8:
                                foreach (var weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory)))
                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + " " +
                                            Strings.GetText("arrayItem_FeatParam_WeaponSpecialization") + " - " +
                                            weapon.ToString() + " " + Strings.GetText("misc_To") +
                                            $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var featureParam = new FeatureParam(weapon);
                                            var unitEntityData =
                                                Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                    "31470b17e8446ae4ea0dacd6c5817d86"), (MechanicsContext)null,
                                                featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }

                                break;
                            case 9:
                                foreach (var weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory)))
                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + " " +
                                            Strings.GetText("arrayItem_FeatParam_WeaponSpecializationGreater") + " - " +
                                            weapon.ToString() + " " + Strings.GetText("misc_To") +
                                            $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var featureParam = new FeatureParam(weapon);
                                            var unitEntityData =
                                                Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                    "7cf5edc65e785a24f9cf93af987d66b3"), (MechanicsContext)null,
                                                featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }

                                break;
                            case 10:
                                foreach (var weapon in (SpellSchool[])Enum.GetValues(typeof(SpellSchool)))
                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + " " +
                                            Strings.GetText("arrayItem_FeatParam_SpellFocus") + " - " +
                                            weapon.ToString() + " " + Strings.GetText("misc_To") +
                                            $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var featureParam = new FeatureParam(weapon);
                                            var unitEntityData =
                                                Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                    "16fa59cc9a72a6043b566b49184f53fe"), (MechanicsContext)null,
                                                featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }

                                break;
                            case 11:
                                foreach (var weapon in (SpellSchool[])Enum.GetValues(typeof(SpellSchool)))
                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + " " +
                                            Strings.GetText("arrayItem_FeatParam_GreaterSpellFocus") + " - " +
                                            weapon.ToString() + " " + Strings.GetText("misc_To") +
                                            $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var featureParam = new FeatureParam(weapon);
                                            var unitEntityData =
                                                Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                    "5b04b45b228461c43bad768eb0f7c7bf"), (MechanicsContext)null,
                                                featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }

                                break;
                        }

                        GL.Space(10);

                        GL.BeginVertical("box");

                        GL.BeginHorizontal();
                        settings.featSearch = GL.TextField(settings.featSearch, GL.Width(500f));
                        if (GL.Button(RichText.Bold(Strings.GetText("header_Search")), GL.ExpandWidth(false)))
                            Main.SearchValidFeats(Storage.validFeatTypes);
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_SearchBy") + ": ", GL.ExpandWidth(false));

                        if (GL.Button($"{settings.toggleSearchByFeatObjectName} " + Strings.GetText("label_ObjectName"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByFeatObjectName == Storage.isTrueString)
                                settings.toggleSearchByFeatObjectName = Storage.isFalseString;
                            else
                                settings.toggleSearchByFeatObjectName = Storage.isTrueString;
                            Main.SearchValidFeats(Storage.validFeatTypes);
                        }

                        if (GL.Button($"{settings.toggleSearchByFeatName} " + Strings.GetText("label_FeatName"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByFeatName == Storage.isTrueString)
                                settings.toggleSearchByFeatName = Storage.isFalseString;
                            else
                                settings.toggleSearchByFeatName = Storage.isTrueString;
                            Main.SearchValidFeats(Storage.validFeatTypes);
                        }

                        if (GL.Button(
                            $"{settings.toggleSearchByFeatDescription} " + Strings.GetText("label_FeatDescription"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByFeatDescription == Storage.isTrueString)
                                settings.toggleSearchByFeatDescription = Storage.isFalseString;
                            else
                                settings.toggleSearchByFeatDescription = Storage.isTrueString;
                            Main.SearchValidFeats(Storage.validFeatTypes);
                        }

                        GL.EndHorizontal();

                        MenuTools.SingleLineLabel(Strings.GetText("label_SearchInfoParametrizedFeats"));

                        if (!Storage.featResultNames.Any())
                        {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoResult"));
                        }
                        else
                        {
                            if (Storage.featResultNames.Count > settings.finalResultLimit)
                            {
                                MenuTools.SingleLineLabel($"{Storage.featResultNames.Count} " +
                                                          Strings.GetText("label_Results"));
                                MenuTools.SingleLineLabel(Strings.GetText("label_TooManyResults_0") +
                                                          $" {settings.finalResultLimit} " +
                                                          Strings.GetText("label_TooManyResults_1"));
                                GL.Space(10);
                            }
                            else
                            {
                                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_Results") + ":"));
                                for (var i = 0; i < Storage.featResultNames.Count; i++)
                                {
                                    GL.BeginHorizontal();
                                    Storage.featToggleResultDescription.Add(false);
                                    if (!string.IsNullOrEmpty(Storage.featResultNames[i]))
                                        Storage.featToggleResultDescription[i] = GL.Toggle(Storage.featToggleResultDescription[i], Storage.featResultNames[i], GL.ExpandWidth(false));

                                    GL.FlexibleSpace();
                                    if (Main.CraftMagicItems.ModIsActive() && Storage.featResultGuids[i]
                                            .Contains(Storage.craftMagicItemsBlueprintPrefix))
                                        GL.Label(Strings.Parenthesis(Storage.craftMagicItemsBlueprintPrefix),
                                            GL.ExpandWidth(false));

                                    if (GL.Button(
                                        Strings.GetText("misc_Add") + $" {Storage.featResultNames[i]} " +
                                        Strings.GetText("misc_To") +
                                        $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}",
                                        GL.ExpandWidth(false)))
                                    {
                                        var unitEntityData =
                                            Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];

                                        unitEntityData.Descriptor.AddFact(
                                            (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(
                                                Storage.featResultGuids[i]), (MechanicsContext)null,
                                            new FeatureParam());

                                        Storage.featAllLoad = true;
                                    }

                                    if (Storage.featFavourites.Contains(Storage.featResultGuids[i]))
                                    {
                                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                        {
                                            Storage.featFavourites.Remove(Storage.featResultGuids[i]);
                                            Storage.featFavouritesLoad = true;
                                        }
                                    }
                                    else
                                    {
                                        if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                                        {
                                            Storage.featFavourites.Add(Storage.featResultGuids[i]);
                                            Storage.featFavouritesLoad = true;
                                        }
                                    }
                                    GL.EndHorizontal();

                                    FeatDetails(Storage.featToggleResultDescription[i], Storage.featResultGuids[i], Storage.featResultDescriptions[i]);
                                }

                                var filename = "feat-" + Regex.Replace(Storage.currentFeatSearch, @"[\\/:*?""<>|]", "");
                                MenuTools.ExportCopyGuidsNamesButtons(Storage.featResultGuids.ToArray(), Storage.featResultNames.ToArray(), filename);
                            }
                        }

                        GL.EndVertical();
                    }
                }
                else
                {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitFound"));
                }
            }

            GL.EndVertical();
        }

        private static bool showAddSpellbooks = false;
        private static bool showRemoveSpellbook = false;

        public static void SpellsAndSpellbooks()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showSpellSpellbooksCategory = GL.Toggle(settings.showSpellSpellbooksCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_SpellsAndSpellbooks")), GL.ExpandWidth(false));
            if (!settings.showSpellSpellbooksCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("SpellsAndSpellbooks");

                GL.Space(10);


                GL.BeginHorizontal();
                Storage.spellSpellbooksFilterUnitEntityDataIndex = GL.SelectionGrid(Storage.spellSpellbooksFilterUnitEntityDataIndex, Storage.unitEntityDataArray, 3);
                GL.EndHorizontal();

                var player = Game.Instance.Player;

                switch (Storage.spellSpellbooksFilterUnitEntityDataIndex)
                {
                    case 0:
                        Storage.spellSpellbooksUnitEntityData = player.Party;
                        break;
                    case 1:
                        Storage.spellSpellbooksUnitEntityData = player.ControllableCharacters;
                        break;
                    case 2:
                        Storage.spellSpellbooksUnitEntityData = player.ActiveCompanions;
                        break;
                    case 3:
                        Storage.spellSpellbooksUnitEntityData = player.AllCharacters;
                        break;
                    case 4:
                        Storage.spellSpellbooksUnitEntityData = Common.GetCustomCompanions();
                        break;
                    case 5:
                        Storage.spellSpellbooksUnitEntityData = Common.GetPets();
                        break;
                    case 6:
                        Storage.spellSpellbooksUnitEntityData = Common.GetEnemies();
                        break;
                }

                if (Storage.spellSpellbooksFilterUnitEntityDataIndex !=
                    Storage.spellSpellbooksFilterUnitEntityDataIndexOld)
                {
                    Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                    Storage.reloadSpellSpellbooksLearnableSpells = true;
                    Storage.reloadPartySpellSpellbookSpellbooks = true;
                    Storage.reloadPartySpellSpellbookChars = true;
                    Storage.spellSpellbooksFilterUnitEntityDataIndexOld =
                        Storage.spellSpellbooksFilterUnitEntityDataIndex;
                }

                if (Storage.spellSpellbooksUnitEntityData.Count != Storage.spellSpellbooksAllUnits.Count ||
                    Storage.reloadPartySpellSpellbookChars)
                {
                    Storage.spellSpellbooksSelectedControllableCharacterIndex = 0;
                    Storage.spellSpellbooksAllUnitsNamesList.Clear();
                    foreach (var controllableCharacter in Storage.spellSpellbooksUnitEntityData)
                        Storage.spellSpellbooksAllUnitsNamesList.Add(controllableCharacter.CharacterName);
                    Storage.spellSpellbooksAllUnits = Storage.spellSpellbooksUnitEntityData;
                    Storage.reloadPartySpellSpellbookChars = false;
                }

                if (Storage.spellSpellbooksUnitEntityData.Count - 1 <
                    Storage.spellSpellbooksSelectedControllableCharacterIndex)
                    Storage.spellSpellbooksSelectedControllableCharacterIndex =
                        Storage.spellSpellbooksUnitEntityData.Count - 1;

                if (Storage.spellSpellbooksUnitEntityData.Any())
                {
                    if (!Storage.reloadPartySpellSpellbookChars)
                    {
                        GL.Space(10);

                        GL.BeginHorizontal();
                        Storage.spellSpellbooksSelectedControllableCharacterIndex = GL.SelectionGrid(Storage.spellSpellbooksSelectedControllableCharacterIndex, Storage.spellSpellbooksAllUnitsNamesList.ToArray(), 3);
                        GL.EndHorizontal();

                        if (Storage.spellSpellbooksSelectedControllableCharacterIndex !=
                            Storage.spellSpellbooksSelectedControllableCharacterIndexOld)
                        {
                            Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                            Storage.reloadSpellSpellbooksLearnableSpells = true;
                            Storage.reloadPartySpellSpellbookSpellbooks = true;
                            Storage.spellSpellbooksSelectedControllableCharacterIndexOld =
                                Storage.spellSpellbooksSelectedControllableCharacterIndex;
                        }

                        if (Storage.spellSpellbooksSavedSpellbooks.Count !=
                            Storage.spellSpellbooksSavedSpellbooksCount || Storage.reloadPartySpellSpellbookSpellbooks)
                        {
                            Storage.spellSpellbooksSelectedSavedSpellbooksIndex = 0;
                            Storage.spellSpellbooksSavedSpellbooks.Clear();
                            Storage.spellSpellbooksSavedSpellbooksNames.Clear();
                            var spellbooks = Storage.spellSpellbooksUnitEntityData[Storage.spellSpellbooksSelectedControllableCharacterIndex].Descriptor.Spellbooks;
                            foreach (var spellbook in spellbooks)
                            {
                                Storage.spellSpellbooksSavedSpellbooks.Add(spellbook);
                                Storage.spellSpellbooksSavedSpellbooksNames.Add(spellbook.Blueprint.DisplayName);
                            }

                            Storage.spellSpellbooksSavedSpellbooksCount = Storage.spellSpellbooksSavedSpellbooks.Count;
                            Storage.reloadPartySpellSpellbookSpellbooks = false;

                            Storage.spellSpellbooksSavedSpellbooksNames.Sort();
                            Storage.spellSpellbooksSavedSpellbooks = Storage.spellSpellbooksSavedSpellbooks
                                .OrderBy(d => d.Blueprint.DisplayName).ToList();
                        }

                        if (Storage.spellSpellbooksSavedSpellbooks.Count - 1 <
                            Storage.spellSpellbooksSelectedSavedSpellbooksIndex)
                        {
                            if (Storage.spellSpellbooksSavedSpellbooks.Count - 1 > 0)
                                Storage.spellSpellbooksSelectedSavedSpellbooksIndex =
                                    Storage.spellSpellbooksSavedSpellbooks.Count - 1;
                            else
                                Storage.spellSpellbooksSelectedSavedSpellbooksIndex = 0;
                        }

                        GL.Space(10);
                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        showAddSpellbooks = GL.Toggle(showAddSpellbooks, RichText.Bold(Strings.GetText("toggle_AddSpellsbooks")), GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        if (showAddSpellbooks)
                        {
                            MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_AddSpellsbooks")));
                            GL.Space(10);
                            var blueprintSpellbooks = ResourcesLibrary.GetBlueprints<BlueprintSpellbook>();
                            foreach (var spellbook in blueprintSpellbooks)
                            {
                                GL.BeginHorizontal();
                                if (GL.Button(Strings.GetText("misc_Add") + $" <b>{spellbook.DisplayName}</b> ({spellbook.name})", GL.ExpandWidth(false)) && 5 > Storage.spellSpellbooksUnitEntityData[Storage.spellSpellbooksSelectedControllableCharacterIndex].Descriptor.Spellbooks.Count())
                                {
                                    Storage.spellSpellbooksUnitEntityData[Storage.spellSpellbooksSelectedControllableCharacterIndex].Descriptor.DemandSpellbook(spellbook);
                                    Storage.spellSpellbooksSelectedSavedSpellbooksIndex = 0;
                                    Storage.reloadPartySpellSpellbookSpellbooks = true;
                                    Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                                    Storage.reloadSpellSpellbooksLearnableSpells = true;
                                }
                                GL.EndHorizontal();
                            }
                        }

                        GL.EndVertical();
                        if (!Storage.reloadPartySpellSpellbookSpellbooks)
                        {
                            GL.Space(10);
                            GL.BeginVertical("box");

                            GL.BeginHorizontal();
                            GL.Label(RichText.Bold(Strings.GetText("label_Spellbooks")), GL.ExpandWidth(false));
                            GL.FlexibleSpace();
                            if (GL.Button(RichText.Bold(Strings.GetText("button_LoadRefresh")), GL.ExpandWidth(false)))
                            {
                                Storage.reloadPartySpellSpellbookSpellbooks = true;
                                Storage.reloadSpellSpellbooksLearnableSpells = true;
                                Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                            }
                            GL.EndHorizontal();

                            if (Storage.spellSpellbooksSavedSpellbooks.Any())
                            {
                                if (Storage.spellSpellbooksSelectedSavedSpellbooksIndex !=
                                    Storage.spellSpellbooksSelectedSavedSpellbooksIndexOld)
                                {
                                    Storage.reloadSpellSpellbooksLearnableSpells = true;
                                    Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                                    Storage.spellSpellbooksSelectedSavedSpellbooksIndexOld =
                                        Storage.spellSpellbooksSelectedSavedSpellbooksIndex;
                                }

                                GL.BeginHorizontal();
                                Storage.spellSpellbooksSelectedSavedSpellbooksIndex = GL.SelectionGrid(Storage.spellSpellbooksSelectedSavedSpellbooksIndex, Storage.spellSpellbooksSavedSpellbooksNames.ToArray(), 3);
                                GL.EndHorizontal();

                                GL.Space(10);

                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                showRemoveSpellbook = GL.Toggle(showRemoveSpellbook, RichText.Bold(Strings.GetText("button_RemoveSpellbook")), GL.ExpandWidth(false));
                                GL.EndHorizontal();
                                if (showRemoveSpellbook)
                                {
                                    GL.Space(10);
                                    GL.BeginHorizontal();
                                    if (GL.Button(
                                        RichText.BoldRedFormat(
                                            Strings.GetText("button_RemoveSpellbook") +
                                            $" {Storage.spellSpellbooksSavedSpellbooksNames[Storage.spellSpellbooksSelectedSavedSpellbooksIndex]} ({Storage.spellSpellbooksSavedSpellbooks[Storage.spellSpellbooksSelectedSavedSpellbooksIndex].Blueprint.name})"),
                                        GL.ExpandWidth(false)))
                                    {
                                        Storage
                                            .spellSpellbooksUnitEntityData
                                                [Storage.spellSpellbooksSelectedControllableCharacterIndex].Descriptor
                                            .DeleteSpellbook(Storage
                                                .spellSpellbooksSavedSpellbooks[
                                                    Storage.spellSpellbooksSelectedSavedSpellbooksIndex].Blueprint);

                                        Storage.reloadPartySpellSpellbookSpellbooks = true;
                                        Storage.reloadSpellSpellbooksLearnableSpells = true;
                                        Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                                    }

                                    GL.EndHorizontal();
                                }

                                GL.EndVertical();

                                GL.Space(10);

                                MenuTools.SingleLineLabel(
                                    $"{Strings.GetText("label_CurrentCasterLevel")}: {Storage.spellSpellbooksSavedSpellbooks[Storage.spellSpellbooksSelectedSavedSpellbooksIndex].CasterLevel}");
                                GL.BeginHorizontal();
                                if (GL.Button(
                                    Strings.GetText("button_AddCasterLevelTo") +
                                    $" {Storage.spellSpellbooksSavedSpellbooksNames[Storage.spellSpellbooksSelectedSavedSpellbooksIndex]} ({Storage.spellSpellbooksSavedSpellbooks[Storage.spellSpellbooksSelectedSavedSpellbooksIndex].Blueprint.name})",
                                    GL.ExpandWidth(false)))
                                    Storage.spellSpellbooksSavedSpellbooks[
                                        Storage.spellSpellbooksSelectedSavedSpellbooksIndex].AddCasterLevel();
                                GL.EndHorizontal();

                                GL.Space(10);

                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                Storage.spellSpellbooksShowKnownSpells = GL.Toggle(Storage.spellSpellbooksShowKnownSpells, RichText.Bold(Strings.GetText("toggle_ShowKnownSpells")), GL.ExpandWidth(false));
                                GL.EndHorizontal();

                                if (Storage.spellSpellbooksShowKnownSpells)
                                {
                                    GL.Space(10);


                                    IEnumerable<AbilityData> spells = Storage
                                        .spellSpellbooksSavedSpellbooks[
                                            Storage.spellSpellbooksSelectedSavedSpellbooksIndex].GetAllKnownSpells()
                                        .OrderBy(d => d.Name).ToList();
                                    if (spells.Any())
                                    {
                                        MenuTools.SingleLineLabel(
                                            RichText.WarningLargeRedFormat(Strings.GetText("warning_CantripsRemoval")));

                                        foreach (var spell in spells)
                                        {
                                            GL.BeginVertical("box");
                                            GL.BeginHorizontal();
                                            GL.Label($"{spell.Name} ({spell.Blueprint.School})");
                                            GL.FlexibleSpace();

                                            if (GL.Button(Strings.GetText("label_Remove"), GL.ExpandWidth(false)))
                                            {
                                                Storage.spellSpellbooksSavedSpellbooks[
                                                        Storage.spellSpellbooksSelectedSavedSpellbooksIndex]
                                                    .RemoveSpell(spell.Blueprint);
                                                Storage.spellSpellbooksSavedSpellbooks[
                                                        Storage.spellSpellbooksSelectedSavedSpellbooksIndex]
                                                    .RemoveSpell(spell.Blueprint);
                                                Storage.reloadSpellSpellbooksLearnableSpells = true;
                                                Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                                            }

                                            if (Storage.abilitiesFavourites.Contains(spell.Blueprint.AssetGuid))
                                            {
                                                if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                                {
                                                    Storage.abilitiesFavourites.Remove(spell.Blueprint.AssetGuid);
                                                    Storage.addAbilitiesFavouritesLoad = true;
                                                }
                                            }
                                            else
                                            {
                                                if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                                                {
                                                    Storage.abilitiesFavourites.Add(spell.Blueprint.AssetGuid);
                                                    Storage.addAbilitiesFavouritesLoad = true;
                                                }
                                            }

                                            GL.EndHorizontal();
                                            GL.EndVertical();
                                        }
                                    }
                                    else
                                    {
                                        MenuTools.SingleLineLabel(Strings.GetText("message_NoSpellsFound"));
                                    }
                                }

                                GL.EndVertical();

                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                Storage.spellSpellbooksShowLearnableSpells = GL.Toggle(Storage.spellSpellbooksShowLearnableSpells, RichText.Bold(Strings.GetText("toggle_ShowAllLearnableSpells")), GL.ExpandWidth(false));
                                GL.EndHorizontal();

                                if (Storage.spellSpellbooksShowLearnableSpells)
                                {
                                    if (Storage.reloadSpellSpellbooksLearnableSpells)
                                    {
                                        Storage.spellSpellbooksSpellLevelListIndex = 0;
                                        Storage.spellSpellbooksSavedSpellbooksLevels.Clear();
                                        Storage.spellSpellbooksSpellLevelLists.Clear();
                                        foreach (var spellLevelList in Storage
                                            .spellSpellbooksSavedSpellbooks[
                                                Storage.spellSpellbooksSelectedSavedSpellbooksIndex].Blueprint.SpellList
                                            .SpellsByLevel)
                                        {
                                            Storage.spellSpellbooksSavedSpellbooksLevels.Add(spellLevelList.SpellLevel
                                                .ToString());
                                            Storage.spellSpellbooksSpellLevelLists.Add(spellLevelList);
                                        }

                                        Storage.reloadSpellSpellbooksLearnableSpells = false;
                                    }

                                    if (Storage.spellSpellbooksSpellLevelLists.Count - 1 <
                                        Storage.spellSpellbooksSpellLevelListIndex)
                                    {
                                        if (Storage.spellSpellbooksSpellLevelLists.Count - 1 > 0)
                                            Storage.spellSpellbooksSpellLevelListIndex =
                                                Storage.spellSpellbooksSpellLevelLists.Count - 1;
                                        else
                                            Storage.spellSpellbooksSpellLevelListIndex = 0;
                                    }

                                    if (!Storage.reloadSpellSpellbooksLearnableSpells &&
                                        Storage.spellSpellbooksSavedSpellbooksLevels.Any())
                                    {
                                        GL.Space(10);

                                        MenuTools.SingleLineLabel(Strings.GetText("label_SpellListLevel"));

                                        GL.BeginHorizontal();
                                        Storage.spellSpellbooksSpellLevelListIndex =
                                            GL.SelectionGrid(Storage.spellSpellbooksSpellLevelListIndex,
                                                Storage.spellSpellbooksSavedSpellbooksLevels.ToArray(), 10,
                                                GL.ExpandWidth(false));
                                        GL.EndHorizontal();

                                        GL.Space(10);

                                        GL.BeginHorizontal();
                                        GL.Label(Strings.GetText("warning_NewSpellsAdded") + ": ",
                                            GL.ExpandWidth(false));

                                        if (GL.Button(
                                            MenuTools.TextWithTooltip("button_InstantRest", "tooltip_InstantRest",
                                                false), GL.ExpandWidth(false))) Cheats.InstantRest();
                                        GL.EndHorizontal();

                                        if (Storage.spellSpellbooksSpellLevelListIndex !=
                                            Storage.spellSpellbooksSpellLevelListIndexOld)
                                        {
                                            Storage.spellSpellbooksSpellLevelListIndexOld =
                                                Storage.spellSpellbooksSpellLevelListIndex;
                                            Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                                        }

                                        if (Storage.reloadSpellSpellbooksLearnableSpellsList)
                                        {
                                            Storage.spellSpellbooksSavedSpellbooksDescription.Clear();
                                            foreach (var spell in Storage
                                                .spellSpellbooksSpellLevelLists[
                                                    Storage.spellSpellbooksSpellLevelListIndex]
                                                .Spells) Storage.spellSpellbooksSavedSpellbooksDescription.Add(false);
                                            Storage.reloadSpellSpellbooksLearnableSpellsList = false;
                                        }

                                        GL.Space(10);

                                        if (!Storage.reloadSpellSpellbooksLearnableSpellsList)
                                        {
                                            Storage.spellSpellbooksLearnableSpellsListShowKnown = GL.Toggle(Storage.spellSpellbooksLearnableSpellsListShowKnown, Strings.GetText("toggle_ShowKnownSpells"), GL.ExpandWidth(false));

                                            GL.Space(10);

                                            for (var i = 0;
                                                i < Storage.spellSpellbooksSpellLevelLists[
                                                    Storage.spellSpellbooksSpellLevelListIndex].Spells.Count;
                                                i++)
                                                if (!Storage
                                                        .spellSpellbooksSavedSpellbooks
                                                            [Storage.spellSpellbooksSelectedSavedSpellbooksIndex]
                                                        .IsKnown(Storage
                                                            .spellSpellbooksSpellLevelLists[
                                                                Storage.spellSpellbooksSpellLevelListIndex]
                                                            .Spells[i]) && !Storage
                                                        .spellSpellbooksLearnableSpellsListShowKnown)
                                                {
                                                    GL.BeginVertical("box");
                                                    GL.BeginHorizontal();
                                                    Storage.spellSpellbooksSavedSpellbooksDescription[i] = GL.Toggle(Storage.spellSpellbooksSavedSpellbooksDescription[i], $"{Storage.spellSpellbooksSpellLevelLists[Storage.spellSpellbooksSpellLevelListIndex].Spells[i].Name} ({Storage.spellSpellbooksSpellLevelLists[Storage.spellSpellbooksSpellLevelListIndex].Spells[i].School})", GL.ExpandWidth(false));
                                                    GL.FlexibleSpace();
                                                    if (GL.Button(
                                                        Strings.GetText("misc_Learn") +
                                                        $" {Storage.spellSpellbooksSpellLevelLists[Storage.spellSpellbooksSpellLevelListIndex].Spells[i].Name}")
                                                    )
                                                    {
                                                        Storage
                                                            .spellSpellbooksSavedSpellbooks[
                                                                Storage.spellSpellbooksSelectedSavedSpellbooksIndex]
                                                            .AddKnown(Storage.spellSpellbooksSpellLevelListIndex,
                                                                Storage.spellSpellbooksSpellLevelLists[
                                                                        Storage.spellSpellbooksSpellLevelListIndex]
                                                                    .Spells[i]);
                                                        Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                                                    }

                                                    if (Storage.abilitiesFavourites.Contains(Storage
                                                        .spellSpellbooksSpellLevelLists[
                                                            Storage.spellSpellbooksSpellLevelListIndex].Spells[i]
                                                        .AssetGuid))
                                                    {
                                                        if (GL.Button(Storage.favouriteTrueString,
                                                            GL.ExpandWidth(false)))
                                                        {
                                                            Storage.abilitiesFavourites.Remove(Storage
                                                                .spellSpellbooksSpellLevelLists[
                                                                    Storage.spellSpellbooksSpellLevelListIndex]
                                                                .Spells[i].AssetGuid);
                                                            Storage.addAbilitiesFavouritesLoad = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (GL.Button(Storage.favouriteFalseString,
                                                            GL.ExpandWidth(false)))
                                                        {
                                                            Storage.abilitiesFavourites.Add(Storage
                                                                .spellSpellbooksSpellLevelLists[
                                                                    Storage.spellSpellbooksSpellLevelListIndex]
                                                                .Spells[i].AssetGuid);
                                                            Storage.addAbilitiesFavouritesLoad = true;
                                                        }
                                                    }

                                                    GL.EndHorizontal();
                                                    if (Storage.spellSpellbooksSavedSpellbooksDescription[i])
                                                        MenuTools.SingleLineLabel(Storage
                                                            .spellSpellbooksSpellLevelLists[
                                                                Storage.spellSpellbooksSpellLevelListIndex].Spells[i]
                                                            .Description);
                                                    GL.EndVertical();
                                                }
                                                else if (Storage.spellSpellbooksLearnableSpellsListShowKnown)
                                                {
                                                    GL.BeginVertical("box");
                                                    GL.BeginHorizontal();
                                                    Storage.spellSpellbooksSavedSpellbooksDescription[i] = GL.Toggle(Storage.spellSpellbooksSavedSpellbooksDescription[i], $"{Storage.spellSpellbooksSpellLevelLists[Storage.spellSpellbooksSpellLevelListIndex].Spells[i].Name} ({Storage.spellSpellbooksSpellLevelLists[Storage.spellSpellbooksSpellLevelListIndex].Spells[i].School})", GL.ExpandWidth(false));
                                                    GL.FlexibleSpace();

                                                    if (!Storage
                                                        .spellSpellbooksSavedSpellbooks[
                                                            Storage.spellSpellbooksSelectedSavedSpellbooksIndex]
                                                        .IsKnown(Storage
                                                            .spellSpellbooksSpellLevelLists[
                                                                Storage.spellSpellbooksSpellLevelListIndex].Spells[i]))
                                                        if (GL.Button(
                                                            Strings.GetText("misc_Learn") +
                                                            $" {Storage.spellSpellbooksSpellLevelLists[Storage.spellSpellbooksSpellLevelListIndex].Spells[i].Name}")
                                                        )
                                                        {
                                                            Storage
                                                                .spellSpellbooksSavedSpellbooks[
                                                                    Storage.spellSpellbooksSelectedSavedSpellbooksIndex]
                                                                .AddKnown(Storage.spellSpellbooksSpellLevelListIndex,
                                                                    Storage.spellSpellbooksSpellLevelLists[
                                                                            Storage.spellSpellbooksSpellLevelListIndex]
                                                                        .Spells[i]);
                                                            Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                                                        }

                                                    if (Storage.abilitiesFavourites.Contains(Storage
                                                        .spellSpellbooksSpellLevelLists[
                                                            Storage.spellSpellbooksSpellLevelListIndex].Spells[i]
                                                        .AssetGuid))
                                                    {
                                                        if (GL.Button(Storage.favouriteTrueString,
                                                            GL.ExpandWidth(false)))
                                                        {
                                                            Storage.abilitiesFavourites.Remove(Storage
                                                                .spellSpellbooksSpellLevelLists[
                                                                    Storage.spellSpellbooksSpellLevelListIndex]
                                                                .Spells[i].AssetGuid);
                                                            Storage.addAbilitiesFavouritesLoad = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (GL.Button(Storage.favouriteFalseString,
                                                            GL.ExpandWidth(false)))
                                                        {
                                                            Storage.abilitiesFavourites.Add(Storage
                                                                .spellSpellbooksSpellLevelLists[
                                                                    Storage.spellSpellbooksSpellLevelListIndex]
                                                                .Spells[i].AssetGuid);
                                                            Storage.addAbilitiesFavouritesLoad = true;
                                                        }
                                                    }

                                                    GL.EndHorizontal();
                                                    if (Storage.spellSpellbooksSavedSpellbooksDescription[i])
                                                        MenuTools.SingleLineLabel(Storage
                                                            .spellSpellbooksSpellLevelLists[
                                                                Storage.spellSpellbooksSpellLevelListIndex].Spells[i]
                                                            .Description);
                                                    GL.EndVertical();
                                                }
                                        }
                                    }
                                }

                                GL.EndVertical();


                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                Storage.spellSpellbooksShowAddAbilitiesAsSpells = GL.Toggle(Storage.spellSpellbooksShowAddAbilitiesAsSpells, RichText.Bold(Strings.GetText("toggle_AddAbilitiesAsSpells")), GL.ExpandWidth(false));
                                GL.EndHorizontal();
                                if (Storage.spellSpellbooksShowAddAbilitiesAsSpells)
                                {
                                    if (Storage.reloadSpellSpellbooksLearnableSpells)
                                    {
                                        Storage.spellSpellbooksSpellLevelAbilitiesIndex = 0;
                                        Storage.spellSpellbooksSavedSpellbooksLevels.Clear();
                                        Storage.spellSpellbooksSpellLevelLists.Clear();
                                        foreach (var spellLevelList in Storage
                                            .spellSpellbooksSavedSpellbooks[
                                                Storage.spellSpellbooksSelectedSavedSpellbooksIndex].Blueprint.SpellList
                                            .SpellsByLevel)
                                        {
                                            Storage.spellSpellbooksSavedSpellbooksLevels.Add(spellLevelList.SpellLevel
                                                .ToString());
                                            Storage.spellSpellbooksSpellLevelLists.Add(spellLevelList);
                                        }

                                        Storage.reloadSpellSpellbooksLearnableSpells = false;
                                        Storage.addAbilitiesFavouritesLoad = true;
                                    }

                                    if (Storage.spellSpellbooksSpellLevelLists.Count - 1 <
                                        Storage.spellSpellbooksSpellLevelAbilitiesIndex)
                                    {
                                        if (Storage.spellSpellbooksSpellLevelLists.Count - 1 > 0)
                                            Storage.spellSpellbooksSpellLevelAbilitiesIndex =
                                                Storage.spellSpellbooksSpellLevelLists.Count - 1;
                                        else
                                            Storage.spellSpellbooksSpellLevelAbilitiesIndex = 0;
                                    }


                                    if (Storage.addAbilitiesFavouritesLoad == true)
                                    {
                                        Main.RefreshAbilityFavourites();
                                        Storage.addAbilitiesFavouritesLoad = false;
                                    }

                                    GL.Space(10);
                                    if (!Storage.abilitiesFavourites.Any())
                                    {
                                        MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                                    }
                                    else
                                    {
                                        MenuTools.SingleLineLabel(Strings.GetText("message_AddAbilitiesAsSpells"));

                                        GL.Space(10);

                                        MenuTools.SingleLineLabel(Strings.GetText("label_SpellLevel"));

                                        GL.BeginHorizontal();
                                        Storage.spellSpellbooksSpellLevelAbilitiesIndex =
                                            GL.SelectionGrid(Storage.spellSpellbooksSpellLevelAbilitiesIndex,
                                                Storage.spellSpellbooksSavedSpellbooksLevels.ToArray(), 10,
                                                GL.ExpandWidth(false));
                                        GL.EndHorizontal();
                                        GL.Space(10);

                                        GL.BeginHorizontal();
                                        GL.Label(Strings.GetText("warning_NewSpellsAddedAlt") + ": ",
                                            GL.ExpandWidth(false));

                                        if (GL.Button(
                                            MenuTools.TextWithTooltip("button_InstantRest", "tooltip_InstantRest",
                                                false), GL.ExpandWidth(false))) Cheats.InstantRest();
                                        GL.EndHorizontal();

                                        if (Storage.spellSpellbooksSpellLevelAbilitiesIndex !=
                                            Storage.spellSpellbooksSpellLevelAbilitiesIndexOld)
                                        {
                                            Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                                            Storage.spellSpellbooksSpellLevelAbilitiesIndexOld =
                                                Storage.spellSpellbooksSpellLevelAbilitiesIndex;
                                        }

                                        GL.Space(10);

                                        for (var i = 0; i < Storage.abilitiesFavourites.Count; i++)
                                        {
                                            GL.BeginHorizontal();
                                            Storage.abilitiesToggleFavouriteDescription.Add(false);
                                            if (Storage.abilitiesFavouritesDescriptions[i] != null && Storage.abilitiesFavouritesDescriptions[i] != "")
                                                Storage.abilitiesToggleFavouriteDescription[i] = GL.Toggle(Storage.abilitiesToggleFavouriteDescription[i], Storage.abilitiesFavouritesNames[i], GL.ExpandWidth(false));

                                            try
                                            {
                                                if (GL.Button(
                                                    Strings.GetText("misc_Learn") +
                                                    $" {Storage.abilitiesFavouritesNames[i]} " +
                                                    Strings.GetText("misc_AtLevel") + " " +
                                                    Storage.spellSpellbooksSpellLevelAbilitiesIndex.ToString(),
                                                    GL.ExpandWidth(false)))
                                                {
                                                    Storage
                                                        .spellSpellbooksSavedSpellbooks[
                                                            Storage.spellSpellbooksSelectedSavedSpellbooksIndex]
                                                        .AddKnown(Storage.spellSpellbooksSpellLevelAbilitiesIndex,
                                                            Utilities.GetBlueprintByGuid<BlueprintAbility>(
                                                                Storage.abilitiesFavouritesGuids[i]));
                                                    Storage.reloadPartySpellSpellbookSpellbooks = true;
                                                    Storage.reloadSpellSpellbooksLearnableSpellsList = true;
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                modLogger.Log(e.ToString());
                                            }

                                            if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                            {
                                                Storage.abilitiesFavouritesGuids.Remove(
                                                    Storage.abilitiesFavouritesGuids[i]);
                                                Storage.addAbilitiesFavouritesLoad = true;
                                            }

                                            GL.EndHorizontal();

                                            AbilityDetails(Storage.abilitiesToggleFavouriteDescription[i], Storage.abilitiesFavouritesGuids[i], Storage.abilitiesFavouritesDescriptions[i], Storage.spellSpellbooksUnitEntityData[Storage.spellSpellbooksSelectedControllableCharacterIndex]);
                                        }
                                    }

                                    if (Storage.abilitiesFavourites != Storage.abilitiesFavouritesGuids)
                                        Storage.abilitiesFavourites = Storage.abilitiesFavouritesGuids;
                                }

                                GL.EndVertical();
                            }
                            else
                            {
                                MenuTools.SingleLineLabel(Strings.GetText("message_NoSpellbooksFound"));
                            }

                            GL.EndVertical();
                        }
                    }
                }
                else
                {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitFound"));
                }
            }

            GL.EndVertical();
        }

        public static void AddXP()
        {
            GL.BeginHorizontal();
            settings.experienceAmount = GL.TextField(settings.experienceAmount, 8, GL.Width(100f));

            settings.experienceAmount = MenuTools.IntTestSettingStage1(settings.experienceAmount);
            settings.finalExperienceAmount =
                MenuTools.IntTestSettingStage2(settings.experienceAmount, settings.finalExperienceAmount);

            if (GL.Button($"+{settings.experienceAmount}" + " " + Strings.GetText("button_PartyExperience"),
                GL.ExpandWidth(false)))
            {
                var tempExperienceMultiplier = settings.experienceMultiplier;
                var tempCustomExperienceMultiplier = settings.customExperienceMultiplier;
                settings.experienceMultiplier = 1f;
                settings.customExperienceMultiplier = "1";

                Game.Instance.Player.GainPartyExperience(settings.finalExperienceAmount);

                settings.experienceMultiplier = tempExperienceMultiplier;
                settings.customExperienceMultiplier = tempCustomExperienceMultiplier;
            }

            GL.EndHorizontal();

            GL.Space(10);

            GL.BeginHorizontal();
            Storage.xpFilterUnitEntityDataIndex = GL.SelectionGrid(Storage.xpFilterUnitEntityDataIndex, Storage.unitEntityDataArrayNoEnemies, 3);
            GL.EndHorizontal();
            var player = Game.Instance.Player;
            switch (Storage.xpFilterUnitEntityDataIndex)
            {
                case 0:
                    Storage.xpUnitEntityData = player.Party;
                    break;
                case 1:
                    Storage.xpUnitEntityData = player.ControllableCharacters;
                    break;
                case 2:
                    Storage.xpUnitEntityData = player.ActiveCompanions;
                    break;
                case 3:
                    Storage.xpUnitEntityData = player.AllCharacters;
                    break;
                case 4:
                    Storage.xpUnitEntityData = Common.GetCustomCompanions();
                    break;
                case 5:
                    Storage.xpUnitEntityData = Common.GetPets();
                    break;
            }

            GL.Space(10);

            if (Storage.xpUnitEntityData.Any())
                foreach (var controllableCharacter in Storage.xpUnitEntityData)
                {
                    GL.BeginVertical("box");

                    GL.BeginHorizontal();
                    if (GL.Button(
                        $"{controllableCharacter.CharacterName}:  +{settings.experienceAmount} " +
                        Strings.GetText("header_Experience"), GL.ExpandWidth(false)))
                    {
                        var tempExperienceMultiplier = settings.experienceMultiplier;
                        var tempCustomExperienceMultiplier = settings.customExperienceMultiplier;
                        settings.experienceMultiplier = 1f;
                        settings.customExperienceMultiplier = "1";

                        controllableCharacter.Descriptor.Progression.GainExperience(settings.finalExperienceAmount,
                            true);

                        settings.experienceMultiplier = tempExperienceMultiplier;
                        settings.customExperienceMultiplier = tempCustomExperienceMultiplier;
                    }

                    if (controllableCharacter.Descriptor.Progression.Experience <
                        BlueprintRoot.Instance.Progression.XPTable.GetBonus(
                            controllableCharacter.Descriptor.Progression.CharacterLevel + 1) &&
                        controllableCharacter.Descriptor.Progression.CharacterLevel < 20)
                    {
                        if (GL.Button(" +1 " + Strings.GetText("header_Level") + " ", GL.ExpandWidth(false)))
                            controllableCharacter.Descriptor.Progression.AdvanceExperienceTo(
                                BlueprintRoot.Instance.Progression.XPTable.GetBonus(
                                    controllableCharacter.Descriptor.Progression.CharacterLevel + 1), true);
                    }
                    else if (controllableCharacter.Descriptor.Progression.Experience >=
                             BlueprintRoot.Instance.Progression.XPTable.GetBonus(
                                 controllableCharacter.Descriptor.Progression.CharacterLevel + 1) &&
                             controllableCharacter.Descriptor.Progression.CharacterLevel < 20)
                    {
                        if (GL.Button(Strings.GetText("button_ReadyToLevel"), GL.ExpandWidth(false)))
                        {
                        }
                    }

                    GL.Label(" " + Strings.GetText("label_CurrentXP") +
                             $": {controllableCharacter.Descriptor.Progression.Experience}" + " | " +
                             Strings.GetText("label_CurrentLevel") +
                             $": {controllableCharacter.Descriptor.Progression.CharacterLevel}");


                    GL.EndHorizontal();

                    GL.EndVertical();
                }
            else
                MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitFound"));
        }

        public static void CharacterCreationProgressionExperience()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showExperienceCategory = GL.Toggle(settings.showExperienceCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_XP")), GL.ExpandWidth(false));
            if (!settings.showExperienceCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("CharacterCreationProgressionExperience");

                GL.Space(10);

                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_CC")));
                GL.BeginHorizontal();
                GL.Label(
                    MenuTools.TextWithTooltip("headerOption_MaxAbilityScoreAtCC", "tooltip_MaxAbilityScoreAtCC", "",
                        " "), GL.ExpandWidth(false));
                settings.characterCreationAbilityPointsMaxString =
                    GL.TextField(settings.characterCreationAbilityPointsMaxString, 10, GL.Width(90f));
                MenuTools.SettingParse(ref settings.characterCreationAbilityPointsMaxString,
                    ref settings.characterCreationAbilityPointsMax);
                GL.EndHorizontal();

                GL.BeginHorizontal();
                GL.Label(
                    MenuTools.TextWithTooltip("headerOption_MinAbilityScoreAtCC", "tooltip_MaxAbilityScoreAtCC", "",
                        " "), GL.ExpandWidth(false));
                settings.characterCreationAbilityPointsMinString =
                    GL.TextField(settings.characterCreationAbilityPointsMinString, 10, GL.Width(90f));
                MenuTools.SettingParse(ref settings.characterCreationAbilityPointsMinString,
                    ref settings.characterCreationAbilityPointsMin);
                GL.EndHorizontal();

                GL.BeginHorizontal();
                GL.Label(
                    MenuTools.TextWithTooltip("headerOption_AbilityPointsAtCCPlayer", "tooltip_AbilityPointsAtCCPlayer",
                        "", " "), GL.ExpandWidth(false));
                settings.characterCreationAbilityPointsPlayerString =
                    GL.TextField(settings.characterCreationAbilityPointsPlayerString, 10, GL.Width(90f));
                MenuTools.SettingParse(ref settings.characterCreationAbilityPointsPlayerString,
                    ref settings.characterCreationAbilityPointsPlayer);
                GL.EndHorizontal();
                GL.BeginHorizontal();

                GL.Label(
                    MenuTools.TextWithTooltip("headerOption_AbilityPointsAtCCmerc", "tooltip_AbilityPointsAtCCmerc", "",
                        " "), GL.ExpandWidth(false));
                settings.characterCreationAbilityPointsMercString =
                    GL.TextField(settings.characterCreationAbilityPointsMercString, 10, GL.Width(90f));
                MenuTools.SettingParse(ref settings.characterCreationAbilityPointsMercString,
                    ref settings.characterCreationAbilityPointsMerc);
                GL.EndHorizontal();
                GL.Space(10);

                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Progression")));
                GL.BeginHorizontal();
                GL.Label(MenuTools.TextWithTooltip("headerOption_AddCasterLevel", "tooltip_AddCasterLevel", "", " "),
                    GL.ExpandWidth(false));
                settings.addCasterLevelString = GL.TextField(settings.addCasterLevelString, 10, GL.Width(90f));
                MenuTools.SettingParse(ref settings.addCasterLevelString, ref settings.addCasterLevel);
                GL.EndHorizontal();

                GL.Space(10);
                MenuTools.ToggleButton(ref settings.toggleFullHitdiceEachLevel, "buttonToggle_FullHitdiceEachLevel",
                    "tooltip_FullHitdiceEachLevel", nameof(settings.toggleFullHitdiceEachLevel));
                Main.ToggleActiveWarning(ref settings.toggleFullHitdiceEachLevel,
                    ref settings.toggleRollHitDiceEachLevel, "buttonToggle_RollHitDiceEachLevel");

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleNoLevelUpRestirctions, "buttonToggle_NoLevelUpRestirctions",
                    "tooltip_NoLevelUpRestirctions", nameof(settings.toggleNoLevelUpRestirctions));
                MenuTools.SingleLineLabel(Strings.GetText("warning_toggleNoLevelUpRestirctions"));

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleInfiniteSkillpoints, "buttonToggle_InfiniteSkillPoints",
                    "tooltip_InfiniteSkillPoints", nameof(settings.toggleInfiniteSkillpoints));
                MenuTools.SingleLineLabel(Strings.GetText("warning_InfiniteSkillPoints"));

                GL.Space(10);

                Prerequisites();

                if (Strings.ToBool(settings.toggleExperienceMultiplier))
                {
                    if (Main.ScaleXp.ModIsActive())
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("warning_XPAndScaleXPActive")));
                    GL.Space(10);
                    GL.BeginHorizontal();
                    GL.Label(MenuTools.TextWithTooltip("header_Multiplier", "tooltip_Multiplier", "", " "),
                        GL.ExpandWidth(false));
                    settings.experienceMultiplier =
                        GL.HorizontalSlider(settings.experienceMultiplier, 0.1f, 10f, GL.Width(300f));
                    GL.Label($" {Math.Round(settings.experienceMultiplier, 1)}", GL.ExpandWidth(false));
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    GL.Label(
                        MenuTools.TextWithTooltip("header_UseCustomMultiplier", "tooltip_CustomMultiplier", "", " "),
                        GL.ExpandWidth(false));
                    settings.useCustomExperienceMultiplier = GL.Toggle(settings.useCustomExperienceMultiplier, new GUIContent("", Strings.GetText("tooltip_CustomMultiplier")), GL.ExpandWidth(false));
                    GL.EndHorizontal();

                    if (settings.useCustomExperienceMultiplier == true)
                    {
                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("header_CustomMultiplier") + " ", GL.ExpandWidth(false));
                        settings.customExperienceMultiplier =
                            GL.TextField(settings.customExperienceMultiplier, 8, GL.Width(100f));
                        settings.customExperienceMultiplier =
                            MenuTools.FloatTestSettingStage1(settings.customExperienceMultiplier);
                        GL.EndHorizontal();

                        settings.finalCustomExperienceMultiplier =
                            MenuTools.FloatTestSettingStage2(settings.customExperienceMultiplier,
                                settings.finalCustomExperienceMultiplier);

                        GL.BeginHorizontal();
                        GL.Label(
                            Strings.GetText("label_CurrentMultiplier") +
                            $": {settings.finalCustomExperienceMultiplier}", GL.Width(150f));
                        GL.EndHorizontal();
                    }
                }

                GL.Space(10);

                AddXP();
            }

            GL.EndVertical();
        }

        public static void Prerequisites()
        {
            GL.BeginVertical("box");

            MenuTools.ToggleButton(ref settings.toggleIgnorePrerequisites, "buttonToggle_IgnorePrerequisites",
                "tooltip_IgnorePrerequisites", nameof(settings.toggleIgnorePrerequisites));

            GL.Space(10);
            MenuTools.ToggleButton(ref settings.toggleIgnoreCasterTypeSpellLevel,
                "buttonToggle_IgnoreCasterTypeSpellLevel", "tooltip_IgnoreCasterTypeSpellLevel",
                nameof(settings.toggleIgnoreCasterTypeSpellLevel));

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleIgnoreForbiddenArchetype, "buttonToggle_IgnoreForbiddenArchetype",
                "tooltip_IgnoreForbiddenArchetype", nameof(settings.toggleIgnoreForbiddenArchetype));

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleIgnorePrerequisiteStatValue,
                "buttonToggle_IgnorePrerequisiteStatValue", "tooltip_IgnorePrerequisiteStatValue",
                nameof(settings.toggleIgnorePrerequisiteStatValue));

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleIgnoreClassAlignment, "buttonToggle_IgnoreClassAlignment",
                "tooltip_IgnoreClassAlignment", nameof(settings.toggleIgnoreClassAlignment));
            MenuTools.SingleLineLabel(Strings.GetText("warning_IgnoreClassAlignment"));

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleIgnoreForbiddenFeatures, "buttonToggle_IgnoreForbiddenFeatures",
                "tooltip_IgnoreForbiddenFeatures", nameof(settings.toggleIgnoreForbiddenFeatures));

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleIgnoreFeaturePrerequisites,
                "buttonToggle_IgnorenFeaturePrerequisites", "tooltip_IgnorenFeaturePrerequisites",
                nameof(settings.toggleIgnoreFeaturePrerequisites));

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleIgnoreFeatureListPrerequisites,
                "buttonToggle_IgnorenFeatureListPrerequisites", "tooltip_IgnorenFeatureListPrerequisites",
                nameof(settings.toggleIgnoreFeatureListPrerequisites));

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleFeaturesIgnorePrerequisites,
                "buttonToggle_FeaturesIgnorePrerequisites", "tooltip_FeaturesIgnorePrerequisites",
                nameof(settings.toggleFeaturesIgnorePrerequisites));

            GL.Space(10);

            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Experience")));

            MenuTools.ToggleButton(ref settings.toggleExperienceMultiplier, "buttonToggle_ExperienceMultiplier",
                "tooltip_ExperienceMultiplier");

            GL.Space(10);

            GL.EndVertical();
        }

        public static SelectionGrid setEncumbranceGrid = new SelectionGrid(Storage.encumbranceArray, 4);

        public static void Encumbrance()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showCarryMoreCategory = GL.Toggle(settings.showCarryMoreCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_Encumbrance")), GL.ExpandWidth(false));
            if (!settings.showCarryMoreCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("Encumbrance");

                GL.BeginVertical("box");
                MenuTools.ToggleButton(ref settings.toggleSetEncumbrance, "buttonToggle_SetEncumbrance",
                    "tooltip_SetEncumbrance", true);
                if (Strings.ToBool(settings.toggleSetEncumbrance))
                {
                    setEncumbranceGrid.Render();
                    settings.setEncumbrancIndex = setEncumbranceGrid.selected;
                }

                GL.EndVertical();

                GL.Space(10);

                GL.BeginVertical("box");
                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_CarryingCapacity")));

                GL.BeginHorizontal();
                GL.Label(MenuTools.TextWithTooltip("header_Multiplier", "tooltip_Multiplier", "", " "),
                    GL.ExpandWidth(false));
                settings.heavyEncumbranceMultiplier =
                    GL.HorizontalSlider(settings.heavyEncumbranceMultiplier, 1f, 100f, GL.Width(300f));
                GL.Label($" {Mathf.RoundToInt(settings.heavyEncumbranceMultiplier)}", GL.ExpandWidth(false));
                GL.EndHorizontal();

                GL.BeginHorizontal();
                settings.useCustomHeavyEncumbranceMultiplier = GL.Toggle(settings.useCustomHeavyEncumbranceMultiplier, MenuTools.TextWithTooltip("header_UseCustomMultiplier", "tooltip_CustomMultiplier", "", " "), GL.ExpandWidth(false));
                GL.EndHorizontal();

                if (settings.useCustomHeavyEncumbranceMultiplier == true)
                {
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("header_CustomMultiplier") + " ", GL.ExpandWidth(false));
                    settings.customHeavyEncumbranceMultiplier =
                        GL.TextField(settings.customHeavyEncumbranceMultiplier, 6, GL.Width(100f));
                    settings.customHeavyEncumbranceMultiplier =
                        MenuTools.FloatTestSettingStage1(settings.customHeavyEncumbranceMultiplier);
                    GL.EndHorizontal();

                    settings.finalCustomHeavyEncumbranceMultiplier = MenuTools.FloatTestSettingStage2(
                        settings.customHeavyEncumbranceMultiplier, settings.finalCustomHeavyEncumbranceMultiplier);

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_CurrentMultiplier") + $": {settings.finalCustomHeavyEncumbranceMultiplier}");
                    GL.EndHorizontal();
                }

                GL.EndVertical();
            }

            GL.EndVertical();
        }

        public static void RestRations()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showRestLessCategory = GL.Toggle(settings.showRestLessCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_Rest")), GL.ExpandWidth(false));
            if (!settings.showRestLessCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("RestRations");

                GL.Space(10);

                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_RestLess")));
                GL.BeginHorizontal();
                GL.Label(MenuTools.TextWithTooltip("header_Multiplier", "tooltip_Multiplier", "", " "),
                    GL.ExpandWidth(false));
                settings.fatigueHoursModifierMultiplier = GL.HorizontalSlider(settings.fatigueHoursModifierMultiplier,
                    0.1f, 30f, GL.Width(300f));
                GL.Label($" {Math.Round(settings.fatigueHoursModifierMultiplier, 1)}", GL.ExpandWidth(false));
                GL.EndHorizontal();

                GL.BeginHorizontal();
                settings.useCustomFatigueHoursModifierMultiplier = GL.Toggle(settings.useCustomFatigueHoursModifierMultiplier, MenuTools.TextWithTooltip("header_UseCustomMultiplier", "tooltip_CustomMultiplier", "", " "), GL.ExpandWidth(false));
                GL.EndHorizontal();

                if (settings.useCustomFatigueHoursModifierMultiplier == true)
                {
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("header_CustomMultiplier") + " ", GL.ExpandWidth(false));
                    settings.customFatigueHoursModifierMultiplier =
                        GL.TextField(settings.customFatigueHoursModifierMultiplier, 6, GL.Width(100f));
                    settings.customFatigueHoursModifierMultiplier =
                        MenuTools.FloatTestSettingStage1(settings.customFatigueHoursModifierMultiplier);
                    GL.EndHorizontal();

                    settings.finalCustomFatigueHoursModifierMultiplier = MenuTools.FloatTestSettingStage2(
                        settings.customFatigueHoursModifierMultiplier,
                        settings.finalCustomFatigueHoursModifierMultiplier);

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_CurrentMultiplier") + $": {settings.finalCustomFatigueHoursModifierMultiplier}");
                    GL.EndHorizontal();
                }

                GL.Space(10);
                GL.BeginHorizontal();
                if (GL.Button(
                    MenuTools.TextWithTooltip("button_LearnAllCookingRecipes", "tooltip_LearnAllCookingRecipes", false),
                    GL.ExpandWidth(false)))
                {
                    var bcr = ResourcesLibrary.GetBlueprints<BlueprintCookingRecipe>();
                    foreach (var b in bcr) Game.Instance.Player.Camping.UnlockRecipe(b);
                }

                GL.EndHorizontal();
                MenuTools.ToggleButton(ref settings.toggleNoIngredientsRequired, "buttonToggle_NoIngredientsRequired",
                    "tooltip_NoIngredientsRequired", nameof(settings.toggleNoIngredientsRequired));
                GL.Space(10);

                InstantRest();
                MenuTools.ToggleButton(ref settings.toggleInstantRestAfterCombat, "buttonToggle_InstantRestAfterCombat",
                    "tooltip_InstantRestAfterCombat", nameof(settings.toggleInstantRestAfterCombat));
                Main.ToggleActiveWarning(ref settings.toggleInstantRestAfterCombat,
                    ref settings.toggleRestoreSpellsAbilitiesAfterCombat, "button_RestoreSpellsAbilitiesAfterCombat");

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleNoRationsRequired, "headerOption_NoRationsRequired",
                    "tooltip_NoRationsRequired", nameof(settings.toggleNoRationsRequired));

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleCookingAndHuntingInDungeons,
                    "buttonToggle_CookingAndHuntingInDungeons", "tooltip_CookingAndHuntingInDungeons",
                    nameof(settings.toggleCookingAndHuntingInDungeons));

                GL.Space(10);

                GL.BeginHorizontal();
                if (GL.Button(MenuTools.TextWithTooltip("button_Add6Rations", "tooltip_Add6Rations", false),
                    GL.ExpandWidth(false))) MenuTools.AddSingleItemAmount("efa6c2ee9e630384188a50b1ce6600fe", 6);
                GL.EndHorizontal();
            }

            GL.EndVertical();
        }

        public static void Money()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showMoneyCategory = GL.Toggle(settings.showMoneyCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_Money")), GL.ExpandWidth(false));
            if (!settings.showMoneyCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("Money");

                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(MenuTools.TextWithTooltip("header_Multiplier", "tooltip_Multiplier", "", " "),
                    GL.ExpandWidth(false));
                settings.moneyMultiplier = GL.HorizontalSlider(settings.moneyMultiplier, 0.1f, 10f, GL.Width(300f));
                GL.Label($" {Math.Round(settings.moneyMultiplier, 1)}", GL.ExpandWidth(false));
                GL.EndHorizontal();

                GL.BeginHorizontal();
                settings.useCustomMoneyMultiplier = GL.Toggle(settings.useCustomMoneyMultiplier, MenuTools.TextWithTooltip("header_UseCustomMultiplier", "tooltip_CustomMultiplier", "", " "), GL.ExpandWidth(false));
                GL.EndHorizontal();

                if (settings.useCustomMoneyMultiplier == true)
                {
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("header_CustomMultiplier") + " ", GL.ExpandWidth(false));
                    settings.customMoneyMultiplier = GL.TextField(settings.customMoneyMultiplier, 6, GL.Width(100f));
                    settings.customMoneyMultiplier = MenuTools.FloatTestSettingStage1(settings.customMoneyMultiplier);
                    GL.EndHorizontal();

                    settings.finalCustomMoneyMultiplier =
                        MenuTools.FloatTestSettingStage2(settings.customMoneyMultiplier,
                            settings.finalCustomMoneyMultiplier);

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_CurrentMultiplier") + $": {settings.finalCustomMoneyMultiplier}");
                    GL.EndHorizontal();
                }

                GL.Space(10);

                GL.BeginHorizontal();
                settings.moneyAmount = GL.TextField(settings.moneyAmount, 10, GL.Width(200f));

                settings.moneyAmount = MenuTools.IntTestSettingStage1(settings.moneyAmount);
                settings.finalMoneyAmount =
                    MenuTools.IntTestSettingStage2(settings.moneyAmount, settings.finalMoneyAmount);

                if (GL.Button($"+{settings.moneyAmount} " + Strings.GetText("button_Money")))
                {
                    var tempMoneyMultiplicer = settings.moneyMultiplier;
                    var tempCustomMoneyMultiplier = settings.customMoneyMultiplier;
                    settings.moneyMultiplier = 1f;
                    settings.customMoneyMultiplier = "1";

                    Game.Instance.Player.GainMoney(settings.finalMoneyAmount);
                    Common.AddLogEntry(
                        Strings.GetText("logMessage_Received") + " " + settings.finalMoneyAmount + " " +
                        Strings.GetText("logMessage_Gold"), Color.black);


                    settings.moneyMultiplier = tempMoneyMultiplicer;
                    settings.customMoneyMultiplier = tempCustomMoneyMultiplier;
                }

                if (GL.Button($"-{settings.moneyAmount} " + Strings.GetText("button_Money")))
                {
                    Game.Instance.Player.GainMoney(settings.finalMoneyAmount * -1);
                    Common.AddLogEntry(
                        Strings.GetText("logMessage_Removed") + " " + settings.finalMoneyAmount + " " +
                        Strings.GetText("logMessage_Gold"), Color.black);
                }
                if (GL.Button($"{Strings.GetText("button_SetTo")} {settings.moneyAmount}"))
                {
                    if (Game.Instance.Player.Money > settings.finalMoneyAmount)
                    {
                        long setMoney = (Game.Instance.Player.Money - settings.finalMoneyAmount) * -1;
                        Game.Instance.Player.GainMoney(setMoney);
                        Common.AddLogEntry(Strings.GetText("logMessage_Removed") + " " + setMoney + " " + Strings.GetText("logMessage_Gold"), Color.black);
                    }
                    else if (Game.Instance.Player.Money < settings.finalMoneyAmount)
                    {
                        long setMoney = settings.finalMoneyAmount - Game.Instance.Player.Money;
                        Game.Instance.Player.GainMoney(setMoney);
                        Common.AddLogEntry(Strings.GetText("logMessage_Received") + " " + setMoney + " " + Strings.GetText("logMessage_Gold"), Color.black);
                    }
                }

                GL.EndHorizontal();

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_CurrentMoney") + $": {Game.Instance.Player.Money}",
                    GL.ExpandWidth(false));
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(
                    MenuTools.TextWithTooltip("headerOption_MercenaryCostMultiplier", "tooltip_MercenaryCostMultiplier",
                        "", " "), GL.ExpandWidth(false));
                settings.companionCostMultiplierString =
                    GL.TextField(settings.companionCostMultiplierString, 10, GL.Width(90f));
                MenuTools.SettingParse(ref settings.companionCostMultiplierString,
                    ref settings.companionCostMultiplier);
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                if (GL.Button(
                    MenuTools.TextWithTooltip("headerOption_VendorSellMultiplier", "tooltip_VendorSellMultiplier",
                        $"{settings.toggleVendorSellPriceMultiplier}" + " ", ""), GL.ExpandWidth(false)))
                {
                    if (settings.toggleVendorSellPriceMultiplier == Storage.isFalseString)
                    {
                        settings.toggleVendorSellPriceMultiplier = Storage.isTrueString;
                    }
                    else if (Strings.ToBool(settings.toggleVendorSellPriceMultiplier))
                    {
                        settings.toggleVendorSellPriceMultiplier = Storage.isFalseString;
                        Game.Instance.BlueprintRoot.Vendors.SellModifier = Storage.defaultVendorSellPriceMultiplier;
                    }
                }

                GL.EndHorizontal();

                if (Strings.ToBool(settings.toggleVendorSellPriceMultiplier))
                {
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("headerOption_VendorSellMultiplier") + " ", GL.ExpandWidth(false));
                    settings.vendorSellPriceMultiplier =
                        GL.TextField(settings.vendorSellPriceMultiplier, 6, GL.Width(100f));
                    settings.vendorSellPriceMultiplier =
                        MenuTools.FloatTestSettingStage1(settings.vendorSellPriceMultiplier);
                    GL.EndHorizontal();

                    settings.finalVendorSellPriceMultiplier =
                        MenuTools.FloatTestSettingStage2(settings.vendorSellPriceMultiplier,
                            settings.finalVendorSellPriceMultiplier);

                    GL.BeginHorizontal();
                    GL.Label(
                        Strings.GetText("label_CurrentMultiplier") +
                        $": {Game.Instance.BlueprintRoot.Vendors.SellModifier}", GL.Width(150f));
                    GL.EndHorizontal();

                    GL.BeginHorizontal();

                    if (GL.Button(Strings.GetText("button_Apply") + $" ({settings.finalVendorSellPriceMultiplier})",
                        GL.ExpandWidth(false)))
                        Game.Instance.BlueprintRoot.Vendors.SellModifier = settings.finalVendorSellPriceMultiplier;
                    GL.EndHorizontal();

                    GL.Space(10);
                }

                MenuTools.ToggleButton(ref settings.toggleVendorsSellFor0, "buttonToggle_VendorsSellFor0",
                    "tooltip_VendorsSellFor0", nameof(settings.toggleVendorsSellFor0));

                MenuTools.ToggleButton(ref settings.toggleVendorsBuyFor0, "buttonToggle_VendorsBuyFor0",
                    "tooltip_VendorsBuyFor0", nameof(settings.toggleVendorsBuyFor0));
            }

            GL.EndVertical();
        }


        public static void BeneathTheStolenLands()
        {
            if (settings.settingShowDebugInfo)
            {
                if (Common.DLCEndless())
                {
                    GL.BeginVertical("box");
                    GL.BeginHorizontal();
                    settings.showBeneathTheStolenLandsCategory = GL.Toggle(settings.showBeneathTheStolenLandsCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_BeneathTheStolenLands")), GL.ExpandWidth(false));
                    if (!settings.showBeneathTheStolenLandsCategory)
                    {
                        GL.EndHorizontal();
                    }
                    else
                    {
                        MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("BeneathTheStolenLands");
                    }
                    GL.EndVertical();
                }
            }
        }

        public static void PartyOptions()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showPartyStatisticsCategory = GL.Toggle(settings.showPartyStatisticsCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_PartyStats")), GL.ExpandWidth(false));
            if (!settings.showPartyStatisticsCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("PartyOptions");

                GL.Space(10);

                GL.BeginHorizontal();
                Storage.statsFilterUnitEntityDataIndex = GL.SelectionGrid(Storage.statsFilterUnitEntityDataIndex, Storage.unitEntityDataArrayNoEnemies, 3);
                GL.EndHorizontal();
                var player = Game.Instance.Player;
                switch (Storage.statsFilterUnitEntityDataIndex)
                {
                    case 0:
                        Storage.statsUnitEntityData = player.Party;
                        break;
                    case 1:
                        Storage.statsUnitEntityData = player.ControllableCharacters;
                        break;
                    case 2:
                        Storage.statsUnitEntityData = player.ActiveCompanions;
                        break;
                    case 3:
                        Storage.statsUnitEntityData = player.AllCharacters;
                        break;
                    case 4:
                        Storage.statsUnitEntityData = Common.GetCustomCompanions();
                        break;
                    case 5:
                        Storage.statsUnitEntityData = Common.GetPets();
                        break;
                }

                if (Storage.statsFilterUnitEntityDataIndex != Storage.statsFilterUnitEntityDataIndexOld)
                {
                    Storage.reloadPartyStats = true;
                    Storage.statsFilterUnitEntityDataIndexOld = Storage.statsFilterUnitEntityDataIndex;
                }

                GL.Space(10);

                if (Storage.statsUnitEntityData.Any())
                {
                    if (Storage.reloadPartyStats)
                    {
                        Storage.statsSelectedControllableCharacterIndex = 0;
                        Storage.statsPartyMembers = Storage.statsUnitEntityData;
                        Storage.statsControllableCharacterNamesList.Clear();
                        foreach (var controllableCharacter in Storage.statsUnitEntityData)
                            Storage.statsControllableCharacterNamesList.Add(controllableCharacter.CharacterName);
                        Storage.reloadPartyStats = false;
                    }

                    if (!Storage.reloadPartyStats)
                    {
                        GL.BeginHorizontal();
                        Storage.statsSelectedControllableCharacterIndex = GL.SelectionGrid(Storage.statsSelectedControllableCharacterIndex, Storage.statsControllableCharacterNamesList.ToArray(), 3);
                        GL.EndHorizontal();

                        GL.Space(10);

                        CurrentHitPointsOptions();

                        GL.Space(10);

                        ChangeName();

                        GL.Space(10);

                        ChangeGender();

                        GL.Space(10);

                        ClassData();

                        GL.Space(10);

                        //Menu.RaceData();

                        //GL.Space(10);

                        /*
                        if (GL.Button(MenuTools.TextWithTooltip("button_RemoveEquippedItems", "tooltip_RemoveEquippedItems", false), GL.ExpandWidth(false)))
                        {
                            foreach (ItemEntity itemEntity in Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Inventory.Items)
                            {
                                if (itemEntity.Owner == Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor)
                                {
                                    itemEntity.HoldingSlot.RemoveItem();
                                }
                            }
                        }
                        GL.Space(10);*/

                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        if (GL.Button(
                            MenuTools.TextWithTooltip("button_ResetCharacterLevel", "tooltip_ResetCharacterLevel",
                                false), GL.ExpandWidth(false)))
                        {
                            var level = 21;
                            var xp = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex]
                                .Descriptor.Progression.Experience;
                            var xpTable = BlueprintRoot.Instance.Progression.XPTable;

                            for (var i = 20; i >= 1; i--)
                            {
                                var xpBonus = xpTable.GetBonus(i);

                                modLogger.Log(i + ": " + xpBonus + " | " + xp);

                                if (xp - xpBonus >= 0)
                                {
                                    modLogger.Log(i + ": " + (xp - xpBonus));
                                    level = i;
                                    break;
                                }
                            }

                            var type = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex]
                                .Descriptor.Progression.GetType();
                            var propertyInfo = type.GetProperty("CharacterLevel");
                            propertyInfo.SetValue(
                                Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                                    .Progression, level, null);
                        }

                        GL.EndHorizontal();

                        GL.Space(10);

                        GL.BeginHorizontal();
                        Storage.setCharLevel = GL.HorizontalSlider(Storage.setCharLevel, 1f, 20f, GL.Width(250f));
                        GL.Label($" {Mathf.RoundToInt(Storage.setCharLevel)}", GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        GL.BeginHorizontal();
                        if (GL.Button(
                            MenuTools.TextWithTooltip("button_SetCharacterLevel", "tooltip_SetCharacterLevel", "",
                                $" {Mathf.RoundToInt(Storage.setCharLevel)}" + " " + Strings.Parenthesis(Storage
                                    .statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName)),
                            GL.ExpandWidth(false)))
                        {
                            var type = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex]
                                .Descriptor.Progression.GetType();
                            var propertyInfoLvl = type.GetProperty("CharacterLevel");
                            propertyInfoLvl.SetValue(
                                Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                                    .Progression, Mathf.RoundToInt(Storage.setCharLevel), null);

                            var newXp = BlueprintRoot.Instance.Progression.XPTable.GetBonus(
                                Mathf.RoundToInt(Storage.setCharLevel));
                            var propertyInfoXp = type.GetProperty("Experience");
                            propertyInfoXp.SetValue(
                                Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                                    .Progression, newXp, null);
                        }

                        GL.EndHorizontal();
                        MenuTools.SingleLineLabel(Strings.GetText("warning_SetCharacterLevel"));
                        GL.EndVertical();

                        GL.Space(10);

                        MenuTools.UnitAlignment(Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex]);

                        GL.Space(10);

                        GL.BeginVertical("box");

                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Size")));
                        GL.BeginHorizontal();
                        Storage.partySelectedSizeIndex = GL.SelectionGrid(Storage.partySelectedSizeIndex, Storage.charSizeArray, 3);
                        GL.EndHorizontal();
                        GL.Space(10);
                        GL.BeginHorizontal();
                        if (GL.Button(
                            MenuTools.TextWithTooltip("button_SetSizeTo", "tooltip_SetSize", "",
                                $" {Storage.charSizeArray[Storage.partySelectedSizeIndex]}"), GL.ExpandWidth(false)))
                            Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.State
                                .Size = (Size)Storage.partySelectedSizeIndex;
                        GL.EndHorizontal();
                        GL.BeginHorizontal();
                        if (GL.Button(
                            MenuTools.TextWithTooltip("button_SetToOriginalSize", "tooltip_SetToOriginalSize", "",
                                $" ({Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.OriginalSize})"),
                            GL.ExpandWidth(false)))
                            Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.State
                                .Size = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex]
                                .Descriptor.OriginalSize;
                        GL.EndHorizontal();
                        MenuTools.SingleLineLabel(Strings.GetText("label_CurrentSize") + ": " +
                                                  Common.SizeToString(Storage
                                                      .statsPartyMembers[
                                                          Storage.statsSelectedControllableCharacterIndex].Descriptor
                                                      .State.Size));
                        GL.EndVertical();

                        GL.Space(10);

                        GL.BeginHorizontal();
                        GL.Label(MenuTools.TextWithTooltip("header_Statistics", "tooltip_Statistics", true));
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        settings.partyStatsAmount = GL.TextField(settings.partyStatsAmount, 10, GL.Width(100f));
                        settings.partyStatsAmount = MenuTools.IntTestSettingStage1(settings.partyStatsAmount);
                        settings.partyFinalStatsAmount = MenuTools.IntTestSettingStage2(settings.partyStatsAmount,
                            settings.partyFinalStatsAmount);
                        GL.EndHorizontal();

                        var charStats = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex]
                            .Descriptor.Stats;
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_AttributesBaseValues")));
                        foreach (var entry in Storage.statsAttributesDict)
                            MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                                settings.partyFinalStatsAmount);
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_SkillsRanks")));

                        MenuTools.ToggleButton(ref settings.toggleShowOnlyClassSkills,
                            "buttonToggle_ShowOnlyClassSkills", "tooltip_ShowOnlyClassSkills");

                        foreach (var entry in Storage.statsSkillsDict)
                            if (Strings.ToBool(settings.toggleShowOnlyClassSkills))
                            {
                                var stat = charStats.GetStat(entry.Value) as ModifiableValueSkill;
                                if (stat.ClassSkill && stat.BaseValue > 0)
                                    MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                                        settings.partyFinalStatsAmount);
                            }
                            else
                            {
                                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                                    settings.partyFinalStatsAmount, true);
                            }

                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_SocialSkillsBaseValues")));
                        foreach (var entry in Storage.statsSocialSkillsDict)
                            MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                                settings.partyFinalStatsAmount);
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_StatsSaves")));
                        foreach (var entry in Storage.statsSavesDict)
                            MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                                settings.partyFinalStatsAmount);
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_StatsCombat")));
                        foreach (var entry in Storage.statsCombatDict)
                            MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                                settings.partyFinalStatsAmount);
                        GL.Space(10);

                        GL.BeginHorizontal();
                        GL.Label(MenuTools.TextWithTooltip("header_PartyMultipliers", "tooltip_PartyMultipliers",
                            true));
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        settings.partyStatMultiplier =
                            GL.HorizontalSlider(settings.partyStatMultiplier, 0.1f, 10f, GL.Width(300f));
                        GL.Label($" {Math.Round(settings.partyStatMultiplier, 1)}", GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Attributes")));
                        foreach (var entry in Storage.statsAttributesDict)
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value,
                                Storage.statsPartyMembers, settings.partyStatMultiplier);
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Skills")));
                        foreach (var entry in Storage.statsSkillsDict)
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value,
                                Storage.statsPartyMembers, settings.partyStatMultiplier);
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_SocialSkills")));
                        foreach (var entry in Storage.statsSocialSkillsDict)
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value,
                                Storage.statsPartyMembers, settings.partyStatMultiplier);
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Saves")));
                        foreach (var entry in Storage.statsSavesDict)
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value,
                                Storage.statsPartyMembers, settings.partyStatMultiplier);
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Combat")));
                        foreach (var entry in Storage.statsCombatDict)
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value,
                                Storage.statsPartyMembers, settings.partyStatMultiplier);

                        GL.BeginHorizontal();
                        if (GL.Button(
                            MenuTools.TextWithTooltip("button_ExportCharInfo", "tooltip_ExportCharInfo", false),
                            GL.ExpandWidth(false)))
                        {
                            var charInfoTxt = new List<string>();
                            charInfoTxt.Add(
                                $"{Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}");
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_AttributesBaseValues"));
                            foreach (var entry in Storage.statsAttributesDict)
                                charInfoTxt.Add(
                                    $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_SkillsRanks"));
                            foreach (var entry in Storage.statsSkillsDict)
                                charInfoTxt.Add(
                                    $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_SocialSkillsBaseValues"));
                            foreach (var entry in Storage.statsSocialSkillsDict)
                                charInfoTxt.Add(
                                    $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_StatsSaves"));
                            foreach (var entry in Storage.statsSavesDict)
                                charInfoTxt.Add(
                                    $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_StatsCombat"));
                            foreach (var entry in Storage.statsCombatDict)
                                charInfoTxt.Add(
                                    $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");

                            File.WriteAllLines(
                                Path.Combine(Common.ExportPath(),
                                    $"{Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}.txt"),
                                charInfoTxt.ToArray());
                        }
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        GL.Label(" " + Strings.GetText("label_Location") +
                                 $": {Path.Combine(Common.ExportPath(), $"{Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}.txt")}", GL.ExpandWidth(false));
                        GL.EndHorizontal();

                        if (File.Exists(Storage.modEntryPath + Storage.charactersImportFolder + "\\" +
                                        Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex]
                                            .CharacterName + ".txt"))
                            if (GL.Button(
                                MenuTools.TextWithTooltip("button_ImportStatsFrom", "tooltip_ImportStatsFrom", "",
                                    $" {Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}.txt"),
                                GL.ExpandWidth(false)))
                            {
                                if (settings.settingCreateBackupBeforeImport)
                                {
                                    var charInfoTxt = new List<string>();
                                    charInfoTxt.Add(
                                        $"{Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}");
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_AttributesBaseValues"));
                                    foreach (var entry in Storage.statsAttributesDict)
                                        charInfoTxt.Add(
                                            $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_SkillsRanks"));
                                    foreach (var entry in Storage.statsSkillsDict)
                                        charInfoTxt.Add(
                                            $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_SocialSkillsBaseValues"));
                                    foreach (var entry in Storage.statsSocialSkillsDict)
                                        charInfoTxt.Add(
                                            $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_StatsSaves"));
                                    foreach (var entry in Storage.statsSavesDict)
                                        charInfoTxt.Add(
                                            $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_StatsCombat"));
                                    foreach (var entry in Storage.statsCombatDict)
                                        charInfoTxt.Add(
                                            $"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})");
                                    File.WriteAllLines(
                                        Path.Combine(Storage.modEntryPath + Storage.charactersImportFolder + "\\" +
                                                     Storage.statsPartyMembers[
                                                             Storage.statsSelectedControllableCharacterIndex]
                                                         .CharacterName + "_Backup.txt"), charInfoTxt.ToArray());
                                }

                                var lines = File.ReadAllLines(Storage.modEntryPath + Storage.charactersImportFolder +
                                                              "\\" + Storage
                                                                  .statsPartyMembers[
                                                                      Storage.statsSelectedControllableCharacterIndex]
                                                                  .CharacterName + ".txt");
                                lines = lines.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                lines = lines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                                for (var i = 0; i < lines.Length; i++)
                                    if (Regex.IsMatch(lines[i], @"[\x20A-Za-z()]+:\s*[0-9]+"))
                                    {
                                        var match = Regex.Match(lines[i], @"[\x20A-Za-z()]+:\s*[0-9]+");
                                        lines[i] = match.Value;
                                        var splitLine = lines[i].Split(':');
                                        var allStats = Storage.statsAttributesDict.Union(Storage.statsSkillsDict)
                                            .Union(Storage.statsSocialSkillsDict).Union(Storage.statsSavesDict)
                                            .Union(Storage.statsCombatDict).ToDictionary(k => k.Key, v => v.Value);
                                        if (allStats.TryGetValue(splitLine[0], out var statType) &&
                                            int.TryParse(splitLine[1], out var baseValue))
                                            charStats.GetStat(statType).BaseValue = baseValue;
                                    }
                                    else
                                    {
                                    }
                            }
                    }
                }
                else
                {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitFound"));
                }
            }

            GL.EndVertical();
        }

        public static void ChangeGender()
        {
            GL.BeginVertical("box");
            MenuTools.SingleLineLabel(MenuTools.TextWithTooltip("label_CustomGender", "tooltip_CustomGender", true));
            MenuTools.SingleLineLabel(Strings.GetText("warning_CustomGender"));


            if (Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.CustomGender
                .HasValue)
            {
                var gender = Common.GenderToSymbol(Storage
                    .statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.CustomGender);
                MenuTools.SingleLineLabel(Strings.GetText("label_CurrentCustomGender") + ": " + gender);
            }
            else
            {
                var gender = Common.GenderToSymbol(Storage
                    .statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Gender);
                MenuTools.SingleLineLabel(Strings.GetText("label_CurrentDefaultGender") + ": " + gender);
            }


            GL.BeginHorizontal();
            if (GL.Button(
                new GUIContent(Strings.GetText("button_SetTo") + RichText.Bold(" ♀"),
                    Strings.GetText("tooltip_CustomName")), GL.ExpandWidth(false)))
                Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.CustomGender =
                    Gender.Female;
            GL.Space(10);
            if (GL.Button(
                new GUIContent(Strings.GetText("button_SetTo") + RichText.Bold(" ♂"),
                    Strings.GetText("tooltip_CustomName")), GL.ExpandWidth(false)))
                Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.CustomGender =
                    Gender.Male;
            GL.Space(10);
            if (GL.Button(new GUIContent(Strings.GetText("button_SetToDefault"), Strings.GetText("tooltip_CustomName")),
                GL.ExpandWidth(false)))
                Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.CustomGender =
                    null;
            GL.EndHorizontal();

            GL.EndVertical();
        }

        public static void RaceData()
        {
            GL.BeginVertical("box");

            MenuTools.ToggleButton(ref settings.toggleShowClassDataOptions, "buttonToggle_ShowClassDataOptions",
                "tooltip_ShowClassDataOptions");

            if (Strings.ToBool(settings.toggleShowClassDataOptions))
            {
                GL.Space(10);

                MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_Experimental")));

                GL.BeginHorizontal();
                if (GL.Button(
                    RichText.Bold($"{settings.toggleExperimentalIUnderstand} " +
                                  Strings.GetText("buttonToggle_experimentalIUnderstand")), GL.ExpandWidth(false)))
                {
                    if (settings.toggleExperimentalIUnderstand == Storage.isFalseString)
                        settings.toggleExperimentalIUnderstand = Storage.isTrueString;
                    else if (settings.toggleExperimentalIUnderstand == Storage.isTrueString)
                        settings.toggleExperimentalIUnderstand = Storage.isFalseString;
                }

                GL.EndHorizontal();
                GL.Space(10);

                if (Strings.ToBool(settings.toggleExperimentalIUnderstand))
                {
                    if (GL.Button("Golbin"))
                    {
                        Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                            .Progression
                            .SetRace(Utilities.GetBlueprintByGuid<BlueprintRace>("9d168ca7100e9314385ce66852385451"));
                        Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Doll
                                .RacePreset =
                            Utilities.GetBlueprintByGuid<BlueprintRaceVisualPreset>("b0d1b9d4f75b12549bb23be18f8d1101");
                        Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.State.Size
                            = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                                .OriginalSize;
                    }

                    if (GL.Button("Elf"))
                    {
                        Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                            .Progression
                            .SetRace(Utilities.GetBlueprintByGuid<BlueprintRace>("25a5878d125338244896ebd3238226c8"));
                        Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Doll
                                .RacePreset =
                            Utilities.GetBlueprintByGuid<BlueprintRaceVisualPreset>("33c5153d85a687d40ab3a56f8c391f0c");
                        Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.State.Size
                            = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor
                                .OriginalSize;
                    }
                }
            }

            GL.EndVertical();
        }

        public static void EnemyStats()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showEnemyStatisticsCategory = GL.Toggle(settings.showEnemyStatisticsCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_EnemyStats")), GL.ExpandWidth(false));
            if (!settings.showEnemyStatisticsCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("EnemyStats");

                GL.Space(10);

                EnemyBaseHitPointsMultiplier();

                GL.Space(10);

                var enemyUnits = new List<UnitEntityData>();
                var enemyInStealth = false;
                var enemyAlive = false;
                Main.GetEnemyUnits(enemyUnits);

                for (var i = 0; i < Storage.enemyUnits.Count; i++)
                {
                    if (Storage.enemyUnits[i].Descriptor.State.IsInStealth == true)
                    {
                        Storage.enemyUnitsNamesList[i] = Storage.enemyUnitsNamesList[i] + " " +
                                                         Strings.Parenthesis(Strings.GetText("label_InStealth"));
                        enemyInStealth = true;
                    }

                    if (Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.HPLeft > 0) enemyAlive = true;
                }

                GL.BeginHorizontal();
                Storage.enemyUnitIndex = GL.SelectionGrid(Storage.enemyUnitIndex, Storage.enemyUnitsNamesList.ToArray(), 3);
                GL.EndHorizontal();

                if (enemyUnits.Any())
                {
                    GL.Space(10);

                    GL.BeginVertical("box");
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Size")));
                    GL.BeginHorizontal();
                    Storage.enemiesSelectedSizeIndex =
                        GL.SelectionGrid(Storage.enemiesSelectedSizeIndex, Storage.charSizeArray, 4);
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    if (GL.Button(
                        MenuTools.TextWithTooltip("button_SetSizeTo", "tooltip_SetSize", "",
                            $" {Storage.charSizeArray[Storage.enemiesSelectedSizeIndex]}"), GL.ExpandWidth(false)))
                        Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.State.Size =
                            (Size)Storage.enemiesSelectedSizeIndex;
                    GL.EndHorizontal();
                    GL.BeginHorizontal();
                    if (GL.Button(
                        MenuTools.TextWithTooltip("button_SetToOriginalSize", "tooltip_SetToOriginalSize", "",
                            $" ({Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.OriginalSize})"),
                        GL.ExpandWidth(false)))
                        Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.State.Size =
                            Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.OriginalSize;
                    GL.EndHorizontal();
                    MenuTools.SingleLineLabel(Strings.GetText("label_CurrentSize") + ": " +
                                              Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.State.Size);

                    GL.EndVertical();

                    GL.Space(10);

                    GL.Space(10);
                    MenuTools.UnitAlignment(Storage.enemyUnits[Storage.enemyUnitIndex]);

                    GL.Space(10);

                    GL.BeginHorizontal();
                    if (Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.HPLeft > 0)
                    {
                        if (GL.Button(Strings.GetText("button_Kill"), GL.ExpandWidth(false)))
                            Common.Kill(Storage.enemyUnits[Storage.enemyUnitIndex]);
                        if (GL.Button(Strings.GetText("button_Panic"), GL.ExpandWidth(false)))
                            enemyUnits[Storage.enemyUnitIndex].Descriptor.AddFact(
                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                    "cf0e277e6b785f449bbaf4e993b556e0"), (MechanicsContext)null, new FeatureParam());
                        if (GL.Button(Strings.GetText("button_Freeze"), GL.ExpandWidth(false)))
                            enemyUnits[Storage.enemyUnitIndex].Descriptor.AddFact(
                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                    "af1e2d232ebbb334aaf25e2a46a92591"), (MechanicsContext)null, new FeatureParam());
                        if (GL.Button(Strings.GetText("button_MakeCower"), GL.ExpandWidth(false)))
                            enemyUnits[Storage.enemyUnitIndex].Descriptor.AddFact(
                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                    "6062e3a8206a4284d867cbb7120dc091"), (MechanicsContext)null, new FeatureParam());
                        if (GL.Button(Strings.GetText("button_SetOnFire"), GL.ExpandWidth(false)))
                            enemyUnits[Storage.enemyUnitIndex].Descriptor.AddFact(
                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                    "315acb0b29671f74c8c7cc062b23b9d6"), (MechanicsContext)null, new FeatureParam());
                    }

                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    if (Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.State.IsInStealth)
                        if (GL.Button(Strings.GetText("button_RevealFromStealth"), GL.ExpandWidth(false)))
                            Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.State.IsInStealth = false;
                    GL.EndHorizontal();

                    if (enemyAlive)
                    {
                        GL.BeginHorizontal();
                        if (GL.Button(Strings.GetText("button_KillAll"), GL.ExpandWidth(false)))
                            foreach (var enemy in Storage.enemyUnits)
                                Common.Kill(enemy);
                        if (GL.Button(Strings.GetText("button_PanicAll"), GL.ExpandWidth(false)))
                            foreach (var enemy in Storage.enemyUnits)
                                enemy.Descriptor.AddFact(
                                    (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                        "cf0e277e6b785f449bbaf4e993b556e0"), (MechanicsContext)null,
                                    new FeatureParam());
                        if (GL.Button(Strings.GetText("button_FreezeAll"), GL.ExpandWidth(false)))
                            foreach (var enemy in Storage.enemyUnits)
                                enemy.Descriptor.AddFact(
                                    (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                        "af1e2d232ebbb334aaf25e2a46a92591"), (MechanicsContext)null,
                                    new FeatureParam());
                        if (GL.Button(Strings.GetText("button_MakeAllCower"), GL.ExpandWidth(false)))
                            foreach (var enemy in Storage.enemyUnits)
                                enemy.Descriptor.AddFact(
                                    (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                        "6062e3a8206a4284d867cbb7120dc091"), (MechanicsContext)null,
                                    new FeatureParam());
                        if (GL.Button(Strings.GetText("button_SetAllOnFire"), GL.ExpandWidth(false)))
                            foreach (var enemy in Storage.enemyUnits)
                                enemy.Descriptor.AddFact(
                                    (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                        "315acb0b29671f74c8c7cc062b23b9d6"), (MechanicsContext)null,
                                    new FeatureParam());
                        GL.EndHorizontal();
                    }

                    if (enemyInStealth)
                    {
                        GL.BeginHorizontal();
                        if (GL.Button(Strings.GetText("button_RevealAllFromStealth"), GL.ExpandWidth(false)))
                            foreach (var enemy in Storage.enemyUnits)
                                enemy.Descriptor.State.IsInStealth = false;
                        GL.EndHorizontal();
                    }

                    GL.BeginHorizontal();
                    settings.enemyStatsAmount = GL.TextField(settings.enemyStatsAmount, 10, GL.Width(85f));

                    settings.enemyStatsAmount = MenuTools.IntTestSettingStage1(settings.enemyStatsAmount);
                    settings.enemyFinalStatsAmount =
                        MenuTools.IntTestSettingStage2(settings.enemyStatsAmount, settings.enemyFinalStatsAmount);
                    GL.EndHorizontal();

                    var charStats = Storage.enemyUnits[Storage.enemyUnitIndex].Descriptor.Stats;
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_AttributesBaseValues")));
                    foreach (var entry in Storage.statsAttributesDict)
                        MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                            settings.enemyFinalStatsAmount);
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_SkillsRanks")));
                    foreach (var entry in Storage.statsSkillsDict)
                        MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                            settings.enemyFinalStatsAmount);
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_SocialSkillsBaseValues")));
                    foreach (var entry in Storage.statsSocialSkillsDict)
                        MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                            settings.enemyFinalStatsAmount);
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_StatsSaves")));
                    foreach (var entry in Storage.statsSavesDict)
                        MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                            settings.enemyFinalStatsAmount);
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_StatsCombat")));
                    foreach (var entry in Storage.statsCombatDict)
                        MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value,
                            settings.enemyFinalStatsAmount);
                    GL.Space(10);
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_EnemyMultipliers")));

                    GL.BeginHorizontal();
                    settings.enemyStatMultiplier =
                        GL.HorizontalSlider(settings.enemyStatMultiplier, 0.1f, 10f, GL.Width(300f));
                    GL.Label($" {Math.Round(settings.enemyStatMultiplier, 1)}", GL.ExpandWidth(false));
                    GL.EndHorizontal();
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Attributes")));
                    foreach (var entry in Storage.statsAttributesDict)
                        MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.enemyUnits,
                            settings.enemyStatMultiplier);
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Skills")));
                    foreach (var entry in Storage.statsSkillsDict)
                        MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.enemyUnits,
                            settings.enemyStatMultiplier);
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_SocialSkills")));
                    foreach (var entry in Storage.statsSocialSkillsDict)
                        MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.enemyUnits,
                            settings.enemyStatMultiplier);
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Saves")));
                    foreach (var entry in Storage.statsSavesDict)
                        MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.enemyUnits,
                            settings.enemyStatMultiplier);
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Combat")));
                    foreach (var entry in Storage.statsCombatDict)
                        MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.enemyUnits,
                            settings.enemyStatMultiplier);
                }
                else
                {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoEnemies"));
                }
            }
            GL.EndVertical();
        }

        public static MultiplierCustom enemyBaseHitPointMultiplier = new MultiplierCustom(0.1f, 10f);
        public static void EnemyBaseHitPointsMultiplier()
        {
            enemyBaseHitPointMultiplier.LoadSettings(settings.enemyBaseHitPointsMultiplier, settings.customEnemyBaseHitPointsMultiplier, settings.useCustomEnemyBaseHitPointsMultiplier);
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleEnemyBaseHitPointsMultiplier, "label_EnemyBaseHitPointsMultiplier", "tooltip_EnemyBaseHitPointsMultiplier");
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(EnemyBaseHitPointsMultiplier));
            if (Strings.ToBool(settings.toggleEnemyBaseHitPointsMultiplier))
            {
                GL.BeginVertical("box");
                enemyBaseHitPointMultiplier.Render(ref settings.enemyBaseHitPointsMultiplier, ref settings.customEnemyBaseHitPointsMultiplier, ref settings.useCustomEnemyBaseHitPointsMultiplier);
                GL.EndVertical();
            }
            GL.EndVertical();
        }

        public static void ItemsEquipment()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showItemsCategory = GL.Toggle(settings.showItemsCategory,
                RichText.MainCategoryFormat(Strings.GetText("mainCategory_Items")), GL.ExpandWidth(false));

            if (!settings.showItemsCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("ItemsEquipment");

                GL.Space(10);

                GL.BeginHorizontal();
                if (GL.Button(Strings.GetText("button_RestoreAllItemCharges"), GL.ExpandWidth(false)))
                    Cheats.RestoreAllItemCharges();
                GL.EndHorizontal();

                MenuTools.ToggleButton(ref settings.toggleItemsWeighZero, "buttonToggle_ItemsWeighZero",
                    "tooltip_ItemsWeighZero", nameof(settings.toggleItemsWeighZero));

                MenuTools.ToggleButton(ref settings.toggleDexBonusLimit99, "buttonToggle_DexBonusLimit99",
                    "tooltip_DexBonusLimit99", nameof(settings.toggleDexBonusLimit99));

                ArmourChecksPenalty0();

                MenuTools.ToggleButton(ref settings.toggleInfiniteItems, "buttonToggle_UnlimitedItemCharges",
                    "tooltip_UnlimitedItemCharges", nameof(settings.toggleInfiniteItems));

                MenuTools.ToggleButton(ref settings.toggleEquipmentRestrictions,
                    "buttonToggle_IgnoreEquipmentRestrictions", "tooltip_IgnoreEquipmentRestrictions",
                    nameof(settings.toggleEquipmentRestrictions));

                MenuTools.ToggleButton(ref settings.toggleRestoreItemChargesAfterCombat,
                    "button_RestoreItemChargesAfterCombat", "tooltip_RestoreItemChargesAfterCombat",
                    nameof(settings.toggleRestoreItemChargesAfterCombat));

                GL.Space(10);

                ShowInventory();

                GL.Space(10);

                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_SearchAndAddItems")));
                if (!Game.Instance.Player.IsInCombat)
                {
                    GL.BeginVertical("box");
                    GL.Space(10);
                    GL.BeginHorizontal();
                    settings.addItemIdentified = GL.Toggle(settings.addItemIdentified, new GUIContent(RichText.Bold(Strings.GetText("toggle_AddItemIdentified")), Strings.GetText("tooltip_AddItemIdentified")), GL.ExpandWidth(false));
                    GL.EndHorizontal();
                    GL.Space(10);
                    GL.BeginHorizontal();
                    settings.showItemFavourites = GL.Toggle(settings.showItemFavourites, new GUIContent(RichText.Bold(Strings.GetText("headerOption_ShowFavourites"))), GL.ExpandWidth(false));
                    GL.EndHorizontal();
                    if (settings.showItemFavourites)
                    {
                        if (Storage.itemFavouritesLoad)
                        {
                            Main.RefreshItemFavourites();
                            Storage.itemFavouritesLoad = false;
                        }

                        if (!Storage.itemFavourites.Any())
                        {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                        }
                        else
                        {
                            for (var i = 0; i < Storage.itemFavourites.Count; i++)
                            {
                                GL.BeginHorizontal();
                                Storage.toggleItemFavouriteDescription.Add(false);
                                Storage.toggleItemFavouriteDescription[i] = GL.Toggle(Storage.toggleItemFavouriteDescription[i], $" {Storage.itemFavouritesNames[i]}", GL.ExpandWidth(false));
                                GL.FlexibleSpace();

                                if (Main.CraftMagicItems.ModIsActive() && Storage.itemFavourites[i].Contains(Storage.scribeScrollBlueprintPrefix))
                                {
                                    GL.Label(Strings.Parenthesis(Storage.scribeScrollBlueprintPrefix), GL.ExpandWidth(false));
                                }
                                else if (Main.CraftMagicItems.ModIsActive() && Storage.itemFavourites[i].Contains(Storage.craftMagicItemsBlueprintPrefix))
                                {
                                    GL.Label(Strings.Parenthesis(Storage.craftMagicItemsBlueprintPrefix), GL.ExpandWidth(false));
                                }

                                if (settings.multipleItems == true)
                                {
                                    if (GL.Button(Strings.GetText("button_AddGuid"), GL.ExpandWidth(false)))
                                    {
                                        settings.itemGuid = settings.itemGuid + "," + Storage.itemFavouritesGuids[i];
                                    }
                                }
                                if (GL.Button(Strings.GetText("button_SetGuid"), GL.ExpandWidth(false)))
                                {
                                    settings.itemGuid = Storage.itemFavouritesGuids[i];
                                }
                                if (GL.Button(Strings.GetText("button_Receive") + $" {settings.finalItemAmount} {Storage.itemFavouritesNames[i]}", GL.Width(400f)))
                                {
                                    MenuTools.AddSingleItemAmount(Storage.itemFavouritesGuids[i], settings.finalItemAmount, settings.addItemIdentified);
                                }
                                if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                {
                                    Storage.itemFavouritesGuids.Remove(Storage.itemFavouritesGuids[i]);
                                    Storage.itemFavouritesLoad = true;
                                }
                                GL.EndHorizontal();

                                ItemDetails(Storage.toggleItemFavouriteDescription[i], Storage.itemFavouritesGuids[i]);
                            }

                            GL.Space(10);
                            GL.BeginHorizontal();
                            if (GL.Button(Strings.GetText("button_AddFavouritesToInventory"), GL.ExpandWidth(false)))
                                Main.ModEntry.OnModActions.Push(m =>
                                {
                                    for (var i = 0; i < Storage.itemFavouritesGuids.Count; i++)
                                        if (Main.ExcludeGuid != Storage.itemFavouritesGuids[i])
                                            MenuTools.AddSingleItemAmount(Storage.itemFavouritesGuids[i], 1, settings.addItemIdentified);
                                });
                            GL.EndHorizontal();

                            MenuTools.ExportCopyGuidsNamesButtons(Storage.itemFavouritesGuids.ToArray(), Storage.itemFavouritesNames.ToArray(), "item-favourites");
                        }

                        if (Storage.itemFavourites != Storage.itemFavouritesGuids)
                            Storage.itemFavourites = Storage.itemFavouritesGuids;
                    }

                    GL.Space(10);

                    GL.BeginHorizontal();
                    settings.showItemSets = GL.Toggle(settings.showItemSets, new GUIContent(RichText.Bold(Strings.GetText("headerOption_ShowItemSets"))), GL.ExpandWidth(false));
                    GL.EndHorizontal();
                    if (settings.showItemSets)
                    {
                        MenuTools.SingleLineLabel(Strings.GetText("warning_UnresposiveAddingItems"));
                        string[] typeIn = { };

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllArmour"),
                            typeIn = new[] { "BlueprintItemArmor" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllBelts"),
                            typeIn = new[] { "BlueprintItemEquipmentBelt" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllFootwear"),
                            typeIn = new[] { "BlueprintItemEquipmentFeet" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllGloves"),
                            typeIn = new[] { "BlueprintItemEquipmentGloves" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllHandSimple"),
                            typeIn = new[] { "BlueprintItemEquipmentHandSimple" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllHeadwear"),
                            typeIn = new[] { "BlueprintItemEquipmentHead" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllKeys"),
                            typeIn = new[] { "BlueprintItemKey" });

                        GL.BeginHorizontal();
                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllMiscellaneousItems"),
                            typeIn = new[] { "BlueprintItem" });
                        if (GL.Button(Strings.GetText("button_Add6Rations"), GL.ExpandWidth(false)))
                            MenuTools.AddSingleItemAmount("efa6c2ee9e630384188a50b1ce6600fe", 6);
                        if (GL.Button(Strings.GetText("button_AddAllArtifactsAndRelics"), GL.ExpandWidth(false)))
                        {
                            Main.ModEntry.OnModActions.Push(m =>
                            {
                                MenuTools.AddSingleItemAmount("a513944ab55fe1b43846a6f78331106c", 7);
                                MenuTools.AddSingleItemAmount("562eaf7481f1daf4baf199eb61ad5e9a", 1);
                                MenuTools.AddSingleItemAmount("61532117b25583048a9891954df5abc3", 1);
                                MenuTools.AddSingleItemAmount("b2e2b1f9388942c4394a6da390d5da43", 1);
                                MenuTools.AddSingleItemAmount("d3fe153c324901f4896502c84bb3c459", 1);
                                MenuTools.AddSingleItemAmount("3cb994fc827107140b790ac3b48ac701", 1);

                                MenuTools.AddSingleItemAmount("4bf54205cc8503446bd4d50016399285", 10);
                                MenuTools.AddSingleItemAmount("0102386282d8ca248bc8c5d0dc419a84", 1);
                                MenuTools.AddSingleItemAmount("d9975af6562358142bc3ebaf3e519513", 1);
                                MenuTools.AddSingleItemAmount("8379e20b955a87a4b85c21edef5d730b", 1);
                                MenuTools.AddSingleItemAmount("fac63f8aab736c5448ef775ee7250202", 1);
                                MenuTools.AddSingleItemAmount("1df75540183c031479a6531169033c0f", 1);

                                MenuTools.AddSingleItemAmount("6ab5e33d31f85e24f896c2460f82371e", 10);
                                MenuTools.AddSingleItemAmount("9de4b55f4c0a5264d8ff3796f3f70746", 1);
                                MenuTools.AddSingleItemAmount("96f746d27e0c00f428619df19269f467", 1);
                                MenuTools.AddSingleItemAmount("d7ed5c74f53301f4c81d618fdab53497", 1);
                                MenuTools.AddSingleItemAmount("ad5aa64554127dd4293c0f8756ef0a8e", 1);
                                MenuTools.AddSingleItemAmount("fd51248ade706fc4480c085dbf9dd839", 1);

                                MenuTools.AddSingleItemAmount("4bb8dd73910b8524188984faad1b3fb8", 12);
                                MenuTools.AddSingleItemAmount("3878c9c7d2ef3be48a009824ec79377d", 1);
                                MenuTools.AddSingleItemAmount("e12fe25f16575ac458305f8f485831dd", 1);
                                MenuTools.AddSingleItemAmount("816044bcf51aaa846a768f97aaad795e", 1);
                                MenuTools.AddSingleItemAmount("f71d5d37b69e5d141aa04c6ad5b27212", 1);
                                MenuTools.AddSingleItemAmount("e2a8ca9df166bb143987f0516a2854a3", 1);

                                MenuTools.AddSingleItemAmount("9b1c8421ee354004881651345840b9e4", 16);
                                MenuTools.AddSingleItemAmount("85880df1eadc4f343bfc3522a4d18f45", 1);
                                MenuTools.AddSingleItemAmount("cd462bb1c71b258448386c760d65b699", 1);
                                MenuTools.AddSingleItemAmount("12531e061aef4db40b337defa211512a", 1);
                                MenuTools.AddSingleItemAmount("e60418c921346c84f8573fd9e87571d4", 1);
                                MenuTools.AddSingleItemAmount("c4c9d4fdd66d5af468a388808157d644", 1);

                                MenuTools.AddSingleItemAmount("1c8862cdbdc5dcf49839be298851ba98", 16);
                                MenuTools.AddSingleItemAmount("68b4ae2a51823db41a335d7890172754", 1);
                                MenuTools.AddSingleItemAmount("a8932aa7c77f64546b1e92a5b5660802", 1);
                                MenuTools.AddSingleItemAmount("de37597119ec6a64ba1b36c1074bffab", 1);
                                MenuTools.AddSingleItemAmount("56835cc33d374964aa4539ce0b9611e1", 1);
                                MenuTools.AddSingleItemAmount("9bb0730d67d20324e9304ff4adfa7286", 1);

                                MenuTools.AddSingleItemAmount("3cca2f4d03e7733459c14c4a6544ddc8", 17);
                                MenuTools.AddSingleItemAmount("1fffb377497479447918553b1ea15922", 1);
                                MenuTools.AddSingleItemAmount("0afb3e721d4dcf2439272d7c50edc3ec", 1);
                                MenuTools.AddSingleItemAmount("8af9e12f734d16342b6fa4f7269cedd4", 1);
                                MenuTools.AddSingleItemAmount("a8eb937eb6c51244bbc73a683f0b731d", 1);
                                MenuTools.AddSingleItemAmount("d0d6f17f68411bc48b60675debf745a4", 1);

                                MenuTools.AddSingleItemAmount("4fe3d2e33319b9745865a86cb2d2cc9a", 1);
                                MenuTools.AddSingleItemAmount("ba61f03932816c04c911bd1b14c3bee7", 1);
                                MenuTools.AddSingleItemAmount("935751bcb24deea4892373237a596fe9", 1);
                                MenuTools.AddSingleItemAmount("ce40d439d0a0ce94290dfe2710808659", 1);
                                MenuTools.AddSingleItemAmount("0207d04092a1ff245a6140a6d0c7435b", 1);
                            });
                        }

                        GL.EndHorizontal();

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllNeckItems"),
                            typeIn = new[] { "BlueprintItemEquipmentNeck" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllNotes"),
                            typeIn = new[] { "BlueprintItemNote" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllRings"),
                            typeIn = new[] { "BlueprintItemEquipmentRing" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllShields"),
                            typeIn = new[] { "BlueprintItemShield" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllShoulderItems"),
                            typeIn = new[] { "BlueprintItemEquipmentShoulders" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllUsableItems"),
                            typeIn = new[] { "BlueprintItemEquipmentUsable" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllWeapons"),
                            typeIn = new[] { "BlueprintItemWeapon" });

                        Main.CreateFilteredItemSet(Strings.GetText("button_AddAllWristItems"),
                            typeIn = new[] { "BlueprintItemEquipmentWrist" });

                        GL.Space(10);

                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("button_CustomSets") + ":", GL.ExpandWidth(false));
                        GL.FlexibleSpace();
                        if (GL.Button(Strings.GetText("button_LoadRefresh"), GL.ExpandWidth(false)))
                            try
                            {
                                if (settings.settingSearchForCsv)
                                {
                                    Storage.itemSetsCsv = Directory.GetFiles(Storage.modEntryPath + Storage.itemSetsFolder, "*.csv");
                                    Array.Sort<string>(Storage.itemSetsCsv);
                                }

                                if (settings.settingSearchForTxt)
                                {
                                    Storage.itemSetsTxt = Directory.GetFiles(Storage.modEntryPath + Storage.itemSetsFolder, "*.txt");
                                    Array.Sort<string>(Storage.itemSetsTxt);
                                }
                            }
                            catch (IOException exception)
                            {
                                modLogger.Log(exception.ToString());
                            }
                        GL.EndHorizontal();

                        if (settings.settingSearchForCsv)
                        {
                            GL.Space(5);
                            Main.GetCustomItemSets(Storage.itemSetsCsv, Storage.previewItemSetsStringsCsv, Storage.toggleItemSetsPreviewCsv);
                        }

                        if (settings.settingSearchForTxt)
                        {
                            GL.Space(5);
                            Main.GetCustomItemSets(Storage.itemSetsTxt, Storage.previewItemSetsStringsCsv, Storage.toggleItemSeitsPreviewTxt);
                        }
                    }

                    GL.EndVertical();
                    GL.Space(10);

                    GL.BeginHorizontal();
                    GL.Label(MenuTools.TextWithTooltip("label_AssetGuid", "tooltip_AssetGuid", "", ": "), GL.ExpandWidth(false));
                    settings.itemGuid = GL.TextField(settings.itemGuid.Trim().TrimStart(','), GL.Width(800f));
                    if (!settings.itemGuid.Contains(Main.ExcludeGuid)) Storage.errorString = "";
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_Amount") + ": ", GL.ExpandWidth(false));
                    settings.itemAmount = GL.TextField(settings.itemAmount, 2, GL.Width(50f));
                    settings.itemAmount = MenuTools.IntTestSettingStage1(settings.itemAmount);
                    settings.finalItemAmount = MenuTools.IntTestSettingStage2(settings.itemAmount, settings.finalItemAmount);
                    GL.Space(80);
                    settings.multipleItems = GL.Toggle(settings.multipleItems, new GUIContent(Strings.ToggleSpaceLeftFormat(Strings.GetText("toggle_AddMultipleItems")), Strings.GetText("tooltip_AddMultipleItems")), GL.ExpandWidth(false));
                    GL.EndHorizontal();

                    if (settings.multipleItems)
                    {
                        Storage.itemMultipleGuid = settings.itemGuid.Trim().Split(',').Select(p => p.Trim()).ToList();
                        var anyInvalid = Storage.itemMultipleGuid.Find(g => null == Utilities.GetBlueprintByGuid<BlueprintItem>(g));

                        if (string.IsNullOrEmpty(anyInvalid))
                        {
                            GL.BeginHorizontal();
                            var items = Strings.GetText("misc_items");
                            var times = Strings.GetText("misc_times");
                            if (Storage.itemMultipleGuid.Count == 1) items = Strings.GetText("misc_item");
                            if (settings.finalItemAmount == 1) times = Strings.GetText("misc_time");

                            if (GL.Button(Strings.GetText("button_Receive") + $" {Storage.itemMultipleGuid.Count} {items} {settings.finalItemAmount} {times}", GL.ExpandWidth(false)))
                                for (var i = 0; i < Storage.itemMultipleGuid.Count; i++)
                                    if (Main.ExcludeGuid == Storage.itemMultipleGuid[i])
                                    {
                                        Storage.errorString = Strings.GetText("error_EkunQ2");
                                        modLogger.Log(Storage.notificationEkunQ2rewardArmor);
                                    }
                                    else
                                        MenuTools.AddSingleItemAmount(Storage.itemMultipleGuid[i], settings.finalItemAmount, settings.addItemIdentified);

                            GL.Label(Storage.errorString);
                            GL.EndHorizontal();
                        }
                        else
                        {
                            GL.BeginHorizontal();
                            if (GL.Button(Strings.GetText("message_NoValidGuids"), GL.ExpandWidth(false))) MenuTools.Beep();
                            GL.EndHorizontal();
                        }
                    }
                    else
                    {
                        if (Utilities.GetBlueprintByGuid<BlueprintItem>(settings.itemGuid) != null)
                        {
                            GL.BeginHorizontal();
                            var itemByGuid = Utilities.GetBlueprintByGuid<BlueprintItem>(settings.itemGuid);
                            if (GL.Button(Strings.GetText("button_Receive") + $" {settings.finalItemAmount} {itemByGuid.Name}", GL.ExpandWidth(false)))
                            {
                                if (settings.itemGuid != Main.ExcludeGuid)
                                {
                                    MenuTools.AddSingleItemAmount(settings.itemGuid, settings.finalItemAmount,
                                        settings.addItemIdentified);
                                }
                                else
                                {
                                    Storage.errorString = Strings.GetText("error_EkunQ2");
                                    modLogger.Log(Storage.notificationEkunQ2rewardArmor);
                                }
                            }

                            GL.Label(Storage.errorString);
                            GL.EndHorizontal();

                            GL.Space(10);

                            if (settings.itemGuid != Main.ExcludeGuid)
                            {
                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                settings.showItemInfo = GL.Toggle(settings.showItemInfo, Strings.GetText("label_DisplayItemInformation"), GL.ExpandWidth(false));
                                GL.EndHorizontal();

                                ItemDetails(settings.showItemInfo, itemByGuid.AssetGuid);
                                GL.EndVertical();
                            }
                        }
                        else
                        {
                            GL.BeginHorizontal();
                            if (GL.Button(Strings.GetText("message_NoValidGuid"), GL.ExpandWidth(false))) MenuTools.Beep();
                            GL.EndHorizontal();
                        }
                    }

                    GL.Space(10);

                    GL.BeginVertical("box");
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_ItemSearch")));

                    GL.BeginHorizontal();
                    settings.itemSearch = GL.TextField(settings.itemSearch, GL.Width(500f));
                    if (GL.Button(RichText.Bold(Strings.GetText("header_Search")), GL.ExpandWidth(false)))
                    {
                        if (settings.filterButtonText == Strings.GetText("misc_Disable"))
                            Main.SearchValidItems(Storage.validItemTypesFiltered);
                        else if (settings.filterButtonText == Strings.GetText("misc_Enable"))
                            Main.SearchValidItems(Storage.validItemTypes);
                    }
                    GL.EndHorizontal();

                    if (Main.CraftMagicItems.ModIsActive())
                    {
                        MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_CraftMagicItemsSearchItemInfo")));

                        if (GL.Button(
                            $"{settings.toggleSearchOnlyCraftMagicItems} " +
                            Strings.GetText("buttonToggle_SearchOnlyCraftMagicItems"), GL.ExpandWidth(false)))
                        {
                            if (Strings.ToBool(settings.toggleSearchOnlyCraftMagicItems))
                                settings.toggleSearchOnlyCraftMagicItems = Storage.isFalseString;
                            else
                                settings.toggleSearchOnlyCraftMagicItems = Storage.isTrueString;

                            if (settings.filterButtonText == Strings.GetText("misc_Disable"))
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                            else if (settings.filterButtonText == Strings.GetText("misc_Enable"))
                                Main.SearchValidItems(Storage.validItemTypes);
                        }
                    }

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_SearchBy") + ": ", GL.ExpandWidth(false));

                    if (GL.Button($"{settings.toggleSearchByItemObjectName} " + Strings.GetText("label_ObjectName"),
                        GL.ExpandWidth(false)))
                    {
                        if (Strings.ToBool(settings.toggleSearchByItemObjectName))
                            settings.toggleSearchByItemObjectName = Storage.isFalseString;
                        else
                            settings.toggleSearchByItemObjectName = Storage.isTrueString;

                        if (settings.filterButtonText == Strings.GetText("misc_Disable"))
                            Main.SearchValidItems(Storage.validItemTypesFiltered);
                        else if (settings.filterButtonText == Strings.GetText("misc_Enable"))
                            Main.SearchValidItems(Storage.validItemTypes);
                    }

                    if (GL.Button($"{settings.toggleSearchByItemName} " + Strings.GetText("label_ItemName"),
                        GL.ExpandWidth(false)))
                    {
                        if (Strings.ToBool(settings.toggleSearchByItemName))
                            settings.toggleSearchByItemName = Storage.isFalseString;
                        else
                            settings.toggleSearchByItemName = Storage.isTrueString;

                        if (settings.filterButtonText == Strings.GetText("misc_Disable"))
                            Main.SearchValidItems(Storage.validItemTypesFiltered);
                        else if (settings.filterButtonText == Strings.GetText("misc_Enable"))
                            Main.SearchValidItems(Storage.validItemTypes);
                    }

                    if (GL.Button($"{settings.toggleSearchByItemDescription} " + Strings.GetText("label_Description"),
                        GL.ExpandWidth(false)))
                    {
                        if (Strings.ToBool(settings.toggleSearchByItemDescription))
                            settings.toggleSearchByItemDescription = Storage.isFalseString;
                        else
                            settings.toggleSearchByItemDescription = Storage.isTrueString;

                        if (settings.filterButtonText == Strings.GetText("misc_Disable"))
                            Main.SearchValidItems(Storage.validItemTypesFiltered);
                        else if (settings.filterButtonText == Strings.GetText("misc_Enable"))
                            Main.SearchValidItems(Storage.validItemTypes);
                    }

                    if (GL.Button($"{settings.toggleSearchByItemFlavorText} " + Strings.GetText("label_FlavourText"),
                        GL.ExpandWidth(false)))
                    {
                        if (Strings.ToBool(settings.toggleSearchByItemFlavorText))
                            settings.toggleSearchByItemFlavorText = Storage.isFalseString;
                        else
                            settings.toggleSearchByItemFlavorText = Storage.isTrueString;

                        if (settings.filterButtonText == Strings.GetText("misc_Disable"))
                            Main.SearchValidItems(Storage.validItemTypesFiltered);
                        else if (settings.filterButtonText == Strings.GetText("misc_Enable"))
                            Main.SearchValidItems(Storage.validItemTypes);
                    }

                    GL.EndHorizontal();

                    GL.Space(10);
                    GL.BeginVertical("box");
                    GL.BeginHorizontal();
                    if (GL.Button(RichText.Bold($"{settings.filterButtonText} " + Strings.GetText("button_Filters")),
                        GL.ExpandWidth(false)))
                    {
                        if (settings.filterButtonText == Strings.GetText("misc_Enable"))
                        {
                            Main.SearchValidItems(Storage.validItemTypesFiltered);
                            settings.filterButtonText = Strings.GetText("misc_Disable");
                        }
                        else
                        {
                            Main.SearchValidItems(Storage.validItemTypes);
                            settings.filterButtonText = Strings.GetText("misc_Enable");
                        }
                    }

                    GL.EndHorizontal();

                    if (settings.filterButtonText == Strings.GetText("misc_Disable"))
                    {
                        GL.BeginHorizontal();
                        if (GL.Button(Strings.GetText("button_ResetFilters"), GL.ExpandWidth(false)))
                        {
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

                            Main.ResetFilteredItems();

                            Main.SearchValidItems(Storage.validItemTypesFiltered);
                        }

                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        if (GL.Button($"{settings.toggleFilterArmours} " + Strings.GetText("label_Armours"),
                            GL.ExpandWidth(false)))
                        {
                            if (Strings.ToBool(settings.toggleFilterArmours))
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemArmor");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterArmours = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemArmor");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterArmours = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterBelts} " + Strings.GetText("label_Belts"),
                            GL.ExpandWidth(false)))
                        {
                            if (Strings.ToBool(settings.toggleFilterBelts))
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemEquipmentBelt");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterBelts = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemEquipmentBelt");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterBelts = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterFootwear} " + Strings.GetText("label_Footwear"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterFootwear == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemEquipmentFeet");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterFootwear = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemEquipmentFeet");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterFootwear = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterGloves} " + Strings.GetText("label_Gloves"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterGloves == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemEquipmentGloves");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterGloves = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemEquipmentGloves");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterGloves = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterHeadwear} " + Strings.GetText("label_Headwear"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterHeadwear == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemEquipmentHead");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterHeadwear = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemEquipmentHead");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterHeadwear = Storage.isTrueString;
                            }
                        }
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        if (GL.Button($"{settings.toggleFilterKeys} " + Strings.GetText("label_Keys"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterKeys == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemKey");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterKeys = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemKey");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterKeys = Storage.isTrueString;
                            }
                        }

                        if (GL.Button(
                            $"{settings.toggleFilterMiscellaneousItems} " + Strings.GetText("label_MiscellaneousItems"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterMiscellaneousItems == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItem");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterMiscellaneousItems = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItem");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterMiscellaneousItems = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterNeckItems} " + Strings.GetText("label_NeckItems"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterNeckItems == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemEquipmentNeck");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterNeckItems = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemEquipmentNeck");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterNeckItems = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterNotes} " + Strings.GetText("label_Notes"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterNotes == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemNote");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterNotes = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemNote");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterNotes = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterRings} " + Strings.GetText("label_Rings"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterRings == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemEquipmentRing");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterRings = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemEquipmentRing");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterRings = Storage.isTrueString;
                            }
                        }
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        if (GL.Button($"{settings.toggleFilterShields} " + Strings.GetText("label_Shields"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterShields == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemShield");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterShields = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemShield");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterShields = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterShoulderItems} " + Strings.GetText("label_ShoulderItems"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterShoulderItems == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemEquipmentShoulders");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterShoulderItems = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemEquipmentShoulders");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterShoulderItems = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterUsableItems} " + Strings.GetText("label_UsableItems"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterUsableItems == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemEquipmentUsable");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterUsableItems = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemEquipmentUsable");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterUsableItems = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterWeapons} " + Strings.GetText("label_Weapons"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterWeapons == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemWeapon");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterWeapons = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemWeapon");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterWeapons = Storage.isTrueString;
                            }
                        }

                        if (GL.Button($"{settings.toggleFilterWristItems} " + Strings.GetText("label_WristItems"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleFilterWristItems == Storage.isTrueString)
                            {
                                Storage.validItemTypesFiltered.Remove("BlueprintItemEquipmentWrist");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterWristItems = Storage.isFalseString;
                            }
                            else
                            {
                                Storage.validItemTypesFiltered.Add("BlueprintItemEquipmentWrist");
                                Main.SearchValidItems(Storage.validItemTypesFiltered);
                                settings.toggleFilterWristItems = Storage.isTrueString;
                            }
                        }
                        GL.EndHorizontal();
                    }

                    GL.EndVertical();

                    if (!Storage.resultItemNames.Any())
                    {
                        MenuTools.SingleLineLabel(Strings.GetText("message_NoResult"));
                    }
                    else
                    {
                        if (Storage.resultItemNames.Count > settings.finalResultLimit)
                        {
                            MenuTools.SingleLineLabel($"{Storage.resultItemNames.Count} " +
                                                      Strings.GetText("label_Results"));
                            MenuTools.SingleLineLabel(Strings.GetText("label_TooManyResults_0") +
                                                      $" {settings.finalResultLimit} " +
                                                      Strings.GetText("label_TooManyResults_1"));
                            GL.Space(10);
                        }
                        else
                        {
                            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_Results") + ":"));
                            for (var i = 0; i < Storage.resultItemNames.Count; i++)
                                if (Storage.resultItemGuids[i] == Main.ExcludeGuid)
                                {
                                    GL.BeginHorizontal();
                                    GL.Label(Storage.resultItemNames[i]);
                                    GL.Label(Strings.GetText("warning_EkunQ2"));
                                    GL.EndHorizontal();
                                    modLogger.Log(Storage.notificationEkunQ2rewardArmor);
                                }
                                else
                                {
                                    GL.BeginHorizontal();
                                    Storage.toggleItemSearchDescription.Add(false);

                                    if (string.IsNullOrEmpty(Storage.resultItemNames[i]))
                                    {
                                        var itemByGuid = Utilities.GetBlueprintByGuid<BlueprintItem>(Storage.resultItemGuids[i]);
                                        Storage.toggleItemSearchDescription[i] = GL.Toggle(Storage.toggleItemSearchDescription[i], Strings.CleanName(itemByGuid.name), GL.ExpandWidth(false));
                                    }
                                    else
                                    {
                                        Storage.toggleItemSearchDescription[i] = GL.Toggle(Storage.toggleItemSearchDescription[i], Storage.resultItemNames[i], GL.ExpandWidth(false));
                                    }

                                    GL.FlexibleSpace();
                                    if (Storage.itemFavourites.Contains(Storage.resultItemGuids[i]))
                                    {
                                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                        {
                                            Storage.itemFavourites.Remove(Storage.resultItemGuids[i]);
                                            Storage.itemFavouritesLoad = true;
                                        }
                                    }
                                    else if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                                    {
                                        Storage.itemFavourites.Add(Storage.resultItemGuids[i]);
                                        Storage.itemFavouritesLoad = true;
                                    }
                                    GL.EndHorizontal();
                                    ItemDetails(Storage.toggleItemSearchDescription[i], Storage.resultItemGuids[i]);
                                }


                            GL.BeginHorizontal();
                            if (GL.Button(Strings.GetText("button_AddCurrentSearchResults"), GL.ExpandWidth(false)))
                                Main.ModEntry.OnModActions.Push(m =>
                                {
                                    for (var i = 0; i < Storage.resultItemGuids.Count; i++)
                                        if (Utilities.GetBlueprintByGuid<BlueprintItem>(Storage.resultItemGuids[i]) !=
                                            null && Storage.resultItemGuids[i] != Main.ExcludeGuid)
                                            MenuTools.AddSingleItemAmount(Storage.resultItemGuids[i], 1, settings.addItemIdentified);
                                });
                            GL.EndHorizontal();

                            var filename = "item-" + Regex.Replace(Storage.currentItemSearch, @"[\\/:*?""<>|]", "");
                            MenuTools.ExportCopyGuidsNamesButtons(Storage.resultItemGuids.ToArray(), Storage.resultItemNames.ToArray(), filename);
                        }
                    }

                    GL.EndVertical();
                }
                else
                {
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("message_ExitCombat"), GL.ExpandWidth(false));
                    GL.EndHorizontal();
                }

                GL.Space(10);
            }
            GL.EndVertical();
        }

        public static void AbilityDetails(bool toggle, string abiltiyGuid, string description, UnitEntityData unitEntityData)
        {
            if (toggle)
            {
                GL.BeginVertical("box");
                BlueprintAbility abilityByGuid = Utilities.GetBlueprintByGuid<BlueprintAbility>(abiltiyGuid);
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_AbilityName") + ": ") + $"{abilityByGuid.Name}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_AbilityGuid") + ": ") + $"{abilityByGuid.AssetGuid}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectName") + ": ") + $"{abilityByGuid.name}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectNameClean") + ": ") + $"{Strings.CleanName(abilityByGuid.name)}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_AbilityRange") + ": ") + $"{abilityByGuid.Range}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_AbilityType") + ": ") + $"{abilityByGuid.Type}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_AbilityEffectOnAlly") + ": ") + $"{abilityByGuid.EffectOnAlly}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_AbilityEffectOnEnemy") + ": ") + $"{abilityByGuid.EffectOnEnemy}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_AbilityResourceLogic") + ": ") + $"{abilityByGuid.GetComponents<AbilityResourceLogic>()?.FirstOrDefault<AbilityResourceLogic>()}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_AbilityRequiredResource") + ": ") + $"{abilityByGuid.GetComponents<AbilityResourceLogic>()?.FirstOrDefault<AbilityResourceLogic>()?.RequiredResource}");

                if (abilityByGuid.GetComponents<AbilityResourceLogic>()?.FirstOrDefault<AbilityResourceLogic>()?.RequiredResource != null)
                {
                    if (!unitEntityData.Descriptor.Resources.ContainsResource(abilityByGuid.GetComponents<AbilityResourceLogic>()?.FirstOrDefault<AbilityResourceLogic>()?.RequiredResource))
                    {
                        if (GL.Button(Strings.GetText("misc_Add") + $" {abilityByGuid.GetComponents<AbilityResourceLogic>()?.FirstOrDefault<AbilityResourceLogic>()?.RequiredResource}", GL.ExpandWidth(false)))
                        {
                            unitEntityData.Descriptor.Resources.Add(abilityByGuid.GetComponents<AbilityResourceLogic>()?.FirstOrDefault<AbilityResourceLogic>()?.RequiredResource, true);
                        }
                    }
                }

                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_AbilityDescription") + ": ") + $"{description}");
                MenuTools.CopyExportButtons("button_ExportAbilityInfo", abilityByGuid.name + ".txt", AbilityInfo(abilityByGuid, description), "label_CopyAbilityInformationToClipboard");
                GL.EndVertical();
            }
        }

        public static string[] AbilityInfo(BlueprintAbility ability, string description)
        {
            return new string[]
            {
                    Strings.GetText("label_AbilityName") + ": " +$"{ability.Name}",
                    Strings.GetText("label_AbilityGuid") + ": " +$"{ability.AssetGuid}",
                    Strings.GetText("label_ObjectName") + ": " +$"{ability.name}",
                    Strings.GetText("label_ObjectNameClean") + ": " +$"{Strings.CleanName(ability.name)}",
                    Strings.GetText("label_AbilityRange") + ": " +$"{ability.Range}",
                    Strings.GetText("label_AbilityType") + ": " +$"{ability.Type}",
                    Strings.GetText("label_AbilityEffectOnAlly") + ": " +$"{ability.EffectOnAlly}",
                    Strings.GetText("label_AbilityEffectOnEnemy") + ": " +$"{ability.EffectOnEnemy}",
                    Strings.GetText("label_AbilityResourceLogic") + ": " +$"{ability.GetComponents<AbilityResourceLogic>()?.FirstOrDefault<AbilityResourceLogic>()}",
                    Strings.GetText("label_AbilityRequiredResource") + ": " +$"{ability.GetComponents<AbilityResourceLogic>()?.FirstOrDefault<AbilityResourceLogic>()?.RequiredResource}",
                    Strings.GetText("label_AbilityDescription") + $": {description}",
            };
        }

        public static void BuffDetails(bool toggle, string featGuid, string description)
        {
            if (toggle)
            {
                GL.BeginVertical("box");
                BlueprintBuff buffByGuid = Utilities.GetBlueprintByGuid<BlueprintBuff>(featGuid);
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_BuffName") + ": ") + $"{buffByGuid.Name}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(Strings.GetText("label_BuffGuid") + $": {buffByGuid.AssetGuid}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectName") + ": ") + $"{buffByGuid.name}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectNameClean") + ": ") + $"{Strings.CleanName(buffByGuid.name)}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_BuffTickTime") + ": ") + $"{buffByGuid.TickTime}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(Strings.GetText("label_BuffDescription") + $": {description}");
                MenuTools.CopyExportButtons("button_ExportBuffInfo", buffByGuid.name + ".txt", BuffInfo(buffByGuid, description), "label_CopyBuffInformationToClipboard");
                GL.EndVertical();
            }
        }
        public static string[] BuffInfo(BlueprintBuff buff, string description)
        {
            return new string[]
            {
                    Strings.GetText("label_BuffName") + ": " + $"{buff.Name}",
                    Strings.GetText("label_BuffGuid") + $": {buff.AssetGuid}",
                    Strings.GetText("label_ObjectName") + ": " + $"{buff.name}",
                    Strings.GetText("label_ObjectNameClean") + ": " + $"{Strings.CleanName(buff.name)}",
                    Strings.GetText("label_BuffTickTime") + ": " + $"{buff.TickTime}",
                    Strings.GetText("label_BuffDescription") + $": {description}",
            };
        }

        public static void FeatDetails(bool toggle, string featGuid, string description)
        {
            if (toggle)
            {
                GL.BeginVertical("box");

                BlueprintFeature featByGuid = Utilities.GetBlueprintByGuid<BlueprintFeature>(featGuid);
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_FeatName") + ": ") + $"{featByGuid.Name}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_FeatGUID") + ": ") + $"{featByGuid.AssetGuid}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectName") + ": ") + $"{featByGuid.name}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectNameClean") + ": ") + $"{Strings.CleanName(featByGuid.name)}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_FeatDescription") + ": ") + $"{description}");
                MenuTools.CopyExportButtons("button_ExportFeatInfo", featByGuid.name + ".txt", FeatInfo(featByGuid, description), "label_CopyFeatInformationToClipboard");
                GL.EndVertical();
            }
        }

        public static string[] FeatInfo(BlueprintFeature feat, string description)
        {
            return new string[]
            {
                    Strings.GetText("label_FeatName") + ": " + $"{feat.Name}",
                    Strings.GetText("label_FeatGUID") + ": " + $"{feat.AssetGuid}",
                    Strings.GetText("label_ObjectName") + ": " + $"{feat.name}",
                    Strings.GetText("label_ObjectNameClean") + ": " + $"{Strings.CleanName(feat.name)}",
                    Strings.GetText("label_FeatDescription") + ": " + $"{description}",
            };
        }

        public static TextFieldInt itemSearchDescriptionTextFieldInt = new TextFieldInt();
        public static TextFieldFloat itemSearchDescriptionTextFieldFloat = new TextFieldFloat();
        public static SelectionGrid diceTypesGrid = new SelectionGrid(Storage.diceTypes, 4);

        public static void ItemDetails(bool toggle, string itemGuid)
        {
            if (toggle == true)
            {
                GL.BeginVertical("box");
                BlueprintItem itemByGuid = Utilities.GetBlueprintByGuid<BlueprintItem>(itemGuid);
                if (Strings.ToBool(settings.toggleItemModding))
                {
                    if (File.Exists(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + itemGuid + ".json"))
                    {
                        GL.BeginHorizontal();
                        if (GL.Button(MenuTools.TextWithTooltip("button_RemoveItemModification", "misc_RequiresRestart", true), GL.ExpandWidth(false)))
                        {
                            try
                            {
                                File.Delete(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + itemGuid + ".json");
                                ModifiedBlueprintTools.blueprintLists = false;
                            }
                            catch (Exception e)
                            {
                                modLogger.Log(e.ToString());
                            }
                        }
                        GL.EndHorizontal();
                    }
                }
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ItemName") + ": ") + $"{itemByGuid.Name}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ItemGuid") + ": ") + $"{itemByGuid.AssetGuid}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ItemType") + ": ") + $"{itemByGuid.ItemType}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ItemDescription") + ": ") + $"{itemByGuid.Description}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ItemFlavourText") + ": ") + $"{itemByGuid.FlavorText}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ItemSellPrice") + ": ") + $"{itemByGuid.SellPrice}");

                if (Strings.ToBool(settings.toggleItemModding))
                {
                    GL.BeginVertical("box");
                }
                GL.BeginHorizontal();
                GL.Label(RichText.Bold(Strings.GetText("label_ItemCost") + ": ") + $"{itemByGuid.Cost}");
                if (Strings.ToBool(settings.toggleItemModding))
                {
                    GL.Space(10);
                    itemSearchDescriptionTextFieldInt.RenderFieldNoGroup();
                    ModifiedBlueprintTools.SetModifiedValueButton<ModifiedItem>(itemSearchDescriptionTextFieldInt.finalAmount, "m_Cost", itemGuid);
                }
                GL.EndHorizontal();
                if (Strings.ToBool(settings.toggleItemModding))
                {
                    GL.EndVertical();
                }

                if (Strings.ToBool(settings.toggleItemModding))
                {
                    GL.BeginVertical("box");
                }
                GL.BeginHorizontal();
                GL.Label(RichText.Bold(Strings.GetText("label_ItemWeight") + ": ") + $"{itemByGuid.Weight}");
                if (Strings.ToBool(settings.toggleItemModding))
                {
                    GL.Space(10);
                    itemSearchDescriptionTextFieldFloat.RenderFieldNoGroup();
                    ModifiedBlueprintTools.SetModifiedValueButton<ModifiedItem>(itemSearchDescriptionTextFieldFloat.finalAmount, "m_Weight", itemGuid);
                }
                GL.EndHorizontal();
                if (Strings.ToBool(settings.toggleItemModding))
                {
                    GL.EndVertical();
                }

                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectName") + ": ") + $"{itemByGuid.name}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ObjectNameClean") + ": ") + $"{Strings.CleanName(itemByGuid.name)}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ItemSubtypeName") + ": ") + $"{itemByGuid.SubtypeName}");
                MenuTools.SingleLineLabelCopyBlueprintDetail(RichText.Bold(Strings.GetText("label_ItemSubtypeDescription") + ": ") + $"{itemByGuid.SubtypeDescription}");
                if (itemByGuid.GetType() == typeof(BlueprintItemWeapon) && Strings.ToBool(settings.toggleItemModding))
                {
                    GL.BeginVertical("box");
                    GL.BeginHorizontal();
                    GL.Label("m_OverrideDamageDice: " + Traverse.Create(itemByGuid).Field("m_OverrideDamageDice").GetValue().ToString());
                    ModifiedBlueprintTools.SetModifiedValueButtonBool<ModifiedWeapon>("m_OverrideDamageDice", itemByGuid.AssetGuid);
                    GL.EndHorizontal();
                    GL.EndVertical();

                    GL.BeginVertical("box");
                    GL.BeginHorizontal();
                    GL.Label("m_DamageDice: " + Traverse.Create(itemByGuid).Field("m_DamageDice").GetValue().ToString());
                    GL.EndHorizontal();
                    GL.BeginHorizontal();
                    itemSearchDescriptionTextFieldInt.RenderFieldNoGroup("misc_NumberOfRolls");
                    GL.EndHorizontal();
                    diceTypesGrid.Render();
                    GL.BeginHorizontal();
                    GL.FlexibleSpace();
                    ModifiedBlueprintTools.SetModifiedValueButtonDiceFormula<ModifiedWeapon>(itemSearchDescriptionTextFieldInt.finalAmount, Common.IntToDiceType(diceTypesGrid.selected), "m_DamageDice", itemByGuid.AssetGuid);
                    GL.EndHorizontal();
                    GL.EndVertical();
                }

                MenuTools.CopyExportButtons("button_ExportItemInfo", itemByGuid.name + ".txt", ItemInfo(itemByGuid), "label_CopyItemInformationToClipboard");
                GL.EndVertical();
            }
        }

        public static string[] ItemInfo(BlueprintItem item)
        {
            return new string[]
            {
                Strings.GetText("label_ItemGuid") + $": {item.AssetGuid}",
                Strings.GetText("label_ItemName") + $": {item.Name}",
                Strings.GetText("label_ItemType") + $": {item.ItemType}",
                Strings.GetText("label_ItemDescription") + $": {item.Description}",
                Strings.GetText("label_ItemFlavourText") + $": {item.FlavorText}",
                Strings.GetText("label_ItemCost") + $": {item.Cost}",
                Strings.GetText("label_ItemSellPrice") + $": {item.SellPrice}",
                Strings.GetText("label_ItemWeight") + $": {item.Weight}",
                Strings.GetText("label_ObjectName") + $": {item.name}",
                Strings.GetText("label_ObjectNameClean") + $": {Strings.CleanName(item.name)}",
                Strings.GetText("label_ItemComment") + $": {item.Comment}",
                Strings.GetText("label_ItemIcon") + $": {item.Icon}",
                Strings.GetText("label_ItemIdentifyDC") + $": {item.IdentifyDC}",
                Strings.GetText("label_ItemInventoryPutSound") + $": {item.InventoryPutSound}",
                Strings.GetText("label_ItemInventoryTakeSound") + $": {item.InventoryTakeSound}",
                Strings.GetText("label_ItemIsStackable") + $": {item.IsActuallyStackable}",
                Strings.GetText("label_ItemIsNotable") + $": {item.IsNotable}",
                Strings.GetText("label_ItemMiscellaneousType") + $": {item.MiscellaneousType}",
                Strings.GetText("label_ItemNonIdentifiedDescription") + $": {item.NonIdentifiedDescription}",
                Strings.GetText("label_ItemNonIdentifiedName") + $": {item.NonIdentifiedName}",
                Strings.GetText("label_ItemSubtypeDescription") + $": {item.SubtypeDescription}",
                Strings.GetText("label_ItemSubtypeName") + $": {item.SubtypeName}"
            };
        }

        public static SelectionGrid inventoryItemTypesGrid = new SelectionGrid(Storage.inventoryItemTypesArray, 3);

        public static void ShowInventory()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            Storage.showInventory = GL.Toggle(Storage.showInventory, RichText.Bold(Strings.GetText("toggle_ShowInventory")), GL.ExpandWidth(false));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(ShowInventory));
            GL.EndHorizontal();
            if (Storage.showInventory)
            {
                inventoryItemTypesGrid.Render();
                GL.Space(10);
                switch (inventoryItemTypesGrid.selected)
                {
                    case 0:
                        Game.Instance.Player.Inventory.Items.Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 1:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Armor).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 2:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Belt).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 3:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Feet).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 4:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Gloves).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 5:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Head).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 6:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Neck).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 7:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.NonUsable).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 8:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Other).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 9:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Ring).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 10:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Shield).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 11:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Shoulders).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 12:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Usable).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 13:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Weapon).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    case 14:
                        Game.Instance.Player.Inventory.Items.Where(e => e.Blueprint.ItemType == ItemsFilter.ItemType.Wrist).Take(Main.QueryLimit).OrderBy(i => i.Name).ForEach(e => RenderInventoryItem(e));
                        break;
                    default:
                        MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat("ERROR"));
                        break;
                }
            }

            GL.EndVertical();
        }

        public static void RenderInventoryItem(ItemEntity item)
        {
            GL.BeginVertical("box");

            GL.BeginHorizontal();
            GL.Label($"{item.Name} ({item.Count})", GL.ExpandWidth(true));
            GL.FlexibleSpace();

            try
            {
                if (item.Count > 1)
                    if (GL.Button(Strings.GetText("label_RemoveAll")))
                        Game.Instance.Player.Inventory.Remove(item, item.Count);
                if (GL.Button(Strings.GetText("label_Remove"))) Game.Instance.Player.Inventory.Remove(item, 1);
            }
            catch (Exception e)
            {
                modLogger.Log(e.ToString());
            }

            GL.EndHorizontal();

            GL.EndVertical();
        }

        public static void ArmourChecksPenalty0()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("buttonToggle_ArmourChecksPenalty0", "tooltip_ArmourChecksPenalty0", $"{ settings.toggleArmourChecksPenalty0} ", ""), GL.ExpandWidth(false)))
            {
                if (settings.toggleArmourChecksPenalty0 == Storage.isFalseString)
                {
                    settings.toggleArmourChecksPenalty0 = Storage.isTrueString;
                    foreach (UnitEntityData unitEntityData in Game.Instance.Player.Party)
                    {
                        Common.RecalculateArmourItemStats(unitEntityData);
                    }
                }
                else if (settings.toggleArmourChecksPenalty0 == Storage.isTrueString)
                {
                    settings.toggleArmourChecksPenalty0 = Storage.isFalseString;
                    foreach (UnitEntityData unitEntityData in Game.Instance.Player.Party)
                    {
                        Common.RecalculateArmourItemStats(unitEntityData);
                    }
                }
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(ArmourChecksPenalty0));
            GL.EndHorizontal();
            if (Strings.ToBool(settings.toggleArmourChecksPenalty0))
            {
                MenuTools.ToggleButton(ref settings.toggleArmourChecksPenalty0OutOfCombatOnly, "buttonToggle_OutOfCombatOnly", "tooltip_OutOfCombatOnly_ArmourChecksPenalty0");
            }
            GL.EndVertical();
        }



        public static void Buffs()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showBuffsCategory = GL.Toggle(settings.showBuffsCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_Buffs")), GL.ExpandWidth(false));
            if (!settings.showBuffsCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("Buffs");

                GL.Space(10);

                BuffDurationMultiplier();

                GL.Space(10);

                GL.BeginHorizontal();
                Storage.buffsFilterUnitEntityDataIndex = GL.SelectionGrid(Storage.buffsFilterUnitEntityDataIndex, Storage.unitEntityDataArray, 3);
                GL.EndHorizontal();

                var player = Game.Instance.Player;

                switch (Storage.buffsFilterUnitEntityDataIndex)
                {
                    case 0:
                        Storage.buffsUnitEntityData = player.Party;
                        break;
                    case 1:
                        Storage.buffsUnitEntityData = player.ControllableCharacters;
                        break;
                    case 2:
                        Storage.buffsUnitEntityData = player.ActiveCompanions;
                        break;
                    case 3:
                        Storage.buffsUnitEntityData = player.AllCharacters;
                        break;
                    case 4:
                        Storage.buffsUnitEntityData = Common.GetCustomCompanions();
                        break;
                    case 5:
                        Storage.buffsUnitEntityData = Common.GetPets();
                        break;
                    case 6:
                        Storage.buffsUnitEntityData = Common.GetEnemies();
                        break;
                }

                if (Storage.buffsFilterUnitEntityDataIndex != Storage.buffsFilterUnitEntityDataIndexOld)
                {
                    Storage.reloadPartyBuffs = true;
                    Storage.buffsFilterUnitEntityDataIndexOld = Storage.buffsFilterUnitEntityDataIndex;
                }

                if (Storage.buffsUnitEntityData.Count != Storage.buffAllUnits.Count || Storage.reloadPartyBuffs)
                {
                    Storage.buffSelectedControllableCharacterIndex = 0;
                    Storage.buffAllUnitsNamesList.Clear();
                    foreach (var controllableCharacter in Storage.buffsUnitEntityData)
                        Storage.buffAllUnitsNamesList.Add(controllableCharacter.CharacterName);
                    Storage.buffAllUnits = Storage.buffsUnitEntityData;
                    Storage.reloadPartyBuffs = false;
                }

                if (Storage.buffsUnitEntityData.Count - 1 < Storage.buffSelectedControllableCharacterIndex)
                    Storage.buffSelectedControllableCharacterIndex = Storage.buffsUnitEntityData.Count - 1;

                GL.Space(10);

                if (Storage.buffsUnitEntityData.Any())
                {
                    if (!Storage.reloadPartyBuffs)
                    {
                        GL.BeginHorizontal();
                        Storage.buffSelectedControllableCharacterIndex = GL.SelectionGrid(Storage.buffSelectedControllableCharacterIndex, Storage.buffAllUnitsNamesList.ToArray(), 3);
                        GL.EndHorizontal();

                        MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_Buffs")));
                        MenuTools.SingleLineLabel(Strings.GetText("label_NumberOfActiveBuffs") +
                                                  $" ({Storage.buffAllUnitsNamesList[Storage.buffSelectedControllableCharacterIndex]}): {Storage.buffAllUnits[Storage.buffSelectedControllableCharacterIndex].Buffs.Count.ToString()}");
                        if (GL.Button(Strings.GetText("button_RemoveAllBuffs") + $" ({Storage.buffAllUnitsNamesList[Storage.buffSelectedControllableCharacterIndex]})", GL.ExpandWidth(false)))
                        {
                            var unitEntityData = Storage.buffAllUnits[Storage.buffSelectedControllableCharacterIndex];
                            for (var i = unitEntityData.Buffs.Count; i > 0; i--)
                                unitEntityData.Descriptor.RemoveFact(
                                    (BlueprintUnitFact)unitEntityData.Buffs[i - 1].Blueprint);
                        }

                        if (GL.Button($"{settings.showAllBuffs}" + Strings.GetText("button_AllBuffs") + $" ({Storage.buffAllUnitsNamesList[Storage.buffSelectedControllableCharacterIndex]})", GL.ExpandWidth(false)))
                        {
                            if (settings.showAllBuffs == Strings.GetText("misc_Hide"))
                            {
                                settings.showAllBuffs = Strings.GetText("misc_Display");
                            }
                            else if (settings.showAllBuffs == Strings.GetText("misc_Display"))
                            {
                                settings.showAllBuffs = Strings.GetText("misc_Hide");
                                Storage.buffAllLoad = true;
                            }
                            else
                            {
                                settings.showAllBuffs = Strings.GetText("misc_Display");
                            }
                        }

                        if (settings.showAllBuffs == Strings.GetText("misc_Hide"))
                        {
                            var unitEntityData = Storage.buffAllUnits[Storage.buffSelectedControllableCharacterIndex];
                            if (unitEntityData != Storage.buffAllUnitEntityData)
                            {
                                Storage.buffAllUnitEntityData = unitEntityData;
                                Storage.buffAllLoad = true;
                            }

                            if (Storage.buffAllLoad == true)
                            {
                                Main.RefreshAllBuffs(unitEntityData);
                                Storage.buffAllLoad = false;
                            }

                            for (var i = 0; i < unitEntityData.Buffs.Count; i++)
                            {
                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label(Strings.GetText("label_BuffName") + $": {Storage.buffAllNames[i]}");
                                GL.FlexibleSpace();
                                if (Storage.buffFavourites.Contains(unitEntityData.Buffs[i].Blueprint.AssetGuid))
                                {
                                    if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                    {
                                        Storage.buffFavourites.Remove(unitEntityData.Buffs[i].Blueprint.AssetGuid);
                                        Storage.buffFavouritesLoad = true;
                                    }
                                }
                                else
                                {
                                    if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                                    {
                                        Storage.buffFavourites.Add(unitEntityData.Buffs[i].Blueprint.AssetGuid);
                                        Storage.buffFavouritesLoad = true;
                                    }
                                }

                                GL.EndHorizontal();
                                MenuTools.SingleLineLabel(Strings.GetText("label_BuffObjectName") + $": {Storage.buffAllObjectNames[i]}");
                                MenuTools.SingleLineLabel(Strings.GetText("label_BuffGuid") + $": {Storage.buffAllGuids[i]}");
                                MenuTools.SingleLineLabel(Strings.GetText("label_BuffDescription") + $": {Storage.buffAllDescriptions[i]}");
                                if (GL.Button(
                                    Strings.GetText("label_Remove") + $" {unitEntityData.Buffs[i].Blueprint.name}",
                                    GL.ExpandWidth(false)))
                                {
                                    unitEntityData.Descriptor.RemoveFact(
                                        (BlueprintUnitFact)unitEntityData.Buffs[i].Blueprint);
                                    Storage.buffAllLoad = true;
                                }

                                GL.EndVertical();
                            }
                        }

                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        settings.showBuffFavourites = GL.Toggle(settings.showBuffFavourites, RichText.Bold(Strings.GetText("headerOption_ShowFavourites")), GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        if (settings.showBuffFavourites == true)
                        {
                            if (Storage.buffFavouritesLoad == true)
                            {
                                Main.RefreshBuffFavourites();
                                Storage.buffFavouritesLoad = false;
                            }

                            if (!Storage.buffFavourites.Any())
                            {
                                MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                            }
                            else
                            {
                                for (var i = 0; i < Storage.buffFavourites.Count; i++)
                                {
                                    GL.BeginHorizontal();
                                    Storage.buffToggleFavouriteDescription.Add(false);
                                    if (!string.IsNullOrEmpty(Storage.buffFavouritesNames[i]))
                                        Storage.buffToggleFavouriteDescription[i] = GL.Toggle(Storage.buffToggleFavouriteDescription[i], Storage.buffFavouritesNames[i], GL.ExpandWidth(false));
                                    GL.FlexibleSpace();

                                    try
                                    {
                                        if (GL.Button(Strings.GetText("misc_Add") + $" { Storage.buffFavouritesNames[i]} " + Strings.GetText("misc_To") + $" {Storage.buffAllUnitsNamesList[Storage.buffSelectedControllableCharacterIndex]}", GL.Width(400f)))
                                        {
                                            var unitEntityData = Storage.buffAllUnits[Storage.buffSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(Utilities.GetBlueprintByGuid<BlueprintBuff>(Storage.buffFavouritesGuids[i]), null, new FeatureParam());
                                            Storage.buffAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllBuffs(Storage.buffAllUnitEntityData);
                                    }

                                    try
                                    {
                                        if (GL.Button(Strings.GetText("button_AddToParty"), GL.ExpandWidth(false)))
                                            foreach (var controllableCharacter in Storage.buffAllUnits)
                                            {
                                                controllableCharacter.Descriptor.AddFact(
                                                    (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                                        Storage.buffFavouritesGuids[i]), (MechanicsContext)null,
                                                    new FeatureParam());
                                                Storage.buffAllLoad = true;
                                            }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllBuffs(Storage.buffAllUnitEntityData);
                                    }
                                    if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                    {
                                        Storage.buffFavouritesGuids.Remove(Storage.buffFavouritesGuids[i]);
                                        Storage.buffFavouritesLoad = true;
                                    }
                                    GL.EndHorizontal();
                                    BuffDetails(Storage.buffToggleFavouriteDescription[i], Storage.buffFavouritesGuids[i], Storage.buffFavouritesDescriptions[i]);
                                }

                                GL.Space(10);
                                GL.BeginHorizontal();
                                if (GL.Button(
                                    Strings.GetText("button_AddFavouritesTo") +
                                    $" {Storage.buffAllUnitsNamesList[Storage.buffSelectedControllableCharacterIndex]}",
                                    GL.ExpandWidth(false)))
                                    for (var i = 0; i < Storage.buffFavouritesGuids.Count; i++)
                                        if (Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                                Storage.buffFavouritesGuids[i]) != null)
                                        {
                                            var buffByGuid =
                                                Utilities.GetBlueprintByGuid<BlueprintBuff>(
                                                    Storage.buffFavouritesGuids[i]);
                                            var unitEntityData =
                                                Storage.buffAllUnits[Storage.buffSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)buffByGuid,
                                                (MechanicsContext)null, new FeatureParam());
                                            Storage.buffAllLoad = true;
                                        }
                                GL.EndHorizontal();

                                MenuTools.ExportCopyGuidsNamesButtons(Storage.buffFavouritesGuids.ToArray(), Storage.buffFavouritesNames.ToArray(), "buff-favourites");
                            }

                            if (Storage.buffFavourites != Storage.buffFavouritesGuids)
                                Storage.buffFavourites = Storage.buffFavouritesGuids;
                        }

                        GL.EndVertical();

                        GL.Space(10);

                        GL.BeginVertical("box");

                        GL.BeginHorizontal();
                        settings.buffSearch = GL.TextField(settings.buffSearch, GL.Width(500f));
                        if (GL.Button(RichText.Bold(Strings.GetText("header_Search")), GL.ExpandWidth(false)))
                            Main.SearchValidBuffs(Storage.validBuffTypes);
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_SearchBy") + ": ", GL.ExpandWidth(false));

                        if (GL.Button($"{settings.toggleSearchByBuffObjectName} " + Strings.GetText("label_ObjectName"), GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByBuffObjectName == Storage.isTrueString)
                                settings.toggleSearchByBuffObjectName = Storage.isFalseString;
                            else
                                settings.toggleSearchByBuffObjectName = Storage.isTrueString;
                            Main.SearchValidBuffs(Storage.validBuffTypes);
                        }

                        if (GL.Button($"{settings.toggleSearchByBuffName} " + Strings.GetText("label_BuffName"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByBuffName == Storage.isTrueString)
                                settings.toggleSearchByBuffName = Storage.isFalseString;
                            else
                                settings.toggleSearchByBuffName = Storage.isTrueString;
                            Main.SearchValidBuffs(Storage.validBuffTypes);
                        }

                        if (GL.Button(
                            $"{settings.toggleSearchByBuffDescription} " + Strings.GetText("label_BuffDescription"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByBuffDescription == Storage.isTrueString)
                                settings.toggleSearchByBuffDescription = Storage.isFalseString;
                            else
                                settings.toggleSearchByBuffDescription = Storage.isTrueString;
                            Main.SearchValidBuffs(Storage.validBuffTypes);
                        }

                        if (GL.Button(
                            $"{settings.toggleSearchByBuffComponents} " + Strings.GetText("label_BuffComponents"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByBuffComponents == Storage.isTrueString)
                                settings.toggleSearchByBuffComponents = Storage.isFalseString;
                            else
                                settings.toggleSearchByBuffComponents = Storage.isTrueString;
                            Main.SearchValidBuffs(Storage.validBuffTypes);
                        }
                        GL.EndHorizontal();

                        if (!Storage.buffResultNames.Any())
                        {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoResult"));
                        }
                        else
                        {
                            if (Storage.buffResultNames.Count > settings.finalResultLimit)
                            {
                                MenuTools.SingleLineLabel($"{Storage.buffResultNames.Count} " +
                                                          Strings.GetText("label_Results"));
                                MenuTools.SingleLineLabel(Strings.GetText("label_TooManyResults_0") +
                                                          $" {settings.finalResultLimit} " +
                                                          Strings.GetText("label_TooManyResults_1"));
                                GL.Space(10);
                            }
                            else
                            {
                                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_Results") + ":"));
                                for (var i = 0; i < Storage.buffResultNames.Count; i++)
                                {
                                    GL.BeginHorizontal();
                                    Storage.buffToggleResultDescription.Add(false);
                                    if (!string.IsNullOrEmpty(Storage.buffResultNames[i]))
                                        Storage.buffToggleResultDescription[i] = GL.Toggle(Storage.buffToggleResultDescription[i], Storage.buffResultNames[i], GL.ExpandWidth(false));
                                    GL.FlexibleSpace();

                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + $" {Storage.buffResultNames[i]} " +
                                            Strings.GetText("misc_To") +
                                            $" {Storage.buffAllUnitsNamesList[Storage.buffSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var unitEntityData = Storage.buffAllUnits[Storage.buffSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(Utilities.GetBlueprintByGuid<BlueprintBuff>(Storage.buffResultGuids[i]), null, new FeatureParam());
                                            Storage.buffAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllBuffs(Storage.buffAllUnitEntityData);
                                    }

                                    try
                                    {
                                        if (GL.Button(Strings.GetText("button_AddToParty"), GL.ExpandWidth(false)))
                                            foreach (var controllableCharacter in Storage.buffAllUnits)
                                            {
                                                controllableCharacter.Descriptor.AddFact(Utilities.GetBlueprintByGuid<BlueprintBuff>(Storage.buffResultGuids[i]), null, new FeatureParam());
                                                Storage.buffAllLoad = true;
                                            }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllBuffs(Storage.buffAllUnitEntityData);
                                    }

                                    if (Storage.buffFavourites.Contains(Storage.buffResultGuids[i]))
                                    {
                                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                        {
                                            Storage.buffFavourites.Remove(Storage.buffResultGuids[i]);
                                            Storage.buffFavouritesLoad = true;
                                        }
                                    }
                                    else
                                    {
                                        if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                                        {
                                            Storage.buffFavourites.Add(Storage.buffResultGuids[i]);
                                            Storage.buffFavouritesLoad = true;
                                        }
                                    }
                                    GL.EndHorizontal();
                                    BuffDetails(Storage.buffToggleResultDescription[i], Storage.buffResultGuids[i], Storage.buffResultDescriptions[i]);
                                }

                                var filename = "buff-" + Regex.Replace(Storage.currentBuffSearch, @"[\\/:*?""<>|]", "");
                                MenuTools.ExportCopyGuidsNamesButtons(Storage.buffResultGuids.ToArray(), Storage.buffResultNames.ToArray(), filename);
                            }
                        }

                        GL.EndVertical();
                    }
                }
                else
                {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitFound"));
                }
            }

            GL.EndVertical();
        }

        public static void Abilities()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showAddAbilitiesCategory = GL.Toggle(settings.showAddAbilitiesCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_AddAbilities")), GL.ExpandWidth(false));
            if (!settings.showAddAbilitiesCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("Abilities");

                GL.Space(10);

                GL.BeginHorizontal();
                Storage.addAbilitiesFilterUnitEntityDataIndex = GL.SelectionGrid(Storage.addAbilitiesFilterUnitEntityDataIndex, Storage.unitEntityDataArray, 3);
                GL.EndHorizontal();

                var player = Game.Instance.Player;

                switch (Storage.addAbilitiesFilterUnitEntityDataIndex)
                {
                    case 0:
                        Storage.addAbilitiesUnitEntityData = player.Party;
                        break;
                    case 1:
                        Storage.addAbilitiesUnitEntityData = player.ControllableCharacters;
                        break;
                    case 2:
                        Storage.addAbilitiesUnitEntityData = player.ActiveCompanions;
                        break;
                    case 3:
                        Storage.addAbilitiesUnitEntityData = player.AllCharacters;
                        break;
                    case 4:
                        Storage.addAbilitiesUnitEntityData = Common.GetCustomCompanions();
                        break;
                    case 5:
                        Storage.addAbilitiesUnitEntityData = Common.GetPets();
                        break;
                    case 6:
                        Storage.addAbilitiesUnitEntityData = Common.GetEnemies();
                        break;
                }

                if (Storage.addAbilitiesFilterUnitEntityDataIndex != Storage.addAbilitiesFilterUnitEntityDataIndexOld)
                {
                    Storage.reloadPartyAddAbilities = true;
                    Storage.addAbilitiesFilterUnitEntityDataIndexOld = Storage.addAbilitiesFilterUnitEntityDataIndex;
                }

                if (Storage.addAbilitiesUnitEntityData.Count != Storage.addAbilitiesAllUnits.Count ||
                    Storage.reloadPartyAddAbilities)
                {
                    Storage.addAbilitiesSelectedControllableCharacterIndex = 0;
                    Storage.addAbilitiesAllUnitsNamesList.Clear();
                    foreach (var controllableCharacter in Storage.addAbilitiesUnitEntityData)
                        Storage.addAbilitiesAllUnitsNamesList.Add(controllableCharacter.CharacterName);
                    Storage.addAbilitiesAllUnits = Storage.addAbilitiesUnitEntityData;
                    Storage.reloadPartyAddAbilities = false;
                }

                if (Storage.addAbilitiesUnitEntityData.Count - 1 <
                    Storage.addAbilitiesSelectedControllableCharacterIndex)
                    Storage.addAbilitiesSelectedControllableCharacterIndex =
                        Storage.addAbilitiesUnitEntityData.Count - 1;

                if (Storage.addAbilitiesUnitEntityData.Any())
                {
                    if (!Storage.reloadPartyAddAbilities)
                    {
                        GL.Space(10);

                        GL.BeginHorizontal();
                        Storage.addAbilitiesSelectedControllableCharacterIndex = GL.SelectionGrid(Storage.addAbilitiesSelectedControllableCharacterIndex, Storage.addAbilitiesAllUnitsNamesList.ToArray(), 3);
                        GL.EndHorizontal();

                        GL.Space(10);

                        if (GL.Button(
                            $"{settings.showAllAbilities} " + Strings.GetText("button_AllAbilities") +
                            $" ({Storage.addAbilitiesAllUnitsNamesList[Storage.addAbilitiesSelectedControllableCharacterIndex]})",
                            GL.ExpandWidth(false)))
                        {
                            if (settings.showAllAbilities == Strings.GetText("misc_Hide"))
                            {
                                settings.showAllAbilities = Strings.GetText("misc_Display");
                            }
                            else if (settings.showAllAbilities == Strings.GetText("misc_Display"))
                            {
                                settings.showAllAbilities = Strings.GetText("misc_Hide");
                                Storage.addAbilitiesAllLoad = true;
                            }
                            else
                            {
                                settings.showAllAbilities = Strings.GetText("misc_Display");
                            }
                        }

                        if (settings.showAllAbilities == Strings.GetText("misc_Hide"))
                        {
                            var unitEntityData =
                                Storage.addAbilitiesAllUnits[Storage.addAbilitiesSelectedControllableCharacterIndex];
                            if (unitEntityData != Storage.addAbilitiesAllUnitEntityData)
                            {
                                Storage.addAbilitiesAllUnitEntityData = unitEntityData;
                                Storage.addAbilitiesAllLoad = true;
                            }

                            if (Storage.addAbilitiesAllLoad == true)
                            {
                                Main.RefreshAllAbilities(unitEntityData);
                                Storage.addAbilitiesAllLoad = false;
                            }

                            for (var i = 0; i < unitEntityData.Descriptor.Abilities.Count; i++)
                            {
                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label(Strings.GetText("label_AbilityName") + $": {Storage.addAbilitiesAllNames[i]}");
                                GL.FlexibleSpace();
                                if (Storage.abilitiesFavourites.Contains(unitEntityData.Descriptor.Abilities[i]
                                    .Blueprint.AssetGuid))
                                {
                                    if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                    {
                                        Storage.abilitiesFavourites.Remove(unitEntityData.Descriptor.Abilities[i]
                                            .Blueprint.AssetGuid);
                                        Storage.addAbilitiesFavouritesLoad = true;
                                    }
                                }
                                else
                                {
                                    if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                                    {
                                        Storage.abilitiesFavourites.Add(unitEntityData.Descriptor.Abilities[i].Blueprint
                                            .AssetGuid);
                                        Storage.addAbilitiesFavouritesLoad = true;
                                    }
                                }

                                GL.EndHorizontal();
                                MenuTools.SingleLineLabel(Strings.GetText("label_AbilityObjectName") +
                                                          $": {Storage.addAbilitiesAllObjectNames[i]}");
                                MenuTools.SingleLineLabel(Strings.GetText("label_AbilityBlueprintAssetGuid") +
                                                          $": {Storage.addAbilitiesAllGuids[i]}");
                                MenuTools.SingleLineLabel(Strings.GetText("label_AbilityDescription") +
                                                          $": {Storage.addAbilitiesAllDescriptions[i]}");

                                if (GL.Button(
                                    Strings.GetText("label_Remove") +
                                    $" {unitEntityData.Descriptor.Abilities[i].Blueprint.name}", GL.ExpandWidth(false)))
                                {
                                    unitEntityData.Descriptor.Abilities.RemoveFact(
                                        (BlueprintUnitFact)unitEntityData.Descriptor.Abilities[i].Blueprint);
                                    Storage.addAbilitiesAllLoad = true;
                                }

                                GL.EndVertical();
                            }
                        }

                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        settings.showAbilitiesFavourites = GL.Toggle(settings.showAbilitiesFavourites, RichText.Bold(Strings.GetText("headerOption_ShowFavourites")), GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        if (settings.showAbilitiesFavourites == true)
                        {
                            if (Storage.addAbilitiesFavouritesLoad == true)
                            {
                                Main.RefreshAbilityFavourites();
                                Storage.addAbilitiesFavouritesLoad = false;
                            }

                            if (!Storage.abilitiesFavourites.Any())
                            {
                                MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                            }
                            else
                            {
                                for (var i = 0; i < Storage.abilitiesFavourites.Count; i++)
                                {
                                    GL.BeginHorizontal();
                                    Storage.abilitiesToggleFavouriteDescription.Add(false);
                                    if (!string.IsNullOrEmpty(Storage.abilitiesFavouritesNames[i]))
                                        Storage.abilitiesToggleFavouriteDescription[i] = GL.Toggle(Storage.abilitiesToggleFavouriteDescription[i], Storage.abilitiesFavouritesNames[i], GL.ExpandWidth(false));
                                    GL.FlexibleSpace();

                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + $" {Storage.abilitiesFavouritesNames[i]} " +
                                            Strings.GetText("misc_To") +
                                            $" {Storage.addAbilitiesAllUnitsNamesList[Storage.addAbilitiesSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var unitEntityData =
                                                Storage.addAbilitiesAllUnits[
                                                    Storage.addAbilitiesSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(Utilities.GetBlueprintByGuid<BlueprintAbility>(Storage.abilitiesFavouritesGuids[i]), null, new FeatureParam());
                                            Storage.addAbilitiesAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllAbilities(Storage.addAbilitiesAllUnitEntityData);
                                    }

                                    if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                    {
                                        Storage.abilitiesFavouritesGuids.Remove(Storage.abilitiesFavouritesGuids[i]);
                                        Storage.addAbilitiesFavouritesLoad = true;
                                    }
                                    GL.EndHorizontal();

                                    AbilityDetails(Storage.abilitiesToggleFavouriteDescription[i], Storage.abilitiesFavouritesGuids[i], Storage.abilitiesFavouritesDescriptions[i], Storage.addAbilitiesAllUnits[Storage.addAbilitiesSelectedControllableCharacterIndex]);
                                }

                                GL.Space(10);

                                if (GL.Button(
                                    Strings.GetText("button_AddFavouritesTo") +
                                    $" {Storage.addAbilitiesAllUnitsNamesList[Storage.addAbilitiesSelectedControllableCharacterIndex]}",
                                    GL.ExpandWidth(false)))
                                    for (var i = 0; i < Storage.abilitiesFavouritesGuids.Count; i++)
                                        if (Utilities.GetBlueprintByGuid<BlueprintAbility>(
                                                Storage.abilitiesFavouritesGuids[i]) != null)
                                        {
                                            var unitEntityData =
                                                Storage.addAbilitiesAllUnits[
                                                    Storage.addAbilitiesSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintAbility>(
                                                    Storage.abilityResultGuids[i]), (MechanicsContext)null,
                                                new FeatureParam());
                                            Storage.addAbilitiesAllLoad = true;
                                        }

                                MenuTools.ExportCopyGuidsNamesButtons(Storage.abilitiesFavouritesGuids.ToArray(), Storage.abilitiesFavouritesNames.ToArray(), "ability-favourites");
                            }

                            if (Storage.abilitiesFavourites != Storage.abilitiesFavouritesGuids)
                                Storage.abilitiesFavourites = Storage.abilitiesFavouritesGuids;
                        }

                        GL.EndVertical();

                        GL.Space(10);

                        string campingSpecialAbility = Kingmaker.Controllers.Rest.Special.CampingSpecialAbility
                            .Get(Storage.addAbilitiesAllUnits[Storage.addAbilitiesSelectedControllableCharacterIndex]
                                .Blueprint)?.Name;

                        if (campingSpecialAbility == "<null>") campingSpecialAbility = "-";

                        MenuTools.SingleLineLabel(Strings.GetText("label_CampingSpecialAbility") + ": " +
                                                  campingSpecialAbility);

                        GL.Space(10);

                        GL.BeginVertical("box");

                        GL.BeginHorizontal();
                        settings.abilitySearch = GL.TextField(settings.abilitySearch, GL.Width(500f));
                        if (GL.Button(RichText.Bold(Strings.GetText("header_Search")), GL.ExpandWidth(false)))
                            Main.SearchValidAbilities(Storage.validAbilityTypes);
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_SearchBy") + ": ", GL.ExpandWidth(false));

                        if (GL.Button(
                            $"{settings.toggleSearchByAbilityObjectName} " + Strings.GetText("label_ObjectName"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByAbilityObjectName == Storage.isTrueString)
                                settings.toggleSearchByAbilityObjectName = Storage.isFalseString;
                            else
                                settings.toggleSearchByAbilityObjectName = Storage.isTrueString;
                            Main.SearchValidAbilities(Storage.validAbilityTypes);
                        }

                        if (GL.Button($"{settings.toggleSearchByAbilityName} " + Strings.GetText("label_AbilityName"),
                            GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByAbilityName == Storage.isTrueString)
                                settings.toggleSearchByAbilityName = Storage.isFalseString;
                            else
                                settings.toggleSearchByAbilityName = Storage.isTrueString;
                            Main.SearchValidAbilities(Storage.validAbilityTypes);
                        }

                        if (GL.Button(
                            $"{settings.toggleSearchByAbilityDescription} " +
                            Strings.GetText("label_AbilityDescription"), GL.ExpandWidth(false)))
                        {
                            if (settings.toggleSearchByAbilityDescription == Storage.isTrueString)
                                settings.toggleSearchByAbilityDescription = Storage.isFalseString;
                            else
                                settings.toggleSearchByAbilityDescription = Storage.isTrueString;
                            Main.SearchValidAbilities(Storage.validAbilityTypes);
                        }

                        GL.EndHorizontal();

                        if (!Storage.abilityResultNames.Any())
                        {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoResult"));
                        }
                        else
                        {
                            if (Storage.abilityResultNames.Count > settings.finalResultLimit)
                            {
                                MenuTools.SingleLineLabel($"{Storage.abilityResultNames.Count} " +
                                                          Strings.GetText("label_Results"));
                                MenuTools.SingleLineLabel(Strings.GetText("label_TooManyResults_0") +
                                                          $" {settings.finalResultLimit} " +
                                                          Strings.GetText("label_TooManyResults_1"));
                                GL.Space(10);
                            }
                            else
                            {
                                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_Results") + ":"));
                                for (var i = 0; i < Storage.abilityResultNames.Count; i++)
                                {
                                    GL.BeginHorizontal();
                                    Storage.abilityToggleResultDescription.Add(false);
                                    if (!string.IsNullOrEmpty(Storage.abilityResultDescriptions[i]))
                                        Storage.abilityToggleResultDescription[i] = GL.Toggle(Storage.abilityToggleResultDescription[i], $"{Storage.abilityResultNames[i]} ({Storage.abilityResultTypes[i]})", GL.ExpandWidth(false));
                                    GL.FlexibleSpace();

                                    try
                                    {
                                        if (GL.Button(
                                            Strings.GetText("misc_Add") + $" {Storage.abilityResultNames[i]} " +
                                            Strings.GetText("misc_To") +
                                            $" {Storage.addAbilitiesAllUnitsNamesList[Storage.addAbilitiesSelectedControllableCharacterIndex]}",
                                            GL.ExpandWidth(false)))
                                        {
                                            var unitEntityData =
                                                Storage.addAbilitiesAllUnits[
                                                    Storage.addAbilitiesSelectedControllableCharacterIndex];

                                            unitEntityData.Descriptor.AddFact(
                                                (BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintAbility>(
                                                    Storage.abilityResultGuids[i]), (MechanicsContext)null,
                                                new FeatureParam());

                                            Storage.addAbilitiesAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e)
                                    {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        Main.RefreshAllAbilities(Storage.addAbilitiesAllUnitEntityData);
                                    }

                                    if (Storage.abilitiesFavourites.Contains(Storage.abilityResultGuids[i]))
                                    {
                                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                                        {
                                            Storage.abilitiesFavourites.Remove(Storage.abilityResultGuids[i]);
                                            Storage.addAbilitiesFavouritesLoad = true;
                                        }
                                    }
                                    else
                                    {
                                        if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                                        {
                                            Storage.abilitiesFavourites.Add(Storage.abilityResultGuids[i]);
                                            Storage.addAbilitiesFavouritesLoad = true;
                                        }
                                    }
                                    GL.EndHorizontal();

                                    AbilityDetails(Storage.abilityToggleResultDescription[i], Storage.abilityResultGuids[i], Storage.abilityResultDescriptions[i], Storage.addAbilitiesAllUnits[Storage.addAbilitiesSelectedControllableCharacterIndex]);
                                }

                                var filename = "ability-" + Regex.Replace(Storage.currentAbilitySearch, @"[\\/:*?""<>|]", "");
                                MenuTools.ExportCopyGuidsNamesButtons(Storage.abilityResultGuids.ToArray(), Storage.abilityResultNames.ToArray(), filename);
                            }
                        }

                        GL.EndVertical();
                    }
                }
                else
                {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitFound"));
                }
            }

            GL.EndVertical();
        }

        public static void SpellAbilityOptions()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showSpellCategory = GL.Toggle(settings.showSpellCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_SpellAbilityOptions")), GL.ExpandWidth(false));
            if (!settings.showSpellCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("SpellAbilityOptions");

                GL.Space(10);

                RestoreSpellsAbilities();
                SpellAbilityRangeMultiplier();

                SpellAbilityRange();
                MenuTools.ToggleButton(ref settings.toggleTabletopSpellAbilityRange, "buttonToggle_TabletopSpellAbilityRange", "tooltip_TabletopSpellAbilityRange", nameof(settings.toggleTabletopSpellAbilityRange));
                MenuTools.ToggleButton(ref settings.toggleSpellbookAbilityAlignmentChecks, "buttonToggle_SpellbookAbilityAlignmentChecks", "tooltip_SpellbookAbilityAlignmentChecks", nameof(settings.toggleSpellbookAbilityAlignmentChecks));

                ArcaneSpellFailureRoll();

                MenuTools.ToggleButton(ref settings.toggleAlwaysSucceedCastingDefensively,
                    "buttonToggle_AlwaysSucceedCastingDefensively", "tooltip_AlwaysSucceedCastingDefensively",
                    nameof(settings.toggleAlwaysSucceedCastingDefensively));

                MenuTools.ToggleButton(ref settings.toggleAlwaysSucceedConcentration,
                    "buttonToggle_AlwaysSucceedConcentration", "tooltip_AlwaysSucceedConcentration",
                    nameof(settings.toggleAlwaysSucceedConcentration));

                MenuTools.ToggleButton(ref settings.toggleUnlimitedCasting, "buttonToggle_UnlimitedCasting",
                    "tooltip_UnlimitedCasting", nameof(settings.toggleUnlimitedCasting));

                MenuTools.ToggleButton(ref settings.toggleMetamagic, "buttonToggle_MetamagicZero",
                    "tooltip_MetamagicZero", nameof(settings.toggleMetamagic));

                MenuTools.ToggleButton(ref settings.toggleMaterialComponent, "buttonToggle_IgnoreMaterialComponents",
                    "tooltip_IgnoreMaterialComponents", nameof(settings.toggleMaterialComponent));

                MenuTools.ToggleButton(ref settings.toggleSpontaneousCopyScrolls,
                    "buttonToggle_SpontaneousCastersCopyScrolls", "tooltip_SpontaneousCastersCopyScrolls",
                    nameof(settings.toggleSpontaneousCopyScrolls));

                MenuTools.ToggleButton(ref settings.toggleInfiniteAbilities, "buttonToggle_UnlimitedAbilities",
                    "tooltip_UnlimitedAbilities", nameof(settings.toggleInfiniteAbilities));

                MenuTools.ToggleButton(ref settings.toggleRestoreSpellsAbilitiesAfterCombat,
                    "button_RestoreSpellsAbilitiesAfterCombat", "tooltip_RestoreSpellsAbilitiesAfterCombat",
                    nameof(settings.toggleRestoreSpellsAbilitiesAfterCombat));
                Main.ToggleActiveWarning(ref settings.toggleRestoreSpellsAbilitiesAfterCombat,
                    ref settings.toggleInstantRestAfterCombat, "buttonToggle_InstantRestAfterCombat");

                MenuTools.ToggleButton(ref settings.toggleUndetectableStealth,
                    "buttonToggle_MostlyUndetectableDuringStealth", "tooltip_MostlyUndetectableDuringStealth",
                    nameof(settings.toggleUndetectableStealth));

                MenuTools.ToggleButton(ref settings.toggleFfAOE, "buttonToggle_NoFriendlyFireAOE",
                    "tooltip_NoFriendlyFireAOE", nameof(settings.toggleFfAOE));

                MenuTools.ToggleButton(ref settings.toggleFfAny, "buttonToggle_NoFriendlyFireAny",
                    "tooltip_NoFriendlyFireAny", nameof(settings.toggleFfAny));

                MenuTools.ToggleButton(ref settings.toggleInstantCooldown, "buttonToggle_NoCooldown",
                    "tooltip_NoCooldown", nameof(settings.toggleInstantCooldown));
                MenuTools.ToggleButton(ref settings.toggleInstantCooldownMainChar,
                    "buttonToggle_InstantCooldownMainChar", "tooltip_InstantCooldownMainChar",
                    nameof(settings.toggleInstantCooldownMainChar));
                Main.ToggleActiveWarning(ref settings.toggleInstantCooldown, ref settings.toggleInstantCooldownMainChar,
                    "buttonToggle_NoCooldown");

                if (Common.DLCTieflings())
                {
                    MenuTools.ToggleButton(ref settings.toggleNoBurnKineticist, "buttonToggle_NoBurnKineticist",
                        "tooltip_NoBurnKineticist", nameof(settings.toggleNoBurnKineticist));
                    GL.BeginHorizontal();
                    if (GL.Button(
                        MenuTools.TextWithTooltip("buttonToggle_MaximiseAcceptedBurnKineticist",
                            "tooltip_MaximiseAcceptedBurnKineticist", $"{settings.toggleMaximiseAcceptedBurn}" + " ",
                            ""), GL.ExpandWidth(false)))
                    {
                        if (!Strings.ToBool(settings.toggleMaximiseAcceptedBurn))
                        {
                            settings.toggleMaximiseAcceptedBurn = Storage.isTrueString;
                            foreach (var unit in Game.Instance.Player?.Party)
                                try
                                {
                                    var unitPartKineticist = unit?.Get<UnitPartKineticist>();
                                    if ((bool)(UnitPart)unitPartKineticist)
                                    {
                                        var abilityData = new AbilityData(
                                            Utilities.GetBlueprintByGuid<BlueprintAbility>(
                                                "a5631955254ae5c4d9cc2d16870448a2"), unit.Descriptor);
                                        for (var i = unitPartKineticist.AcceptedBurn;
                                            i < unitPartKineticist.MaxBurn;
                                            i++) unitPartKineticist.AcceptBurn(unitPartKineticist.MaxBurn, abilityData);
                                    }
                                }
                                catch (Exception e)
                                {
                                    modLogger.Log(e.ToString());
                                }
                        }
                        else if (Strings.ToBool(settings.toggleMaximiseAcceptedBurn))
                        {
                            settings.toggleMaximiseAcceptedBurn = Storage.isFalseString;
                        }
                    }

                    GL.EndHorizontal();

                    GL.BeginVertical("box");
                    GL.BeginHorizontal();
                    if (GL.Button(
                        MenuTools.TextWithTooltip("buttonToggle_NoTempHPKineticist", "tooltip_NoTempHPKineticist",
                            $"{settings.toggleNoTempHPKineticist}" + " ", ""), GL.ExpandWidth(false)))
                    {
                        if (settings.toggleNoTempHPKineticist == Storage.isFalseString)
                        {
                            Cheats.PatchBurnEffectBuff(0);
                            settings.toggleNoTempHPKineticist = Storage.isTrueString;
                        }
                        else if (settings.toggleNoTempHPKineticist == Storage.isTrueString)
                        {
                            Cheats.PatchBurnEffectBuff(1);
                            settings.toggleNoTempHPKineticist = Storage.isFalseString;
                        }
                    }

                    GL.FlexibleSpace();
                    if (Storage.togglesFavourites.Contains(nameof(settings.toggleNoTempHPKineticist) + "," +
                                                           "buttonToggle_NoTempHPKineticist" + "," +
                                                           "tooltip_NoTempHPKineticist"))
                    {
                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                        {
                            Storage.togglesFavourites.Remove(nameof(settings.toggleNoTempHPKineticist) + "," +
                                                             "buttonToggle_NoTempHPKineticist" + "," +
                                                             "tooltip_NoTempHPKineticist");
                            Storage.togglesFavouritesLoad = true;
                        }
                    }
                    else
                    {
                        if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                        {
                            Storage.togglesFavourites.Add(nameof(settings.toggleNoTempHPKineticist) + "," +
                                                          "buttonToggle_NoTempHPKineticist" + "," +
                                                          "tooltip_NoTempHPKineticist");
                            Storage.togglesFavouritesLoad = true;
                        }
                    }

                    GL.EndHorizontal();
                    GL.EndVertical();
                }

                GL.Space(10);
                GL.BeginHorizontal();
                GL.Label(new GUIContent(RichText.Bold(Strings.GetText("header_SpellsPerDay")),
                    Strings.GetText("tooltip_SpellsPerDay")));
                GL.EndHorizontal();
                MenuTools.SingleLineLabel(Strings.GetText("warning_SpellsPerDay"));
                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(MenuTools.TextWithTooltip("header_Multiplier", "tooltip_SpellsPerDay", "", " "),
                    GL.ExpandWidth(false));
                settings.spellsPerDayMultiplier =
                    GL.HorizontalSlider(settings.spellsPerDayMultiplier, 0.1f, 4f, GL.Width(300f));
                GL.Label($" {Math.Round(settings.spellsPerDayMultiplier, 1)}", GL.ExpandWidth(false));
                GL.EndHorizontal();

                GL.BeginHorizontal();
                settings.useCustomspellsPerDayMultiplier = GL.Toggle(settings.useCustomspellsPerDayMultiplier, MenuTools.TextWithTooltip("header_UseCustomMultiplier", "tooltip_CustomMultiplier", "", " "), GL.ExpandWidth(false));
                GL.EndHorizontal();

                if (settings.useCustomspellsPerDayMultiplier == true)
                {
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("header_CustomMultiplier") + " ", GL.ExpandWidth(false));
                    settings.customSpellsPerDayMultiplier =
                        GL.TextField(settings.customSpellsPerDayMultiplier, 6, GL.Width(100f));
                    settings.customSpellsPerDayMultiplier =
                        MenuTools.FloatTestSettingStage1(settings.customSpellsPerDayMultiplier);
                    GL.EndHorizontal();

                    settings.finalCustomspellsPerDayMultiplier =
                        MenuTools.FloatTestSettingStage2(settings.customSpellsPerDayMultiplier,
                            settings.finalCustomspellsPerDayMultiplier);

                    MenuTools.SingleLineLabel(Strings.GetText("label_CurrentMultiplier") + $": {settings.finalCustomExperienceMultiplier}");
                }
            }
            GL.EndVertical();
        }

        public static MultiplierCustom spellAbilityRangeMultiplierCustom = new MultiplierCustom(0.1f, 10f);
        public static void SpellAbilityRangeMultiplier()
        {
            spellAbilityRangeMultiplierCustom.LoadSettings(settings.spellAbilityRangeMultiplier, settings.customSpellAbilityRangeMultiplier, settings.useCustomSpellAbilityRangeMultiplier);
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleSpellAbilityRangeMultiplier, "label_SpellAbilityRangeMultiplier", "tooltip_SpellAbilityRangeMultiplier");
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(SpellAbilityRangeMultiplier));
            if (Strings.ToBool(settings.toggleSpellAbilityRangeMultiplier))
            {
                GL.BeginVertical("box");
                spellAbilityRangeMultiplierCustom.Render(ref settings.spellAbilityRangeMultiplier, ref settings.customSpellAbilityRangeMultiplier, ref settings.useCustomSpellAbilityRangeMultiplier);
                GL.EndVertical();
            }
            GL.EndVertical();
        }

        private static TextFieldInt closeRangeField = new TextFieldInt();
        private static TextFieldInt mediumRangeField = new TextFieldInt();
        private static TextFieldInt longRangeField = new TextFieldInt();
        public static void SpellAbilityRange()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleCustomSpellAbilityRange, "label_SpellAbilityRange", "tooltip_SpellAbilityRange");
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(SpellAbilityRange));
            if (Strings.ToBool(settings.toggleCustomSpellAbilityRange))
            {
                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("misc_Close") + ": ", GL.ExpandWidth(false));
                closeRangeField.RenderFieldNoGroup();
                closeRangeField.RendeSetButtonNoGroup(Strings.GetText("button_SetTo"), ref settings.customSpellAbilityRangeClose);
                GL.EndHorizontal();
                MenuTools.CurrentValue(settings.customSpellAbilityRangeClose);
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("misc_Medium") + ": ", GL.ExpandWidth(false));
                mediumRangeField.RenderFieldNoGroup();
                mediumRangeField.RendeSetButtonNoGroup(Strings.GetText("button_SetTo"), ref settings.customSpellAbilityRangeMedium);
                GL.EndHorizontal();
                MenuTools.CurrentValue(settings.customSpellAbilityRangeMedium);
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("misc_Long") + ": ", GL.ExpandWidth(false));
                longRangeField.RenderFieldNoGroup();
                longRangeField.RendeSetButtonNoGroup(Strings.GetText("button_SetTo"), ref settings.customSpellAbilityRangeLong);
                GL.EndHorizontal();
                MenuTools.CurrentValue(settings.customSpellAbilityRangeLong);
                GL.EndVertical();
            }
            MenuTools.IncompatibilityWarning(RichText.BoldRedFormat(Strings.GetText("label_SpellAbilityCustomRangeTabletopWarning")), settings.toggleTabletopSpellAbilityRange, settings.toggleCustomSpellAbilityRange);
            GL.EndVertical();
        }

        public static void RestoreSpellsAbilities()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("button_RestoreSpellsAbilites", "tooltip_RestoreSpellsAbilites", false), GL.ExpandWidth(false)))
            {
                List<UnitEntityData> partyMembers = Game.Instance.Player.ControllableCharacters;
                foreach (UnitEntityData u in partyMembers)
                {
                    foreach (BlueprintScriptableObject resource in u.Descriptor.Resources)
                        u.Descriptor.Resources.Restore(resource);
                    foreach (Spellbook spellbook in u.Descriptor.Spellbooks)
                        spellbook.Rest();
                    u.Brain.RestoreAvailableActions();
                }
            }
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(RestoreSpellsAbilities));
            GL.EndVertical();
        }

        public static void ArcaneSpellFailureRoll()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButtonFavouritesMenu(ref settings.toggleArcaneSpellFailureRoll, "buttonToggle_NoArcaneSpellFailure", "tooltip_NoArcaneSpellFailure");
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(ArcaneSpellFailureRoll));
            if (Strings.ToBool(settings.toggleArcaneSpellFailureRoll))
            {
                MenuTools.ToggleButton(ref settings.toggleArcaneSpellFailureRollOutOfCombatOnly, "buttonToggle_OutOfCombatOnly", "tooltip_OutOfCombatOnly_ArcaneSpellFailureRoll");
            }
            GL.EndVertical();
        }

        public static void MapTravel()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showTravelAndMapCategory = GL.Toggle(settings.showTravelAndMapCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_MapTravel")), GL.ExpandWidth(false));
            if (!settings.showTravelAndMapCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("MapTravel");

                GL.Space(10);

                MapTravelInfo();

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleInstantPartyChange, "buttonToggle_InstantPartyChange",
                    "tooltip_InstantPartyChange", nameof(settings.toggleInstantPartyChange));

                GL.Space(10);

                MoveSpeedAsOne();

                GL.Space(10);

                RevealLocations();

                GL.Space(10);

                ClaimResources();

                GL.Space(10);

                NoResourceClaimCost();

                GL.Space(10);

                TravelTeleport();

                GL.Space(10);

                TeleportPartyToPlayer();

                GL.Space(10);

                TravelSpeedMultiplier();

                GL.Space(10);

                RandomEncounterSettings();
            }

            GL.EndVertical();
        }

        public static void TravelSpeedMultiplier()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(MenuTools.TextWithTooltip("headerOption_TravelSpeedMultiplier", "tooltip_TravelSpeedMultiplier", "", " "), GL.ExpandWidth(false));
            settings.travelSpeedMultiplierString = GL.TextField(settings.travelSpeedMultiplierString, 10, GL.Width(90f));
            MenuTools.SettingParse(ref settings.travelSpeedMultiplierString, ref settings.travelSpeedMultiplier);
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(TravelSpeedMultiplier));
            GL.EndVertical();
        }

        public static void TravelTeleport()
        {
            GL.BeginVertical("box");
            MenuTools.ToggleButton(ref settings.toggleEnableTeleport, "buttonToggle_EnableTeleport", "tooltip_EnableTeleport", nameof(settings.toggleEnableTeleport));

            GL.BeginHorizontal();
            if (settings.toggleEnableTeleport == Storage.isTrueString)
            {
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_Teleport") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.teleportKey);
                GL.EndHorizontal();
            }
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(TravelTeleport));
            GL.EndVertical();
        }

        public static void TeleportPartyToPlayer()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(new GUIContent(Strings.GetText("button_TeleportPartyToPlayer"), Strings.GetText("tooltip_TeleportPartyToPlayer")), GL.ExpandWidth(false)))
            {
                Common.TeleportPartyToPlayer();
            }
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(TeleportPartyToPlayer));
            GL.EndVertical();
        }

        public static void MapTravelInfo()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(Strings.GetText("label_MilesTravelled") + $": {Game.Instance.Player.GlobalMap.MilesTravelled}");
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(MapTravelInfo));
            MenuTools.SingleLineLabel(Strings.GetText("label_NextEncounterRollAt") + $": {Game.Instance.Player.GlobalMap.NextEncounterRollMiles}");
            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentRegionCR") + $": {Game.Instance.Player.GlobalMap.CurrentRegionCR}");
            GL.EndVertical();
        }

        public static void ClaimResources()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("buttonToggle_ClaimResources", "tooltip_ClaimResources", false),
                GL.ExpandWidth(false))) Cheats.ClaimResources();
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(ClaimResources));
            GL.EndVertical();
        }

        public static void RevealLocations()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.Bold(Strings.GetText("buttonToggle_RevealMapLocations")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(RevealLocations));
            MenuTools.SingleLineLabel(RichText.BoldRedFormat(Strings.GetText("warning_RevealLocations")));

            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_RevealAllMapLocationsXWaypointHidden"), GL.ExpandWidth(false)))
                Cheats.RevealLocationsByType(new List<LocationType> { LocationType.Landmark, LocationType.Location });
            GL.EndHorizontal();

            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_RevealAllMapLocations"), GL.ExpandWidth(false)))
                Cheats.RevealLocationsByType(new List<LocationType> { LocationType.Landmark, LocationType.HiddenLocation, LocationType.Location, LocationType.Waypoint });
            GL.Space(10);
            if (GL.Button(Strings.GetText("button_RevealAllReachableMapLocations"), GL.ExpandWidth(false)))
                Cheats.RevealReachableLocations();
            GL.EndHorizontal();

            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_RevealAllRegularLocations"), GL.ExpandWidth(false)))
                Cheats.RevealLocationsByType(new List<LocationType> { LocationType.Location });
            GL.Space(10);
            if (GL.Button(Strings.GetText("button_RevealAllLandmarks"), GL.ExpandWidth(false)))
                Cheats.RevealLocationsByType(new List<LocationType> { LocationType.Landmark });
            GL.Space(10);
            if (GL.Button(Strings.GetText("button_RevealAllWaypoints"), GL.ExpandWidth(false)))
                Cheats.RevealLocationsByType(new List<LocationType> { LocationType.Waypoint });
            GL.EndHorizontal();

            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_RevealAllHiddenLocations"), GL.ExpandWidth(false)))
                Cheats.RevealLocationsByType(new List<LocationType> { LocationType.HiddenLocation });
            GL.Space(10);
            if (GL.Button(Strings.GetText("button_RevealAllSystemWaypoints"), GL.ExpandWidth(false)))
                Cheats.RevealLocationsByType(new List<LocationType> { LocationType.SystemWaypoint });
            GL.EndHorizontal();

            GL.EndVertical();
        }

        private static TextFieldFloat partyMovementSpeedMultiplierTextField = new TextFieldFloat();

        public static void MoveSpeedAsOne()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(
                MenuTools.TextWithTooltip("buttonToggle_SetPartySpeedToFastestMember",
                    "tooltip_SetPartySpeedToFastestMember", $"{settings.toggleMoveSpeedAsOne}" + " ", ""),
                GL.ExpandWidth(false)))
            {
                if (settings.toggleMoveSpeedAsOne == Storage.isFalseString)
                {
                    settings.toggleMoveSpeedAsOne = Storage.isTrueString;
                }
                else if (settings.toggleMoveSpeedAsOne == Storage.isTrueString)
                {
                    settings.togglePartyMovementSpeedMultiplier = Storage.isFalseString;
                    settings.toggleMoveSpeedAsOne = Storage.isFalseString;
                }
            }

            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(MoveSpeedAsOne));
            MenuTools.ToggleButtonDependant(ref settings.togglePartyMovementSpeedMultiplier, ref settings.toggleMoveSpeedAsOne, "buttonToggle_PartyMovementSpeedMultiplier", "tooltip_PartyMovementSpeedMultiplier");

            if (Strings.ToBool(settings.togglePartyMovementSpeedMultiplier))
            {
                partyMovementSpeedMultiplierTextField.RenderField();
                GL.Space(10);
                GL.BeginHorizontal();
                if (GL.Button(Strings.GetText("button_SetTo") + $" {partyMovementSpeedMultiplierTextField.finalAmount}",
                    GL.ExpandWidth(false)))
                    settings.partyMovementSpeedMultiplierValue = partyMovementSpeedMultiplierTextField.finalAmount;
                GL.EndHorizontal();
                MenuTools.SingleLineLabel(Strings.GetText("label_CurrentValue") +
                                          $": {settings.partyMovementSpeedMultiplierValue}");
            }

            GL.EndVertical();
        }

        private static SelectionGrid kingdomAlignmentGrid = new SelectionGrid(Storage.alignmentsArrayKingdomStats, 3);
        private static TextFieldInt consumableEventBonusField = new TextFieldInt();

        public static void Kingdom()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showKingdomCategory = GL.Toggle(settings.showKingdomCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_Kingdom")), GL.ExpandWidth(false));
            if (!settings.showKingdomCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("Kingdom");

                GL.Space(10);

                if (KingdomState.Instance != null)
                {
                    GL.Space(10);
                    GL.BeginVertical("box");
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_KingdomName")));

                    MenuTools.SingleLineLabel(
                        $"{Strings.GetText("label_CurrentName")}: {Game.Instance.Player.Kingdom.KingdomName}");
                    GL.BeginHorizontal();
                    Storage.kingdomCustomName = GL.TextField(Storage.kingdomCustomName, 36, GL.Width(300f));
                    GL.EndHorizontal();
                    GL.BeginHorizontal();
                    if (GL.Button(Strings.GetText("button_SetTo") + $" {Storage.kingdomCustomName}",
                        GL.ExpandWidth(false))) Game.Instance.Player.Kingdom.SetKingdomName(Storage.kingdomCustomName);
                    GL.EndHorizontal();
                    GL.EndVertical();

                    GL.Space(10);

                    GL.BeginVertical("box");
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_CrisisPoints")));
                    MenuTools.SingleLineLabel(
                        $"{Strings.GetText("label_CurrentCrisisPoints")}: {Game.Instance.Player.Kingdom.ConsumableEventBonus}");

                    consumableEventBonusField.RenderField();

                    if (GL.Button($"+ {consumableEventBonusField.finalAmount}", GL.ExpandWidth(false)))
                        Game.Instance.Player.Kingdom.ConsumableEventBonus =
                            Game.Instance.Player.Kingdom.ConsumableEventBonus + consumableEventBonusField.finalAmount;
                    GL.EndVertical();


                    GL.Space(10);

                    GL.BeginVertical("box");
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("label_ArtisanMasterPieceChance")) +
                                              $": {KingdomRoot.Instance.ArtisanMasterpieceChance}");
                    GL.BeginHorizontal();
                    settings.artisanMasterpieceChance =
                        GL.HorizontalSlider(settings.artisanMasterpieceChance, 0f, 1f, GL.Width(400f));
                    GL.Label($" {Math.Round(settings.artisanMasterpieceChance, 3)}", GL.ExpandWidth(false));
                    GL.EndHorizontal();
                    GL.BeginHorizontal();
                    if (GL.Button(
                        Strings.GetText("button_SetTo") + $" {Math.Round(settings.artisanMasterpieceChance, 3)}",
                        GL.ExpandWidth(false)))
                        KingdomRoot.Instance.ArtisanMasterpieceChance = (float)Math.Round(settings.artisanMasterpieceChance, 3);
                    if (GL.Button(Strings.GetText("button_SetToDefault") + $" ({Defaults.artisanMasterpieceChance})",
                        GL.ExpandWidth(false)))
                        KingdomRoot.Instance.ArtisanMasterpieceChance = Defaults.artisanMasterpieceChance;
                    GL.EndHorizontal();
                    GL.EndVertical();

                    GL.BeginVertical("box");
                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_KingdomAlignment")));
                    kingdomAlignmentGrid.Render();

                    GL.Space(10);
                    GL.BeginHorizontal();
                    if (GL.Button(
                        MenuTools.TextWithTooltip("button_SetAlignment", "tooltip_SetKingdomAlignment", "",
                            $" {Storage.alignmentsArrayKingdomStats[kingdomAlignmentGrid.selected]}"),
                        GL.ExpandWidth(false)))
                        KingdomState.Instance.Alignment =
                            (Alignment)Common.IndexToAligment(kingdomAlignmentGrid.selected);
                    GL.EndHorizontal();
                    MenuTools.SingleLineLabel(Strings.GetText("label_CurrentAlignment") + ": " +
                                              Common.AlignmentToString(KingdomState.Instance.Alignment));
                    GL.EndVertical();

                    GL.Space(10);

                    GL.BeginVertical("box");
                    settings.showUpgradSettlementsAndBuildings = GL.Toggle(settings.showUpgradSettlementsAndBuildings, new GUIContent(RichText.Bold(Strings.GetText("toggle_UpgradeSettlementsAndBuildings")), Strings.GetText("tooltip_UpgradeSettlementsAndBuildings")));
                    if (settings.showUpgradSettlementsAndBuildings)
                        if (KingdomState.Instance.Regions != null)
                        {
                            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("warning_UpgradeSetllement")));
                            foreach (var region in KingdomState.Instance.Regions)
                                if (region.IsClaimed)
                                    if (region.Settlement != null)
                                    {
                                        MenuTools.SingleLineLabel(RichText.Bold(
                                            region.Blueprint.LocalizedName.ToString() + " - " +
                                            region.Settlement.Name));
                                        MenuTools.SingleLineLabel(
                                            Strings.GetText("label_CurrentLevel") + ": " +
                                            region.Settlement.Level.ToString());

                                        if (region.Settlement.Level == SettlementState.LevelType.Village ||
                                            region.Settlement.Level == SettlementState.LevelType.Town)
                                            if (GL.Button(Strings.GetText("button_LevelUp"), GL.ExpandWidth(false)))
                                                region.Settlement.LevelUp();

                                        foreach (var building in region.Settlement.Buildings)
                                            if (building.Blueprint.UpgradesTo != null)
                                            {
                                                GL.BeginVertical("box");
                                                GL.BeginHorizontal();
                                                GL.Label(building.Blueprint.Name.ToString());
                                                GL.FlexibleSpace();
                                                if (GL.Button(
                                                    Strings.GetText("button_Upgrade") + " " +
                                                    Strings.GetText("misc_To") + " " +
                                                    building.Blueprint.UpgradesTo.Name.ToString(),
                                                    GL.ExpandWidth(false)))
                                                    region.Settlement.BuildingCollection.Upgrade(building);
                                                GL.EndHorizontal();
                                                GL.EndVertical();
                                            }
                                    }
                        }

                    GL.EndVertical();
                    GL.Space(10);

                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_BuildingTimeModifier")));
                    MenuTools.SingleLineLabel(Strings.Parenthesis(Strings.GetText("label_BuildingTimeInfo")));
                    GL.BeginHorizontal();
                    settings.kingdomBuildingTimeModifier = GL.HorizontalSlider(settings.kingdomBuildingTimeModifier,
                        -10f, 10f, GL.Width(300f));
                    GL.Label($" {Math.Round(settings.kingdomBuildingTimeModifier, 3)}", GL.ExpandWidth(false));
                    GL.EndHorizontal();
                    GL.BeginHorizontal();
                    if (GL.Button(
                        Strings.GetText("headerOption_AdjustBy") +
                        $" {Math.Round(settings.kingdomBuildingTimeModifier, 3)}", GL.ExpandWidth(false)))
                        KingdomState.Instance.BuildingTimeModifier =
                            KingdomState.Instance.BuildingTimeModifier +
                            (float)Math.Round(settings.kingdomBuildingTimeModifier, 3);
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    if (GL.Button(Strings.GetText("button_SetTo0"), GL.ExpandWidth(false)))
                        KingdomState.Instance.BuildingTimeModifier = 0;
                    GL.EndHorizontal();
                    MenuTools.SingleLineLabel(Strings.GetText("label_CurrentBuildingTimeModifier") +
                                              $": {KingdomState.Instance.BuildingTimeModifier}");

                    GL.Space(10);

                    GL.BeginHorizontal();
                    GL.Label(MenuTools.TextWithTooltip("headerOption_OverrideKingdomAlignmentResult",
                        "tooltip_OverrideKingdomAlignmentResult", "", ":", true));
                    GL.EndHorizontal();
                    GL.BeginHorizontal();
                    settings.selectedKingdomAlignment = GL.SelectionGrid(settings.selectedKingdomAlignment,
                        Storage.alignmentsArrayKingdom, 3);
                    settings.selectedKingdomAlignmentString = settings.selectedKingdomAlignment.ToString();
                    settings.selectedKingdomAlignmentTranslated =
                        Main.IntToAlignment(settings.selectedKingdomAlignment);
                    GL.EndHorizontal();
                    GL.Space(10);

                    MenuTools.ToggleButton(ref settings.toggleInstantEvent, "buttonToggle_InstantKingdomEvents",
                        "tooltip_InstantKingdomEvents", nameof(settings.toggleInstantEvent));

                    MenuTools.ToggleButton(ref settings.toggleKingdomEventResultSuccess,
                        "buttonToggle_ForceSuccessTriumphs", "tooltip_ForceSuccessTriumphs",
                        nameof(settings.toggleKingdomEventResultSuccess));

                    MenuTools.ToggleButton(ref settings.toggleSettlementRestrictions,
                        "buttonToggle_IgnoreBuildingRestrictions", "tooltip_IgnoreBuildingRestrictions",
                        nameof(settings.toggleSettlementRestrictions));

                    GL.Space(10);

                    MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_BuildPoints")));

                    GL.BeginHorizontal();
                    settings.buildPointAmount = GL.TextField(settings.buildPointAmount, 10, GL.Width(100f));

                    settings.buildPointAmount = MenuTools.IntTestSettingStage1(settings.buildPointAmount);
                    settings.finalBuildPointAmount =
                        MenuTools.IntTestSettingStage2(settings.buildPointAmount, settings.finalBuildPointAmount);
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    if (GL.Button($"+{settings.buildPointAmount} BP", GL.ExpandWidth(false)))
                        KingdomState.Instance.BP = KingdomState.Instance.BP + settings.finalBuildPointAmount;
                    if (GL.Button($"-{settings.buildPointAmount} BP", GL.ExpandWidth(false)))
                        KingdomState.Instance.BP = KingdomState.Instance.BP - settings.finalBuildPointAmount;
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_CurrentBuildPoints") + $": {KingdomState.Instance.BP}", GL.ExpandWidth(false));
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    settings.buildPointAmount = MenuTools.IntTestSettingStage1(settings.buildPointAmount);
                    settings.finalBuildPointAmount =
                        MenuTools.IntTestSettingStage2(settings.buildPointAmount, settings.finalBuildPointAmount);

                    if (GL.Button($"+{settings.buildPointAmount} " + Strings.GetText("button_BaseBPIncome"), GL.ExpandWidth(false)))
                        KingdomState.Instance.BPPerTurn =
                            KingdomState.Instance.BPPerTurn + settings.finalBuildPointAmount;
                    if (GL.Button($"-{settings.buildPointAmount} " + Strings.GetText("button_BaseBPIncome"), GL.ExpandWidth(false)))
                        KingdomState.Instance.BPPerTurn =
                            KingdomState.Instance.BPPerTurn - settings.finalBuildPointAmount;
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_CurrentBaseBuildPointIncome") + $": {KingdomState.Instance.BPPerTurn}", GL.ExpandWidth(false));
                    GL.EndHorizontal();
                    GL.Space(10);

                    GL.BeginHorizontal();
                    GL.Label(MenuTools.TextWithTooltip("header_Statistics", "tooltip_Statistics", true));
                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    settings.kingdomStatsAmount = GL.TextField(settings.kingdomStatsAmount, 10, GL.Width(85f));

                    settings.kingdomStatsAmount = MenuTools.IntTestSettingStage1(settings.kingdomStatsAmount);
                    settings.kingdomFinalStatsAmount = MenuTools.IntTestSettingStage2(settings.kingdomStatsAmount,
                        settings.kingdomFinalStatsAmount);
                    GL.EndHorizontal();
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Community"), KingdomStats.Type.Community);
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Loyalty"), KingdomStats.Type.Loyalty);
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Military"), KingdomStats.Type.Military);
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Economy"), KingdomStats.Type.Economy);
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Relations"), KingdomStats.Type.Relations);
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Divine"), KingdomStats.Type.Divine);
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Arcane"), KingdomStats.Type.Arcane);
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Stability"), KingdomStats.Type.Stability);
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Culture"), KingdomStats.Type.Culture);
                    CreateKingdomStatInterface(Strings.GetText("headerOption_Espionage"), KingdomStats.Type.Espionage);
                }
                else
                {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoKingdom"));
                }
            }

            GL.EndVertical();
        }

        public static void CreateKingdomStatInterface(string stat, KingdomStats.Type statType)
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label($"{stat}: {KingdomState.Instance.Stats[statType].Value}");
            if (GL.Button(RichText.Bold("+"), GL.ExpandWidth(false))) KingdomState.Instance.Stats[statType].Value++;
            if (GL.Button(RichText.Bold("-"), GL.ExpandWidth(false))) KingdomState.Instance.Stats[statType].Value--;
            if (GL.Button(Strings.GetText("button_SetTo") + $" {settings.kingdomStatsAmount}", GL.ExpandWidth(false)))
                KingdomState.Instance.Stats[statType].Value = settings.kingdomFinalStatsAmount;
            GL.EndHorizontal();

            GL.BeginHorizontal();
            GL.Label(Strings.GetText("header_Rank") + $": {KingdomState.Instance.Stats[statType].Rank}");
            if (GL.Button(RichText.Bold("+"), GL.ExpandWidth(false))) KingdomState.Instance.Stats[statType].Rank++;
            if (GL.Button(RichText.Bold("-"), GL.ExpandWidth(false))) KingdomState.Instance.Stats[statType].Rank--;
            if (GL.Button(Strings.GetText("button_SetTo") + $" {settings.kingdomStatsAmount}", GL.ExpandWidth(false)))
                KingdomState.Instance.Stats[statType].Rank = settings.kingdomFinalStatsAmount;
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void DiceRolls()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showDiceRollCategory = GL.Toggle(settings.showDiceRollCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_Dice")), GL.ExpandWidth(false));
            if (!settings.showDiceRollCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("DiceRolls");

                GL.Space(10);

                PassSkillChecksIndividual();

                GL.Space(10);

                PassSavingThrowIndividual();

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleAllHitsAreCritical, "buttonToggle_AllHitsAreCritical",
                    "tooltip_AllHitsAreCritical", nameof(settings.toggleAllHitsAreCritical));

                MenuTools.ToggleButton(ref settings.toggleMainCharacterRoll20,
                    "buttonToggle_MainCharacterAlwaysRolls20", "tooltip_MainCharacterRolls20",
                    nameof(settings.toggleMainCharacterRoll20));

                GL.BeginHorizontal();
                if (GL.Button(
                    MenuTools.TextWithTooltip("buttonToggle_PartyAlwaysRolls20", "tooltip_PartyAlwaysRolls20",
                        $"{settings.togglePartyAlwaysRoll20}" + " ", ""), GL.ExpandWidth(false)))
                {
                    if (settings.togglePartyAlwaysRoll20 == Storage.isFalseString)
                    {
                        settings.togglePartyAlwaysRoll20 = Storage.isTrueString;
                        Common.AddLogEntry(
                            Strings.GetText("buttonToggle_PartyAlwaysRolls20") + ": " +
                            Strings.GetText("logMessage_Enabled"), Color.black);
                    }
                    else if (settings.togglePartyAlwaysRoll20 == Storage.isTrueString)
                    {
                        settings.togglePartyAlwaysRoll20 = Storage.isFalseString;
                        Common.AddLogEntry(
                            Strings.GetText("buttonToggle_PartyAlwaysRolls20") + ": " +
                            Strings.GetText("logMessage_Disabled"), Color.black);
                    }
                }

                GL.EndHorizontal();
                MenuTools.ToggleButton(ref settings.toggleEnablePartyAlwaysRoll20Hotkey, "buttonToggle_EnableHotkey",
                    "tooltip_EnablePartyAlwaysRoll20Hotkey");

                if (settings.toggleEnablePartyAlwaysRoll20Hotkey == Storage.isTrueString)
                {
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_TogglePartyAlwaysRolls20") + ": ", GL.ExpandWidth(false));
                    MenuTools.SetKeyBinding(ref settings.togglePartyAlwaysRoll20Key);
                    GL.EndHorizontal();
                }

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleEnemiesAlwaysRoll1, "buttonToggle_EnemiesAlwaysRoll1",
                    "tooltip_EnemiesAlwaysRoll1", nameof(settings.toggleEnemiesAlwaysRoll1));

                MenuTools.ToggleButton(ref settings.toggleEveryoneExceptPlayerFactionRolls1,
                    "buttonToggle_EveryoneExceptPlayerFactionRolls1", "tooltip_EveryoneExceptPlayerFactionRolls1",
                    nameof(settings.toggleEveryoneExceptPlayerFactionRolls1));

                MenuTools.ToggleButton(ref settings.toggleNeverRoll1, "buttonToggle_NeverRoll1", "tooltip_NeverRoll1");
                if (settings.toggleNeverRoll1 == Storage.isTrueString)
                {
                    GL.BeginHorizontal();
                    settings.neverRoll1Index = GL.SelectionGrid(settings.neverRoll1Index, Storage.neverRollXArray, 3);
                    GL.EndHorizontal();
                }

                MenuTools.ToggleButton(ref settings.toggleNeverRoll20, "buttonToggle_NeverRoll20",
                    "tooltip_NeverRoll20");
                if (settings.toggleNeverRoll20 == Storage.isTrueString)
                {
                    GL.BeginHorizontal();
                    settings.neverRoll20Index = GL.SelectionGrid(settings.neverRoll20Index, Storage.neverRollXArray, 3);
                    GL.EndHorizontal();
                }

                MenuTools.ToggleButton(ref settings.toggleRollWithAdvantage, "buttonToggle_RollWithAdvantage",
                    "tooltip_RollWithAdvantage");
                if (settings.toggleRollWithAdvantage == Storage.isTrueString)
                {
                    GL.BeginHorizontal();
                    settings.rollWithAdvantageIndex =
                        GL.SelectionGrid(settings.rollWithAdvantageIndex, Storage.neverRollXArray, 3);
                    GL.EndHorizontal();
                }

                MenuTools.ToggleButton(ref settings.toggleRollWithDisadvantage, "buttonToggle_RollWithDisadvantage",
                    "tooltip_RollWithDisadvantage");
                if (settings.toggleRollWithDisadvantage == Storage.isTrueString)
                {
                    GL.BeginHorizontal();
                    settings.rollWithDisadvantageeIndex = GL.SelectionGrid(settings.rollWithDisadvantageeIndex,
                        Storage.neverRollXArray, 3);
                    GL.EndHorizontal();
                }

                GL.BeginHorizontal();
                if (GL.Button(
                    MenuTools.TextWithTooltip("buttonToggle_AlwaysRoll20ForInitiative",
                        "tooltip_AlwaysRoll20ForInitiative", $"{settings.toggleRoll20Initiative}" + " ", ""),
                    GL.ExpandWidth(false)))
                {
                    if (settings.toggleRoll20Initiative == Storage.isFalseString)
                    {
                        settings.toggleRoll20Initiative = Storage.isTrueString;
                        HarmonyInstance.Create("kingmaker.toggleRoll20Initiative").Patch(
                            AccessTools.Method(typeof(RuleInitiativeRoll), "OnTrigger"),
                            new HarmonyMethod(typeof(HarmonyPatches).GetMethod("RuleInitiativeRoll_OnTrigger_Patch")),
                            null);
                    }
                    else if (settings.toggleRoll20Initiative == Storage.isTrueString)
                    {
                        settings.toggleRoll20Initiative = Storage.isFalseString;
                        HarmonyInstance.Create("kingmaker.toggleRoll20Initiative").Patch(
                            AccessTools.Method(typeof(RuleInitiativeRoll), "OnTrigger"),
                            new HarmonyMethod(typeof(HarmonyPatches).GetMethod("RuleInitiativeRoll_OnTrigger_Patch")),
                            null);
                    }
                }

                GL.Label(Strings.Parenthesis(Strings.GetText("label_AlwaysRoll20ForIntiativeInfo")),
                    GL.ExpandWidth(false));
                GL.EndHorizontal();

                GL.Space(10);

                TakeX();
            }
            GL.EndVertical();
        }

        public static void TakeX()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(MenuTools.TextWithTooltip("header_TakeX", "tooltip_TakeX", true), GL.ExpandWidth(false));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(TakeX));

            GL.BeginHorizontal();
            settings.takeXIndex = GL.SelectionGrid(settings.takeXIndex, Storage.takeXArray, 4);
            GL.EndHorizontal();
            if (settings.togglePartyAlwaysRoll20 == Storage.isTrueString && settings.takeXIndex != 0)
            {
                MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_TakeXAndPartyRoll20")));
            }
            if (settings.takeXIndex != 0)
            {
                MenuTools.SingleLineLabel(Strings.GetText("warning_TakeX"));
            }
            if (settings.takeXIndex == 3)
            {
                GL.BeginHorizontal();
                GL.Label("Custom Value: ", GL.ExpandWidth(false));
                settings.takeXCustom = GL.HorizontalSlider(settings.takeXCustom, 1f, 20f, GL.Width(300f));
                GL.Label($" {Mathf.RoundToInt(settings.takeXCustom)}", GL.ExpandWidth(false));
                GL.EndHorizontal();
            }

            GL.EndVertical();
        }

        public static void Camera()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.MainCategoryFormat(Strings.GetText("mainCategory_CameraTools")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(Camera));

            MenuTools.ToggleButton(ref settings.toggleEnableCameraRotation, "buttonToggle_CameraRotation",
                "tooltip_CameraRotation");

            if (settings.toggleEnableCameraRotation == Storage.isTrueString)
            {
                Game.Instance.Player.OnAreaLoaded();
                if (Game.Instance?.UI?.GetCameraRig() != null)
                {
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_CameraTurnRate") + ": ", GL.ExpandWidth(false));
                    settings.cameraTurnRate = GL.HorizontalSlider(settings.cameraTurnRate, 0.2f, 5.0f);
                    GL.EndHorizontal();

                    MenuTools.SingleLineLabel(Strings.GetText("label_CurrentRotation") +
                                              $": {settings.cameraRotation}");
                    MenuTools.SingleLineLabel(Strings.GetText("label_CurrentDefaultRotation") +
                                              $": {settings.defaultRotation}");

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_RotateCameraLeft") + ": ", GL.ExpandWidth(false));
                    MenuTools.SetKeyBinding(ref settings.cameraTurnLeft);
                    GL.EndHorizontal();
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_RotateCameraRight") + ": ", GL.ExpandWidth(false));
                    MenuTools.SetKeyBinding(ref settings.cameraTurnRight);
                    GL.EndHorizontal();
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_ResetCameraRotation") + ": ", GL.ExpandWidth(false));
                    MenuTools.SetKeyBinding(ref settings.cameraReset);
                    GL.EndHorizontal();
                }

                GL.Space(10);
            }

            MenuTools.ToggleButton(ref settings.toggleEnableCameraScrollSpeed, "buttonToggle_CameraScrollSpeed",
                "tooltip_CameraScrollSpeed");

            if (settings.toggleEnableCameraScrollSpeed == Storage.isTrueString)
            {
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_CameraScrollSpeed") + ": ", GL.ExpandWidth(false));
                unsafe
                {
                    if (Main.CameraScrollSpeed != null)
                        *Main.CameraScrollSpeed = GL.HorizontalSlider(*Main.CameraScrollSpeed, 0f, 250f);
                }

                GL.EndHorizontal();
                GL.Space(10);
            }

            MenuTools.ToggleButton(ref settings.toggleEnableCameraZoom, "buttonToggle_CameraZoomUnlocked",
                "tooltip_CameraZoomUnlocked");

            if (settings.toggleEnableCameraZoom == Storage.isTrueString)
                if (Game.Instance?.UI?.GetCameraRig() != null)
                {
                    GL.BeginHorizontal();
                    Storage.fovValue = GL.TextField(Storage.fovValue, 10, GL.Width(250f));
                    Storage.fovValue = MenuTools.FloatTestSettingStage1(Storage.fovValue);
                    GL.EndHorizontal();

                    settings.finalFovValue = MenuTools.FloatTestSettingStage2(Storage.fovValue, settings.finalFovValue);

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_CurrentLowerZoomLimit") + $": {settings.savedFovMin}",
                        GL.Width(250f));
                    if (GL.Button(Strings.GetText("button_SetLowerZoomLimit") + $" {settings.finalFovValue}",
                        GL.Width(250f)))
                    {
                        settings.savedFovMin = settings.finalFovValue.ToString();
                        HarmonyInstance.Create("kingmaker.camerazoom").Patch(
                            AccessTools.Method(typeof(CameraZoom), "TickZoom"),
                            new HarmonyMethod(typeof(HarmonyPatches).GetMethod("CameraZoom_TickZoom_Patch")), null);
                    }

                    if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    {
                        settings.savedFovMin = Defaults.fovMin.ToString();
                        HarmonyInstance.Create("kingmaker.camerazoom").Patch(
                            AccessTools.Method(typeof(CameraZoom), "TickZoom"),
                            new HarmonyMethod(typeof(HarmonyPatches).GetMethod("CameraZoom_TickZoom_Patch")), null);
                    }

                    GL.EndHorizontal();

                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("label_CurrentUpperZoomLimit") + $": {settings.savedFovMax}",
                        GL.Width(250f));
                    if (GL.Button(Strings.GetText("button_SetUpperZoomLimit") + $" {settings.finalFovValue}",
                        GL.Width(250f)))
                    {
                        settings.savedFovMax = settings.finalFovValue.ToString();
                        HarmonyInstance.Create("kingmaker.camerazoom").Patch(
                            AccessTools.Method(typeof(CameraZoom), "TickZoom"),
                            new HarmonyMethod(typeof(HarmonyPatches).GetMethod("CameraZoom_TickZoom_Patch")), null);
                    }

                    if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    {
                        settings.savedFovMax = Defaults.fovMax.ToString();
                        HarmonyInstance.Create("kingmaker.camerazoom").Patch(
                            AccessTools.Method(typeof(CameraZoom), "TickZoom"),
                            new HarmonyMethod(typeof(HarmonyPatches).GetMethod("CameraZoom_TickZoom_Patch")), null);
                    }

                    GL.EndHorizontal();

                    GL.Space(10);

                    GL.BeginHorizontal();
                    GL.Label($"{Storage.globalString}: {settings.savedFovGlobalMap}", GL.Width(250f));
                    if (GL.Button(Strings.GetText("button_SetGlobalMapZoomLevel") + $" {settings.finalFovValue}",
                        GL.Width(250f)))
                    {
                        settings.savedFovGlobalMap = settings.finalFovValue.ToString();
                        Storage.globalChanged = true;
                        Storage.globalString = Strings.GetText("label_GlobalMapZoomLevelRestartButtonChange");
                    }

                    if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                    {
                        settings.savedFovGlobalMap = Defaults.fovGlobalMap.ToString();
                        HarmonyInstance.Create("kingmaker.camerazoom").Patch(
                            AccessTools.Method(typeof(CameraZoom), "TickZoom"),
                            new HarmonyMethod(typeof(HarmonyPatches).GetMethod("CameraZoom_TickZoom_Patch")), null);
                        Storage.globalChanged = true;
                        Storage.globalString = Strings.GetText("label_GlobalMapZoomLevelRestartButtonChange");
                    }

                    GL.EndHorizontal();
                    if (Storage.globalChanged)
                    {
                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_GlobalMapZoomLevelRestart"));
                        GL.EndHorizontal();
                    }

                    if (float.Parse(settings.savedFovMin) > float.Parse(settings.savedFovMax))
                    {
                        GL.BeginHorizontal();
                        GL.Label(
                            Strings.GetText("warning_ZoomLimits_1") + $" ({settings.savedFovMin}) " +
                            Strings.GetText("warning_ZoomLimits_0") + " ({settings.savedFovMax}).",
                            GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("warning_ZoomLimits_2"), GL.ExpandWidth(false));
                        GL.EndHorizontal();
                    }

                    GL.Space(10);
                }


            MenuTools.ToggleButton(ref settings.toggleEnableFocusCamera, "buttonToggle_EnableFocusCamera",
                "tooltip_EnableFocusCamera");

            if (settings.toggleEnableFocusCamera == Storage.isTrueString)
            {
                MenuTools.ToggleButton(ref settings.toggleEnableFocusCameraSelectedUnit,
                    "buttonToggle_EnableFocusCameraSelectedUnit", "tooltip_EnableFocusCameraSelectedUnit");

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_FocusCamera") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.focusCameraKey);
                GL.EndHorizontal();
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_CycleFocus") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.focusCylceKey);
                GL.EndHorizontal();

                if (settings.focusCameraToggle)
                    MenuTools.SingleLineLabel(Strings.GetText("label_CurrentFocus") +
                                              $": {Storage.partyMembersFocusUnits[settings.partyMembersFocusPositionCounter].CharacterName}");
                GL.Space(10);
            }


            GL.BeginHorizontal();
            if (Game.Instance.UI.GetCameraRig() != null)
            {
                if (!Game.Instance.UI.GetCameraRig().NoClamp)
                    settings.toggleCameraBounds = Storage.isFalseString;
                else
                    settings.toggleCameraBounds = Storage.isTrueString;
            }


            if (GL.Button(
                MenuTools.TextWithTooltip("buttonToggle_DisableCameraBounds", "tooltip_DisableCameraBounds",
                    $"{settings.toggleCameraBounds}" + " ", ""), GL.ExpandWidth(false)))
            {
                if (settings.toggleCameraBounds == Storage.isFalseString)
                {
                    Game.Instance.UI.GetCameraRig().NoClamp = true;

                    settings.toggleCameraBounds = Storage.isTrueString;
                }
                else if (settings.toggleCameraBounds == Storage.isTrueString)
                {
                    Game.Instance.UI.GetCameraRig().NoClamp = false;

                    settings.toggleCameraBounds = Storage.isFalseString;
                }
            }

            GL.EndHorizontal();

            GL.Space(10);

            GL.BeginHorizontal();
            if (GL.Button(
                MenuTools.TextWithTooltip("buttonToggle_MoveCameraToPlayer", "tooltip_MoveCameraToPlayer", false),
                GL.ExpandWidth(false)))
                Game.Instance.UI.GetCameraRig().ScrollTo(GameHelper.GetPlayerCharacter().Position);
            GL.EndHorizontal();


            GL.BeginHorizontal();

            if (GL.Button(MenuTools.TextWithTooltip("button_ResetCutsceneLock", "tooltip_MoveCameraToPlayer", false),
                GL.ExpandWidth(false)))
            {
                Game.Instance.CheatResetCutsceneLock();
                Common.AddLogEntry(
                    Strings.GetText("button_ResetCutsceneLock") + ": " + Strings.GetText("logMessage_Enabled"),
                    Color.black);
            }

            MenuTools.ToggleButton(ref settings.toggleEnableResetCutsceneLockHotkey, "buttonToggle_EnableHotkey",
                "tooltip_EnableResetCutsceneLockHotkey");
            if (settings.toggleEnableResetCutsceneLockHotkey == Storage.isTrueString)
            {
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("button_ResetCutsceneLock") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.resetCutsceneLockKey);
                GL.EndHorizontal();
            }

            GL.EndHorizontal();
            MenuTools.SingleLineLabel(Strings.Parenthesis(Strings.GetText("label_ResetCutsceneLock")));
            GL.BeginHorizontal();

            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void RepeatableLockPickingOptions()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(
                MenuTools.TextWithTooltip("buttonToggle_RepeatableLockPicking", "tooltip_RepeatableLockPicking",
                    $"{settings.toggleRepeatableLockPicking}" + " ", ""), GL.ExpandWidth(false)))
            {
                if (settings.toggleRepeatableLockPicking == Storage.isFalseString)
                {
                    settings.toggleRepeatableLockPicking = Storage.isTrueString;
                }
                else if (settings.toggleRepeatableLockPicking == Storage.isTrueString)
                {
                    RepeatableLockPickingDependantsOff();
                    settings.toggleRepeatableLockPicking = Storage.isFalseString;
                }
            }

            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(RepeatableLockPickingOptions));
            MenuTools.ToggleButtonDependant(ref settings.toggleRepeatableLockPickingWeariness, ref settings.toggleRepeatableLockPicking, "buttonToggle_RepeatableLockPickingWeariness", "tooltip_RepeatableLockPickingWeariness");

            if (Strings.ToBool(settings.toggleRepeatableLockPickingWeariness))
            {
                GL.BeginVertical("box");
                GL.BeginHorizontal();
                GL.Label(
                    MenuTools.TextWithTooltip("label_WearinessPerAttempt", "tooltip_1EqualsHourWeariness", "", ": "),
                    GL.ExpandWidth(false));
                settings.repeatableLockPickingWeariness =
                    GL.TextField(settings.repeatableLockPickingWeariness, 6, GL.Width(100f));
                settings.repeatableLockPickingWeariness =
                    MenuTools.FloatTestSettingStage1(settings.repeatableLockPickingWeariness);
                settings.finalRepeatableLockPickingWeariness = MenuTools.FloatTestSettingStage2(
                    settings.repeatableLockPickingWeariness, settings.finalRepeatableLockPickingWeariness);
                GL.EndHorizontal();
                MenuTools.SingleLineLabel(MenuTools.TextWithTooltip("label_CurrentValue",
                    "tooltip_1EqualsHourWeariness", "", ": " + settings.finalRepeatableLockPickingWeariness));
                GL.EndVertical();
            }

            MenuTools.ToggleButtonDependant(ref settings.toggleRepeatableLockPickingLockPicks,
                ref settings.toggleRepeatableLockPicking, "buttonToggle_RepeatableLockPickingLockPicks",
                "tooltip_RepeatableLockPickingLockPicks");
            if (Strings.ToBool(settings.toggleRepeatableLockPickingLockPicks))
            {
                MenuTools.SingleLineLabel(Strings.GetText("label_LockPicks") + $": {Storage.lockPicks}");
                MenuTools.ToggleButtonDependant(ref settings.toggleRepeatableLockPickingLockPicksInventoryText,
                    ref settings.toggleRepeatableLockPickingLockPicks,
                    "buttonToggle_RepeatableLockPickingLockPicksInventoryText",
                    "tooltip_RepeatableLockPickingLockPicksInventoryText");
            }

            GL.EndVertical();
        }

        public static void RepeatableLockPickingDependantsOff()
        {
            settings.toggleRepeatableLockPickingWeariness = Storage.isFalseString;
            settings.toggleRepeatableLockPickingLockPicks = Storage.isFalseString;
        }

        public static void Misc()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showMiscCategory = GL.Toggle(settings.showMiscCategory, RichText.MainCategoryFormat(Strings.GetText("mainCategory_Misc")), GL.ExpandWidth(false));
            if (!settings.showMiscCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("Misc");

                GL.Space(10);

                RunPerceptionTrigger();

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleMakeSummmonsControllable, "buttonToggle_MakeSummonsControllable", "tooltip_MakeSummonsControllable", nameof(settings.toggleMakeSummmonsControllable));
                if (Strings.ToBool(settings.toggleMakeSummmonsControllable))
                {
                    MenuTools.ToggleButton(ref settings.toggleDisableWarpaintedSkullAbilityForSummonedBarbarians, "buttonToggle_DisableWarpaintedSkullAbilityForSummonedBarbarians", "tooltip_DisableWarpaintedSkullAbilityForSummonedBarbarians", nameof(settings.toggleDisableWarpaintedSkullAbilityForSummonedBarbarians));
                }

                GL.Space(10);

                SummonDurationMultiplier();

                GL.Space(10);

                SetSummonLevelTo20();

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleNoNegativeLevels, "buttonToggle_NoNegativeLevels",
                    "tooltip_NoNegativeLevels", nameof(settings.toggleNoNegativeLevels));

                GL.Space(10);

                WeatherControl();

                GL.Space(10);

                NoAttacksOfOpportunity();

                GL.Space(10);

                SetSpeedOnSummon();

                GL.Space(10);

                ExtraAttacksParty();

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleFreezeTimedQuestAt90Days,
                    "buttonToggle_FreezeTimedQuestAt90Days", "tooltip_FreezeTimedQuestAt90Days",
                    nameof(settings.toggleFreezeTimedQuestAt90Days));

                GL.Space(10);

                StopGameTime();

                GL.Space(10);

                TimeScale();

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleNoDamageFromEnemies, "buttonToggle_NoDamageFromEnemies",
                    "tooltip_NoDamageFromEnemies", nameof(settings.toggleNoDamageFromEnemies));
                MenuTools.ToggleButton(ref settings.togglePartyOneHitKills, "buttonToggle_PartyOneHitKills",
                    "tooltip_PartyOneHitKill", nameof(settings.togglePartyOneHitKills));

                MenuTools.ToggleButton(ref settings.toggleDamageDealtMultipliers, "buttonToggle_DamageDealtMultipliers",
                    "tooltip_DamageDealtMultipliers", nameof(settings.toggleDamageDealtMultipliers));

                if (Strings.ToBool(settings.toggleDamageDealtMultipliers))
                {
                    GL.Space(10);
                    GL.BeginVertical("box");
                    MenuTools.SettingsValueFloat(ref settings.damageDealtMultipliersValue,
                        ref settings.finalDamageDealtMultipliersValue);
                    GL.Space(10);

                    MenuTools.ToggleButton(ref settings.togglePartyDamageDealtMultiplier,
                        "buttonToggle_PartyDamageDealtMultiplier", "tooltip_PartyDamageDealtMultiplier",
                        nameof(settings.togglePartyDamageDealtMultiplier));

                    if (Strings.ToBool(settings.togglePartyDamageDealtMultiplier))
                    {
                        MenuTools.SingleLineLabel(Strings.GetText("label_CurrentMultiplier") +
                                                  $": {settings.partyDamageDealtMultiplier}");
                        GL.BeginHorizontal();
                        if (GL.Button(
                            Strings.GetText("button_SetTo") +
                            $" {Math.Round(settings.finalDamageDealtMultipliersValue, 3)}", GL.ExpandWidth(false)))
                            settings.partyDamageDealtMultiplier =
                                (float)Math.Round(settings.finalDamageDealtMultipliersValue, 3);
                        GL.EndHorizontal();
                    }

                    MenuTools.ToggleButton(ref settings.toggleEnemiesDamageDealtMultiplier,
                        "buttonToggle_EnemiesDamageDealtMultiplier", "tooltip_EnemiesDamageDealtMultiplier",
                        nameof(settings.toggleEnemiesDamageDealtMultiplier));
                    if (Strings.ToBool(settings.toggleEnemiesDamageDealtMultiplier))
                    {
                        MenuTools.SingleLineLabel(Strings.GetText("label_CurrentMultiplier") +
                                                  $": {settings.enemiesDamageDealtMultiplier}");
                        GL.BeginHorizontal();
                        if (GL.Button(
                            Strings.GetText("button_SetTo") +
                            $" {Math.Round(settings.finalDamageDealtMultipliersValue, 3)}", GL.ExpandWidth(false)))
                            settings.enemiesDamageDealtMultiplier =
                                (float)Math.Round(settings.finalDamageDealtMultipliersValue, 3);
                        GL.EndHorizontal();
                    }

                    GL.EndVertical();
                }

                GL.Space(10);
                MenuTools.ToggleButton(ref settings.toggleAllDoorContainersUnlocked,
                    "buttonToggle_AllDoorContainersUnlocked", "tooltip_AllDoorContainersUnlocked",
                    nameof(settings.toggleAllDoorContainersUnlocked));
                GL.Space(10);

                FogOfWar();

                if (SettingsRoot.Instance.SCCInsteadofCloodSprinkleRandomCritters.CurrentValue == true)
                {
                    GL.Space(10);
                    MenuTools.SingleLineLabel(Strings.GetText("headerOption_CrittersInsteadOfBloodChance"));
                    GL.BeginHorizontal();
                    GL.Label(Strings.GetText("headerOption_SettingsValue") + ": ", GL.ExpandWidth(false));
                    settings.sillyBloodChance = GL.HorizontalSlider(settings.sillyBloodChance, 0f, 1f, GL.Width(400f));
                    GL.Label($" {settings.sillyBloodChance:p0}", GL.ExpandWidth(false));
                    GL.EndHorizontal();
                    MenuTools.SingleLineLabel("(" + Strings.GetText("label_CurrentValue") +
                                              $": {Game.Instance.BlueprintRoot.Cheats.SillyBloodChance:p0})");
                    GL.BeginHorizontal();
                    if (GL.Button(Strings.GetText("button_Apply") + $"({settings.sillyBloodChance:p0})",
                        GL.ExpandWidth(false)))
                    {
                        settings.sillyBloodChanceSaved = settings.sillyBloodChance;
                        Game.Instance.BlueprintRoot.Cheats.SillyBloodChance = settings.sillyBloodChanceSaved;
                    }

                    if (GL.Button(Strings.GetText("button_Default"), GL.ExpandWidth(false)))
                        Game.Instance.BlueprintRoot.Cheats.SillyBloodChance = Defaults.sillyBloddChance;
                    GL.EndHorizontal();
                }

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleDialogRestrictions, "buttonToggle_IgnoreDialogueRestrictions",
                    "tooltip_IgnoreDialogueRestrictions", nameof(settings.toggleDialogRestrictions));

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleReverseCasterAlignmentChecks,
                    "buttonToggle_ReverseCasterAlignmentChecks", "tooltip_ReverseCasterAlignmentChecks",
                    nameof(settings.toggleReverseCasterAlignmentChecks));

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.togglePreventAlignmentChanges,
                    "buttonToggle_PreventAlignmentChanges", "tooltip_PreventAlignmentChanges",
                    nameof(settings.togglePreventAlignmentChanges));

                GL.Space(10);

                CreateGameHistoryLog();
                DumpKingdomState();

                GL.Space(10);

                FadeToBlack();

                GL.Space(10);

                ActiveSceneName();

                GL.Space(10);

                Achievements();

                GL.Space(10);

                RomanceCounterDisplay();
                if (settings.settingShowDebugInfo)
                {
                    MenuTools.SingleLineLabel(Strings.GetText("label_CurrentGameLocale") + ": " +
                                              LocalizationManager.CurrentLocale.ToString());
                    MenuTools.SingleLineLabel(Strings.GetText("label_CurrentGameID") + ": " +
                                              Game.Instance.Player.GameId);
                }

                GL.Space(10);

                PlayAnimations();

                GL.Space(10);

                PlaySounds();

                GL.Space(10);

                Experimental();
            }
            GL.EndVertical();
        }

        public static void InstantRest()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("button_InstantRest", "tooltip_InstantRest", false), GL.ExpandWidth(false)))
            {
                BagOfTricks.Cheats.InstantRest();
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(InstantRest));
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void RunPerceptionTrigger()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("button_RunPerceptionTriggerActions", "tooltip_RunPerceptionTriggerActions", false), GL.ExpandWidth(false)))
            {
                foreach (BlueprintComponent bc in Game.Instance.State.LoadedAreaState.Blueprint.CollectComponents())

                {
                    modLogger.Log(bc.name);
                    if (bc.name.Contains("PerceptionTrigger"))
                    {
                        PerceptionTrigger pt = (PerceptionTrigger)bc;
                        pt.OnSpotted.Run();
                    }
                }
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(RunPerceptionTrigger));
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void FogOfWar()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("buttonToggle_OverwriteFogOfWar", "tooltip_OverwriteFogOfWar", $"{settings.toggleOverwriteFogOfWar}" + " ", ""), GL.ExpandWidth(false)))
            {
                if (settings.toggleOverwriteFogOfWar == Storage.isFalseString)
                {
                    settings.toggleOverwriteFogOfWar = Storage.isTrueString;
                    if (LocationMaskRenderer.Instance.FogOfWar.Enabled)
                    {
                        settings.toggleFogOfWarFull = Storage.isTrueString;
                        settings.toggleFogOfWarBool = true;
                    }
                    else
                    {
                        settings.toggleFogOfWarFull = Storage.isFalseString;
                        settings.toggleFogOfWarBool = false;
                    }

                }
                else if (settings.toggleOverwriteFogOfWar == Storage.isTrueString)
                {
                    LocationMaskRenderer.Instance.FogOfWar.Enabled = Storage.toggleFogOfWarBoolDefault;
                    if (LocationMaskRenderer.Instance.FogOfWar.Enabled)
                    {
                        settings.toggleFogOfWarFull = Storage.isTrueString;
                        settings.toggleFogOfWarBool = true;
                    }
                    else
                    {
                        settings.toggleFogOfWarFull = Storage.isFalseString;
                        settings.toggleFogOfWarBool = false;
                    }

                    settings.toggleFogOfWarVisuals = Storage.isTrueString;
                    HarmonyInstance.Create("kingmaker.fogofwar").Patch(AccessTools.Method(typeof(FogOfWarRenderer), "Update"), new HarmonyMethod(typeof(BagOfTricks.HarmonyPatches).GetMethod("FogOfWarRenderer_Update_Patch")), null);
                    settings.toggleOverwriteFogOfWar = Storage.isFalseString;
                }
            }
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(FogOfWar));

            if (settings.toggleOverwriteFogOfWar == Storage.isTrueString)
            {
                GL.BeginHorizontal();
                if (GL.Button($"{settings.toggleFogOfWarFull} " + Strings.GetText("buttonToggle_FogOfWarFull"), GL.ExpandWidth(false)))
                {
                    if (settings.toggleFogOfWarFull == Storage.isTrueString)
                    {
                        settings.toggleFogOfWarFull = Storage.isFalseString;
                        settings.toggleFogOfWarBool = false;
                        LocationMaskRenderer.Instance.FogOfWar.Enabled = settings.toggleFogOfWarBool;
                    }
                    else if (settings.toggleFogOfWarFull == Storage.isFalseString)
                    {
                        settings.toggleFogOfWarFull = Storage.isTrueString;
                        settings.toggleFogOfWarBool = true;
                        LocationMaskRenderer.Instance.FogOfWar.Enabled = settings.toggleFogOfWarBool;
                    }
                }
                GL.EndHorizontal();

                if (settings.toggleFogOfWarFull == Storage.isTrueString)
                {
                    GL.BeginHorizontal();
                    if (GL.Button($"{settings.toggleFogOfWarVisuals} " + Strings.GetText("buttonToggle_FogOfWar"), GL.ExpandWidth(false)))
                    {
                        if (settings.toggleFogOfWarVisuals == Storage.isTrueString)
                        {
                            settings.toggleFogOfWarVisuals = Storage.isFalseString;
                            HarmonyInstance.Create("kingmaker.fogofwar").Patch(AccessTools.Method(typeof(FogOfWarRenderer), "Update"), new HarmonyMethod(typeof(BagOfTricks.HarmonyPatches).GetMethod("FogOfWarRenderer_Update_Patch")), null);
                        }
                        else if (settings.toggleFogOfWarVisuals == Storage.isFalseString)
                        {
                            settings.toggleFogOfWarVisuals = Storage.isTrueString;
                            HarmonyInstance.Create("kingmaker.fogofwar").Patch(AccessTools.Method(typeof(FogOfWarRenderer), "Update"), new HarmonyMethod(typeof(BagOfTricks.HarmonyPatches).GetMethod("FogOfWarRenderer_Update_Patch")), null);
                        }
                    }
                    GL.EndHorizontal();
                }
            }
            GL.EndVertical();
        }

        public static void TimeScale()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(new GUIContent(RichText.Bold(Strings.GetText("header_TimeScale")), Strings.GetText("tooltip_TimeScale")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(TimeScale));
            GL.BeginHorizontal();
            GL.Label(MenuTools.TextWithTooltip("header_Multiplier", "tooltip_Multiplier", "", " "), GL.ExpandWidth(false));
            settings.debugTimeMultiplier = GL.HorizontalSlider(settings.debugTimeMultiplier, 0.1f, 30f, GL.Width(300f));
            GL.Label($" {Math.Round(settings.debugTimeMultiplier, 1)}", GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            GL.Label(MenuTools.TextWithTooltip("header_UseCustomMultiplier", "tooltip_CustomMultiplier", "", " "), GL.ExpandWidth(false));
            settings.useCustomDebugTimeMultiplier = GL.Toggle(settings.useCustomDebugTimeMultiplier, new GUIContent("", Strings.GetText("tooltip_CustomMultiplier")), GL.ExpandWidth(false));
            GL.EndHorizontal();

            if (settings.useCustomDebugTimeMultiplier == true)
            {
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("header_CustomMultiplier") + " ", GL.ExpandWidth(false));
                settings.customDebugTimeMultiplier = GL.TextField(settings.customDebugTimeMultiplier, 6, GL.Width(50f));
                settings.customDebugTimeMultiplier = MenuTools.FloatTestSettingStage1(settings.customDebugTimeMultiplier);
                GL.EndHorizontal();

                settings.finalCustomDebugTimeMultiplier = MenuTools.FloatTestSettingStage2(settings.customDebugTimeMultiplier, settings.finalCustomDebugTimeMultiplier);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_CurrentMultiplier") + $": {settings.finalCustomDebugTimeMultiplier}", GL.Width(150f));
                GL.EndHorizontal();
            }
            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentValue") + $": {Math.Round(Game.Instance.TimeController.DebugTimeScale, 1)}");
            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_SetToDefault"), GL.ExpandWidth(false)))
            {
                settings.debugTimeMultiplier = Defaults.debugTimeScale;
                settings.customDebugTimeMultiplier = "1";
                settings.finalCustomDebugTimeMultiplier = Defaults.debugTimeScale;
                Game.Instance.TimeController.DebugTimeScale = Defaults.debugTimeScale;
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void StopGameTime()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.Bold(Strings.GetText("warning_StopGameTime")));
            MenuTools.FlexibleSpaceFavouriteButtonEndHorizontal(nameof(StopGameTime));
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("buttonToggle_StopGameTime", "tooltip_StopGameTime", $"{settings.toggleStopGameTime}" + " ", ""), GL.ExpandWidth(false)))
            {
                if (settings.toggleStopGameTime == Storage.isFalseString)
                {
                    settings.toggleStopGameTime = Storage.isTrueString;
                    HarmonyInstance.Create("kingmaker.stopgametime").Patch(AccessTools.Method(typeof(TimeController), "Tick"), new HarmonyMethod(typeof(BagOfTricks.HarmonyPatches).GetMethod("TimeController_Tick_Patch2")), null);
                }
                else if (settings.toggleStopGameTime == Storage.isTrueString)
                {
                    settings.toggleStopGameTime = Storage.isFalseString;
                    HarmonyInstance.Create("kingmaker.stopgametime").Patch(AccessTools.Method(typeof(TimeController), "Tick"), new HarmonyMethod(typeof(BagOfTricks.HarmonyPatches).GetMethod("TimeController_Tick_Patch2")), null);
                }
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void FadeToBlack()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button($"{Storage.fadeToBlackState} " + Strings.GetText("buttonToggle_FadeToBlack"), GL.ExpandWidth(false)))
            {
                if (Storage.fadeToBlackState == Strings.GetText("misc_Enable"))
                {
                    Game.Instance.UI.Fadeout(true);
                    Storage.fadeToBlackState = Strings.GetText("misc_Disable");
                }
                else if (Storage.fadeToBlackState == Strings.GetText("misc_Disable"))
                {
                    Game.Instance.UI.Fadeout(false);
                    Storage.fadeToBlackState = Strings.GetText("misc_Enable");
                }
                else
                {
                    Storage.fadeToBlackState = Strings.GetText("misc_Enable");
                }
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(FadeToBlack));
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void ActiveSceneName()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button($"{settings.currentSceneString} " + Strings.GetText("buttonToggle_ActiveSceneName"), GL.ExpandWidth(false)))
            {
                if (settings.currentSceneString == Strings.GetText("misc_Hide"))
                {
                    settings.currentSceneString = Strings.GetText("misc_Display");
                }
                else if (settings.currentSceneString == Strings.GetText("misc_Display"))
                {
                    settings.currentSceneString = Strings.GetText("misc_Hide");
                }
                else
                {
                    settings.currentSceneString = Strings.GetText("misc_Display");
                }
            }
            if (settings.currentSceneString == Strings.GetText("misc_Hide"))
            {
                string currenctSceneName = SceneManager.GetActiveScene().name;
                GL.Label(currenctSceneName);
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(ActiveSceneName));
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void Experimental()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showExperimentalCategory = GL.Toggle(settings.showExperimentalCategory, RichText.Bold(" " + Strings.GetText("header_Experimental")), GL.ExpandWidth(false));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(Experimental));
            GL.EndHorizontal();
            if (settings.showExperimentalCategory)
            {
                MenuTools.SingleLineLabel(RichText.WarningLargeRedFormat(Strings.GetText("warning_Experimental")));

                GL.BeginHorizontal();
                if (GL.Button(RichText.Bold($"{settings.toggleExperimentalIUnderstand} " + Strings.GetText("buttonToggle_experimentalIUnderstand")), GL.ExpandWidth(false)))
                {
                    if (settings.toggleExperimentalIUnderstand == Storage.isFalseString)
                    {
                        settings.toggleExperimentalIUnderstand = Storage.isTrueString;
                    }
                    else if (settings.toggleExperimentalIUnderstand == Storage.isTrueString)
                    {
                        settings.toggleExperimentalIUnderstand = Storage.isFalseString;
                    }
                }
                GL.EndHorizontal();

                if (settings.toggleExperimentalIUnderstand == Storage.isTrueString)
                {

                    GL.Space(10);

                    MenuTools.ToggleButton(ref settings.togglePreventQuestFailure, "buttonToggle_PreventQuestFailure", "tooltip_PreventQuestFailure", nameof(settings.togglePreventQuestFailure));

                    GL.Space(10);

                    Menu.AdvanceGameTime();

                    GL.Space(10);

                    Menu.DowngradeSettlement();

                    GL.Space(10);

                    ChangePartyMembers();

                    GL.Space(10);

                    ReloadArea();

                    GL.Space(10);

                    TeleportEveryoneToPlayer();

                    GL.Space(10);

                    RomanceCounterExperimental();

                    GL.Space(10);

                    FeatSelectionMultiplier();

                    GL.Space(10);

                    SorcererSupreme();

                    GL.Space(10);

                    FlyingHeight();

                    GL.Space(10);

                    GameConstants();

                    GL.Space(10);

                    MiscExtras();

                    if (settings.settingShowDebugInfo)
                    {

                        if (GL.Button("Spawn Spiders & Spider Swarms"))
                        {

                            Vector3 worldPosition = Game.Instance.ClickEventsController.WorldPosition;

                            List<BlueprintUnit> blueprintUnitList = new List<BlueprintUnit>();
                            foreach (string guid in Storage.spiderGuids)
                            {
                                blueprintUnitList.Add(Utilities.GetBlueprintByGuid<BlueprintUnit>(guid));
                            }

                            float numF = 0.0f;
                            foreach (BlueprintUnit unit in blueprintUnitList)
                            {
                                if (!((UnityEngine.Object)unit == (UnityEngine.Object)null))
                                {
                                    Game.Instance.EntityCreator.SpawnUnit(unit, new Vector3(worldPosition.x + 2f * numF, worldPosition.y, worldPosition.z + 2f * numF), Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                                    ++numF;
                                }
                            }
                        }


                        if (GL.Button("+10 Days"))
                        {
                            Game.Instance.AdvanceGameTime(TimeSpan.FromDays(10));
                        }
                        if (GL.Button("+1 Days"))
                        {
                            Game.Instance.AdvanceGameTime(TimeSpan.FromDays(1));
                        }

                        GL.Space(10);

                        foreach (KeyValuePair<string, StatType> entry in Storage.statsSkillsDict.Union(Storage.statsSocialSkillsDict).ToDictionary(d => d.Key, d => d.Value))
                        {
                            if (GL.Button($"Check {entry.Key} (Player)"))
                            {
                                RuleSkillCheck evt = new RuleSkillCheck(Game.Instance.Player.MainCharacter.Value, entry.Value, 20);
                                Rulebook.Trigger<RuleSkillCheck>(evt);
                            }
                        }
                        GL.Space(10);
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsSkillsDict.Union(Storage.statsSocialSkillsDict).ToDictionary(d => d.Key, d => d.Value))
                        {
                            if (GL.Button($"Check {entry.Key} (Party)"))
                            {
                                RulePartySkillCheck evt = new RulePartySkillCheck(entry.Value, 20);
                                Rulebook.Trigger<RulePartySkillCheck>(evt);
                            }
                        }

                        GL.Space(10);

                        if (GL.Button("Add Nyrissa Ray To Player"))
                        {
                            Game.Instance.Player.MainCharacter.Value.Descriptor.AddFact(Utilities.GetBlueprintByGuid<BlueprintUnitFact>("6a36a87c3d0094c46a9bef26afc3cb50"), (MechanicsContext)null, new FeatureParam());
                        }

                        if (GL.Button("Add Summon Natures Ally To Player"))
                        {
                            Game.Instance.Player.MainCharacter.Value.Descriptor.AddFact(Utilities.GetBlueprintByGuid<BlueprintUnitFact>("c6147854641924442a3bb736080cfeb6"), (MechanicsContext)null, new FeatureParam());
                        }

                        if (GL.Button("Fire Acid Arrow At Player"))
                        {
                            BlueprintAbility bp = Utilities.GetBlueprint<BlueprintAbility>("9a46dfd390f943647ab4395fc997936d");
                            UnitDescriptor caster = Game.Instance.Player.MainCharacter.Value.Descriptor;
                            AbilityData abilityData = new AbilityData(bp, caster);
                            AbilityParams abilityParams = new AbilityParams();
                            abilityParams.CasterLevel = 10;
                            abilityParams.SpellLevel = 10;
                            abilityParams.SpellSource = SpellSource.None;
                            TargetWrapper targetWrapper = GameHelper.GetPlayerCharacter();
                            AbilityExecutionContext context = new AbilityExecutionContext(abilityData, abilityParams, targetWrapper);
                            Game.Instance.AbilityExecutor.Execute(context);
                        }


                        if (GL.Button("Log Player CR"))
                        {
                            modLogger.Log(Game.Instance.Player.MainCharacter.Value.Blueprint.CR.ToString());
                        }


                        if (GUILayout.Button("Spawn Enemy With Random Tag At Player (CR10)"))

                        {
                            UnitTag reUnitTag = (UnitTag)UnityEngine.Random.Range(1, 136);

                            List<BlueprintUnit> units = OwlcatRESelector.SelectUnits(10, reUnitTag);

                            Game.Instance.EntityCreator.SpawnUnit(units[0], GameHelper.GetPlayerCharacter().Position, new Quaternion(), Game.Instance.Player.CrossSceneState);
                        }
                        if (GUILayout.Button("Spawn Enemy With Humanoid Tag At Player"))

                        {
                            UnitTag reUnitTag = (UnitTag)UnityEngine.Random.Range(1, 136);

                            List<BlueprintUnit> units = OwlcatRESelector.SelectUnits(10, reUnitTag);

                            Game.Instance.EntityCreator.SpawnUnit(units[0], GameHelper.GetPlayerCharacter().Position, new Quaternion(), Game.Instance.Player.CrossSceneState);
                        }


                        MenuTools.SingleLineLabel("<b>Start Random Encounter</b>");
                        GL.BeginHorizontal();
                        if (GL.Button($"{settings.toggleForcedEncounterIsHard} Hard Encounter", GL.ExpandWidth(false)))
                        {
                            if (settings.toggleForcedEncounterIsHard == Storage.isFalseString)
                            {
                                settings.toggleForcedEncounterIsHard = Storage.isTrueString;
                            }
                            else if (settings.toggleForcedEncounterIsHard == Storage.isTrueString)
                            {
                                settings.toggleForcedEncounterIsHard = Storage.isFalseString;
                            }
                        }
                        GL.EndHorizontal();
                        GL.BeginHorizontal();
                        settings.forcedEncounterCR = GL.TextField(settings.forcedEncounterCR, 10, GL.Width(90f));
                        settings.forcedEncounterCR = MenuTools.IntTestSettingStage1(settings.forcedEncounterCR);
                        settings.forcedEncounterFinalCR = MenuTools.IntTestSettingStage2(settings.forcedEncounterCR, settings.forcedEncounterFinalCR);
                        MenuTools.SingleLineLabel("Current CR: " + settings.forcedEncounterFinalCR);
                        GL.EndHorizontal();
                        GL.BeginHorizontal();
                        GL.Label("Avoidance: ", GL.ExpandWidth(false));
                        settings.forcedEncounterSelectedAvoidance = GL.SelectionGrid(settings.forcedEncounterSelectedAvoidance, new string[] { "Skill Test", "Succeed", "Fail", "Fail Critically" }, 4);
                        GL.EndHorizontal();
                        GL.BeginHorizontal();
                        settings.forcedEncounterSelectedBlueprintMode = GL.SelectionGrid(settings.forcedEncounterSelectedBlueprintMode, new string[] { "Random Combat Encounter", "Pick Blueprint (not implemented yet)" }, 2);
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        if (GL.Button("Start Random Encounter", GL.ExpandWidth(false)))
                        {
                            Storage.encounterError = "";
                            GameModeType currentMode = Game.Instance.CurrentMode;
                            if (currentMode == GameModeType.GlobalMap)
                            {
                                bool forcedEncounterIsHard;
                                bool forcedEncounterIsCamp = false;
                                if (settings.toggleForcedEncounterIsHard == Storage.isTrueString)
                                {
                                    forcedEncounterIsHard = true;
                                }
                                else
                                {
                                    forcedEncounterIsHard = false;
                                }
                                switch (settings.forcedEncounterSelectedBlueprintMode)
                                {
                                    case 0:
                                        List<string> blueprintRandomEncounter = Main.BlueprintsByTypes(new string[] { "BlueprintRandomEncounter" });
                                        if (!Storage.blueprintRandomCombatEncounterGuids.Any())
                                        {
                                            foreach (string s in blueprintRandomEncounter)
                                            {
                                                BlueprintRandomEncounter bre = Utilities.GetBlueprintByGuid<BlueprintRandomEncounter>(s);
                                                if (bre.name.Contains("Combat"))
                                                {
                                                    Storage.blueprintRandomCombatEncounterGuids.Add(s);
                                                    modLogger.Log(settings.forcedEncounterGuid + " | " + Utilities.GetBlueprintByGuid<BlueprintRandomEncounter>(settings.forcedEncounterGuid).name + " | " + settings.forcedEncounterFinalCR + " | " + settings.forcedEncounterPostion + " | " + forcedEncounterIsHard + " | " + forcedEncounterIsCamp);

                                                }
                                            }
                                        }

                                        System.Random random = new System.Random();
                                        int i = random.Next(0, blueprintRandomEncounter.Count);
                                        settings.forcedEncounterGuid = Storage.blueprintRandomCombatEncounterGuids[i];
                                        break;
                                    case 1:
                                        settings.forcedEncounterGuid = "3ec6e886801cb5c4fb6315163be3e0e1";
                                        break;
                                }
                                modLogger.Log("!!!" + settings.forcedEncounterGuid + " | " + Utilities.GetBlueprintByGuid<BlueprintRandomEncounter>(settings.forcedEncounterGuid).name + " | " + settings.forcedEncounterFinalCR + " | " + settings.forcedEncounterPostion + " | " + forcedEncounterIsHard + " | " + forcedEncounterIsCamp);
                                Main.StartEncounter(Utilities.GetBlueprintByGuid<BlueprintRandomEncounter>(settings.forcedEncounterGuid), settings.forcedEncounterFinalCR, settings.forcedEncounterPostion, forcedEncounterIsHard, forcedEncounterIsCamp);
                            }
                            else
                            {
                                Storage.encounterError = "Can only be done on the Global Map.";
                            }
                        }
                        GL.EndHorizontal();
                        if (Storage.encounterError != "")
                        {
                            MenuTools.SingleLineLabel(Storage.encounterError);
                        }
                    }

                }

            }
            GL.EndVertical();
        }

        public static void GameConstants()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.Bold(Strings.GetText("header_GameConstants")));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(GameConstants));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            settings.gameConstsAmount = GL.TextField(settings.gameConstsAmount, 10, GL.Width(85f));

            settings.gameConstsAmount = MenuTools.IntTestSettingStage1(settings.gameConstsAmount);
            settings.finalGameConstsAmount = MenuTools.IntTestSettingStage2(settings.gameConstsAmount, settings.finalGameConstsAmount);
            GL.EndHorizontal();

            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_MinimalUnitSpeed"), GL.ExpandWidth(false)))
            {
                FieldInfo minUnitSpeedMps = typeof(GameConsts).GetField("MinUnitSpeedMps");
                minUnitSpeedMps.SetValue(null, settings.finalGameConstsAmount);
                settings.gameConstsMinUnitSpeedMps = settings.finalGameConstsAmount;
            }
            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentMinimalUnitSpeed") + $": {typeof(GameConsts).GetField("MinUnitSpeedMps").GetValue(null)}");
            GL.EndHorizontal();

            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_MinimalWeaponRange"), GL.ExpandWidth(false)))
            {
                FieldInfo minWeaponRange = typeof(GameConsts).GetField("MinWeaponRange");
                minWeaponRange.SetValue(null, settings.finalGameConstsAmount.Feet());
                settings.gameConstsMinWeaponRange = settings.finalGameConstsAmount;
            }
            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentMinimalWeaponRange") + $": {typeof(GameConsts).GetField("MinWeaponRange").GetValue(null)}");
            GL.EndHorizontal();

            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_StealthDCIncrement"), GL.ExpandWidth(false)))
            {
                FieldInfo stealthDCIncrement = typeof(GameConsts).GetField("StealthDCIncrement");
                stealthDCIncrement.SetValue(null, settings.finalGameConstsAmount.Feet());
                settings.gameConstsMinWeaponRange = settings.finalGameConstsAmount;
            }
            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentStealthDCIncrement") + $": {typeof(GameConsts).GetField("StealthDCIncrement").GetValue(null)}");
            GL.EndHorizontal();

            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_ResetToDefault"), GL.ExpandWidth(false)))
            {
                FieldInfo minUnitSpeedMps = typeof(GameConsts).GetField("MinUnitSpeedMps");
                minUnitSpeedMps.SetValue(null, 5.Feet().Meters / 3f);
                settings.gameConstsMinUnitSpeedMps = -1;

                FieldInfo minWeaponRange = typeof(GameConsts).GetField("MinWeaponRange");
                minWeaponRange.SetValue(null, 2.Feet());
                settings.gameConstsMinWeaponRange = -1;

                FieldInfo stealthDCIncrement = typeof(GameConsts).GetField("StealthDCIncrement");
                stealthDCIncrement.SetValue(null, 10.Feet());
                settings.gameConstsMinWeaponRange = -1;
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void FlyingHeight()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.Bold(Strings.GetText("headerOption_FlyingHeight")));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(FlyingHeight));
            GL.EndHorizontal();

            settings.flyingHeight = GL.TextField(settings.flyingHeight, 10, GL.Width(90f));

            settings.flyingHeight = MenuTools.FloatTestSettingStage1(settings.flyingHeight);
            settings.finalflyingHeight = MenuTools.FloatTestSettingStage2(settings.flyingHeight, settings.finalflyingHeight);

            List<UnitEntityData> partyMembers = Game.Instance.Player.ControllableCharacters;

            foreach (UnitEntityData controllableCharacter in partyMembers)
            {
                GL.BeginHorizontal();
                MenuTools.SingleLineLabel(Strings.GetText("label_CurrentFlyingHeight") + $" ({controllableCharacter.CharacterName}): {controllableCharacter.FlyHeight}");
                if (GL.Button($"+{settings.flyingHeight}", GL.Width(160f)))
                {
                    controllableCharacter.FlyHeight += settings.finalflyingHeight;
                }
                if (GL.Button(Strings.GetText("button_SetTo0"), GL.Width(160f)))
                {
                    controllableCharacter.FlyHeight = 0f;
                }
                if (GL.Button($"-{settings.flyingHeight}", GL.Width(160f)))
                {
                    controllableCharacter.FlyHeight -= settings.finalflyingHeight;
                }
                GL.EndHorizontal();
            }

            GL.BeginHorizontal();
            settings.flyingHeightUseSlider = GL.Toggle(settings.flyingHeightUseSlider, "", GL.ExpandWidth(false));
            GL.Label(Strings.GetText("headerOption_UseSlider"), GL.ExpandWidth(false));

            Storage.flyingHeightSlider = GL.HorizontalSlider(Storage.flyingHeightSlider, -10f, 100f);
            if (settings.flyingHeightUseSlider)
            {
                foreach (UnitEntityData controllableCharacter in partyMembers)
                {
                    controllableCharacter.FlyHeight = Storage.flyingHeightSlider;
                }
            }
            GL.EndHorizontal();
            GL.BeginHorizontal();

            if (GL.Button(Strings.GetText("button_SetAllTo0"), GL.Width(160f)))
            {
                foreach (UnitEntityData controllableCharacter in partyMembers)
                {
                    Storage.flyingHeightSlider = 0f;
                    controllableCharacter.FlyHeight = 0f;
                }
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void SorcererSupreme()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(RichText.Bold(Strings.GetText("button_SorcererSupreme")), GL.ExpandWidth(false)))
            {
                Main.HyperTheurgeHelper.SorcererSupreme2();
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(SorcererSupreme));
            GL.EndHorizontal();
            MenuTools.SingleLineLabel(Strings.GetText("warning_SorcererSupreme_0"));
            MenuTools.SingleLineLabel(Strings.GetText("warning_SorcererSupreme_1"));
            GL.BeginHorizontal();
            if (GL.Button(RichText.Bold($"{settings.toggleSkipSpellSelection} " + Strings.GetText("buttonToggle_SkipSpellSelection")), GL.ExpandWidth(false)))
            {
                if (settings.toggleSkipSpellSelection == Storage.isFalseString)
                {
                    settings.toggleSkipSpellSelection = Storage.isTrueString;
                }
                else if (settings.toggleSkipSpellSelection == Storage.isTrueString)
                {
                    settings.toggleSkipSpellSelection = Storage.isFalseString;
                }
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void FeatSelectionMultiplier()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.Bold(Strings.GetText("button_FeatSelectionMultiplier") + " "), GL.ExpandWidth(false));
            settings.featMultiplierString = GL.TextField(settings.featMultiplierString, 10, GL.Width(90f));
            MenuTools.SettingParse(ref settings.featMultiplierString, ref settings.featMultiplier);
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(FeatSelectionMultiplier));
            GL.EndHorizontal();
            MenuTools.SingleLineLabel(Strings.GetText("warning_FeatSelectionMultiplier_0"));
            MenuTools.SingleLineLabel(Strings.GetText("warning_FeatSelectionMultiplier_1"));
            MenuTools.SingleLineLabel(Strings.GetText("warning_FeatSelectionMultiplier_2"));
            GL.EndVertical();
        }

        public static void TeleportEveryoneToPlayer()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(RichText.Bold(Strings.GetText("button_TeleportEveryoneToPlayer")), GL.ExpandWidth(false)))
            {
                Common.TeleportEveryoneToPlayer();
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(TeleportEveryoneToPlayer));
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void ReloadArea()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(RichText.Bold(Strings.GetText("button_ReloadArea")), GL.ExpandWidth(false)))
            {
                Game.Instance.ReloadArea();
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(ReloadArea));
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void ChangePartyMembers()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(RichText.Bold(Strings.GetText("button_ChangePartyMembers")), GL.ExpandWidth(false)))
            {
                GameModeType currentMode = Game.Instance.CurrentMode;

                if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
                {
                    UnityModManager.UI.Instance.ToggleWindow();

                    Game.Instance.UI.Canvas.GroupManager.OpenPartyWithCallback(null, true);
                }
            }

            if (GL.Button(RichText.Bold(Strings.GetText("button_PostPartyChange")), GL.ExpandWidth(false)))
            {
                Common.PostPartyChange();
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(ChangePartyMembers));
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void PlaySounds()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showSoundsCategory = GL.Toggle(settings.showSoundsCategory, RichText.Bold(" " + Strings.GetText("header_Sounds")), GL.ExpandWidth(false));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(PlaySounds));
            GL.EndHorizontal();
            if (settings.showSoundsCategory == true)
            {
                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_UISoundTypes")));
                foreach (UISoundType sound in (UISoundType[])Enum.GetValues(typeof(UISoundType)))
                {
                    GL.BeginHorizontal();
                    if (GL.Button(Strings.GetText("misc_Play") + " " + sound.ToString(), GL.ExpandWidth(false)))
                    {
                        Game.Instance.UI.Common.UISound.Play(sound);
                    }
                    GL.EndHorizontal();
                }
            }
            GL.EndVertical();
        }

        public static void CreateGameHistoryLog()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_CreateGameHistoryLog"), GL.ExpandWidth(false)))
            {
                File.WriteAllLines(Path.Combine(Common.ExportPath(), "game-history.txt"), (IEnumerable<string>)Game.Instance.Statistic.GameHistoryLog);
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(CreateGameHistoryLog));
            GL.EndHorizontal();
            MenuTools.SingleLineLabel(" " + Strings.GetText("label_Location") + $": {Path.Combine(Common.ExportPath(), "game-history.txt")}");
            GL.EndVertical();
        }

        public static void DumpKingdomState()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(Strings.GetText("button_DumpKingdomState"), GL.ExpandWidth(false)))
            {
                File.WriteAllText(Path.Combine(Common.ExportPath(), "kingdom-state.txt"), Utilities.DumpKingdomState());
            }
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(DumpKingdomState));
            GL.EndHorizontal();
            MenuTools.SingleLineLabel(" " + Strings.GetText("label_Location") + $": {Path.Combine(Common.ExportPath(), "kingdom-state.txt")}");
            GL.EndVertical();
        }

        public static void RomanceCounterExperimental()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showRomanceCountersExperimental = GL.Toggle(settings.showRomanceCountersExperimental, RichText.Bold($"{Strings.GetText("header_RomanceCounters")} ({Strings.GetText("header_Experimental")})"), GL.ExpandWidth(false));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(RomanceCounterExperimental));
            GL.EndHorizontal();
            if (settings.showRomanceCountersExperimental)
            {
                GL.Space(10);

                GL.BeginHorizontal();
                settings.showRomanceCountersSpoilers = GL.Toggle(settings.showRomanceCountersSpoilers, Strings.GetText("label_ShowSpoilers"), GL.ExpandWidth(false));
                GL.EndHorizontal();

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("headerOption_SettingsValue") + ": ", GL.ExpandWidth(false));
                settings.romanceCounterSetValue = GL.TextField(settings.romanceCounterSetValue, 6, GL.Width(100f));
                settings.romanceCounterSetValue = MenuTools.FloatTestSettingStage1(settings.romanceCounterSetValue);
                settings.finalRomanceCounterSetValue = MenuTools.FloatTestSettingStage2(settings.romanceCounterSetValue, settings.finalRomanceCounterSetValue);
                GL.EndHorizontal();

                if (Storage.romanceCounterLoadExperimental)
                {
                    Storage.blueprintRomanceCounters = Resources.FindObjectsOfTypeAll<BlueprintRomanceCounter>();
                    Storage.romanceCounterLoadExperimental = false;
                }

                foreach (BlueprintRomanceCounter bpc in Storage.blueprintRomanceCounters)
                {
                    if (!settings.showRomanceCountersSpoilers)
                    {
                        if (bpc.AssetGuid != "7345208aba59cf74ca1e26091e1446d0")
                        {
                            MenuTools.SingleLineLabel(bpc.name.Replace("RomanceCounter", "") + " " + Strings.GetText("label_RomanceCounter"));
                            if (settings.settingShowDebugInfo)
                            {
                                MenuTools.SingleLineLabel(bpc.AssetGuid);
                            }

                            GL.BeginHorizontal();
                            GL.Label(Strings.GetText("label_CounterValue") + ": " + bpc.CounterFlag.Value.ToString());
                            GL.FlexibleSpace();
                            if (GL.Button("<b>+</b>", GL.ExpandWidth(false)))
                            {
                                bpc.CounterFlag.Value++;
                                Storage.romanceCounterLoadExperimental = true;
                            }
                            if (GL.Button("<b>-</b>", GL.ExpandWidth(false)))
                            {
                                bpc.CounterFlag.Value--;
                                Storage.romanceCounterLoadExperimental = true;
                            }
                            if (GL.Button(Strings.GetText("button_SetTo") + $" {Mathf.RoundToInt(settings.finalRomanceCounterSetValue)}", GL.ExpandWidth(false)))
                            {
                                bpc.CounterFlag.Value = Mathf.RoundToInt(settings.finalRomanceCounterSetValue);
                                Storage.romanceCounterLoadExperimental = true;
                            }
                            GL.EndHorizontal();

                            GL.BeginHorizontal();
                            GL.Label(Strings.GetText("label_LowerCutoff") + ": " + bpc.MinValueFlag.Value.ToString());
                            GL.FlexibleSpace();
                            if (GL.Button("<b>+</b>", GL.ExpandWidth(false)))
                            {
                                bpc.MinValueFlag.Value++;
                                Storage.romanceCounterLoadExperimental = true;
                            }
                            if (GL.Button("<b>-</b>", GL.ExpandWidth(false)))
                            {
                                bpc.MinValueFlag.Value--;
                                Storage.romanceCounterLoadExperimental = true;
                            }
                            if (GL.Button(Strings.GetText("button_SetTo") + $" {Mathf.RoundToInt(settings.finalRomanceCounterSetValue)}", GL.ExpandWidth(false)))
                            {
                                bpc.MinValueFlag.Value = Mathf.RoundToInt(settings.finalRomanceCounterSetValue);
                                Storage.romanceCounterLoadExperimental = true;
                            }
                            GL.EndHorizontal();

                            GL.BeginHorizontal();
                            GL.Label(Strings.GetText("label_UpperCutoff") + ": " + bpc.MaxValueFlag.Value.ToString());
                            GL.FlexibleSpace();
                            if (GL.Button("<b>+</b>", GL.ExpandWidth(false)))
                            {
                                bpc.MaxValueFlag.Value++;
                                Storage.romanceCounterLoadExperimental = true;
                            }
                            if (GL.Button("<b>-</b>", GL.ExpandWidth(false)))
                            {
                                bpc.MaxValueFlag.Value--;
                                Storage.romanceCounterLoadExperimental = true;
                            }
                            if (GL.Button(Strings.GetText("button_SetTo") + $" {Mathf.RoundToInt(settings.finalRomanceCounterSetValue)}", GL.ExpandWidth(false)))
                            {
                                bpc.MaxValueFlag.Value = Mathf.RoundToInt(settings.finalRomanceCounterSetValue);
                                Storage.romanceCounterLoadExperimental = true;
                            }
                            GL.EndHorizontal();

                            GL.Space(10);
                        }
                    }
                    else
                    {
                        MenuTools.SingleLineLabel(bpc.name.Replace("RomanceCounter", ""));
                        if (settings.settingShowDebugInfo)
                        {
                            MenuTools.SingleLineLabel(bpc.AssetGuid);
                        }
                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_CounterValue") + ": " + bpc.CounterFlag.Value.ToString());
                        GL.FlexibleSpace();
                        if (GL.Button("<b>+</b>", GL.ExpandWidth(false)))
                        {
                            bpc.CounterFlag.Value++;
                            Storage.romanceCounterLoadExperimental = true;
                        }
                        if (GL.Button("<b>-</b>", GL.ExpandWidth(false)))
                        {
                            bpc.CounterFlag.Value--;
                            Storage.romanceCounterLoadExperimental = true;
                        }
                        if (GL.Button(Strings.GetText("button_SetTo") + $" {Mathf.RoundToInt(settings.finalRomanceCounterSetValue)}", GL.ExpandWidth(false)))
                        {
                            bpc.CounterFlag.Value = Mathf.RoundToInt(settings.finalRomanceCounterSetValue);
                            Storage.romanceCounterLoadExperimental = true;
                        }
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_LowerCutoff") + ": " + bpc.MinValueFlag.Value.ToString());
                        GL.FlexibleSpace();
                        if (GL.Button("<b>+</b>", GL.ExpandWidth(false)))
                        {
                            bpc.MinValueFlag.Value++;
                            Storage.romanceCounterLoadExperimental = true;
                        }
                        if (GL.Button("<b>-</b>", GL.ExpandWidth(false)))
                        {
                            bpc.MinValueFlag.Value--;
                            Storage.romanceCounterLoadExperimental = true;
                        }
                        if (GL.Button(Strings.GetText("button_SetTo") + $" {Mathf.RoundToInt(settings.finalRomanceCounterSetValue)}", GL.ExpandWidth(false)))
                        {
                            bpc.MinValueFlag.Value = Mathf.RoundToInt(settings.finalRomanceCounterSetValue);
                            Storage.romanceCounterLoadExperimental = true;
                        }
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_UpperCutoff") + ": " + bpc.MaxValueFlag.Value.ToString());
                        GL.FlexibleSpace();
                        if (GL.Button("<b>+</b>", GL.ExpandWidth(false)))
                        {
                            bpc.MaxValueFlag.Value++;
                            Storage.romanceCounterLoadExperimental = true;
                        }
                        if (GL.Button("<b>-</b>", GL.ExpandWidth(false)))
                        {
                            bpc.MaxValueFlag.Value--;
                            Storage.romanceCounterLoadExperimental = true;
                        }
                        if (GL.Button(Strings.GetText("button_SetTo") + $" {Mathf.RoundToInt(settings.finalRomanceCounterSetValue)}", GL.ExpandWidth(false)))
                        {
                            bpc.MaxValueFlag.Value = Mathf.RoundToInt(settings.finalRomanceCounterSetValue);
                            Storage.romanceCounterLoadExperimental = true;
                        }
                        GL.EndHorizontal();

                        GL.Space(10);
                    }
                }
            }
            else
            {
                Storage.romanceCounterLoadExperimental = true;
            }
            GL.EndVertical();
        }

        public static void RomanceCounterDisplay()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showRomanceCounters = GL.Toggle(settings.showRomanceCounters, RichText.Bold(Strings.GetText("header_RomanceCounters")), GL.ExpandWidth(false));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(RomanceCounterDisplay));
            GL.EndHorizontal();
            if (settings.showRomanceCounters)
            {
                GL.Space(10);
                MenuTools.SingleLineLabel(Strings.GetText("label_EditingInExperimental"));
                GL.Space(10);

                GL.BeginHorizontal();
                settings.showRomanceCountersSpoilers = GL.Toggle(settings.showRomanceCountersSpoilers, Strings.GetText("label_ShowSpoilers"), GL.ExpandWidth(false));
                GL.EndHorizontal();

                GL.Space(10);

                if (Storage.romanceCounterLoad)
                {
                    Storage.blueprintRomanceCounters = Resources.FindObjectsOfTypeAll<BlueprintRomanceCounter>();
                    Storage.romanceCounterLoad = false;
                }


                foreach (BlueprintRomanceCounter bpc in Storage.blueprintRomanceCounters)
                {
                    if (!settings.showRomanceCountersSpoilers)
                    {
                        if (bpc.AssetGuid != "7345208aba59cf74ca1e26091e1446d0")
                        {
                            MenuTools.SingleLineLabel(bpc.name.Replace("RomanceCounter", "") + " " + Strings.GetText("label_RomanceCounter"));
                            if (settings.settingShowDebugInfo)
                            {
                                MenuTools.SingleLineLabel(bpc.AssetGuid);
                            }
                            MenuTools.SingleLineLabel(Strings.GetText("label_CounterValue") + ": " + bpc.CounterFlag.Value.ToString());
                            MenuTools.SingleLineLabel(Strings.GetText("label_LowerCutoff") + ": " + bpc.MinValueFlag.Value.ToString());
                            MenuTools.SingleLineLabel(Strings.GetText("label_UpperCutoff") + ": " + bpc.MaxValueFlag.Value.ToString());
                            GL.Space(10);
                        }
                    }
                    else
                    {
                        MenuTools.SingleLineLabel(bpc.name.Replace("RomanceCounter", ""));
                        if (settings.settingShowDebugInfo)
                        {
                            MenuTools.SingleLineLabel(bpc.AssetGuid);
                        }
                        MenuTools.SingleLineLabel(bpc.CounterFlag.ToString() + ": " + bpc.CounterFlag.Value.ToString());
                        MenuTools.SingleLineLabel(bpc.MinValueFlag.ToString() + ": " + bpc.MinValueFlag.Value.ToString());
                        MenuTools.SingleLineLabel(bpc.MaxValueFlag.ToString() + ": " + bpc.MaxValueFlag.Value.ToString());
                        GL.Space(10);
                    }
                }
            }
            else
            {
                Storage.romanceCounterLoad = true;
            }
            GL.EndVertical();
        }

        public static SelectionGrid weatherControlGrid = new SelectionGrid(Storage.weatherArray, 3);

        public static void WeatherControl()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.Bold(" " + Strings.GetText("label_Weather")), GL.ExpandWidth(false));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(WeatherControl));
            GL.EndHorizontal();

            weatherControlGrid.Render();

            GL.Space(10);

            GL.BeginHorizontal();
            GL.Label(Strings.GetText("label_Intensity") + ": ", GL.ExpandWidth(false));
            Storage.weatherIntensity = GL.HorizontalSlider(Storage.weatherIntensity, 0f, 1f, GL.Width(400f));
            GL.Label($" {Math.Round(Storage.weatherIntensity, 3)}", GL.ExpandWidth(false));
            GL.EndHorizontal();


            GL.Space(10);

            if (GL.Button(Strings.GetText("button_SetTo") + $" {Storage.weatherArray[weatherControlGrid.selected]}",
                GL.ExpandWidth(false)))
            {
                WeatherSystemBehaviour.Instance.RainIntensity = Storage.weatherIntensity;
                WeatherSystemBehaviour.Instance.SnowIntensity = Storage.weatherIntensity;
                WeatherSystemBehaviour.Instance.WeatherType = (WeatherType)weatherControlGrid.selected;
                WeatherSystemBehaviour.Instance.Reset();
                EventBus.RaiseEvent<IWeatherUpdateHandler>(
                    (Action<IWeatherUpdateHandler>)(h => h.OnUpdateWeatherSystem(true)));
            }

            GL.EndVertical();
        }

        public static void Settings()
        {
            GL.BeginVertical("box");

            KeyBindings();

            GL.Space(10);

            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showLanguageMenu = GL.Toggle(settings.showLanguageMenu, RichText.Bold(" " + Strings.GetText("header_Language")), GL.ExpandWidth(false));
            GL.EndHorizontal();
            if (settings.showLanguageMenu == true)
            {
                GL.Space(10);

                GL.BeginHorizontal();
                if (GL.Button(Strings.GetText("button_LoadRefresh"), GL.ExpandWidth(false)))
                {
                    try
                    {
                        Storage.localisationsXMLFiles.Clear();
                        Storage.localisationsXML =
                            Directory.GetFiles(Storage.modEntryPath + Storage.localisationFolder, "*.xml");
                        foreach (var s in Storage.localisationsXML)
                            Storage.localisationsXMLFiles.Add(Path.GetFileNameWithoutExtension(s));

                        Main.SortLocalisationsAndFlags();
                    }
                    catch (IOException exception)
                    {
                        modLogger.Log(exception.ToString());
                    }

                    if (Storage.localisationsXMLFiles.Count - 1 < settings.selectedLocalisation)
                        settings.selectedLocalisation = 0;
                    if (Storage.localisationsXMLFiles.Contains(settings.selectedLocalisationName))
                        settings.selectedLocalisation =
                            Storage.localisationsXMLFiles.FindIndex(a => a.Contains(settings.selectedLocalisationName));
                }

                GL.EndHorizontal();
                GL.Space(10);
                GL.BeginHorizontal();
                settings.selectedLocalisation = GL.SelectionGrid(settings.selectedLocalisation,
                    Storage.localeGrid.ToArray(), 1, GL.ExpandWidth(false));
                GL.EndHorizontal();
                if (settings.selectedLocalisation != Storage.selectedLocalisationOld)
                {
                    Storage.selectedLocalisationOld = settings.selectedLocalisation;
                    settings.selectedLocalisationName =
                        Path.GetFileNameWithoutExtension(
                            Storage.localisationsXMLFiles.ToArray()[settings.selectedLocalisation]);
                    if (File.Exists(Storage.modEntryPath + Storage.localisationFolder + "\\" +
                                    settings.selectedLocalisationName + ".xml"))
                    {
                        Strings.temp = Strings.XMLtoDict(settings.selectedLocalisationName);
                        if (Strings.temp.Count == MenuText.fallback.Count &&
                            Strings.temp.Keys == MenuText.fallback.Keys)
                            Strings.current = Strings.temp;
                        else
                            Strings.current = Strings.temp.Union(MenuText.fallback).GroupBy(kvp => kvp.Key)
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
                        Strings.RefreshStrings();
                        Storage.mainToolbarStrings = new string[]
                        {
                            RichText.MainCategoryFormat(Strings.GetText("mainCategory_FavouriteFunctions")),
                            RichText.MainCategoryFormat(Strings.GetText("mainCategory_Cheats")),
                            RichText.MainCategoryFormat(Strings.GetText("mainCategory_Mods")),
                            RichText.MainCategoryFormat(Strings.GetText("mainCategory_Tools")),
                            RichText.MainCategoryFormat(Strings.GetText("mainCategory_Settings"))
                        };
                        settings.filterButtonText = Strings.GetText("misc_Enable");
                        settings.currentSceneString = Strings.GetText("misc_Display");
                        settings.showAllBuffs = Strings.GetText("misc_Display");
                        settings.showAllFeats = Strings.GetText("misc_Display");
                        settings.showAllAbilities = Strings.GetText("misc_Display");
                        if (settings.selectedLocalisation != Storage.selectedLocalisationOld)
                        {
                            Storage.selectedLocalisationOld = settings.selectedLocalisation;
                            settings.selectedLocalisationName =
                                Path.GetFileNameWithoutExtension(
                                    Storage.localisationsXMLFiles.ToArray()[settings.selectedLocalisation]);
                            if (File.Exists(Storage.modEntryPath + Storage.localisationFolder + "\\" +
                                            settings.selectedLocalisationName + ".xml"))
                            {
                                Strings.temp = Strings.XMLtoDict(settings.selectedLocalisationName);
                                if (Strings.temp.Count == MenuText.fallback.Count &&
                                    Strings.temp.Keys == MenuText.fallback.Keys)
                                    Strings.current = Strings.temp;
                                else
                                    Strings.current = Strings.temp.Union(MenuText.fallback).GroupBy(kvp => kvp.Key)
                                        .ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
                                Strings.RefreshStrings();
                                Storage.mainToolbarStrings = new string[]
                                {
                                    RichText.MainCategoryFormat(Strings.GetText("mainCategory_FavouriteFunctions")),
                                    RichText.MainCategoryFormat(Strings.GetText("mainCategory_Cheats")),
                                    RichText.MainCategoryFormat(Strings.GetText("mainCategory_Mods")),
                                    RichText.MainCategoryFormat(Strings.GetText("mainCategory_Tools")),
                                    RichText.MainCategoryFormat(Strings.GetText("mainCategory_Settings"))
                                };
                                settings.filterButtonText = Strings.GetText("misc_Enable");
                                settings.currentSceneString = Strings.GetText("misc_Display");
                                settings.showAllBuffs = Strings.GetText("misc_Display");
                                settings.showAllFeats = Strings.GetText("misc_Display");
                                settings.showAllAbilities = Strings.GetText("misc_Display");
                            }
                        }
                    }
                }
            }

            GL.EndVertical();

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleShowTooltips, "buttonToggle_ShowTooltips", "tooltip_ShowTooltips",
                nameof(settings.toggleShowTooltips));

            MenuTools.ToggleButton(ref settings.toggleAddToLog, "buttonToggle_LogMessages", "tooltip_LogMessages",
                nameof(settings.toggleAddToLog));

            MenuTools.ToggleButton(ref settings.toggleExportToModFolder, "buttonToggle_ExportToModFolder",
                "tooltip_ExportToModFolder", nameof(settings.toggleExportToModFolder));

            //MenuTools.ToggleButton(ref settings.toggleBoTScrollBar, "buttonToggle_BoTScrollbar", "tooltip_BoTScrollbar", nameof(settings.toggleBoTScrollBar));

            GL.BeginHorizontal();
            GL.Label(Strings.GetText("headerOption_SearchResultLimit") + ": ", GL.ExpandWidth(false));
            settings.resultLimit = GL.TextField(settings.resultLimit, GL.Width(150f));
            settings.resultLimit = MenuTools.IntTestSettingStage1(settings.resultLimit);
            settings.finalResultLimit = MenuTools.IntTestSettingStage2(settings.resultLimit, settings.finalResultLimit);
            GL.EndHorizontal();

            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentSearchResultLimit") +
                                      $": {settings.finalResultLimit}");

            GL.BeginHorizontal();
            settings.settingSearchForCsv = GL.Toggle(settings.settingSearchForCsv, RichText.Bold(Strings.GetText("toggle_SearchCsvForCustomSets")), GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            settings.settingSearchForTxt = GL.Toggle(settings.settingSearchForTxt, RichText.Bold(Strings.GetText("toggle_SearchTxtForCustomSets")), GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            settings.settingKeepGuids = GL.Toggle(settings.settingKeepGuids, RichText.Bold(Strings.GetText("toggle_RememberLastEnteredGuids")), GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            settings.settingKeepCategories = GL.Toggle(settings.settingKeepCategories, RichText.Bold(Strings.GetText("toggle_RememberOpenCategories")), GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            settings.settingKeepSubMenus = GL.Toggle(settings.settingKeepSubMenus, RichText.Bold(Strings.GetText("toggle_RememberOpenSubMenus")), GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            settings.settingCreateBackupBeforeImport = GL.Toggle(settings.settingCreateBackupBeforeImport, RichText.Bold(Strings.GetText("toggle_CreateCharStatsBackup")), GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            settings.settingShowDebugInfo = GL.Toggle(settings.settingShowDebugInfo, RichText.Bold(Strings.GetText("toggle_ShowDebugInfo")), GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("button_CreateFallbackXML", "tooltip_CreateFallbackXML", false),
                GL.ExpandWidth(false))) Main.SerializeFallback();
            GL.EndHorizontal();

            About();

            GL.EndVertical();
        }

        private static bool showAboutSection = false;

        public static void About()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            showAboutSection = GL.Toggle(showAboutSection, RichText.Bold(" " + Strings.GetText("header_About")), GL.ExpandWidth(false));
            GL.EndHorizontal();
            if (showAboutSection)
            {
                GL.Space(10);
                MenuTools.SingleLineLabel($"{Strings.GetText("contributors_text")}: {Strings.GetText("contributors")}");
                GL.Space(10);

                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("about_KingmakerModsPW") + ":"));
                MenuTools.SingleLineLabel(
                    "\"The MIT License(MIT)\n\nCopyright(c) 2018 fireundubh < fireundubh@gmail.com >\nPermission is hereby granted, free of charge, to any person obtaining a copy of\nthis software and associated documentation files(the \"Software\"), to deal in\nthe Software without restriction, including without limitation the rights to\nuse, copy, modify, merge, publish, distribute, sublicense, and / or sell copies of\nthe Software, and to permit persons to whom the Software is furnished to do so,\nsubject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all\ncopies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR\nIMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS\nFOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR\nCOPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER\nIN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN\nCONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. \"");
                GL.BeginHorizontal();
                if (GL.Button(
                    new GUIContent("KingmakerMods.pw on GitHub", "https://github.com/fireundubh/KingmakerMods.pw"),
                    GL.ExpandWidth(false))) Application.OpenURL("https://github.com/fireundubh/KingmakerMods.pw");
                GL.EndHorizontal();
                GL.BeginHorizontal();
                if (GL.Button(new GUIContent("fireundubh on Patreon", "https://www.patreon.com/fireundubh"),
                    GL.ExpandWidth(false))) Application.OpenURL("https://www.patreon.com/fireundubh");
                GL.EndHorizontal();
            }

            GL.EndVertical();
        }

        public static void KeyBindings()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showKeyBindingsCategory = GL.Toggle(settings.showKeyBindingsCategory, RichText.Bold(" " + Strings.GetText("mainCategory_KeyBindings")), GL.ExpandWidth(false));
            GL.EndHorizontal();
            if (settings.showKeyBindingsCategory)
            {
                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_Teleport") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.teleportKey);
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_RotateCameraLeft") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.cameraTurnLeft);
                GL.EndHorizontal();

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_RotateCameraRight") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.cameraTurnRight);
                GL.EndHorizontal();

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_ResetCameraRotation") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.cameraReset);
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_FocusCamera") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.focusCameraKey);
                GL.EndHorizontal();
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_CycleFocus") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.focusCylceKey);
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_TogglePartyAlwaysRolls20") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.togglePartyAlwaysRoll20Key);
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("button_ResetCutsceneLock") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.resetCutsceneLockKey);
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_ActionKey") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.actionKey);
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                GL.Label(Strings.GetText("buttonToggle_ToggleHUD") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.hudToggleKey);
                GL.EndHorizontal();
            }

            GL.EndVertical();
        }

        public static void TaxCollectorRender()
        {
            GL.BeginVertical("box");

            GL.BeginHorizontal();
            GL.Label(RichText.MainCategoryFormat(Strings.GetText("mainCategory_TaxCollector")));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton("TaxCollectorRender");
            GL.EndHorizontal();

            MenuTools.ToggleButton(ref settings.toggleEnableTaxCollector, "taxCollector_EmployTaxCollector",
                "tooltip_TaxCollector", nameof(settings.toggleEnableTaxCollector));

            if (settings.toggleEnableTaxCollector == Storage.isTrueString)
            {
                settings.showTaxCollectorCategory = GL.Toggle(settings.showTaxCollectorCategory, RichText.Bold(Strings.GetText("taxCollector_Approach")), GL.ExpandWidth(false));

                if (settings.showTaxCollectorCategory)
                {
                    if (KingdomState.Instance != null)
                    {
                        if (Main.TaxSettings.initialLaunch)
                        {
                            TaxCollector.resultLine_0 = "";
                            var textLineInitial = "";
                            switch (Main.TaxSettings.textLineInitialCounter)
                            {
                                case 0:
                                    if (Game.Instance?.Player?.MainCharacter.Value.Gender == Gender.Male)
                                        Main.TaxSettings.playerTitle = Strings.GetText("taxCollector_Lord");
                                    else if (Game.Instance?.Player?.MainCharacter.Value.Gender == Gender.Female)
                                        Main.TaxSettings.playerTitle = Strings.GetText("taxCollector_Lady");
                                    else
                                        Main.TaxSettings.playerTitle = Strings.GetText("taxCollector_Excellence");
                                    if (Main.TaxSettings.playerTitle == Strings.GetText("taxCollector_Excellence"))
                                        textLineInitial =
                                            Strings.GetText("taxCollector_GreetingInitial_0_1") +
                                            $" {Main.TaxSettings.playerTitle}!";
                                    else
                                        textLineInitial =
                                            Strings.GetText("taxCollector_GreetingInitial_0_0") +
                                            $" {Main.TaxSettings.playerTitle}!";
                                    break;
                                case 1:
                                    textLineInitial = Strings.GetText("taxCollector_GreetingInitial_1");
                                    break;
                                case 2:
                                    textLineInitial = Strings.GetText("taxCollector_GreetingInitial_2");
                                    break;
                                case 3:
                                    textLineInitial = Strings.GetText("taxCollector_GreetingInitial_3");
                                    break;
                                case 4:
                                    textLineInitial = "";
                                    break;
                            }

                            MenuTools.SingleLineLabel(textLineInitial);
                            switch (Main.TaxSettings.textLineInitialCounter)
                            {
                                case 0:
                                    if (GL.Button(Strings.GetText("misc_Next"), GL.ExpandWidth(false)))
                                        Main.TaxSettings.textLineInitialCounter++;
                                    break;
                                case 1:
                                    if (GL.Button(Strings.GetText("misc_Yes"), GL.ExpandWidth(false)))
                                        Main.TaxSettings.textLineInitialCounter = 2;
                                    if (GL.Button(Strings.GetText("misc_No"), GL.ExpandWidth(false)))
                                        Main.TaxSettings.textLineInitialCounter = 3;
                                    break;
                                case 2:
                                    textLineInitial = Strings.GetText("taxCollector_GreetingInitial_2");
                                    if (GL.Button(Strings.GetText("taxCollector_ButtonAddresseMe") + ": ",
                                        GL.ExpandWidth(false)))
                                    {
                                        Main.TaxSettings.playerTitle = TaxCollector.playerTitleInput;
                                        Main.TaxSettings.textLineInitialCounter = 3;
                                    }

                                    TaxCollector.playerTitleInput = GL.TextField(TaxCollector.playerTitleInput, 100,
                                        GL.Width(300f));
                                    break;
                                case 3:
                                    if (GL.Button(Strings.GetText("misc_Next"), GL.ExpandWidth(false)))
                                        Main.TaxSettings.textLineInitialCounter = 4;
                                    break;
                                case 4:
                                    break;
                            }

                            Main.TaxSettings.saveTime = DateTime.Now;
                            TaxCollector.saveTimeGame = DateTime.Now;
                        }

                        if (Main.TaxSettings.textLineInitialCounter == 4)
                        {
                            var stats = KingdomState.Instance.Stats;
                            Main.TaxSettings.initialLaunch = false;
                            if (TaxCollector.isFirstVisit)
                            {
                                MenuTools.SingleLineLabel(Strings.GetText("taxCollector_GreetingReturn_0_0") +
                                                          $" {Main.TaxSettings.playerTitle} {Game.Instance.Player.MainCharacter.Value.CharacterName}!");
                                TaxCollector.isFirstVisit = false;
                            }
                            else
                            {
                                MenuTools.SingleLineLabel(Strings.GetText("taxCollector_GreetingReturn_0_1") +
                                                          $" {Main.TaxSettings.playerTitle} {Game.Instance.Player.MainCharacter.Value.CharacterName}!");
                            }

                            MenuTools.SingleLineLabel(Strings.GetText("taxCollector_GreetingReturn_1_0"));

                            GL.BeginHorizontal();
                            if (GL.Button(Strings.GetText("taxCollector_ButtonCollectTaxes"), GL.ExpandWidth(false)))
                            {
                                var collectedMoney = TaxCollector.CollectMoney(
                                    TaxCollector.MoneyRank(stats[KingdomStats.Type.Economy].Rank,
                                        stats[KingdomStats.Type.Stability].Rank, stats[KingdomStats.Type.Loyalty].Rank),
                                    Game.Instance.Player.MainCharacter.Value.Stats.Charisma.ModifiedValue,
                                    TaxCollector.saveTimeGame);
                                var collectedBP = TaxCollector.CollectBP(
                                    TaxCollector.BPRank(stats[KingdomStats.Type.Economy].Rank,
                                        stats[KingdomStats.Type.Stability].Rank,
                                        stats[KingdomStats.Type.Community].Rank), TaxCollector.saveTimeGame);


                                if (collectedMoney >= 1 && collectedBP >= 1)
                                {
                                    Game.Instance.Player.GainMoney(collectedMoney);
                                    TaxCollector.resultLine_0 =
                                        Strings.GetText("taxCollector_Collected_0_0") + ": + " + collectedMoney + " " +
                                        Strings.GetText("taxCollector_Money") + ", + " + collectedBP + " " +
                                        Strings.GetText("taxCollector_BuildPoints");
                                    KingdomState.Instance.BP = KingdomState.Instance.BP + collectedBP;
                                    Game.Instance.UI.Common.UISound.Play(UISoundType.LootCollectGold);
                                    Common.AddLogEntry(
                                        Strings.GetText("logMessage_Received") + " " + collectedMoney + " " +
                                        Strings.GetText("taxCollector_Money"), Color.black);
                                    Common.AddLogEntry(
                                        Strings.GetText("logMessage_Received") + " " + collectedBP + " " +
                                        Strings.GetText("taxCollector_BuildPoints"), Color.black);
                                }
                                else if (collectedMoney < 1 && collectedBP < 1)
                                {
                                    TaxCollector.resultLine_0 = Strings.GetText("taxCollector_NothingToCollect_0_0");
                                    Game.Instance.UI.Common.UISound.Play(UISoundType.ErrorEquip);
                                    Common.AddLogEntry(TaxCollector.resultLine_0, Color.black);
                                }
                                else if (collectedMoney >= 1 && collectedBP < 1)
                                {
                                    Game.Instance.Player.GainMoney(collectedMoney);
                                    TaxCollector.resultLine_0 =
                                        Strings.GetText("taxCollector_Collected_0_0") + " " + collectedMoney + " " +
                                        Strings.GetText("taxCollector_Money");
                                    Game.Instance.UI.Common.UISound.Play(UISoundType.LootCollectGold);
                                    Common.AddLogEntry(
                                        Strings.GetText("logMessage_Received") + " " + collectedMoney + " " +
                                        Strings.GetText("taxCollector_Money"), Color.black);
                                }
                                else if (collectedMoney < 1 && collectedBP >= 1)
                                {
                                    KingdomState.Instance.BP = KingdomState.Instance.BP + collectedBP;
                                    TaxCollector.resultLine_0 =
                                        Strings.GetText("taxCollector_Collected_0_0") + " " + collectedBP + " " +
                                        Strings.GetText("taxCollector_BuildPoints");
                                    Game.Instance.UI.Common.UISound.Play(UISoundType.LootCollectGold);
                                    Common.AddLogEntry(
                                        Strings.GetText("logMessage_Received") + " " + collectedBP + " " +
                                        Strings.GetText("taxCollector_BuildPoints"), Color.black);
                                }
                                else
                                {
                                    TaxCollector.resultLine_0 = "";
                                }

                                Main.TaxSettings.saveTime = DateTime.Now;
                                TaxCollector.saveTimeGame = Main.TaxSettings.saveTime;
                            }

                            GL.EndHorizontal();
                            MenuTools.SingleLineLabel(TaxCollector.resultLine_0);
                        }
                    }
                    else
                    {
                        MenuTools.SingleLineLabel(Strings.GetText("message_NoKingdom"));
                    }
                }
                else
                {
                    TaxCollector.resultLine_0 = "";
                }
            }

            GL.EndVertical();
        }

        public static void FavouriteFunctions()
        {
            GL.BeginVertical("box");

            if (Storage.togglesFavouritesLoad == true)
            {
                Main.RefreshTogglesFavourites();
                Storage.togglesFavouritesLoad = false;
            }

            if (!Storage.togglesFavourites.Any())
            {
                MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
            }
            else
            {
                GL.BeginVertical("box");
                GL.BeginHorizontal();
                settings.editFavouriteFunctionsPosition = GL.Toggle(settings.editFavouriteFunctionsPosition, RichText.Bold(Strings.GetText("toggle_MoveFavourites")), GL.ExpandWidth(false));
                GL.EndHorizontal();
                GL.EndVertical();
                GL.Space(10);
                for (var i = 0; i < Storage.togglesFavourites.Count; i++)
                {
                    var sA = Storage.togglesFavourites[i].Split(new char[] { ',' });
                    if (sA.Length == 3)
                    {
                        GL.BeginVertical("box");
                        if (settings.editFavouriteFunctionsPosition)
                        {
                            GL.BeginHorizontal();
                            MenuTools.AddUpDownButtons(Storage.togglesFavourites[i], ref Storage.togglesFavourites, 13);
                            GL.EndHorizontal();
                        }

                        MenuTools.ToggleButtonFavouritesMenu(ref MenuTools.GetToggleButton(sA[0]), sA[1], sA[2]);
                        GL.FlexibleSpace();
                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                        {
                            Storage.togglesFavourites.Remove(Storage.togglesFavourites[i]);
                            Storage.togglesFavouritesLoad = true;
                        }

                        GL.EndHorizontal();
                        GL.EndVertical();
                    }
                    else if (sA.Length == 1)
                    {
                        try
                        {
                            if (settings.editFavouriteFunctionsPosition)
                            {
                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                MenuTools.AddUpDownButtons(Storage.togglesFavourites[i], ref Storage.togglesFavourites,
                                    13);
                                GL.EndHorizontal();
                            }

                            typeof(Menu).GetMethod(sA[0]).Invoke(typeof(Menu), new object[] { });
                            if (settings.editFavouriteFunctionsPosition) GL.EndVertical();
                        }
                        catch (Exception e)
                        {
                            modLogger.Log(e.ToString());
                        }
                    }
                }
            }

            GL.EndVertical();
        }

        public static void Cheat()
        {
            foreach (var method in settings.cheatsCategories)
                typeof(Menu).GetMethod(method).Invoke(typeof(Menu), new object[] { });
        }

        public static void Mods()
        {
            UIOptions();

            GL.Space(10);

            MiscMods();

            GL.Space(10);

            BlueprintModdingRender();

            GL.Space(10);

            TaxCollectorRender();
        }

        public static void BlueprintModdingRender()
        {
            ModifiedBlueprintTools.RenderMenu();
        }

        public static void Tools()
        {
            Camera();

            GL.Space(10);

            ActionKeyRender();

            GL.Space(10);

            DevToolsRender();

            GL.Space(10);

            MiscTools();
        }

        public static void MiscTools()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.MainCategoryFormat(Strings.GetText("mainCategory_Misc")));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(MiscTools));
            GL.EndHorizontal();

            GL.BeginVertical("box");
            MenuTools.ToggleButton(ref settings.toggleHUDToggle, "buttonToggle_EnableToggleHUD", "tooltip_ToggleHUD");
            if (Strings.ToBool(settings.toggleHUDToggle))
            {
                GL.Space(10);
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("buttonToggle_ToggleHUD") + ": ", GL.ExpandWidth(false));
                MenuTools.SetKeyBinding(ref settings.hudToggleKey);
                GL.EndHorizontal();
            }

            GL.EndVertical();

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleDisplayObjectInfo, "buttonToggle_DisplayObjectInfo", "tooltip_DisplayObjectInfo", nameof(settings.toggleDisplayObjectInfo));

            GL.Space(10);

            MenuTools.ToggleButton(ref settings.toggleAutomaticallyLoadLastSave, "buttonToggle_AutomaticallyLoadLastSave", "tooltip_AutomaticallyLoadLastSave", nameof(settings.toggleAutomaticallyLoadLastSave));

            GL.EndVertical();
        }

        public static void DevToolsRender()
        {
            DevTools.Render();
        }

        public static void Achievements()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            Storage.showAchievementsMenu = GL.Toggle(Storage.showAchievementsMenu, RichText.Bold(Strings.GetText("header_Achievements")), GL.ExpandWidth(false));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton(nameof(Achievements));
            GL.EndHorizontal();
            if (Storage.showAchievementsMenu)
            {
                GL.Space(10);
                GL.BeginHorizontal();
                if (GL.Button(Strings.GetText("misc_UnlockAll"), GL.ExpandWidth(false)))
                    try
                    {
                        foreach (var achievementEntity in Game.Instance.Player.Achievements) achievementEntity.Unlock();
                    }
                    catch (Exception e)
                    {
                        modLogger.Log(e.ToString());
                    }

                GL.EndHorizontal();
                GL.Space(10);
                foreach (var achievementEntity in Game.Instance.Player.Achievements)
                    if (!achievementEntity.IsUnlocked)
                    {
                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        GL.Label(achievementEntity.Data.name);
                        GL.Space(10);
                        if (GL.Button(Strings.GetText("misc_Unlock"), GL.ExpandWidth(false)))
                            achievementEntity.Unlock();
                        GL.EndHorizontal();
                        GL.EndVertical();
                    }
            }

            GL.EndVertical();
        }

        public static void ActionKeyRender()
        {
            ActionKey.RenderMenu();
        }

        public static void VersionMismatch()
        {
            if (GameVersion.GetVersion() != settings.hiddenGameVersion)
            {
                MenuTools.SingleLineLabel(RichText.Bold(
                    $"{Strings.GetText("warning_CreatedForVersion_0")} {Storage.gamerVersionAtCreation} - {Strings.GetText("warning_CreatedForVersion_1")} {GameVersion.GetVersion()}.\n{Strings.GetText("warning_CreatedForVersion_2")}"));
                if (GL.Button(Strings.GetText("misc_Hide"), GL.ExpandWidth(false)))
                {
                    Storage.hideVersionMismatch = true;
                    settings.hiddenGameVersion = GameVersion.GetVersion();
                }
            }
        }
    }
}