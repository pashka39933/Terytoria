using System.Collections.Generic;
using UnityEngine;

public class GeneratorsController : MonoBehaviour
{

    public static GeneratorsController instance;
    GeneratorsController() { instance = this; }

    public GameObject generatorPrefab;
    public Material energy, biomass, gadgets, fuel;

    public List<GeneratorController> generators = new List<GeneratorController>();

    // Adding generator method
    public void AddGenerator(Generator generator)
    {
        if (generators.FindIndex(x => x.generatorData.id.Equals(generator.id)) > -1)
        {
            generators.Find(x => x.generatorData.id.Equals(generator.id)).generatorData = generator;
            return;
        }

        GeneratorController generatorController = Instantiate(generatorPrefab, this.transform).GetComponent<GeneratorController>();
        generatorController.generatorData = generator;
        generators.Add(generatorController);
        generatorController.name = generator.nick;
        generatorController.transform.position = new Vector3(generator.position.x, 0, generator.position.z);
        generatorController.GetComponent<CircleLineRenderer>().CreatePoints(16, AppConstants.GeneratorRange / 12f, 0.1f);
        generatorController.transform.GetChild(0).gameObject.SetActive(generator.nick == PlayerPrefs.GetString(AppConstants.NickTag) && generator.batteryLevel > 0);
        generatorController.transform.GetChild(1).gameObject.SetActive(generator.nick == PlayerPrefs.GetString(AppConstants.NickTag) && generator.converterLevel > 0);

        generatorController.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material =
                               generatorController.generatorData.converterFromResource == (int)AppConstants.ResourceType.ENERGY ? energy :
                               generatorController.generatorData.converterFromResource == (int)AppConstants.ResourceType.BIOMASS ? biomass :
                               generatorController.generatorData.converterFromResource == (int)AppConstants.ResourceType.GADGETS ? gadgets :
                               fuel;
        generatorController.transform.GetChild(1).GetChild(1).GetComponent<MeshRenderer>().material =
                               generatorController.generatorData.converterToResource == (int)AppConstants.ResourceType.ENERGY ? energy :
                               generatorController.generatorData.converterToResource == (int)AppConstants.ResourceType.BIOMASS ? biomass :
                               generatorController.generatorData.converterToResource == (int)AppConstants.ResourceType.GADGETS ? gadgets :
                               fuel;

        generatorController.GetComponent<SphereCollider>().enabled = true;
    }

    // Creating generator method
    public void CreateGeneratorAtUserPosition(bool confirmed = false)
    {
        if (PlayerPrefs.GetInt(AppConstants.GeneratorCreatedTag) > 0)
        {
            BannerController.instance.showBannerWithText(true, "Posiadasz już swój generator", true);
            return;
        }
        if (!PlayerColliderController.instance.CanPlaceGeneratorHere())
        {
            BannerController.instance.showBannerWithText(true, "Zbyt blisko innego generatora! Oddal się kilka metrów", true);
            return;
        }
        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { CreateGeneratorAtUserPosition(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz tutaj umieścić swój generator?");
            return;
        }

        Utils.Web.PostValues(AppConstants.AddGeneratorUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
                new KeyValuePair<string, string>("LAT", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).latitude.ToString()),
                new KeyValuePair<string, string>("LON", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).longitude.ToString())
        }), (code, response) =>
        {
            if (code == 200)
            {
                long id = Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "id");
                Generator generator = new Generator(id, PlayerColliderController.instance.transform.position, PlayerPrefs.GetString(AppConstants.NickTag), 1, 0, 0, (int)AppConstants.ResourceType.ENERGY, (int)AppConstants.ResourceType.ENERGY, 0);
                AddGenerator(generator);
                PlayerPrefs.SetInt(AppConstants.GeneratorCreatedTag, 1);
                PlayerPrefs.Save();

                GameInitializer.instance.PlaceGeneratorSuccess();
            }
            else
            {
                Debug.Log(response);
                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
            }
        });
    }

    // Creating battery method
    public void CreateBattery(bool confirmed = false)
    {
        if (DownButtonController.instance.standardMenuButtonsVisible)
            DownButtonController.instance.ToggleStandardMenu();

        GeneratorController ownGenerator = PlayerColliderController.instance.GetOwnGenerator(true);
        if (ownGenerator == null)
        {
            BannerController.instance.showBannerWithText(true, "Aby dołączyć baterię, musisz znajdować się w pobliżu własnego generatora", true);
            return;
        }
        if (PlayerPrefs.GetInt(AppConstants.BatteryCreatedTag) > 0)
        {
            BannerController.instance.showBannerWithText(true, "Twój generator posiada już baterię", true);
            return;
        }
        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { CreateBattery(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz dołączyć baterię do swojego generatora za " + AppConstants.GeneratorBatteryPurchaseCost + " energii ?");
            return;
        }
        if (PlayerPrefs.GetFloat(AppConstants.EnergyTag) < AppConstants.GeneratorBatteryPurchaseCost)
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało energii", true);
            return;
        }
        ResourcesController.instance.AddEnergy(-AppConstants.GeneratorBatteryPurchaseCost);

        Utils.Web.PostValues(AppConstants.AddBatteryUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID)
        }), (code, response) =>
        {
            if (code == 200)
            {
                ownGenerator.transform.GetChild(0).gameObject.SetActive(true);
                ownGenerator.generatorData.batteryLevel = 1;
                PlayerPrefs.SetInt(AppConstants.BatteryCreatedTag, 1);
                PlayerPrefs.Save();
            }
            else
            {
                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
            }
        });
    }

    // Creating converter method
    public void CreateConverter(bool confirmed = false)
    {
        if (DownButtonController.instance.standardMenuButtonsVisible)
            DownButtonController.instance.ToggleStandardMenu();

        GeneratorController ownGenerator = PlayerColliderController.instance.GetOwnGenerator(true);
        if (ownGenerator == null)
        {
            BannerController.instance.showBannerWithText(true, "Aby dołączyć konwerter, musisz znajdować się w pobliżu własnego generatora", true);
            return;
        }
        if (PlayerPrefs.GetInt(AppConstants.ConverterCreatedTag) > 0)
        {
            BannerController.instance.showBannerWithText(true, "Twój generator posiada już konwerter", true);
            return;
        }
        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { CreateConverter(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz dołączyć konwerter do swojego generatora za " + AppConstants.GeneratorConverterPurchaseCost + " energii ?");
            return;
        }
        if (PlayerPrefs.GetFloat(AppConstants.EnergyTag) < AppConstants.GeneratorConverterPurchaseCost)
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało energii", true);
            return;
        }
        ResourcesController.instance.AddEnergy(-AppConstants.GeneratorConverterPurchaseCost);

        Utils.Web.PostValues(AppConstants.AddConverterUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
                new KeyValuePair<string, string>("FROM_RESOURCE", ((int)AppConstants.ResourceType.ENERGY).ToString()),
                new KeyValuePair<string, string>("TO_RESOURCE", (PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? (int)AppConstants.ResourceType.BIOMASS : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? (int)AppConstants.ResourceType.GADGETS : (int)AppConstants.ResourceType.FUEL).ToString()),
        }), (code, response) =>
        {
            if (code == 200)
            {
                ownGenerator.generatorData.converterLevel = 1;
                ownGenerator.generatorData.converterFromResource = (int)AppConstants.ResourceType.ENERGY;
                ownGenerator.generatorData.converterToResource = (PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? (int)AppConstants.ResourceType.BIOMASS : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? (int)AppConstants.ResourceType.GADGETS : (int)AppConstants.ResourceType.FUEL);
                ownGenerator.transform.GetChild(1).gameObject.SetActive(true);

                ownGenerator.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material =
                                        ownGenerator.generatorData.converterFromResource == (int)AppConstants.ResourceType.ENERGY ? energy :
                                        ownGenerator.generatorData.converterFromResource == (int)AppConstants.ResourceType.BIOMASS ? biomass :
                                        ownGenerator.generatorData.converterFromResource == (int)AppConstants.ResourceType.GADGETS ? gadgets :
                                        fuel;
                ownGenerator.transform.GetChild(1).GetChild(1).GetComponent<MeshRenderer>().material =
                                        ownGenerator.generatorData.converterToResource == (int)AppConstants.ResourceType.ENERGY ? energy :
                                        ownGenerator.generatorData.converterToResource == (int)AppConstants.ResourceType.BIOMASS ? biomass :
                                        ownGenerator.generatorData.converterToResource == (int)AppConstants.ResourceType.GADGETS ? gadgets :
                                        fuel;

                PlayerPrefs.SetInt(AppConstants.ConverterCreatedTag, 1);
                PlayerPrefs.Save();
            }
            else
            {
                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
            }
        });
    }

}