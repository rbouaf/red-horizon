using UnityEngine;

public enum RockType { Basaltic, Andesitic, Sedimentary, Clay, Quartz, Volcaniclastic }

public class SampleInfo : MonoBehaviour
{
    public RockType rockType;

    // rarity values for each rock type.
    public float basalticConcentration = 0.9f;
    public float andesiticConcentration = 0.7f;
    public float sedimentaryConcentration = 0.3f;
    public float clayConcentration = 0.4f;
    public float quartzConcentration = 0.05f;
    public float volcaniclasticConcentration = 0.1f;

    // rarity score (1 - abundance)
    public float GetRarity()
    {
        switch (rockType)
        {
            case RockType.Basaltic: return 1f - basalticConcentration;
            case RockType.Andesitic: return 1f - andesiticConcentration;
            case RockType.Sedimentary: return 1f - sedimentaryConcentration;
            case RockType.Clay: return 1f - clayConcentration;
            case RockType.Quartz: return 1f - quartzConcentration;
            case RockType.Volcaniclastic: return 1f - volcaniclasticConcentration;
            default: return 0f;
        }
    }
}
