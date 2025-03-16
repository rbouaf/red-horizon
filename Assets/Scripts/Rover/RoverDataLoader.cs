using UnityEngine;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class RoverDataLoader : MonoBehaviour
{
    [System.Serializable]
    public class RoverDataList
    {
        public List<RoverModel> rovers;
    }

    private void Start()
    {
        string yamlPath = Path.Combine(Application.streamingAssetsPath, "configs/rover_config.yaml");

        if (File.Exists(yamlPath))
        {
            string yaml = File.ReadAllText(yamlPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance) // Matches YAML format
                .Build();

            RoverDataList roverData = deserializer.Deserialize<RoverDataList>(yaml);
            foreach (var rover in roverData.rovers)
            {
                Debug.Log($"Loaded Rover ID: {rover.id}, Mass: {rover.chassis.mass} kg");
            }
        }
        else
        {
            Debug.LogError("YAML file not found!");
        }
    }
}
