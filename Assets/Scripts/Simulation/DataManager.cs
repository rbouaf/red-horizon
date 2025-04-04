using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    private Dictionary<string, IDataLoader> loaders = new Dictionary<string, IDataLoader>();
    
    [Header("Data Paths")]
    [SerializeField] private string dataFolder = "configs";
    [SerializeField] private string environmentSettingsPath = "environmentSettings.yaml";
    [SerializeField] private string roverConfigPath = "roverConfig.yaml";
    [SerializeField] private string simulationSettingsPath = "simulationSettings.yaml";
    
    [Header("Loading Options")]
    [SerializeField] private bool loadOnStart = true;
    
    // Data storage
    public EnvironmentSettings EnvironmentSettings { get; private set; }
    public RoverConfig RoverConfig { get; private set; }
    //public SimulationSettings SimulationSettings { get; private set; }
    
    // Loading status
    public bool IsLoading { get; private set; }
    public float LoadingProgress { get; private set; }
    public bool IsDataLoaded { get; private set; }
    
    // Events
    public event Action OnDataLoadingComplete;
    public event Action<string> OnLoadingError;
    
    private int totalLoadingOperations = 3; // Three files to load
    private int completedLoadingOperations = 0;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Register loaders
        RegisterLoader(new JsonLoader());
        RegisterLoader(new YamlLoader());
        
        IsLoading = false;
        LoadingProgress = 0f;
        IsDataLoaded = false;
    }
    
    public void RegisterLoader(IDataLoader loader)
    {
        string[] extensions = loader.GetSupportedExtensions();
        foreach (string extension in extensions)
        {
            loaders[extension.ToLower()] = loader;
        }
        
        Debug.Log($"Registered loader for extensions: {string.Join(", ", extensions)}");
    }
    
    private IDataLoader GetLoaderForFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        if (loaders.TryGetValue(extension, out IDataLoader loader))
        {
            return loader;
        }
        
        throw new NotSupportedException($"No loader found for file type: {extension}");
    }
    
    public async void LoadAllData()
    {
        if (IsLoading)
            return;
            
        IsLoading = true;
        LoadingProgress = 0f;
        completedLoadingOperations = 0;
        
        try
        {
            EnvironmentSettings = await LoadDataAsync<EnvironmentSettings>(environmentSettingsPath);
            Debug.Log($"Loaded environment settings successfully.");
            IncrementLoadingProgress();
            
            RoverConfig = await LoadDataAsync<RoverConfig>(roverConfigPath);
            Debug.Log($"Loaded rover configuration successfully.");
            IncrementLoadingProgress();
            
            // SimulationSettings = await LoadDataAsync<SimulationSettings>(simulationSettingsPath);
            // Debug.Log($"Loaded simulation settings successfully.");
            // IncrementLoadingProgress();
            
            Debug.Log("All simulation data loaded successfully.");
            IsDataLoaded = true;
            
            // Notify that loading is complete
            OnDataLoadingComplete?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading data: {ex.Message}");
            OnLoadingError?.Invoke(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task<T> LoadDataAsync<T>(string path)
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, dataFolder, path);
            
            Debug.Log($"Loading from file system: {filePath}");
            
            IDataLoader loader = GetLoaderForFile(filePath);
            return await loader.LoadAsync<T>(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading data from {path}: {ex.Message}");
            throw;
        }
    }
    
    private void IncrementLoadingProgress()
    {
        completedLoadingOperations++;
        LoadingProgress = (float)completedLoadingOperations / totalLoadingOperations;
        Debug.Log($"Loading progress: {LoadingProgress:P0}");
    }
    
    public T LoadData<T>(string path)
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, dataFolder, path);
            
            IDataLoader loader = GetLoaderForFile(filePath);
            return loader.Load<T>(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading data from {path}: {ex.Message}");
            throw;
        }
    }
    
    public void SaveData<T>(T data, string path)
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, dataFolder, path);
            
            IDataLoader loader = GetLoaderForFile(filePath);
            loader.Save(data, filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving data to {path}: {ex.Message}");
            throw;
        }
    }
}

[Serializable]
public class RoverConfig
{
    public List<RoverModel> rovers { get; set; } = new List<RoverModel>();
}

[Serializable]
public class EnvironmentSettings
{
    public EnvironmentModel environment { get; set; }
}