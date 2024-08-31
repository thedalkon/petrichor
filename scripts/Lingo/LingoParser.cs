using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

namespace Petrichor.scripts.Lingo;

public static class LingoParser
{
    public static LingoCategory[] ParseFromFile(string path)
    {
        List<string> fileList = File.ReadAllLines(path).ToList();
        fileList.RemoveAll(string.IsNullOrWhiteSpace);
        fileList.RemoveAll(s => s.StartsWith("--"));
        string[] fileLines = fileList.ToArray();
        
        List<int> categoryLines = new();
        for (int i = 0; i < fileLines.Length; i++)
        {
            if (fileLines[i].StartsWith('-'))
                categoryLines.Add(i);
            fileLines[i] = fileLines[i].Replace("\t", "");
        }
        
        LingoCategory[] categories = new LingoCategory[categoryLines.Count];

        for (int i = 0; i < categoryLines.Count; i++)
        {
            int subLines;
            if (i + 1 == categoryLines.Count)
                subLines = fileLines.Length - categoryLines[i] - 2;
            else
                subLines = categoryLines[i + 1] - categoryLines[i] - 1;
            
            if (subLines < 1)
            {
                LingoCategory emptyCat = LingoCategory.FromString(fileLines[categoryLines[i]], Array.Empty<string>());
                Debug.WriteLine(Utils.WARNING_STR + "Category " + emptyCat.Name + " is empty, skipping.");
                continue;
            }

            var substrings = i + 1 == categoryLines.Count ? 
                fileLines.Slice(categoryLines[i] + 1, fileLines.Length) 
                : 
                fileLines.Slice(categoryLines[i] + 1, categoryLines[i + 1] - 1);
            
            categories[i] = LingoCategory.FromString(fileLines[categoryLines[i]], substrings);
        }

        return categories;
    }
    
    public static object ParseValue(string value)
    {
        value = value.TrimUnquotedWhitespace();
        
        if (value[0] == '"') // String
        {
            return value.Trim(1, 1);
        }
        
        if (int.TryParse(value, out int strInt)) // Int
        {
            return strInt;
        }
        
        if (float.TryParse(value, out float strFloat)) // Float
        {
            return strFloat;
        }
        
        if (value.StartsWith("point(")) // Point
        {
            bool isFloat = value.Contains('.');
            string[] numArray = value.Trim(6, 1).Split(',');
            try
            {
                return isFloat ? new Vector2(float.Parse(numArray[0]), float.Parse(numArray[1])) :
                    new Vector2I(int.Parse(numArray[0]), int.Parse(numArray[1]));
            }
            catch (FormatException)
            {
                throw new Exception("Point() value wrongly formatted: " + value);
            }
        }

        if (value.StartsWith("rect(")) // Rect
        {
            bool isFloat = value.Contains('.');
            string[] numArray = value.Trim(5, 1).Split(',');
            try
            {
                return isFloat
                    ? new Rect2(new Vector2(float.Parse(numArray[0]), float.Parse(numArray[1])),
                        new Vector2(float.Parse(numArray[2]), float.Parse(numArray[3])))
                    : new Rect2I(new Vector2I(int.Parse(numArray[0]), int.Parse(numArray[1])),
                        new Vector2I(int.Parse(numArray[2]), int.Parse(numArray[3])));
            }
            catch (FormatException)
            {
                throw new Exception("Rect() value wrongly formatted: " + value);
            }
        }

        if (value.StartsWith('[')) // Linear List
        {
            if (value.StartsWith("[#"))
                return LingoPropertyList.FromString(value);
            return LingoLinearList.FromString(value);
        }

        if (value == "void") // Void??
        {
            return null;
        }
        
        throw new Exception("Tried to parse unknown property: " + value);
    }
}

public class LingoCategory
{
    public string Name;
    public Color Color;
    public LingoPropertyList[] Collections;

    private LingoCategory(string name, Color color, LingoPropertyList[] collections)
    {
        Name = name;
        Color = color;
        Collections = collections;
    }
    
    public static LingoCategory FromString(string header, string[] substrings)
    {
        string[] categoryParams = header.Split(',', 2);
        int[] rgbComps = Regex.Match(categoryParams[1],@"\(([^\)]+)\)").Value.Trim(1,1).Split(',').ToIntArray();

        LingoPropertyList[] collections = new LingoPropertyList[substrings.Length];
        for (int i = 0; i < substrings.Length; i++)
        {
            collections[i] = LingoPropertyList.FromString(substrings[i]);
        }
        
        return new LingoCategory(categoryParams[0].Replace("\"", "").Trim(2,0),
            Utils.Color8(rgbComps[0], rgbComps[1], rgbComps[2]), collections);
    }
}

public class LingoLinearList
{
    public object[] Values;

    public object this[int i]
   {
      get { return Values[i]; }
      set { Values[i] = value; }
   }

    public int Length => Values.Length;

    private LingoLinearList(object[] values)
    {
        Values = values;
    }

    public static LingoLinearList FromString(string str)
    {
        string[] valueStr = str.Trim(1, 1).SplitUnnested(',');
        object[] values = new object[valueStr.Length];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = LingoParser.ParseValue(valueStr[i]);
        }
        return new LingoLinearList(values);
    }
}

public class LingoPropertyList
{
    public readonly LingoProperty[] Properties;
    
    public LingoProperty this[int i]
   {
      get { return Properties[i]; }
      set { Properties[i] = value; }
   }

    private LingoPropertyList(LingoProperty[] properties)
    {
        Properties = properties;
    }

    public static LingoPropertyList FromString(string str)
    {
        string[] propertyStr = str.Trim().Trim(1, 1).SplitUnnested(',');
        LingoProperty[] properties = new LingoProperty[propertyStr.Length];
        for (int i = 0; i < properties.Length; i++)
        {
            properties[i] = LingoProperty.FromString(propertyStr[i]);
        }

        return new LingoPropertyList(properties);
    }

    public LingoProperty GetProperty(string tag)
    {
        for (int i = 0; i < Properties.Length; i++)
        {
            if (Properties[i].Tag == tag)
                return Properties[i];
        }

        return null;
    }
}

public class LingoProperty
{
    public readonly string Tag;
    public readonly object Value;

    private LingoProperty(string tag, object value)
    {
        Tag = tag;
        Value = value;
    }

    public static LingoProperty FromString(string str)
    {
        string[] components = str.Split(':', 2);
        return new LingoProperty(components[0].TrimStart('#'), LingoParser.ParseValue(components[1]));
    }
}