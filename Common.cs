using BagOfTricks.Utils;
using Harmony;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Cheats;
using Kingmaker.Designers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.RuleSystem;
using Kingmaker.UI;
using Kingmaker.UI.Log;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    public static class Common
    {
        public static Settings settings = Main.settings;

        public static UnitEntityData PlayerCharacter
        {
            get { return Game.Instance.Player.MainCharacter; }
        }

        public static void AddLogEntry(string message, Color color, bool gameDefaultColour = true)
        {
            GameModeType currentGameMode = Game.Instance.CurrentMode;
            if (Main.settings.toggleAddToLog == Storage.isTrueString && (currentGameMode == GameModeType.Default || currentGameMode == GameModeType.Pause))
            {
                if (gameDefaultColour)
                {
                    color = GameLogStrings.Instance.DefaultColor;
                }

                LogItemData logItemData = new LogItemData(message, GameLogStrings.Instance.DefaultColor, null, PrefixIcon.None, new List<LogChannel> { LogChannel.None });
                Game.Instance.UI.BattleLogManager.LogView.AddLogEntry(logItemData);
            }

        }

        public static bool IsEnemyOfPlayerGroup(UnitEntityData unit)
        {
            return unit.Group.IsEnemy(Game.Instance.Player.Group);
        }

        public static UnitEntityData GetUnitUnderMouse()
        {
            UnitEntityView hoverUnit = Game.Instance?.UI?.SelectionManagerPC?.HoverUnit;
            if (hoverUnit != null)
            {
                return hoverUnit.EntityData;
            }

            Camera camera = Game.GetCamera();
            if (camera == null)
            {
                return (UnitEntityData)null;
            }

            foreach (RaycastHit raycastHit in Physics.RaycastAll(camera.ScreenPointToRay(Input.mousePosition), camera.farClipPlane, 21761))
            {
                GameObject gameObject = raycastHit.collider.gameObject;
                if (gameObject.CompareTag("SecondarySelection"))
                {
                    while (!(bool)((UnityEngine.Object)gameObject.GetComponent<UnitEntityView>()) && (bool)((UnityEngine.Object)gameObject.transform.parent))
                        gameObject = gameObject.transform.parent.gameObject;
                }
                UnitEntityView component = gameObject.GetComponent<UnitEntityView>();
                if ((bool)((UnityEngine.Object)component))
                    return component.EntityData;
            }
            return (UnitEntityData)null;
        }

        public static void PostPartyChange()
        {
            foreach (UnitReference unit in Game.Instance.Player.PartyCharacters)
            {
                unit.Value.IsInGame = true;
                unit.Value.Position = GameHelper.GetPlayerCharacter().Position;
            }
        }

        public static void TeleportPartyToPlayer()
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            List<UnitEntityData> partyMembers = Game.Instance.Player.ControllableCharacters;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                foreach (UnitEntityData unit in partyMembers)
                {
                    if (unit != Game.Instance.Player.MainCharacter.Value)
                    {
                        unit.Commands.InterruptMove();
                        unit.Commands.InterruptMove();
                        unit.Position = Game.Instance.Player.MainCharacter.Value.Position;

                    }

                }

            }
        }
        public static void TeleportEveryoneToPlayer()
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                foreach (UnitEntityData unit in Game.Instance.State.Units)
                {
                    if (unit != Game.Instance.Player.MainCharacter.Value)
                    {
                        unit.Commands.InterruptMove();
                        unit.Commands.InterruptMove();
                        unit.Position = Game.Instance.Player.MainCharacter.Value.Position;

                    }

                }

            }
        }
        public static void TeleportUnit(UnitEntityData unit, Vector3 position)
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                unit.Commands.InterruptMove();
                unit.Commands.InterruptMove();
                unit.Position = position;
            }
        }

        public static Vector3 MousePositionLocalMap()
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                Camera camera = Game.GetCamera();
                RaycastHit hit = default(RaycastHit);
                if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, camera.farClipPlane, 21761))
                {
                    return hit.point;
                }
                return new Vector3();
            }
            return new Vector3();
        }

        public static void SpawnPassiveUnit(Vector3 position, string guid)
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                BlueprintUnit blueprintUnit = Utilities.GetBlueprintByGuid<BlueprintUnit>(guid);
                if (blueprintUnit != null) {
                    UnitEntityData player = Game.Instance.Player.MainCharacter.Value;
                    UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit((BlueprintUnit)Utilities.GetBlueprintByGuid<BlueprintUnit>(guid), position, Quaternion.LookRotation(player.OrientationDirection), Game.Instance.CurrentScene.MainState);
                    unit.Descriptor.SwitchFactions(Utilities.GetBlueprintByGuid<BlueprintFaction>("d8de50cc80eb4dc409a983991e0b77ad"), true); // d8de50cc80eb4dc409a983991e0b77ad Neutrals
                    unit.AttackFactions.Clear();
                }
            }
        }

        public static void SpawnFriendlyUnit(Vector3 position, string guid)
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                BlueprintUnit blueprintUnit = Utilities.GetBlueprintByGuid<BlueprintUnit>(guid);
                if (blueprintUnit != null) {
                    UnitEntityData player = Game.Instance.Player.MainCharacter.Value;
                    UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit((BlueprintUnit)Utilities.GetBlueprintByGuid<BlueprintUnit>(guid), position, Quaternion.LookRotation(player.OrientationDirection), Game.Instance.CurrentScene.MainState);
                    unit.Descriptor.SwitchFactions(Game.Instance.BlueprintRoot.PlayerFaction, true);
                }
            }
        }

        public static void SpawnHostileUnit(Vector3 position, string guid)
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                BlueprintUnit blueprintUnit = Utilities.GetBlueprintByGuid<BlueprintUnit>(guid);
                if (blueprintUnit != null) {
                    Player player = Game.Instance.Player;
                    UnitEntityData target = player.Party.Random();
                    UnitEntityData executor = Game.Instance.EntityCreator.SpawnUnit((BlueprintUnit)Utilities.GetBlueprintByGuid<BlueprintUnit>(guid), position, Quaternion.LookRotation(target.OrientationDirection), Game.Instance.CurrentScene.MainState);
                    if (!executor.AttackFactions.Contains(Game.Instance.BlueprintRoot.PlayerFaction))
                    {
                        executor.AttackFactions.Add(Game.Instance.BlueprintRoot.PlayerFaction);
                    }
                    executor.Commands.Run(UnitAttack.CreateAttackCommand(executor, target));
                }
            }
        }

        public static void SpawnHostileUnit(Vector3 position, BlueprintUnit unit)
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                if (unit != null) {
                    Player player = Game.Instance.Player;
                    UnitEntityData target = player.Party.Random();
                    UnitEntityData executor = Game.Instance.EntityCreator.SpawnUnit(unit, position, Quaternion.LookRotation(target.OrientationDirection), Game.Instance.CurrentScene.MainState);
                    if (!executor.AttackFactions.Contains(Game.Instance.BlueprintRoot.PlayerFaction))
                    {
                        executor.AttackFactions.Add(Game.Instance.BlueprintRoot.PlayerFaction);
                    }
                    executor.Commands.Run(UnitAttack.CreateAttackCommand(executor, target));
                }
            }
        }

        public static void RotateUnit(UnitEntityData unit, Vector3 position)
        {
            GameModeType currentMode = Game.Instance.CurrentMode;
            if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause)
            {
                unit.LookAt(position);
                unit.ForceLookAt(position);
            }
        }

        public static List<UnitEntityData> GetEnemies()
        {
            List<UnitEntityData> enemyUnits = new List<UnitEntityData>();
            using (EntityPoolEnumerator<UnitEntityData> units = Game.Instance.State.Units.GetEnumerator())
            {
                while (units.MoveNext())
                {
                    UnitEntityData unit;
                    if ((unit = units.Current) != null && !unit.IsPlayerFaction && (unit.IsInGame && unit.IsRevealed) && (!unit.Descriptor.State.IsFinallyDead && unit.Descriptor.AttackFactions.Contains(Game.Instance.BlueprintRoot.PlayerFaction)))
                    {
                        enemyUnits.Add(unit);
                    }
                }

            }
            return enemyUnits;
        }


        public static string SizeToString(Size size)
        {
            string sizeString = "!";
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
            string alignmentString = "!";
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
            int alignment = 1;
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
            Encumbrance encumbrance = Encumbrance.Light;
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
            DiceType diceType = DiceType.Zero;
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
            {
                return "♀";
            }
            else if (gender == Gender.Male)
            {
                return "♂";
            }
            else
            {
                return "!";
            }
        }

        public static void CloseMessageBox(DialogMessageBoxBase.BoxButton btn)
        {
        }

        public static void MoveArrayElementUp<T>(ref T[] array, T element)
        {
            int index = Array.IndexOf(array, element);

            if (index < array.Length - 1)
            {
                T swappedElement = array[index + 1];

                array[index + 1] = element;
                array[index] = swappedElement;
            }
        }
        public static void MakeArrayElementLast<T>(ref T[] array, T element)
        {
            int index = Array.IndexOf(array, element);

            if (index < array.Length - 1)
            {
                int rotations = array.Length - 1 - index;
                array.Rotate(rotations);
            }
        }
        public static void MoveArrayElementDown<T>(ref T[] array, T element)
        {
            int index = Array.IndexOf(array, element);

            if (index > 0)
            {
                T swappedElement = array[index - 1];

                array[index - 1] = element;
                array[index] = swappedElement;
            }
        }
        public static void MakeArrayElementFirst<T>(ref T[] array, T element)
        {
            int index = Array.IndexOf(array, element);

            if (index > 0)
            {
                array.Rotate(-index);
            }
        }
        public static void Rotate<T>(this T[] array, int count)
        {
            if (array == null || array.Length < 2)
            {
                return;
            }

            count %= array.Length;
            if (count == 0)
            {
                return;
            }
            int left = count < 0 ? -count : array.Length + count;
            int right = count > 0 ? count : array.Length - count;
            if (left <= right)
            {
                for (int i = 0; i < left; i++)
                {
                    var temp = array[0];
                    Array.Copy(array, 1, array, 0, array.Length - 1);
                    array[array.Length - 1] = temp;
                }
            }
            else
            {
                for (int i = 0; i < right; i++)
                {
                    var temp = array[array.Length - 1];
                    Array.Copy(array, 0, array, 1, array.Length - 1);
                    array[0] = temp;
                }
            }
        }

        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.RandomElementUsing<T>(new System.Random());
        }

        public static T RandomElementUsing<T>(this IEnumerable<T> enumerable, System.Random rand)
        {
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }


        private static Canvas[] hiddenCanvases;
        public static void ToggleHUD()
        {
            Canvas[] temp = UnityEngine.Object.FindObjectsOfType<Canvas>();
            Storage.hudHidden = !Storage.hudHidden;
            if (Storage.hudHidden && temp != null && temp.Length > 0)
            {
                hiddenCanvases = temp;
            }
            if (hiddenCanvases != null && hiddenCanvases.Length > 0)
            {
                foreach (Component hiddenCanvas in hiddenCanvases)
                {
                    if (hiddenCanvas != null)
                    {
                        hiddenCanvas?.gameObject?.SetActive(!Storage.hudHidden);
                    }
                }
            }
        }

        public static T CastObject<T>(object input)
        {
            return (T)input;
        }

        public static bool DLCCheck(DlcType dlc)
        {
            bool? enabled = BlueprintRoot.Instance.DlcSettings.Get(dlc)?.Enabled;
            if (enabled.HasValue)
                return enabled.Value;
            return false;
        }
        public static bool DLCTieflings()
        {
            return DLCCheck(DlcType.Tieflings);
        }
        public static bool DLCVarnhold()
        {
            return DLCCheck(DlcType.Varnhold);
        }
        public static bool DLCEndless()
        {
            return DLCCheck(DlcType.Endless);
        }

        public static string ExportPath()
        {
            if (StringUtils.ToToggleBool(settings.toggleExportToModFolder))
            {
                return Storage.modEntryPath + Storage.exportFolder;
            }
            else
            {
                return Application.persistentDataPath;
            }
        }

        public static int GetEncounterCR()
        {
            if ((bool)((SerializedScriptableObject)BlueprintRoot.Instance.Progression.CRTable))
                return Utilities.GetTotalChallengeRating(Game.Instance.State.Units.Where((Func<UnitEntityData, bool>)(u =>
                {
                    if (u.IsInCombat)
                        return !u.IsPlayerFaction;
                    return false;
                })).Select((Func<UnitEntityData, BlueprintUnit>)(u => u.Blueprint)).ToList());
            UberDebug.LogChannel("SmartConsole", "CR table not found at Assets/Mechanics/Blueprints/Classes/Basic/CRTable.asset or 19b09eaa18b203645b6f1d5f2edcb1e4, cannot calculate", (object[])Array.Empty<object>());
            return -1;
        }

        public static string GetDifficulty()
        {
            int num = GetEncounterCR() - Game.Instance.Player.PartyLevel;
            if (num < 3)
                return "Easy";
            return num < 5 ? "Hard" : "Boss";
        }

        public static void ModLoggerDebug(string message)
        {
            if (settings.settingShowDebugInfo)
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

        public static void RefreshArrayFromFile(ref string[] array, string folder, string fileFormat)
        {
            try
            {
                array = Directory.GetFiles(Storage.modEntryPath + folder, $"*.{fileFormat}");
                Array.Sort(array);
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

        public static string[] getObjectInfo(object o)
        {

            string fields = "";
            foreach (string field in Traverse.Create(o).Fields())
            {
                fields = fields + field + ", ";
            }
            string methods = "";
            foreach (string method in Traverse.Create(o).Methods())
            {
                methods = methods + method + ", ";
            }
            string properties = "";
            foreach (string property in Traverse.Create(o).Properties())
            {
                properties = properties + property + ", ";
            }
            return new string[] { fields, methods, properties };
        }

    }

    public class ModEntryCheck
    {

        UnityModManager.ModEntry modEntry;


        public ModEntryCheck(string modId)
        {
            modEntry = UnityModManager.FindMod(modId);
        }

        public bool ModIsActive()
        {
            if (modEntry != null && modEntry.Assembly != null)
            {
                return modEntry.Active;
            }
            else
            {
                return false;
            }
        }
        public bool IsInstalled()
        {
            if (modEntry != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string Version()
        {
            if (modEntry != null)
            {
                return modEntry.Info.Version;
            }
            else
            {
                return "";
            }
        }
    }

    class AutoCompleteDictionary<T> : SortedDictionary<string, T>
    {
        private AutoCompleteDictionary<T>.AutoCompleteComparer m_comparer;

        public AutoCompleteDictionary()
          : base((IComparer<string>)new AutoCompleteDictionary<T>.AutoCompleteComparer())
        {
            this.m_comparer = this.Comparer as AutoCompleteDictionary<T>.AutoCompleteComparer;
        }

        public T LowerBound(string lookupString)
        {
            this.m_comparer.Reset();
            this.ContainsKey(lookupString);
            return this[this.m_comparer.LowerBound];
        }

        public T UpperBound(string lookupString)
        {
            this.m_comparer.Reset();
            this.ContainsKey(lookupString);
            return this[this.m_comparer.UpperBound];
        }

        public T AutoCompleteLookup(string lookupString)
        {
            this.m_comparer.Reset();
            this.ContainsKey(lookupString);
            return this[this.m_comparer.UpperBound != null ? this.m_comparer.UpperBound : this.m_comparer.LowerBound];
        }

        private class AutoCompleteComparer : IComparer<string>
        {
            private string m_lowerBound;
            private string m_upperBound;

            public string LowerBound
            {
                get
                {
                    return this.m_lowerBound;
                }
            }

            public string UpperBound
            {
                get
                {
                    return this.m_upperBound;
                }
            }

            public int Compare(string x, string y)
            {
                int num = Comparer<string>.Default.Compare(x, y);
                if (num >= 0)
                    this.m_lowerBound = y;
                if (num <= 0)
                    this.m_upperBound = y;
                return num;
            }

            public void Reset()
            {
                this.m_lowerBound = (string)null;
                this.m_upperBound = (string)null;
            }
        }
    }
}