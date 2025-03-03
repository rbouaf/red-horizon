using UnityEngine;

[CreateAssetMenu(fileName = "RoverData", menuName = "ScriptableObjects/Rover")]
public class RoverModel : ScriptableObject
{
    public string id;
    public Chassis chassis;
    public PowerSystem powerSystem;
    public Systems systems;
    public DamageModel damageModel;
}

[System.Serializable]
public class Chassis
{
    public float mass; // kg
}

[System.Serializable]
public class PowerSystem
{
    public float batteryCapacity;
    public float rtgPower;
    public SolarPanel solarPanel;
}

[System.Serializable]
public struct SolarPanel
{
    public float area;
    public float efficiency;
}

[System.Serializable]
public struct Systems
{
    public Mobility mobility;
    public Science science;
}

[System.Serializable]
public struct Mobility
{
    public int wheelsNumber;
    public float wheelTorque;
    public float wheelHorsepower;
}

[System.Serializable]
public struct Science
{
    public int slots;
}

[System.Serializable]
public class DamageModel
{
    public EnvironmentalFactors environmentalFactors;
    public FailureModes failureModes;
}

[System.Serializable]
public struct EnvironmentalFactors
{
    public float dust;
    public float temperature;
    public float vibration;
}

[System.Serializable]
public struct FailureModes
{
    public float mechanical;
    public float electrical;
}
