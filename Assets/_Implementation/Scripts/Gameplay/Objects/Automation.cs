using UnityEngine;

[System.Serializable]
public class Automation
{

    public long id;
    public Vector3 position;
    public string nick;
    public int convertedResourceType;
    public int ownerGadgetsConsumptionTimestamp;
    public float convertedEnergyAmount;
    public float convertedBiomassAmount;
    public float convertedFuelAmount;

    public Automation(long id, Vector3 position, string nick, int convertedResourceType, int ownerGadgetsConsumptionTimestamp, float convertedEnergyAmount, float convertedBiomassAmount, float convertedFuelAmount)
    {
        this.id = id;
        this.position = position;
        this.nick = nick;
        this.convertedResourceType = convertedResourceType;
        this.ownerGadgetsConsumptionTimestamp = ownerGadgetsConsumptionTimestamp;
        this.convertedEnergyAmount = convertedEnergyAmount;
        this.convertedBiomassAmount = convertedBiomassAmount;
        this.convertedFuelAmount = convertedFuelAmount;
    }

}
