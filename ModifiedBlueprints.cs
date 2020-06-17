using BagOfTricks.Utils;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using GL = UnityEngine.GUILayout;


namespace BagOfTricks {
    [Serializable]
    public class ModifiedItem {
        [JsonProperty]
        public int? m_Cost { get; set; } = null;
        [JsonProperty]
        public float? m_Weight { get; set; } = null;
    }
    [Serializable]
    public class ModifiedWeapon : ModifiedItem {
        [JsonProperty]
        public bool? m_OverrideDamageDice { get; set; } = null;
        [JsonProperty]
        public ModifiedDiceFormula m_DamageDice { get; set; } = null;
        [JsonProperty]
        public string[] m_Enchantments { get; set; } = null;
    }
    [Serializable]
    public class EnchantableEquipment : ModifiedItem {
        [JsonProperty]
        public string[] m_Enchantments { get; set; } = null;
    }

    [Serializable]
    public class ModifiedArmourType {
        [JsonProperty]
        public int? m_ArmorBonus { get; set; } = null;
        [JsonProperty]
        public int? m_ArmorChecksPenalty { get; set; } = null;
        [JsonProperty]
        public bool? m_HasDexterityBonusLimit { get; set; } = null;
        [JsonProperty]
        public int? m_MaxDexterityBonus { get; set; } = null;
        [JsonProperty]
        public int? m_ArcaneSpellFailureChance { get; set; } = null;
        [JsonProperty]
        public float? m_Weight { get; set; } = null;
    }
    [Serializable]
    public class ModifiedWeaponType {
        [JsonProperty]
        public AttackType? m_AttackType { get; set; } = null;
        [JsonProperty]
        public Feet? m_AttackRange { get; set; } = null;
        [JsonProperty]
        public ModifiedDiceFormula m_BaseDamage { get; set; } = null;
        [JsonProperty]
        public int? m_CriticalRollEdge { get; set; } = null;
        [JsonProperty]
        public DamageCriticalModifierType? m_CriticalModifier { get; set; } = null;
        [JsonProperty]
        public float? m_Weight { get; set; } = null;
        [JsonProperty]
        public bool? m_IsTwoHanded { get; set; } = null;
        [JsonProperty]
        public bool? m_IsLight { get; set; } = null;
        [JsonProperty]
        public bool? m_IsMonk { get; set; } = null;
        [JsonProperty]
        public bool? m_IsNatural { get; set; } = null;
        [JsonProperty]
        public bool? m_IsUnarmed { get; set; } = null;
        [JsonProperty]
        public bool? m_OverrideAttackBonusStat { get; set; } = null;
        [JsonProperty]
        public StatType? m_AttackBonusStatOverride { get; set; } = null;
    }
    [Serializable]
    public class ModifiedDiceFormula {
        [JsonProperty]
        public int? m_Rolls { get; set; } = null;
        [JsonProperty]
        public DiceType? m_Dice { get; set; } = null;
    }



    public static class ModifiedBlueprintTools {
        public static Settings settings = Main.settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;

        public static bool showModifiedBlueprints = false;
        public static bool blueprintLists = false;

        public static List<Type> blueprintItemCategory = new List<Type>
        {
            typeof(BlueprintItem),
            typeof(BlueprintItemKey),
            typeof(BlueprintItemNote),
            typeof(BlueprintItemArmor),
            typeof(BlueprintItemEquipmentBelt),
            typeof(BlueprintItemEquipmentFeet),
            typeof(BlueprintItemEquipmentGloves),
            typeof(BlueprintItemEquipmentHand),
            typeof(BlueprintItemEquipmentHandSimple),
            typeof(BlueprintItemEquipmentHead),
            typeof(BlueprintItemEquipmentNeck),
            typeof(BlueprintItemEquipmentRing),
            typeof(BlueprintItemEquipmentShoulders),
            typeof(BlueprintItemEquipmentSimple),
            typeof(BlueprintItemEquipmentUsable),
            typeof(BlueprintItemEquipmentWrist),
            typeof(BlueprintItemShield),
            typeof(BlueprintItemWeapon),
        };
        public static List<Type> blueprintEnchantableEquipmentCategory = new List<Type>
        {
            typeof(BlueprintItemArmor),
            typeof(BlueprintItemEquipmentBelt),
            typeof(BlueprintItemEquipmentFeet),
            typeof(BlueprintItemEquipmentGloves),
            typeof(BlueprintItemEquipmentHead),
            typeof(BlueprintItemEquipmentNeck),
            typeof(BlueprintItemEquipmentRing),
            typeof(BlueprintItemEquipmentShoulders),
            typeof(BlueprintItemEquipmentWrist),
        };
        public static Type blueprintItemWeapon = typeof(BlueprintItemWeapon);

        public static List<Type> blueprintTypeArmourCategory = new List<Type>
        {
            typeof(BlueprintArmorType),
            typeof(BlueprintShieldType),
        };
        public static Type blueprintWeaponType = typeof(BlueprintWeaponType);

        public static List<string> blueprintsPaths = new List<string>();
        public static List<string> blueprintsNames = new List<string>();
        public static List<string> blueprintsTypes = new List<string>();

        public static bool showItemTypes = false;
        public static bool refreshItemTypes = true;
        public static string[] itemTypes = { "BlueprintArmorType", "BlueprintShieldType", "BlueprintWeaponType" };
        public static SelectionGrid itemTypesGrid = new SelectionGrid(itemTypes, 3);
        public static List<bool> itemTypeEdit = new List<bool>();
        public static int selectedItemTypeOld = 0;

        public static void RenderMenu() {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.MainCategoryFormat(Strings.GetText("mainCategory_BlueprintModding")));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton("BlueprintModdingRender");
            GL.EndHorizontal();


            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("misc_Enable", "tooltip_BlueprintModding", $"{ settings.toggleItemModding}" + " ", "", false), GL.ExpandWidth(false))) {
                if (settings.toggleItemModding == Storage.isFalseString) {
                    settings.toggleItemModding = Storage.isTrueString;
                    ModifiedBlueprintTools.Patch();
                }
                else if (settings.toggleItemModding == Storage.isTrueString) {
                    settings.toggleItemModding = Storage.isFalseString;
                }
            }
            GL.EndHorizontal();

            if (Strings.ToBool(settings.toggleItemModding)) {
                MenuTools.SingleLineLabel(Strings.GetText("label_ItemModdingInfo"));

                GL.BeginHorizontal();
                if (GL.Button(new GUIContent("spacehamster's JSON Blueprint Dump on github", "https://github.com/spacehamster/KingmakerCustomBlueprints/releases/tag/blueprints"), GL.ExpandWidth(false))) {
                    Application.OpenURL("https://github.com/spacehamster/KingmakerCustomBlueprints/releases/tag/blueprints");
                }
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                if (GL.Button(MenuTools.TextWithTooltip("button_PatchManually", "tooltip_PatchManually", true), GL.ExpandWidth(false))) {
                    Patch();
                }
                GL.EndHorizontal();


                GL.Space(10);

                ItemTypesMenu();

                GL.Space(10);

                showModifiedBlueprints = GL.Toggle(showModifiedBlueprints, RichText.Bold(Strings.GetText("toggle_ShowModifiedItems")));
                if (showModifiedBlueprints) {
                    GL.Space(10);

                    GL.BeginHorizontal();
                    if (GL.Button(RichText.Bold(Strings.GetText("button_LoadRefresh")), GL.ExpandWidth(false))) {
                        blueprintLists = false;
                    }
                    GL.EndHorizontal();

                    GL.Space(10);

                    try {

                        if (!blueprintLists) {
                            blueprintsPaths.Clear();
                            blueprintsNames.Clear();
                            blueprintsTypes.Clear();
                            string path = Storage.modEntryPath + Storage.modifiedBlueprintsFolder;
                            DirectoryInfo directory = new DirectoryInfo(path);
                            if (directory.GetFiles("*.json").Any()) {
                                foreach (FileInfo file in directory.GetFiles("*.json")) {

                                    string json = File.ReadAllText(file.FullName);
                                    string guid = Path.GetFileNameWithoutExtension(file.Name);

                                    if (guid == "Example" && directory.GetFiles("*.json").Count() == 1) {
                                        MenuTools.SingleLineLabel(Strings.GetText("message_NoModItems"));
                                        continue;
                                    }
                                    else if (guid == "Example" && directory.GetFiles("*.json").Count() > 1) {
                                        continue;
                                    }

                                    BlueprintScriptableObject blueprintScriptableObject = Utilities.GetBlueprintByGuid<BlueprintScriptableObject>(guid);

                                    if (blueprintScriptableObject != null) {

                                        if (blueprintItemCategory.Contains(blueprintScriptableObject.GetType()) || blueprintTypeArmourCategory.Contains(blueprintScriptableObject.GetType()) || blueprintScriptableObject.GetType() == blueprintWeaponType) {
                                            blueprintsPaths.Add(file.FullName);
                                            blueprintsNames.Add(blueprintScriptableObject.name);
                                            blueprintsTypes.Add(blueprintScriptableObject.GetType().ToString());

                                        }
                                    }

                                }
                            }
                            blueprintLists = true;
                        }

                        if (blueprintsPaths.Any()) {
                            for (int i = 0; i < blueprintsPaths.Count(); i++) {
                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label(blueprintsNames[i] + $" ({blueprintsTypes[i]})");
                                GL.FlexibleSpace();
                                if (GL.Button(MenuTools.TextWithTooltip("button_RemoveItemModification", "misc_RequiresRestart", true), GL.ExpandWidth(false))) {
                                    try {
                                        blueprintLists = false;
                                        File.Delete(blueprintsPaths[i]);
                                    }
                                    catch (Exception e) {
                                        modLogger.Log(e.ToString());
                                    }
                                }
                                GL.EndHorizontal();
                                GL.EndVertical();
                            }
                        }
                        else {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoModItems"));
                        }
                    }
                    catch (Exception e) {
                        modLogger.Log(e.ToString());
                    }

                }
            }
            GL.EndVertical();
        }


        public static TextFieldInt itemTypesTextFieldInt = new TextFieldInt();
        public static TextFieldFloat itemTypesTextFieldFloat = new TextFieldFloat();
        public static SelectionGrid diceTypesGrid = new SelectionGrid(Storage.diceTypes, 4);
        public static void ItemTypesMenu() {
            showItemTypes = GL.Toggle(showItemTypes, RichText.Bold(Strings.GetText("toggle_ShowItemTypes")));
            if (showItemTypes) {
                GL.Space(10);
                itemTypesGrid.Render();
                GL.Space(10);
                if (selectedItemTypeOld != itemTypesGrid.selected) {
                    selectedItemTypeOld = itemTypesGrid.selected;
                    refreshItemTypes = true;
                }
                switch (itemTypesGrid.selected) {
                    case 0:
                        List<BlueprintArmorType> blueprintArmorTypes = ResourcesLibrary.GetBlueprints<BlueprintArmorType>().ToList();
                        if (refreshItemTypes) {
                            itemTypeEdit.Clear();
                            foreach (BlueprintArmorType b in blueprintArmorTypes) {
                                itemTypeEdit.Add(false);
                            }
                            refreshItemTypes = false;
                        }

                        for (int i = 0; i < blueprintArmorTypes.Count(); i++) {
                            itemTypeEdit[i] = GL.Toggle(itemTypeEdit[i], blueprintArmorTypes[i].name);
                            if (itemTypeEdit[i]) {
                                GL.BeginVertical("box");

                                GL.BeginHorizontal();
                                GL.Label("m_ArcaneSpellFailureChance: " + blueprintArmorTypes[i].ArcaneSpellFailureChance.ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount, "m_ArcaneSpellFailureChance", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_ArmorBonus: " + blueprintArmorTypes[i].ArmorBonus.ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount, "m_ArmorBonus", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_ArmorChecksPenalty: " + blueprintArmorTypes[i].ArmorChecksPenalty.ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount, "m_ArmorChecksPenalty", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_HasDexterityBonusLimit: " + blueprintArmorTypes[i].HasDexterityBonusLimit.ToString());
                                SetModifiedValueButtonBool<ModifiedArmourType>("m_HasDexterityBonusLimit", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();


                                GL.BeginHorizontal();
                                GL.Label("m_MaxDexterityBonus: " + blueprintArmorTypes[i].MaxDexterityBonus.ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount, "m_MaxDexterityBonus", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_Weight: " + blueprintArmorTypes[i].Weight.ToString());
                                itemTypesTextFieldFloat.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldFloat.finalAmount, "m_Weight", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.EndVertical();
                            }
                        }
                        break;
                    case 1:
                        List<BlueprintShieldType> blueprintShieldTypes = ResourcesLibrary.GetBlueprints<BlueprintShieldType>().ToList();
                        if (refreshItemTypes) {
                            itemTypeEdit.Clear();
                            foreach (BlueprintShieldType b in blueprintShieldTypes) {
                                itemTypeEdit.Add(false);
                            }
                            refreshItemTypes = false;
                        }

                        for (int i = 0; i < blueprintShieldTypes.Count(); i++) {
                            itemTypeEdit[i] = GL.Toggle(itemTypeEdit[i], blueprintShieldTypes[i].name);
                            if (itemTypeEdit[i]) {
                                GL.BeginVertical("box");

                                GL.BeginHorizontal();
                                GL.Label("m_ArcaneSpellFailureChance: " + blueprintShieldTypes[i].ArcaneSpellFailureChance.ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount, "m_ArcaneSpellFailureChance", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_ArmorBonus: " + blueprintShieldTypes[i].ArmorBonus.ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount, "m_ArmorBonus", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_ArmorChecksPenalty: " + blueprintShieldTypes[i].ArmorChecksPenalty.ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount, "m_ArmorChecksPenalty", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_HasDexterityBonusLimit: " + blueprintShieldTypes[i].HasDexterityBonusLimit.ToString());
                                SetModifiedValueButtonBool<ModifiedArmourType>("m_HasDexterityBonusLimit", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();


                                GL.BeginHorizontal();
                                GL.Label("m_MaxDexterityBonus: " + blueprintShieldTypes[i].MaxDexterityBonus.ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount, "m_MaxDexterityBonus", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_Weight: " + blueprintShieldTypes[i].Weight.ToString());
                                itemTypesTextFieldFloat.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldFloat.finalAmount, "m_Weight", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.EndVertical();
                            }
                        }
                        break;
                    case 2:
                        List<BlueprintWeaponType> blueprintWeaponTypes = ResourcesLibrary.GetBlueprints<BlueprintWeaponType>().ToList();
                        if (refreshItemTypes) {
                            itemTypeEdit.Clear();
                            foreach (BlueprintWeaponType b in blueprintWeaponTypes) {
                                itemTypeEdit.Add(false);
                            }
                            refreshItemTypes = false;
                        }

                        for (int i = 0; i < blueprintWeaponTypes.Count(); i++) {
                            itemTypeEdit[i] = GL.Toggle(itemTypeEdit[i], blueprintWeaponTypes[i].name);
                            if (itemTypeEdit[i]) {
                                GL.BeginVertical("box");

                                GL.BeginHorizontal();
                                GL.Label("m_AttackType: " + blueprintWeaponTypes[i].AttackType.ToString());
                                SetModifiedValueButtonAttackType<ModifiedWeaponType>("m_AttackType", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();


                                GL.BeginHorizontal();
                                GL.Label("m_AttackRange: " + Traverse.Create(blueprintWeaponTypes[i]).Field("m_AttackRange").GetValue().ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedWeaponType>(itemTypesTextFieldInt.finalAmount.Feet(), "m_AttackRange", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();
                                MenuTools.SingleLineLabel("AttackRange = (m_AttackRange > 10) ? m_AttackRange : Math.Max(2, m_AttackRange - 4)");

                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label("m_BaseDamage: " + blueprintWeaponTypes[i].BaseDamage.ToString());
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                itemTypesTextFieldInt.RenderFieldNoGroup("misc_NumberOfRolls");
                                GL.EndHorizontal();

                                diceTypesGrid.Render();

                                GL.BeginHorizontal();
                                GL.FlexibleSpace();
                                SetModifiedValueButtonDiceFormula<ModifiedWeaponType>(itemTypesTextFieldInt.finalAmount, Common.IntToDiceType(diceTypesGrid.selected), "m_BaseDamage", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();
                                GL.EndVertical();

                                GL.BeginHorizontal();
                                GL.Label("m_CriticalRollEdge: " + blueprintWeaponTypes[i].CriticalRollEdge.ToString());
                                itemTypesTextFieldInt.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedWeaponType>(itemTypesTextFieldInt.finalAmount, "m_CriticalRollEdge", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_CriticalModifier: " + blueprintWeaponTypes[i].CriticalModifier.ToString());
                                SetModifiedValueDamageCriticalModifierType<ModifiedWeaponType>("m_CriticalModifier", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_Weight: " + blueprintWeaponTypes[i].Weight.ToString());
                                itemTypesTextFieldFloat.RenderFieldNoGroup();
                                SetModifiedValueButton<ModifiedWeaponType>(itemTypesTextFieldFloat.finalAmount, "m_Weight", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsTwoHanded: " + blueprintWeaponTypes[i].IsTwoHanded.ToString());
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsTwoHanded", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsLight: " + blueprintWeaponTypes[i].IsLight.ToString());
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsLight", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsMonk: " + blueprintWeaponTypes[i].IsMonk.ToString());
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsMonk", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsNatural: " + blueprintWeaponTypes[i].IsNatural.ToString());
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsNatural", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsUnarmed: " + blueprintWeaponTypes[i].IsUnarmed.ToString());
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsUnarmed", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_OverrideAttackBonusStat: " + Traverse.Create(blueprintWeaponTypes[i]).Field("m_OverrideAttackBonusStat").GetValue().ToString());
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_OverrideAttackBonusStat", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();


                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label("m_AttackBonusStatOverride: " + Traverse.Create(blueprintWeaponTypes[i]).Field("m_AttackBonusStatOverride").GetValue().ToString());
                                GL.EndHorizontal();
                                SetModifiedValueStatType<ModifiedWeaponType>("m_AttackBonusStatOverride", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndVertical();


                                GL.EndVertical();
                            }
                        }
                        break;
                }
            }
        }

        public static string CleanEnchantment(string s) {
            if (s.Contains(':')) {
                string[] split = s.Split(':'); // s = "Blueprint:GUID:BlueprintName"
                return split[1];
            }
            else {
                return s;
            }

        }
        public static void SetModifiedValueStatType<T>(string name, string guid) {
            StatType[] statTypes = (StatType[])Enum.GetValues(typeof(StatType));
            for (int i = 0; i < statTypes.Count(); i++) {
                switch (i) {
                    case 0:
                    case 4:
                    case 8:
                    case 12:
                    case 16:
                    case 20:
                    case 24:
                    case 28:
                    case 32:
                    case 36:
                        GL.BeginHorizontal();
                        break;
                }
                if (GL.Button(Strings.GetText("button_SetTo") + $" {statTypes[i]}", GL.Width(210f))) {
                    FileInfo file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid + ".json");
                    if (File.Exists(file.FullName)) {
                        T modifiedItem = ModifiedBlueprintTools.DeserialiseItem<T>(file);
                        Traverse.Create(modifiedItem).Property(name).SetValue(statTypes[i]);
                        string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }
                    else {
                        T modifiedItem = default(T);
                        modifiedItem = Activator.CreateInstance<T>();
                        Traverse.Create(modifiedItem).Property(name).SetValue(statTypes[i]);
                        string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }
                    ModifiedBlueprintTools.blueprintLists = false;
                    ModifiedBlueprintTools.Patch();
                }
                switch (i) {
                    case 3:
                    case 7:
                    case 11:
                    case 15:
                    case 19:
                    case 23:
                    case 27:
                    case 31:
                    case 35:
                    case 37:
                        GL.EndHorizontal();
                        break;
                }
            }
        }

        public static void SetModifiedValueButtonDiceFormula<T>(int rolls, DiceType dice, string name, string guid) {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {rolls} * {dice}", GL.ExpandWidth(false))) {
                ModifiedDiceFormula diceFormula = new ModifiedDiceFormula();
                diceFormula.m_Rolls = rolls;
                diceFormula.m_Dice = dice;
                FileInfo file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid + ".json");
                if (File.Exists(file.FullName)) {
                    T modifiedItem = ModifiedBlueprintTools.DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(diceFormula);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else {
                    T modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(diceFormula);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                ModifiedBlueprintTools.blueprintLists = false;
                ModifiedBlueprintTools.Patch();
            }
        }
        public static void SetModifiedValueDamageCriticalModifierType<T>(string name, string guid) {
            foreach (DamageCriticalModifierType critMod in (DamageCriticalModifierType[])Enum.GetValues(typeof(DamageCriticalModifierType))) {
                if (GL.Button(Strings.GetText("button_SetTo") + $" {critMod}", GL.ExpandWidth(false))) {
                    FileInfo file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid + ".json");
                    if (File.Exists(file.FullName)) {
                        T modifiedItem = ModifiedBlueprintTools.DeserialiseItem<T>(file);
                        Traverse.Create(modifiedItem).Property(name).SetValue(critMod);
                        string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }
                    else {
                        T modifiedItem = default(T);
                        modifiedItem = Activator.CreateInstance<T>();
                        Traverse.Create(modifiedItem).Property(name).SetValue(critMod);
                        string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }
                    ModifiedBlueprintTools.blueprintLists = false;
                    ModifiedBlueprintTools.Patch();
                }
            }

        }

        public static void SetModifiedValueButtonAttackType<T>(string name, string guid) {
            foreach (AttackType attackType in (AttackType[])Enum.GetValues(typeof(AttackType))) {
                if (GL.Button(Strings.GetText("button_SetTo") + $" {attackType}", GL.ExpandWidth(false))) {
                    FileInfo file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid + ".json");
                    if (File.Exists(file.FullName)) {
                        T modifiedItem = ModifiedBlueprintTools.DeserialiseItem<T>(file);
                        Traverse.Create(modifiedItem).Property(name).SetValue(attackType);
                        string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }
                    else {
                        T modifiedItem = default(T);
                        modifiedItem = Activator.CreateInstance<T>();
                        Traverse.Create(modifiedItem).Property(name).SetValue(attackType);
                        string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }
                    ModifiedBlueprintTools.blueprintLists = false;
                    ModifiedBlueprintTools.Patch();
                }
            }
        }
        public static void SetModifiedValueButtonBool<T>(string name, string guid) {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {Strings.GetText("misc_True")}", GL.ExpandWidth(false))) {
                FileInfo file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid + ".json");
                if (File.Exists(file.FullName)) {
                    T modifiedItem = ModifiedBlueprintTools.DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(true);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else {
                    T modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(true);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                ModifiedBlueprintTools.blueprintLists = false;
                ModifiedBlueprintTools.Patch();
            }
            GL.Space(10);
            if (GL.Button(Strings.GetText("button_SetTo") + $" {Strings.GetText("misc_False")}", GL.ExpandWidth(false))) {
                FileInfo file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid + ".json");
                if (File.Exists(file.FullName)) {
                    T modifiedItem = ModifiedBlueprintTools.DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(false);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else {
                    T modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(false);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                ModifiedBlueprintTools.blueprintLists = false;
                ModifiedBlueprintTools.Patch();
            }
        }
        public static void SetModifiedValueButton<T>(Feet value, string name, string guid) {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {value}", GL.ExpandWidth(false))) {
                FileInfo file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid + ".json");
                if (File.Exists(file.FullName)) {
                    T modifiedItem = ModifiedBlueprintTools.DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else {
                    T modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                ModifiedBlueprintTools.blueprintLists = false;
                ModifiedBlueprintTools.Patch();
            }
        }
        public static void SetModifiedValueButton<T>(int value, string name, string guid) {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {value}", GL.ExpandWidth(false))) {
                FileInfo file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid + ".json");
                if (File.Exists(file.FullName)) {
                    T modifiedItem = ModifiedBlueprintTools.DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else {
                    T modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                ModifiedBlueprintTools.blueprintLists = false;
                ModifiedBlueprintTools.Patch();
            }
        }
        public static void SetModifiedValueButton<T>(float value, string name, string guid) {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {value}", GL.ExpandWidth(false))) {
                FileInfo file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid + ".json");
                if (File.Exists(file.FullName)) {
                    T modifiedItem = ModifiedBlueprintTools.DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else {
                    T modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    string json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                ModifiedBlueprintTools.blueprintLists = false;
                ModifiedBlueprintTools.Patch();
            }
        }

        public static T DeserialiseItem<T>(FileInfo file) {
            object modifiedItem = new object();
            using (StreamReader reader = file.OpenText()) {
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Include;
                modifiedItem = (T)serializer.Deserialize(reader, typeof(T));
            }
            return (T)modifiedItem;
        }

        public static void SetBlueprintField(object blueprint, object modifiedItem, string modifiedValue) {
            if (Traverse.Create(modifiedItem).Property(modifiedValue).GetValue() != null) {
                Traverse.Create(blueprint).Field(modifiedValue).SetValue(Traverse.Create(modifiedItem).Property(modifiedValue).GetValue());

                Common.ModLoggerDebug($"{blueprint} {modifiedValue} set to {Traverse.Create(modifiedItem).Property(modifiedValue).GetValue()}.");
            }
        }

        public static void PatchItem<T>(FileInfo file, BlueprintScriptableObject blueprintScriptableObject, string guid) {
            Common.ModLoggerDebug($"{guid} -> {blueprintScriptableObject} ({blueprintScriptableObject.GetType()})");

            T jsonItem = DeserialiseItem<T>(file);
            var blueprint = Convert.ChangeType(blueprintScriptableObject, blueprintScriptableObject.GetType());
            foreach (PropertyInfo property in jsonItem.GetType().GetProperties()) {
                if (property.Name == "m_BaseDamage" && Traverse.Create(jsonItem).Property("m_BaseDamage").GetValue() != null) {
                    ModifiedDiceFormula dice = (ModifiedDiceFormula)property.GetValue(jsonItem);
                    DiceFormula diceFormula = new DiceFormula((int)dice.m_Rolls, (DiceType)dice.m_Dice);
                    Traverse.Create(blueprint).Field("m_BaseDamage").SetValue(diceFormula);

                    Common.ModLoggerDebug($"{blueprint} m_BaseDamage set to {diceFormula}.");

                }
                else if (property.Name == "m_DamageDice" && Traverse.Create(jsonItem).Property("m_DamageDice").GetValue() != null) {
                    ModifiedDiceFormula dice = (ModifiedDiceFormula)property.GetValue(jsonItem);
                    DiceFormula diceFormula = new DiceFormula((int)dice.m_Rolls, (DiceType)dice.m_Dice);
                    Traverse.Create(blueprint).Field("m_DamageDice").SetValue(diceFormula);

                    Common.ModLoggerDebug($"{blueprint} m_DamageDice set to {diceFormula}.");

                }
                else if (property.Name == "m_Enchantments" && Traverse.Create(jsonItem).Property("m_Enchantments").GetValue() != null) {
                    if (typeof(T) == typeof(EnchantableEquipment)) {
                        string[] enchantmentsGUIDS = (string[])Traverse.Create(jsonItem).Property("m_Enchantments").GetValue();
                        BlueprintEquipmentEnchantment[] enchantmentsBlueprints = new BlueprintEquipmentEnchantment[enchantmentsGUIDS.Length];
                        for (int i = 0; i < enchantmentsGUIDS.Length; i++) {
                            string enchantmentGUID = CleanEnchantment(enchantmentsGUIDS[i]);
                            enchantmentsBlueprints[i] = (BlueprintEquipmentEnchantment)Utilities.GetBlueprintByGuid<BlueprintEquipmentEnchantment>(enchantmentGUID);
                        }

                        Traverse.Create(blueprint).Field("m_Enchantments").SetValue(enchantmentsBlueprints);

                        Common.ModLoggerDebug($"{blueprint} m_Enchantments set to {enchantmentsBlueprints}.");


                    }
                    else if (typeof(T) == typeof(ModifiedWeapon)) {
                        string[] enchantmentsGUIDS = (string[])Traverse.Create(jsonItem).Property("m_Enchantments").GetValue();
                        BlueprintWeaponEnchantment[] enchantmentsBlueprints = new BlueprintWeaponEnchantment[enchantmentsGUIDS.Length];
                        for (int i = 0; i < enchantmentsGUIDS.Length; i++) {
                            string enchantmentGUID = CleanEnchantment(enchantmentsGUIDS[i]);
                            enchantmentsBlueprints[i] = (BlueprintWeaponEnchantment)Utilities.GetBlueprintByGuid<BlueprintWeaponEnchantment>(enchantmentGUID);
                        }
                        Traverse.Create(blueprint).Field("m_Enchantments").SetValue(enchantmentsBlueprints);

                        Common.ModLoggerDebug($"{blueprint} m_Enchantments set to {enchantmentsBlueprints}.");

                    }
                }
                else {
                    SetBlueprintField(blueprint, jsonItem, property.Name);
                }
            }
        }

        public static void Patch() {
            if (Strings.ToBool(settings.toggleItemModding)) {
                try {
                    string path = Storage.modEntryPath + Storage.modifiedBlueprintsFolder;
                    DirectoryInfo directory = new DirectoryInfo(path);

                    foreach (FileInfo file in directory.GetFiles("*.json")) {
                        bool example = false;

                        Common.ModLoggerDebug($"{file.Name}");

                        string json = File.ReadAllText(file.FullName);
                        string guid = Path.GetFileNameWithoutExtension(file.Name);

                        if (guid.Contains("Example")) {

                            Common.ModLoggerDebug($"Ignoring {file.Name}");

                            example = true;
                        }

                        if (!example) {
                            BlueprintScriptableObject blueprintScriptableObject = Utilities.GetBlueprintByGuid<BlueprintScriptableObject>(guid);


                            if (blueprintScriptableObject != null) {

                                if (blueprintItemCategory.Contains(blueprintScriptableObject.GetType())) {
                                    if (blueprintScriptableObject.GetType() == blueprintItemWeapon) {
                                        PatchItem<ModifiedWeapon>(file, blueprintScriptableObject, guid);
                                    }
                                    else if (blueprintEnchantableEquipmentCategory.Contains(blueprintScriptableObject.GetType())) {
                                        PatchItem<EnchantableEquipment>(file, blueprintScriptableObject, guid);
                                    }
                                    else {
                                        PatchItem<ModifiedItem>(file, blueprintScriptableObject, guid);
                                    }
                                }
                                else if (blueprintTypeArmourCategory.Contains(blueprintScriptableObject.GetType())) {
                                    PatchItem<ModifiedArmourType>(file, blueprintScriptableObject, guid);
                                }
                                else if (blueprintScriptableObject.GetType() == blueprintWeaponType) {
                                    PatchItem<ModifiedWeaponType>(file, blueprintScriptableObject, guid);
                                }
                                else {
                                    modLogger.Log($"'{blueprintScriptableObject.GetType()}' {Strings.GetText("error_NotFound")}");

                                }

                            }
                            else {
                                modLogger.Log($"'{guid}' {Strings.GetText("error_NotFound")}");
                            }
                        }
                    }
                }
                catch (Exception e) {
                    modLogger.Log(e.ToString());
                }

            }
        }
    }
}
