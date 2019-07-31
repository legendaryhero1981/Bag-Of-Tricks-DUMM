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

namespace BagOfTricks
{
    public static class OwlcatRESelector
    {
        public static int GetXp(int cr)
        {
            return ExperienceHelper.GetXp(EncounterType.Mob, cr, 1f, (IntEvaluator) null);
        }

        public static int GetXp(BlueprintUnit unit)
        {
            return GetXp(GetCR(unit));
        }

        public static int GetCR(BlueprintUnit unit)
        {
            var component = unit.GetComponent<Experience>();
            if ((UnityEngine.Object) component != (UnityEngine.Object) null)
                return component.CR;
            return 0;
        }

        public static List<BlueprintUnit> SelectUnits(int cr, UnitTag tag)
        {
            var minCR = cr - 6;
            var list = BlueprintRoot.Instance.RE.UnitsForRandomEncounters
                .Where<BlueprintUnit>((Func<BlueprintUnit, bool>) (u => ContainsTag(u.GetComponent<AddTags>(), tag)))
                .Where<BlueprintUnit>((Func<BlueprintUnit, bool>) (u => GetCR(u) >= minCR)).ToList<BlueprintUnit>();
            var xp = GetXp(cr);
            var maxTotalXp = GetXp(cr + 1);
            var currentXp = 0;
            var blueprintUnitList = new List<BlueprintUnit>();
            while (currentXp < xp)
            {
                list.RemoveAll((Predicate<BlueprintUnit>) (u => GetXp(u) > maxTotalXp - currentXp));
                if (list.Count != 0)
                {
                    var unit = list.Random<BlueprintUnit>();
                    currentXp += GetXp(unit);
                    blueprintUnitList.Add(unit);
                }
                else
                {
                    break;
                }
            }

            return blueprintUnitList;
        }

        public static bool ContainsTag([CanBeNull] AddTags unit, UnitTag tag)
        {
            if ((UnityEngine.Object) unit == (UnityEngine.Object) null)
                return false;
            for (var index = 0; index < unit.Tags.Length; ++index)
                if (tag == unit.Tags[index])
                    return true;
            return false;
        }
    }
}