using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Alignments;

using UnityEngine;

using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;
namespace BagOfTricks
{
    public static class MenuTools
    {
        public static Settings settings = Main.Settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.ModLogger;


        public static void ToggleButtonDependant(ref string toggle, ref string dependancy, string buttonText, string buttonTooltip)
        {
            GL.BeginHorizontal();
            if (GL.Button(TextWithTooltip(buttonText, buttonTooltip, $"{toggle}" + " ", ""), GL.ExpandWidth(false)))
            {
                if (toggle == Storage.isFalseString)
                {
                    if (!Strings.ToBool(dependancy))
                    {
                        dependancy = Storage.isTrueString;
                    }
                    toggle = Storage.isTrueString;

                }
                else if (toggle == Storage.isTrueString)
                {
                    toggle = Storage.isFalseString;
                }
            }
            GL.EndHorizontal();
        }
        public static void ToggleButtonActionAtOn(ref string toggle, Action action, string buttonText, string buttonTooltip)
        {
            GL.BeginHorizontal();
            if (GL.Button(TextWithTooltip(buttonText, buttonTooltip, $"{toggle}" + " ", ""), GL.ExpandWidth(false)))
            {
                if (toggle == Storage.isFalseString)
                {
                    action();
                    toggle = Storage.isTrueString;

                }
                else if (toggle == Storage.isTrueString)
                {
                    toggle = Storage.isFalseString;
                }
            }
            GL.EndHorizontal();
        }
        public static void ToggleButtonActionAtOff(ref string toggle, Action action, string buttonText, string buttonTooltip)
        {
            GL.BeginHorizontal();
            if (GL.Button(TextWithTooltip(buttonText, buttonTooltip, $"{toggle}" + " ", ""), GL.ExpandWidth(false)))
            {
                if (toggle == Storage.isFalseString)
                {
                    toggle = Storage.isTrueString;

                }
                else if (toggle == Storage.isTrueString)
                {
                    action();
                    toggle = Storage.isFalseString;
                }
            }
            GL.EndHorizontal();
        }

        public static void ToggleButtonFavouritesMenu(ref string toggle, string buttonText, string buttonTooltip, bool boldText = false)
        {
            GL.BeginHorizontal();
            if (GL.Button(TextWithTooltip(buttonText, buttonTooltip, $"{toggle}" + " ", "", boldText), GL.ExpandWidth(false)))
            {
                if (toggle == Storage.isFalseString)
                {
                    toggle = Storage.isTrueString;

                }
                else if (toggle == Storage.isTrueString)
                {
                    toggle = Storage.isFalseString;
                }
            }
        }
        public static void ToggleButton(ref string toggle, string buttonText, string buttonTooltip, bool boldText = false)
        {
            GL.BeginHorizontal();
            if (GL.Button(TextWithTooltip(buttonText, buttonTooltip, $"{toggle}" + " ", "", boldText), GL.ExpandWidth(false)))
            {
                if (toggle == Storage.isFalseString)
                {
                    toggle = Storage.isTrueString;

                }
                else if (toggle == Storage.isTrueString)
                {
                    toggle = Storage.isFalseString;
                }
            }
            GL.EndHorizontal();
        }
        public static void ToggleButton(ref string toggle, string buttonText, string buttonTooltip, string toggleName, bool boldText = false)
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            if (GL.Button(TextWithTooltip(buttonText, buttonTooltip, $"{toggle}" + " ", "", boldText), GL.ExpandWidth(false)))
            {
                if (toggle == Storage.isFalseString)
                {
                    toggle = Storage.isTrueString;

                }
                else if (toggle == Storage.isTrueString)
                {
                    toggle = Storage.isFalseString;
                }
            }
            GL.FlexibleSpace();
            if (Storage.togglesFavourites.Contains(toggleName + "," + buttonText + "," + buttonTooltip))
            {
                if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                {
                    Storage.togglesFavourites.Remove(toggleName + "," + buttonText + "," + buttonTooltip);
                    Storage.togglesFavouritesLoad = true;
                }
            }
            else
            {
                if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                {
                    Storage.togglesFavourites.Add(toggleName + "," + buttonText + "," + buttonTooltip);
                    Storage.togglesFavouritesLoad = true;
                }
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static GUIContent TextWithTooltip(string textMain, string tooltip, string textFront = "", string textBack = "", bool boldText = false)
        {

            if (boldText)
            {
                return new GUIContent(textFront + RichText.Bold(Strings.GetText(textMain)) + textBack, Strings.GetText(tooltip));
            }
            else
            {
                return new GUIContent(textFront + Strings.GetText(textMain) + textBack, Strings.GetText(tooltip));
            }
        }

        public static GUIContent TextWithTooltip(string textMain, string tooltip, bool boldText = false)
        {

            if (boldText)
            {
                return new GUIContent(RichText.Bold(Strings.GetText(textMain)), Strings.GetText(tooltip));
            }
            else
            {
                return new GUIContent(Strings.GetText(textMain), Strings.GetText(tooltip));
            }
        }

        public static void SingleLineLabel(string labelString)
        {
            GL.BeginHorizontal();
            GL.Label(labelString, Main.WindowWidth);
            GL.EndHorizontal();
        }

        public static void SingleLineLabel(GUIContent guiContent, bool expand = false)
        {
            GL.BeginHorizontal();
            GL.Label(guiContent, GL.ExpandWidth(expand));
            GL.EndHorizontal();
        }


        public static void SingleLineLabelGT(string label, string tooltip, bool bold = false)
        {
            GUIContent content;
            if (bold)
            {
                content = new GUIContent(RichText.Bold(Strings.GetText(label)), Strings.GetText(tooltip));
            }
            else
            {
                content = new GUIContent(Strings.GetText(label), Strings.GetText(tooltip));
            }
            GL.BeginHorizontal();
            GL.Label(content);
            GL.EndHorizontal();
        }

        public static void SingleLineLabelGT(string label, bool bold = false)
        {
            string content = Strings.GetText(label);
            if (bold)
            {
                content = RichText.Bold(content);
            }

            GL.BeginHorizontal();
            GL.Label(content);
            GL.EndHorizontal();
        }


        public static void ToggleToToggleButton(ref string toggle)
        {
            if (toggle == "false")
            {
                toggle = Storage.isFalseString;
            }
            else if (toggle == "true")
            {
                toggle = Storage.isTrueString;
            }
            else if (toggle != Storage.isFalseString && toggle != Storage.isTrueString)
            {
                toggle = Storage.isFalseString;
            }
        }


        public static void Tooltips()
        {
            /*
            if (GUI.tooltip.Length >= 30)
            {
                string[] words = GUI.tooltip.Split(null);
                int length = 0;
                string newTooltip = "";
                for (int i = 0; i < words.Length; i++)
                {
                    length += words[i].Length;
                    if (length > 29)
                    {
                        newTooltip += "\n";
                        length = 0;
                    }
                    newTooltip += words[i] + " ";

                }
                GUI.tooltip = newTooltip;
            }*/
            GUIContent tooltipString = new GUIContent(GUI.tooltip);
            GUIStyle styleRect = GUI.skin.box;
            Vector2 tooltipSize = styleRect.CalcSize(tooltipString);
            GUIStyle styleTooltip = new GUIStyle();
            Texture2D background = new Texture2D(1, 1);
            float textHeight = styleRect.CalcHeight(tooltipString, tooltipSize.x);
            if (GUI.tooltip != "" && GUI.tooltip != null)
            {
                background.SetPixels32(new Color32[] { new Color32(0, 0, 0, 220) });
                GUIStyleState stylestate = new GUIStyleState();
                stylestate.background = background;
                styleTooltip.normal = stylestate;
            }
            else
            {
                background.SetPixels32(new Color32[] { new Color32(0, 0, 0, 0) });
            }
            float rectX = Input.mousePosition.x - Storage.ummRect.min.x + Storage.ummScrollPosition[Storage.ummTabId].x - 8;
            if (rectX > 470)
            {
                rectX = rectX - tooltipSize.x + 8;
            }
            float rectY = Screen.height - Storage.ummRect.min.y + Storage.ummScrollPosition[Storage.ummTabId].y - Input.mousePosition.y - 65 - textHeight;

            GUI.Label(new Rect(rectX, rectY, tooltipSize.x, tooltipSize.y), GUI.tooltip, styleTooltip);
        }

        public static ref string GetToggleButton(string toggleButton)
        {
            try
            {
                switch (toggleButton)
                {
                    case "toggleNoLevelUpRestirctions":
                        return ref settings.toggleNoLevelUpRestirctions;
                    case "toggleInfiniteSkillpoints":
                        return ref settings.toggleInfiniteSkillpoints;
                    case "toggleIgnoreClassAlignment":
                        return ref settings.toggleIgnoreClassAlignment;
                    case "toggleIgnoreForbiddenFeatures":
                        return ref settings.toggleIgnoreForbiddenFeatures;
                    case "toggleNoRationsRequired":
                        return ref settings.toggleNoRationsRequired;
                    case "toggleVendorsSellFor0":
                        return ref settings.toggleVendorsSellFor0;
                    case "toggleVendorsBuyFor0":
                        return ref settings.toggleVendorsBuyFor0;
                    case "toggleInfiniteItems":
                        return ref settings.toggleInfiniteItems;
                    case "toggleEquipmentRestrictions":
                        return ref settings.toggleEquipmentRestrictions;
                    case "toggleArcaneSpellFailureRoll":
                        return ref settings.toggleArcaneSpellFailureRoll;
                    case "toggleUnlimitedCasting":
                        return ref settings.toggleUnlimitedCasting;
                    case "toggleMetamagic":
                        return ref settings.toggleMetamagic;
                    case "toggleMaterialComponent":
                        return ref settings.toggleMaterialComponent;
                    case "toggleSpontaneousCopyScrolls":
                        return ref settings.toggleSpontaneousCopyScrolls;
                    case "toggleInfiniteAbilities":
                        return ref settings.toggleInfiniteAbilities;
                    case "toggleRestoreSpellsAbilitiesAfterCombat":
                        return ref settings.toggleRestoreSpellsAbilitiesAfterCombat;
                    case "toggleUndetectableStealth":
                        return ref settings.toggleUndetectableStealth;
                    case "toggleFfAOE":
                        return ref settings.toggleFfAOE;
                    case "toggleFfAny":
                        return ref settings.toggleFfAny;
                    case "toggleInstantCooldown":
                        return ref settings.toggleInstantCooldown;
                    case "toggleMoveSpeedAsOne":
                        return ref settings.toggleMoveSpeedAsOne;
                    case "toggleEnableTeleport":
                        return ref settings.toggleEnableTeleport;
                    case "toggleEnableAvoidanceSuccess":
                        return ref settings.toggleEnableAvoidanceSuccess;
                    case "toggleInstantEvent":
                        return ref settings.toggleInstantEvent;
                    case "toggleKingdomEventResultSuccess":
                        return ref settings.toggleKingdomEventResultSuccess;
                    case "toggleSettlementRestrictions":
                        return ref settings.toggleSettlementRestrictions;
                    case "toggleEnemiesAlwaysRoll1":
                        return ref settings.toggleEnemiesAlwaysRoll1;
                    case "toggleEveryoneExceptPlayerFactionRolls1":
                        return ref settings.toggleEveryoneExceptPlayerFactionRolls1;
                    case "toggleDialogRestrictions":
                        return ref settings.toggleDialogRestrictions;
                    case "toggleNoInactiveCamp":
                        return ref settings.toggleNoInactiveCamp;
                    case "toggleEnableTaxCollector":
                        return ref settings.toggleEnableTaxCollector;
                    case "toggleShowTooltips":
                        return ref settings.toggleShowTooltips;
                    case "toggleAddToLog":
                        return ref settings.toggleAddToLog;
                    case "toggleInstantRestAfterCombat":
                        return ref settings.toggleInstantRestAfterCombat;
                    case "toggleFullHitdiceEachLevel":
                        return ref settings.toggleFullHitdiceEachLevel;
                    case "toggleRollHitDiceEachLevel":
                        return ref settings.toggleRollHitDiceEachLevel;
                    case "toggleIgnoreFeaturePrerequisites":
                        return ref settings.toggleIgnoreFeaturePrerequisites;
                    case "toggleIgnoreFeatureListPrerequisites":
                        return ref settings.toggleIgnoreFeatureListPrerequisites;
                    case "toggleFeaturesIgnorePrerequisites":
                        return ref settings.toggleFeaturesIgnorePrerequisites;
                    case "toggleNoDamageFromEnemies":
                        return ref settings.toggleNoDamageFromEnemies;
                    case "togglePartyOneHitKills":
                        return ref settings.togglePartyOneHitKills;
                    case "toggleAllDoorContainersUnlocked":
                        return ref settings.toggleAllDoorContainersUnlocked;
                    case "toggleNoIngredientsRequired":
                        return ref settings.toggleNoIngredientsRequired;
                    case "toggleUnityModManagerButton":
                        return ref settings.toggleUnityModManagerButton;
                    case "toggleDamageDealtMultipliers":
                        return ref settings.toggleDamageDealtMultipliers;
                    case "togglePartyDamageDealtMultiplier":
                        return ref settings.togglePartyDamageDealtMultiplier;
                    case "toggleEnemiesDamageDealtMultiplier":
                        return ref settings.toggleEnemiesDamageDealtMultiplier;
                    case "toggleScaleInventoryMoney":
                        return ref settings.toggleScaleInventoryMoney;
                    case "toggleNoBurnKineticist":
                        return ref settings.toggleNoBurnKineticist;
                    case "toggleMaximiseAcceptedBurn":
                        return ref settings.toggleMaximiseAcceptedBurn;
                    case "toggleMainCharacterRoll20":
                        return ref settings.toggleMainCharacterRoll20;
                    case "toggleShowPetInventory":
                        return ref settings.toggleShowPetInventory;
                    case "toggleAnimationCloseUMM":
                        return ref settings.toggleAnimationCloseUMM;
                    case "toggleInstantCooldownMainChar":
                        return ref settings.toggleInstantCooldownMainChar;
                    case "toggleDexBonusLimit99":
                        return ref settings.toggleDexBonusLimit99;
                    case "toggleItemsWeighZero":
                        return ref settings.toggleItemsWeighZero;
                    case "toggleAlignmentFix":
                        return ref settings.toggleAlignmentFix;
                    case "toggleSpellbookAbilityAlignmentChecks":
                        return ref settings.toggleSpellbookAbilityAlignmentChecks;
                    case "toggleFreezeTimedQuestAt90Days":
                        return ref settings.toggleFreezeTimedQuestAt90Days;
                    case "togglePreventQuestFailure":
                        return ref settings.togglePreventQuestFailure;
                    case "toggleAlwaysSucceedCastingDefensively":
                        return ref settings.toggleAlwaysSucceedCastingDefensively;
                    case "toggleAlwaysSucceedConcentration":
                        return ref settings.toggleAlwaysSucceedConcentration;
                    case "toggleSortSpellbooksAlphabetically":
                        return ref settings.toggleSortSpellbooksAlphabetically;
                    case "toggleSortSpellsAlphabetically":
                        return ref settings.toggleSortSpellsAlphabetically;
                    case "toggleAllHitsAreCritical":
                        return ref settings.toggleAllHitsAreCritical;
                    case "toggleCookingAndHuntingInDungeons":
                        return ref settings.toggleCookingAndHuntingInDungeons;
                    case "toggleArmourChecksPenalty0":
                        return ref settings.toggleArmourChecksPenalty0;
                    case "toggleInstantPartyChange":
                        return ref settings.toggleInstantPartyChange;
                    case "toggleSmartConsole":
                        return ref settings.toggleDevTools;
                    case "toggleBoTScrollBar":
                        return ref settings.toggleBoTScrollBar;
                    case "toggleNoNegativeLevels":
                        return ref settings.toggleNoNegativeLevels;
                    case "togglAutoEquipConsumables":
                        return ref settings.togglAutoEquipConsumables;
                    case "toggleIgnorePrerequisites":
                        return ref settings.toggleIgnorePrerequisites;
                    case "toggleIgnoreCasterTypeSpellLevel":
                        return ref settings.toggleIgnoreCasterTypeSpellLevel;
                    case "toggleIgnoreForbiddenArchetype":
                        return ref settings.toggleIgnoreForbiddenArchetype;
                    case "toggleIgnorePrerequisiteStatValue":
                        return ref settings.toggleIgnorePrerequisiteStatValue;
                    case "toggleSpiderBegone":
                        return ref settings.toggleSpiderBegone;
                    case "toggleNoTempHPKineticist":
                        return ref settings.toggleNoTempHPKineticist;
                    case "toggleShowAreaName":
                        return ref settings.toggleShowAreaName;
                    case "toggleRestoreItemChargesAfterCombat":
                        return ref settings.toggleRestoreItemChargesAfterCombat;
                    case "toggleReverseCasterAlignmentChecks":
                        return ref settings.toggleReverseCasterAlignmentChecks;
                    case "toggleDisplayObjectInfo":
                        return ref settings.toggleDisplayObjectInfo;
                    case "togglePreventAlignmentChanges":
                        return ref settings.togglePreventAlignmentChanges;
                    case "toggleExportToModFolder":
                        return ref settings.toggleExportToModFolder;
                    default:
                        throw new ArgumentException($"GetToggleButton received an invalid toggle name ({toggleButton})!");
                }
            }
            catch (Exception e)
            {
                modLogger.Log(e.ToString());
                throw;
            }
        }


        public static void AddFavouriteButton(string methodName)
        {
            if (Storage.togglesFavourites.Contains(methodName))
            {
                if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                {
                    Storage.togglesFavourites.Remove(methodName);
                    Storage.togglesFavouritesLoad = true;
                }
            }
            else
            {
                if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                {
                    Storage.togglesFavourites.Add(methodName);
                    Storage.togglesFavouritesLoad = true;
                }
            }
        }
        public static void AddFavouriteButton(string methodName, int size)
        {
            if (Storage.togglesFavourites.Contains(methodName))
            {
                if (GL.Button(RichText.Size(Storage.favouriteTrueString, size), GL.ExpandWidth(false)))
                {
                    Storage.togglesFavourites.Remove(methodName);
                    Storage.togglesFavouritesLoad = true;
                }
            }
            else
            {
                if (GL.Button(RichText.Size(Storage.favouriteFalseString, size), GL.ExpandWidth(false)))
                {
                    Storage.togglesFavourites.Add(methodName);
                    Storage.togglesFavouritesLoad = true;
                }
            }
        }

        public static void AddUpDownButtons(string methodName, ref string[] methods, int size)
        {
            if (Array.IndexOf(methods, methodName) > 0)
            {
                if (GL.Button(new GUIContent(RichText.Size(Storage.upArrow, size), Strings.GetText("tooltip_ArrowUp")), GL.ExpandWidth(false)))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                    {
                        Common.MakeArrayElementFirst(ref methods, methodName);
                    }
                    else
                    {
                        Common.MoveArrayElementDown(ref methods, methodName);
                    }
                }
            }
            if (Array.IndexOf(methods, methodName) < methods.Length - 1)
            {
                if (GL.Button(new GUIContent(RichText.Size(Storage.downArrow, size), Strings.GetText("tooltip_ArrowDown")), GL.ExpandWidth(false)))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                    {
                        Common.MakeArrayElementLast(ref methods, methodName);
                    }
                    else
                    {
                        Common.MoveArrayElementUp(ref methods, methodName);
                    }
                }
            }
        }
        public static void AddUpDownButtons(string methodName, ref List<string> methods, int size)
        {
            if (methods.IndexOf(methodName) > 0)
            {
                if (GL.Button(new GUIContent(RichText.Size(Storage.upArrow, size), Strings.GetText("tooltip_ArrowUp")), GL.ExpandWidth(false)))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                    {
                        Common.MakeListElementFirst(ref methods, methodName);
                    }
                    else
                    {
                        Common.MoveListElementDown(ref methods, methodName);
                    }
                }
            }
            if (methods.IndexOf(methodName) < methods.Count - 1)
            {
                if (GL.Button(new GUIContent(RichText.Size(Storage.downArrow, size), Strings.GetText("tooltip_ArrowDown")), GL.ExpandWidth(false)))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                    {
                        Common.MakeListElementLast(ref methods, methodName);
                    }
                    else
                    {
                        Common.MoveListElementUp(ref methods, methodName);
                    }
                }
            }
        }

        public static void CategoryMenuElements(string methodName, ref string[] methods)
        {
            if (settings.mainToolbarIndex == 1)
            {
                AddUpDownButtons(methodName, ref methods, 20);
                GL.Space(20);
            }
            AddFavouriteButton(methodName, 20);
        }

        public static void SettingsValueFloat(ref string setValue, ref float finalSetValue)
        {
            GL.BeginHorizontal();
            GL.Label(Strings.GetText("headerOption_SettingsValue") + ": ", GL.ExpandWidth(false));
            setValue = GL.TextField(setValue, 6, GL.Width(50f));
            setValue = MenuTools.FloatTestSettingStage1(setValue);
            finalSetValue = MenuTools.FloatTestSettingStage2(setValue, finalSetValue);
            GL.EndHorizontal();
        }

        public static void SettingParse(ref string stringSetting, ref int finalSetting)
        {
            if (int.TryParse(stringSetting, out int result))
            {
                finalSetting = result;
            }
            else
            {
                stringSetting = "";
            }
        }

        public static void SettingParse(ref string stringSetting, ref float finalSetting)
        {
            if (float.TryParse(stringSetting, out float result))
            {
                finalSetting = result;
            }
            else
            {
                stringSetting = "";
            }
        }

        public static string IntTestSettingStage1(string setting)
        {
            if (int.TryParse(setting, out int iTest) && Regex.IsMatch(setting, @"^[0]*$") == false)
            {
                return setting;
            }
            else
            {
                return "";
            }
        }

        static string IntTestSettingStageNeg1(string setting)
        {
            if (int.TryParse(setting, out int iTest) && Regex.IsMatch(setting, @"^[0]*$") == false)
            {
                return setting;
            }
            else
            {
                return "";
            }
        }

        public static int IntTestSettingStage2(string setting, int finalSetting)
        {
            if (setting != "")
            {
                return finalSetting = int.Parse(setting);
            }
            else
            {
                return finalSetting;
            }
        }

        public static string FloatTestSettingStage1(string setting)
        {
            if (float.TryParse(setting, out float fTest) && !setting.Contains(","))
            {
                return setting;
            }
            else
            {
                return "";
            }
        }

        public static float FloatTestSettingStage2(string setting, float finalSetting)
        {
            if (setting != "" && float.Parse(setting) != 0f)
            {
                return finalSetting = float.Parse(setting);
            }
            else
            {
                return finalSetting;
            }
        }

        public static void CreateStatInterface(string stat, CharacterStats charStats, StatType statType, int amount, bool isSkill = false)
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label($"{stat}: {charStats.GetStat(statType).BaseValue} ({charStats.GetStat(statType).ModifiedValue})");
            if (GL.Button("<b>+</b>"))
            {
                charStats.GetStat(statType).BaseValue++;
            }
            if (GL.Button("<b>-</b>"))
            {
                charStats.GetStat(statType).BaseValue--;
            }
            if (GL.Button(Strings.GetText("button_SetTo") + $" {amount}", GL.ExpandWidth(false)))
            {
                charStats.GetStat(statType).BaseValue = amount;
            }

            if (isSkill)
            {
                ModifiableValueSkill modSkill = charStats.GetStat(statType) as ModifiableValueSkill;
                if (modSkill.ClassSkill && modSkill.BaseValue > 0)
                {
                }
                else
                {
                    if (GL.Button(Strings.GetText("button_MakeClassSkills"), GL.ExpandWidth(false)))
                    {
                        charStats.AddClassSkill(statType);
                    }
                }
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static void CreateStatMultiplierInterface(string stat, CharacterStats charStats, StatType statType, List<UnitEntityData> units, float multiplier)
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(stat + " ");
            if (GL.Button(Strings.GetText("button_Apply") + $" (X{ Math.Round(multiplier, 1)})", GL.ExpandWidth(false)))
            {
                if (multiplier != 1f)
                {
                    foreach (UnitEntityData unit in units)
                    {
                        CharacterStats stats = unit.Descriptor.Stats;
                        stats.GetStat(statType).BaseValue = Mathf.RoundToInt(stats.GetStat(statType).BaseValue * (float)Math.Round(multiplier, 1));
                    }
                }
                multiplier = 1f;
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }


        public static TextFieldInt unitAlignmentTextField = new TextFieldInt();
        public static void UnitAlignment(UnitEntityData unit)
        {
            GL.BeginVertical("box");
            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("header_Alignment")));
            MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("warning_Alignment")));

            if (Strings.ToBool(settings.togglePreventAlignmentChanges))
            {
                GL.Space(10);
                MenuTools.SingleLineLabel(RichText.Bold(Strings.GetText("warning_PreventAlignmentChanges")));
                GL.Space(10);
            }

            GL.BeginHorizontal();
            Storage.statsSelectedAlignmentIndex = GL.SelectionGrid(Storage.statsSelectedAlignmentIndex, Storage.alignmentsArrayStats, 3);
            GL.EndHorizontal();
            GL.Space(10);

            GL.BeginHorizontal();
            if (Storage.statsSelectedAlignmentIndex < 9)
            {
                if (GL.Button(MenuTools.TextWithTooltip("button_SetAlignment", "tooltip_SetAlignment", "", $" {Storage.alignmentsArrayStats[Storage.statsSelectedAlignmentIndex]}"), GL.ExpandWidth(false)))
                {
                    unit.Descriptor.Alignment.Set((Alignment)Common.IndexToAligment(Storage.statsSelectedAlignmentIndex));
                }
            }
            else
            {
                if (GL.Button(MenuTools.TextWithTooltip("button_AlignmentCantBeSetTo", "tooltip_SetAlignment", "", $"<b> {Storage.alignmentsArrayStats[Storage.statsSelectedAlignmentIndex]} {Strings.GetText("button_AlignmentUseShiftInstead")}</b>", true), GL.ExpandWidth(false)))
                {
                }
            }

            GL.EndHorizontal();

            GL.Space(10);
            unitAlignmentTextField.Render();
            GL.BeginHorizontal();
            if (GL.Button(MenuTools.TextWithTooltip("button_ShiftAlignmentTowards", "tooltip_ShiftAlignmentTowards", "", $" {Storage.alignmentsArrayStats[Storage.statsSelectedAlignmentIndex]} {Strings.GetText("misc_By")} {unitAlignmentTextField.finalAmount}"), GL.ExpandWidth(false)))
            {
                unit.Descriptor.Alignment.Shift((AlignmentShiftDirection)Storage.statsSelectedAlignmentIndex, unitAlignmentTextField.finalAmount, (IAlignmentShiftProvider)null);
            }
            GL.EndHorizontal();
            GL.Space(10);
            MenuTools.SingleLineLabel(Strings.GetText("label_CurrentAlignment") + ": " + Common.AlignmentToString(unit.Descriptor.Alignment.Value));
            GL.EndVertical();
        }

        public static void AddSingleItemAmount(string itemGuid, int itemAmount, bool identify = false)
        {
            if (1 > itemAmount) itemAmount = 1;
            if (settings.itemGuid != Main.ExcludeGuid)
            {
                var itemByGuid = Utilities.GetBlueprintByGuid<BlueprintItem>(itemGuid);
                if (null == itemByGuid) return;
                if (identify)
                {
                    var itemEntity = itemByGuid.CreateEntity();
                    itemEntity.Identify();
                    for (var i = 0; i < itemAmount; i++)
                        Game.Instance.Player.Inventory.Add(itemEntity);
                }
                else
                    Game.Instance.Player.Inventory.Add(itemByGuid, itemAmount);
            }
            else
                modLogger.Log(Storage.notificationEkunQ2rewardArmor);
        }

        public static void SetKeyBinding(ref KeyCode keyCode)
        {
            string label = (keyCode == KeyCode.None) ? Strings.GetText("button_PressKey") : keyCode.ToString();
            if (GL.Button(label, GL.ExpandWidth(false)))
            {
                keyCode = KeyCode.None;
            }
            if (keyCode == KeyCode.None && Event.current != null && Event.current.isKey)
            {
                keyCode = Event.current.keyCode;
            }
        }

        public static void Beep()
        {
            SystemSounds.Beep.Play();
        }


    }

    public static class Strings
    {
        public static Settings settings = Main.Settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.ModLogger;

        public static string RemoveWhitespaces(string s)
        {
            string remove = Regex.Replace(s, @"s", "");
            return remove;
        }

        public static bool ToBool(string s)
        {
            if (s == Storage.isTrueString)
            {
                return true;
            }
            else if (s == Storage.isFalseString)
            {
                return false;
            }
            else
            {
                throw new ArgumentOutOfRangeException("StringToBool received an invalid string!");
            }
        }

        public static string GetText(string key)
        {
            string value;
            if (current.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                modLogger.Log($"{key} not found!");
                return RichText.WarningLargeRedFormat("ERROR");
            }
        }

        public static string GetText(string key, Dictionary<String, String> dict)
        {
            string value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                modLogger.Log($"{key} not found!");
                return RichText.WarningLargeRedFormat("ERROR");
            }
        }

        public static string ToggleSpaceLeftFormat(string s)
        {
            return s = " " + s;
        }

        public static string Parenthesis(string s)
        {
            return s = "(" + s + ")";
        }

        public static string SpaceBeforeCaptialFormat(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return "";
            }
            StringBuilder stringBuilder = new StringBuilder(s.Length * 2);
            stringBuilder.Append(s[0]);
            for (int i = 1; i < s.Length; i++)
            {
                if (char.IsUpper(s[i]) && s[i - 1] != ' ')
                {
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(s[i]);
            }
            return stringBuilder.ToString();
        }

        public static string RemoveExt(string s)
        {
            s = s.Substring(0, s.LastIndexOf('.'));
            return s;
        }

        public static string CleanName(string itemName)
        {
            itemName = Regex.Replace(itemName, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
            itemName = itemName.Replace("Plus", " +");
            itemName = itemName.Replace("_", " ");
            itemName = itemName.Replace("-", " ");
            itemName = Regex.Replace(itemName, @"[ ]{2,}", " ");
            return itemName;
        }

        public class Localisation
        {
            [XmlAttribute]
            public string key;
            [XmlAttribute]
            public string value;
        }

        public static Dictionary<string, string> current = MenuText.fallback;
        public static Dictionary<string, string> temp = new Dictionary<string, string>();

        public static Dictionary<string, string> XMLtoDict(string loc)
        {
            try
            {
                Dictionary<string, string> dict;
                XmlSerializer serializer = new XmlSerializer(typeof(Localisation[]), new XmlRootAttribute() { ElementName = "resources" });
                using (var stream = File.OpenRead(Storage.modEntryPath + Storage.localisationFolder + "\\" + loc + ".xml"))
                {
                    dict = ((Localisation[])serializer.Deserialize(stream)).ToDictionary(i => i.key, i => i.value);
                }
                return dict;
            }
            catch (Exception e)
            {
                Main.ModLogger.Log(e.ToString());
                return MenuText.fallback;
            }

        }
        public static void RefreshStrings()
        {
            Storage.fadeToBlackState = Strings.GetText("misc_Enable");
            Storage.notificationEkunQ2rewardArmor = Strings.GetText("error_EkunQ2");
            Storage.individualSkillsArray = new string[] { Strings.GetText("charStat_Athletics"), Strings.GetText("charStat_KnowledgeArcana"), Strings.GetText("charStat_KnowledgeWorld"), Strings.GetText("charStat_LoreNature"), Strings.GetText("charStat_LoreReligion"), Strings.GetText("charStat_Mobility"), Strings.GetText("charStat_Perception"), Strings.GetText("charStat_Persuasion"), Strings.GetText("charStat_Stealth"), Strings.GetText("charStat_Thievery"), Strings.GetText("charStat_UseMagicDevice"), Strings.GetText("charStat_Bluff"), Strings.GetText("charStat_Diplomacy"), Strings.GetText("charStat_Intimidate") };
            Storage.individualSavesArray = new string[] { Strings.GetText("charStat_Fortitude"), Strings.GetText("charStat_Reflex"), Strings.GetText("charStat_Will") };
            Storage.statsAttributesDict = new Dictionary<string, StatType>
            {
                { Strings.GetText("charStat_Strength"), StatType.Strength},
                { Strings.GetText("charStat_Dexterity"), StatType.Dexterity},
                { Strings.GetText("charStat_Constitution"), StatType.Constitution},
                { Strings.GetText("charStat_Intelligence"), StatType.Intelligence},
                { Strings.GetText("charStat_Wisdom"), StatType.Wisdom},
                { Strings.GetText("charStat_Charisma"), StatType.Charisma}
            };
            Storage.statsCombatDict = new Dictionary<string, StatType>
            {
                { Strings.GetText("charStat_Initiative"), StatType.Initiative},
                { Strings.GetText("charStat_BaseAttackBonus"), StatType.BaseAttackBonus},
                { Strings.GetText("charStat_AdditionalAttackBonus"), StatType.AdditionalAttackBonus},
                { Strings.GetText("charStat_NumberOfAttacksOfOpportunity"), StatType.AttackOfOpportunityCount},
                { Strings.GetText("charStat_SneakAttack"), StatType.SneakAttack},
                { Strings.GetText("charStat_AdditionalDamage"), StatType.AdditionalDamage},
                { Strings.GetText("charStat_AdditionalCombatManeuverBonus"), StatType.AdditionalCMB},
                { Strings.GetText("charStat_AdditionalCombatManeuverDefense"), StatType.AdditionalCMD},
                { Strings.GetText("charStat_ArmourClass"), StatType.AC},
                { Strings.GetText("charStat_Reach"), StatType.Reach},
                { Strings.GetText("charStat_HitPoints"), StatType.HitPoints},
                { Strings.GetText("charStat_Speed"), StatType.Speed}
            };
            Storage.globalString = Strings.GetText("button_SetGlobalMapZoomLevel");
            Storage.alignmentsArrayKingdom = new string[] { Strings.GetText("arrayItem_Alignments_LawfulGood"), Strings.GetText("arrayItem_Alignments_NeutralGood"), Strings.GetText("arrayItem_Alignments_ChaoticGood"), Strings.GetText("arrayItem_Alignments_LawfulNeutral"), Strings.GetText("arrayItem_Alignments_TrueNeutral"), Strings.GetText("arrayItem_Alignments_ChaoticNeutral"), Strings.GetText("arrayItem_Alignments_LawfulEvil"), Strings.GetText("arrayItem_Alignments_NeutralEvil"), Strings.GetText("arrayItem_Alignments_ChaoticEvil"), Strings.GetText("arrayItem_Alignments_NoChange") };
            Storage.alignmentsArrayStats = new string[] { Strings.GetText("arrayItem_Alignments_LawfulGood"), Strings.GetText("arrayItem_Alignments_NeutralGood"), Strings.GetText("arrayItem_Alignments_ChaoticGood"), Strings.GetText("arrayItem_Alignments_LawfulNeutral"), Strings.GetText("arrayItem_Alignments_TrueNeutral"), Strings.GetText("arrayItem_Alignments_ChaoticNeutral"), Strings.GetText("arrayItem_Alignments_LawfulEvil"), Strings.GetText("arrayItem_Alignments_NeutralEvil"), Strings.GetText("arrayItem_Alignments_ChaoticEvil"), Strings.GetText("arrayItem_Alignments_Good"), Strings.GetText("arrayItem_Alignments_Evil"), Strings.GetText("arrayItem_Alignments_Lawful"), Strings.GetText("arrayItem_Alignments_Chaotic") };
            Storage.alignmentsArrayKingdomStats = new string[] { Strings.GetText("arrayItem_Alignments_LawfulGood"), Strings.GetText("arrayItem_Alignments_NeutralGood"), Strings.GetText("arrayItem_Alignments_ChaoticGood"), Strings.GetText("arrayItem_Alignments_LawfulNeutral"), Strings.GetText("arrayItem_Alignments_TrueNeutral"), Strings.GetText("arrayItem_Alignments_ChaoticNeutral"), Strings.GetText("arrayItem_Alignments_LawfulEvil"), Strings.GetText("arrayItem_Alignments_NeutralEvil"), Strings.GetText("arrayItem_Alignments_ChaoticEvil") };
            Storage.takeXArray = new string[] { Strings.GetText("arrayItem_takeX_Off"), Strings.GetText("arrayItem_takeX_Take10"), Strings.GetText("arrayItem_takeX_Take20"), Strings.GetText("arrayItem_takeX_TakeCustomValue") };
            Storage.charSizeArray = new string[] { Strings.GetText("arrayItem_Size_Fine"), Strings.GetText("arrayItem_Size_Diminutive"), Strings.GetText("arrayItem_Size_Tiny"), Strings.GetText("arrayItem_Size_Small"), Strings.GetText("arrayItem_Size_Medium"), Strings.GetText("arrayItem_Size_Large"), Strings.GetText("arrayItem_Size_Huge"), Strings.GetText("arrayItem_Size_Gargantuan"), Strings.GetText("arrayItem_Size_Colossal") };
            Storage.featsParamArray = new string[] { Strings.GetText("misc_None"), Strings.GetText("arrayItem_FeatParam_FencingGrace"), Strings.GetText("arrayItem_FeatParam_ImprovedCritical"), Strings.GetText("arrayItem_FeatParam_SlashingGrace"), Strings.GetText("arrayItem_FeatParam_SwordSaintChosenWeapon"), Strings.GetText("arrayItem_FeatParam_WeaponFocus"), Strings.GetText("arrayItem_FeatParam_WeaponFocusGreater"), Strings.GetText("arrayItem_FeatParam_WeaponMastery"), Strings.GetText("arrayItem_FeatParam_WeaponSpecialization"), Strings.GetText("arrayItem_FeatParam_WeaponSpecializationGreater"), Strings.GetText("arrayItem_FeatParam_SpellFocus"), Strings.GetText("arrayItem_FeatParam_GreaterSpellFocus") };
            Storage.neverRollXArray = new string[] { Strings.GetText("arrayItem_NeverRollX_Everyone"), Strings.GetText("arrayItem_NeverRollX_OnlyParty"), Strings.GetText("arrayItem_NeverRollX_OnlyEnemies") };
            Storage.unitEntityDataSelectionGridArray = new string[] { Strings.GetText("arrayItem_NeverRollX_Everyone"), Strings.GetText("arrayItem_NeverRollX_OnlyParty"), Strings.GetText("arrayItem_NeverRollX_OnlyMainChar"), Strings.GetText("arrayItem_NeverRollX_OnlyEnemies") };
            Storage.unitEntityDataArray = new string[] { Strings.GetText("arrayItem_UnityEntityData_Party"), Strings.GetText("arrayItem_UnityEntityData_ControllableCharacters"), Strings.GetText("arrayItem_UnityEntityData_ActiveCompanions"), Strings.GetText("arrayItem_UnityEntityData_AllCharacters"), Strings.GetText("arrayItem_UnityEntityData_Mercenaries"), Strings.GetText("arrayItem_UnityEntityData_Pets"), Strings.GetText("arrayItem_UnityEntityData_Enemies") };
            Storage.unitEntityDataArrayNoEnemies = Storage.unitEntityDataArray.Take(Storage.unitEntityDataArray.Count() - 1).ToArray();
            Storage.encumbranceArray = new string[] { Strings.GetText("encumbrance_Light"), Strings.GetText("encumbrance_Medium"), Strings.GetText("encumbrance_Heavy"), Strings.GetText("encumbrance_Overload") };
            Storage.inventoryItemTypesArray = new string[] { RichText.Bold(Strings.GetText("misc_All")), Strings.GetText("label_Armours"), Strings.GetText("label_Belts"), Strings.GetText("label_Footwear"), Strings.GetText("label_Gloves"), Strings.GetText("label_Headwear"), Strings.GetText("label_Neckwear"), Strings.GetText("label_NonUsable"), Strings.GetText("misc_Other"), Strings.GetText("label_Rings"), Strings.GetText("label_Shields"), Strings.GetText("label_ShoulderItems"), Strings.GetText("label_UsableItems"), Strings.GetText("label_Weapons"), Strings.GetText("label_WristItems") };
            Storage.weatherArray = new string[] { Strings.GetText("arrayItem_Weather_Normal"), Strings.GetText("arrayItem_Weather_Rain"), Strings.GetText("arrayItem_Weather_Snow") };

        }

    }

    public static class RichText
    {
        public static string Size(string s, int size)
        {
            return s;
        }

        public static string MainCategoryFormat(string s)
        {
            return $"<b> {s}</b>";
        }

        public static string Bold(string s)
        {
            return $"<b> {s}</b>";
        }

        public static string Italic(string s)
        {
            return $"<i> {s}</i>";
        }

        public static string BoldRedFormat(string s)
        {
            return $"<b><color=red>{s}</color></b>";
        }

        public static string WarningLargeRedFormat(string s)
        {
            return $"<b><color=red>{s}</color></b>";
        }

        public static string SizePercent(string s, int percent)
        {
            return $"<size={percent}%>{s}</size>";
        }
    }

    public class UnitEntityDataSelection
    {
        public int indexUnitEntitiyData = 0;
        public int indexUnitEntitiyDataOld = 0;
        public string[] unitEntityDataStrings = Storage.unitEntityDataArray;
        private Player player = Game.Instance.Player;
        public List<UnitEntityData> unitEntityData;
        public List<UnitEntityData> unitEntityDataOld;
        public List<string> unitEntityDataNames = new List<string>();
        public bool updateUnitEntityData = true;

        public void Render()
        {
            GL.BeginHorizontal();
            indexUnitEntitiyData = GL.SelectionGrid(indexUnitEntitiyData, unitEntityDataStrings, 3);
            GL.EndHorizontal();

            switch (indexUnitEntitiyData)
            {
                case 0:
                    unitEntityData = player.Party;
                    break;
                case 1:
                    unitEntityData = player.ControllableCharacters;
                    break;
                case 2:
                    unitEntityData = player.ActiveCompanions;
                    break;
                case 3:
                    unitEntityData = player.AllCharacters;
                    break;
                case 4:
                    unitEntityData = Common.GetCustomCompanions();
                    break;
                case 5:
                    unitEntityData = Common.GetPets();
                    break;
                case 6:
                    unitEntityData = Common.GetEnemies();
                    break;
            }

            this.CheckIndex();
            this.UpdateUnityEntityData();
            this.CheckUnityEntityDataCount();
        }

        private void CheckIndex()
        {
            if (indexUnitEntitiyData != indexUnitEntitiyDataOld)
            {
                updateUnitEntityData = true;
                indexUnitEntitiyDataOld = indexUnitEntitiyData;
            }
        }

        private void UpdateUnityEntityData()
        {
            if (unitEntityData.Count != unitEntityDataOld.Count || Storage.reloadPartyBuffs)
            {
                indexUnitEntitiyData = 0;
                unitEntityDataNames.Clear();
                foreach (UnitEntityData controllableCharacter in unitEntityData)
                {
                    unitEntityDataNames.Add(controllableCharacter.CharacterName);
                }
                unitEntityDataOld = unitEntityData;
                updateUnitEntityData = false;
            }
        }

        private void CheckUnityEntityDataCount()
        {
            if (unitEntityData.Count - 1 < indexUnitEntitiyData)
            {
                indexUnitEntitiyData = unitEntityData.Count - 1;
            }
        }
    }

    public class TextFieldInt
    {
        public string label = "label_Amount";
        public string amount = "";
        public int finalAmount = 0;

        public void Render()
        {
            GL.BeginHorizontal();
            GL.Label(Strings.GetText(label) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            amount = MenuTools.IntTestSettingStage1(amount);
            finalAmount = MenuTools.IntTestSettingStage2(amount, finalAmount);
            GL.EndHorizontal();
        }
        public void Render(string newLabel)
        {
            GL.BeginHorizontal();
            GL.Label(Strings.GetText(newLabel) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            amount = MenuTools.IntTestSettingStage1(amount);
            finalAmount = MenuTools.IntTestSettingStage2(amount, finalAmount);
            GL.EndHorizontal();
        }
        public void RenderNoGL()
        {
            GL.Label(Strings.GetText(label) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            amount = MenuTools.IntTestSettingStage1(amount);
            finalAmount = MenuTools.IntTestSettingStage2(amount, finalAmount);
        }
        public void RenderNoGL(string newLabel)
        {
            GL.Label(Strings.GetText(newLabel) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            amount = MenuTools.IntTestSettingStage1(amount);
            finalAmount = MenuTools.IntTestSettingStage2(amount, finalAmount);
        }
    }

    public class TextFieldFloat
    {
        public string label = "label_Amount";
        public string amount = "";
        public float finalAmount = 0;

        public void Render()
        {
            GL.BeginHorizontal();
            GL.Label(Strings.GetText(label) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            MenuTools.SettingParse(ref amount, ref finalAmount);
            GL.EndHorizontal();
        }
        public void Render(string newLabel)
        {
            GL.BeginHorizontal();
            GL.Label(Strings.GetText(newLabel) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            MenuTools.SettingParse(ref amount, ref finalAmount);
            GL.EndHorizontal();
        }
        public void RenderNoGL()
        {
            GL.Label(Strings.GetText(label) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            MenuTools.SettingParse(ref amount, ref finalAmount);
        }
        public void RenderNoGL(string newLabel)
        {
            GL.Label(Strings.GetText(newLabel) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            MenuTools.SettingParse(ref amount, ref finalAmount);
        }
    }

    public class SelectionGrid
    {
        public int selected = 0;
        private string[] texts;
        private int xCount;

        public SelectionGrid(string[] entries, int entriesPerLine)
        {
            texts = entries;
            xCount = entriesPerLine;
        }

        public void Render()
        {
            GL.BeginHorizontal();
            selected = GL.SelectionGrid(selected, texts, xCount);
            GL.EndHorizontal();
        }

        public void Render(ref int index)
        {
            GL.BeginHorizontal();
            index = GL.SelectionGrid(index, texts, xCount);
            GL.EndHorizontal();
        }
    }
}
