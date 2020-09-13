using Kingmaker;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using System.Collections.Generic;
using System.Linq;

namespace BagOfTricks.Utils.Kingmaker
{
    static class UnitEntityDataUtils {


        public static float GetMaxSpeed(List<UnitEntityData> data) {
            return (data.Select((u => u.ModifiedSpeedMps)).Max());
        }

        public static bool CheckUnitEntityData(UnitEntityData unitEntityData, UnitSelectType selectType) {
            if (unitEntityData == null) {
                return false;
            }
            switch (selectType) {
                case UnitSelectType.Everyone:
                    return true;
                case UnitSelectType.Party:
                    if (unitEntityData.IsPlayerFaction) {
                        return true;
                    }
                    return false;
                case UnitSelectType.MainCharacter:
                    if (unitEntityData.IsMainCharacter) {
                        return true;
                    }
                    return false;
                case UnitSelectType.Enemies:
                    if (!unitEntityData.IsPlayerFaction && unitEntityData.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot.PlayerFaction)) {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public static void Kill(UnitEntityData unit) {
            unit.Descriptor.Damage = unit.Descriptor.Stats.HitPoints.ModifiedValue + unit.Descriptor.Stats.TemporaryHitPoints.ModifiedValue;
        }
        public static void ForceKill(UnitEntityData unit) {
            unit.Descriptor.State.ForceKill = true;
        }
        public static void ResurrectAndFullRestore(UnitEntityData unit) {
            unit.Descriptor.ResurrectAndFullRestore();
        }
        public static void Buff(UnitEntityData unit, string buffGuid) {
            unit.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(buffGuid), (MechanicsContext)null, new FeatureParam());
        }
        public static void Charm(UnitEntityData unit) {
            if (unit != null) {
                unit.Descriptor.SwitchFactions(Game.Instance.BlueprintRoot.PlayerFaction, true);
            }
            else {
                Common.ModLoggerDebug("Unit is null!");
            }
        }
        public static void AddToParty(UnitEntityData unit) {
            Charm(unit);
            Game.Instance.Player.AddCompanion(unit);
        }
    }
}
