using System;
using System.Collections.Generic;
using Godot;
// ReSharper disable InconsistentNaming

namespace Petrichor.scripts;

public static class Utils
{
    public static readonly FontFile CozzetteFont = (FontFile)ResourceLoader.Load("res://resources/CozetteVector.ttf");
    
    public const string INFO_STR = "\x1b[97m[INFO]\x1b[97m ";
    public const string ERROR_STR = "\x1b[91m[ERROR]\x1b[97m ";
    public const string WARNING_STR = "\x1b[93m[WARNING]\x1b[97m ";
    public const string CHECK_STR = "\x1b[92m[CHECK]\x1b[97m ";
    public const string TEST_STR = "\x1b[95m[TEST]\x1b[97m ";
    
    public static string Trim(this string str, int fromStart, int fromEnd)
    {
        char[] chars = new char[str.Length - fromStart - fromEnd];
        for (int i = fromStart; i < str.Length - fromEnd; i++)
        {
            chars[i - fromStart] = str[i];
        }

        return new string(chars);
    }

    public static string[] SplitUnnested(this string str, char c)
    {
        List<string> substrings = new List<string>();
        int lastChar = 0;
        int nests = 0;
        bool inQuotes = false;
        
        for (int i = 0; i < str.Length; i++)
        {
            if ((str[i] == c && !inQuotes && nests == 0) || i == str.Length - 1)
            {
                substrings.Add(str.Trim(lastChar, str.Length - i - (i == str.Length - 1 ? 1 : 0)));
                lastChar = i;
            }
            else if (str[i] == '[' || str[i] == '(')
                nests++;
            else if (str[i] == ']' || str[i] == ')')
                nests--;
            else if (str[i] == '"')
                inQuotes = !inQuotes;
        }

        for (int i = 0; i < substrings.Count; i++)
        {
            substrings[i] = substrings[i].Trim(c, ' ');
        }
        
        substrings.RemoveAll(string.IsNullOrWhiteSpace);
        return substrings.ToArray();
    }
    
    public static int[] ToIntArray(this string[] str)
    {
        int[] ret = new int[str.Length];
        for (int i = 0; i < str.Length; i++)
        {
            ret[i] = str[i].ToInt();
        }

        return ret;
    }
    
    public static T[] Slice<T>(this T[] source, int start, int end)
    {
        if (end < 0)
        {
            end = source.Length + end;
        }
        int len = end - start;
        
        T[] res = new T[len];
        for (int i = 0; i < len; i++)
        {
            res[i] = source[i + start];
        }
        return res;
    }

    public static Color Color8(int r, int g, int b)
    {
        return new Color(r * 0.00392156862f, g * 0.00392156862f, b * 0.00392156862f);
    }

    public static class FlagsHelper
    {
        public static bool IsSet<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void Set<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        public static void Unset<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }

        public static List<int> DecomposeFlag(int flag)
        {
            var bitStr = Convert.ToString(flag, 2);
            var returnValue = new List<int>();
            for (var i = 0; i < bitStr.Length; i++)
            {
                if (bitStr[bitStr.Length - i - 1] == '1')
                {
                    returnValue.Add((int)Math.Pow(2, i));
                }
            }

            return returnValue;
        }
    }
}