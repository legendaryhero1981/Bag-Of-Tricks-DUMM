using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers.Rest.Cooking;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Dungeon;
using Kingmaker.Dungeon.Blueprints;
using Kingmaker.Dungeon.Units.Debug;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Formations;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Items;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Kingdom.Tasks;
using Kingmaker.Kingdom.UI;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.TextTools;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.Group;
using Kingmaker.UI.IngameMenu;
using Kingmaker.UI.Kingdom;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.ServiceWindow.LocalMap;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.Visual;
using Kingmaker.Visual.FogOfWar;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.LocalMap;
using Kingmaker.Visual.Sound;
using TMPro;
using UberLogger;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModManagerNet;
using static Kingmaker.UnitLogic.Class.LevelUp.LevelUpState;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BagOfTricks
{
    internal static class HarmonyPatches
    {
        public static Settings settings = Main.settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;
        public static Player player = Game.Instance.Player;

        public static BlueprintAbility ExtractSpell([NotNull] ItemEntity item)
        {
            var itemEntityUsable = item as ItemEntityUsable;
            if (itemEntityUsable?.Blueprint.Type != UsableItemType.Scroll) return null;
            return itemEntityUsable.Blueprint.Ability.Parent
                ? itemEntityUsable.Blueprint.Ability.Parent
                : itemEntityUsable.Blueprint.Ability;
        }

        public static string GetSpellbookActionName(string actionName, ItemEntity item, UnitEntityData unit)
        {
            if (actionName != LocalizedTexts.Instance.Items.CopyScroll) return actionName;

            var spell = ExtractSpell(item);
            if (spell == null) return actionName;

            var spellbooks = unit.Descriptor.Spellbooks.Where(x => x.Blueprint.SpellList.Contains(spell)).ToList();

            var count = spellbooks.Count;

            if (count <= 0) return actionName;

            var actionFormat = LocalizationHelper.Process("{0} <{1}>");

            return string.Format(actionFormat, actionName, count == 1 ? spellbooks.First().Blueprint.Name : "Multiple");
        }

        [HarmonyPatch(typeof(EncumbranceHelper), "GetHeavy")]
        private static class EncumbranceHelper_GetHeavy_Patch
        {
            private static void Postfix(ref int __result)
            {
                if (settings.useCustomHeavyEncumbranceMultiplier == false)
                    __result = __result * Mathf.RoundToInt(settings.heavyEncumbranceMultiplier);
                else if (settings.useCustomHeavyEncumbranceMultiplier)
                    __result = Mathf.RoundToInt(__result * settings.finalCustomHeavyEncumbranceMultiplier);
            }
        }

        [HarmonyPatch(typeof(UnitPartWeariness), "GetFatigueHoursModifier")]
        private static class EncumbranceHelper_GetFatigueHoursModifier_Patch
        {
            private static void Postfix(ref float __result)
            {
                if (settings.useCustomFatigueHoursModifierMultiplier == false)
                    __result = __result * (float) Math.Round(settings.fatigueHoursModifierMultiplier, 1);
                else if (settings.useCustomFatigueHoursModifierMultiplier)
                    __result = __result * float.Parse(settings.customFatigueHoursModifierMultiplier);
            }
        }

        [HarmonyPatch(typeof(RestController), "CalculateNeededRations")]
        private static class RestController_CalculateNeededRations_Patch
        {
            private static void Postfix(ref int __result)
            {
                if (settings.toggleNoRationsRequired == Storage.isTrueString) __result = 0;
            }
        }

        [HarmonyPatch(typeof(Player), "GainPartyExperience")]
        public static class Player_GainPartyExperience_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Player __instance, ref int gained)
            {
                if (settings.useCustomExperienceMultiplier == false &&
                    settings.toggleExperienceMultiplier == Storage.isTrueString)
                    gained = Mathf.RoundToInt(gained * (float) Math.Round(settings.experienceMultiplier, 1));
                else if (settings.useCustomExperienceMultiplier &&
                         settings.toggleExperienceMultiplier == Storage.isTrueString)
                    gained = Mathf.RoundToInt(gained * float.Parse(settings.customExperienceMultiplier));

                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "GainMoney")]
        public static class Player_GainMoney_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Player __instance, ref long amount)
            {
                if (settings.useCustomMoneyMultiplier == false)
                    amount = Mathf.RoundToInt(amount * (float) Math.Round(settings.moneyMultiplier, 1));
                else if (settings.useCustomMoneyMultiplier)
                    amount = Mathf.RoundToInt(amount * float.Parse(settings.customMoneyMultiplier));

                return true;
            }
        }

        [HarmonyPatch(typeof(FogOfWarRenderer), "Update")]
        public static class FogOfWarRenderer_Update_Patch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var foundfoundFogOfWarGlobalFlag = -1;

                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Ldstr && (string) codes[i].operand == "_FogOfWarGlobalFlag" &&
                        codes[i + 5].opcode == OpCodes.Brfalse)
                    {
                        foundfoundFogOfWarGlobalFlag = i + 6;
                        break;
                    }

                if (settings.toggleFogOfWarVisuals == Storage.isFalseString)
                    codes[foundfoundFogOfWarGlobalFlag].opcode = OpCodes.Ldc_I4_0;
                else
                    codes[foundfoundFogOfWarGlobalFlag].opcode = OpCodes.Ldc_I4_1;

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(RuleCastSpell), "IsArcaneSpellFailed", MethodType.Getter)]
        public static class RuleCastSpell_IsArcaneSpellFailed_Patch
        {
            private static void Postfix(RuleCastSpell __instance, ref bool __result)
            {
                if ((__instance.Spell.Caster?.Unit?.IsPlayerFaction ?? false) &&
                    settings.toggleArcaneSpellFailureRoll == Storage.isTrueString) __result = false;
            }
        }

        [HarmonyPatch(typeof(RuleRollD20), "Roll")]
        public static class RuleRollD20_Roll_Patch
        {
            private static void Postfix(RuleRollD20 __instance, ref int __result)
            {
                if (settings.toggleRollWithDisadvantage == Storage.isTrueString)
                    switch (settings.rollWithDisadvantageeIndex)
                    {
                        case 0:
                            __result = Math.Min(__result, RulebookEvent.Dice.D20);
                            break;
                        case 1:
                            if (__instance.Initiator.IsPlayerFaction)
                                __result = Math.Min(__result, RulebookEvent.Dice.D20);
                            break;
                        case 2:
                            if (!__instance.Initiator.IsPlayerFaction &&
                                __instance.Initiator.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot
                                    .PlayerFaction)) __result = Math.Min(__result, RulebookEvent.Dice.D20);
                            break;
                    }

                if (settings.toggleRollWithAdvantage == Storage.isTrueString)
                    switch (settings.rollWithAdvantageIndex)
                    {
                        case 0:
                            __result = Math.Max(__result, RulebookEvent.Dice.D20);
                            break;
                        case 1:
                            if (__instance.Initiator.IsPlayerFaction)
                                __result = Math.Max(__result, RulebookEvent.Dice.D20);
                            break;
                        case 2:
                            if (!__instance.Initiator.IsPlayerFaction &&
                                __instance.Initiator.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot
                                    .PlayerFaction)) __result = Math.Max(__result, RulebookEvent.Dice.D20);
                            break;
                    }

                if (settings.toggleNeverRoll1 == Storage.isTrueString && __result == 1)
                    switch (settings.neverRoll1Index)
                    {
                        case 0:
                            __result = Random.Range(2, 21);
                            break;
                        case 1:
                            if (__instance.Initiator.IsPlayerFaction) __result = Random.Range(2, 21);
                            break;
                        case 2:
                            if (!__instance.Initiator.IsPlayerFaction &&
                                __instance.Initiator.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot
                                    .PlayerFaction)) __result = Random.Range(2, 21);
                            break;
                    }

                if (settings.toggleNeverRoll20 == Storage.isTrueString && __result == 20)
                    switch (settings.neverRoll20Index)
                    {
                        case 0:
                            __result = Random.Range(1, 20);
                            break;
                        case 1:
                            if (__instance.Initiator.IsPlayerFaction) __result = Random.Range(1, 20);
                            break;
                        case 2:
                            if (!__instance.Initiator.IsPlayerFaction &&
                                __instance.Initiator.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot
                                    .PlayerFaction)) __result = Random.Range(1, 20);
                            break;
                    }

                if (settings.togglePartyAlwaysRoll20 == Storage.isFalseString &&
                    __instance.Initiator.IsInCombat == false && settings.takeXIndex != 0)
                    switch (settings.takeXIndex)
                    {
                        case 1:
                            __result = 10;
                            break;
                        case 2:
                            __result = 20;
                            break;
                        case 3:
                            __result = Mathf.RoundToInt(settings.takeXCustom);
                            break;
                    }

                if (Strings.ToBool(settings.toggleMainCharacterRoll20))
                    if (__instance.Initiator.IsMainCharacter)
                        __result = 20;
                if (settings.togglePartyAlwaysRoll20 == Storage.isTrueString)
                    if (__instance.Initiator.IsPlayerFaction)
                        __result = 20;
                if (settings.toggleEnemiesAlwaysRoll1 == Storage.isTrueString)
                    if (!__instance.Initiator.IsPlayerFaction &&
                        __instance.Initiator.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot
                            .PlayerFaction))
                        __result = 1;
                if (settings.toggleEveryoneExceptPlayerFactionRolls1 == Storage.isTrueString)
                    if (!__instance.Initiator.IsPlayerFaction)
                        __result = 1;
            }
        }

        [HarmonyPatch(typeof(RuleInitiativeRoll), "OnTrigger")]
        public static class RuleInitiativeRoll_OnTrigger_Patch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var codesDefault = new List<CodeInstruction>(instructions);
                var found20 = -1;

                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Ret && codes[i - 1].opcode == OpCodes.Call &&
                        codes[i - 2].opcode == OpCodes.Call)
                    {
                        found20 = i - 2;
                        break;
                    }

                if (settings.toggleRoll20Initiative == Storage.isTrueString)
                {
                    codes[found20].opcode = OpCodes.Ldc_I4_S;
                    codes[found20].operand = 20;
                    return codes.AsEnumerable();
                }

                return codesDefault.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Spellbook), "GetSpellsPerDay")]
        private static class Spellbook_GetSpellsPerDay_Patch
        {
            private static void Postfix(ref int __result)
            {
                if (settings.useCustomspellsPerDayMultiplier == false)
                    __result = Mathf.RoundToInt(__result * (float) Math.Round(settings.spellsPerDayMultiplier, 1));
                else if (settings.useCustomspellsPerDayMultiplier)
                    __result = Mathf.RoundToInt(__result * float.Parse(settings.customSpellsPerDayMultiplier));
            }
        }

        [HarmonyPatch(typeof(Spellbook), "SpendInternal")]
        public static class Spellbook_SpendInternal_Patch
        {
            public static bool Prefix([CanBeNull] AbilityData spell, ref bool doSpend)
            {
                var currentGameMode = Game.Instance.CurrentMode;

                if ((spell?.Caster?.Unit?.IsPlayerFaction ?? false) &&
                    settings.toggleUnlimitedCasting == Storage.isTrueString &&
                    currentGameMode == GameModeType.Default) doSpend = false;
                return true;
            }
        }

        public static class LocalizationHelper
        {
            public static string Process(string value)
            {
                if (Application.isPlaying) return TextTemplateEngine.Process(value);
                return value;
            }
        }

        [HarmonyPatch(typeof(CopyScroll), "CanCopySpell")]
        public static class CanCopySpell_CanCopySpell_Patch
        {
            private static bool Prefix()
            {
                return false;
            }

            private static void Postfix([NotNull] BlueprintAbility spell, [NotNull] Spellbook spellbook,
                ref bool __result)
            {
                if (spellbook.IsKnown(spell))
                {
                    __result = false;
                    return;
                }

                var spellListContainsSpell = spellbook.Blueprint.SpellList.Contains(spell);

                if (settings.toggleSpontaneousCopyScrolls == Storage.isTrueString && spellbook.Blueprint.Spontaneous &&
                    spellListContainsSpell)
                {
                    __result = true;
                    return;
                }

                __result = spellbook.Blueprint.CanCopyScrolls && spellListContainsSpell;
            }
        }

        [HarmonyPatch(typeof(ItemSlot), "ScrollContent", MethodType.Getter)]
        public static class ItemSlot_ScrollContent_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(ItemSlot __instance, ref string __result)
            {
                var currentCharacter = UIUtility.GetCurrentCharacter();
                var component = __instance.Item.Blueprint.GetComponent<CopyItem>();
                var actionName = component?.GetActionName(currentCharacter) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(actionName))
                    actionName = GetSpellbookActionName(actionName, __instance.Item, currentCharacter);
                __result = actionName;
            }
        }

        [HarmonyPatch(typeof(KingdomUIEventWindow), "OnClose")]
        public static class KingdomUIEventWindow_OnClose_Patch
        {
            public static bool Prefix(ref bool __state)
            {
                __state = settings.toggleInstantEvent == Storage.isTrueString;
                return !__state;
            }

            public static void Postfix(bool __state, KingdomEventUIView ___m_KingdomEventView,
                KingdomEventHandCartController ___m_Cart)
            {
                if (__state)
                    if (___m_KingdomEventView != null)
                    {
                        EventBus.RaiseEvent((IEventSceneHandler h) => h.OnEventSelected(null, ___m_Cart));

                        if (___m_KingdomEventView.IsFinished || ___m_KingdomEventView.AssignedLeader == null ||
                            ___m_KingdomEventView.Blueprint.NeedToVisitTheThroneRoom) return;

                        var inProgress = ___m_KingdomEventView.IsInProgress;
                        var leader = ___m_KingdomEventView.AssignedLeader.Blueprint;

                        if (!inProgress || leader == null) return;

                        ___m_KingdomEventView.Event.Resolve(___m_KingdomEventView.Task);

                        if (___m_KingdomEventView.RulerTimeRequired <= 0) return;

                        foreach (var unitEntityData in player.AllCharacters)
                            RestController.ApplyRest(unitEntityData.Descriptor);

                        new KingdomTimelineManager().MaybeUpdateTimeline();
                    }
            }
        }

        [HarmonyPatch(typeof(Player), "GetCustomCompanionCost")]
        public static class Player_GetCustomCompanionCost_Patch
        {
            public static bool Prefix(ref bool __state)
            {
                return !__state;
            }

            public static void Postfix(ref int __result)
            {
                __result = __result * settings.companionCostMultiplier;
            }
        }

        [HarmonyPatch(typeof(CampPlaceView), "ReplaceWithInactiveCamp")]
        public static class CampPlaceView_ReplaceWithInactiveCamp_Patch
        {
            public static bool Prefix(ref bool __state)
            {
                __state = settings.toggleNoInactiveCamp == Storage.isTrueString;
                return !__state;
            }

            public static void Postfix(ref MapObjectView __result, ref CampPlaceView __instance, ref bool __state)
            {
                if (__state)
                {
                    __instance.Destroy();
                    __result = null;
                }
            }
        }

        [HarmonyPatch(typeof(KingdomTaskEvent), "SkipPlayerTime", MethodType.Getter)]
        public static class KingdomTaskEvent_SkipPlayerTime_Patch
        {
            public static void Postfix(ref int __result)
            {
                if (settings.toggleInstantEvent == Storage.isTrueString) __result = 0;
            }
        }

        [HarmonyPatch(typeof(KingdomUIEventWindowFooter), "OnStart")]
        public static class KingdomUIEventWindowFooter_OnStart_Patch
        {
            public static bool Prefix(ref bool __state)
            {
                __state = settings.toggleInstantEvent == Storage.isTrueString;
                return !__state;
            }

            public static void Postfix(KingdomEventUIView ___m_KingdomEventView, bool __state)
            {
                if (__state)
                {
                    EventBus.RaiseEvent((IKingdomUIStartSpendTimeEvent h) =>
                        h.OnStartSpendTimeEvent(___m_KingdomEventView.Blueprint));
                    var kingdomTaskEvent = ___m_KingdomEventView?.Task;
                    EventBus.RaiseEvent((IKingdomUICloseEventWindow h) => h.OnClose());
                    kingdomTaskEvent?.Start(false);

                    if (kingdomTaskEvent == null) return;

                    if (kingdomTaskEvent.IsFinished || kingdomTaskEvent.AssignedLeader == null ||
                        ___m_KingdomEventView.Blueprint.NeedToVisitTheThroneRoom) return;

                    kingdomTaskEvent.Event.Resolve(kingdomTaskEvent);

                    if (___m_KingdomEventView.RulerTimeRequired <= 0) return;
                    foreach (var unitEntityData in player.AllCharacters)
                        RestController.ApplyRest(unitEntityData.Descriptor);
                    new KingdomTimelineManager().MaybeUpdateTimeline();
                }
            }
        }

        [HarmonyPatch(typeof(AbilityResourceLogic), "Spend")]
        public static class AbilityResourceLogic_Spend_Patch
        {
            public static bool Prefix(AbilityData ability)
            {
                var unit = ability.Caster.Unit;
                if (unit?.IsPlayerFaction == true && settings.toggleInfiniteAbilities == Storage.isTrueString)
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(ActivatableAbilityResourceLogic), "SpendResource")]
        public static class ActivatableAbilityResourceLogic_SpendResource_Patch
        {
            public static bool Prefix()
            {
                return settings.toggleInfiniteAbilities != Storage.isTrueString;
            }
        }

        [HarmonyPatch(typeof(UnitCombatState), "HasCooldownForCommand")]
        [HarmonyPatch(new[] {typeof(UnitCommand)})]
        public static class UnitCombatState_HasCooldownForCommand_Patch1
        {
            public static void Postfix(ref bool __result, UnitCombatState __instance)
            {
                if (Strings.ToBool(settings.toggleInstantCooldown) && __instance.Unit.IsDirectlyControllable)
                    __result = false;
                if (Strings.ToBool(settings.toggleInstantCooldownMainChar) && __instance.Unit.IsMainCharacter)
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(UnitCombatState), "HasCooldownForCommand")]
        [HarmonyPatch(new[] {typeof(UnitCommand.CommandType)})]
        public static class UnitCombatState_HasCooldownForCommand_Patch2
        {
            public static void Postfix(ref bool __result, UnitCombatState __instance)
            {
                if (Strings.ToBool(settings.toggleInstantCooldown) && __instance.Unit.IsDirectlyControllable)
                    __result = false;
                if (Strings.ToBool(settings.toggleInstantCooldownMainChar) && __instance.Unit.IsMainCharacter)
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(UnitCombatState), "OnNewRound")]
        public static class UnitCombatState_OnNewRound_Patch
        {
            public static bool Prefix(UnitCombatState __instance)
            {
                if (__instance.Unit.IsDirectlyControllable && Strings.ToBool(settings.toggleInstantCooldown))
                {
                    __instance.Cooldown.Initiative = 0f;
                    __instance.Cooldown.StandardAction = 0f;
                    __instance.Cooldown.MoveAction = 0f;
                    __instance.Cooldown.SwiftAction = 0f;
                    __instance.Cooldown.AttackOfOpportunity = 0f;
                }

                if (__instance.Unit.IsMainCharacter && Strings.ToBool(settings.toggleInstantCooldownMainChar))
                {
                    __instance.Cooldown.Initiative = 0f;
                    __instance.Cooldown.StandardAction = 0f;
                    __instance.Cooldown.MoveAction = 0f;
                    __instance.Cooldown.SwiftAction = 0f;
                    __instance.Cooldown.AttackOfOpportunity = 0f;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(UnitActionController), "UpdateCooldowns")]
        public static class UnitActionController_UpdateCooldowns_Patch
        {
            public static void Postfix(ref UnitCommand command)
            {
                if (Strings.ToBool(settings.toggleInstantCooldown))
                {
                    if (!command.Executor.IsInCombat || command.IsIgnoreCooldown) return;

                    var isPlayerFaction = command.Executor.IsPlayerFaction;
                    var timeSinceStart = command.TimeSinceStart;

                    var moveActionCooldown = isPlayerFaction ? 0f : 3f - timeSinceStart;
                    var standardActionCooldown = isPlayerFaction ? 0f : 6f - timeSinceStart;
                    var swiftActionCooldown = isPlayerFaction ? 0f : 6f - timeSinceStart;

                    switch (command.Type)
                    {
                        case UnitCommand.CommandType.Free:
                        case UnitCommand.CommandType.Move:
                            command.Executor.CombatState.Cooldown.MoveAction = moveActionCooldown;
                            break;
                        case UnitCommand.CommandType.Standard:
                            command.Executor.CombatState.Cooldown.StandardAction = standardActionCooldown;
                            break;
                        case UnitCommand.CommandType.Swift:
                            command.Executor.CombatState.Cooldown.SwiftAction = swiftActionCooldown;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (Strings.ToBool(settings.toggleInstantCooldownMainChar))
                {
                    if (!command.Executor.IsInCombat || command.IsIgnoreCooldown) return;

                    var isMainChar = command.Executor.IsMainCharacter;
                    var timeSinceStart = command.TimeSinceStart;

                    var moveActionCooldown = isMainChar ? 0f : 3f - timeSinceStart;
                    var standardActionCooldown = isMainChar ? 0f : 6f - timeSinceStart;
                    var swiftActionCooldown = isMainChar ? 0f : 6f - timeSinceStart;

                    switch (command.Type)
                    {
                        case UnitCommand.CommandType.Free:
                        case UnitCommand.CommandType.Move:
                            command.Executor.CombatState.Cooldown.MoveAction = moveActionCooldown;
                            break;
                        case UnitCommand.CommandType.Standard:
                            command.Executor.CombatState.Cooldown.StandardAction = standardActionCooldown;
                            break;
                        case UnitCommand.CommandType.Swift:
                            command.Executor.CombatState.Cooldown.SwiftAction = swiftActionCooldown;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(UnitStealthController), "TickUnit")]
        public static class UnitStealthController_TickUnit_Patch
        {
            public static bool Prefix(UnitEntityData unit, ref UnitStealthController __instance)
            {
                if (!unit.IsPlayerFaction || settings.toggleUndetectableStealth != Storage.isTrueString) return true;
                var unitState = unit.Descriptor.State;

                var isInStealth = unitState.IsInStealth;
                var shouldBeInStealth = __instance.ShouldBeInStealth(unit);

                if (unitState.IsInStealth)
                {
                    if (shouldBeInStealth)
                        for (var i = 0; i < Game.Instance.State.AwakeUnits.Count; i++)
                        {
                            var spotterUnit = Game.Instance.State.AwakeUnits[i];

                            var hasBeenSpotted = unit.Stealth.SpottedBy.Contains(spotterUnit);
                            var consciousWithLOS = spotterUnit.Descriptor.State.IsConscious && spotterUnit.HasLOS(unit);

                            if (!consciousWithLOS || hasBeenSpotted || !spotterUnit.IsEnemy(unit))
                            {
                                if (!hasBeenSpotted && consciousWithLOS && !spotterUnit.IsEnemy(unit) &&
                                    !unit.Stealth.InAmbush) unit.Stealth.AddSpottedBy(spotterUnit);
                            }
                            else
                            {
                                var distanceToSpotter = unit.DistanceTo(spotterUnit.Position) - unit.View.Corpulence -
                                                        spotterUnit.View.Corpulence;
                                if (distanceToSpotter > GameConsts.MinWeaponRange.Meters - 0.1f) continue;
                                var source_SpotterBreaksStealth = Traverse.Create(__instance)
                                    .Method("SpotterBreaksStealth", unit, spotterUnit);
                                if (source_SpotterBreaksStealth.GetValue<bool>())
                                {
                                    shouldBeInStealth = false;
                                    break;
                                }

                                if (!unit.Stealth.AddSpottedBy(spotterUnit)) continue;
                                EventBus.RaiseEvent<IUnitSpottedHandler>(h => h.HandleUnitSpotted(unit, spotterUnit));
                            }
                        }

                    if (!shouldBeInStealth)
                    {
                        unitState.IsInStealth = false;
                        unit.Stealth.Clear();
                        if (unit.IsPlayerFaction) unit.Stealth.WantEnterStealth = false;
                    }
                }
                else if (!unitState.IsInStealth && shouldBeInStealth)
                {
                    Rulebook.Trigger(new RuleEnterStealth(unit));
                }

                if (isInStealth != unitState.IsInStealth)
                    EventBus.RaiseEvent<IUnitStealthHandler>(h =>
                        h.HandleUnitSwitchStealthCondition(unit, unitState.IsInStealth));
                unit.Stealth.ForceEnterStealth = false;
                unit.Stealth.BecameInvisibleThisFrame = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelUpState), MethodType.Constructor)]
        [HarmonyPatch(new[] {typeof(UnitDescriptor), typeof(CharBuildMode)})]
        public static class LevelUpState_Patch
        {
            public static void Postfix(UnitDescriptor unit, CharBuildMode mode, ref LevelUpState __instance)
            {
                if (__instance.NextLevel == 1)
                    if (mode != CharBuildMode.PreGen)
                    {
                        var pointCount = Math.Max(0,
                            unit.IsCustomCompanion()
                                ? settings.characterCreationAbilityPointsMerc
                                : settings.characterCreationAbilityPointsPlayer);
                        __instance.StatsDistribution.Start(pointCount);
                    }
            }
        }

        [HarmonyPatch(typeof(SpendSkillPoint), "Apply")]
        public static class SpendSkillPoint_Apply_Patch
        {
            public static bool Prefix(ref bool __state)
            {
                __state = settings.toggleInfiniteSkillpoints == Storage.isTrueString;
                return !__state;
            }

            public static void Postfix(ref bool __state, LevelUpState state, UnitDescriptor unit, StatType ___Skill)
            {
                if (__state) unit.Stats.GetStat(___Skill).BaseValue++;
            }
        }

        [HarmonyPatch(typeof(LevelUpHelper), "AddFeatures")]
        public static class MultiplyFeatPoints_LevelUpHelper_AddFeatures_Patch
        {
            public static bool Prefix([NotNull] LevelUpState state, [NotNull] UnitDescriptor unit,
                [NotNull] IList<BlueprintFeatureBase> features, [CanBeNull] BlueprintScriptableObject source, int level)
            {
                for (var i = 0; i < settings.featMultiplier; ++i)
                {
                    foreach (var item in features.OfType<BlueprintFeatureSelection>())
                        state.AddSelection(null, source, item, level);
                    foreach (var item2 in features.OfType<BlueprintFeature>())
                    {
                        var feature = (Feature) unit.AddFact(item2);
                        var blueprintProgression = item2 as BlueprintProgression;
                        if (blueprintProgression != null)
                            LevelUpHelper.UpdateProgression(state, unit, blueprintProgression);
                        feature.Source = source;
                    }
                }

                return false;
            }
        }


        [HarmonyPatch(typeof(MapMovementController), "GetRegionalModifier", new Type[] { })]
        public static class MovementSpeed_GetRegionalModifier_Patch1
        {
            public static void Postfix(ref float __result)
            {
                var speedMultiplier = Mathf.Clamp(settings.travelSpeedMultiplier, 0.1f, 100f);
                __result = speedMultiplier * __result;
            }
        }

        [HarmonyPatch(typeof(MapMovementController), "GetRegionalModifier", typeof(Vector3))]
        public static class MovementSpeed_GetRegionalModifier_Patch2
        {
            public static void Postfix(ref float __result)
            {
                var speedMultiplier = Mathf.Clamp(settings.travelSpeedMultiplier, 0.1f, 100f);
                __result = speedMultiplier * __result;
            }
        }


        [HarmonyPatch(typeof(StatsDistribution), "CanRemove")]
        public static class StatsDistribution_CanRemove_Patch
        {
            public static void Postfix(ref bool __result, StatType attribute, StatsDistribution __instance)
            {
                if (settings.characterCreationAbilityPointsMin != 7)
                    __result = __instance.Available &&
                               __instance.StatValues[attribute] > settings.characterCreationAbilityPointsMin;
            }
        }

        [HarmonyPatch(typeof(StatsDistribution), "CanAdd")]
        public static class StatsDistribution_CanAdd_Patch
        {
            public static void Postfix(ref bool __result, StatType attribute, StatsDistribution __instance)
            {
                var attributeMax = settings.characterCreationAbilityPointsMax;
                if (!__instance.Available)
                {
                    __result = false;
                }
                else
                {
                    if (attributeMax <= 18) attributeMax = 18;
                    var attributeValue = __instance.StatValues[attribute];
                    __result = attributeValue < attributeMax && __instance.GetAddCost(attribute) <= __instance.Points;
                }
            }
        }

        [HarmonyPatch(typeof(StatsDistribution), "GetAddCost")]
        public static class StatsDistribution_GetAddCost_Patch
        {
            public static bool Prefix(StatsDistribution __instance, StatType attribute)
            {
                var attributeValue = __instance.StatValues[attribute];
                return attributeValue > 7 && attributeValue < 17;
            }

            public static void Postfix(StatsDistribution __instance, ref int __result, StatType attribute)
            {
                var attributeValue = __instance.StatValues[attribute];
                if (attributeValue <= 7) __result = 2;
                if (attributeValue >= 17) __result = 4;
            }
        }

        [HarmonyPatch(typeof(StatsDistribution), "GetRemoveCost")]
        public static class StatsDistribution_GetRemoveCost_Patch
        {
            public static bool Prefix(StatsDistribution __instance, StatType attribute)
            {
                var attributeValue = __instance.StatValues[attribute];
                return attributeValue > 7 && attributeValue < 17;
            }

            public static void Postfix(StatsDistribution __instance, ref int __result, StatType attribute)
            {
                var attributeValue = __instance.StatValues[attribute];
                if (attributeValue <= 7)
                    __result = -2;
                else if (attributeValue >= 17) __result = -4;
            }
        }


        [HarmonyPatch(typeof(KingdomEvent), "ForceFinalResolve")]
        public static class KingdomEvent_ForceFinalResolve_Patch
        {
            public static bool Prefix(KingdomEvent __instance, ref EventResult.MarginType margin,
                ref AlignmentMaskType? overrideAlignment)
            {
                var alignmentString = settings.selectedKingdomAlignmentTranslated.ToLowerInvariant();
                overrideAlignment = Main.GetAlignment(alignmentString,
                    overrideAlignment ?? KingdomState.Instance.Alignment.ToMask());


                if (settings.toggleKingdomEventResultSuccess == Storage.isTrueString)
                {
                    var overrideMargin = Main.GetOverrideMargin(__instance);
                    if (overrideMargin == EventResult.MarginType.Success ||
                        overrideMargin == EventResult.MarginType.GreatSuccess) margin = overrideMargin;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(KingdomEvent), "Resolve", typeof(int), typeof(AlignmentMaskType), typeof(LeaderType))]
        public static class KingdomEvent_Resolve_Patch
        {
            public static bool Prefix(KingdomEvent __instance, ref int checkMargin, ref AlignmentMaskType alignment)
            {
                var alignmentString = settings.selectedKingdomAlignmentTranslated.ToLowerInvariant();
                alignment = Main.GetAlignment(alignmentString, alignment);


                if (settings.toggleKingdomEventResultSuccess == Storage.isTrueString)
                {
                    var overrideMargin = Main.GetOverrideMargin(__instance);
                    if (overrideMargin == EventResult.MarginType.Success ||
                        overrideMargin == EventResult.MarginType.GreatSuccess)
                        checkMargin = EventResult.MarginToInt(overrideMargin);
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(EquipmentRestrictionAlignment), "CanBeEquippedBy")]
        public static class EquipmentRestrictionAlignment_CanBeEquippedBy_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleEquipmentRestrictions == Storage.isTrueString) __result = true;
            }
        }

        [HarmonyPatch(typeof(EquipmentRestrictionClass), "CanBeEquippedBy")]
        public static class EquipmentRestrictionClassNew_CanBeEquippedBy_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleEquipmentRestrictions == Storage.isTrueString) __result = true;
            }
        }

        [HarmonyPatch(typeof(EquipmentRestrictionStat), "CanBeEquippedBy")]
        public static class EquipmentRestrictionStat_CanBeEquippedBy_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleEquipmentRestrictions == Storage.isTrueString) __result = true;
            }
        }

        [HarmonyPatch(typeof(ItemEntityArmor), "CanBeEquippedInternal")]
        public static class ItemEntityArmor_CanBeEquippedInternal_Patch
        {
            public static void Postfix(ItemEntityArmor __instance, UnitDescriptor owner, ref bool __result)
            {
                if (settings.toggleEquipmentRestrictions == Storage.isTrueString)
                {
                    var blueprint = __instance.Blueprint as BlueprintItemEquipment;
                    __result = blueprint == null ? false : blueprint.CanBeEquippedBy(owner);
                }
            }
        }

        [HarmonyPatch(typeof(ItemEntityShield), "CanBeEquippedInternal")]
        public static class ItemEntityShield_CanBeEquippedInternal_Patch
        {
            public static void Postfix(ItemEntityShield __instance, UnitDescriptor owner, ref bool __result)
            {
                if (settings.toggleEquipmentRestrictions == Storage.isTrueString)
                {
                    var blueprint = __instance.Blueprint as BlueprintItemEquipment;
                    __result = blueprint == null ? false : blueprint.CanBeEquippedBy(owner);
                }
            }
        }

        [HarmonyPatch(typeof(ItemEntityWeapon), "CanBeEquippedInternal")]
        public static class ItemEntityWeapon_CanBeEquippedInternal_Patch
        {
            public static void Postfix(ItemEntityWeapon __instance, UnitDescriptor owner, ref bool __result)
            {
                if (settings.toggleEquipmentRestrictions == Storage.isTrueString)
                {
                    var blueprint = __instance.Blueprint as BlueprintItemEquipment;
                    __result = blueprint == null ? false : blueprint.CanBeEquippedBy(owner);
                }
            }
        }

        [HarmonyPatch(typeof(BlueprintAnswerBase), "IsAlignmentRequirementSatisfied", MethodType.Getter)]
        public static class BlueprintAnswerBase_IsAlignmentRequirementSatisfied_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleDialogRestrictions == Storage.isTrueString) __result = true;
            }
        }

        [HarmonyPatch(typeof(BlueprintSettlementBuilding), "CheckRestrictions", typeof(SettlementState))]
        public static class BlueprintSettlementBuilding_CheckRestrictions_Patch1
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleSettlementRestrictions == Storage.isTrueString) __result = true;
            }
        }

        [HarmonyPatch(typeof(BlueprintSettlementBuilding), "CheckRestrictions", typeof(SettlementState),
            typeof(SettlementGridTopology.Slot))]
        public static class BlueprintSettlementBuilding_CheckRestrictions_Patch2
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleSettlementRestrictions == Storage.isTrueString) __result = true;
            }
        }


        [HarmonyPatch(typeof(ClickGroundHandler), "RunCommand")]
        public static class ClickGroundHandler_RunCommand_Patch
        {
            public static bool Prefix(ref bool __state)
            {
                __state = settings.toggleMoveSpeedAsOne == Storage.isTrueString;
                return !__state;
            }

            public static void Postfix(bool __state, UnitEntityData unit, Vector3 p, float? speedLimit,
                float orientation, float delay)
            {
                if (__state)
                {
                    var unitMoveTo = new UnitMoveTo(p, 0.3f);
                    unitMoveTo.MovementDelay = delay;
                    unitMoveTo.Orientation = orientation;
                    unitMoveTo.SpeedLimit = speedLimit;
                    unitMoveTo.OverrideSpeed = speedLimit;
                    unit.Commands.Run(unitMoveTo);
                    if (unit.Commands.Queue.FirstOrDefault(c => c is UnitMoveTo) == unitMoveTo || Game.Instance.IsPaused
                    ) ClickGroundHandler.ShowDestination(unit, unitMoveTo.Target, false);
                }
            }
        }

        [HarmonyPatch(typeof(ClickGroundHandler), "MoveSelectedUnitsToPoint", typeof(Vector3), typeof(Vector3),
            typeof(bool), typeof(float), typeof(Action<UnitEntityData, Vector3, float?, float, float>))]
        public static class ClickGroundHandler_MoveSelectedUnitsToPoint_Patche
        {
            public static bool Prefix(ref bool __state)
            {
                __state = settings.toggleMoveSpeedAsOne == Storage.isTrueString;
                return !__state;
            }

            public static void Postfix(bool __state, ClickGroundHandler __instance, Vector3 worldPosition,
                Vector3 direction, bool preview, float formationSpaceFactor,
                Action<UnitEntityData, Vector3, float?, float, float> commandRunner)
            {
                if (__state)
                {
                    if (!preview) Traverse.Create(__instance).Field("m_UnitWaitAgentList").Method("Clear").GetValue();
                    var selectedUnits = Game.Instance.UI.SelectionManager.GetSelectedUnits();
                    List<UnitEntityData> allUnits;
                    if (selectedUnits.Count == 1)
                        allUnits = selectedUnits;
                    else
                        allUnits = Game.Instance.Player.ControllableCharacters.Where(c => c.IsDirectlyControllable)
                            .ToList();
                    var orientation = Mathf.Atan2(direction.x, direction.z) * 57.29578f;
                    var speedLimit = 0f;
                    if (selectedUnits.Count >= 1)
                    {
                        speedLimit = selectedUnits.Aggregate(float.MinValue,
                            (current, u) => Mathf.Max(current <= 0f ? 0f : current, u.ModifiedSpeedMps));

                        if (Strings.ToBool(settings.togglePartyMovementSpeedMultiplier))
                            speedLimit = speedLimit * settings.partyMovementSpeedMultiplierValue;
                    }

                    if (selectedUnits.Count <= 0) return;
                    var array = new int[allUnits.Count];
                    for (var i = 0; i < array.Length; i++) array[i] = i;
                    Array.Sort(array,
                        (o1, o2) => (allUnits[o1].Position - worldPosition).sqrMagnitude.CompareTo(
                            (allUnits[o2].Position - worldPosition).sqrMagnitude));
                    PartyFormationHelper.FillFormationPositions(worldPosition, FormationAnchor.Front, direction,
                        allUnits, selectedUnits, formationSpaceFactor);
                    var count = 0;
                    for (var i = 0; i < allUnits.Count; i++)
                    {
                        var unit = allUnits[i];
                        if (!selectedUnits.HasItem(unit)) continue;
                        if (preview)
                        {
                            ClickGroundHandler.ShowDestination(unit, PartyFormationHelper.ResultPositions[i], true);
                        }
                        else
                        {
                            if (commandRunner == null)
                                Traverse.Create<ClickGroundHandler>().Method("RunCommand", unit,
                                    PartyFormationHelper.ResultPositions[i],
                                    speedLimit > unit.CurrentSpeedMps ? speedLimit : unit.CurrentSpeedMps, orientation,
                                    array[count] * 0.05f).GetValue();
                            else
                                commandRunner(unit, PartyFormationHelper.ResultPositions[i],
                                    speedLimit > unit.CurrentSpeedMps ? speedLimit : unit.CurrentSpeedMps, orientation,
                                    array[count] * 0.05f);
                        }

                        count++;
                    }

                    var previousMagnitude = 0f;
                    for (var i = 0; i < allUnits.Count; i++)
                    {
                        var unit = allUnits[i];
                        if (!selectedUnits.HasItem(unit)) continue;
                        var currentMagnitude =
                            (worldPosition - PartyFormationHelper.ResultPositions[i]).To2D().magnitude;
                        if (currentMagnitude > previousMagnitude) previousMagnitude = currentMagnitude;
                    }

                    for (var i = 0; i < selectedUnits.Count; i++)
                    {
                        var selectedUnit = selectedUnits[i];
                        if (allUnits.HasItem(selectedUnit)) continue;
                        var vector = selectedUnits.Count == 1
                            ? worldPosition
                            : GeometryUtils.ProjectToGround(
                                worldPosition - direction.normalized * (previousMagnitude + 2f));
                        if (preview)
                        {
                            ClickGroundHandler.ShowDestination(selectedUnit, vector, true);
                        }
                        else
                        {
                            if (commandRunner == null)
                                Traverse.Create<ClickGroundHandler>().Method("RunCommand", selectedUnit, vector,
                                    speedLimit > selectedUnit.CurrentSpeedMps
                                        ? speedLimit
                                        : selectedUnit.CurrentSpeedMps, orientation, 0f).GetValue();
                            else
                                commandRunner(selectedUnit, vector,
                                    speedLimit > selectedUnit.CurrentSpeedMps
                                        ? speedLimit
                                        : selectedUnit.CurrentSpeedMps, orientation, 0.0f);
                        }
                    }

                    if (preview)
                        Game.Instance.UI.ClickPointerManager.ShowPreviewArrow(worldPosition, direction);
                    else
                        Game.Instance.UI.ClickPointerManager.CancelPreview();

                    EventBus.RaiseEvent<IClickActionHandler>(h => h.OnMoveRequested(worldPosition));
                }
            }
        }

        [HarmonyPatch(typeof(ItemEntity), "SpendCharges", typeof(UnitDescriptor))]
        public static class ItemEntity_SpendCharges_Patch
        {
            public static bool Prefix(ref bool __state)
            {
                __state = settings.toggleInfiniteItems == Storage.isTrueString;
                return !__state;
            }

            public static void Postfix(bool __state, ItemEntity __instance, ref bool __result, UnitDescriptor user)
            {
                if (__state)
                {
                    var blueprintItemEquipment = __instance.Blueprint as BlueprintItemEquipment;
                    if (!blueprintItemEquipment || !blueprintItemEquipment.GainAbility)
                    {
                        __result = false;
                        return;
                    }

                    if (!__instance.IsSpendCharges)
                    {
                        __result = true;
                        return;
                    }

                    var hasNoCharges = false;
                    if (__instance.Charges > 0)
                    {
                        var itemEntityUsable =
                            new ItemEntityUsable((BlueprintItemEquipmentUsable) __instance.Blueprint);
                        if (user.State.Features.HandOfMagusDan &&
                            itemEntityUsable.Blueprint.Type == UsableItemType.Scroll)
                        {
                            var ruleRollDice = new RuleRollDice(user.Unit, new DiceFormula(1, DiceType.D100));
                            Rulebook.Trigger(ruleRollDice);
                            if (ruleRollDice.Result <= 25)
                            {
                                __result = true;
                                return;
                            }
                        }

                        if (user.IsPlayerFaction)
                        {
                            __result = true;
                            return;
                        }

                        --__instance.Charges;
                    }
                    else
                    {
                        hasNoCharges = true;
                    }

                    if (__instance.Charges >= 1 || blueprintItemEquipment.RestoreChargesOnRest)
                    {
                        __result = !hasNoCharges;
                        return;
                    }

                    if (__instance.Count > 1)
                    {
                        __instance.DecrementCount(1);
                        __instance.Charges = 1;
                    }
                    else
                    {
                        var collection = __instance.Collection;
                        collection?.Remove(__instance);
                    }

                    __result = !hasNoCharges;
                }
            }
        }

        [HarmonyPatch(typeof(AbilityTargetsAround), "Select")]
        public static class AbilityTargetsAround_Select_Patch
        {
            public static void Postfix(ref IEnumerable<TargetWrapper> __result, AbilityTargetsAround __instance,
                ConditionsChecker ___m_Condition, AbilityExecutionContext context, TargetWrapper anchor)
            {
                if (settings.toggleFfAOE == Storage.isTrueString)
                {
                    var caster = context.MaybeCaster;
                    var targets = GameHelper.GetTargetsAround(anchor.Point, __instance.AoERadius);
                    if (caster == null)
                    {
                        __result = Enumerable.Empty<TargetWrapper>();
                        return;
                    }

                    switch (__instance.TargetType)
                    {
                        case TargetType.Enemy:
                            targets = targets.Where(caster.IsEnemy);
                            break;
                        case TargetType.Ally:
                            targets = targets.Where(caster.IsAlly);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                        case TargetType.Any:
                            break;
                    }

                    if (___m_Condition.HasConditions)
                        targets = targets.Where(u =>
                        {
                            using (context.GetDataScope(u))
                            {
                                return ___m_Condition.Check();
                            }
                        }).ToList();
                    if (caster.IsPlayerFaction &&
                        (context.AbilityBlueprint.EffectOnAlly == AbilityEffectOnUnit.Harmful ||
                         context.AbilityBlueprint.EffectOnEnemy == AbilityEffectOnUnit.Harmful))
                    {
                        if (context.AbilityBlueprint.HasLogic<AbilityUseOnRest>())
                        {
                            var componentType = context.AbilityBlueprint.GetComponent<AbilityUseOnRest>().Type;
                            var healDamage = componentType == AbilityUseOnRestType.HealDamage ||
                                             componentType == AbilityUseOnRestType.HealDamage;
                            targets = targets.Where(target =>
                            {
                                if (target.IsPlayerFaction && !healDamage)
                                {
                                    var forUndead = componentType == AbilityUseOnRestType.HealMassUndead ||
                                                    componentType == AbilityUseOnRestType.HealSelfUndead ||
                                                    componentType == AbilityUseOnRestType.HealUndead;
                                    return forUndead == target.Descriptor.IsUndead;
                                }

                                return true;
                            });
                        }
                        else
                        {
                            targets = targets.Where(target => !target.IsPlayerFaction);
                        }
                    }

                    __result = targets.Select(target => new TargetWrapper(target));
                }
            }
        }


        [HarmonyPatch(typeof(RuleDealDamage), "ApplyDifficultyModifiers")]
        public static class RuleDealDamage_ApplyDifficultyModifiers_Patch
        {
            public static void Postfix(ref int __result, RuleDealDamage __instance, int damage)
            {
                if (settings.toggleFfAny == Storage.isTrueString)
                {
                    var blueprint = __instance.Reason.Context?.AssociatedBlueprint;
                    if (!(blueprint is BlueprintBuff))
                    {
                        var blueprintAbility = __instance.Reason.Context?.SourceAbility;
                        if (blueprintAbility != null &&
                            __instance.Initiator.IsPlayerFaction &&
                            __instance.Target.IsPlayerFaction &&
                            (blueprintAbility.EffectOnAlly == AbilityEffectOnUnit.Harmful ||
                             blueprintAbility.EffectOnEnemy == AbilityEffectOnUnit.Harmful))
                            __result = 0;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MetamagicHelper), "DefaultCost")]
        public static class MetamagicHelper_DefaultCost_Patch
        {
            public static void Postfix(ref int __result)
            {
                if (settings.toggleMetamagic == Storage.isTrueString) __result = 0;
            }
        }

        [HarmonyPatch(typeof(RuleCollectMetamagic), "AddMetamagic")]
        public static class RuleCollectMetamagic_AddMetamagic_Patch
        {
            public static bool Prefix()
            {
                return settings.toggleMetamagic != Storage.isTrueString;
            }

            public static void Postfix(ref RuleCollectMetamagic __instance, int ___m_SpellLevel,
                Feature metamagicFeature)
            {
                if (settings.toggleMetamagic == Storage.isTrueString)
                {
                    var addMetamagicFeat = metamagicFeature.Get<AddMetamagicFeat>();
                    if (addMetamagicFeat == null)
                    {
                        UberDebug.LogWarning("Trying to add metamagic feature without metamagic component: {0}",
                            metamagicFeature);
                    }
                    else
                    {
                        var metamagic = addMetamagicFeat.Metamagic;
                        __instance.KnownMetamagics.Add(metamagicFeature);
                        if (___m_SpellLevel >= 0 && ___m_SpellLevel + addMetamagicFeat.Metamagic.DefaultCost() <= 9 &&
                            !__instance.SpellMetamagics.Contains(metamagicFeature) &&
                            (__instance.Spell.AvailableMetamagic & metamagic) == metamagic)
                            __instance.SpellMetamagics.Add(metamagicFeature);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AbilityData), "RequireMaterialComponent", MethodType.Getter)]
        public static class AbilityData_RequireMaterialComponent_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleMaterialComponent == Storage.isTrueString) __result = false;
            }
        }

        [HarmonyPatch(typeof(PrerequisiteAlignment), "Check")]
        public static class PrerequisiteAlignment_Check_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleIgnoreClassAlignment == Storage.isTrueString) __result = true;
            }
        }

        [HarmonyPatch(typeof(PrerequisiteNoFeature), "Check")]
        public static class PrerequisiteNoFeature_Check_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleIgnoreForbiddenFeatures == Storage.isTrueString) __result = true;
            }
        }

        [HarmonyPatch(typeof(Spellbook), "AddCasterLevel")]
        public static class Spellbook_AddCasterLevel_Patch
        {
            public static bool Prefix()
            {
                return false;
            }

            public static void Postfix(ref Spellbook __instance, ref int ___m_CasterLevelInternal,
                List<BlueprintSpellList> ___m_SpecialLists)
            {
                var maxSpellLevel = __instance.MaxSpellLevel;
                ___m_CasterLevelInternal += settings.addCasterLevel;
                var maxSpellLevel2 = __instance.MaxSpellLevel;
                if (__instance.Blueprint.AllSpellsKnown)
                {
                    var addSpecialMethod = Traverse.Create(__instance)
                        .Method("AddSpecial", new[] {typeof(int), typeof(BlueprintAbility)});
                    for (var i = maxSpellLevel + 1; i <= maxSpellLevel2; i++)
                    {
                        foreach (var spell in __instance.Blueprint.SpellList.GetSpells(i))
                            __instance.AddKnown(i, spell);
                        foreach (var specialList in ___m_SpecialLists)
                        foreach (var spell2 in specialList.GetSpells(i))
                            addSpecialMethod.GetValue(i, spell2);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SpellSelectionData), "CanSelectAnything")]
        public static class SpellSelectionData_CanSelectAnything_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (settings.toggleSkipSpellSelection == Storage.isTrueString) __result = false;
            }
        }

        [HarmonyPatch(typeof(GlobalMapLocation), "HandleHoverChange", typeof(bool))]
        internal static class GlobalMapLocation_HandleHoverChange_Patch
        {
            private static void Postfix(GlobalMapLocation __instance, ref bool isHover)
            {
                if (Main.enabled && isHover)
                    Storage.lastHoveredLocation = __instance;
                else
                    Storage.lastHoveredLocation = null;
            }
        }

        [HarmonyPatch(typeof(UnityModManager.UI), "Update")]
        internal static class UnityModManager_UI_Update_Patch
        {
            private static void Postfix(UnityModManager.UI __instance, ref Rect ___mWindowRect,
                ref Vector2[] ___mScrollPosition, ref int ___tabId)
            {
                Storage.ummRect = ___mWindowRect;
                //Storage.ummWidth = ___mWindowWidth; float ___mWindowWidth
                Storage.ummScrollPosition = ___mScrollPosition;
                Storage.ummTabId = ___tabId;

                if (Main.enabled)
                {
                    var currentGameMode = Game.Instance.CurrentMode;

                    if ((currentGameMode == GameModeType.Default || currentGameMode == GameModeType.EscMode) &&
                        Strings.ToBool(settings.toggleShowAreaName) && !Game.Instance.IsPaused)
                    {
                        Main.sceneAreaInfo.On();
                        Main.sceneAreaInfo.Text("<b>" + Game.Instance.CurrentlyLoadedArea.AreaName + "</b>");
                    }
                    else
                    {
                        Main.sceneAreaInfo.Off();
                    }

                    if (currentGameMode != GameModeType.None && Strings.ToBool(settings.toggleDisplayObjectInfo))
                    {
                        if (Main.sceneAreaInfo.baseGameObject.activeSelf)
                        {
                            var temp = Main.sceneAreaInfo.baseGameObject.GetComponent<RectTransform>().position;
                            temp.y = Main.sceneAreaInfo.baseGameObject.GetComponent<RectTransform>().position.y -
                                     Main.sceneAreaInfo.baseGameObject.GetComponent<RectTransform>().sizeDelta.y;
                            Main.objectInfo.baseGameObject.GetComponent<RectTransform>().position = temp;
                        }
                        else
                        {
                            Main.objectInfo.baseGameObject.GetComponent<RectTransform>().position =
                                new Vector3(Screen.width / 2f, Screen.height * 0.95f, 0);
                        }

                        Main.objectInfo.On();
                        BlueprintScriptableObject blueprint;
                        var unitUnderMouse = Common.GetUnitUnderMouse();
                        var scriptableObjectArray = ActionKey.Tooltip();
                        if (unitUnderMouse != null)
                        {
                            blueprint = unitUnderMouse.Blueprint;
                        }
                        else
                        {
                            if (scriptableObjectArray == null) return;
                            blueprint = scriptableObjectArray[0];
                        }

                        Main.objectInfo.Text($"<b>{blueprint.AssetGuid}\n{Utilities.GetBlueprintName(blueprint)}</b>");
                    }
                    else
                    {
                        Main.objectInfo.Off();
                    }


                    if (settings.toggleEnableFocusCamera == Storage.isTrueString &&
                        Game.Instance?.UI?.GetCameraRig() != null &&
                        (currentGameMode == GameModeType.Default || currentGameMode == GameModeType.Pause))
                    {
                        var partyMembers = Game.Instance.Player?.ControllableCharacters;

                        if (partyMembers != Storage.partyMembersFocusUnits)
                            Storage.partyMembersFocusUnits = partyMembers;

                        if (settings.partyMembersFocusPositionCounter >= Storage.partyMembersFocusUnits.Count)
                            settings.partyMembersFocusPositionCounter = Storage.partyMembersFocusUnits.Count - 1;

                        if (Input.GetKeyDown(settings.focusCameraKey))
                        {
                            if (settings.focusCameraToggle)
                                settings.focusCameraToggle = false;
                            else
                                settings.focusCameraToggle = true;
                        }

                        if (settings.focusCameraToggle && !Input.GetKey(KeyCode.Mouse2))
                            Game.Instance.UI.GetCameraRig().ScrollTo(Storage
                                .partyMembersFocusUnits[settings.partyMembersFocusPositionCounter].Position);

                        if (Input.GetKeyDown(settings.focusCylceKey))
                        {
                            if (settings.partyMembersFocusPositionCounter < Storage.partyMembersFocusUnits.Count)
                            {
                                if (settings.partyMembersFocusPositionCounter ==
                                    Storage.partyMembersFocusUnits.Count - 1)
                                    settings.partyMembersFocusPositionCounter = 0;
                                else
                                    settings.partyMembersFocusPositionCounter++;
                            }
                            else
                            {
                                settings.partyMembersFocusPositionCounter = 0;
                            }
                        }
                    }

                    if (Input.GetKeyDown(settings.togglePartyAlwaysRoll20Key) &&
                        settings.toggleEnablePartyAlwaysRoll20Hotkey == Storage.isTrueString)
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

                    if (Input.GetKeyDown(settings.resetCutsceneLockKey) &&
                        settings.toggleEnableResetCutsceneLockHotkey == Storage.isTrueString)
                    {
                        Game.Instance.CheatResetCutsceneLock();
                        Common.AddLogEntry(
                            Strings.GetText("button_ResetCutsceneLock") + ": " + Strings.GetText("logMessage_Enabled"),
                            Color.black);
                    }


                    if (Input.GetKeyDown(settings.actionKey) &&
                        settings.toggleEnableActionKey == Storage.isTrueString && settings.actionKeyIndex == 6 &&
                        ActionKey.teleportUnit != null && ActionKey.teleportUnit.IsInGame)
                    {
                        var currentMode = Game.Instance.CurrentMode;
                        if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
                        {
                            var mousePosition = Common.MousePositionLocalMap();
                            Common.TeleportUnit(ActionKey.teleportUnit, mousePosition);
                        }

                        ActionKey.teleportUnit = null;
                    }

                    if (Input.GetKeyDown(settings.actionKey) &&
                        settings.toggleEnableActionKey == Storage.isTrueString && settings.actionKeyIndex == 8 &&
                        ActionKey.rotateUnit != null && ActionKey.rotateUnit.IsInGame)
                    {
                        var currentMode = Game.Instance.CurrentMode;
                        if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
                        {
                            var mousePosition = Common.MousePositionLocalMap();
                            Common.RotateUnit(ActionKey.rotateUnit, mousePosition);
                        }

                        ActionKey.rotateUnit = null;
                    }

                    if (Input.GetKeyDown(settings.actionKey) && Strings.ToBool(settings.toggleEnableActionKey) &&
                        settings.actionKeyIndex != 0) ActionKey.Functions(settings.actionKeyIndex);


                    if (Strings.ToBool(settings.toggleHUDToggle) && Input.GetKeyDown(settings.hudToggleKey))
                        Common.ToggleHUD();


                    if (Input.GetKey(settings.teleportKey) && settings.toggleEnableTeleport == Storage.isTrueString)
                    {
                        var currentMode = Game.Instance.CurrentMode;
                        if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
                        {
                            var selectedUnits = Game.Instance.UI.SelectionManager.GetSelectedUnits();

                            foreach (var unit in selectedUnits)
                            {
                                var mousePosition = Common.MousePositionLocalMap();
                                Common.TeleportUnit(unit, mousePosition);
                            }
                        }
                        else if (currentMode == GameModeType.GlobalMap && Storage.lastHoveredLocation != null)
                        {
                            GlobalMapRules.Instance.TeleportParty(Storage.lastHoveredLocation.Blueprint);
                            Storage.lastHoveredLocation = null;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RandomEncountersController), "GetAvoidanceCheckResult")]
        internal static class RandomEncountersControllern_GetAvoidanceCheckResult_Patch
        {
            private static void Postfix(ref RandomEncounterAvoidanceCheckResult __result)
            {
                if (settings.toggleEnableAvoidanceSuccess == Storage.isTrueString)
                    __result = RandomEncounterAvoidanceCheckResult.Success;
            }
        }

        [HarmonyPatch(typeof(GroupCharacterPortraitController), "OnUnitSelectionAdd")]
        internal static class GroupCharacterPortraitController_OnUnitSelectionAdd_Patch
        {
            private static void Postfix(UnitEntityData selected)
            {
                if (Strings.ToBool(settings.toggleEnableFocusCamera) &&
                    Strings.ToBool(settings.toggleEnableFocusCameraSelectedUnit))
                    settings.partyMembersFocusPositionCounter =
                        Storage.partyMembersFocusUnits.FindIndex(a => a == selected);
            }
        }

        [HarmonyPatch(typeof(UnityModManager), "SaveSettingsAndParams")]
        internal static class UnityModManager_SaveSettingsAndParams_Patch
        {
            private static void Postfix(UnityModManager __instance)
            {
                if (Storage.itemFavourites.Any())
                    Storage.itemFavourites.SerializeListString(
                        Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesItemsFile);
                if (Storage.buffFavourites.Any())
                    Storage.buffFavourites.SerializeListString(
                        Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesBuffsFile);
                if (Storage.featFavourites.Any())
                    Storage.featFavourites.SerializeListString(
                        Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesFeatsFile);
                if (Storage.abilitiesFavourites.Any())
                    Storage.abilitiesFavourites.SerializeListString(
                        Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesAbilitiesFile);
                if (Storage.togglesFavourites.Any())
                    Storage.togglesFavourites.SerializeListString(
                        Storage.modEntryPath + Storage.favouritesFolder + "\\" + Storage.favouritesTogglesFile);
                if (settings.toggleEnableTaxCollector == Storage.isTrueString)
                    TaxCollector.Serialize(Main.taxSettings,
                        Storage.modEntryPath + Storage.taxCollectorFolder + "\\" + Storage.taxCollectorFile);
            }
        }

        [HarmonyPatch(typeof(CameraRig), "SetRotation")]
        private static class CameraRig_SetRotation_Patch
        {
            private static bool Prefix(ref float cameraRotation)
            {
                if (settings.toggleEnableCameraRotation == Storage.isTrueString)
                {
                    if (cameraRotation != settings.defaultRotation)
                    {
                        // If we enter a new area with a different default camera angle, reset the rotation to 0
                        settings.defaultRotation = cameraRotation;
                        settings.cameraRotation = 0;
                    }

                    cameraRotation += settings.cameraRotation;
                    Main.rotationChanged = true;
                    if (Main.localMap)
                        // If the local map is open, call the Set method to redraw things
                        Traverse.Create(Main.localMap).Method("Set").GetValue();
                    return true;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CameraRig), "TickScroll")]
        private static class CameraKey_Patch
        {
            private static bool Prefix(ref float ___m_ScrollSpeed)
            {
                if (Main.enabled && settings.toggleEnableCameraScrollSpeed == Storage.isTrueString)
                    unsafe
                    {
                        fixed (float* pointer = &___m_ScrollSpeed)
                        {
                            Main.cameraScrollSpeed = pointer;
                        }
                    }

                if (settings.toggleEnableCameraRotation == Storage.isTrueString)
                {
                    if (Input.GetKey(settings.cameraTurnLeft))
                    {
                        settings.cameraRotation -= settings.cameraTurnRate;
                        if (settings.cameraRotation < -180) settings.cameraRotation += 360;
                    }
                    else if (Input.GetKey(settings.cameraTurnRight))
                    {
                        settings.cameraRotation += settings.cameraTurnRate;
                        if (settings.cameraRotation >= 180) settings.cameraRotation -= 360;
                    }
                    else if (Input.GetKey(settings.cameraReset))
                    {
                        settings.cameraRotation = 0;
                    }
                    else
                    {
                        return true;
                    }

                    HarmonyInstance.Create("kingmaker.camerarotation").Patch(
                        AccessTools.Method(typeof(CameraRig), "SetRotation"),
                        new HarmonyMethod(typeof(Main).GetMethod("CameraRig_SetRotation_Patch")));
                    Game.Instance.UI.GetCameraRig().SetRotation(settings.defaultRotation);
                    return true;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(LocalMap), "OnShow")]
        private static class LocalMap_OnShow_Patch
        {
            private static void Prefix(LocalMap __instance)
            {
                Main.localMap = __instance;
            }
        }

        [HarmonyPatch(typeof(LocalMap), "OnHide")]
        private static class LocalMap_OnHide_Patch
        {
            private static void Postfix()
            {
                Main.localMap = null;
            }
        }

        [HarmonyPatch(typeof(LocalMapRenderer), "IsDirty")]
        private static class LocalMapRenderer_IsDirty_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Main.rotationChanged)
                {
                    // If rotation has changed since drawing the map image, return that it's dirty.
                    __result = true;
                    Main.rotationChanged = false;
                }
            }
        }

        [HarmonyPatch(typeof(CameraZoom))]
        [HarmonyPatch("TickZoom")]
        public static class CameraZoom_TickZoom_Patch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                if (settings.toggleEnableCameraZoom == Storage.isTrueString)
                {
                    var foundFovMin = -1;
                    var foundFovMax = -1;

                    for (var i = 0; i < codes.Count; i++)
                        if (codes[i].opcode == OpCodes.Callvirt && codes[i - 1].opcode == OpCodes.Call &&
                            codes[i - 2].opcode == OpCodes.Call)
                        {
                            foundFovMin = i - 4;
                            foundFovMax = i - 6;
                            break;
                        }

                    codes[foundFovMin - 1].opcode = OpCodes.Nop;
                    codes[foundFovMin].opcode = OpCodes.Ldc_R4;
                    codes[foundFovMin].operand = float.Parse(settings.savedFovMin);

                    codes[foundFovMax - 1].opcode = OpCodes.Nop;
                    codes[foundFovMax].opcode = OpCodes.Ldc_R4;
                    codes[foundFovMax].operand = float.Parse(settings.savedFovMax);

                    return codes.AsEnumerable();
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(CameraRig))]
        [HarmonyPatch("SetMapMode")]
        public static class CameraRig_SetMapMode_Patch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                if (settings.toggleEnableCameraZoom == Storage.isTrueString)
                {
                    var foundFovGlobalMap = -1;

                    for (var i = 0; i < codes.Count; i++)
                        if (codes[i].opcode == OpCodes.Br && codes[i - 1].opcode == OpCodes.Ldfld &&
                            codes[i - 2].opcode == OpCodes.Call && codes[i - 3].opcode == OpCodes.Ldarg_0)
                        {
                            foundFovGlobalMap = i - 1;
                            break;
                        }

                    codes[foundFovGlobalMap - 2].opcode = OpCodes.Nop;
                    codes[foundFovGlobalMap - 1].opcode = OpCodes.Nop;
                    codes[foundFovGlobalMap].opcode = OpCodes.Ldc_R4;
                    codes[foundFovGlobalMap].operand = float.Parse(settings.savedFovGlobalMap, Storage.cultureEN);

                    return codes.AsEnumerable();
                }

                return codes.AsEnumerable();
            }
        }

        //UI start
        [HarmonyPatch(typeof(EscMenuWindow), "Initialize")]
        internal static class EscMenuWindow_Initialize_Patch
        {
            private static void Postfix(EscMenuWindow __instance)
            {
                if (Strings.ToBool(settings.toggleUnityModManagerButton) &&
                    Traverse.Create(__instance).Field("UnityModManager_Button").GetValue<Button>() == null)
                    try
                    {
                        var saveButton = Traverse.Create(__instance).Field("ButtonSave").GetValue<Button>();
                        var saveButtonParent = saveButton.transform.parent;
                        var ummButton = Object.Instantiate(saveButton);
                        ummButton.name = "UnityModManager_Button";
                        ummButton.transform.SetParent(saveButtonParent, false);
                        ummButton.onClick = new Button.ButtonClickedEvent();
                        ummButton.onClick.AddListener(() => OnClick());
                        ummButton.GetComponentInChildren<TextMeshProUGUI>().text =
                            Strings.GetText("misc_UnityModManager");
                        ummButton.transform.SetSiblingIndex(ummButton.transform.GetSiblingIndex() -
                                                            Mathf.RoundToInt(settings.unityModManagerButtonIndex));
                    }
                    catch (Exception exception)
                    {
                        modLogger.Log(exception.ToString());
                    }
            }

            public static void OnClick()
            {
                try
                {
                    UnityModManager.UI.Instance.ToggleWindow(true);
                }
                catch (Exception e)
                {
                    modLogger.Log(e.ToString());
                    ;
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "Initialize")]
        internal static class Inventory_Initialize_Patch
        {
            private static void Postfix(Inventory __instance)
            {
                if (__instance != null)
                    if (Strings.ToBool(settings.toggleRepeatableLockPickingLockPicks) &&
                        Strings.ToBool(settings.toggleRepeatableLockPickingLockPicksInventoryText) &&
                        Traverse.Create(__instance).Field("LockPicks_Text").GetValue<TextMeshProUGUI>() == null)
                        try
                        {
                            var playerMoneyNow = Traverse.Create(__instance).Field("PlayerMoneyNow")
                                .GetValue<TextMeshProUGUI>();
                            var playerMoneyNowParent = playerMoneyNow.transform.parent;
                            Storage.lockPicksNow = Object.Instantiate(playerMoneyNow);
                            Storage.lockPicksNow.name = "LockPicks_Text";
                            Storage.lockPicksNow.richText = true;
                            Storage.lockPicksNow.transform.SetParent(playerMoneyNowParent.transform.parent, false);
                            Storage.lockPicksNow.transform.position = new Vector3(
                                playerMoneyNow.transform.position.x + 150f, playerMoneyNow.transform.position.y + 2,
                                playerMoneyNow.transform.position.z);
                            var lockPicksTextRectTransform = Storage.lockPicksNow.GetComponent<RectTransform>();
                            lockPicksTextRectTransform.sizeDelta = new Vector2(
                                lockPicksTextRectTransform.rect.width * 4, lockPicksTextRectTransform.rect.height);
                            Storage.lockPicksNow.text =
                                $"<size=90%><b>{Strings.GetText("label_LockPicks")}:</b> {Storage.lockPicks}</size>";
                        }
                        catch (Exception exception)
                        {
                            modLogger.Log(exception.ToString());
                        }
            }
        }

        [HarmonyPatch(typeof(Inventory), "OnShow")]
        internal static class Inventory_OnShow_Patch
        {
            private static void Postfix(Inventory __instance)
            {
                if (Strings.ToBool(settings.toggleRepeatableLockPickingLockPicks) &&
                    Strings.ToBool(settings.toggleRepeatableLockPickingLockPicksInventoryText))
                    try
                    {
                        Storage.lockPicksNow.text =
                            $"<size=90%><b>{Strings.GetText("label_LockPicks")}:</b> {Storage.lockPicks}</size>";
                    }
                    catch (Exception exception)
                    {
                        modLogger.Log(exception.ToString());
                    }
            }
        }

        [HarmonyPatch(typeof(Inventory), "UpdatePlayerMoneyAndInventoryWeight")]
        internal static class Inventory_UpdatePlayerMoneyAndInventoryWeight_Patch
        {
            private static void Postfix(Inventory __instance, TextMeshProUGUI ___PlayerMoneyNow)
            {
                if (Strings.ToBool(settings.toggleScaleInventoryMoney))
                    try
                    {
                        var digits = Math.Abs((int) Math.Floor(Math.Log10(player.Money)) + 1);
                        var s = player.Money.ToString();
                        switch (digits)
                        {
                            case 8:
                                ___PlayerMoneyNow.text = RichText.SizePercent(s, 80);
                                break;
                            case 9:
                                ___PlayerMoneyNow.text = RichText.SizePercent(s, 75);
                                break;
                            case 10:
                                ___PlayerMoneyNow.text = RichText.SizePercent(s, 70);
                                break;
                            case 11:
                                ___PlayerMoneyNow.text = RichText.SizePercent(s, 65);
                                break;
                            case 12:
                                ___PlayerMoneyNow.text = RichText.SizePercent(s, 60);
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        modLogger.Log(exception.ToString());
                    }
            }
        }
        //UI end

        [HarmonyPatch(typeof(Player), "OnAreaLoaded")]
        internal static class Player_OnAreaLoaded_Patch
        {
            private static void Postfix()
            {
                Main.ReloadPartyState();
                ActionKey.teleportUnit = null;
                ActionKey.rotateUnit = null;
            }
        }

        [HarmonyPatch(typeof(Player), "AttachPartyMember")]
        internal static class Player_AttachPartyMember_Patch
        {
            private static void Postfix()
            {
                Main.ReloadPartyState();
            }
        }

        [HarmonyPatch(typeof(Player), "AddCompanion")]
        internal static class Player_AddCompanion_Patch
        {
            private static void Postfix()
            {
                Main.ReloadPartyState();
            }
        }

        [HarmonyPatch(typeof(Player), "RemoveCompanion")]
        internal static class Player_RemoveCompanion_Patch
        {
            private static void Postfix()
            {
                Main.ReloadPartyState();
            }
        }

        [HarmonyPatch(typeof(Player), "DismissCompanion")]
        internal static class Player_DismissCompanion_Patch
        {
            private static void Postfix()
            {
                Main.ReloadPartyState();
            }
        }

        [HarmonyPatch(typeof(Player), "SwapAttachedAndDetachedPartyMembers")]
        internal static class Player_SwapAttachedAndDetachedPartyMembers_Patch
        {
            private static void Postfix()
            {
                Main.ReloadPartyState();
            }
        }

        [HarmonyPatch(typeof(EscMenuWindow), "OnHotKeyEscPressed")]
        internal static class EscMenuWindow_OnHotKeyEscPressed_Patch
        {
            private static void Postfix()
            {
                if (settings.toggleEnableTaxCollector == Storage.isTrueString)
                    try
                    {
                        TaxCollector.Serialize(Main.taxSettings,
                            Storage.modEntryPath + Storage.taxCollectorFolder + "\\" + Storage.taxCollectorFile);
                    }
                    catch (Exception e)
                    {
                        modLogger.Log(e.ToString());
                    }
            }
        }

        [HarmonyPatch(typeof(Player), "Initialize")]
        internal static class Game_Initialize_Patch
        {
            private static void Postfix()
            {
                Storage.defaultVendorSellPriceMultiplier = (float) Game.Instance.BlueprintRoot.Vendors.SellModifier;
                Main.CheckRandomEncounterSettings();
                if (settings.artisanMasterpieceChance != Defaults.artisanMasterpieceChance &&
                    KingdomRoot.Instance != null)
                    KingdomRoot.Instance.ArtisanMasterpieceChance = settings.artisanMasterpieceChance;
                if (Strings.ToBool(settings.toggleNoResourcesClaimCost) && KingdomRoot.Instance != null)
                    KingdomRoot.Instance.DefaultMapResourceCost = 0;
                if (settings.toggleVendorSellPriceMultiplier == Storage.isTrueString)
                    Game.Instance.BlueprintRoot.Vendors.SellModifier = settings.finalVendorSellPriceMultiplier;
                if (settings.sillyBloodChanceSaved != Defaults.sillyBloddChance)
                    Game.Instance.BlueprintRoot.Cheats.SillyBloodChance = settings.sillyBloodChanceSaved;
            }
        }

        [HarmonyPatch(typeof(LocationMaskRenderer), "OnAreaDidLoad")]
        private static class LocationMaskRenderer_OnAreaDidLoad_PostPatch
        {
            private static void Postfix()
            {
                settings.toggleFogOfWarBoolDefault = LocationMaskRenderer.Instance.FogOfWar.Enabled;

                if (settings.toggleOverwriteFogOfWar == Storage.isTrueString)
                    LocationMaskRenderer.Instance.FogOfWar.Enabled = settings.toggleFogOfWarBool;
            }
        }

        [HarmonyPatch(typeof(VendorLogic), "GetItemSellPrice")]
        private static class VendorLogic_GetItemSellPrice_Patch
        {
            private static void Postfix(ref long __result)
            {
                if (settings.toggleVendorsSellFor0 == Storage.isTrueString) __result = 0L;
            }
        }

        [HarmonyPatch(typeof(VendorLogic), "GetItemBuyPrice")]
        private static class VendorLogic_GetItemBuyPrice_Patch
        {
            private static void Postfix(ref long __result)
            {
                if (settings.toggleVendorsBuyFor0 == Storage.isTrueString) __result = 0L;
            }
        }

        [HarmonyPatch(typeof(LevelUpController), "CanLevelUp")]
        private static class LevelUpController_CanLevelUp_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (settings.toggleNoLevelUpRestirctions == Storage.isTrueString) __result = true;
            }
        }

        [HarmonyPatch(typeof(GameHistoryLog), "HandlePartyCombatStateChanged")]
        private static class GameHistoryLog_HandlePartyCombatStateChanged_Patch
        {
            private static void Postfix(ref bool inCombat)
            {
                if (!inCombat && settings.toggleRestoreSpellsAbilitiesAfterCombat == Storage.isTrueString)
                {
                    var partyMembers = Game.Instance?.Player.ControllableCharacters;
                    foreach (var u in partyMembers)
                    {
                        foreach (var resource in u.Descriptor.Resources)
                            u.Descriptor.Resources.Restore(resource);
                        foreach (var spellbook in u.Descriptor.Spellbooks)
                            spellbook.Rest();
                        u.Brain.RestoreAvailableActions();
                    }
                }

                if (!inCombat && settings.toggleInstantRestAfterCombat == Storage.isTrueString) Cheats.InstantRest();
                if (!inCombat && settings.toggleRestoreItemChargesAfterCombat == Storage.isTrueString)
                    Cheats.RestoreAllItemCharges();
            }
        }

        [HarmonyPatch(typeof(ApplyClassMechanics), "ApplyHitPoints")]
        private static class ApplyClassMechanics_ApplyHitPoints_Patch
        {
            private static void Postfix(ref LevelUpState state, ref ClassData classData, ref UnitDescriptor unit)
            {
                if (Strings.ToBool(settings.toggleFullHitdiceEachLevel) && unit.IsPlayerFaction)
                {
                    var hitDie = (int) classData.CharacterClass.HitDie;

                    unit.Stats.HitPoints.BaseValue += hitDie / 2;
                }
                else if (Strings.ToBool(settings.toggleRollHitDiceEachLevel) && unit.IsPlayerFaction)
                {
                    var hitDie = (int) classData.CharacterClass.HitDie;
                    var diceFormula = new DiceFormula(1, classData.CharacterClass.HitDie);

                    var roll = RulebookEvent.Dice.D(diceFormula);

                    unit.Stats.HitPoints.BaseValue -= hitDie / 2;
                    unit.Stats.HitPoints.BaseValue += roll;
                }
            }
        }

        [HarmonyPatch(typeof(PrerequisiteFeature), "Check")]
        private static class PrerequisiteFeature_CanLevelUp_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleIgnoreFeaturePrerequisites)) __result = true;
            }
        }

        [HarmonyPatch(typeof(PrerequisiteFeaturesFromList), "Check")]
        private static class PrerequisiteFeaturesFromList_CanLevelUp_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleIgnoreFeatureListPrerequisites)) __result = true;
            }
        }

        [HarmonyPatch(typeof(FeatureSelectionState), "IgnorePrerequisites", MethodType.Getter)]
        private static class FeatureSelectionState_IgnorePrerequisites_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleFeaturesIgnorePrerequisites)) __result = true;
            }
        }

        [HarmonyPatch(typeof(TimeController), "Tick")]
        private static class TimeController_Tick_Patch
        {
            public static bool Prefix()
            {
                if (settings.debugTimeMultiplier != Defaults.debugTimeScale &&
                    settings.useCustomDebugTimeMultiplier == false &&
                    Game.Instance.TimeController.DebugTimeScale != settings.debugTimeMultiplier)
                    Game.Instance.TimeController.DebugTimeScale = settings.debugTimeMultiplier;

                if (settings.finalCustomDebugTimeMultiplier != Defaults.debugTimeScale &&
                    settings.useCustomDebugTimeMultiplier &&
                    Game.Instance.TimeController.DebugTimeScale != settings.finalCustomDebugTimeMultiplier)
                    Game.Instance.TimeController.DebugTimeScale = settings.finalCustomDebugTimeMultiplier;
                return true;
            }
        }

        [HarmonyPatch(typeof(TimeController), "Tick")]
        public static class TimeController_Tick_Patch2
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var found = -1;

                for (var i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Stfld && codes[i - 1].opcode == OpCodes.Call &&
                        codes[i - 2].opcode == OpCodes.Call && codes[i - 3].opcode == OpCodes.Conv_R8 &&
                        codes[i - 4].opcode == OpCodes.Call && codes[i - 5].opcode == OpCodes.Ldarg_0 &&
                        codes[i - 6].opcode == OpCodes.Ldfld && codes[i - 7].opcode == OpCodes.Dup)
                    {
                        found = i;
                        break;
                    }


                if (found != -1 && Strings.ToBool(settings.toggleStopGameTime))
                {
                    codes[found - 5].opcode = OpCodes.Ldc_I4_0;
                    codes[found - 4].opcode = OpCodes.Nop;
                    return codes.AsEnumerable();
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(GlobalMapTeleport), "RunAction")]
        public static class GlobalMapTeleport_RunAction_Patch
        {
            public static bool Prefix(ref float ___SkipHours)
            {
                if (Strings.ToBool(settings.toggleStopGameTime)) ___SkipHours = 0f;
                return true;
            }
        }

        [HarmonyPatch(typeof(GlobalMapTeleportLast), "RunAction")]
        public static class GlobalMapTeleportLast_RunAction_Patch
        {
            public static bool Prefix(ref float ___SkipHours)
            {
                if (Strings.ToBool(settings.toggleStopGameTime)) ___SkipHours = 0f;
                return true;
            }
        }

        [HarmonyPatch(typeof(Game), "AdvanceGameTime")]
        public static class Game_AdvanceGameTime_Patch
        {
            public static bool Prefix(ref TimeSpan delta)
            {
                if (Strings.ToBool(settings.toggleStopGameTime)) delta = TimeSpan.Zero;
                return true;
            }
        }

        [HarmonyPatch(typeof(RuleDealDamage), "ApplyDifficultyModifiers")]
        public static class Game_RollDamage_PrePatch
        {
            public static bool Prefix(RuleDealDamage __instance, ref int damage)
            {
                if (Strings.ToBool(settings.toggleNoDamageFromEnemies) && __instance.Initiator.IsPlayersEnemy)
                    damage = 0;
                if (Strings.ToBool(settings.togglePartyOneHitKills) && __instance.Initiator.IsPlayerFaction)
                {
                    var unit = __instance.Target;
                    damage = unit.Descriptor.Stats.HitPoints.ModifiedValue +
                             unit.Descriptor.Stats.TemporaryHitPoints.ModifiedValue + 1;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(RuleDealDamage), "ApplyDifficultyModifiers")]
        private static class RuleDealDamage_ApplyDifficultyModifiers_PostPatch
        {
            private static void Postfix(RuleDealDamage __instance, ref int __result)
            {
                if (Strings.ToBool(settings.toggleDamageDealtMultipliers))
                {
                    if (Strings.ToBool(settings.toggleEnemiesDamageDealtMultiplier) &&
                        __instance.Initiator.IsPlayersEnemy)
                        __result = Mathf.RoundToInt(__result * settings.enemiesDamageDealtMultiplier);
                    if (Strings.ToBool(settings.togglePartyDamageDealtMultiplier) &&
                        __instance.Initiator.IsPlayerFaction)
                        __result = Mathf.RoundToInt(__result * settings.partyDamageDealtMultiplier);
                }
            }
        }

        [HarmonyPatch(typeof(DisableDeviceRestriction), "CheckRestriction")]
        public static class DisableDeviceRestriction_CheckRestriction_Patch
        {
            public static bool Prefix(DisableDeviceRestriction __instance, ref UnitEntityData user)
            {
                if (Strings.ToBool(settings.toggleAllDoorContainersUnlocked))
                {
                    var data = (DisableDeviceRestriction.DisableDeviceRestrictionData) __instance.Data;
                    data.Unlocked = true;
                    __instance.Data = data;
                }

                if (Strings.ToBool(settings.toggleRepeatableLockPicking))
                {
                    var data = (DisableDeviceRestriction.DisableDeviceRestrictionData) __instance.Data;
                    data.LastSkillRank.Clear();
                    __instance.Data = data;
                    if (Strings.ToBool(settings.toggleRepeatableLockPickingWeariness))
                        user.Ensure<UnitPartWeariness>()
                            .AddWearinessHours(settings.finalRepeatableLockPickingWeariness);
                    if (Strings.ToBool(settings.toggleRepeatableLockPickingLockPicks))
                    {
                        Storage.unitLockPick = user;
                        Storage.checkLockPick = true;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(RuleSkillCheck), MethodType.Constructor)]
        [HarmonyPatch(new[] {typeof(UnitEntityData), typeof(StatType), typeof(int)})]
        public static class RuleSkillCheck_RollResult_Patch
        {
            private static void Prefix([NotNull] UnitEntityData unit, StatType statType, ref int dc)
            {
                if (Strings.ToBool(settings.togglePassSkillChecksIndividual) &&
                    Strings.ToBool(settings.togglePassSkillChecksIndividualDC99))
                    for (var i = 0; i < settings.togglePassSkillChecksIndividualArray.Count(); i++)
                        if (Strings.ToBool(settings.togglePassSkillChecksIndividualArray[i]) &&
                            Storage.statsSkillsDict.Union(Storage.statsSocialSkillsDict)
                                .ToDictionary(d => d.Key, d => d.Value)[Storage.individualSkillsArray[i]] == statType)
                            if (Common.CheckUnitEntityData(unit, settings.indexPassSkillChecksIndividual))
                                dc = -99;
            }
        }

        [HarmonyPatch(typeof(RulePartySkillCheck), MethodType.Constructor)]
        [HarmonyPatch(new[] {typeof(StatType), typeof(int)})]
        public static class RulePartySkillCheckk_RollResult_Patch
        {
            private static void Prefix(StatType statType, ref int difficultyClass)
            {
                if (Strings.ToBool(settings.togglePassSkillChecksIndividual) &&
                    Strings.ToBool(settings.togglePassSkillChecksIndividualDC99))
                    for (var i = 0; i < settings.togglePassSkillChecksIndividualArray.Count(); i++)
                        if (Strings.ToBool(settings.togglePassSkillChecksIndividualArray[i]) &&
                            Storage.statsSkillsDict.Union(Storage.statsSocialSkillsDict)
                                .ToDictionary(d => d.Key, d => d.Value)[Storage.individualSkillsArray[i]] == statType)
                            difficultyClass = -99;
            }
        }

        [HarmonyPatch(typeof(RuleSkillCheck), "IsPassed", MethodType.Getter)]
        public static class RuleSkillCheck_IsPassed_Patch
        {
            private static void Postfix(ref bool __result, RuleSkillCheck __instance)
            {
                if (settings.toggleFfAny == Storage.isTrueString)
                    if (__instance.Reason != null)
                        if (__instance.Reason.Ability != null)
                            if (__instance.Reason.Caster != null && __instance.Reason.Caster.IsPlayerFaction &&
                                __instance.Initiator.IsPlayerFaction && __instance.Reason.Ability.Blueprint != null &&
                                (__instance.Reason.Ability.Blueprint.EffectOnAlly == AbilityEffectOnUnit.Harmful ||
                                 __instance.Reason.Ability.Blueprint.EffectOnEnemy == AbilityEffectOnUnit.Harmful))
                                __result = true;

                if (Strings.ToBool(settings.togglePassSavingThrowIndividual))
                    for (var i = 0; i < settings.togglePassSavingThrowIndividualArray.Count(); i++)
                        if (Strings.ToBool(settings.togglePassSavingThrowIndividualArray[i]) &&
                            Storage.statsSavesDict[Storage.individualSavesArray[i]] == __instance.StatType)
                            if (Common.CheckUnitEntityData(__instance.Initiator,
                                settings.indexPassSavingThrowIndividuall))
                                __result = true;
                if (Strings.ToBool(settings.togglePassSkillChecksIndividual))
                    for (var i = 0; i < settings.togglePassSkillChecksIndividualArray.Count(); i++)
                        if (Strings.ToBool(settings.togglePassSkillChecksIndividualArray[i]) &&
                            Storage.statsSkillsDict.Union(Storage.statsSocialSkillsDict)
                                .ToDictionary(d => d.Key, d => d.Value)[Storage.individualSkillsArray[i]] ==
                            __instance.StatType)
                            if (Common.CheckUnitEntityData(__instance.Initiator,
                                settings.indexPassSkillChecksIndividual))
                                __result = true;
                if (Strings.ToBool(settings.toggleRepeatableLockPickingLockPicks) &&
                    __instance.StatType == StatType.SkillThievery && __instance.Initiator == Storage.unitLockPick &&
                    Storage.checkLockPick)
                {
                    if (__result && Storage.lockPicks < 1)
                    {
                        Common.AddLogEntry(Strings.GetText("message_NoLockPicksLeft"), Color.red, false);
                        __result = false;
                    }
                    else if (__result && Storage.lockPicks >= 1)
                    {
                        Common.AddLogEntry(Strings.GetText("message_LockPickSaved"), Color.black);
                        __result = true;
                    }
                    else if (!__result && Storage.lockPicks >= 1)
                    {
                        Storage.lockPicks--;
                        Common.AddLogEntry(
                            Strings.GetText("message_LockPickLost") +
                            $" ({Storage.lockPicks} {Strings.GetText("misc_Left")})", Color.black);
                        __result = false;
                    }
                    else if (!__result && Storage.lockPicks < 1)
                    {
                        Common.AddLogEntry(Strings.GetText("message_NoLockPicksLeft"), Color.red, false);
                        __result = false;
                    }

                    Storage.checkLockPick = false;
                }
            }
        }


        [HarmonyPatch(typeof(RulePartySkillCheck), "IsPassed", MethodType.Getter)]
        public static class RulePartySkillCheck_IsPassed_Patch
        {
            private static void Postfix(ref bool __result, RulePartySkillCheck __instance)
            {
                if (settings.toggleFfAny == Storage.isTrueString)
                    if (__instance.Reason != null)
                        if (__instance.Reason.Ability != null)
                            if (__instance.Reason.Caster != null && __instance.Reason.Caster.IsPlayerFaction &&
                                __instance.Initiator.IsPlayerFaction && __instance.Reason.Ability.Blueprint != null &&
                                (__instance.Reason.Ability.Blueprint.EffectOnAlly == AbilityEffectOnUnit.Harmful ||
                                 __instance.Reason.Ability.Blueprint.EffectOnEnemy == AbilityEffectOnUnit.Harmful))
                                __result = true;

                if (Strings.ToBool(settings.togglePassSavingThrowIndividual))
                    for (var i = 0; i < settings.togglePassSavingThrowIndividualArray.Count(); i++)
                        if (Strings.ToBool(settings.togglePassSavingThrowIndividualArray[i]) &&
                            Storage.statsSavesDict[Storage.individualSavesArray[i]] == __instance.StatType)
                            if (Common.CheckUnitEntityData(__instance.Initiator,
                                settings.indexPassSavingThrowIndividuall))
                                __result = true;
                if (Strings.ToBool(settings.togglePassSkillChecksIndividual))
                    for (var i = 0; i < settings.togglePassSkillChecksIndividualArray.Count(); i++)
                        if (Strings.ToBool(settings.togglePassSkillChecksIndividualArray[i]) &&
                            Storage.statsSkillsDict.Union(Storage.statsSocialSkillsDict)
                                .ToDictionary(d => d.Key, d => d.Value)[Storage.individualSkillsArray[i]] ==
                            __instance.StatType)
                            if (Common.CheckUnitEntityData(__instance.Initiator,
                                settings.indexPassSkillChecksIndividual))
                                __result = true;
            }
        }

        [HarmonyPatch(typeof(StaticEntityData), "IsPerceptionCheckPassed", MethodType.Getter)]
        public static class StaticEntityData_IsPerceptionCheckPassed_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.togglePassSkillChecksIndividual))
                    if (Strings.ToBool(settings.togglePassSkillChecksIndividualArray[6]))
                        __result = true;
            }
        }

        [HarmonyPatch(typeof(RestController), "PerformCampingChecks")]
        public static class RestController_PerformCampingChecks__Patch
        {
            private static void Postfix(RestController __instance)
            {
                if (Strings.ToBool(settings.toggleRepeatableLockPickingLockPicks))
                {
                    var newLockPicks = Storage.lockPicks;
                    var limit = 0;
                    var partyMembers = Game.Instance.Player?.Party;
                    foreach (var controllableCharacter in partyMembers)
                    {
                        var baseValue = controllableCharacter.Stats.SkillThievery.BaseValue;
                        if (baseValue > 0)
                        {
                            var skillCheck = new RuleSkillCheck(controllableCharacter, StatType.SkillThievery,
                                Storage.lockPicksCreationDC);
                            var result = skillCheck.BaseRollResult;
                            if (result < 5)
                                newLockPicks += 1;
                            else if (result > 5 && result < 10)
                                newLockPicks += 2;
                            else if (result > 10 && result < 15)
                                newLockPicks += 3;
                            else if (result > 15 && result < 20)
                                newLockPicks += 4;
                            else if (result > 20) newLockPicks += 5;
                            limit += 5;
                        }
                    }

                    if (newLockPicks > 0) Storage.lockPicks = Mathf.Clamp(newLockPicks, 1, limit);
                }
            }
        }


        [HarmonyPatch(typeof(BlueprintCookingRecipe), "CheckIngredients")]
        public static class BlueprintCookingRecipe_CheckIngredients_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleNoIngredientsRequired)) __result = true;
            }
        }

        [HarmonyPatch(typeof(BlueprintCookingRecipe), "SpendIngredients")]
        public static class BlueprintCookingRecipe_SpendIngredients_Patch
        {
            public static bool Prefix(BlueprintCookingRecipe __instance)
            {
                if (Strings.ToBool(settings.toggleNoIngredientsRequired))
                    __instance.Ingredients = new BlueprintCookingRecipe.ItemEntry[0];
                return true;
            }
        }

        [HarmonyPatch(typeof(KineticistAbilityBurnCost), "GetTotal")]
        public static class KineticistAbilityBurnCost_GetTotal_Patch
        {
            private static void Postfix(ref int __result)
            {
                if (Strings.ToBool(settings.toggleNoBurnKineticist)) __result = 0;
            }
        }

        [HarmonyPatch(typeof(AbilityAcceptBurnOnCast), "OnCast")]
        public static class UnitPartKineticistt_AcceptBurn_Patch
        {
            public static bool Prefix(ref int ___BurnValue)
            {
                if (Strings.ToBool(settings.toggleNoBurnKineticist)) ___BurnValue = 0;
                return true;
            }
        }

        [HarmonyPatch(typeof(SaveManager), "PrepareSave")]
        public static class SaveManager_PrepareSave_Patch
        {
            private static void Postfix(SaveInfo save)
            {
                if (Strings.ToBool(settings.toggleRepeatableLockPickingLockPicks)) SaveTools.SaveFile(save);
            }
        }

        [HarmonyPatch(typeof(Game), "LoadGame")]
        public static class Game_LoadGame_Patch
        {
            private static void Postfix(SaveInfo saveInfo)
            {
                if (Strings.ToBool(settings.toggleRepeatableLockPickingLockPicks)) SaveTools.LoadFile(saveInfo);

                Main.ReloadPartyState();
            }
        }

        [HarmonyPatch(typeof(SteamSavesReplicator), "DeleteSave")]
        public static class SteamSavesReplicator_DeleteSave_Patch
        {
            private static void Postfix(SaveInfo saveInfo)
            {
                if (Strings.ToBool(settings.toggleRepeatableLockPickingLockPicks)) SaveTools.DeleteFile(saveInfo);
            }
        }

        [HarmonyPatch(typeof(UnitPartKineticist), "HealBurn")]
        public static class UnitPartKineticist_HealBurn_Patch
        {
            private static void Postfix(UnitPartKineticist __instance)
            {
                if (Strings.ToBool(settings.toggleMaximiseAcceptedBurn))
                    for (var i = __instance.AcceptedBurn; i < __instance.MaxBurn; i++)
                    {
                        var abilityData =
                            new AbilityData(
                                Utilities.GetBlueprintByGuid<BlueprintAbility>("a5631955254ae5c4d9cc2d16870448a2"),
                                __instance.Owner);
                        __instance.AcceptBurn(__instance.MaxBurn, abilityData);
                    }
            }
        }

        [HarmonyPatch(typeof(UnitPartKineticist), "ClearAcceptedBurn")]
        public static class UnitPartKineticist_ClearAcceptedBurn_Patch
        {
            private static void Postfix(UnitPartKineticist __instance)
            {
                if (Strings.ToBool(settings.toggleMaximiseAcceptedBurn))
                    for (var i = __instance.AcceptedBurn; i < __instance.MaxBurn; i++)
                    {
                        var abilityData =
                            new AbilityData(
                                Utilities.GetBlueprintByGuid<BlueprintAbility>("a5631955254ae5c4d9cc2d16870448a2"),
                                __instance.Owner);
                        __instance.AcceptBurn(__instance.MaxBurn, abilityData);
                    }
            }
        }

        [HarmonyPatch(typeof(Inventory), "SetCharacter")]
        public static class Inventory_SetCharacter_Patch
        {
            private static void Postfix(Inventory __instance)
            {
                if (Strings.ToBool(settings.toggleShowPetInventory))
                    if (GroupController.Instance.GetCurrentCharacter().Descriptor.IsPet)
                        __instance.Placeholder.gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(BlueprintArmorType), "HasDexterityBonusLimit", MethodType.Getter)]
        public static class BlueprintArmorType_HasDexterityBonusLimit_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleDexBonusLimit99)) __result = false;
            }
        }

        [HarmonyPatch(typeof(BlueprintArmorType), "MaxDexterityBonus", MethodType.Getter)]
        public static class BlueprintArmorType_MaxDexterityBonus_Patch
        {
            private static void Postfix(ref int __result)
            {
                if (Strings.ToBool(settings.toggleDexBonusLimit99)) __result = 99;
            }
        }

        [HarmonyPatch(typeof(BlueprintItem), "Weight", MethodType.Getter)]
        public static class BlueprintItem_Weight_Patch
        {
            private static void Postfix(ref float __result)
            {
                if (Strings.ToBool(settings.toggleItemsWeighZero)) __result = 0f;
            }
        }

        [HarmonyPatch(typeof(BlueprintArmorType), "Weight", MethodType.Getter)]
        public static class BlueprintArmorType_Weight_Patch
        {
            private static void Postfix(ref float __result)
            {
                if (Strings.ToBool(settings.toggleItemsWeighZero)) __result = 0f;
            }
        }

        [HarmonyPatch(typeof(BlueprintItemArmor), "Weight", MethodType.Getter)]
        public static class BlueprintItemArmor_Weight_Patch
        {
            private static void Postfix(ref float __result)
            {
                if (Strings.ToBool(settings.toggleItemsWeighZero)) __result = 0f;
            }
        }

        [HarmonyPatch(typeof(BlueprintItemShield), "Weight", MethodType.Getter)]
        public static class BlueprintItemShield_Weight_Patch
        {
            private static void Postfix(ref float __result)
            {
                if (Strings.ToBool(settings.toggleItemsWeighZero)) __result = 0f;
            }
        }

        [HarmonyPatch(typeof(BlueprintItemWeapon), "Weight", MethodType.Getter)]
        public static class BlueprintItemWeapon_Weight_Patch
        {
            private static void Postfix(ref float __result)
            {
                if (Strings.ToBool(settings.toggleItemsWeighZero)) __result = 0f;
            }
        }

        [HarmonyPatch(typeof(BlueprintWeaponType), "Weight", MethodType.Getter)]
        public static class BlueprintWeaponType_Weight_Patch
        {
            private static void Postfix(ref float __result)
            {
                if (Strings.ToBool(settings.toggleItemsWeighZero)) __result = 0f;
            }
        }

        [HarmonyPatch(typeof(RuleCalculateAttacksCount), "OnTrigger")]
        public static class RuleCalculateAttacksCount_OnTrigger_Patch
        {
            private static void Postfix(RuleCalculateAttacksCount.AttacksCount ___PrimaryHand,
                RuleCalculateAttacksCount.AttacksCount ___SecondaryHand, RuleCalculateAttacksCount __instance)
            {
                if (__instance.Initiator.IsPlayerFaction && Strings.ToBool(settings.toggleExtraAttacksParty))
                {
                    ___PrimaryHand.MainAttacks += settings.extraAttacksPartyPrimaryHand;
                    ___SecondaryHand.MainAttacks += settings.extraAttacksPartySecondaryHand;
                }
            }
        }

        [HarmonyPatch(typeof(UnitAlignment), "GetDirection")]
        private static class UnitAlignment_GetDirection_Patch
        {
            private static void Postfix(UnitAlignment __instance, ref Vector2 __result,
                AlignmentShiftDirection direction)
            {
                if (!Main.enabled) return;
                if (Strings.ToBool(settings.toggleAlignmentFix))
                {
                    if (direction == AlignmentShiftDirection.NeutralGood) __result = new Vector2(0, 1);
                    if (direction == AlignmentShiftDirection.NeutralEvil) __result = new Vector2(0, -1);
                    if (direction == AlignmentShiftDirection.LawfulNeutral) __result = new Vector2(-1, 0);
                    if (direction == AlignmentShiftDirection.ChaoticNeutral) __result = new Vector2(1, 0);
                }
            }
        }

        [HarmonyPatch(typeof(UnitAlignment), "Set", typeof(Alignment), typeof(bool))]
        private static class UnitAlignment_Set_Patch
        {
            private static void Prefix(UnitAlignment __instance, ref Alignment alignment)
            {
                if (Strings.ToBool(settings.togglePreventAlignmentChanges)) alignment = __instance.Value;
            }
        }

        [HarmonyPatch(typeof(UnitAlignment), "Shift", typeof(AlignmentShiftDirection), typeof(int),
            typeof(IAlignmentShiftProvider))]
        private static class UnitAlignment_Shift_Patch
        {
            private static bool Prefix(UnitAlignment __instance, AlignmentShiftDirection direction, ref int value,
                IAlignmentShiftProvider provider)
            {
                try
                {
                    if (!Main.enabled) return true;

                    if (Strings.ToBool(settings.togglePreventAlignmentChanges)) value = 0;

                    if (Strings.ToBool(settings.toggleAlignmentFix))
                    {
                        if (value == 0) return false;
                        var vector = __instance.Vector;
                        var num = value / 50f;
                        var directionVector = Traverse.Create(__instance).Method("GetDirection", direction)
                            .GetValue<Vector2>();
                        var newAlignment = __instance.Vector + directionVector * num;
                        if (newAlignment.magnitude > 1f)
                            //Instead of normalizing towards true neutral, normalize opposite to the alignment vector
                            //to prevent sliding towards neutral
                            newAlignment -= (newAlignment.magnitude - newAlignment.normalized.magnitude) *
                                            directionVector;
                        if (direction == AlignmentShiftDirection.TrueNeutral &&
                            (Vector2.zero - __instance.Vector).magnitude < num) newAlignment = Vector2.zero;
                        Traverse.Create(__instance).Property<Vector2>("Vector").Value = newAlignment;
                        Traverse.Create(__instance).Method("UpdateValue").GetValue();
                        //Traverse requires the parameter types to find interface parameters
                        Traverse.Create(__instance).Method("OnChanged",
                            new[]
                            {
                                typeof(AlignmentShiftDirection), typeof(Vector2), typeof(IAlignmentShiftProvider),
                                typeof(bool)
                            },
                            new object[] {direction, vector, provider, true}).GetValue();
                        return false;
                    }
                }
                catch (Exception e)
                {
                    modLogger.Log(e.ToString());
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ForbidSpellbookOnAlignmentDeviation), "CheckAlignment")]
        private static class ForbidSpellbookOnAlignmentDeviation_CheckAlignment_Patch
        {
            private static bool Prefix(ForbidSpellbookOnAlignmentDeviation __instance)
            {
                if (Strings.ToBool(settings.toggleSpellbookAbilityAlignmentChecks))
                    __instance.Alignment = __instance.Owner.Alignment.Value.ToMask();
                return true;
            }
        }

        [HarmonyPatch(typeof(AbilityCasterAlignment), "CorrectCaster")]
        private static class AbilityCasterAlignment_CheckAlignment_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleSpellbookAbilityAlignmentChecks)) __result = true;
            }
        }

        [HarmonyPatch(typeof(SummonPool), "Register")]
        private static class SummonPool_Register_Patch
        {
            private static void Postfix(ref UnitEntityData unit)
            {
                if (Strings.ToBool(settings.toggleSetSpeedOnSummon))
                    unit.Descriptor.Stats.GetStat(StatType.Speed).BaseValue = settings.setSpeedOnSummonValue;
            }
        }

        [HarmonyPatch(typeof(Quest), "TimeToFail", MethodType.Getter)]
        private static class Quest_HandleTimePassed_Patch
        {
            private static void Postfix(ref TimeSpan? __result)
            {
                if (__result != null && Strings.ToBool(settings.toggleFreezeTimedQuestAt90Days))
                    __result = TimeSpan.FromDays(90);
            }
        }

        [HarmonyPatch(typeof(QuestObjective), "TimeToFail", MethodType.Getter)]
        private static class QuestObjective_HandleTimePassed_Patch
        {
            private static void Postfix(ref TimeSpan? __result)
            {
                if (__result != null && Strings.ToBool(settings.toggleFreezeTimedQuestAt90Days))
                    __result = TimeSpan.FromDays(90);
            }
        }

        [HarmonyPatch(typeof(QuestObjective), "Fail")]
        private static class QuestObjective_Fail_Patch
        {
            private static bool Prefix()
            {
                if (Strings.ToBool(settings.togglePreventQuestFailure)) return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(RuleCheckCastingDefensively), "Success", MethodType.Getter)]
        private static class RuleCheckCastingDefensively_Success_Patch
        {
            private static void Postfix(ref bool __result, RuleCheckCastingDefensively __instance)
            {
                if (Strings.ToBool(settings.toggleAlwaysSucceedCastingDefensively) &&
                    __instance.Initiator.IsPlayerFaction) __result = true;
            }
        }

        [HarmonyPatch(typeof(RuleCheckConcentration), "Success", MethodType.Getter)]
        private static class RuleCheckConcentration_Success_Patch
        {
            private static void Postfix(ref bool __result, RuleCheckConcentration __instance)
            {
                if (Strings.ToBool(settings.toggleAlwaysSucceedConcentration) && __instance.Initiator.IsPlayerFaction)
                    __result = true;
            }
        }

        [HarmonyPatch(typeof(SpellBookToggle), "SetUp")]
        private static class SpellBookToggle_SetUp_Patch
        {
            private static bool Prefix(UnitEntityData unit, SpellBookToggle __instance,
                List<SpellbookClassTab> ___m_Tabs)
            {
                if (Strings.ToBool(settings.toggleSortSpellbooksAlphabetically))
                {
                    __instance.Initialize();
                    foreach (var tab in ___m_Tabs)
                        tab.SetActive(false);
                    __instance.Spellbooks = new List<Spellbook>();
                    var i = 0;
                    __instance.Spellbooks = unit.Descriptor.Spellbooks.OrderBy(d => d.Blueprint.DisplayName).ToList();
                    foreach (var spellbook in __instance.Spellbooks)
                    {
                        spellbook.UpdateAllSlotsSize();
                        if (i < ___m_Tabs.Count)
                        {
                            ___m_Tabs[i].SetName(spellbook.Blueprint.DisplayName);
                            ___m_Tabs[i].SetIndex(i);
                            ___m_Tabs[i].Toggle.onValueChanged.AddListener(__instance.OnToggle);
                            ___m_Tabs[i].SetLevel(spellbook.CasterLevel);
                            ___m_Tabs[i].SetActive(true);
                            ++i;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (__instance.Spellbooks.Count > 0)
                        __instance.SelectSpellbookByIndex(0);
                    __instance.gameObject.SetActive(__instance.Spellbooks.Count > 1);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SpellBookView), "GetSpellsForLevel")]
        private static class SpellBookView_GetSpellsForLevel_Patch
        {
            private static void Postfix(ref List<AbilityData> __result)
            {
                if (Strings.ToBool(settings.toggleSortSpellsAlphabetically))
                    __result = __result.OrderBy(d => d.Name).ToList();
            }
        }

        [HarmonyPatch(typeof(HitPlayer), "CalcHitLevel")]
        private static class HitPlayer_CalcHitLevel_Patch
        {
            private static void Postfix(ref HitLevel __result)
            {
                if (Strings.ToBool(settings.toggleSortSpellsAlphabetically)) __result = HitLevel.Crit;
            }
        }

        [HarmonyPatch(typeof(RuleAttackRoll), "IsCriticalConfirmed", MethodType.Getter)]
        private static class HitPlayer_OnTriggerl_Patch
        {
            private static void Postfix(ref bool __result, RuleAttackRoll __instance)
            {
                if (Strings.ToBool(settings.toggleAllHitsAreCritical) && __instance.Initiator.IsPlayerFaction)
                    __result = true;
            }
        }

        [HarmonyPatch(typeof(RuleSavingThrow), "IsPassed", MethodType.Getter)]
        public static class RuleSavingThrow_IsPassed_Patch
        {
            private static void Postfix(ref bool __result, RuleSavingThrow __instance)
            {
                if (settings.toggleFfAny == Storage.isTrueString)
                    if (__instance.Reason != null)
                        if (__instance.Reason.Ability != null)
                            if (__instance.Reason.Caster != null && __instance.Reason.Caster.IsPlayerFaction &&
                                __instance.Initiator.IsPlayerFaction && __instance.Reason.Ability.Blueprint != null &&
                                (__instance.Reason.Ability.Blueprint.EffectOnAlly == AbilityEffectOnUnit.Harmful ||
                                 __instance.Reason.Ability.Blueprint.EffectOnEnemy == AbilityEffectOnUnit.Harmful))
                                __result = true;

                if (Strings.ToBool(settings.togglePassSavingThrowIndividual))
                    for (var i = 0; i < settings.togglePassSavingThrowIndividualArray.Count(); i++)
                        if (Strings.ToBool(settings.togglePassSavingThrowIndividualArray[i]) &&
                            Storage.statsSavesDict[Storage.individualSavesArray[i]] == __instance.StatType)
                            if (Common.CheckUnitEntityData(__instance.Initiator,
                                settings.indexPassSavingThrowIndividuall))
                                __result = true;
            }
        }

        [HarmonyPatch(typeof(UnitCombatState), "AttackOfOpportunity")]
        private static class UnitCombatState_AttackOfOpportunity_Patch
        {
            private static bool Prefix(UnitEntityData target)
            {
                if (Strings.ToBool(settings.toggleNoAttacksOfOpportunity) &&
                    Common.CheckUnitEntityData(target, settings.indexNoAttacksOfOpportunity)) return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(CampingSettings), "IsDungeon", MethodType.Getter)]
        private static class CampingSettings_IsDungeon_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (!Main.enabled) return;
                if (Strings.ToBool(settings.toggleCookingAndHuntingInDungeons)) __result = false;
            }
        }

        [HarmonyPatch(typeof(BlueprintArmorType), "ArmorChecksPenalty", MethodType.Getter)]
        public static class BlueprintArmorType_ArmorChecksPenalty_Patch
        {
            private static void Postfix(ref int __result)
            {
                if (Strings.ToBool(settings.toggleArmourChecksPenalty0)) __result = 0;
            }
        }

        [HarmonyPatch(typeof(SoundState), "OnAreaLoadingComplete")]
        public static class SoundState_OnAreaLoadingComplete_Patch
        {
            private static void Postfix()
            {
                if (settings.settingShowDebugInfo)
                {
                    modLogger.Log(Game.Instance.CurrentlyLoadedArea.AreaName.ToString());
                    modLogger.Log(Game.Instance.CurrentlyLoadedArea.AssetGuid);
                    modLogger.Log(SceneManager.GetActiveScene().name);
                }

                if (Strings.ToBool(settings.toggleUnlimitedCasting) &&
                    SceneManager.GetActiveScene().name == "HouseAtTheEdgeOfTime_Courtyard_Light")
                    UIUtility.ShowMessageBox(Strings.GetText("warning_UnlimitedCasting"),
                        DialogMessageBox.BoxType.Message, Common.CloseMessageBox);
                if (Strings.ToBool(settings.toggleNoDamageFromEnemies) && Game.Instance.CurrentlyLoadedArea.AssetGuid ==
                    "0ba5b24abcd5523459e54cd5877cb837")
                    UIUtility.ShowMessageBox(Strings.GetText("warning_NoDamageFromEnemies"),
                        DialogMessageBox.BoxType.Message, Common.CloseMessageBox);
            }
        }

        [HarmonyPatch(typeof(EncumbranceHelper.CarryingCapacity), "GetEncumbrance")]
        [HarmonyPatch(new[] {typeof(float)})]
        private static class EncumbranceHelperCarryingCapacity_GetEncumbrance_Patch
        {
            private static void Postfix(ref Encumbrance __result)
            {
                if (Strings.ToBool(settings.toggleSetEncumbrance))
                    __result = Common.IntToEncumbrance(settings.setEncumbrancIndex);
            }
        }

        [HarmonyPatch(typeof(PartyEncumbranceController), "UpdateUnitEncumbrance")]
        private static class PartyEncumbranceController_UpdateUnitEncumbrance_Patch
        {
            private static void Postfix(UnitDescriptor unit)
            {
                if (Strings.ToBool(settings.toggleSetEncumbrance))
                {
                    unit.Encumbrance = Common.IntToEncumbrance(settings.setEncumbrancIndex);
                    unit.Remove<UnitPartEncumbrance>();
                }
            }
        }

        [HarmonyPatch(typeof(PartyEncumbranceController), "UpdatePartyEncumbrance")]
        private static class PartyEncumbranceController_UpdatePartyEncumbrance_Patch
        {
            private static bool Prefix()
            {
                if (Strings.ToBool(settings.toggleSetEncumbrance))
                {
                    player.Encumbrance = Common.IntToEncumbrance(settings.setEncumbrancIndex);
                    return false;
                }

                return true;
            }
        }

        //KingmakerMods.pw start
        [HarmonyPatch(typeof(GlobalMapRules), "ChangePartyOnMap")]
        private static class GlobalMapRules_ChangePartyOnMap_Patch
        {
            private static bool Prefix()
            {
                if (Strings.ToBool(settings.toggleInstantPartyChange)) return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(IngameMenuManager), "OpenGroupManager")]
        private static class IngameMenuManager_OpenGroupManager_Patch
        {
            private static bool Prefix(IngameMenuManager __instance)
            {
                if (Strings.ToBool(settings.toggleInstantPartyChange))
                {
                    var startChangedPartyOnGlobalMap = __instance.GetType().GetMethod("StartChangedPartyOnGlobalMap",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    startChangedPartyOnGlobalMap.Invoke(__instance, new object[] { });
                    return false;
                }

                return true;
            }
        }
        //KingmakerMods.pw end

        [HarmonyPatch(typeof(BuildModeUtility), "IsDevelopment", MethodType.Getter)]
        private static class BuildModeUtility_IsDevelopment_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleDevTools)) __result = true;
            }
        }

        [HarmonyPatch(typeof(SmartConsole), "WriteLine")]
        private static class SmartConsole_WriteLine_Patch
        {
            private static void Postfix(string message)
            {
                if (Strings.ToBool(settings.toggleDevTools))
                {
                    modLogger.Log(message);
                    UberLoggerAppWindow.Instance.Log(new LogInfo(null, nameof(SmartConsole), LogSeverity.Message,
                        new List<LogStackFrame>(), message, Array.Empty<object>()));
                }
            }
        }

        [HarmonyPatch(typeof(SmartConsole), "Initialise")]
        private static class SmartConsole_Initialise_Patch
        {
            private static void Postfix()
            {
                if (Strings.ToBool(settings.toggleDevTools)) SmartConsoleCommands.Register();
            }
        }

        [HarmonyPatch(typeof(MainMenu), "Start")]
        private static class MainMenu_Start_Patch
        {
            private static void Postfix()
            {
                ModifiedBlueprintTools.Patch();

                if (Strings.ToBool(settings.toggleNoTempHPKineticist)) Cheats.PatchBurnEffectBuff(0);
            }
        }

        [HarmonyPatch(typeof(UnitPartNegativeLevels), "Add")]
        private static class UnitPartNegativeLevels_Add_Patch
        {
            private static bool Prefix(UnitPartNegativeLevels __instance)
            {
                if (Strings.ToBool(settings.toggleNoNegativeLevels) && __instance.Owner.IsPlayerFaction) return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(Kingmaker.Items.Slots.ItemSlot), "RemoveItem")]
        [HarmonyPatch(new[] {typeof(bool)})]
        private static class ItemSlot_RemoveItem_Patch
        {
            private static void Prefix(Kingmaker.Items.Slots.ItemSlot __instance, ItemEntity ___m_Item,
                UnitDescriptor ___Owner, ref ItemEntity __state)
            {
                if (Game.Instance.CurrentMode == GameModeType.Default &&
                    Strings.ToBool(settings.togglAutoEquipConsumables))
                {
                    __state = null;
                    if (___Owner.Body.QuickSlots.Any(x => x.HasItem && x.Item == ___m_Item)) __state = ___m_Item;
                }
            }

            private static void Postfix(Kingmaker.Items.Slots.ItemSlot __instance, ItemEntity ___m_Item,
                UnitDescriptor ___Owner, ItemEntity __state)
            {
                if (Game.Instance.CurrentMode == GameModeType.Default &&
                    Strings.ToBool(settings.togglAutoEquipConsumables))
                    if (__state != null)
                    {
                        var blueprint = __state.Blueprint;
                        foreach (var item in Game.Instance.Player.Inventory.Items)
                            if (item.Blueprint.ItemType == ItemsFilter.ItemType.Usable && item.Blueprint == blueprint)
                            {
                                __instance.InsertItem(item);
                                break;
                            }

                        __state = null;
                    }
            }
        }

        [HarmonyPatch(typeof(IgnorePrerequisites), "Ignore", MethodType.Getter)]
        private static class IgnorePrerequisites_Ignore_Patch
        {
            private static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleIgnorePrerequisites)) __result = true;
            }
        }

        [HarmonyPatch(typeof(PrerequisiteCasterTypeSpellLevel), "Check")]
        public static class PrerequisiteCasterTypeSpellLevel_Check_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleIgnoreCasterTypeSpellLevel)) __result = true;
            }
        }

        [HarmonyPatch(typeof(PrerequisiteNoArchetype), "Check")]
        public static class PrerequisiteNoArchetype_Check_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleIgnoreForbiddenArchetype)) __result = true;
            }
        }

        [HarmonyPatch(typeof(PrerequisiteStatValue), "Check")]
        public static class PrerequisiteStatValue_Check_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (Strings.ToBool(settings.toggleIgnorePrerequisiteStatValue)) __result = true;
            }
        }

        [HarmonyPatch(typeof(BlueprintUnit), "PreloadResources")]
        public static class BlueprintUnit_PreloadResources_Patch
        {
            public static void Postfix(BlueprintUnit __instance)
            {
                if (Strings.ToBool(settings.toggleSpiderBegone))
                {
                    if (__instance.Type?.AssetGuidThreadSafe == "243702bdc53e2574aaa34d1e3eafe6aa")
                        __instance.Prefab = Utilities
                            .GetBlueprintByGuid<BlueprintUnit>("cc124e00e9ed4a441bc10de414f02312").Prefab;
                    else if (__instance.Type?.AssetGuidThreadSafe == "0fd1473096fbdda4db770cca8366c5e1")
                        __instance.Prefab = Utilities
                            .GetBlueprintByGuid<BlueprintUnit>("12a5944fa27307e4e8b6f56431d5cc8c").Prefab;
                    else if (Storage.spiderGuids.Contains(__instance.AssetGuidThreadSafe))
                        __instance.Prefab = Utilities
                            .GetBlueprintByGuid<BlueprintUnit>("cc124e00e9ed4a441bc10de414f02312").Prefab;
                }
            }
        }

        [HarmonyPatch(typeof(EntityCreationController), "SpawnUnit")]
        [HarmonyPatch(new[] {typeof(BlueprintUnit), typeof(Vector3), typeof(Quaternion), typeof(SceneEntitiesState)})]
        public static class EntityCreationControllert_SpawnUnit_Patch1
        {
            public static void Postfix(ref BlueprintUnit unit)
            {
                if (Strings.ToBool(settings.toggleSpiderBegone))
                {
                    if (unit.Type?.AssetGuidThreadSafe == "243702bdc53e2574aaa34d1e3eafe6aa")
                        unit.Prefab = Utilities.GetBlueprintByGuid<BlueprintUnit>("cc124e00e9ed4a441bc10de414f02312")
                            .Prefab;
                    else if (unit.Type?.AssetGuidThreadSafe == "0fd1473096fbdda4db770cca8366c5e1")
                        unit.Prefab = Utilities.GetBlueprintByGuid<BlueprintUnit>("12a5944fa27307e4e8b6f56431d5cc8c")
                            .Prefab;
                    else if (Storage.spiderGuids.Contains(unit.AssetGuidThreadSafe))
                        unit.Prefab = Utilities.GetBlueprintByGuid<BlueprintUnit>("cc124e00e9ed4a441bc10de414f02312")
                            .Prefab;
                }
            }
        }

        [HarmonyPatch(typeof(EntityCreationController), "SpawnUnit")]
        [HarmonyPatch(new[]
        {
            typeof(BlueprintUnit), typeof(UnitEntityView), typeof(Vector3), typeof(Quaternion),
            typeof(SceneEntitiesState)
        })]
        public static class EntityCreationControllert_SpawnUnit_Patch2
        {
            public static void Postfix(ref BlueprintUnit unit)
            {
                if (Strings.ToBool(settings.toggleSpiderBegone))
                {
                    if (unit.Type?.AssetGuidThreadSafe == "243702bdc53e2574aaa34d1e3eafe6aa")
                        unit.Prefab = Utilities.GetBlueprintByGuid<BlueprintUnit>("cc124e00e9ed4a441bc10de414f02312")
                            .Prefab;
                    else if (unit.Type?.AssetGuidThreadSafe == "0fd1473096fbdda4db770cca8366c5e1")
                        unit.Prefab = Utilities.GetBlueprintByGuid<BlueprintUnit>("12a5944fa27307e4e8b6f56431d5cc8c")
                            .Prefab;
                    else if (Storage.spiderGuids.Contains(unit.AssetGuidThreadSafe))
                        unit.Prefab = Utilities.GetBlueprintByGuid<BlueprintUnit>("cc124e00e9ed4a441bc10de414f02312")
                            .Prefab;
                }
            }
        }

        [HarmonyPatch(typeof(ContextConditionAlignment), "CheckCondition")]
        public static class ContextConditionAlignment_CheckCondition_Patch
        {
            public static void Postfix(ref bool __result, ContextConditionAlignment __instance)
            {
                if (Strings.ToBool(settings.toggleReverseCasterAlignmentChecks))
                    if (__instance.CheckCaster)
                        __result = !__result;
            }
        }

        [HarmonyPatch(typeof(RuleSummonUnit), MethodType.Constructor)]
        [HarmonyPatch(new[]
            {typeof(UnitEntityData), typeof(BlueprintUnit), typeof(Vector3), typeof(Rounds), typeof(int)})]
        public static class RuleSummonUnit_Constructor_Patch
        {
            public static void Prefix(UnitEntityData initiator, BlueprintUnit blueprint, Vector3 position,
                ref Rounds duration, ref int level)
            {
                if (Strings.ToBool(settings.toggleSummonDurationMultiplier) &&
                    Common.CheckUnitEntityData(initiator, settings.indexSummonDurationMultiplier))
                {
                    duration = new Rounds(
                        Convert.ToInt32(duration.Value * settings.finalSummonDurationMultiplierValue));
                    ;
                }

                if (Strings.ToBool(settings.toggleSetSummonLevelTo20) &&
                    Common.CheckUnitEntityData(initiator, settings.indexSetSummonLevelTo20)) level = 20;

                if (settings.settingShowDebugInfo)
                    modLogger.Log("Initiator: " + initiator.CharacterName + "\nBlueprint: " + blueprint.CharacterName +
                                  "\nPosition: " + position + "\nDuration: " + duration.Value + "\nLevel: " + level);
            }
        }

        [HarmonyPatch(typeof(RuleApplyBuff), MethodType.Constructor)]
        [HarmonyPatch(new[]
        {
            typeof(UnitEntityData), typeof(BlueprintBuff), typeof(MechanicsContext), typeof(TimeSpan?),
            typeof(Func<BlueprintBuff, MechanicsContext, TimeSpan?, Buff>)
        })]
        public static class RuleApplyBuff_TickTime_Patch
        {
            public static void Prefix(UnitEntityData initiator, BlueprintBuff blueprint, MechanicsContext context,
                ref TimeSpan? duration)
            {
                try
                {
                    if (Strings.ToBool(settings.toggleBuffDurationMultiplier) &&
                        Common.CheckUnitEntityData(initiator, settings.indexBuffDurationMultiplier) &&
                        duration != null)
                        duration = TimeSpan.FromTicks(
                            Convert.ToInt64(duration.Value.Ticks * settings.finalBuffDurationMultiplierValue));
                }
                catch (Exception e)
                {
                    modLogger.Log(e.ToString());
                }

                if (settings.settingShowDebugInfo)
                    modLogger.Log("Initiator: " + initiator.CharacterName + "\nBlueprintBuff: " + blueprint.Name +
                                  "\nContext: " + context.Name + "\nDuration: " + duration);
            }
        }

        [HarmonyPatch(typeof(UberLogger.Logger), "ForwardToUnity")]
        private static class UberLoggerLogger_ForwardToUnity_Patch
        {
            private static void Prefix(ref object message)
            {
                if (Strings.ToBool(settings.toggleUberLoggerForwardPrefix))
                {
                    var message1 = "[UberLogger] " + message;
                    message = message1;
                }
            }
        }


        [HarmonyPatch(typeof(DungeonStageInitializer), "Initialize")]
        private static class DungeonStageInitializer_Initialize_Patch
        {
            private static void Prefix(BlueprintDungeonArea area)
            {
                if (settings.settingShowDebugInfo)
                    modLogger.Log("Game.Instance.Player.DungeonState.Stage: " +
                                  Game.Instance.Player.DungeonState.Stage);
            }
        }

        [HarmonyPatch(typeof(DungeonDebug), "SaveStage")]
        private static class DungeonDebug_SaveStage_Patch_Pre
        {
            private static void Prefix(string filename)
            {
                if (settings.settingShowDebugInfo)
                {
                    modLogger.Log("DungeonDebug.SaveStage filename: " + filename);
                    modLogger.Log("DungeonDebug.SaveStage Path: " +
                                  Path.Combine(Application.persistentDataPath, "DungeonStages"));
                }
            }
        }

        [HarmonyPatch(typeof(DungeonDebug), "SaveStage")]
        private static class DungeonDebug_SaveStage_Patch_Post
        {
            private static void Postfix(string filename)
            {
                if (settings.settingShowDebugInfo)
                    try
                    {
                        var str = File.ReadAllText(Path.Combine(Application.persistentDataPath,
                            $"DungeonStages\\{filename}"));
                        modLogger.Log($"START---{filename}---START\n" + str + $"\nEND---{filename}---END");
                    }
                    catch (Exception e)
                    {
                        modLogger.Log(e.ToString());
                    }
            }
        }
    }
}