namespace BagOfTricks
{
    public static class Defaults
    {
        public static readonly float randomEncounterChanceOnGlobalMap = 0.3f;
        public static readonly float randomEncounterChanceOnCamp = 0.4f;
        public static readonly float randomEncounterChanceOnCampSecondTime = 0.1f;
        public static readonly float randomEncounterHardEncounterChance = 0.05f;
        public static readonly float randomEncounterHardEncounterMaxChance = 0.9f;
        public static readonly float randomEncounterHardEncounterChanceIncrease = 0.05f;
        public static readonly float randomEncounterStalkerAmbushChance = 0.25f;

        public static readonly float randomEncounterRollMiles = 8f;
        public static readonly float randomEncounterSafeMilesAfterEncounter = 16f;
        public static readonly float randomEncounterDefaultSafeZoneSize = 4f;
        public static readonly float randomEncounterEncounterPawnOffset = 1.5f;
        public static readonly float randomEncounterEncounterPawnDistanceFromLocation = 1.5f;

        public static readonly int randomEncounterEncounterMinBonusCR = -1;
        public static readonly int randomEncounterEncounterMaxBonusCR = 1;
        public static readonly int randomEncounterHardEncounterBonusCR = 3;
        public static readonly int randomEncounterRandomEncounterAvoidanceFailMargin = 5;

        public static readonly string fovMin = "10";
        public static readonly string fovMax = "22";
        public static readonly string fovGlobalMap = "28.3";

        public static readonly float sillyBloddChance = 0.1f;
        public static readonly float debugTimeScale = 1f;

        public static readonly float artisanMasterpieceChance = 0.33f;
        public static readonly int defaultMapResourceCost = 15;
    }
}