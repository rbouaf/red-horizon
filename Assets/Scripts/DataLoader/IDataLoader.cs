using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Interface for data loading and saving operations.
/// Implementations handle specific file formats (JSON, YAML, etc.)
/// </summary>
public interface IDataLoader
{
    T Load<T>(string filePath);
    
    T LoadFromText<T>(string text);
    
    Task<T> LoadAsync<T>(string filePath);
    
    void Save<T>(T data, string filePath);
    
    Task SaveAsync<T>(T data, string filePath);
    
    string[] GetSupportedExtensions();
}
