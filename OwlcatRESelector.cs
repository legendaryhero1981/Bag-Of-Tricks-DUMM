using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;

namespace BagOfTricks
{
    public static class OwlcatRESelector
    {
        public static int GetXp(int cr)
        {
            return ExperienceHelper.GetXp(EncounterType.Mob, cr);
        }

        public static int GetXp(BlueprintUnit unit)
        {
            return GetXp(GetCR(unit));
        }

        public static int GetCR(BlueprintUnit unit)
        {
            var component = unit.GetComponent<Experience>();
            if (component != null)
                return component.CR;
            return 0;
        }


        public static List<BlueprintUnit> SelectUnits(int cr, UnitTag tag)
        {
            var minCR = cr - 6;
            var list = BlueprintRoot.Instance.RE.UnitsForRandomEncounters
                .Where(u => ContainsTag(u.GetComponent<AddTags>(), tag)).Where(u => GetCR(u) >= minCR).ToList();
            var xp = GetXp(cr);
            var maxTotalXp = GetXp(cr + 1);
            var currentXp = 0;
            var blueprintUnitList = new List<BlueprintUnit>();
            while (currentXp < xp)
            {
                list.RemoveAll(u => GetXp(u) > maxTotalXp - currentXp);
                if (list.Count != 0)
                {
                    var unit = list.Random();
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
            if (unit == null)
                return false;
            for (var index = 0; index < unit.Tags.Length; ++index)
                if (tag == unit.Tags[index])
                    return true;
            return false;
        }
    }
}