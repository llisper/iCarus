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

        public static int ToInt(this Color color)
        {
            return ((int)(color.r * 255) << 24) |
                   ((int)(color.g * 255) << 16) |
                   ((int)(color.b * 255) << 8) |
                   (int)(color.a * 255);
        }

        public static Color FromInt(this Color color, int value)
        {
            return new Color(
                ((value >> 24) & 0xff) / 255f,
                ((value >> 16) & 0xff) / 255f,
                ((value >> 8) & 0xff) / 255f,
                (value & 0xff) / 255f);
        }
    }
}
