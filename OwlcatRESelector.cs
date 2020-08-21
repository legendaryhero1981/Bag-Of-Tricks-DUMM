using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;

namespace BagOfTricks {
    public static class OwlcatRESelector {

        public static int GetXp(int cr) {
            return ExperienceHelper.GetXp(EncounterType.Mob, cr, 1f, (IntEvaluator)null);
        }

        public static int GetXp(BlueprintUnit unit) {
            return OwlcatRESelector.GetXp(OwlcatRESelector.GetCR(unit));
        }

        public static int GetCR(BlueprintUnit unit) {
            Experience component = unit.GetComponent<Experience>();
            if (component != null)
                return component.CR;
            return 0;
        }


        public static List<BlueprintUnit> SelectUnits(int cr, UnitTag tag) {
            int minCR = cr - 6;
            List<BlueprintUnit> list = BlueprintRoot.Instance.RE.UnitsForRandomEncounters.Where<BlueprintUnit>((Func<BlueprintUnit, bool>)(u => OwlcatRESelector.ContainsTag(u.GetComponent<AddTags>(), tag))).Where<BlueprintUnit>((Func<BlueprintUnit, bool>)(u => OwlcatRESelector.GetCR(u) >= minCR)).ToList<BlueprintUnit>();
            int xp = OwlcatRESelector.GetXp(cr);
            int maxTotalXp = OwlcatRESelector.GetXp(cr + 1);
            int currentXp = 0;
            List<BlueprintUnit> blueprintUnitList = new List<BlueprintUnit>();
            while (currentXp < xp) {
                list.RemoveAll((Predicate<BlueprintUnit>)(u => OwlcatRESelector.GetXp(u) > maxTotalXp - currentXp));
                if (list.Count != 0) {
                    BlueprintUnit unit = list.Random<BlueprintUnit>();
                    currentXp += OwlcatRESelector.GetXp(unit);
                    blueprintUnitList.Add(unit);
                }
                else
                    break;
            }
            return blueprintUnitList;
        }

        public static bool ContainsTag([CanBeNull] AddTags unit, UnitTag tag) {
            if (unit == null)
                return false;
            for (int index = 0; index < unit.Tags.Length; ++index) {
                if (tag == unit.Tags[index])
                    return true;
            }
            return false;
        }
    }

}
