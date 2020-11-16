using BagOfTricks.Favourites;
using BagOfTricks.Utils;
using BagOfTricks.Utils.Kingmaker;
using BagOfTricks.Utils.Mods;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks.ModUI.CheatsUI
{
    public static class FeatsMenu {
        private static Settings settings = Main.settings;
        private static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;

        public static void Render() {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            settings.showFeatsCategory = GL.Toggle(settings.showFeatsCategory, RichTextUtils.MainCategoryFormat(Strings.GetText("mainCategory_Feats")), GL.ExpandWidth(false));
            if (!settings.showFeatsCategory) {
                GL.EndHorizontal();
            }
            else {
                MenuTools.FlexibleSpaceCategoryMenuElementsEndHorizontal("Feats");

                GL.Space(10);

                MenuTools.SingleLineLabel(RichTextUtils.WarningLargeRedFormat(Strings.GetText("warning_Feats_0")));
                MenuTools.SingleLineLabel(RichTextUtils.WarningLargeRedFormat(Strings.GetText("warning_Feats_1")));

                GL.Space(10);

                GL.BeginHorizontal();
                Storage.featsFilterUnitEntityDataIndex = GL.SelectionGrid(Storage.featsFilterUnitEntityDataIndex, Storage.unitEntityDataArray, 3);
                GL.EndHorizontal();

                Player player = Game.Instance.Player;

                switch (Storage.featsFilterUnitEntityDataIndex) {
                    case 0:
                        Storage.featsUnitEntityData = player.Party;
                        break;
                    case 1:
                        Storage.featsUnitEntityData = player.ControllableCharacters;
                        break;
                    case 2:
                        Storage.featsUnitEntityData = player.ActiveCompanions;
                        break;
                    case 3:
                        Storage.featsUnitEntityData = player.AllCharacters;
                        break;
                    case 4:
                        Storage.featsUnitEntityData = PartyUtils.GetCustomCompanions();
                        break;
                    case 5:
                        Storage.featsUnitEntityData = PartyUtils.GetPets();
                        break;
                    case 6:
                        Storage.featsUnitEntityData = Common.GetEnemies();
                        break;
                }
                if (Storage.featsFilterUnitEntityDataIndex != Storage.featsFilterUnitEntityDataIndexOld) {
                    Storage.reloadPartyFeats = true;
                    Storage.featsFilterUnitEntityDataIndexOld = Storage.featsFilterUnitEntityDataIndex;
                }
                if (Storage.featsUnitEntityData.Count != Storage.featsAllUnits.Count || Storage.reloadPartyFeats) {
                    Storage.featsSelectedControllableCharacterIndex = 0;
                    Storage.featsAllUnitsNamesList.Clear();
                    foreach (UnitEntityData controllableCharacter in Storage.featsUnitEntityData) {
                        Storage.featsAllUnitsNamesList.Add(controllableCharacter.CharacterName);
                    }
                    Storage.featsAllUnits = Storage.featsUnitEntityData;
                    Storage.reloadPartyFeats = false;
                }
                if (Storage.featsUnitEntityData.Count - 1 < Storage.featsSelectedControllableCharacterIndex) {
                    Storage.featsSelectedControllableCharacterIndex = Storage.featsUnitEntityData.Count - 1;
                }

                if (Storage.featsUnitEntityData.Any()) {
                    if (!Storage.reloadPartyFeats) {
                        GL.Space(10);

                        GL.BeginHorizontal();
                        Storage.featsSelectedControllableCharacterIndex = GL.SelectionGrid(Storage.featsSelectedControllableCharacterIndex, Storage.featsAllUnitsNamesList.ToArray(), 6);
                        GL.EndHorizontal();

                        GL.Space(10);

                        if (GL.Button($"{settings.showAllFeats} " + Strings.GetText("button_AllFeats") + $" ({Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]})", GL.ExpandWidth(false))) {
                            if (settings.showAllFeats == Strings.GetText("misc_Hide")) {
                                settings.showAllFeats = Strings.GetText("misc_Display");
                            }
                            else if (settings.showAllFeats == Strings.GetText("misc_Display")) {
                                settings.showAllFeats = Strings.GetText("misc_Hide");
                                Storage.featAllLoad = true;
                            }
                            else {
                                settings.showAllFeats = Strings.GetText("misc_Display");
                            }
                        }
                        if (settings.showAllFeats == Strings.GetText("misc_Hide")) {
                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                            if (unitEntityData != Storage.featAllUnitEntityData) {
                                Storage.featAllUnitEntityData = unitEntityData;
                                Storage.featAllLoad = true;
                            }

                            if (Storage.featAllLoad == true) {
                                RefreshAllFeats(unitEntityData);
                                Storage.featAllLoad = false;
                            }

                            for (int i = 0; i < unitEntityData.Descriptor.Progression.Features.Count; i++) {
                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                GL.Label(Strings.GetText("label_FeatName") + $": { Storage.featAllNames[i]}");
                                GL.FlexibleSpace();
                                if (FavouritesFactory.GetFavouriteFeats.FavouritesList.Contains(unitEntityData.Descriptor.Progression.Features[i].Blueprint.AssetGuid)) {
                                    if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false))) {
                                        FavouritesFactory.GetFavouriteFeats.FavouritesList.Remove(unitEntityData.Descriptor.Progression.Features[i].Blueprint.AssetGuid);
                                        Storage.featFavouritesLoad = true;
                                    }
                                }
                                else {
                                    if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false))) {
                                        FavouritesFactory.GetFavouriteFeats.AddGuid(unitEntityData.Descriptor.Progression.Features[i].Blueprint.AssetGuid);
                                        Storage.featFavouritesLoad = true;
                                    }
                                }
                                GL.EndHorizontal();
                                MenuTools.SingleLineLabel(Strings.GetText("label_FeatObjectName") + $": {Storage.featAllObjectNames[i]}");
                                GL.BeginVertical("box");
                                MenuTools.SingleLineLabel(Strings.GetText("label_FeatRank") + $": {Storage.featAllRanks[i]}");
                                MainMenu.setRankForceTextField.RenderField();
                                GL.BeginHorizontal();
                                if (GL.Button(Strings.GetText("button_SetRankTo") + $" {MainMenu.setRankForceTextField.finalAmount}", GL.ExpandWidth(false))) {
                                    Feature feat = unitEntityData.Descriptor.Progression.Features[i] as Feature;
                                    feat.SetRankForce(MainMenu.setRankForceTextField.finalAmount);
                                    Storage.featAllLoad = true;
                                }
                                GL.EndHorizontal();
                                GL.EndVertical();
                                MenuTools.SingleLineLabel(Strings.GetText("label_FeatBlueprintAssetGuid") + $": {Storage.featAllGuids[i]}");
                                MenuTools.SingleLineLabel(Strings.GetText("label_FeatDescription") + $": {Storage.featAllDescriptions[i]}");

                                if (GL.Button(Strings.GetText("label_Remove") + $" {unitEntityData.Descriptor.Progression.Features[i].Blueprint.name}", GL.ExpandWidth(false))) {
                                    unitEntityData.Descriptor.Progression.Features.RemoveFact((BlueprintUnitFact)unitEntityData.Descriptor.Progression.Features[i].Blueprint);
                                    Storage.featAllLoad = true;
                                }
                                GL.EndVertical();
                            }
                        }

                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        settings.showFeatsFavourites = GL.Toggle(settings.showFeatsFavourites, RichTextUtils.Bold(Strings.GetText("headerOption_ShowFavourites")), GL.ExpandWidth(false));
                        GL.EndHorizontal();
                        if (settings.showFeatsFavourites == true) {

                            if (Storage.featFavouritesLoad == true) {
                                Main.RefreshFeatFavourites();
                                Storage.featFavouritesLoad = false;
                            }

                            if (!FavouritesFactory.GetFavouriteFeats.FavouritesList.Any()) {
                                MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
                            }
                            else {
                                for (int i = 0; i < FavouritesFactory.GetFavouriteFeats.FavouritesList.Count; i++) {

                                    GL.BeginHorizontal();

                                    Storage.featToggleFavouriteDescription.Add(false);
                                    Storage.featToggleFavouriteDescription[i] = GL.Toggle(Storage.featToggleFavouriteDescription[i], RichTextUtils.Bold($"{Storage.featFavouritesNames[i]}"), GL.ExpandWidth(false));

                                    if (Main.craftMagicItems.ModIsActive() && FavouritesFactory.GetFavouriteFeats.FavouritesList[i].Contains(CraftMagicItemsUtils.blueprintSuffix)) {
                                        GL.Label(StringUtils.PutInParenthesis(CraftMagicItemsUtils.blueprintSuffix), GL.ExpandWidth(false));
                                    }

                                    try {
                                        if (GL.Button(Strings.GetText("misc_Add") + $" { Storage.featFavouritesNames[i]} " + Strings.GetText("misc_To") + $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                            if (StringUtils.IsGUID(FavouritesFactory.GetFavouriteFeats.FavouritesList[i])) {
                                                UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                                BlueprintFeature blueprintFeature = Utilities.GetBlueprintByGuid<BlueprintFeature>(FavouritesFactory.GetFavouriteFeats.FavouritesList[i]);
                                                if (blueprintFeature != null) {
                                                    unitEntityData.Descriptor.AddFact((BlueprintUnitFact)blueprintFeature, (MechanicsContext)null, new FeatureParam());
                                                }
                                                Storage.featAllLoad = true;
                                            }
                                        }
                                    }
                                    catch (IndexOutOfRangeException e) {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }

                                    if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false))) {
                                        FavouritesFactory.GetFavouriteFeats.FavouritesList.Remove(FavouritesFactory.GetFavouriteFeats.FavouritesList[i]);
                                        Main.RefreshFeatFavourites();
                                        break;
                                    }
                                    GL.EndHorizontal();
                                    FeatDetails(Storage.featToggleFavouriteDescription[i], FavouritesFactory.GetFavouriteFeats.FavouritesList[i], Storage.featFavouritesDescriptions[i]);
                                }
                                GL.Space(10);

                                if (GL.Button(Strings.GetText("button_AddFavouritesTo") + $" {Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                    for (int i = 0; i < FavouritesFactory.GetFavouriteFeats.FavouritesList.Count; i++) {
                                        if (StringUtils.IsGUID(FavouritesFactory.GetFavouriteFeats.FavouritesList[i])) {
                                            BlueprintFeature blueprintFeature = Utilities.GetBlueprintByGuid<BlueprintFeature>(FavouritesFactory.GetFavouriteFeats.FavouritesList[i]);
                                            if (StringUtils.IsGUID(FavouritesFactory.GetFavouriteFeats.FavouritesList[i]) && blueprintFeature != null) {
                                                UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                                unitEntityData.Descriptor.AddFact((BlueprintUnitFact)blueprintFeature, (MechanicsContext)null, new FeatureParam());
                                                Storage.featAllLoad = true;
                                            }
                                        }
                                    }
                                }

                                MenuTools.ExportCopyGuidsNamesButtons(FavouritesFactory.GetFavouriteFeats.FavouritesList.ToArray(), Storage.featFavouritesNames.ToArray(), "feat-favourites");
                            }

                        }
                        GL.EndVertical();

                        GL.Space(10);

                        MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("label_ParametrizedFeats")));

                        GL.BeginHorizontal();
                        Storage.featsParamIndex = GL.SelectionGrid(Storage.featsParamIndex, Storage.featsParamArray, 3);
                        GL.EndHorizontal();

                        if (Storage.featsParamIndex == 1 || Storage.featsParamIndex == 3 || Storage.featsParamIndex == 4) {
                            GL.BeginHorizontal();
                            settings.featsParamShowAllWeapons = GL.Toggle(settings.featsParamShowAllWeapons, new GUIContent(RichTextUtils.Bold(Strings.GetText("label_ShowAllWeaponCategories")), Strings.GetText("tooltip_ShowAllWeaponCategories")), GL.ExpandWidth(false));
                            GL.EndHorizontal();
                        }

                        switch (Storage.featsParamIndex) {
                            case 0:
                                break;
                            case 1:
                                if (!settings.featsParamShowAllWeapons) {
                                    foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                        if (WeaponCategoryExtension.HasSubCategory(weapon, WeaponSubCategory.OneHandedPiercing)) {
                                            try {
                                                if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_FencingGrace") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                                    FeatureParam featureParam = new FeatureParam(weapon);
                                                    UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                                    unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("47b352ea0f73c354aba777945760b441"), (MechanicsContext)null, featureParam);
                                                    Storage.featAllLoad = true;
                                                }
                                            }
                                            catch (IndexOutOfRangeException e) {
                                                modLogger.Log(e.ToString());
                                                modLogger.Log("Forcing refresh");
                                                RefreshAllFeats(Storage.featAllUnitEntityData);
                                            }
                                        }
                                    }
                                }
                                else {
                                    foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                        try {
                                            if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_FencingGrace") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                                FeatureParam featureParam = new FeatureParam(weapon);
                                                UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                                unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("47b352ea0f73c354aba777945760b441"), (MechanicsContext)null, featureParam);
                                                Storage.featAllLoad = true;
                                            }
                                        }
                                        catch (IndexOutOfRangeException e) {
                                            modLogger.Log(e.ToString());
                                            modLogger.Log("Forcing refresh");
                                            RefreshAllFeats(Storage.featAllUnitEntityData);
                                        }
                                    }
                                }
                                break;
                            case 2:
                                foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                    try {
                                        if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_ImprovedCritical") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                            FeatureParam featureParam = new FeatureParam(weapon);
                                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("f4201c85a991369408740c6888362e20"), (MechanicsContext)null, featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e) {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }
                                }
                                break;
                            case 3:
                                if (!settings.featsParamShowAllWeapons) {
                                    foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                        if (WeaponCategoryExtension.HasSubCategory(weapon, WeaponSubCategory.OneHandedSlashing)) {
                                            try {
                                                if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_SlashingGrace") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                                    FeatureParam featureParam = new FeatureParam(weapon);
                                                    UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                                    unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("697d64669eb2c0543abb9c9b07998a38"), (MechanicsContext)null, featureParam);
                                                    Storage.featAllLoad = true;
                                                }
                                            }
                                            catch (IndexOutOfRangeException e) {
                                                modLogger.Log(e.ToString());
                                                modLogger.Log("Forcing refresh");
                                                RefreshAllFeats(Storage.featAllUnitEntityData);
                                            }
                                        }
                                    }
                                }
                                else {
                                    foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                        try {
                                            if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_SlashingGrace") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                                FeatureParam featureParam = new FeatureParam(weapon);
                                                UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                                unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("697d64669eb2c0543abb9c9b07998a38"), (MechanicsContext)null, featureParam);
                                                Storage.featAllLoad = true;
                                            }
                                        }
                                        catch (IndexOutOfRangeException e) {
                                            modLogger.Log(e.ToString());
                                            modLogger.Log("Forcing refresh");
                                            RefreshAllFeats(Storage.featAllUnitEntityData);
                                        }
                                    }
                                }
                                break;
                            case 4:
                                if (!settings.featsParamShowAllWeapons) {
                                    foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                        if (WeaponCategoryExtension.HasSubCategory(weapon, WeaponSubCategory.Melee)) {
                                            try {
                                                if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_SwordSaintChosenWeapon") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                                    FeatureParam featureParam = new FeatureParam(weapon);
                                                    UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                                    unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("c0b4ec0175e3ff940a45fc21f318a39a"), (MechanicsContext)null, featureParam);
                                                    Storage.featAllLoad = true;
                                                }
                                            }
                                            catch (IndexOutOfRangeException e) {
                                                modLogger.Log(e.ToString());
                                                modLogger.Log("Forcing refresh");
                                                RefreshAllFeats(Storage.featAllUnitEntityData);
                                            }
                                        }
                                    }
                                }
                                else {
                                    foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                        try {
                                            if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_SwordSaintChosenWeapon") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                                FeatureParam featureParam = new FeatureParam(weapon);
                                                UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                                unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("c0b4ec0175e3ff940a45fc21f318a39a"), (MechanicsContext)null, featureParam);
                                                Storage.featAllLoad = true;
                                            }
                                        }
                                        catch (IndexOutOfRangeException e) {
                                            modLogger.Log(e.ToString());
                                        }
                                    }
                                }
                                break;
                            case 5:
                                foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                    try {
                                        if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_WeaponFocus") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                            FeatureParam featureParam = new FeatureParam(weapon);
                                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"), (MechanicsContext)null, featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e) {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }
                                }
                                break;
                            case 6:
                                foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                    try {
                                        if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_WeaponFocusGreater") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                            FeatureParam featureParam = new FeatureParam(weapon);
                                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("09c9e82965fb4334b984a1e9df3bd088"), (MechanicsContext)null, featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e) {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }
                                }
                                break;
                            case 7:
                                foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                    try {
                                        if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_WeaponMastery") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                            FeatureParam featureParam = new FeatureParam(weapon);
                                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("38ae5ac04463a8947b7c06a6c72dd6bb"), (MechanicsContext)null, featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e) {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }
                                }
                                break;
                            case 8:
                                foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                    try {
                                        if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_WeaponSpecialization") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                            FeatureParam featureParam = new FeatureParam(weapon);
                                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("31470b17e8446ae4ea0dacd6c5817d86"), (MechanicsContext)null, featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e) {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }
                                }
                                break;
                            case 9:
                                foreach (WeaponCategory weapon in (WeaponCategory[])Enum.GetValues(typeof(WeaponCategory))) {
                                    try {
                                        if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_WeaponSpecializationGreater") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                            FeatureParam featureParam = new FeatureParam(weapon);
                                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("7cf5edc65e785a24f9cf93af987d66b3"), (MechanicsContext)null, featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e) {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }
                                }
                                break;
                            case 10:
                                foreach (SpellSchool weapon in (SpellSchool[])Enum.GetValues(typeof(SpellSchool))) {
                                    try {
                                        if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_SpellFocus") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                            FeatureParam featureParam = new FeatureParam(weapon);
                                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("16fa59cc9a72a6043b566b49184f53fe"), (MechanicsContext)null, featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e) {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }
                                }
                                break;
                            case 11:
                                foreach (SpellSchool weapon in (SpellSchool[])Enum.GetValues(typeof(SpellSchool))) {
                                    try {
                                        if (GL.Button(Strings.GetText("misc_Add") + " " + Strings.GetText("arrayItem_FeatParam_GreaterSpellFocus") + " - " + weapon.ToString() + " " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                            FeatureParam featureParam = new FeatureParam(weapon);
                                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>("5b04b45b228461c43bad768eb0f7c7bf"), (MechanicsContext)null, featureParam);
                                            Storage.featAllLoad = true;
                                        }
                                    }
                                    catch (IndexOutOfRangeException e) {
                                        modLogger.Log(e.ToString());
                                        modLogger.Log("Forcing refresh");
                                        RefreshAllFeats(Storage.featAllUnitEntityData);
                                    }
                                }
                                break;

                        }

                        GL.Space(10);

                        GL.BeginVertical("box");
                        GL.BeginHorizontal();
                        settings.featSearch = GL.TextField(settings.featSearch, GL.Width(800f));
                        if (GL.Button(RichTextUtils.Bold(Strings.GetText("header_Search")), GL.ExpandWidth(false))) {
                            SearchValidFeats(Storage.validFeatTypes);
                        }
                        GL.EndHorizontal();

                        GL.BeginHorizontal();
                        GL.Label(Strings.GetText("label_SearchBy") + ": ", GL.ExpandWidth(false));

                        if (GL.Button($"{settings.toggleSearchByFeatObjectName} " + Strings.GetText("label_ObjectName"), GL.ExpandWidth(false))) {
                            if (settings.toggleSearchByFeatObjectName == Storage.isTrueString) {
                                settings.toggleSearchByFeatObjectName = Storage.isFalseString;
                            }
                            else {
                                settings.toggleSearchByFeatObjectName = Storage.isTrueString;
                            }
                            SearchValidFeats(Storage.validFeatTypes);

                        }
                        if (GL.Button($"{settings.toggleSearchByFeatName} " + Strings.GetText("label_FeatName"), GL.ExpandWidth(false))) {
                            if (settings.toggleSearchByFeatName == Storage.isTrueString) {
                                settings.toggleSearchByFeatName = Storage.isFalseString;
                            }
                            else {
                                settings.toggleSearchByFeatName = Storage.isTrueString;
                            }
                            SearchValidFeats(Storage.validFeatTypes);
                        }
                        if (GL.Button($"{settings.toggleSearchByFeatDescription} " + Strings.GetText("label_FeatDescription"), GL.ExpandWidth(false))) {
                            if (settings.toggleSearchByFeatDescription == Storage.isTrueString) {
                                settings.toggleSearchByFeatDescription = Storage.isFalseString;
                            }
                            else {
                                settings.toggleSearchByFeatDescription = Storage.isTrueString;
                            }
                            SearchValidFeats(Storage.validFeatTypes);
                        }
                        GL.EndHorizontal();

                        MenuTools.SingleLineLabel(Strings.GetText("label_SearchInfoParametrizedFeats"));

                        if (!Storage.featResultNames.Any()) {
                            MenuTools.SingleLineLabel(Strings.GetText("message_NoResult"));
                        }
                        else {
                            if (Storage.featResultNames.Count > settings.finalResultLimit) {
                                MenuTools.SingleLineLabel($"{Storage.featResultNames.Count} " + Strings.GetText("label_Results"));
                                MenuTools.SingleLineLabel(Strings.GetText("label_TooManyResults_0") + $" { settings.finalResultLimit} " + Strings.GetText("label_TooManyResults_1"));
                                GL.Space(10);
                            }
                            else {

                                MenuTools.SingleLineLabel(RichTextUtils.Bold(Strings.GetText("label_Results") + ":"));
                                for (int i = 0; i < Storage.featResultNames.Count; i++) {

                                    GL.BeginHorizontal();

                                    Storage.featToggleResultDescription.Add(false);
                                    Storage.featToggleResultDescription[i] = GL.Toggle(Storage.featToggleResultDescription[i], RichTextUtils.Bold(Storage.featResultNames[i]), GL.ExpandWidth(false));

                                    GL.FlexibleSpace();

                                    if (Main.craftMagicItems.ModIsActive() && Storage.featResultGuids[i].Contains(CraftMagicItemsUtils.blueprintSuffix)) {
                                        GL.Label(StringUtils.PutInParenthesis(CraftMagicItemsUtils.blueprintSuffix), GL.ExpandWidth(false));
                                    }

                                    if (GL.Button(Strings.GetText("misc_Add") + $" { Storage.featResultNames[i]} " + Strings.GetText("misc_To") + $" { Storage.featsAllUnitsNamesList[Storage.featsSelectedControllableCharacterIndex]}", GL.ExpandWidth(false))) {
                                        if (StringUtils.IsGUID(Storage.featResultGuids[i])) {
                                            UnitEntityData unitEntityData = Storage.featsAllUnits[Storage.featsSelectedControllableCharacterIndex];
                                            unitEntityData.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintFeature>(Storage.featResultGuids[i]), (MechanicsContext)null, new FeatureParam());
                                            Storage.featAllLoad = true;
                                        }
                                    }

                                    if (FavouritesFactory.GetFavouriteFeats.FavouritesList.Contains(Storage.featResultGuids[i])) {
                                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false))) {
                                            FavouritesFactory.GetFavouriteFeats.FavouritesList.Remove(Storage.featResultGuids[i]);
                                            Storage.featFavouritesLoad = true;
                                        }
                                    }
                                    else {
                                        if (GL.Button(Storage.favouriteFalseString, GL.ExpandWidth(false))) {
                                            FavouritesFactory.GetFavouriteFeats.AddGuid(Storage.featResultGuids[i]);
                                            Storage.featFavouritesLoad = true;
                                        }
                                    }
                                    GL.EndHorizontal();

                                    FeatDetails(Storage.featToggleResultDescription[i], Storage.featResultGuids[i], Storage.featResultDescriptions[i]);
                                }

                                string filename = "feat-" + Regex.Replace(Storage.currentFeatSearch, @"[\\/:*?""<>|]", "");
                                MenuTools.ExportCopyGuidsNamesButtons(Storage.featResultGuids.ToArray(), Storage.featResultNames.ToArray(), filename);
                            }
                        }
                        GL.EndVertical();
                    }

                }
                else {
                    MenuTools.SingleLineLabel(Strings.GetText("message_NoUnitFound"));
                }
            }
            GL.EndVertical();
        }

        public static void FeatDetails(bool toggle, string featGuid, string description) {
            if (toggle) {
                GL.BeginVertical("box");
                if (StringUtils.IsGUID(featGuid)) {
                    BlueprintFeature featByGuid = Utilities.GetBlueprintByGuid<BlueprintFeature>(featGuid);
                    if (featByGuid != null) {
                        MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_FeatName") + ": ") + $"{featByGuid.Name}");
                        MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_FeatGUID") + ": ") + $"{featByGuid.AssetGuid}");
                        MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_ObjectName") + ": ") + $"{featByGuid.name}");
                        MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_ObjectNameClean") + ": ") + $"{Strings.CleanName(featByGuid.name)}");
                        MenuTools.SingleLineLabelCopyBlueprintDetail(RichTextUtils.Bold(Strings.GetText("label_FeatDescription") + ": ") + $"{description}");
                        MenuTools.CopyExportButtons("button_ExportFeatInfo", featByGuid.name + ".txt", FeatInfo(featByGuid, description), "label_CopyFeatInformationToClipboard");
                    }
                    else {
                        MenuTools.SingleLineLabel(featGuid + " " + Strings.GetText("error_NotFound"));
                    }
                }
                else {
                    MenuTools.SingleLineLabel(featGuid + " " + Strings.GetText("error_NotFound"));
                }
                GL.EndVertical();
            }
        }
        public static string[] FeatInfo(BlueprintFeature feat, string description) {
            return new string[]
            {
                    Strings.GetText("label_FeatName") + ": " + $"{feat.Name}",
                    Strings.GetText("label_FeatGUID") + ": " + $"{feat.AssetGuid}",
                    Strings.GetText("label_ObjectName") + ": " + $"{feat.name}",
                    Strings.GetText("label_ObjectNameClean") + ": " + $"{Strings.CleanName(feat.name)}",
                    Strings.GetText("label_FeatDescription") + ": " + $"{description}",
            };
        }

        public static void SearchValidFeats(List<string> validFeatTypesList) {
            Storage.featResultNames.Clear();
            Storage.featResultGuids.Clear();
            Storage.featResultDescriptions.Clear();

            if (settings.featSearch != "") {
                Storage.currentFeatSearch = settings.featSearch;
                Storage.blueprintList = ResourcesLibrary.LibraryObject.GetAllBlueprints();
                for (int i = 0; i < Storage.blueprintList.Count; i++) {
                    BlueprintScriptableObject bpObejct = Storage.blueprintList[i];
                    if (validFeatTypesList.Contains(bpObejct.GetType().Name)) {
                        BlueprintFeature bpFeat = bpObejct as BlueprintFeature;
                        if (bpFeat != null) {
                            string stringFeatName = bpFeat.Name;
                            string stringFeatObjectNames = bpFeat.name;
                            string stringFeatGuid = bpFeat.AssetGuid;
                            String stringFeatDescription = BlueprintScriptableObjectUtils.GetDescription(bpFeat);
                            if (stringFeatObjectNames.Contains(settings.featSearch, StringComparison.CurrentCultureIgnoreCase)) {
                                if (settings.toggleSearchByFeatObjectName == Storage.isTrueString) {
                                    if (!Storage.featResultGuids.Contains(stringFeatGuid)) {
                                        Storage.featResultGuids.Add(stringFeatGuid);

                                        if (stringFeatName != "" && stringFeatName != null) {
                                            Storage.featResultNames.Add(stringFeatName);
                                        }
                                        else {
                                            Storage.featResultNames.Add(stringFeatObjectNames);
                                        }

                                        Storage.featResultDescriptions.Add(stringFeatDescription);
                                    }
                                }
                            }
                            if (stringFeatName.Contains(settings.featSearch, StringComparison.CurrentCultureIgnoreCase)) {
                                if (settings.toggleSearchByFeatName == Storage.isTrueString) {
                                    if (!Storage.featResultGuids.Contains(stringFeatGuid)) {
                                        if (!Storage.featResultGuids.Contains(stringFeatGuid)) {
                                            Storage.featResultGuids.Add(stringFeatGuid);

                                            if (stringFeatName != "" && stringFeatName != null) {
                                                Storage.featResultNames.Add(stringFeatName);
                                            }
                                            else {
                                                Storage.featResultNames.Add(stringFeatObjectNames);
                                            }

                                            Storage.featResultDescriptions.Add(stringFeatDescription);
                                        }
                                    }
                                }
                            }
                            if (stringFeatDescription.Contains(settings.featSearch, StringComparison.CurrentCultureIgnoreCase)) {
                                if (settings.toggleSearchByFeatDescription == Storage.isTrueString) {
                                    if (!Storage.featResultGuids.Contains(stringFeatGuid)) {
                                        if (!Storage.featResultGuids.Contains(stringFeatGuid)) {
                                            Storage.featResultGuids.Add(stringFeatGuid);

                                            if (stringFeatName != "" && stringFeatName != null) {
                                                Storage.featResultNames.Add(stringFeatName);
                                            }
                                            else {
                                                Storage.featResultNames.Add(stringFeatObjectNames);
                                            }

                                            Storage.featResultDescriptions.Add(stringFeatDescription);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

        public static void RefreshAllFeats(UnitEntityData unitEntityData) {
            Storage.featAllNames.Clear();
            Storage.featAllObjectNames.Clear();
            Storage.featAllRanks.Clear();
            Storage.featAllGuids.Clear();
            Storage.featAllDescriptions.Clear();

            for (int i = 0; i < unitEntityData.Descriptor.Progression.Features.Count; i++) {
                Storage.featAllNames.Add(unitEntityData.Descriptor.Progression.Features[i].Name);
                Storage.featAllObjectNames.Add(unitEntityData.Descriptor.Progression.Features[i].Blueprint.name);
                Storage.featAllRanks.Add(unitEntityData.Descriptor.Progression.Features[i].GetRank());
                Storage.featAllGuids.Add(unitEntityData.Descriptor.Progression.Features[i].Blueprint.AssetGuid);
                Storage.featAllDescriptions.Add(BlueprintScriptableObjectUtils.GetDescription(unitEntityData.Descriptor.Progression.Features[i].Blueprint));
            }
        }
    }
}
