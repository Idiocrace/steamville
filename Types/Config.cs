using Microsoft.VisualBasic;

namespace TileGame.Types;

// I think this is the only class that has an internal reference to its serialize method
public class Config
{
    private Dictionary<string, object?> settings = new Dictionary<string, object?>();

    public T GetSetting<T>(string key, T defaultValue = default!)
    {
        if (settings.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    public void SetSetting<T>(string key, T value)
    {
        settings[key] = value;
    }

    public bool HasSetting(string key)
    {
        return settings.ContainsKey(key);
    }   

    public void RemoveSetting(string key)
    {
        settings.Remove(key);
    }

    public void ClearSettings()
    {
        settings.Clear();
    }

    // Gets used for saving
    public void Serialize(string filePath)
    {
        var options = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        };
        string jsonData = System.Text.Json.JsonSerializer.Serialize(settings, options);
        File.WriteAllText(filePath, jsonData);
    }

    // Gets used for loading
    public static Config Deserialize(string filePath)
    {
        string jsonData = File.ReadAllText(filePath);
        var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonData);
        var config = new Config();
        if (settings != null)
        {
            foreach (var kvp in settings)
            {
                config.settings[kvp.Key] = kvp.Value;
            }
        }
        return config;
    }
}
