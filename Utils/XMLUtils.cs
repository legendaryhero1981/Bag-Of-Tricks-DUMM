using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BagOfTricks.Utils {
    public static class XMLUtils {
        public static void SerializeListString(this List<string> list, string filePath) {
            var serializer = new XmlSerializer(typeof(List<string>));
            using (var stream = File.Create(filePath)) {
                serializer.Serialize(stream, list);
            }
        }
        public static void DeserializeListString(this List<string> list, string filePath) {
            try {
                var serializer = new XmlSerializer(typeof(List<string>));
                using (var stream = File.OpenRead(filePath)) {
                    var other = (List<string>)(serializer.Deserialize(stream));
                    list.Clear();
                    list.AddRange(other);
                }
            }
            catch (Exception e) {
                Main.modLogger.Log("\n" + filePath + "\n" + e.ToString());
            }
        }
    }
}
