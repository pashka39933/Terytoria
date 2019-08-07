using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResourcesController : MonoBehaviour
{

    // Instance
    public static ResourcesController instance;
    ResourcesController() { instance = this; }

    // Resources texts
    public Text energyText, biomassText, gadgetsText, fuelText;

    // Initialization
    private void Start()
    {
        energyText.text = FormatNumber((PlayerPrefs.GetFloat(AppConstants.EnergyTag)));
        biomassText.text = FormatNumber((PlayerPrefs.GetFloat(AppConstants.BiomassTag)));
        gadgetsText.text = FormatNumber((PlayerPrefs.GetFloat(AppConstants.GadgetsTag)));
        fuelText.text = FormatNumber((PlayerPrefs.GetFloat(AppConstants.FuelTag)));
    }

    // Method to add energy resource
    public void AddEnergy(float value)
    {
        float totalAmount = PlayerPrefs.GetFloat(AppConstants.EnergyTag) + value;
        totalAmount = totalAmount < 0 ? 0 : totalAmount;
        PlayerPrefs.SetFloat(AppConstants.EnergyTag, totalAmount);
        PlayerPrefs.Save();
        energyText.text = FormatNumber((totalAmount));
    }

    // Method to add biomass resource
    public void AddBiomass(float value)
    {
        float totalAmount = PlayerPrefs.GetFloat(AppConstants.BiomassTag) + value;
        totalAmount = totalAmount < 0 ? 0 : totalAmount;
        PlayerPrefs.SetFloat(AppConstants.BiomassTag, totalAmount);
        PlayerPrefs.Save();
        biomassText.text = FormatNumber((totalAmount));
    }

    // Method to add gadgets resource
    public void AddGadgets(float value)
    {
        float totalAmount = PlayerPrefs.GetFloat(AppConstants.GadgetsTag) + value;
        totalAmount = totalAmount < 0 ? 0 : totalAmount;
        PlayerPrefs.SetFloat(AppConstants.GadgetsTag, totalAmount);
        PlayerPrefs.Save();
        gadgetsText.text = FormatNumber((totalAmount));
    }

    // Method to add fuel resource
    public void AddFuel(float value)
    {
        float totalAmount = PlayerPrefs.GetFloat(AppConstants.FuelTag) + value;
        totalAmount = totalAmount < 0 ? 0 : totalAmount;
        PlayerPrefs.SetFloat(AppConstants.FuelTag, totalAmount);
        PlayerPrefs.Save();
        fuelText.text = FormatNumber((totalAmount));
    }

    // Numbers to friendly text conversion static method
    public static string FormatNumber(float number)
    {
        if (number < 1)
            return number.ToString();

        int num = Mathf.RoundToInt(number);
        if (num >= 100000)
            return FormatNumber(num / 1000) + "K";
        if (num >= 10000)
        {
            return (num / 1000D).ToString("0.#") + "K";
        }
        return num.ToString("#,0");
    }

}