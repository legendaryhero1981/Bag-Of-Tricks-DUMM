namespace BagOfTricks.Utils
{
    public static class RichTextUtils
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
            return $"<b>  {s}</b>";
        }

        public static string Italic(string s)
        {
            return $"<i>  {s}</i>";
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
}
