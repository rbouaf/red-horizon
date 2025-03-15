using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;


public class JsonLoader : IDataLoader
{
    private JsonSerializerSettings settings;

    public JsonLoader()
    {
        settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Auto
        };
    }

    public T Load<T>(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        string json = File.ReadAllText(filePath);
        return LoadFromText<T>(json);
    }
    
    public T LoadFromText<T>(string text)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(text, settings);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deserializing JSON: {ex.Message}");
            throw;
        }
    }
    
    public async Task<T> LoadAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        using (StreamReader reader = new StreamReader(filePath))
        {
            string json = await reader.ReadToEndAsync();
            return LoadFromText<T>(json);
        }
    }
    
    public void Save<T>(T data, string filePath)
    {
        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            
            string json = JsonConvert.SerializeObject(data, settings);
            File.WriteAllText(filePath, json);
            
            Debug.Log($"Data saved to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving JSON to {filePath}: {ex.Message}");
            throw;
        }
    }
    
    public async Task SaveAsync<T>(T data, string filePath)
    {
        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
                
            string json = JsonConvert.SerializeObject(data, settings);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync(json);
            }
            
            Debug.Log($"Data saved asynchronously to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving JSON asynchronously to {filePath}: {ex.Message}");
            throw;
        }
    }
    
    public string[] GetSupportedExtensions()
    {
        return new string[] { ".json" };
    }
}
