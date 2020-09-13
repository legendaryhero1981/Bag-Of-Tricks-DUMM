using System;

namespace BagOfTricks.Extensions
{
    public static class StringExtensions {
        public static bool ContainsAny(this String genericString, params string[] searchStrings) {
            foreach (string s in searchStrings) {
                if (genericString.Contains(s)) {
                    return true;
                }
            }
            return false;
        }
    }
}