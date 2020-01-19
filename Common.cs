using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Cheats;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.RuleSystem;
using Kingmaker.UI;
using Kingmaker.UI.Log;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using UnityEngine;

using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    public static class Common
    {
        public static Settings settings = Main.settings;

        public static UnitEntityData PlayerCharacter => Game.Instance.Player.MainCharacter;

        public static void SerializeListString(this List<string> list, string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<string>));
            using (var stream = File.Create(filePath))
            {
                serializer.Serialize(stream, list);
            }
        }

        public static void DeserializeListString(this List<string> list, string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(List<string>));
                using (var stream = File.OpenRead(filePath))
                {
                    var other = (List<string>) serializer.Deserialize(stream);
                    list.Clear();
                    list.AddRange(other);
                }
            }
            catch (Exception e)
            {
                Main.modLogger.Log("\n" + filePath + "\n" + e.ToString());
            }
        }

        public static void AddLogEntry(string message, Color color, bool gameDefaultColour = true)
        {
            var currentGameMode = Game.Instance.CurrentMode;
            if (Main.settings.toggleAddToLog == Storage.isTrueString &&
                (currentGameMode == GameModeType.Default || currentGameMode == GameModeType.Pause))
            {
                if (gameDefaultColour) color = GameLogStrings.Instance.DefaultColor;

                var logItemData = new LogDataManager.LogItemData(message, GameLogStrings.Instance.DefaultColor, null,
                    PrefixIcon.None);
                Game.Instance.UI.BattleLogManager.LogView.AddLogEntry(logItemData);
            }
        }

        public static bool IsEnemyOfPlayerGroup(UnitEntityData unit)
        {
            return unit.Group.IsEnemy(Game.Instance.Player.Group);
        }

        public static UnitEntityData GetUnitUnderMouse()
        {
            var hoverUnit = Game.Instance?.UI?.SelectionManager?.HoverUnit;
            if ((UnityEngine.Object) hoverUnit != (UnityEngine.Object) null) return hoverUnit.EntityData;

            var camera = Game.GetCamera();
            if (camera == null) return (UnitEntityData) null;

            foreach (var raycastHit in Physics.RaycastAll(camera.ScreenPointToRay(Input.mousePosition),
                camera.farClipPlane, 21761))
            {
                var gameObject = raycastHit.collider.gameObject;
                if (gameObject.CompareTag("SecondarySelection"))
                    while (!(bool) (UnityEngine.Object) gameObject.GetComponent<UnitEntityView>() &&
                           (bool) (UnityEngine.Object) gameObject.transform.parent)
                        gameObject = gameObject.transform.parent.gameObject;
                var component = gameObject.GetComponent<UnitEntityView>();
                if ((bool) (UnityEngine.Object) component)
                    return component.EntityData;
            }

            return (UnitEntityData) null;
        }

        public static void Kill(UnitEntityData unit)
        {
            unit.Descriptor.Damage = unit.Descriptor.Stats.HitPoints.ModifiedValue +
                                     unit.Descriptor.Stats.TemporaryHitPoints.ModifiedValue;
        }

        public static void ForceKill(UnitEntityData unit)
        {
            unit.Descriptor.State.ForceKill = true;
        }

        public static void ResurrectAndFullRestore(UnitEntityData unit)
        {
            unit.Descriptor.ResurrectAndFullRestore();
        }

        public static void Buff(UnitEntityData unit, string buffGuid)
        {
            unit.Descriptor.AddFact((BlueprintUnitFact)Utilities.GetBlueprintByGuid<BlueprintBuff>(buffGuid), (MechanicsContext)null, new FeatureParam());
        }

        public static void Charm(UnitEntityData unit)
        {
            unit.Descriptor.Master = Game.Instance.Player.MainCharacter.Value;
            unit.Descriptor.SetMaster(Game.Instance.Player.MainCharacter.Value);

            unit.Descriptor.SwitchFactions(Game.Instance.BlueprintRoot.PlayerFaction, true);
        }

        public static void AddToParty(UnitEntityData unit)
        {
            Charm(unit);
            Game.Instance.Player.AddCompanion(unit);
        }

        public static void PostPartyChange()
        {
            foreach (var unit in Game.Instance.Player.PartyCharacters)
            {
                unit.Value.IsInGame = true;
                unit.Value.Position = GameHelper.GetPlayerCharacter().Position;
            }
        }

        public static void TeleportPartyToPlayer()
        {
            var currentMode = Game.Instance.CurrentMode;
            var partyMembers = Game.Instance.Player?.ControllableCharacters;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
                foreach (var unit in partyMembers)
                    if (unit != Game.Instance.Player.MainCharacter.Value)
                    {
                        unit.Commands.InterruptMove();
                        unit.Commands.InterruptMove();
                        unit.Position = Game.Instance.Player.MainCharacter.Value.Position;
                    }
        }

        public static void TeleportEveryoneToPlayer()
        {
            var currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
                foreach (var unit in Game.Instance.State.Units)
                    if (unit != Game.Instance.Player.MainCharacter.Value)
                    {
                        unit.Commands.InterruptMove();
                        unit.Commands.InterruptMove();
                        unit.Position = Game.Instance.Player.MainCharacter.Value.Position;
                    }
        }

        public static void TeleportUnit(UnitEntityData unit, Vector3 position)
        {
            var currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                unit.Commands.InterruptMove();
                unit.Commands.InterruptMove();
                unit.Position = position;
            }
        }

        public static Vector3 MousePositionLocalMap()
        {
            var currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                var camera = Game.GetCamera();
                var hit = default(RaycastHit);
                if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, camera.farClipPlane, 21761))
                    return hit.point;
                return new Vector3();
            }

            return new Vector3();
        }

        public static void SpawnPassiveUnit(Vector3 position, string guid)
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                UnitEntityData player = Game.Instance.Player.MainCharacter.Value;
                UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit((BlueprintUnit)Utilities.GetBlueprintByGuid<BlueprintUnit>(guid), position, Quaternion.LookRotation(player.OrientationDirection), Game.Instance.CurrentScene.MainState);                
                unit.Descriptor.SwitchFactions(Utilities.GetBlueprintByGuid<BlueprintFaction>("d8de50cc80eb4dc409a983991e0b77ad"), true); // d8de50cc80eb4dc409a983991e0b77ad Neutrals
                unit.AttackFactions.Clear();
            }
        }
        public static void SpawnFriendlyUnit(Vector3 position, string guid)
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                UnitEntityData player = Game.Instance.Player.MainCharacter.Value;
                UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit((BlueprintUnit)Utilities.GetBlueprintByGuid<BlueprintUnit>(guid), position, Quaternion.LookRotation(player.OrientationDirection), Game.Instance.CurrentScene.MainState);
                unit.Descriptor.SwitchFactions(Game.Instance.BlueprintRoot.PlayerFaction, true);
            }
        }
        public static void SpawnHostileUnit(Vector3 position, string guid)
        {
            var currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                var player = Game.Instance.Player;
                var target = player.Party.Random<UnitEntityData>();
                var executor = Game.Instance.EntityCreator.SpawnUnit(
                    (BlueprintUnit) Utilities.GetBlueprintByGuid<BlueprintUnit>(guid), position,
                    Quaternion.LookRotation(target.OrientationDirection), Game.Instance.CurrentScene.MainState);
                if (!executor.AttackFactions.Contains(Game.Instance.BlueprintRoot.PlayerFaction))
                    executor.AttackFactions.Add(Game.Instance.BlueprintRoot.PlayerFaction);
                executor.Commands.Run(UnitAttack.CreateAttackCommand(executor, target));
            }
        }

        public static void SpawnHostileUnit(Vector3 position, BlueprintUnit unit)
        {
            var currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                var player = Game.Instance.Player;
                var target = player.Party.Random<UnitEntityData>();
                var executor = Game.Instance.EntityCreator.SpawnUnit(unit, position,
                    Quaternion.LookRotation(target.OrientationDirection), Game.Instance.CurrentScene.MainState);
                if (!executor.AttackFactions.Contains(Game.Instance.BlueprintRoot.PlayerFaction))
                    executor.AttackFactions.Add(Game.Instance.BlueprintRoot.PlayerFaction);
                executor.Commands.Run(UnitAttack.CreateAttackCommand(executor, target));
            }
        }

        public static void RotateUnit(UnitEntityData unit, Vector3 position)
        {
            var currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                unit.LookAt(position);
                unit.ForceLookAt(position);
            }
        }

        public static List<UnitEntityData> GetCustomCompanions()
        {
            var unitEntityData = Game.Instance.Player.AllCharacters;
            var unitEntityDataNew = new List<UnitEntityData>();

            foreach (var unit in unitEntityData)
                if (unit.IsCustomCompanion())
                    unitEntityDataNew.Add(unit);
            return unitEntityDataNew;
        }

        public static List<UnitEntityData> GetPets()
        {
            var unitEntityData = Game.Instance.Player.AllCharacters;
            var unitEntityDataNew = new List<UnitEntityData>();
            foreach (var unit in unitEntityData)
                if (unit.Descriptor.IsPet)
                    unitEntityDataNew.Add(unit);
            return unitEntityDataNew;
        }

        public static List<UnitEntityData> GetEnemies()
        {
            var enemyUnits = new List<UnitEntityData>();
            using (var units = Game.Instance.State.Units.GetEnumerator())
            {
                while (units.MoveNext())
                {
                    UnitEntityData unit;
                    if ((unit = units.Current) != null && !unit.IsPlayerFaction && unit.IsInGame && unit.IsRevealed &&
                        !unit.Descriptor.State.IsFinallyDead &&
                        unit.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot.PlayerFaction))
                        enemyUnits.Add(unit);
                }
            }

            return enemyUnits;
        }

        public static bool CheckUnitEntityData(UnitEntityData unitEntityData, int selection)
        {
            switch (selection)
            {
                case 0:
                    return true;
                case 1:
                    if (unitEntityData.IsPlayerFaction) return true;
                    return false;
                case 2:
                    if (unitEntityData.IsMainCharacter) return true;
                    return false;
                case 3:
                    if (!unitEntityData.IsPlayerFaction &&
                        unitEntityData.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot.PlayerFaction))
                        return true;
                    return false;
            }

            return false;
        }

        public static string SizeToString(Size size)
        {
            var sizeString = "!";
            switch (size)
            {
                case Size.Colossal:
                    sizeString = Strings.GetText("arrayItem_Size_Colossal");
                    break;
                case Size.Diminutive:
                    sizeString = Strings.GetText("arrayItem_Size_Diminutive");
                    break;
                case Size.Fine:
                    sizeString = Strings.GetText("arrayItem_Size_Fine");
                    break;
                case Size.Gargantuan:
                    sizeString = Strings.GetText("arrayItem_Size_Gargantuan");
                    break;
                case Size.Huge:
                    sizeString = Strings.GetText("arrayItem_Size_Huge");
                    break;
                case Size.Large:
                    sizeString = Strings.GetText("arrayItem_Size_Large");
                    break;
                case Size.Medium:
                    sizeString = Strings.GetText("arrayItem_Size_Medium");
                    break;
                case Size.Small:
                    sizeString = Strings.GetText("arrayItem_Size_Small");
                    break;
                case Size.Tiny:
                    sizeString = Strings.GetText("arrayItem_Size_Tiny");
                    break;
            }

            return sizeString;
        }

        public static string AlignmentToString(Alignment alignment)
        {
            var alignmentString = "!";
            switch (alignment)
            {
                case Alignment.ChaoticEvil:
                    alignmentString = Strings.GetText("arrayItem_Alignments_ChaoticEvil");
                    break;
                case Alignment.ChaoticGood:
                    alignmentString = Strings.GetText("arrayItem_Alignments_ChaoticGood");
                    break;
                case Alignment.ChaoticNeutral:
                    alignmentString = Strings.GetText("arrayItem_Alignments_ChaoticNeutral");
                    break;
                case Alignment.LawfulEvil:
                    alignmentString = Strings.GetText("arrayItem_Alignments_LawfulEvil");
                    break;
                case Alignment.LawfulGood:
                    alignmentString = Strings.GetText("arrayItem_Alignments_LawfulGood");
                    break;
                case Alignment.LawfulNeutral:
                    alignmentString = Strings.GetText("arrayItem_Alignments_LawfulNeutral");
                    break;
                case Alignment.NeutralEvil:
                    alignmentString = Strings.GetText("arrayItem_Alignments_NeutralEvil");
                    break;
                case Alignment.NeutralGood:
                    alignmentString = Strings.GetText("arrayItem_Alignments_NeutralGood");
                    break;
                case Alignment.TrueNeutral:
                    alignmentString = Strings.GetText("arrayItem_Alignments_TrueNeutral");
                    break;
            }

            return alignmentString;
        }

        public static int IndexToAligment(int index)
        {
            var alignment = 1;
            switch (index)
            {
                case 0:
                    alignment = 10; //LawfulGood
                    break;
                case 1:
                    alignment = 3; //NeutralGood
                    break;
                case 2:
                    alignment = 18; //ChaoticGood
                    break;
                case 3:
                    alignment = 9; //LawfulNeutral
                    break;
                case 4:
                    alignment = 1; //TrueNeutral
                    break;
                case 5:
                    alignment = 17; //ChaoticNeutral
                    break;
                case 6:
                    alignment = 12; //LawfulEvil
                    break;
                case 7:
                    alignment = 5; //NeutralEvil
                    break;
                case 8:
                    alignment = 20; //ChaoticEvil
                    break;
            }

            return alignment;
        }

        public static Encumbrance IntToEncumbrance(int index)
        {
            var encumbrance = Encumbrance.Light;
            switch (index)
            {
                case 0:
                    encumbrance = Encumbrance.Light;
                    break;
                case 1:
                    encumbrance = Encumbrance.Medium;
                    break;
                case 2:
                    encumbrance = Encumbrance.Heavy;
                    break;
                case 3:
                    encumbrance = Encumbrance.Overload;
                    break;
            }

            return encumbrance;
        }

        public static DiceType IntToDiceType(int index)
        {
            var diceType = DiceType.Zero;
            switch (index)
            {
                case 0:
                    diceType = DiceType.Zero;
                    break;
                case 1:
                    diceType = DiceType.One;
                    break;
                case 2:
                    diceType = DiceType.D2;
                    break;
                case 3:
                    diceType = DiceType.D3;
                    break;
                case 4:
                    diceType = DiceType.D4;
                    break;
                case 5:
                    diceType = DiceType.D6;
                    break;
                case 6:
                    diceType = DiceType.D8;
                    break;
                case 7:
                    diceType = DiceType.D10;
                    break;
                case 8:
                    diceType = DiceType.D12;
                    break;
                case 9:
                    diceType = DiceType.D20;
                    break;
                case 10:
                    diceType = DiceType.D100;
                    break;
            }

            return diceType;
        }

        public static string GenderToSymbol(Gender? gender)
        {
            if (gender == Gender.Female)
                return "♀";
            else if (gender == Gender.Male)
                return "♂";
            else
                return "!";
        }

        public static void CloseMessageBox(DialogMessageBox.BoxButton btn)
        {
        }

        public static void MoveArrayElementUp<T>(ref T[] array, T element)
        {
            var index = Array.IndexOf(array, element);

            if (index < array.Length - 1)
            {
                var swappedElement = array[index + 1];

                array[index + 1] = element;
                array[index] = swappedElement;
            }
        }

        public static void MakeArrayElementLast<T>(ref T[] array, T element)
        {
            var index = Array.IndexOf(array, element);

            if (index < array.Length - 1)
            {
                var rotations = array.Length - 1 - index;
                array.Rotate(rotations);
            }
        }

        public static void MoveArrayElementDown<T>(ref T[] array, T element)
        {
            var index = Array.IndexOf(array, element);

            if (index > 0)
            {
                var swappedElement = array[index - 1];

                array[index - 1] = element;
                array[index] = swappedElement;
            }
        }

        public static void MakeArrayElementFirst<T>(ref T[] array, T element)
        {
            var index = Array.IndexOf(array, element);

            if (index > 0) array.Rotate(-index);
        }

        public static void Rotate<T>(this T[] array, int count)
        {
            if (array == null || array.Length < 2) return;

            count %= array.Length;
            if (count == 0) return;
            var left = count < 0 ? -count : array.Length + count;
            var right = count > 0 ? count : array.Length - count;
            if (left <= right)
                for (var i = 0; i < left; i++)
                {
                    var temp = array[0];
                    Array.Copy(array, 1, array, 0, array.Length - 1);
                    array[array.Length - 1] = temp;
                }
            else
                for (var i = 0; i < right; i++)
                {
                    var temp = array[array.Length - 1];
                    Array.Copy(array, 0, array, 1, array.Length - 1);
                    array[0] = temp;
                }
        }


        public static void MoveListElementUp<T>(ref List<T> list, T element)
        {
            var index = list.IndexOf(element);

            if (index < list.Count - 1)
            {
                var swappedElement = list[index - 1];

                list[index + 1] = element;
                list[index] = swappedElement;
            }
        }

        public static void MakeListElementLast<T>(ref List<T> list, T element)
        {
            var index = list.IndexOf(element);

            if (index < list.Count - 1)
            {
                list.Remove(element);
                list.Add(element);
            }
        }

        public static void MoveListElementDown<T>(ref List<T> list, T element)
        {
            var index = list.IndexOf(element);

            if (index > 0)
            {
                var swappedElement = list[index - 1];

                list[index - 1] = element;
                list[index] = swappedElement;
            }
        }

        public static void MakeListElementFirst<T>(ref List<T> list, T element)
        {
            var index = list.IndexOf(element);

            if (index > 0)
            {
                list.Remove(element);
                list.Insert(0, element);
            }
        }

        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.RandomElementUsing<T>(new System.Random());
        }

        public static T RandomElementUsing<T>(this IEnumerable<T> enumerable, System.Random rand)
        {
            var index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        private static Canvas[] hiddenCanvases;

        public static void ToggleHud()
        {
            Storage.hudHidden = !Storage.hudHidden;
            if (Storage.hudHidden) hiddenCanvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
            foreach (Component hiddenCanvas in hiddenCanvases) hiddenCanvas.gameObject.SetActive(!Storage.hudHidden);
        }

        public static T CastObject<T>(object input)
        {
            return (T) input;
        }

        public static bool DlcCheck(DlcType dlc)
        {
            var enabled = BlueprintRoot.Instance.DlcSettings.Get(dlc)?.Enabled;
            if (enabled.HasValue)
                return enabled.Value;
            return false;
        }

        public static bool DlcTieflings()
        {
            return DlcCheck(DlcType.Tieflings);
        }

        public static bool DlcVarnhold()
        {
            return DlcCheck(DlcType.Varnhold);
        }

        public static bool DlcEndless()
        {
            return DlcCheck(DlcType.Endless);
        }

        public static string ExportPath()
        {
            if (Strings.ToBool(settings.toggleExportToModFolder))
                return Storage.modEntryPath + Storage.exportFolder;
            else
                return Application.persistentDataPath;
        }

        public static int GetEncounterCr()
        {
            var blueprint =
                Utilities.GetBlueprint<BlueprintStatProgression>(
                    "Assets/Mechanics/Blueprints/Classes/Basic/CRTable.asset");
            if (!(bool) (UnityEngine.Object) blueprint)
                blueprint = Utilities.GetBlueprint<BlueprintStatProgression>("19b09eaa18b203645b6f1d5f2edcb1e4");
            if ((bool) (UnityEngine.Object) blueprint)
                return Utilities.GetTotalChallengeRating(Game.Instance.State.Units.Where<UnitEntityData>(
                        (Func<UnitEntityData, bool>) (u =>
                        {
                            if (u.IsInCombat)
                                return !u.IsPlayerFaction;
                            return false;
                        })).Select<UnitEntityData, BlueprintUnit>(
                        (Func<UnitEntityData, BlueprintUnit>) (u => u.Blueprint))
                    .ToList<BlueprintUnit>());
            UberDebug.LogChannel("SmartConsole",
                string.Format("CR table not found at {0} or {1}, cannot calculate",
                    (object) "Assets/Mechanics/Blueprints/Classes/Basic/CRTable.asset",
                    (object) "19b09eaa18b203645b6f1d5f2edcb1e4"), (object[]) Array.Empty<object>());
            return -1;
        }

        public static string GetDifficulty()
        {
            var num = GetEncounterCr() - Game.Instance.Player.PartyLevel;
            if (num < 3)
                return "Easy";
            return num < 5 ? "Hard" : "Boss";
        }

        public static void ModLoggerDebug(string message)
        {
            if(settings.settingShowDebugInfo)
            {
                Main.modLogger.Log(message);
            }
        }
        public static void ModLoggerDebug(int message)
        {
            if (settings.settingShowDebugInfo)
            {
                Main.modLogger.Log(message.ToString());
            }
        }
        public static void ModLoggerDebug(bool message)
        {
            if (settings.settingShowDebugInfo)
            {
                Main.modLogger.Log(message.ToString());
            }
        }

        public static void RefreshArrayFromFile(ref string [] array, string folder, string fileFormat)
        {
            try
            {
                array = Directory.GetFiles(Storage.modEntryPath + folder, $"*.{fileFormat}");
                Array.Sort<string>(array);
            }
            catch (IOException exception)
            {
                Main.modLogger.Log(exception.ToString());
            }
        }

        public static void RecalculateArmourItemStats(UnitEntityData unitEntityData)
        {
            ModLoggerDebug(unitEntityData.CharacterName);
            if (unitEntityData.Body.Armor.HasArmor)
            {
                ModLoggerDebug(unitEntityData.Body.Armor.Armor.Name);
                unitEntityData.Body.Armor.Armor.RecalculateStats();
            }
            if (unitEntityData.Body.PrimaryHand.HasShield)
            {
                ModLoggerDebug(unitEntityData.Body.PrimaryHand.Shield.ArmorComponent.Name);
                unitEntityData.Body.PrimaryHand.Shield.ArmorComponent.RecalculateStats();
            }
            if (unitEntityData.Body.SecondaryHand.HasShield)
            {
                ModLoggerDebug(unitEntityData.Body.SecondaryHand.Shield.ArmorComponent.Name);
                unitEntityData.Body.SecondaryHand.Shield.ArmorComponent.RecalculateStats();
            }
        }

        public static string RemoveHtmlTags(string s)
        {
            return Regex.Replace(s, "<.*?>", String.Empty);
        }

        public static string UnityRichTextToHtml(string s)
        {
            s = s.Replace("<color=", "<font color=");
            s = s.Replace("</color>", "</font>");
            s = s.Replace("<size=", "<size size=");
            s = s.Replace("</size>", "</font>");
            s += "<br/>";

            return s;
        }
    }

    public class ModEntryCheck
    {
        private UnityModManager.ModEntry _modEntry;


        public ModEntryCheck(string modId)
        {
            _modEntry = UnityModManager.FindMod(modId);
        }

        public bool ModIsActive()
        {
            if (_modEntry != null && _modEntry.Assembly != null)
                return _modEntry.Active;
            else
                return false;
        }

        public bool IsInstalled()
        {
            if (_modEntry != null)
                return true;
            else
                return false;
        }

        public string Version()
        {
            if (_modEntry != null)
                return _modEntry.Info.Version;
            else
                return "";
        }
    }

    internal class AutoCompleteDictionary<T> : SortedDictionary<string, T>
    {
        private AutoCompleteComparer _mComparer;

        public AutoCompleteDictionary()
            : base((IComparer<string>) new AutoCompleteComparer())
        {
            _mComparer = Comparer as AutoCompleteComparer;
        }

        public T LowerBound(string lookupString)
        {
            _mComparer.Reset();
            ContainsKey(lookupString);
            return this[_mComparer.LowerBound];
        }

        public T UpperBound(string lookupString)
        {
            _mComparer.Reset();
            ContainsKey(lookupString);
            return this[_mComparer.UpperBound];
        }

        public T AutoCompleteLookup(string lookupString)
        {
            _mComparer.Reset();
            ContainsKey(lookupString);
            return this[_mComparer.UpperBound != null ? _mComparer.UpperBound : _mComparer.LowerBound];
        }

        private class AutoCompleteComparer : IComparer<string>
        {
            private string _mLowerBound;
            private string _mUpperBound;

            public string LowerBound => _mLowerBound;

            public string UpperBound => _mUpperBound;

            public int Compare(string x, string y)
            {
                var num = Comparer<string>.Default.Compare(x,y);
                if (num >= 0)
                    _mLowerBound = y;
                if (num <= 0)
                    _mUpperBound = y;
                return num;
            }

            public void Reset()
            {
                _mLowerBound = (string)null;
                _mUpperBound = (string)null;
            }
        }
    }
}