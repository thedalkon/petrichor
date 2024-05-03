using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Petrichor.scripts;

[Serializable]
public class UserData
{
    private static JsonSerializerOptions _jsonOptions = new()
    {
        AllowTrailingCommas = false,
        IncludeFields = true,
        WriteIndented = true,
        IgnoreReadOnlyProperties = true
    };
    
    public string SavedTileDir;

    [JsonConstructor]
    public UserData()
    {
        SavedTileDir = "";
    }
    
    public static UserData Load(string path)
    {
        UserData loadedData = JsonSerializer.Deserialize<UserData>(File.ReadAllText(path), _jsonOptions);

        if (loadedData == null)
            throw new JsonException($"Error while loading user data, verify path '{path}' exists.");

        return loadedData;
    }

    public void Save(string path)
    {
        string serializedData = JsonSerializer.Serialize(this, _jsonOptions);
        File.WriteAllText(path, serializedData);
    }
}