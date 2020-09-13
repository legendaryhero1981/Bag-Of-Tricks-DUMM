using System;

namespace BagOfTricks.Utils
{

    public static class StringUtils
    {

        public static bool IsGUID(string guid)
        {
            Guid id;
            return Guid.TryParse(guid, out id);
        }

        public static string PutInParenthesis(string s)
        {
            return s = "(" + s + ")";
        }

        public static bool ToToggleBool(string s)
        {
            if (s == Storage.isTrueString)
            {
                return true;
            }

            if (s == Storage.isFalseString)
            {
                return false;
            }

            throw new ArgumentOutOfRangeException("StringToBool received an invalid string!");
        }
    }
}
