using UnityEngine;

[System.Serializable]
public class Deposit
{
    
    public string id;
    public Vector3 position;
    public string previewUrl;
    public string name;
    public float receiverEnergy;
    public float receiverBiomass;
    public float receiverGadgets;
    public float receiverFuel;

    public Deposit(string id, Vector3 position, string previewUrl, string name, float receiverEnergy, float receiverBiomass, float receiverGadgets, float receiverFuel)
    {
        this.id = id;
        this.position = position;
        this.previewUrl = previewUrl;
        this.name = name;
        this.receiverEnergy = receiverEnergy;
        this.receiverBiomass = receiverBiomass;
        this.receiverGadgets = receiverGadgets;
        this.receiverFuel = receiverFuel;
    }

}
