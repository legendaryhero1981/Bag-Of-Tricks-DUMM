using BagOfTricks.Favourites;
using BagOfTricks.Utils;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Alignments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;
using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    public static class MenuTools
    {
        public static Settings settings = Main.settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;


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

        public static void ToggleButtonActionAtOnFavouritesMenu(ref string toggle, Action action, string buttonText, string buttonTooltip)
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

        public static void ToggleButtonNoGroup(ref string toggle, string buttonText, string buttonTooltip, bool boldText = false)
        {
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
            if (FavouritesFactory.GetFavouriteFunctions.FavouritesList.Contains(toggleName + "," + buttonText + "," + buttonTooltip)) {
                if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                    FavouritesFactory.GetFavouriteFunctions.FavouritesList.Remove(toggleName + "," + buttonText + "," + buttonTooltip);
            }
            else {
                if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                    FavouritesFactory.GetFavouriteFunctions.FavouritesList.Add(toggleName + "," + buttonText + "," + buttonTooltip);
            }
            GL.EndHorizontal();
            GL.EndVertical();
        }

        public static GUIContent TextWithTooltip(string textMain, string tooltip, string textFront = "", string textBack = "", bool boldText = false)
        {

            if (boldText)
                return new GUIContent(textFront + RichTextUtils.Bold(Strings.GetText(textMain)) + textBack, Strings.GetText(tooltip));
            else
                return new GUIContent(textFront + Strings.GetText(textMain) + textBack, Strings.GetText(tooltip));
        }

        public static GUIContent TextWithTooltip(string textMain, string tooltip, bool boldText = false)
        {

            if (boldText)
            {
                return new GUIContent(RichTextUtils.Bold(Strings.GetText(textMain)), Strings.GetText(tooltip));
            }
            else
            {
                return new GUIContent(Strings.GetText(textMain), Strings.GetText(tooltip));
            }
        }

        public static void SingleLineLabelCopyBlueprintDetail(string labelString)
        {
            GL.BeginHorizontal();
            GL.Label(labelString);
            GL.FlexibleSpace();
            if (GL.Button(Strings.GetText("label_CopyToClipboard")))
            {
                if (labelString != null)
                {
                    string clipboardString = labelString.Split(':')[1].Substring(5);
                    try
                    {
                        GUIUtility.systemCopyBuffer = clipboardString;
                    }
                    catch (Exception e)
                    {

                        modLogger.Log(e.ToString());
                    }
                }
            }
            GL.EndHorizontal();
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

        public static void SingleLineLabelGt(string label, string tooltip, bool bold = false)
        {
            GUIContent content;
            if (bold)
                content = new GUIContent(RichTextUtils.Bold(Strings.GetText(label)), Strings.GetText(tooltip));
            else
                content = new GUIContent(Strings.GetText(label), Strings.GetText(tooltip));
            GL.BeginHorizontal();
            GL.Label(content);
            GL.EndHorizontal();
        }

        public static void SingleLineLabelGt(string label, bool bold = false)
        {
            string content = Strings.GetText(label);
            if (bold) content = RichTextUtils.Bold(content);

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
            var tooltipString = new GUIContent(GUI.tooltip);
            var styleRect = GUI.skin.box;
            var tooltipSize = styleRect.CalcSize(tooltipString);
            var styleTooltip = new GUIStyle();
            var background = new Texture2D(1, 1);
            var textHeight = styleRect.CalcHeight(tooltipString, tooltipSize.x);
            if (!string.IsNullOrEmpty(GUI.tooltip))
            {
                background.SetPixels32(new[] { new Color32(0, 0, 0, 220) });
                var state = new GUIStyleState { background = background };
                styleTooltip.normal = state;
            }
            else
            {
                background.SetPixels32(new[] { new Color32(0, 0, 0, 0) });
            }
            var rectX = Input.mousePosition.x - Storage.ummRect.min.x + Storage.ummScrollPosition[Storage.ummTabId].x - 8;
            if (rectX > 470)
            {
                rectX = rectX - tooltipSize.x + 8;
            }
            var rectY = Screen.height - Storage.ummRect.min.y + Storage.ummScrollPosition[Storage.ummTabId].y - Input.mousePosition.y - 65 - textHeight;
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
                        return ref settings.toggleFfAoe;
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
                        return ref settings.toggleAnimationCloseUmm;
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
                        return ref settings.toggleNoTempHpKineticist;
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
                    case "toggleMakeSummmonsControllable":
                        return ref settings.toggleMakeSummmonsControllable;
                    case "toggleArmourChecksPenalty0":
                        return ref settings.toggleArmourChecksPenalty0;
                    case "toggleDisableWarpaintedSkullAbilityForSummonedBarbarians":
                        return ref settings.toggleDisableWarpaintedSkullAbilityForSummonedBarbarians;
                    case "toggleRemoveSummonsGlow":
                        return ref settings.toggleRemoveSummonsGlow;
                    case "toggleTabletopSpellAbilityRange":
                        return ref settings.toggleTabletopSpellAbilityRange;
                    case "toggleAutomaticallyLoadLastSave":
                        return ref settings.toggleAutomaticallyLoadLastSave;
                    case "toggleSpawnEnemiesFromUnitFavourites":
                        return ref settings.toggleSpawnEnemiesFromUnitFavourites;
                    case "toggleAllowCampingEverywhere":
                        return ref settings.toggleAllowCampingEverywhere;
                    case "toggleEnableRandomEncounterSettings":
                        return ref settings.toggleEnableRandomEncounterSettings;
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
            if (FavouritesFactory.GetFavouriteFunctions.FavouritesList.Contains(methodName)) {
                if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false)))
                    FavouritesFactory.GetFavouriteFunctions.FavouritesList.Remove(methodName);
            }
            else {
                if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false)))
                    FavouritesFactory.GetFavouriteFunctions.FavouritesList.Add(methodName);
            }
        }

        public static void AddFavouriteButton(string methodName, int size)
        {
            if (FavouritesFactory.GetFavouriteFunctions.FavouritesList.Contains(methodName)) {
                if (GL.Button(RichTextUtils.Size(Storage.favouriteTrueString, size), GL.ExpandWidth(false)))
                    FavouritesFactory.GetFavouriteFunctions.FavouritesList.Remove(methodName);
            }
            else {
                if (GL.Button(RichTextUtils.Size(Storage.favouriteFalseString, size), GL.ExpandWidth(false)))
                    FavouritesFactory.GetFavouriteFunctions.FavouritesList.Add(methodName);
            }
        }

        public static void FlexibleSpaceFavouriteButtonEndHorizontal(string methodName)
        {
            GL.FlexibleSpace();
            AddFavouriteButton(methodName);
            GL.EndHorizontal();
        }

        public static void FlexibleSpaceCategoryMenuElementsEndHorizontal(string methodName)
        {
            GL.FlexibleSpace();
            CategoryMenuElements(methodName, ref settings.cheatsCategories);
            GL.EndHorizontal();
        }

        public static void AddRemoveButton(ref List<string> favouritesList, string guid, ref bool favouritesLoad, string buttonTrue, string buttonFalse)
        {
            if (favouritesList.Contains(guid))
            {
                if (GL.Button(buttonTrue, GL.ExpandWidth(false)))
                {
                    favouritesList.Remove(guid);
                    favouritesLoad = true;
                }
            }
            else
            {
                if (GL.Button(buttonFalse, GL.ExpandWidth(false)))
                {
                    favouritesList.Add(guid);
                    favouritesLoad = true;
                }
            }
        }

        public static void AddRemoveButtonShiftAdd(ref List<string> favouritesList, string guid, ref bool favouritesLoad, string buttonTrue, string buttonFalse, string tooltip)
        {
            if (favouritesList.Contains(guid))
            {
                if (GL.Button(new GUIContent(buttonTrue, tooltip), GL.ExpandWidth(false)))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        favouritesList.Add(guid);
                        favouritesLoad = true;
                    }
                    else
                    {
                        favouritesList.Remove(guid);
                        favouritesLoad = true;
                    }
                }
            }
            else
            {
                if (GL.Button(buttonFalse, GL.ExpandWidth(false)))
                {
                    favouritesList.Add(guid);
                    favouritesLoad = true;
                }
            }
        }

        public static void ExportCopyGuidsNamesButtons(string[] resultGuids, string[] resultNames, string filename)
        {
            CopyExportButtons("button_ExportCurrentSearchResultGuids", $"{filename}-guids.txt", resultGuids);
            CopyExportButtons("button_ExportCurrentSearchResultNames", $"{filename}-names.txt", resultNames);
        }

        public static void ExportButton(string label, string fileNameWithFormat, string[] infomration)
        {
            if (GL.Button(Strings.GetText(label), GL.ExpandWidth(false)))
            {
                File.WriteAllLines(Path.Combine(Common.ExportPath(), fileNameWithFormat), infomration);
            }
        }

        public static void ButtonCopy(string content, string label = "label_CopyToClipboard")
        {
            if (GL.Button(Strings.GetText(label)))
            {
                try
                {
                    GUIUtility.systemCopyBuffer = content;
                }
                catch (Exception e)
                {

                    modLogger.Log(e.ToString());
                }
            }
        }

        public static void CopyExportButtons(string exportLabel, string fileNameWithFormat, string[] information, string labelCopy = "label_CopyToClipboard")
        {
            GL.BeginHorizontal();
            ExportButton(exportLabel, fileNameWithFormat, information);
            GL.FlexibleSpace();
            ButtonCopy(string.Join("\n", information), labelCopy);
            GL.EndHorizontal();
            GL.BeginHorizontal();
            GL.Label(" " + Strings.GetText("label_Location") + $": {Path.Combine(Common.ExportPath(), fileNameWithFormat)}");
            GL.EndHorizontal();
        }

        public static void FilterButton(ref string toggle, string label, Action<string, string> searchMethod, string searchTerm, string filters)
        {
            if (GL.Button($"{toggle} " + Strings.GetText(label), GL.ExpandWidth(false)))
            {
                if (toggle == Storage.isTrueString)
                {
                    toggle = Storage.isFalseString;
                }
                else
                {
                    toggle = Storage.isTrueString;
                }
                searchMethod(searchTerm, filters);
            }
        }

        public static void AddUpDownButtons(string methodName, ref string[] methods, int size)
        {
            if (Array.IndexOf(methods, methodName) > 0)
            {
                if (GL.Button(new GUIContent(RichTextUtils.Size(Storage.upArrow, size), Strings.GetText("tooltip_ArrowUp")), GL.ExpandWidth(false)))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        Common.MakeArrayElementFirst(ref methods, methodName);
                    else
                        Common.MoveArrayElementDown(ref methods, methodName);
                }
            }
            if (Array.IndexOf(methods, methodName) < methods.Length - 1)
            {
                if (GL.Button(new GUIContent(RichTextUtils.Size(Storage.downArrow, size), Strings.GetText("tooltip_ArrowDown")), GL.ExpandWidth(false)))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        Common.MakeArrayElementLast(ref methods, methodName);
                    else
                        Common.MoveArrayElementUp(ref methods, methodName);
                }
            }
        }

        public static void AddUpDownButtons(string methodName, ref List<string> methods, int arrowSize) {
            if (methods.IndexOf(methodName) > 0) {
                if (GL.Button(new GUIContent(RichTextUtils.Size(Storage.upArrow, arrowSize), Strings.GetText("tooltip_ArrowUp")), GL.ExpandWidth(false))) {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        ListUtils.MakeElementFirst(ref methods, methodName);
                    else
                        ListUtils.MoveElementUp(ref methods, methodName);
                }
            }
            if (methods.IndexOf(methodName) < methods.Count - 1) {
                if (GL.Button(new GUIContent(RichTextUtils.Size(Storage.downArrow, arrowSize), Strings.GetText("tooltip_ArrowDown")), GL.ExpandWidth(false))) {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        ListUtils.MakeElementLast(ref methods, methodName);
                    else
                        ListUtils.MoveElementDown(ref methods, methodName);
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

        public static void SettingsFieldNoLabel(ref string stringSetting, ref float finalSetting, int maxLength, float width)
        {
            GL.BeginHorizontal();
            stringSetting = GL.TextField(stringSetting, maxLength, GL.Width(width));
            SettingParse(ref stringSetting, ref finalSetting);
            GL.EndHorizontal();
        }

        public static void SettingsField(ref string stringSetting, ref float finalSetting, string label, string tooltip, bool noZero = true)
        {
            GL.BeginHorizontal();
            if (tooltip != null)
            {
                GL.Label(TextWithTooltip(label, tooltip, "", ": "), GL.ExpandWidth(false));
            }
            else
            {
                GL.Label(Strings.GetText(label) + ": ", GL.ExpandWidth(false));
            }

            stringSetting = GL.TextField(stringSetting, 6, GL.Width(50f));
            SettingParse(ref stringSetting, ref finalSetting, noZero);
            GL.EndHorizontal();
        }

        public static void SettingsField(ref string stringSetting, ref float finalSetting, string label)
        {
            SettingsField(ref stringSetting, ref finalSetting, label, null);
        }

        public static void SettingsField(ref string stringSetting, ref float finalSetting)
        {
            SettingsField(ref stringSetting, ref finalSetting, "headerOption_SettingsValue");
        }

        public static void CurrentMultiplier(float multiplier)
        {
            GL.BeginHorizontal();
            GL.Label(Strings.GetText("label_CurrentMultiplier") + $": {multiplier}", GL.ExpandWidth(false));
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

        public static void SettingParse(ref string stringSetting, ref float finalSetting, bool noZero = true)
        {
            if (noZero && stringSetting == "0" || stringSetting == "0..")
            {
                stringSetting = "0.";
            }
            else if (float.TryParse(stringSetting, out float result))
            {
                if (!noZero || (noZero && result != 0))
                {
                    finalSetting = result;
                }
            }

            else
            {
                stringSetting = "";
            }
        }

        public static void CreateStatInterface(string stat, CharacterStats charStats, StatType statType, int amount, bool isSkill = false)
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label($"{stat}: {charStats.GetStat(statType).BaseValue} ({charStats.GetStat(statType).ModifiedValue})");
            if (GL.Button("<b>+</b>", GL.ExpandWidth(false)))
            {
                charStats.GetStat(statType).BaseValue++;
            }
            if (GL.Button("<b>-</b>", GL.ExpandWidth(false)))
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
            MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("header_Alignment")));
            MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("warning_Alignment")));

            if (Strings.ToBool(settings.togglePreventAlignmentChanges))
            {
                GL.Space(10);
                MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("warning_PreventAlignmentChanges")));
                GL.Space(10);
            }

            GL.BeginHorizontal();
            Storage.statsSelectedAlignmentIndex = GL.SelectionGrid(Storage.statsSelectedAlignmentIndex, Storage.alignmentsArrayStats, 3);
            GL.EndHorizontal();
            GL.Space(10);

            GL.BeginHorizontal();
            if (Storage.statsSelectedAlignmentIndex < 9)
            {
                if (GL.Button(TextWithTooltip("button_SetAlignment", "tooltip_SetAlignment", "", $" {Storage.alignmentsArrayStats[Storage.statsSelectedAlignmentIndex]}"), GL.ExpandWidth(false)))
                {
                    unit.Descriptor.Alignment.Set((Alignment)Common.IndexToAligment(Storage.statsSelectedAlignmentIndex));
                }
            }
            else
            {
                if (GL.Button(TextWithTooltip("button_AlignmentCantBeSetTo", "tooltip_SetAlignment", "", $"<b> {Storage.alignmentsArrayStats[Storage.statsSelectedAlignmentIndex]} {Strings.GetText("button_AlignmentUseShiftInstead")}</b>", true), GL.ExpandWidth(false)))
                {
                }
            }

            GL.EndHorizontal();

            GL.Space(10);
            unitAlignmentTextField.RenderField();
            GL.BeginHorizontal();
            if (GL.Button(TextWithTooltip("button_ShiftAlignmentTowards", "tooltip_ShiftAlignmentTowards", "", $" {Storage.alignmentsArrayStats[Storage.statsSelectedAlignmentIndex]} {Strings.GetText("misc_By")} {unitAlignmentTextField.finalAmount}"), GL.ExpandWidth(false)))
            {
                unit.Descriptor.Alignment.Shift((AlignmentShiftDirection)Storage.statsSelectedAlignmentIndex, unitAlignmentTextField.finalAmount, (IAlignmentShiftProvider)null);
            }
            GL.EndHorizontal();
            GL.Space(10);
            SingleLineLabel(Strings.GetText("label_CurrentAlignment") + ": " + Common.AlignmentToString(unit.Descriptor.Alignment.Value));
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
                    ItemEntity itemEntity = Utilities.GetBlueprintByGuid<BlueprintItem>(itemGuid).CreateEntity();
                    itemEntity.Identify();
                    for (var i = 0; i < itemAmount; i++)
                        Game.Instance.Player.Inventory.Add(itemEntity);
                }
                else Game.Instance.Player.Inventory.Add(itemByGuid, itemAmount);
            }
            else modLogger.Log(Storage.notificationEkunQ2RewardArmor);
        }

        private static KeyCode[] mouseButtonsValid = { KeyCode.Mouse3, KeyCode.Mouse4, KeyCode.Mouse5, KeyCode.Mouse6 };
        public static void SetKeyBinding(ref KeyCode keyCode)
        {
            string label = (keyCode == KeyCode.None) ? Strings.GetText("button_PressKey") : keyCode.ToString();
            if (GL.Button(label, GL.ExpandWidth(false)))
            {
                keyCode = KeyCode.None;
            }
            if (keyCode == KeyCode.None && Event.current != null)
            {
                if (Event.current.isKey)
                {
                    keyCode = Event.current.keyCode;
                }
                foreach (KeyCode mouseButton in mouseButtonsValid)
                {
                    if (Input.GetKey(mouseButton))
                    {
                        keyCode = mouseButton;
                    }
                }
            }
        }

        public static void Beep()
        {
            SystemSounds.Beep.Play();
        }

        public static void IncompatibilityWarning(string warningLabel, string toggleA, string toggleB)
        {
            if (Strings.ToBool(toggleA) && Strings.ToBool(toggleB))
            {
                SingleLineLabel(warningLabel);
            }
        }

        public static void CurrentValue(string setting)
        {
            SingleLineLabel(Strings.GetText("label_CurrentValue") + $": {setting}");
        }

        public static void CurrentValue<T>(T setting)
        {
            SingleLineLabel(Strings.GetText("label_CurrentValue") + $": {setting}");
        }

        public static void CurrentValueNoGroup(string setting, bool expandWidth = false)
        {
            GL.Label(Strings.GetText("label_CurrentValue") + $": {setting}", GL.ExpandWidth(expandWidth));
        }

        public static void CurrentValueNoGroup<T>(T setting, bool expandWidth = false)
        {
            GL.Label(Strings.GetText("label_CurrentValue") + $": {setting}", GL.ExpandWidth(expandWidth));
        }
    }

    public class Localisation
    {
        [XmlAttribute]
        public string key;
        [XmlAttribute]
        public string value;
    }

    public static class Strings
    {
        public static Settings settings = Main.settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;

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
                return RichTextUtils.WarningLargeRedFormat("ERROR");
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
                return RichTextUtils.WarningLargeRedFormat("ERROR");
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

        public static Dictionary<string, string> current = MenuText.fallback;
        public static Dictionary<string, string> temp = new Dictionary<string, string>();

        public static Dictionary<string, string> XmLtoDict(string loc)
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
                Main.modLogger.Log(e.ToString());
                return MenuText.fallback;
            }

        }
        public static void RefreshStrings()
        {
            Storage.fadeToBlackState = GetText("misc_Enable");
            Storage.notificationEkunQ2RewardArmor = GetText("error_EkunQ2");
            Storage.individualSkillsArray = new[] { GetText("charStat_Athletics"), GetText("charStat_KnowledgeArcana"), GetText("charStat_KnowledgeWorld"), GetText("charStat_LoreNature"), GetText("charStat_LoreReligion"), GetText("charStat_Mobility"), GetText("charStat_Perception"), GetText("charStat_Persuasion"), GetText("charStat_Stealth"), GetText("charStat_Thievery"), GetText("charStat_UseMagicDevice"), GetText("charStat_Bluff"), GetText("charStat_Diplomacy"), GetText("charStat_Intimidate") };
            Storage.individualSavesArray = new[] { GetText("charStat_Fortitude"), GetText("charStat_Reflex"), GetText("charStat_Will") };
            Storage.statsAttributesDict = new Dictionary<string, StatType>
            {
                { GetText("charStat_Strength"), StatType.Strength},
                { GetText("charStat_Dexterity"), StatType.Dexterity},
                { GetText("charStat_Constitution"), StatType.Constitution},
                { GetText("charStat_Intelligence"), StatType.Intelligence},
                { GetText("charStat_Wisdom"), StatType.Wisdom},
                { GetText("charStat_Charisma"), StatType.Charisma}
            };
            Storage.statsCombatDict = new Dictionary<string, StatType>
            {
                { GetText("charStat_Initiative"), StatType.Initiative},
                { GetText("charStat_BaseAttackBonus"), StatType.BaseAttackBonus},
                { GetText("charStat_AdditionalAttackBonus"), StatType.AdditionalAttackBonus},
                { GetText("charStat_NumberOfAttacksOfOpportunity"), StatType.AttackOfOpportunityCount},
                { GetText("charStat_SneakAttack"), StatType.SneakAttack},
                { GetText("charStat_AdditionalDamage"), StatType.AdditionalDamage},
                { GetText("charStat_AdditionalCombatManeuverBonus"), StatType.AdditionalCMB},
                { GetText("charStat_AdditionalCombatManeuverDefense"), StatType.AdditionalCMD},
                { GetText("charStat_ArmourClass"), StatType.AC},
                { GetText("charStat_Reach"), StatType.Reach},
                { GetText("charStat_HitPoints"), StatType.HitPoints},
                { GetText("charStat_Speed"), StatType.Speed}
            };
            Storage.globalString = GetText("button_SetGlobalMapZoomLevel");
            Storage.alignmentsArrayKingdom = new[] { GetText("arrayItem_Alignments_LawfulGood"), GetText("arrayItem_Alignments_NeutralGood"), GetText("arrayItem_Alignments_ChaoticGood"), GetText("arrayItem_Alignments_LawfulNeutral"), GetText("arrayItem_Alignments_TrueNeutral"), GetText("arrayItem_Alignments_ChaoticNeutral"), GetText("arrayItem_Alignments_LawfulEvil"), GetText("arrayItem_Alignments_NeutralEvil"), GetText("arrayItem_Alignments_ChaoticEvil"), GetText("arrayItem_Alignments_NoChange") };
            Storage.alignmentsArrayStats = new[] { GetText("arrayItem_Alignments_LawfulGood"), GetText("arrayItem_Alignments_NeutralGood"), GetText("arrayItem_Alignments_ChaoticGood"), GetText("arrayItem_Alignments_LawfulNeutral"), GetText("arrayItem_Alignments_TrueNeutral"), GetText("arrayItem_Alignments_ChaoticNeutral"), GetText("arrayItem_Alignments_LawfulEvil"), GetText("arrayItem_Alignments_NeutralEvil"), GetText("arrayItem_Alignments_ChaoticEvil"), GetText("arrayItem_Alignments_Good"), GetText("arrayItem_Alignments_Evil"), GetText("arrayItem_Alignments_Lawful"), GetText("arrayItem_Alignments_Chaotic") };
            Storage.alignmentsArrayKingdomStats = new[] { GetText("arrayItem_Alignments_LawfulGood"), GetText("arrayItem_Alignments_NeutralGood"), GetText("arrayItem_Alignments_ChaoticGood"), GetText("arrayItem_Alignments_LawfulNeutral"), GetText("arrayItem_Alignments_TrueNeutral"), GetText("arrayItem_Alignments_ChaoticNeutral"), GetText("arrayItem_Alignments_LawfulEvil"), GetText("arrayItem_Alignments_NeutralEvil"), GetText("arrayItem_Alignments_ChaoticEvil") };
            Storage.takeXArray = new[] { GetText("arrayItem_takeX_Off"), GetText("arrayItem_takeX_Take10"), GetText("arrayItem_takeX_Take20"), GetText("arrayItem_takeX_TakeCustomValue") };
            Storage.charSizeArray = new[] { GetText("arrayItem_Size_Fine"), GetText("arrayItem_Size_Diminutive"), GetText("arrayItem_Size_Tiny"), GetText("arrayItem_Size_Small"), GetText("arrayItem_Size_Medium"), GetText("arrayItem_Size_Large"), GetText("arrayItem_Size_Huge"), GetText("arrayItem_Size_Gargantuan"), GetText("arrayItem_Size_Colossal") };
            Storage.featsParamArray = new[] { GetText("misc_None"), GetText("arrayItem_FeatParam_FencingGrace"), GetText("arrayItem_FeatParam_ImprovedCritical"), GetText("arrayItem_FeatParam_SlashingGrace"), GetText("arrayItem_FeatParam_SwordSaintChosenWeapon"), GetText("arrayItem_FeatParam_WeaponFocus"), GetText("arrayItem_FeatParam_WeaponFocusGreater"), GetText("arrayItem_FeatParam_WeaponMastery"), GetText("arrayItem_FeatParam_WeaponSpecialization"), GetText("arrayItem_FeatParam_WeaponSpecializationGreater"), GetText("arrayItem_FeatParam_SpellFocus"), GetText("arrayItem_FeatParam_GreaterSpellFocus") };
            Storage.neverRollXArray = new[] { GetText("arrayItem_NeverRollX_Everyone"), GetText("arrayItem_NeverRollX_OnlyParty"), GetText("arrayItem_NeverRollX_OnlyEnemies") };
            Storage.unitEntityDataSelectionGridArray = new[] { GetText("arrayItem_NeverRollX_Everyone"), GetText("arrayItem_NeverRollX_OnlyParty"), GetText("arrayItem_NeverRollX_OnlyMainChar"), GetText("arrayItem_NeverRollX_OnlyEnemies") };
            Storage.unitEntityDataArray = new[] { GetText("arrayItem_UnityEntityData_Party"), GetText("arrayItem_UnityEntityData_ControllableCharacters"), GetText("arrayItem_UnityEntityData_ActiveCompanions"), GetText("arrayItem_UnityEntityData_AllCharacters"), GetText("arrayItem_UnityEntityData_Mercenaries"), GetText("arrayItem_UnityEntityData_Pets"), GetText("arrayItem_UnityEntityData_Enemies") };
            Storage.unitEntityDataArrayNoEnemies = Storage.unitEntityDataArray.Take(Storage.unitEntityDataArray.Count() - 1).ToArray();
            Storage.encumbranceArray = new[] { GetText("encumbrance_Light"), GetText("encumbrance_Medium"), GetText("encumbrance_Heavy"), GetText("encumbrance_Overload") };
            Storage.inventoryItemTypesArray = new [] { RichTextUtils.Bold(Strings.GetText("misc_All")), Strings.GetText("label_Armours"), Strings.GetText("label_Belts"), Strings.GetText("label_Footwear"), Strings.GetText("label_Gloves"), Strings.GetText("label_Headwear"), Strings.GetText("label_Neckwear"), Strings.GetText("label_NonUsable"), Strings.GetText("misc_Other"), Strings.GetText("label_Rings"), Strings.GetText("label_Shields"), Strings.GetText("label_ShoulderItems"), Strings.GetText("label_UsableItems"), Strings.GetText("label_Weapons"), Strings.GetText("label_WristItems") };
            Storage.weatherArray = new[] { GetText("arrayItem_Weather_Normal"), GetText("arrayItem_Weather_Rain"), GetText("arrayItem_Weather_Snow") };
        }
    }

    public class UnitEntityDataSelection
    {
        public int indexUnitEntitiyData = 0;
        public int indexUnitEntitiyDataOld = 0;
        public string[] unitEntityDataStrings = Storage.unitEntityDataArray;
        private Player _player = Game.Instance.Player;
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
                    unitEntityData = _player.Party;
                    break;
                case 1:
                    unitEntityData = _player.ControllableCharacters;
                    break;
                case 2:
                    unitEntityData = _player.ActiveCompanions;
                    break;
                case 3:
                    unitEntityData = _player.AllCharacters;
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

            CheckIndex();
            UpdateUnityEntityData();
            CheckUnityEntityDataCount();
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

        public void Render(string buttonLabel, ref int setting)
        {
            RenderField(label);
            GL.Space(10);
            RenderSetButton(buttonLabel, ref setting);
        }

        public void RenderField()
        {
            RenderField(label);
        }

        public void RenderField(string newLabel)
        {
            GL.BeginHorizontal();
            RenderFieldNoGroup(label);
            GL.EndHorizontal();
        }

        public void RenderFieldNoGroup()
        {
            RenderFieldNoGroup(label);
        }

        public void RenderFieldNoGroup(string newLabel)
        {
            GL.Label(Strings.GetText(newLabel) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            MenuTools.SettingParse(ref amount, ref finalAmount);
        }

        public void RenderSetButton(string buttonLabel, ref int setting)
        {
            GL.BeginHorizontal();
            RenderSetButtonNoGroup(buttonLabel, ref setting);
            GL.EndHorizontal();
        }

        public void RenderSetButtonNoGroup(string buttonLabel, ref int setting)
        {
            if (GL.Button(buttonLabel + $" {finalAmount}", GL.ExpandWidth(false)))
            {
                setting = finalAmount;
            }
        }
    }

    public class TextFieldFloat
    {
        public string label = "label_Amount";
        public string amount = "";
        public float finalAmount = 0;

        public void RenderField()
        {
            GL.BeginHorizontal();
            GL.Label(Strings.GetText(label) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            MenuTools.SettingParse(ref amount, ref finalAmount);
            GL.EndHorizontal();
        }

        public void RenderField(string newLabel)
        {
            GL.BeginHorizontal();
            GL.Label(Strings.GetText(newLabel) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            MenuTools.SettingParse(ref amount, ref finalAmount);
            GL.EndHorizontal();
        }

        public void RenderFieldNoGroup()
        {
            GL.Label(Strings.GetText(label) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            MenuTools.SettingParse(ref amount, ref finalAmount);
        }

        public void RenderFieldNoGroup(string newLabel)
        {
            GL.Label(Strings.GetText(newLabel) + ": ", GL.ExpandWidth(false));
            amount = GL.TextField(amount, GL.Width(230f));
            MenuTools.SettingParse(ref amount, ref finalAmount);
        }

        public void RendeSetButton(string buttonLabel, ref float setting)
        {
            GL.BeginHorizontal();
            if (GL.Button(buttonLabel + $" {finalAmount}", GL.ExpandWidth(false)))
            {
                setting = finalAmount;
            }
            GL.EndHorizontal();
        }

        public void RendeSetButtonNoGroup(string buttonLabel, ref float setting)
        {
            if (GL.Button(buttonLabel + $" {finalAmount}", GL.ExpandWidth(false)))
            {
                setting = finalAmount;
            }
        }
    }

    public class SelectionGrid
    {
        public int selected = 0;
        private string[] _texts;
        private int _xCount;

        public SelectionGrid(string[] entries, int entriesPerLine)
        {
            _texts = entries;
            _xCount = entriesPerLine;
        }

        public void Render()
        {
            GL.BeginHorizontal();
            selected = GL.SelectionGrid(selected, _texts, _xCount);
            GL.EndHorizontal();
        }
        public void RenderNoGroup()
        {
            selected = GL.SelectionGrid(selected, _texts, _xCount, GL.ExpandWidth(false));
        }

        public void Render(ref int index)
        {
            GL.BeginHorizontal();
            index = GL.SelectionGrid(index, _texts, _xCount);
            GL.EndHorizontal();
        }
    }

    public class MultiplierCustom
    {
        private float _sliderMin;
        private float _sliderMax;
        private float _multiplierSlider = 1f;
        private bool _useCustom = false;
        private TextFieldFloat _customMultiplierTextFieldFloat = new TextFieldFloat();
        private bool _settingsLoaded = false;

        public MultiplierCustom(float min, float max)
        {
            _sliderMin = min;
            _sliderMax = max;
        }

        public void LoadSettings(float settingSlider, float settingCustom, bool settingUseCustom)
        {
            if (!_settingsLoaded)
            {
                _multiplierSlider = Mathf.Clamp(settingSlider, _sliderMin, _sliderMax);
                _customMultiplierTextFieldFloat.amount = settingCustom.ToString();
                _customMultiplierTextFieldFloat.finalAmount = settingCustom;
                _useCustom = settingUseCustom;
                _settingsLoaded = true;
            }
        }

        public void Render(ref float settingSlider, ref float settingCustom, ref bool settingUseCustom)
        {
            GL.BeginHorizontal();
            GL.Label(MenuTools.TextWithTooltip("header_Multiplier", "tooltip_Multiplier", "", " "), GL.ExpandWidth(false));
            _multiplierSlider = GL.HorizontalSlider(_multiplierSlider, _sliderMin, _sliderMax, GL.Width(300f));
            settingSlider = _multiplierSlider;
            GL.Label($" {Math.Round(_multiplierSlider, 1)}", GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            _useCustom = GL.Toggle(_useCustom, MenuTools.TextWithTooltip("header_UseCustomMultiplier", "tooltip_CustomMultiplier", " "), GL.ExpandWidth(false));
            GL.EndHorizontal();
            settingUseCustom = _useCustom;
            if (_useCustom == true)
            {
                _customMultiplierTextFieldFloat.RenderField();
                settingCustom = _customMultiplierTextFieldFloat.finalAmount;
                GL.BeginHorizontal();
                GL.Label(Strings.GetText("label_CurrentMultiplier") + $": {_customMultiplierTextFieldFloat.finalAmount}");
                GL.EndHorizontal();
            }
        }
    }

    public class SimpleShowHideButton
    {
        private string _buttonText;
        public string GetButtonText { get => _buttonText; }

        public SimpleShowHideButton(bool defaultHide)
        {
            if (defaultHide)
            {
                _buttonText = Strings.GetText("misc_Hide");
            }
            else
            {
                _buttonText = Strings.GetText("misc_Display");
            }

        }

        public void Render()
        {
            if (GL.Button(_buttonText, GL.ExpandWidth(false)))
            {
                if (_buttonText == Strings.GetText("misc_Hide"))
                {
                    _buttonText = Strings.GetText("misc_Display");
                }
                else if (_buttonText == Strings.GetText("misc_Display"))
                {
                    _buttonText = Strings.GetText("misc_Hide");

                }
                else
                {
                    _buttonText = Strings.GetText("misc_Display");
                }
            }
        }
    }
}
