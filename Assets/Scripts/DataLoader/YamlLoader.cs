using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class YamlLoader : IDataLoader
{
    private IDeserializer deserializer;
    private ISerializer serializer;
    
    public YamlLoader()
    {
        deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        
        serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
            .Build();
    }
    
    public T Load<T>(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        string yaml = File.ReadAllText(filePath);
        return LoadFromText<T>(yaml);
    }
    
    public T LoadFromText<T>(string text)
    {
        try
        {
            return deserializer.Deserialize<T>(text);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deserializing YAML: {ex.Message}");
            throw;
        }
    }
    
    public async Task<T> LoadAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        using (StreamReader reader = new StreamReader(filePath))
        {
            string yaml = await reader.ReadToEndAsync();
            return LoadFromText<T>(yaml);
        }
    }
    
    public void Save<T>(T data, string filePath)
    {
        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
                
            string yaml = serializer.Serialize(data);
            File.WriteAllText(filePath, yaml);
            
            Debug.Log($"Data saved to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving YAML to {filePath}: {ex.Message}");
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
                
            string yaml = serializer.Serialize(data);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync(yaml);
            }
            
            Debug.Log($"Data saved asynchronously to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving YAML asynchronously to {filePath}: {ex.Message}");
            throw;
        }
    }
    
    public string[] GetSupportedExtensions()
    {
        return new string[] { ".yaml", ".yml" };
    }
}
