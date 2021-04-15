using BagOfTricks.Utils;
using BagOfTricks.Utils.Kingmaker;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks.ModUI.CheatsUI
{
    class PartyOptionsMenu
    {
        private static Settings settings = Main.settings;
        private static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;


        private static string[] unitEntityDataFiltersArray = { Strings.GetText("arrayItem_UnityEntityData_Party"), Strings.GetText("arrayItem_UnityEntityData_ControllableCharacters"), Strings.GetText("arrayItem_UnityEntityData_ActiveCompanions"), Strings.GetText("arrayItem_UnityEntityData_AllCharacters"), Strings.GetText("arrayItem_UnityEntityData_RemoteCharacters"), Strings.GetText("arrayItem_UnityEntityData_Mercenaries"), Strings.GetText("arrayItem_UnityEntityData_Pets") };

        public static void Render()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showPartyStatisticsCategory = GL.Toggle(settings.showPartyStatisticsCategory, RichTextUtils.MainCategoryFormat(Strings.GetText("mainCategory_PartyStats")), GL.ExpandWidth(false));
            if (!settings.showPartyStatisticsCategory)
            {
                GL.EndHorizontal();
            }
            else
            {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("PartyOptions");

                GL.Space(10);

                MenuTools.ToggleButton(ref settings.toggleAccessRemoteCharacters, "buttonToggle_AccessRemoteCharacters", "tooltip_AccessRemoteCharacters", nameof(settings.toggleAccessRemoteCharacters));

                MenuTools.ToggleButtonActions(ref settings.toggleShowAllPartyPortraits, GroupControllerUtils.NaviBlockShowAllPartyMembers, GroupControllerUtils.NaviBlockShowDefault, "buttonToggle_ShowAllPartyPortraits", "tooltip_ShowAllPartyPortraits", nameof(settings.toggleShowAllPartyPortraits));

                GL.Space(10);

                GL.BeginHorizontal();
                Storage.statsFilterUnitEntityDataIndex = GL.SelectionGrid(Storage.statsFilterUnitEntityDataIndex, unitEntityDataFiltersArray, 3);
                GL.EndHorizontal();
                Player player = Game.Instance.Player;
                switch (Storage.statsFilterUnitEntityDataIndex)
                {
                    case 0:
                        Storage.statsUnitEntityData = player.Party;
                        break;
                    case 1:
                        Storage.statsUnitEntityData = player.ControllableCharacters;
                        break;
                    case 2:
                        Storage.statsUnitEntityData = player.ActiveCompanions;
                        break;
                    case 3:
                        Storage.statsUnitEntityData = player.AllCharacters;
                        break;
                    case 4:
                        Storage.statsUnitEntityData = PartyUtils.GetRemoteCompanions();
                        break;
                    case 5:
                        Storage.statsUnitEntityData = PartyUtils.GetCustomCompanions();
                        break;
                    case 6:
                        Storage.statsUnitEntityData = PartyUtils.GetPets();
                        break;
                }
                if (Storage.statsFilterUnitEntityDataIndex != Storage.statsFilterUnitEntityDataIndexOld)
                {
                    Storage.reloadPartyStats = true;
                    Storage.statsFilterUnitEntityDataIndexOld = Storage.statsFilterUnitEntityDataIndex;
                }

                GL.Space(10);

                if (Storage.statsUnitEntityData.Any())
                {
                    if (Storage.reloadPartyStats)
                    {
                        Storage.statsSelectedControllableCharacterIndex = 0;
                        Storage.statsPartyMembers = Storage.statsUnitEntityData;
                        Storage.statsControllableCharacterNamesList.Clear();
                        foreach (UnitEntityData controllableCharacter in Storage.statsUnitEntityData)
                        {
                            Storage.statsControllableCharacterNamesList.Add(controllableCharacter.CharacterName);
                        }
                        Storage.reloadPartyStats = false;
                    }
                    if (!Storage.reloadPartyStats)
                    {
                        GL.BeginHorizontal();
                        Storage.statsSelectedControllableCharacterIndex = GL.SelectionGrid(Storage.statsSelectedControllableCharacterIndex, Storage.statsControllableCharacterNamesList.ToArray(), 6);
                        GL.EndHorizontal();

                        GL.Space(10);

                        GL.BeginVertical("box");
                        if (Storage.statsFilterUnitEntityDataIndex != 4)
                        {
                            MenuTools.SingleLineLabelGt("warning_SelectRemoteCharacters");
                        }
                        else
                        {
                            GL.BeginHorizontal();
                            if (GL.Button(MenuTools.TextWithTooltip("button_AddRemoteCompanionToParty", "tooltip_AddRemoteCompanionToParty", "", $" [{Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}]", false), GL.ExpandWidth(false))) {
                                UnitEntityDataUtils.AddCompanion(Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex]);
                            }
                            GL.Space(10);
                            if (GL.Button(MenuTools.TextWithTooltip("button_AddAllRemoteCompanionsToParty", "tooltip_AddAllRemoteCompanionsToParty", false), GL.ExpandWidth(false))) {
                                Storage.statsUnitEntityData.ForEach(UnitEntityDataUtils.AddCompanion);
                            }
                            GL.EndHorizontal();
                            if (!StringUtils.ToToggleBool(settings.toggleShowAllPartyPortraits) && Game.Instance.Player.Party.Count > 6) {
                                GL.Space(10);
                                MenuTools.SingleLineLabelGt("warning_PartyLimitShowAllPartyPortraits");
                            }
                        }
                        GL.EndVertical();

                        GL.Space(10);

                        MainMenu.CurrentHitPointsOptions();

                        GL.Space(10);

                        MainMenu.ChangeName();

                        GL.Space(10);

                        MainMenu.ChangeGender();

                        GL.Space(10);


                        MainMenu.ClassData();

                        GL.Space(10);

                        //Menu.RaceData();

                        //GL.Space(10);

                        /*
                        if (GL.Button(MenuTools.TextWithTooltip("button_RemoveEquippedItems", "tooltip_RemoveEquippedItems", false), GL.ExpandWidth(false)))
                        {
                            foreach (ItemEntity itemEntity in Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Inventory.Items)
                            {
                                if (itemEntity.Owner == Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor)
                                {
                                    itemEntity.HoldingSlot.RemoveItem();
                                }
                            }
                        }
                        GL.Space(10);*/

                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        if (GL.Button(MenuTools.TextWithTooltip("button_ResetCharacterLevel", "tooltip_ResetCharacterLevel", false), GL.ExpandWidth(false)))
                        {
                            int level = 21;
                            int xp = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Progression.Experience;
                            BlueprintStatProgression xpTable = BlueprintRoot.Instance.Progression.XPTable;

                            for (int i = 20; i >= 1; i--)
                            {
                                int xpBonus = xpTable.GetBonus(i);

                                modLogger.Log(i + ": " + xpBonus + " | " + xp);

                                if ((xp - xpBonus) >= 0)
                                {
                                    modLogger.Log(i + ": " + (xp - xpBonus));
                                    level = i;
                                    break;
                                }
                            }

                            Type type = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Progression.GetType();
                            PropertyInfo propertyInfo = type.GetProperty("CharacterLevel");
                            propertyInfo.SetValue(Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Progression, level, null);
                        }
                        GL.EndHorizontal();

                        GL.Space(10);

                        GL.BeginHorizontal();
                        Storage.setCharLevel = GL.HorizontalSlider(Storage.setCharLevel, 1f, 20f, GL.Width(250f));
                        GL.Label($" {Mathf.RoundToInt(Storage.setCharLevel)}", GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        GL.BeginHorizontal();
                        if (GL.Button(MenuTools.TextWithTooltip("button_SetCharacterLevel", "tooltip_SetCharacterLevel", "", $" {Mathf.RoundToInt(Storage.setCharLevel)}" + " " + StringUtils.PutInParenthesis(Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName)), GL.ExpandWidth(false)))
                        {
                            Type type = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Progression.GetType();
                            PropertyInfo propertyInfoLvl = type.GetProperty("CharacterLevel");
                            propertyInfoLvl.SetValue(Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Progression, Mathf.RoundToInt(Storage.setCharLevel), null);

                            int newXp = BlueprintRoot.Instance.Progression.XPTable.GetBonus(Mathf.RoundToInt(Storage.setCharLevel));
                            PropertyInfo propertyInfoXp = type.GetProperty("Experience");
                            propertyInfoXp.SetValue(Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Progression, newXp, null);
                        }
                        GL.EndHorizontal();
                        MenuTools.SingleLineLabel(Strings.GetText("warning_SetCharacterLevel"));
                        GL.EndVertical();

                        GL.Space(10);

                        MenuTools.UnitAlignment(Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex]);

                        GL.Space(10);

                        GL.BeginVertical("box");

                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_Size")));
                        GL.BeginHorizontal();
                        Storage.partySelectedSizeIndex = GL.SelectionGrid(Storage.partySelectedSizeIndex, Storage.charSizeArray, 4);
                        GL.EndHorizontal();
                        GL.Space(10);
                        GL.BeginHorizontal();
                        if (GL.Button(MenuTools.TextWithTooltip("button_SetSizeTo", "tooltip_SetSize", "", $" {Storage.charSizeArray[Storage.partySelectedSizeIndex]}"), GL.ExpandWidth(false)))
                        {
                            Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.State.Size = (Size)Storage.partySelectedSizeIndex;
                        }
                        GL.EndHorizontal();
                        GL.BeginHorizontal();
                        if (GL.Button(MenuTools.TextWithTooltip("button_SetToOriginalSize", "tooltip_SetToOriginalSize", "", $" ({Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.OriginalSize})"), GL.ExpandWidth(false)))
                        {
                            Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.State.Size = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.OriginalSize;
                        }
                        GL.EndHorizontal();
                        MenuTools.SingleLineLabel(Strings.GetText("label_CurrentSize") + ": " + Common.SizeToString(Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.State.Size));
                        GL.EndVertical();

                        GL.Space(10);

                        GL.BeginHorizontal();
                        GL.Label(MenuTools.TextWithTooltip("header_Statistics", "tooltip_Statistics", true));
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        settings.partyStatsAmount = GL.TextField(settings.partyStatsAmount, 10, GL.Width(85f));
                        MenuTools.SettingParse(ref settings.partyStatsAmount, ref settings.partyFinalStatsAmount);
                        GL.EndHorizontal();

                        CharacterStats charStats = Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].Descriptor.Stats;
                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_AttributesBaseValues")));
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsAttributesDict)
                        {
                            MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, settings.partyFinalStatsAmount);
                        }
                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_SkillsRanks")));

                        MenuTools.ToggleButton(ref settings.toggleShowOnlyClassSkills, "buttonToggle_ShowOnlyClassSkills", "tooltip_ShowOnlyClassSkills");

                        foreach (KeyValuePair<string, StatType> entry in Storage.statsSkillsDict)
                        {
                            if (StringUtils.ToToggleBool(settings.toggleShowOnlyClassSkills))
                            {
                                ModifiableValueSkill stat = charStats.GetStat(entry.Value) as ModifiableValueSkill;
                                if (stat.ClassSkill && stat.BaseValue > 0)
                                {
                                    MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, settings.partyFinalStatsAmount);
                                }
                            }
                            else
                            {
                                MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, settings.partyFinalStatsAmount, true);
                            }
                        }

                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_SocialSkillsBaseValues")));
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsSocialSkillsDict)
                        {
                            MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, settings.partyFinalStatsAmount);
                        }
                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_StatsSaves")));
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsSavesDict)
                        {
                            MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, settings.partyFinalStatsAmount);
                        }
                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_StatsCombat")));
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsCombatDict)
                        {
                            MenuTools.CreateStatInterface(entry.Key, charStats, entry.Value, settings.partyFinalStatsAmount);
                        }
                        GL.Space(10);

                        GL.BeginHorizontal();
                        GL.Label(MenuTools.TextWithTooltip("header_PartyMultipliers", "tooltip_PartyMultipliers", true));
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        settings.partyStatMultiplier = GL.HorizontalSlider(settings.partyStatMultiplier, 0.1f, 10f, GL.Width(300f));
                        GL.Label($" {Math.Round(settings.partyStatMultiplier, 1)}", GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_Attributes")));
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsAttributesDict)
                        {
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.statsPartyMembers, settings.partyStatMultiplier);
                        }
                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_Skills")));
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsSkillsDict)
                        {
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.statsPartyMembers, settings.partyStatMultiplier);
                        }
                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_SocialSkills")));
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsSocialSkillsDict)
                        {
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.statsPartyMembers, settings.partyStatMultiplier);
                        }
                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_Saves")));
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsSavesDict)
                        {
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.statsPartyMembers, settings.partyStatMultiplier);
                        }
                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_Combat")));
                        foreach (KeyValuePair<string, StatType> entry in Storage.statsCombatDict)
                        {
                            MenuTools.CreateStatMultiplierInterface(entry.Key, charStats, entry.Value, Storage.statsPartyMembers, settings.partyStatMultiplier);
                        }

                        GL.BeginHorizontal();

                        if (GL.Button(MenuTools.TextWithTooltip("button_ExportCharInfo", "tooltip_ExportCharInfo", false), GL.ExpandWidth(false)))
                        {
                            List<string> charInfoTxt = new List<string>();
                            charInfoTxt.Add($"{Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}");
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_AttributesBaseValues"));
                            foreach (KeyValuePair<string, StatType> entry in Storage.statsAttributesDict)
                            {
                                charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                            }
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_SkillsRanks"));
                            foreach (KeyValuePair<string, StatType> entry in Storage.statsSkillsDict)
                            {
                                charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                            }
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_SocialSkillsBaseValues"));
                            foreach (KeyValuePair<string, StatType> entry in Storage.statsSocialSkillsDict)
                            {
                                charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                            }
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_StatsSaves"));
                            foreach (KeyValuePair<string, StatType> entry in Storage.statsSavesDict)
                            {
                                charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                            }
                            charInfoTxt.Add("");
                            charInfoTxt.Add(Strings.GetText("header_StatsCombat"));
                            foreach (KeyValuePair<string, StatType> entry in Storage.statsCombatDict)
                            {
                                charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                            }

                            File.WriteAllLines(Path.Combine(Common.ExportPath(), $"{Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}.txt"), charInfoTxt.ToArray());
                        }
                        GL.Label(" " + Strings.GetText("label_Location") + $": {Path.Combine(Common.ExportPath(), $"{Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}.txt")}");
                        GL.EndHorizontal();

                        if (File.Exists(Storage.modEntryPath + Storage.charactersImportFolder + "\\" + Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName + ".txt"))
                        {

                            if (GL.Button(MenuTools.TextWithTooltip("button_ImportStatsFrom", "tooltip_ImportStatsFrom", "", $" { Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}.txt"), GL.ExpandWidth(false)))
                            {
                                if (settings.settingCreateBackupBeforeImport)
                                {
                                    List<string> charInfoTxt = new List<string>();
                                    charInfoTxt.Add($"{Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName}");
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_AttributesBaseValues"));
                                    foreach (KeyValuePair<string, StatType> entry in Storage.statsAttributesDict)
                                    {
                                        charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                                    }
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_SkillsRanks"));
                                    foreach (KeyValuePair<string, StatType> entry in Storage.statsSkillsDict)
                                    {
                                        charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                                    }
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_SocialSkillsBaseValues"));
                                    foreach (KeyValuePair<string, StatType> entry in Storage.statsSocialSkillsDict)
                                    {
                                        charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                                    }
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_StatsSaves"));
                                    foreach (KeyValuePair<string, StatType> entry in Storage.statsSavesDict)
                                    {
                                        charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                                    }
                                    charInfoTxt.Add("");
                                    charInfoTxt.Add(Strings.GetText("header_StatsCombat"));
                                    foreach (KeyValuePair<string, StatType> entry in Storage.statsCombatDict)
                                    {
                                        charInfoTxt.Add(($"{entry.Key}: {charStats.GetStat(entry.Value).BaseValue} ({charStats.GetStat(entry.Value).ModifiedValue})"));
                                    }
                                    File.WriteAllLines(Path.Combine(Storage.modEntryPath + Storage.charactersImportFolder + "\\" + Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName + "_Backup.txt"), charInfoTxt.ToArray());
                                }

                                string[] lines = File.ReadAllLines(Storage.modEntryPath + Storage.charactersImportFolder + "\\" + Storage.statsPartyMembers[Storage.statsSelectedControllableCharacterIndex].CharacterName + ".txt");
                                lines = lines.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                lines = lines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    if (Regex.IsMatch(lines[i], @"[\x20A-Za-z()]+:\s*[0-9]+"))
                                    {
                                        Match match = Regex.Match(lines[i], @"[\x20A-Za-z()]+:\s*[0-9]+");
                                        lines[i] = match.Value;
                                        string[] splitLine = lines[i].Split(':');
                                        Dictionary<string, StatType> allStats = Storage.statsAttributesDict.Union(Storage.statsSkillsDict).Union(Storage.statsSocialSkillsDict).Union(Storage.statsSavesDict).Union(Storage.statsCombatDict).ToDictionary(k => k.Key, v => v.Value);
                                        if (allStats.TryGetValue(splitLine[0], out StatType statType) && int.TryParse(splitLine[1], out int baseValue))
                                        {
                                            charStats.GetStat(statType).BaseValue = baseValue;
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitFound"));
                }
            }
            GL.EndVertical();
        }
    }
}
