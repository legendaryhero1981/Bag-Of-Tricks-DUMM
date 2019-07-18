using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using GL = UnityEngine.GUILayout;
using Random = UnityEngine.Random;

namespace BagOfTricks
{
    [Serializable]
    public class TaxCollectorSettings
    {
        public bool initialLaunch = true;
        public string playerTitle = "";
        public DateTime saveTime;
        public int textLineInitialCounter = 0;
    }

    public static class TaxCollector
    {
        public static Settings settings = Main.settings;

        public static DateTime saveTimeGame = new DateTime();

        public static bool isFirstVisit = true;

        public static string playerTitleInput = "";

        public static string resultLine_0 = "";

        public static int MoneyRank(int rankEconomy, int rankStability, int rankLoyalty)
        {
            if (rankEconomy == 0) rankEconomy = 1;
            if (rankStability == 0) rankStability = 1;
            if (rankLoyalty == 0) rankLoyalty = 1;
            var result = rankEconomy * rankStability * rankLoyalty;
            return result / 3;
        }

        public static int BPRank(int rankEconomy, int rankStability, int rankCommunity)
        {
            if (rankEconomy == 0) rankEconomy = 1;
            if (rankStability == 0) rankStability = 1;
            if (rankCommunity == 0) rankCommunity = 1;
            var result = rankEconomy * rankStability * rankCommunity;
            return result / 3;
        }


        public static int CollectBP(int rank, DateTime saveTime)
        {
            var collectTime = DateTime.Now;
            var timePassed = collectTime.Subtract(saveTime);
            var timeMultiplier = (int) Math.Floor(timePassed.TotalHours);

            if (timeMultiplier < 1) return 0;
            if (timeMultiplier > 72) timeMultiplier = 72;
            var result = Mathf.RoundToInt(timeMultiplier * rank * Random.Range(0.75f, 1.5f));
            return result;
        }

        public static int CollectMoney(int rank, int charisma, DateTime saveTime)
        {
            var collectTime = DateTime.Now;
            var timePassed = collectTime.Subtract(saveTime);
            var timeMultiplier = (int) Math.Floor(timePassed.TotalHours);

            if (timeMultiplier < 1) return 0;
            if (timeMultiplier > 72) timeMultiplier = 72;
            var result = Mathf.RoundToInt(timeMultiplier * rank * charisma / 2 * Random.Range(0.75f, 1.5f));
            return result;
        }


        public static void Serialize(TaxCollectorSettings tax, string filePath)
        {
            var serializer = new XmlSerializer(typeof(TaxCollectorSettings));
            using (var stream = File.Create(filePath))
            {
                serializer.Serialize(stream, tax);
            }
        }

        public static TaxCollectorSettings Deserialize(string filePath)
        {
            var newTax = new TaxCollectorSettings();
            if (File.Exists(filePath))
                try
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var serializer = new XmlSerializer(typeof(TaxCollectorSettings));
                        var save = (TaxCollectorSettings) serializer.Deserialize(stream);
                        return save;
                    }
                }
                catch (Exception exception)
                {
                    Main.modLogger.Log(exception.ToString());
                }

            return newTax;
        }
    }
}