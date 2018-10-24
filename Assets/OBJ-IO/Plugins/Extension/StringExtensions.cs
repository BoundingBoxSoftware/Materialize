
using System;
using System.Globalization;

public static class StringExt
{
    //------------------------------------------------------------------------------------------------------------
    public static float ParseInvariantFloat(this string floatString)
    {
        return float.Parse(floatString, CultureInfo.InvariantCulture.NumberFormat);
    }

    //------------------------------------------------------------------------------------------------------------
    public static int ParseInvariantInt(this string intString)
    {
        return int.Parse(intString, CultureInfo.InvariantCulture.NumberFormat);
    }

    //------------------------------------------------------------------------------------------------------------
    public static bool EqualsInvariantCultureIgnoreCase(this string str, string s)
    {
        return str.Equals(s, StringComparison.InvariantCultureIgnoreCase);
    }

    //------------------------------------------------------------------------------------------------------------
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    //------------------------------------------------------------------------------------------------------------
    public static bool IsNullOrWhiteSpace(string value)
    {
        if (value == null)
            return true;
        for (int index = 0; index < value.Length; ++index)
        {
            if (!char.IsWhiteSpace(value[index]))
                return false;
        }
        return true;
    }
}