using System;
using System.Linq;
using System.Reflection;


namespace BagOfTricks.Extensions {
    public static class EnumExtension {
        public static string GetDescription(this Enum genericEnum) {
            Type type = genericEnum.GetType();
            MemberInfo[] memberInfo = type.GetMember(genericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0)) {
                object[] customAttribut = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((customAttribut != null && customAttribut.Count() > 0)) {
                    return ((System.ComponentModel.DescriptionAttribute)customAttribut.ElementAt(0)).Description;
                }
            }
            return genericEnum.ToString();
        }
    }
}