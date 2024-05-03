using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Petrichor.scripts.Lingo;

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

    public static string TrimUnquotedWhitespace(this string str)
    {
        List<char> chars = new();
        int quoteNum = 0;
        foreach (char c in str)
        {
            if (c == '"')
            {
                quoteNum++;
                chars.Add(c);
            }
            else if (c == ' ' && quoteNum % 2 != 0)
                chars.Add(c);
            else if (c != ' ')
                chars.Add(c);
        }

        return new string(chars.ToArray());
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
    
    public static T[] ToArray<T>(this LingoLinearList list)
    {
        if (list == null)
            return null;
        
        T[] array = new T[list.Values.Length];
        for (int i = 0; i < list.Values.Length; i++)
        {
            if (list.Values[i] is not T)
                throw new ArrayTypeMismatchException("Unable to cast 'LingoLinearList' to array of type '" + typeof(T) + "'.");
            array[i] = (T)list.Values[i];
        }
        return array;
    }
    
    public static T[] Slice<T>(this T[] source, int start, int end)
    {
        if (end == start)
        {
            return new T[] { source[start] };
        }
        
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
        return new Color(r / 255f, g / 255f, b / 255f);
    }
    
    public static Color Color8(int r, int g, int b, int a)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
    
    // This thing is so fucking slow, not using it
    public static bool FileExistsCaseSensitive(string filename)
    {
        string name = Path.GetDirectoryName(filename);

        return name != null 
               && Array.Exists(Directory.GetFiles(name), s => s == Path.GetFullPath(filename));
    }

    public static void Fill3D<T>(this T[,,] array, T fillValue)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                for (int k = 0; k < array.GetLength(2); k++)
                {
                    array[i, j, k] = fillValue;
                }
            }
        }
    }

    public static List<T> LoadFolder<T>(string path, ResourceLoader.CacheMode mode, bool global = true) where T : Resource
    {
        if (global) path = ProjectSettings.GlobalizePath(path);
        
        List<string> filePaths = Directory.EnumerateFiles(path).ToList();
        List<T> files = new();

        foreach (string t in filePaths)
        {
            if (!t.EndsWith(".import"))
            {
                T res = ResourceLoader.Load<T>(t, "", mode);
                res.ResourceName = Path.GetFileNameWithoutExtension(t);
                files.Add(res);
            }
        }

        return files;
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