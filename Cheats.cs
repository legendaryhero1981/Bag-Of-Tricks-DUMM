using System;
using System.Collections.Generic;
using System.Linq;

using Harmony;

using Kingmaker;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Rest;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.State;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;

using UnityModManagerNet;

namespace BagOfTricks
{
    internal static class Cheats
    {
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.ModLogger;
        public static Settings settings = Main.Settings;

        public static void PatchBurnEffectBuff(int multiplier)
        {
            var burnEffectBuff = Utilities.GetBlueprintByGuid<BlueprintBuff>("95b1c0d55f30996429a3a4eba4d2b4a6");

            if (burnEffectBuff != null)
            {
                var components = burnEffectBuff.ComponentsArray;
                foreach (var bc in components)
                    if (bc.name == "$AddContextStatBonus$b73c2975-b1b9-45f6-a300-62711d93f0b4")
                    {
                        var contextStatBonus = bc as AddContextStatBonus;
                        contextStatBonus.Multiplier = multiplier;
                        Traverse.Create(bc).SetValue(contextStatBonus);
                    }
            }
        }

        public static void InstantRest()
        {
            var escMode = false;
            if (Game.Instance.CurrentMode == GameModeType.EscMode)
            {
                escMode = true;
                Game.Instance.UI.EscManager.OnEscPressed();
                Game.Instance.PauseBind();
            }

            var partyMembers = Game.Instance?.Player.ControllableCharacters;
            foreach (var u in partyMembers)
            {
                ApplyInstantRest(u);
                var pair = UnitPartDualCompanion.GetPair(u);
                if (pair != null) ApplyInstantRest(pair);
            }

            if (escMode)
            {
                Game.Instance.PauseBind();
                Game.Instance.UI.EscManager.OnEscPressed();
            }
        }

        public static void ApplyInstantRest(UnitEntityData unitEntityData)
        {
            if (unitEntityData.Descriptor.State.IsFinallyDead)
            {
                unitEntityData.Descriptor.Resurrect();
                unitEntityData.Position = Game.Instance.Player.MainCharacter.Value.Position;
            }

            RestController.RemoveNegativeEffects(unitEntityData.Descriptor);
            RestController.ApplyRest(unitEntityData.Descriptor);
            Rulebook.Trigger(new RuleHealDamage(unitEntityData, unitEntityData, new DiceFormula(),
                unitEntityData.Descriptor.Stats.HitPoints.ModifiedValue));
            foreach (var attribute in StatTypeHelper.Attributes)
                Rulebook.Trigger(new RuleHealStatDamage(unitEntityData, unitEntityData, attribute));

            foreach (var attribute in unitEntityData.Stats.Attributes)
            {
                attribute.Damage = 0;
                attribute.Drain = 0;
            }

            RestoreAllItemCharges();
        }

        public static void RestoreAllItemCharges()
        {
            foreach (var itemEntity in Game.Instance.Player.Inventory) itemEntity.RestoreCharges();
        }

        private static void UpdateMap(GlobalMapRules instance, GlobalMapState globalMap, IEnumerable<BlueprintMapEdge> blueprintMapEdgeSet)
        {
            foreach (var blueprint in blueprintMapEdgeSet)
                try
                {
                    globalMap.GetEdgeData(blueprint).UpdateExplored(1f, 1);
                    if ((bool) instance)
                        instance.GetEdgeObject(blueprint).UpdateRenderers();
                }
                catch (Exception ex)
                {
                    modLogger.Log(ex.ToString());
                }
        }

        public static void ClaimResources()
        {
            var globalMap = Game.Instance.Player.GlobalMap;
            var locations = globalMap.Locations.Values;

            var canClaimLocations = locations.Any(l => l.Resource == LocationData.ResourceStateType.CanClaim);
            if (!canClaimLocations) return;
            foreach (var locationData in locations)
            {
                if (!locationData.IsRevealed) continue;
                if (locationData.Resource != LocationData.ResourceStateType.CanClaim) continue;
                var locationObject = GlobalMapRules.Instance.GetLocationObject(locationData.Blueprint);
                if (!locationObject) continue;
                locationData.ClaimResource();
                Common.ModLoggerDebug(locationData.Name + " claimed.");
                
            }
        }


        public static void RevealLocationsByType(List<LocationType> types)
        {
            var instance = GlobalMapRules.Instance;
            var globalMap = Game.Instance.Player.GlobalMap;
            foreach (var blueprint in Utilities.GetScriptableObjects<BlueprintLocation>())
                if (types.Contains(blueprint.Type))
                    try
                    {
                        var locationData = globalMap.GetLocationData(blueprint);
                        locationData.EdgesOpened = true;
                        locationData.Reveal();

                        var locationObject = instance.GetLocationObject(blueprint);
                        if ((bool) instance)
                            if ((bool) locationObject)
                                instance.RevealLocation(locationObject);
                    }
                    catch (Exception ex)
                    {
                        modLogger.Log(ex.ToString());
                    }

            foreach (var blueprint in Utilities.GetScriptableObjects<BlueprintMapEdge>())
                try
                {
                    globalMap.GetEdgeData(blueprint).UpdateExplored(1f, 1);
                    if ((bool)((UnityEngine.Object)instance))
                        instance.GetEdgeObject(blueprint).UpdateRenderers();
                }
                catch (Exception ex)
                {
                    UberDebug.LogException(ex);
                }
        }

        public static void RevealAllLocations()
        {
            var instance = GlobalMapRules.Instance;
            var globalMap = Game.Instance.Player.GlobalMap;
            foreach (var blueprint in Utilities.GetScriptableObjects<BlueprintLocation>())
                if (blueprint.Type != LocationType.SystemWaypoint)
                    try
                    {
                        var locationData = globalMap.GetLocationData(blueprint);
                        locationData.EdgesOpened = true;
                        locationData.Reveal();

                        var locationObject = instance.GetLocationObject(blueprint);
                        if ((bool) instance)
                            if ((bool) locationObject)
                                instance.RevealLocation(locationObject);
                    }
                    catch (Exception ex)
                    {
                        modLogger.Log(ex.ToString());
                    }

            UpdateMap(instance, globalMap,
                (HashSet<BlueprintMapEdge>) Utilities.GetScriptableObjects<BlueprintMapEdge>());
        }

        public static void RevealReachableLocations()
        {
            var instance = GlobalMapRules.Instance;
            if (instance == null)
                return;
            var globalMap = Game.Instance.Player.GlobalMap;
            var blueprintLocationSet = new HashSet<BlueprintLocation>();
            var blueprintMapEdgeSet = new HashSet<BlueprintMapEdge>();
            var blueprintLocationQueue = new Queue<BlueprintLocation>();
            if (globalMap.CurrentPosition.Location != null)
                blueprintLocationQueue.Enqueue(globalMap.CurrentPosition.Location);
            if (globalMap.CurrentPosition.Edge != null)
            {
                var edgeObject = instance.GetEdgeObject(globalMap.CurrentPosition.Edge);
                blueprintLocationQueue.Enqueue(edgeObject.Location1.Blueprint);
                blueprintLocationQueue.Enqueue(edgeObject.Location2.Blueprint);
            }

            while (blueprintLocationQueue.Count > 0)
            {
                var blueprintLocation = blueprintLocationQueue.Dequeue();
                var locationObject = instance.GetLocationObject(blueprintLocation);
                if (!(locationObject == null))
                    foreach (var edge in locationObject.Edges)
                        if (!edge.IsLocked)
                        {
                            var oppositeLocation = edge.GetOppositeLocation(blueprintLocation);
                            if (oppositeLocation.PossibleToRevealCondition.Check() &&
                                !blueprintLocationSet.Contains(oppositeLocation))
                            {
                                blueprintLocationSet.Add(oppositeLocation);
                                blueprintMapEdgeSet.Add(edge.Blueprint);
                                blueprintLocationQueue.Enqueue(oppositeLocation);
                            }
                        }
            }

            foreach (var blueprint in blueprintLocationSet)
                if (blueprint.Type != LocationType.SystemWaypoint)
                    try
                    {
                        var locationData = globalMap.GetLocationData(blueprint);
                        locationData.EdgesOpened = true;
                        locationData.Reveal();
                        var locationObject = instance.GetLocationObject(blueprint);
                        if ((bool) instance)
                            if ((bool) locationObject)
                                instance.RevealLocation(locationObject);
                    }
                    catch (Exception ex)
                    {
                        modLogger.Log(ex.ToString());
                    }

            UpdateMap(instance, globalMap, blueprintMapEdgeSet);
        }
    }
}