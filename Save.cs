using System;
using System.IO;
using System.Xml.Serialization;

using Kingmaker.EntitySystem.Persistence;

namespace BagOfTricks
{
    [Serializable]
    public class SaveData
    {
        public string  saveVersion = "1.0.0";
        public string fileName;
        public int lockPicks;
    }

    public static class SaveTools
    {
        public static void Serialize(SaveData saveData, string filePath)
        {
            var serializer = new XmlSerializer(typeof(SaveData));
            using (var stream = File.Create(filePath))
            {
                serializer.Serialize(stream, saveData);
            }
        }

        public static SaveData Deserialize(string filePath)
        {
            var newSaveData = new SaveData();
            if (File.Exists(filePath))
                try
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var serializer = new XmlSerializer(typeof(SaveData));
                        var save = (SaveData) serializer.Deserialize(stream);
                        return save;
                    }
                }
                catch (Exception exception)
                {
                    Main.ModLogger.Log(exception.ToString());
                }

            return newSaveData;
        }

        public static void SaveFile(SaveInfo saveInfo)
        {
            try
            {
                var name = Strings.RemoveExt(saveInfo.FileName);
                var filePath = Storage.modEntryPath + Storage.savesFolder + "\\" + name + ".xml";

                Main.SaveData.fileName = name;
                Main.SaveData.lockPicks = Storage.lockPicks;

                Common.ModLoggerDebug($"PrepareSave {name}");

                if (File.Exists(filePath))
                {
                    Serialize(Main.SaveData, filePath);

                    Common.ModLoggerDebug($"{Storage.modEntryPath + Storage.savesFolder + "\\" + name} overwritten.");
                    
                }
                else
                {
                    Serialize(Main.SaveData, filePath);

                    Common.ModLoggerDebug($"{Storage.modEntryPath + Storage.savesFolder + "\\" + name} created.");
                    
                }
            }
            catch (Exception e)
            {
                Main.ModLogger.Log(e.ToString());
            }
        }

        public static void LoadFile(SaveInfo saveInfo)
        {
            try
            {
                var name = Strings.RemoveExt(saveInfo.FileName);
                var filePath = Storage.modEntryPath + Storage.savesFolder + "\\" + name + ".xml";

                Common.ModLoggerDebug($"LoadGame {name}");

                if (File.Exists(filePath))
                {
                    Main.SaveData = Deserialize(filePath);
                    Storage.lockPicks = Main.SaveData.lockPicks;
                    Common.ModLoggerDebug($"{filePath} loaded.");
                }
                else
                {
                    Storage.lockPicks = 5;
                    Main.ModLogger.Log($"{filePath} not found!");
                }
            }
            catch (Exception e)
            {
                Main.ModLogger.Log(e.ToString());
            }
        }

        public static void DeleteFile(SaveInfo saveInfo)
        {
            try
            {
                var name = Strings.RemoveExt(saveInfo.FileName);
                var filePath = Storage.modEntryPath + Storage.savesFolder + "\\" + name + ".xml";

                Common.ModLoggerDebug($"DeleteSave {name}");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                    Common.ModLoggerDebug($"{name} deleted.");                    
                }
            }
            catch (Exception e)
            {
                Main.ModLogger.Log(e.ToString());
            }
        }
    }
}