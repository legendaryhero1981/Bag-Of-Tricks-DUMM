using System;
using Kingmaker.Blueprints.Facts;


namespace BagOfTricks.Utils.Kingmaker {
    static class BlueprintUtils {
        public static string GetName(BlueprintUnitFact blueprintUnitFact) {
            return !String.IsNullOrEmpty(blueprintUnitFact.Name) ? blueprintUnitFact.Name : (!String.IsNullOrEmpty(blueprintUnitFact.name) ? blueprintUnitFact.name : null);
        }
    }
}
