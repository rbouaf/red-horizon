using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class SimulationController : MonoBehaviour
{
    private Dictionary<string, RoverModel> availableRoverModels = new Dictionary<string, RoverModel>();
    
    public RoverModel activeRoverModel;
    
    private DataManager dataManager;
    
    // Events for rover loading
    public event Action OnRoverModelsLoaded;
    public event Action<string> OnRoverLoadingError;
    
    [Header("Debug Options")]
    [SerializeField] private bool loadDataOnStart = true;
    [SerializeField] private bool logDetailedInfo = true;
    
    private void Awake()
    {
        dataManager = FindAnyObjectByType<DataManager>();
        if (dataManager == null)
        {
            Debug.LogWarning("DataManager not found, creating a new instance");
            GameObject dataManagerObj = new GameObject("DataManager");
            dataManager = dataManagerObj.AddComponent<DataManager>();
        }
    }
    
    private void Start()
    {
        if (loadDataOnStart)
        {
            // Subscribe to data loading events
            dataManager.OnDataLoadingComplete += OnDataLoadingComplete;
            dataManager.OnLoadingError += OnDataLoadingError;
            
            if (!dataManager.IsDataLoaded && !dataManager.IsLoading)
            {
                Debug.Log("Starting data loading process...");
                dataManager.LoadAllData();
            }
            else if (dataManager.IsDataLoaded)
            {
                Debug.Log("Data already loaded, processing rover models");
                ProcessLoadedRoverData();
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (dataManager != null)
        {
            dataManager.OnDataLoadingComplete -= OnDataLoadingComplete;
            dataManager.OnLoadingError -= OnDataLoadingError;
        }
    }
    
    private void OnDataLoadingComplete()
    {
        Debug.Log("Data loading complete, processing rover models");
        ProcessLoadedRoverData();
    }
    
    private void OnDataLoadingError(string errorMessage)
    {
        Debug.LogError($"Error loading data: {errorMessage}");
        OnRoverLoadingError?.Invoke($"Failed to load rover data: {errorMessage}");
    }
    
    private void ProcessLoadedRoverData()
    {
        availableRoverModels.Clear();
        
        if (dataManager.RoverConfig == null || dataManager.RoverConfig.rovers == null)
        {
            Debug.LogError("No rover configuration data found!");
            OnRoverLoadingError?.Invoke("No rover configuration data found");
            return;
        }
        
        foreach (RoverModel roverModel in dataManager.RoverConfig.rovers)
        {
            if (string.IsNullOrEmpty(roverModel.id))
            {
                Debug.LogWarning("Found rover with missing ID, skipping");
                continue;
            }
            
            availableRoverModels[roverModel.id] = roverModel;
            
            if (logDetailedInfo)
            {
                LogRoverDetails(roverModel);
            }
        }
        
        Debug.Log($"Loaded {availableRoverModels.Count} rover models");
        
        // Set default rover as the active model
        // TODO: Load default rover ID from simulation settings
        //string defaultRoverId = dataManager.SimulationSettings?.gameplay?.defaultRover;
        string defaultRoverId = "001";
        if (!string.IsNullOrEmpty(defaultRoverId) && availableRoverModels.ContainsKey(defaultRoverId))
        {
            SetActiveRoverModel(defaultRoverId);
        }
        else if (availableRoverModels.Count > 0)
        {
            SetActiveRoverModel(availableRoverModels.Keys.First());
        }
        
        // Notify listeners that rover models are loaded
        OnRoverModelsLoaded?.Invoke();
    }
    
    private void LogRoverDetails(RoverModel rover)
    {
        Debug.Log($"Rover Model: {rover.id}");
        Debug.Log($"  Chassis Mass: {rover.chassis.mass} kg");
        Debug.Log($"  Power: Battery {rover.powerSystem.batteryCapacity} mAh, " +
                 $"RTG {rover.powerSystem.rtgPower}W");
        Debug.Log($"  Mobility: {rover.systems.mobility.wheelsNumber} wheels, " +
                 $"{rover.systems.mobility.wheelTorque} torque, " +
                 $"{rover.systems.mobility.wheelHorsepower} HP");
        Debug.Log($"  Science: {rover.systems.science.slots} slots");
    }
    
    public bool SetActiveRoverModel(string roverId)
    {
        if (string.IsNullOrEmpty(roverId) || !availableRoverModels.ContainsKey(roverId))
        {
            Debug.LogError($"Rover with ID '{roverId}' not found");
            return false;
        }
        
        activeRoverModel = availableRoverModels[roverId];
        Debug.Log($"Active rover model set to {roverId}");
        return true;
    }
    
    public Dictionary<string, RoverModel> GetAllRoverModels()
    {
        return availableRoverModels;
    }
    
    public RoverModel GetRoverModelById(string roverId)
    {
        if (string.IsNullOrEmpty(roverId) || !availableRoverModels.ContainsKey(roverId))
        {
            Debug.LogWarning($"Rover model with ID '{roverId}' not found");
            return null;
        }
        
        return availableRoverModels[roverId];
    }
    
    public RoverModel GetActiveRoverModel()
    {
        return activeRoverModel;
    }

    public DataManager GetDataManager()
    {
        return dataManager;
    }
    
    public GameObject InstantiateRover(GameObject roverPrefab, string roverId, Vector3 position, Quaternion rotation)
    {
        // Get the rover model
        RoverModel roverModel = GetRoverModelById(roverId);
        if (roverModel == null || roverPrefab == null)
        {
            Debug.LogError("Cannot instantiate rover: Invalid model or prefab");
            return null;
        }
        
        // Instantiate the rover prefab
        GameObject roverInstance = Instantiate(roverPrefab, position, rotation);
        
        // Configure the rover with the model data
        ConfigureRoverInstance(roverInstance, roverModel);
        
        return roverInstance;
    }
    
    private void ConfigureRoverInstance(GameObject roverInstance, RoverModel roverModel)
    {
        if (roverInstance == null || roverModel == null)
            return;
            
        // Get the RoverController component
        RoverController roverController = roverInstance.GetComponent<RoverController>();
        if (roverController == null)
        {
            Debug.LogError("RoverController component not found on rover instance");
            return;
        }
        
        // Set the rover model reference
        roverController.roverModel = roverModel;
        
        Debug.Log($"Configured rover instance with model: {roverModel.id}");
    }
}
