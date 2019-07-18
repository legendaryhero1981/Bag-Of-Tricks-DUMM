using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using UnityEngine;
using UnityModManagerNet;
using GL = UnityEngine.GUILayout;


namespace BagOfTricks
{
    [Serializable]
    public class ModifiedItem
    {
        [JsonProperty] public int? m_Cost { get; set; }

        [JsonProperty] public float? m_Weight { get; set; }
    }

    [Serializable]
    public class ModifiedWeapon : ModifiedItem
    {
        [JsonProperty] public bool? m_OverrideDamageDice { get; set; }

        [JsonProperty] public ModifiedDiceFormula m_DamageDice { get; set; }

        [JsonProperty] public string[] m_Enchantments { get; set; }
    }

    [Serializable]
    public class EnchantableEquipment : ModifiedItem
    {
        [JsonProperty] public string[] m_Enchantments { get; set; }
    }

    [Serializable]
    public class ModifiedArmourType
    {
        [JsonProperty] public int? m_ArmorBonus { get; set; }

        [JsonProperty] public int? m_ArmorChecksPenalty { get; set; }

        [JsonProperty] public bool? m_HasDexterityBonusLimit { get; set; }

        [JsonProperty] public int? m_MaxDexterityBonus { get; set; }

        [JsonProperty] public int? m_ArcaneSpellFailureChance { get; set; }

        [JsonProperty] public float? m_Weight { get; set; }
    }

    [Serializable]
    public class ModifiedWeaponType
    {
        [JsonProperty] public AttackType? m_AttackType { get; set; }

        [JsonProperty] public Feet? m_AttackRange { get; set; }

        [JsonProperty] public ModifiedDiceFormula m_BaseDamage { get; set; }

        [JsonProperty] public int? m_CriticalRollEdge { get; set; }

        [JsonProperty] public DamageCriticalModifierType? m_CriticalModifier { get; set; }

        [JsonProperty] public float? m_Weight { get; set; }

        [JsonProperty] public bool? m_IsTwoHanded { get; set; }

        [JsonProperty] public bool? m_IsLight { get; set; }

        [JsonProperty] public bool? m_IsMonk { get; set; }

        [JsonProperty] public bool? m_IsNatural { get; set; }

        [JsonProperty] public bool? m_IsUnarmed { get; set; }

        [JsonProperty] public bool? m_OverrideAttackBonusStat { get; set; }

        [JsonProperty] public StatType? m_AttackBonusStatOverride { get; set; }
    }

    [Serializable]
    public class ModifiedDiceFormula
    {
        [JsonProperty] public int? m_Rolls { get; set; }

        [JsonProperty] public DiceType? m_Dice { get; set; }
    }


    public static class ModifiedBlueprintTools
    {
        public static Settings settings = Main.settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;

        public static bool showModifiedBlueprints;
        public static bool blueprintLists;

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
            typeof(BlueprintItemWeapon)
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
            typeof(BlueprintItemEquipmentWrist)
        };

        public static Type blueprintItemWeapon = typeof(BlueprintItemWeapon);

        public static List<Type> blueprintTypeArmourCategory = new List<Type>
        {
            typeof(BlueprintArmorType),
            typeof(BlueprintShieldType)
        };

        public static Type blueprintWeaponType = typeof(BlueprintWeaponType);

        public static List<string> blueprintsPaths = new List<string>();
        public static List<string> blueprintsNames = new List<string>();
        public static List<string> blueprintsTypes = new List<string>();

        public static bool showItemTypes;
        public static bool refreshItemTypes = true;
        public static string[] itemTypes = {"BlueprintArmorType", "BlueprintShieldType", "BlueprintWeaponType"};
        public static SelectionGrid itemTypesGrid = new SelectionGrid(itemTypes, 3);
        public static List<bool> itemTypeEdit = new List<bool>();
        public static int selectedItemTypeOld;


        public static TextFieldInt itemTypesTextFieldInt = new TextFieldInt();
        public static TextFieldFloat itemTypesTextFieldFloat = new TextFieldFloat();
        public static SelectionGrid diceTypesGrid = new SelectionGrid(Storage.diceTypes, 4);

        public static void RenderMenu()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.MainCategoryFormat(Strings.GetText("mainCategory_BlueprintModding")));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton("BlueprintModdingRender");
            GL.EndHorizontal();


            GL.BeginHorizontal();
            if (GL.Button(
                MenuTools.TextWithTooltip("misc_Enable", "tooltip_BlueprintModding",
                    $"{settings.toggleItemModding}" + " "), GL.ExpandWidth(false)))
            {
                if (settings.toggleItemModding == Storage.isFalseString)
                {
                    settings.toggleItemModding = Storage.isTrueString;
                    Patch();
                }
                else if (settings.toggleItemModding == Storage.isTrueString)
                {
                    settings.toggleItemModding = Storage.isFalseString;
                }
            }

            GL.EndHorizontal();

            if (Strings.ToBool(settings.toggleItemModding))
            {
                MenuTools.SingleLineLabel(Strings.GetText("label_ItemModdingInfo"));

                GL.BeginHorizontal();
                if (GL.Button(
                    new GUIContent("spacehamster's JSON Blueprint Dump on github",
                        "https://github.com/spacehamster/KingmakerCustomBlueprints/releases/tag/blueprints"),
                    GL.ExpandWidth(false)))
                    Application.OpenURL(
                        "https://github.com/spacehamster/KingmakerCustomBlueprints/releases/tag/blueprints");
                GL.EndHorizontal();

                GL.Space(10);

                GL.BeginHorizontal();
                if (GL.Button(MenuTools.TextWithTooltip("button_PatchManually", "tooltip_PatchManually", true),
                    GL.ExpandWidth(false))) Patch();
                GL.EndHorizontal();


                GL.Space(10);

                ItemTypesMenu();

                GL.Space(10);

                showModifiedBlueprints = GL.Toggle(showModifiedBlueprints,
                    RichText.Bold(Strings.GetText("toggle_ShowModifiedItems")));
                if (showModifiedBlueprints)
                {
                    GL.Space(10);

                    GL.BeginHorizontal();
                    if (GL.Button(RichText.Bold(Strings.GetText("button_LoadRefresh")), GL.ExpandWidth(false)))
                        blueprintLists = false;
                    GL.EndHorizontal();

                    GL.Space(10);

                    try
                    {
                        if (!blueprintLists)
                        {
                            blueprintsPaths.Clear();
                            blueprintsNames.Clear();
                            blueprintsTypes.Clear();
                            var path = Storage.modEntryPath + Storage.modifiedBlueprintsFolder;
                            var directory = new DirectoryInfo(path);
                            if (directory.GetFiles("*.json").Any())
                                foreach (var file in directory.GetFiles("*.json"))
                                {
                                    var json = File.ReadAllText(file.FullName);
                                    var guid = Path.GetFileNameWithoutExtension(file.Name);

                                    if (guid == "Example" && directory.GetFiles("*.json").Count() == 1)
                                    {
                                        MenuTools.SingleLineLabel(Strings.GetText("message_NoModItems"));
                                        continue;
                                    }

                                    if (guid == "Example" && directory.GetFiles("*.json").Count() > 1) continue;

                                    var blueprintScriptableObject =
                                        Utilities.GetBlueprintByGuid<BlueprintScriptableObject>(guid);

                                    if (blueprintScriptableObject != null)
                                        if (blueprintItemCategory.Contains(blueprintScriptableObject.GetType()) ||
                                            blueprintTypeArmourCategory.Contains(blueprintScriptableObject.GetType()) ||
                                            blueprintScriptableObject.GetType() == blueprintWeaponType)
                                        {
                                            blueprintsPaths.Add(file.FullName);
                                            blueprintsNames.Add(blueprintScriptableObject.name);
                                            blueprintsTypes.Add(blueprintScriptableObject.GetType().ToString());
                                        }
                                }

                            blueprintLists = true;
                        }

                        if (blueprintsPaths.Any())
                            for (var i = 0; i < blueprintsPaths.Count(); i++)
                            {
                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label(blueprintsNames[i] + $" ({blueprintsTypes[i]})");
                                GL.FlexibleSpace();
                                if (GL.Button(
                                    MenuTools.TextWithTooltip("button_RemoveItemModification", "misc_RequiresRestart",
                                        true), GL.ExpandWidth(false)))
                                    try
                                    {
                                        blueprintLists = false;
                                        File.Delete(blueprintsPaths[i]);
                                    }
                                    catch (Exception e)
                                    {
                                        modLogger.Log(e.ToString());
                                    }

                                GL.EndHorizontal();
                                GL.EndVertical();
                            }
                        else
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoModItems"));
                    }
                    catch (Exception e)
                    {
                        modLogger.Log(e.ToString());
                    }
                }
            }

            GL.EndVertical();
        }

        public static void ItemTypesMenu()
        {
            showItemTypes = GL.Toggle(showItemTypes, RichText.Bold(Strings.GetText("toggle_ShowItemTypes")));
            if (showItemTypes)
            {
                GL.Space(10);
                itemTypesGrid.Render();
                GL.Space(10);
                if (selectedItemTypeOld != itemTypesGrid.selected)
                {
                    selectedItemTypeOld = itemTypesGrid.selected;
                    refreshItemTypes = true;
                }

                switch (itemTypesGrid.selected)
                {
                    case 0:
                        var blueprintArmorTypes = ResourcesLibrary.GetBlueprints<BlueprintArmorType>().ToList();
                        if (refreshItemTypes)
                        {
                            itemTypeEdit.Clear();
                            foreach (var b in blueprintArmorTypes) itemTypeEdit.Add(false);
                            refreshItemTypes = false;
                        }

                        for (var i = 0; i < blueprintArmorTypes.Count(); i++)
                        {
                            itemTypeEdit[i] = GL.Toggle(itemTypeEdit[i], blueprintArmorTypes[i].name);
                            if (itemTypeEdit[i])
                            {
                                GL.BeginVertical("box");

                                GL.BeginHorizontal();
                                GL.Label("m_ArcaneSpellFailureChance: " +
                                         blueprintArmorTypes[i].ArcaneSpellFailureChance);
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount,
                                    "m_ArcaneSpellFailureChance", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_ArmorBonus: " + blueprintArmorTypes[i].ArmorBonus);
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount,
                                    "m_ArmorBonus", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_ArmorChecksPenalty: " + blueprintArmorTypes[i].ArmorChecksPenalty);
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount,
                                    "m_ArmorChecksPenalty", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_HasDexterityBonusLimit: " + blueprintArmorTypes[i].HasDexterityBonusLimit);
                                SetModifiedValueButtonBool<ModifiedArmourType>("m_HasDexterityBonusLimit",
                                    blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();


                                GL.BeginHorizontal();
                                GL.Label("m_MaxDexterityBonus: " + blueprintArmorTypes[i].MaxDexterityBonus);
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount,
                                    "m_MaxDexterityBonus", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_Weight: " + blueprintArmorTypes[i].Weight);
                                itemTypesTextFieldFloat.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldFloat.finalAmount,
                                    "m_Weight", blueprintArmorTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.EndVertical();
                            }
                        }

                        break;
                    case 1:
                        var blueprintShieldTypes = ResourcesLibrary.GetBlueprints<BlueprintShieldType>().ToList();
                        if (refreshItemTypes)
                        {
                            itemTypeEdit.Clear();
                            foreach (var b in blueprintShieldTypes) itemTypeEdit.Add(false);
                            refreshItemTypes = false;
                        }

                        for (var i = 0; i < blueprintShieldTypes.Count(); i++)
                        {
                            itemTypeEdit[i] = GL.Toggle(itemTypeEdit[i], blueprintShieldTypes[i].name);
                            if (itemTypeEdit[i])
                            {
                                GL.BeginVertical("box");

                                GL.BeginHorizontal();
                                GL.Label("m_ArcaneSpellFailureChance: " +
                                         blueprintShieldTypes[i].ArcaneSpellFailureChance);
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount,
                                    "m_ArcaneSpellFailureChance", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_ArmorBonus: " + blueprintShieldTypes[i].ArmorBonus);
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount,
                                    "m_ArmorBonus", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_ArmorChecksPenalty: " + blueprintShieldTypes[i].ArmorChecksPenalty);
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount,
                                    "m_ArmorChecksPenalty", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_HasDexterityBonusLimit: " + blueprintShieldTypes[i].HasDexterityBonusLimit);
                                SetModifiedValueButtonBool<ModifiedArmourType>("m_HasDexterityBonusLimit",
                                    blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();


                                GL.BeginHorizontal();
                                GL.Label("m_MaxDexterityBonus: " + blueprintShieldTypes[i].MaxDexterityBonus);
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldInt.finalAmount,
                                    "m_MaxDexterityBonus", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_Weight: " + blueprintShieldTypes[i].Weight);
                                itemTypesTextFieldFloat.RenderNoGL();
                                SetModifiedValueButton<ModifiedArmourType>(itemTypesTextFieldFloat.finalAmount,
                                    "m_Weight", blueprintShieldTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.EndVertical();
                            }
                        }

                        break;
                    case 2:
                        var blueprintWeaponTypes = ResourcesLibrary.GetBlueprints<BlueprintWeaponType>().ToList();
                        if (refreshItemTypes)
                        {
                            itemTypeEdit.Clear();
                            foreach (var b in blueprintWeaponTypes) itemTypeEdit.Add(false);
                            refreshItemTypes = false;
                        }

                        for (var i = 0; i < blueprintWeaponTypes.Count(); i++)
                        {
                            itemTypeEdit[i] = GL.Toggle(itemTypeEdit[i], blueprintWeaponTypes[i].name);
                            if (itemTypeEdit[i])
                            {
                                GL.BeginVertical("box");

                                GL.BeginHorizontal();
                                GL.Label("m_AttackType: " + blueprintWeaponTypes[i].AttackType);
                                SetModifiedValueButtonAttackType<ModifiedWeaponType>("m_AttackType",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();


                                GL.BeginHorizontal();
                                GL.Label("m_AttackRange: " + Traverse.Create(blueprintWeaponTypes[i])
                                             .Field("m_AttackRange").GetValue());
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedWeaponType>(itemTypesTextFieldInt.finalAmount.Feet(),
                                    "m_AttackRange", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();
                                MenuTools.SingleLineLabel(
                                    "AttackRange = (m_AttackRange > 10) ? m_AttackRange : Math.Max(2, m_AttackRange - 4)");

                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label("m_BaseDamage: " + blueprintWeaponTypes[i].BaseDamage);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                itemTypesTextFieldInt.RenderNoGL("misc_NumberOfRolls");
                                GL.EndHorizontal();

                                diceTypesGrid.Render();

                                GL.BeginHorizontal();
                                GL.FlexibleSpace();
                                SetModifiedValueButtonDiceFormula<ModifiedWeaponType>(itemTypesTextFieldInt.finalAmount,
                                    Common.IntToDiceType(diceTypesGrid.selected), "m_BaseDamage",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();
                                GL.EndVertical();

                                GL.BeginHorizontal();
                                GL.Label("m_CriticalRollEdge: " + blueprintWeaponTypes[i].CriticalRollEdge);
                                itemTypesTextFieldInt.RenderNoGL();
                                SetModifiedValueButton<ModifiedWeaponType>(itemTypesTextFieldInt.finalAmount,
                                    "m_CriticalRollEdge", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_CriticalModifier: " + blueprintWeaponTypes[i].CriticalModifier);
                                SetModifiedValueDamageCriticalModifierType<ModifiedWeaponType>("m_CriticalModifier",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_Weight: " + blueprintWeaponTypes[i].Weight);
                                itemTypesTextFieldFloat.RenderNoGL();
                                SetModifiedValueButton<ModifiedWeaponType>(itemTypesTextFieldFloat.finalAmount,
                                    "m_Weight", blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsTwoHanded: " + blueprintWeaponTypes[i].IsTwoHanded);
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsTwoHanded",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsLight: " + blueprintWeaponTypes[i].IsLight);
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsLight",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsMonk: " + blueprintWeaponTypes[i].IsMonk);
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsMonk",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsNatural: " + blueprintWeaponTypes[i].IsNatural);
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsNatural",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_IsUnarmed: " + blueprintWeaponTypes[i].IsUnarmed);
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_IsUnarmed",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();

                                GL.BeginHorizontal();
                                GL.Label("m_OverrideAttackBonusStat: " + Traverse.Create(blueprintWeaponTypes[i])
                                             .Field("m_OverrideAttackBonusStat").GetValue());
                                SetModifiedValueButtonBool<ModifiedWeaponType>("m_OverrideAttackBonusStat",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndHorizontal();


                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label("m_AttackBonusStatOverride: " + Traverse.Create(blueprintWeaponTypes[i])
                                             .Field("m_AttackBonusStatOverride").GetValue());
                                GL.EndHorizontal();
                                SetModifiedValueStatType<ModifiedWeaponType>("m_AttackBonusStatOverride",
                                    blueprintWeaponTypes[i].AssetGuid);
                                GL.EndVertical();


                                GL.EndVertical();
                            }
                        }

                        break;
                }
            }
        }

        public static string CleanEnchantment(string s)
        {
            if (s.Contains(':'))
            {
                var split = s.Split(':'); // s = "Blueprint:GUID:BlueprintName"
                return split[1];
            }

            return s;
        }

        public static void SetModifiedValueStatType<T>(string name, string guid)
        {
            var statTypes = (StatType[]) Enum.GetValues(typeof(StatType));
            for (var i = 0; i < statTypes.Count(); i++)
            {
                switch (i)
                {
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

                if (GL.Button(Strings.GetText("button_SetTo") + $" {statTypes[i]}", GL.Width(210f)))
                {
                    var file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid +
                                            ".json");
                    if (File.Exists(file.FullName))
                    {
                        var modifiedItem = DeserialiseItem<T>(file);
                        Traverse.Create(modifiedItem).Property(name).SetValue(statTypes[i]);
                        var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }
                    else
                    {
                        var modifiedItem = default(T);
                        modifiedItem = Activator.CreateInstance<T>();
                        Traverse.Create(modifiedItem).Property(name).SetValue(statTypes[i]);
                        var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }

                    blueprintLists = false;
                    Patch();
                }

                switch (i)
                {
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

        public static void SetModifiedValueButtonDiceFormula<T>(int rolls, DiceType dice, string name, string guid)
        {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {rolls} * {dice}", GL.ExpandWidth(false)))
            {
                var diceFormula = new ModifiedDiceFormula();
                diceFormula.m_Rolls = rolls;
                diceFormula.m_Dice = dice;
                var file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid +
                                        ".json");
                if (File.Exists(file.FullName))
                {
                    var modifiedItem = DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(diceFormula);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else
                {
                    var modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(diceFormula);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }

                blueprintLists = false;
                Patch();
            }
        }

        public static void SetModifiedValueDamageCriticalModifierType<T>(string name, string guid)
        {
            foreach (var critMod in (DamageCriticalModifierType[]) Enum.GetValues(typeof(DamageCriticalModifierType)))
                if (GL.Button(Strings.GetText("button_SetTo") + $" {critMod}", GL.ExpandWidth(false)))
                {
                    var file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid +
                                            ".json");
                    if (File.Exists(file.FullName))
                    {
                        var modifiedItem = DeserialiseItem<T>(file);
                        Traverse.Create(modifiedItem).Property(name).SetValue(critMod);
                        var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }
                    else
                    {
                        var modifiedItem = default(T);
                        modifiedItem = Activator.CreateInstance<T>();
                        Traverse.Create(modifiedItem).Property(name).SetValue(critMod);
                        var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }

                    blueprintLists = false;
                    Patch();
                }
        }

        public static void SetModifiedValueButtonAttackType<T>(string name, string guid)
        {
            foreach (var attackType in (AttackType[]) Enum.GetValues(typeof(AttackType)))
                if (GL.Button(Strings.GetText("button_SetTo") + $" {attackType}", GL.ExpandWidth(false)))
                {
                    var file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid +
                                            ".json");
                    if (File.Exists(file.FullName))
                    {
                        var modifiedItem = DeserialiseItem<T>(file);
                        Traverse.Create(modifiedItem).Property(name).SetValue(attackType);
                        var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }
                    else
                    {
                        var modifiedItem = default(T);
                        modifiedItem = Activator.CreateInstance<T>();
                        Traverse.Create(modifiedItem).Property(name).SetValue(attackType);
                        var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                        File.WriteAllText(file.FullName, json);
                    }

                    blueprintLists = false;
                    Patch();
                }
        }

        public static void SetModifiedValueButtonBool<T>(string name, string guid)
        {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {Strings.GetText("misc_True")}", GL.ExpandWidth(false)))
            {
                var file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid +
                                        ".json");
                if (File.Exists(file.FullName))
                {
                    var modifiedItem = DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(true);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else
                {
                    var modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(true);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }

                blueprintLists = false;
                Patch();
            }

            GL.Space(10);
            if (GL.Button(Strings.GetText("button_SetTo") + $" {Strings.GetText("misc_False")}", GL.ExpandWidth(false)))
            {
                var file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid +
                                        ".json");
                if (File.Exists(file.FullName))
                {
                    var modifiedItem = DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(false);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else
                {
                    var modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(false);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }

                blueprintLists = false;
                Patch();
            }
        }

        public static void SetModifiedValueButton<T>(Feet value, string name, string guid)
        {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {value}", GL.ExpandWidth(false)))
            {
                var file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid +
                                        ".json");
                if (File.Exists(file.FullName))
                {
                    var modifiedItem = DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else
                {
                    var modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }

                blueprintLists = false;
                Patch();
            }
        }

        public static void SetModifiedValueButton<T>(int value, string name, string guid)
        {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {value}", GL.ExpandWidth(false)))
            {
                var file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid +
                                        ".json");
                if (File.Exists(file.FullName))
                {
                    var modifiedItem = DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else
                {
                    var modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }

                blueprintLists = false;
                Patch();
            }
        }

        public static void SetModifiedValueButton<T>(float value, string name, string guid)
        {
            if (GL.Button(Strings.GetText("button_SetTo") + $" {value}", GL.ExpandWidth(false)))
            {
                var file = new FileInfo(Storage.modEntryPath + Storage.modifiedBlueprintsFolder + "\\" + guid +
                                        ".json");
                if (File.Exists(file.FullName))
                {
                    var modifiedItem = DeserialiseItem<T>(file);
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }
                else
                {
                    var modifiedItem = default(T);
                    modifiedItem = Activator.CreateInstance<T>();
                    Traverse.Create(modifiedItem).Property(name).SetValue(value);
                    var json = JsonConvert.SerializeObject(modifiedItem, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                }

                blueprintLists = false;
                Patch();
            }
        }

        public static T DeserialiseItem<T>(FileInfo file)
        {
            var modifiedItem = new object();
            using (var reader = file.OpenText())
            {
                var serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Include;
                modifiedItem = (T) serializer.Deserialize(reader, typeof(T));
            }

            return (T) modifiedItem;
        }

        public static void SetBlueprintField(object blueprint, object modifiedItem, string modifiedValue)
        {
            if (Traverse.Create(modifiedItem).Property(modifiedValue).GetValue() != null)
            {
                Traverse.Create(blueprint).Field(modifiedValue)
                    .SetValue(Traverse.Create(modifiedItem).Property(modifiedValue).GetValue());
                if (settings.settingShowDebugInfo)
                    modLogger.Log(
                        $"{blueprint} {modifiedValue} set to {Traverse.Create(modifiedItem).Property(modifiedValue).GetValue()}.");
            }
        }

        public static void PatchItem<T>(FileInfo file, BlueprintScriptableObject blueprintScriptableObject, string guid)
        {
            if (settings.settingShowDebugInfo)
                modLogger.Log($"{guid} -> {blueprintScriptableObject} ({blueprintScriptableObject.GetType()})");
            var jsonItem = DeserialiseItem<T>(file);
            var blueprint = Convert.ChangeType(blueprintScriptableObject, blueprintScriptableObject.GetType());
            foreach (var property in jsonItem.GetType().GetProperties())
                if (property.Name == "m_BaseDamage" &&
                    Traverse.Create(jsonItem).Property("m_BaseDamage").GetValue() != null)
                {
                    var dice = (ModifiedDiceFormula) property.GetValue(jsonItem);
                    var diceFormula = new DiceFormula((int) dice.m_Rolls, (DiceType) dice.m_Dice);
                    Traverse.Create(blueprint).Field("m_BaseDamage").SetValue(diceFormula);
                    if (settings.settingShowDebugInfo) modLogger.Log($"{blueprint} m_BaseDamage set to {diceFormula}.");
                }
                else if (property.Name == "m_DamageDice" &&
                         Traverse.Create(jsonItem).Property("m_DamageDice").GetValue() != null)
                {
                    var dice = (ModifiedDiceFormula) property.GetValue(jsonItem);
                    var diceFormula = new DiceFormula((int) dice.m_Rolls, (DiceType) dice.m_Dice);
                    Traverse.Create(blueprint).Field("m_DamageDice").SetValue(diceFormula);
                    if (settings.settingShowDebugInfo) modLogger.Log($"{blueprint} m_DamageDice set to {diceFormula}.");
                }
                else if (property.Name == "m_Enchantments" &&
                         Traverse.Create(jsonItem).Property("m_Enchantments").GetValue() != null)
                {
                    if (typeof(T) == typeof(EnchantableEquipment))
                    {
                        var enchantmentsGUIDS =
                            (string[]) Traverse.Create(jsonItem).Property("m_Enchantments").GetValue();
                        var enchantmentsBlueprints = new BlueprintEquipmentEnchantment[enchantmentsGUIDS.Length];
                        for (var i = 0; i < enchantmentsGUIDS.Length; i++)
                        {
                            var enchantmentGUID = CleanEnchantment(enchantmentsGUIDS[i]);
                            enchantmentsBlueprints[i] =
                                Utilities.GetBlueprintByGuid<BlueprintEquipmentEnchantment>(enchantmentGUID);
                        }

                        Traverse.Create(blueprint).Field("m_Enchantments").SetValue(enchantmentsBlueprints);
                        if (settings.settingShowDebugInfo)
                            modLogger.Log($"{blueprint} m_Enchantments set to {enchantmentsBlueprints}.");
                    }
                    else if (typeof(T) == typeof(ModifiedWeapon))
                    {
                        var enchantmentsGUIDS =
                            (string[]) Traverse.Create(jsonItem).Property("m_Enchantments").GetValue();
                        var enchantmentsBlueprints = new BlueprintWeaponEnchantment[enchantmentsGUIDS.Length];
                        for (var i = 0; i < enchantmentsGUIDS.Length; i++)
                        {
                            var enchantmentGUID = CleanEnchantment(enchantmentsGUIDS[i]);
                            enchantmentsBlueprints[i] =
                                Utilities.GetBlueprintByGuid<BlueprintWeaponEnchantment>(enchantmentGUID);
                        }

                        Traverse.Create(blueprint).Field("m_Enchantments").SetValue(enchantmentsBlueprints);
                        if (settings.settingShowDebugInfo)
                            modLogger.Log($"{blueprint} m_Enchantments set to {enchantmentsBlueprints}.");
                    }
                }
                else
                {
                    SetBlueprintField(blueprint, jsonItem, property.Name);
                }
        }

        public static void Patch()
        {
            if (Strings.ToBool(settings.toggleItemModding))
                try
                {
                    var path = Storage.modEntryPath + Storage.modifiedBlueprintsFolder;
                    var directory = new DirectoryInfo(path);

                    foreach (var file in directory.GetFiles("*.json"))
                    {
                        var example = false;
                        if (settings.settingShowDebugInfo) modLogger.Log($"{file.Name}");
                        var json = File.ReadAllText(file.FullName);
                        var guid = Path.GetFileNameWithoutExtension(file.Name);

                        if (guid.Contains("Example"))
                        {
                            if (settings.settingShowDebugInfo) modLogger.Log($"Ignoring {file.Name}");
                            example = true;
                        }

                        if (!example)
                        {
                            var blueprintScriptableObject =
                                Utilities.GetBlueprintByGuid<BlueprintScriptableObject>(guid);


                            if (blueprintScriptableObject != null)
                            {
                                if (blueprintItemCategory.Contains(blueprintScriptableObject.GetType()))
                                {
                                    if (blueprintScriptableObject.GetType() == blueprintItemWeapon)
                                        PatchItem<ModifiedWeapon>(file, blueprintScriptableObject, guid);
                                    else if (blueprintEnchantableEquipmentCategory.Contains(blueprintScriptableObject
                                        .GetType()))
                                        PatchItem<EnchantableEquipment>(file, blueprintScriptableObject, guid);
                                    else
                                        PatchItem<ModifiedItem>(file, blueprintScriptableObject, guid);
                                }
                                else if (blueprintTypeArmourCategory.Contains(blueprintScriptableObject.GetType()))
                                {
                                    PatchItem<ModifiedArmourType>(file, blueprintScriptableObject, guid);
                                }
                                else if (blueprintScriptableObject.GetType() == blueprintWeaponType)
                                {
                                    PatchItem<ModifiedWeaponType>(file, blueprintScriptableObject, guid);
                                }
                                else
                                {
                                    modLogger.Log(
                                        $"'{blueprintScriptableObject.GetType()}' {Strings.GetText("error_NotFound")}");
                                }
                            }
                            else
                            {
                                modLogger.Log($"'{guid}' {Strings.GetText("error_NotFound")}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    modLogger.Log(e.ToString());
                }
        }
    }
}