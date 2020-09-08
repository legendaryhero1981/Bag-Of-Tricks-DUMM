using Harmony;
using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using System.Collections.Generic;
using System.Linq;

namespace BagOfTricks.Utils.HarmonyPatches {
    static class SpidersBegone {
        private const string prefabRatSwarmGUID = "ccd9a62b2c6cb304d9abf10f0d95d746";
        private const string prefabWolfBlackGUID = "0dc0f602a83a2034ba5842f73c0012c1";
        private const string typeSpiderGUID = "243702bdc53e2574aaa34d1e3eafe6aa";
        private const string typeSpiderSwarmGUID = "0fd1473096fbdda4db770cca8366c5e1";
        private const string blueprintWolfStandardGUID = "cc124e00e9ed4a441bc10de414f02312";
        private const string blueprintCR2RatSwarmGUID = "12a5944fa27307e4e8b6f56431d5cc8c";

        private static readonly string[] spiderGuids = new string[]
        {
            "ae2806b1e73ed7b4e9e9ae966de4dad6", // Corrupted_BloomInfusedSpiderMatriarch
            "d0e28afa4e4c0994cb6deae66612445a", // CR1_GiantSpiderMite
            "c4b33e5fd3d3a6f46b2aade647b0bf25", // CR1_GiantSpiderStandard
            "a28e944558ed5b64790c3701e8c89d75", // CR1_SpiderSwarm
            "e9c1c68972cc4904dacdf2df9acf6730", // CR2_GiantSpiderAdvanced
            "18a3ceeb3fb44f24ea6d3035a5f05a8c", // CR2_GiantSpiderMiteAdvanced
            "ba9451623f3f13742a8bd12da4822d2b", // CR4_GiantSpider
            "da2f152d19ce4d54e8c17da91f01fabd", // CR5_QuickSpiderSwarm
            "a813d907bc55e734584d99a038c9211e", // CR6_BloomInfusedSpider
            "51c66b0783a748c4b9538f0f0678c4d7", // CR7_GiantSpiderDoombringing
            "07467e9a29a215346ab66fec7963eb62", // CR7_GiantSpiderQuickling
            "63897b4df57da2f4396ca8a6f34723e7", // CR11_BloomInfusedSpiderMatriarch
            "ed734a6c822bdac448b98abfd7c03814", // GiantSpider_Cutscene
            "4622aca7715007147b26b7fc26db3df8", // GiantSpiderBloomInfused
            "5bb1781abca825f49a53869ff79a2c6f", // GiantSpiderMiniboss
            "9e120b5e0ad3c794491c049aa24b9fde", // GiantSpiderSummoned
            "f2327e24765fb6342975b6216bfb307b", // SpiderSwarmSummoned
            "36fe14f64d7746f429d30f9dd7b2e652", // SycamoreBoss_GiantSpider
            "af6700830836d9643939e1a1801a65af", // TrollLairFW_Zzamas
            "254091b112392c04db701331fbea3b8f", // Zzamas
        };

        public static string[] GetSpiderGuids => spiderGuids;

        public static void CheckAndReplace(ref UnitEntityData unitEntityData) {
            if (IsSpiderType(unitEntityData.Blueprint.Type?.AssetGuidThreadSafe)) {
                unitEntityData.Descriptor.CustomPrefabGuid = prefabWolfBlackGUID;
            }
            else if (IsSpiderSwarmType(unitEntityData.Blueprint.Type?.AssetGuidThreadSafe)) {
                unitEntityData.Descriptor.CustomPrefabGuid = prefabRatSwarmGUID;
            }
            else if (IsSpiderBlueprintUnit(unitEntityData.Blueprint.AssetGuidThreadSafe)) {
                unitEntityData.Descriptor.CustomPrefabGuid = prefabWolfBlackGUID;
            }
        }

        public static void CheckAndReplace(ref BlueprintUnit blueprintUnit) {
            if (IsSpiderType(blueprintUnit.Type?.AssetGuidThreadSafe)) {
                blueprintUnit.Prefab = Utilities.GetBlueprintByGuid<BlueprintUnit>(blueprintWolfStandardGUID).Prefab;
            }
            else if (IsSpiderSwarmType(blueprintUnit.Type?.AssetGuidThreadSafe)) {
                blueprintUnit.Prefab = Utilities.GetBlueprintByGuid<BlueprintUnit>(blueprintCR2RatSwarmGUID).Prefab;
            }
            else if (IsSpiderBlueprintUnit(blueprintUnit.AssetGuidThreadSafe)) {
                blueprintUnit.Prefab = Utilities.GetBlueprintByGuid<BlueprintUnit>(blueprintWolfStandardGUID).Prefab;
            }
        }

        private static bool IsSpiderType(string typeGuid) {
            return typeGuid == typeSpiderGUID;
        }

        private static bool IsSpiderSwarmType(string typeGuid) {
            return typeGuid == typeSpiderSwarmGUID;
        }

        private static bool IsSpiderBlueprintUnit(string blueprintUnitGuid) {
            return spiderGuids.Contains(blueprintUnitGuid);

        }
    }
}
