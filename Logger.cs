using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BagOfTricks {

    public class Logger {
        private String _path;
        private bool _removeHtmlTags = true;
        public bool RemoveHtmlTags { get => _removeHtmlTags; set => _removeHtmlTags = value; }
        private bool _useTimeStamp = true;
        public bool UseTimeStamp { get => _useTimeStamp; set => _useTimeStamp = value; }

        public Logger() : this(Storage.bagOfTicksLogFile) {

        }

        public Logger(String fileName, String fileExtension = ".log") {
            _path = Path.Combine(Storage.modEntryPath, (fileName + fileExtension));
            Clear();
        }

        public void Log(string str) {
            if (_removeHtmlTags) {
                str = Common.RemoveHtmlTags(str);
            }
            if (UseTimeStamp) {
                ToFile(TimeStamp() + " " + str);
            }
            else {
                ToFile(str);

            }
        }

        private static string TimeStamp() {
            return "[" + DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.ff") + "]";
        }

        private void ToFile(string s) {
            try {
                using (StreamWriter stream = File.AppendText(_path)) {
                    stream.WriteLine(s);
                }
            }
            catch (Exception e) {
                Main.modLogger.Log(e.ToString());
            }
        }

        public void Clear() {
            if (File.Exists(_path)) {
                try {
                    File.Delete(_path);
                    using (File.Create(_path)) {
                    }
                }
                catch (Exception e) {
                    Main.modLogger.Log(e.ToString());
                }
            }
        }
    }

    public class HtmlLogger : Logger {

        public HtmlLogger() : this(Storage.bagOfTicksLogFile) {
        }

        public HtmlLogger(String fileName) : base(fileName, ".html") {
            this.RemoveHtmlTags = false;
            this.UseTimeStamp = false;
        }

        public new void Log(string str) {
            str = Common.UnityRichTextToHtml(str);
            base.Log(str);
        }
    }

    public static class LoggerUtils {

        public static void InitBagOfTrickLogger() {
            if (Main.botLoggerLog == null) {
                Main.botLoggerLog = new Logger();
            }
        }

        public static void InitBattleLogDefaultLogger() {
            if (Main.battleLoggerLog == null) {
                Main.battleLoggerLog = new Logger(Storage.battleLogFile);
            }
        }

        public static void InitBattleLogHtmlLogger() {
            if (Main.battleLoggerHtml == null) {
                Main.battleLoggerHtml = new HtmlLogger(Storage.battleLogFile);
            }
        }
    }
}
