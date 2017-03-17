using UnityEngine;
using System.Text.RegularExpressions;

namespace iCarus
{
    public static class StringExtensions
    {
        static Regex sCamelCaseToUnderscoreRegex = new Regex(@"(?<=[A-Za-z0-9]+)([A-Z]|\d)");
        public static string CamelCaseToUnderscore(this string str)
        {
            return sCamelCaseToUnderscoreRegex.Replace(str, "_$0").ToLower();
        }
    }

    public static class UnityExtensions
    {
        public static void Reset(this Transform xform)
        {
            xform.localPosition = Vector3.zero;
            xform.localRotation = Quaternion.identity;
            xform.localScale = Vector3.one;
        }
    }
}
