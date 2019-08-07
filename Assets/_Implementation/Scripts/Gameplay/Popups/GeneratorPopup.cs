using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class GeneratorPopup : MonoBehaviour
{

    public static GeneratorPopup instance;
    GeneratorPopup() { instance = this; }

    bool opened = false;

    private void Awake()
    {
        this.transform.localScale = Vector3.zero;
        if (this.GetComponent<CanvasGroup>() != null)
        {
            this.GetComponent<CanvasGroup>().alpha = 0;
            this.GetComponent<CanvasGroup>().blocksRaycasts = false;
            this.GetComponent<CanvasGroup>().interactable = false;
        }
    }

    private GeneratorController generator;

    public Animation mainPanelAnim, generatorPanelAnim, batteryPanelAnim, converterPanelAnim, conversionPanelAnim;
    public CanvasGroup generatorButton, batteryButton, converterButton;
    public CanvasGroup mainPanel, generatorPanel, batteryPanel, converterPanel, conversionPanel, commonView;
    public Text generatorLevel, batteryLevel, converterLevel, commonLevel, commonNick;
    public Text generatorCost, batteryCost, converterCost, conversionChangeCost;
    public Image conversionFrom, conversionTo, conversionButtonFrom, conversionButtonTo;
    public Sprite energySprite, biomassSprite, gadgetsSprite, fuelSprite;
    public CanvasGroup levelupGeneratorButton, levelupBatteryButton, levelupConverterButton, changeConversionButton;

    public void Open(Generator generator)
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        generatorButton.alpha = 1f;
        generatorButton.interactable = true;
        generatorButton.blocksRaycasts = true;
        batteryButton.alpha = generator.batteryLevel > 0 ? 1f : 0.5f;
        batteryButton.interactable = generator.batteryLevel > 0;
        batteryButton.blocksRaycasts = generator.batteryLevel > 0;
        converterButton.alpha = generator.converterLevel > 0 ? 1f : 0.5f;
        converterButton.interactable = generator.converterLevel > 0;
        converterButton.blocksRaycasts = generator.converterLevel > 0;

        commonView.alpha = (generator.nick != PlayerPrefs.GetString(AppConstants.NickTag)) ? 1 : 0;
        commonView.interactable = (generator.nick != PlayerPrefs.GetString(AppConstants.NickTag));
        commonView.blocksRaycasts = (generator.nick != PlayerPrefs.GetString(AppConstants.NickTag));
        commonView.transform.localScale = Vector3.one;

        mainPanel.alpha = (generator.nick == PlayerPrefs.GetString(AppConstants.NickTag)) ? 1 : 0;
        mainPanel.interactable = (generator.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        mainPanel.blocksRaycasts = (generator.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        mainPanel.transform.localScale = Vector3.one;

        generatorPanel.alpha = 0;
        generatorPanel.interactable = false;
        generatorPanel.blocksRaycasts = false;
        generatorPanel.transform.localScale = Vector3.one;

        batteryPanel.alpha = 0;
        batteryPanel.interactable = false;
        batteryPanel.blocksRaycasts = false;
        batteryPanel.transform.localScale = Vector3.one;

        converterPanel.alpha = 0;
        converterPanel.interactable = false;
        converterPanel.blocksRaycasts = false;
        converterPanel.transform.localScale = Vector3.one;

        conversionPanel.alpha = 0;
        conversionPanel.interactable = false;
        conversionPanel.blocksRaycasts = false;
        conversionPanel.transform.localScale = Vector3.one;

        StartCoroutine(UpdateCoroutine(GeneratorsController.instance.generators.FindIndex(x => x.generatorData.id == generator.id)));

        this.GetComponent<Animation>().Play("popupIn");
    }

    public void Close()
    {
        if (!opened)
            return;
        opened = false;

        UIController.instance.uiActive = false;

        StopAllCoroutines();

        this.GetComponent<Animation>().Play("popupOut");
    }

    public void OpenGeneratorPanel()
    {
        mainPanelAnim.Play("popupOut");
        generatorPanelAnim.Play("popupIn");
    }

    public void OpenBatteryPanel()
    {
        mainPanelAnim.Play("popupOut");
        batteryPanelAnim.Play("popupIn");
    }

    public void OpenConverterPanel()
    {
        if (mainPanel.alpha > 0)
            mainPanelAnim.Play("popupOut");
        if (conversionPanel.alpha > 0)
            conversionPanelAnim.Play("popupOut");
        converterPanelAnim.Play("popupIn");
    }

    public void OpenConversionPanel()
    {
        Generator generatorData = GeneratorsController.instance.generators.Find(x => x.generatorData.id == generator.generatorData.id).generatorData;
        conversionButtonFrom.sprite = generatorData.converterFromResource == (int)AppConstants.ResourceType.ENERGY ? energySprite : generatorData.converterFromResource == (int)AppConstants.ResourceType.BIOMASS ? biomassSprite : generatorData.converterFromResource == (int)AppConstants.ResourceType.GADGETS ? gadgetsSprite : fuelSprite;
        conversionButtonTo.sprite = generatorData.converterToResource == (int)AppConstants.ResourceType.ENERGY ? energySprite : generatorData.converterToResource == (int)AppConstants.ResourceType.BIOMASS ? biomassSprite : generatorData.converterToResource == (int)AppConstants.ResourceType.GADGETS ? gadgetsSprite : fuelSprite;

        converterPanelAnim.Play("popupOut");
        conversionPanelAnim.Play("popupIn");
    }

    IEnumerator UpdateCoroutine(int generatorIndex)
    {
        while (true)
        {
            generator = GeneratorsController.instance.generators[generatorIndex];
            UpdatePopup();
            yield return new WaitForEndOfFrame();
            if (Input.GetKey(KeyCode.Escape))
                Close();
        }
    }

    private void UpdatePopup()
    {
        generatorLevel.text = "Poziom " + generator.generatorData.level.ToString() + System.Environment.NewLine + "Wydajność: ~ " + (AppConstants.GeneratorOwnerEnergyConstant * generator.generatorData.level).ToString() + " E/s" + (generator.TerritoryBoosted > 1 ? " (<color=lime>x" + generator.TerritoryBoosted + "</color>)" : "");
        batteryLevel.text = "Poziom " + generator.generatorData.batteryLevel.ToString() + System.Environment.NewLine + "Pojemność: ~ " + (generator.generatorData.batteryLevel * AppConstants.GeneratorBatteryCapacityConstant).ToString() + " E" + (generator.TerritoryBoosted > 1 ? " (<color=lime>x" + generator.TerritoryBoosted + "</color>)" : "");
        converterLevel.text = "Poziom " + generator.generatorData.converterLevel.ToString() + System.Environment.NewLine + "Wydajność: ~ " + (generator.generatorData.converterLevel * AppConstants.GeneratorConverterEfficiencyConstant) + " E/s" + (generator.TerritoryBoosted > 1 ? " (<color=lime>x" + generator.TerritoryBoosted + "</color>)" : "");
        commonLevel.text = "Poziom " + generator.generatorData.level.ToString() + System.Environment.NewLine + "Wydajność: ~ " + (AppConstants.GeneratorCommonEnergyConstant * generator.generatorData.level).ToString() + " E/s" + (generator.TerritoryBoosted > 1 ? " (<color=lime>x" + generator.TerritoryBoosted + "</color>)" : "");

        int generatorUpgradeCost = 0;
        for (int i = 0; i < generator.generatorData.level + 1; i++)
        {
            generatorUpgradeCost += (i * AppConstants.GeneratorUpgradeConstant);
        }
        generatorCost.text = ResourcesController.FormatNumber(generatorUpgradeCost);
        levelupGeneratorButton.GetComponent<Button>().onClick.RemoveAllListeners();
        levelupGeneratorButton.GetComponent<Button>().onClick.AddListener(() => { LevelUpGenerator(generatorUpgradeCost); });

        int batteryUpgradeCost = 0;
        for (int i = 0; i < generator.generatorData.batteryLevel + 1; i++)
        {
            batteryUpgradeCost += (i * AppConstants.GeneratorBatteryUpgradeConstant);
        }
        batteryCost.text = ResourcesController.FormatNumber(batteryUpgradeCost);
        levelupBatteryButton.GetComponent<Button>().onClick.RemoveAllListeners();
        levelupBatteryButton.GetComponent<Button>().onClick.AddListener(() => { LevelUpBattery(batteryUpgradeCost); });

        int converterUpgradeCost = 0;
        for (int i = 0; i < generator.generatorData.converterLevel + 1; i++)
        {
            converterUpgradeCost += (i * AppConstants.GeneratorConverterUpgradeConstant);
        }
        converterCost.text = ResourcesController.FormatNumber(converterUpgradeCost);
        levelupConverterButton.GetComponent<Button>().onClick.RemoveAllListeners();
        levelupConverterButton.GetComponent<Button>().onClick.AddListener(() => { LevelUpConverter(converterUpgradeCost); });

        conversionFrom.sprite = generator.generatorData.converterFromResource == (int)AppConstants.ResourceType.ENERGY ? energySprite : generator.generatorData.converterFromResource == (int)AppConstants.ResourceType.BIOMASS ? biomassSprite : generator.generatorData.converterFromResource == (int)AppConstants.ResourceType.GADGETS ? gadgetsSprite : fuelSprite;
        conversionTo.sprite = generator.generatorData.converterToResource == (int)AppConstants.ResourceType.ENERGY ? energySprite : generator.generatorData.converterToResource == (int)AppConstants.ResourceType.BIOMASS ? biomassSprite : generator.generatorData.converterToResource == (int)AppConstants.ResourceType.GADGETS ? gadgetsSprite : fuelSprite;
        int conversionChangeCostVar = (generator.generatorData.converterChangesCount + 1) * AppConstants.GeneratorConversionChangeConstant;
        conversionChangeCost.text = ResourcesController.FormatNumber(conversionChangeCostVar);
        changeConversionButton.GetComponent<Button>().onClick.RemoveAllListeners();
        changeConversionButton.GetComponent<Button>().onClick.AddListener(() => { ChangeConversion(conversionChangeCostVar); });

        commonNick.text = generator.generatorData.nick;

    }

    public void ChangeConversionFrom()
    {
        Generator generatorData = GeneratorsController.instance.generators.Find(x => x.generatorData.id == generator.generatorData.id).generatorData;
        if (conversionButtonFrom.sprite.Equals(energySprite))
        {
            conversionButtonFrom.sprite = generatorData.converterLevel >= 10 ? biomassSprite : energySprite;
        }
        else if (conversionButtonFrom.sprite.Equals(biomassSprite))
        {
            conversionButtonFrom.sprite = generatorData.converterLevel >= 10 ? gadgetsSprite : energySprite;
        }
        else if (conversionButtonFrom.sprite.Equals(gadgetsSprite))
        {
            conversionButtonFrom.sprite = generatorData.converterLevel >= 10 ? fuelSprite : energySprite;
        }
        else if (conversionButtonFrom.sprite.Equals(fuelSprite))
        {
            conversionButtonFrom.sprite = energySprite;
        }
    }

    public void ChangeConversionTo()
    {
        Generator generatorData = GeneratorsController.instance.generators.Find(x => x.generatorData.id == generator.generatorData.id).generatorData;
        if (conversionButtonTo.sprite.Equals(energySprite))
        {
            conversionButtonTo.sprite = biomassSprite;
        }
        else if (conversionButtonTo.sprite.Equals(biomassSprite))
        {
            conversionButtonTo.sprite = gadgetsSprite;
        }
        else if (conversionButtonTo.sprite.Equals(gadgetsSprite))
        {
            conversionButtonTo.sprite = fuelSprite;
        }
        else if (conversionButtonTo.sprite.Equals(fuelSprite))
        {
            conversionButtonTo.sprite = generatorData.converterLevel >= 10 ? energySprite : biomassSprite;
        }
    }


    private void LevelUpGenerator(int cost)
    {
        Generator generatorData = GeneratorsController.instance.generators.Find(x => x.generatorData.id == generator.generatorData.id).generatorData;
        if (cost <= PlayerPrefs.GetFloat(AppConstants.EnergyTag))
        {
            levelupGeneratorButton.alpha = 0.5f;
            levelupGeneratorButton.interactable = false;
            levelupGeneratorButton.GetComponent<Button>().interactable = false;
            Utils.Web.PostValues(
                AppConstants.LevelUpGeneratorUrl,
                new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("ID", generator.generatorData.id.ToString())
                }),
                (code, response) =>
                {
                    if (code == 200)
                    {
                        ResourcesController.instance.AddEnergy(-cost);
                        generatorData.level++;
                    }
                    else
                    {
                        BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                    }

                    levelupGeneratorButton.alpha = 1f;
                    levelupGeneratorButton.interactable = true;
                    levelupGeneratorButton.GetComponent<Button>().interactable = true;
                    UpdatePopup();
                }
            );
        }
        else
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało energii aby ulepszyć generator", true);
        }
    }

    private void LevelUpBattery(int cost)
    {
        Generator generatorData = GeneratorsController.instance.generators.Find(x => x.generatorData.id == generator.generatorData.id).generatorData;
        if (cost <= PlayerPrefs.GetFloat(AppConstants.EnergyTag))
        {
            levelupBatteryButton.alpha = 0.5f;
            levelupBatteryButton.interactable = false;
            levelupBatteryButton.GetComponent<Button>().interactable = false;
            Utils.Web.PostValues(
                AppConstants.LevelUpBatteryUrl,
                new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("ID", generator.generatorData.id.ToString())
                }),
                (code, response) =>
                {
                    if (code == 200)
                    {
                        ResourcesController.instance.AddEnergy(-cost);
                        generatorData.batteryLevel++;
                    }
                    else
                    {
                        BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                    }

                    levelupBatteryButton.alpha = 1f;
                    levelupBatteryButton.interactable = true;
                    levelupBatteryButton.GetComponent<Button>().interactable = true;
                    UpdatePopup();
                }
            );
        }
        else
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało energii aby ulepszyć baterię", true);
        }
    }

    private void LevelUpConverter(int cost)
    {
        Generator generatorData = GeneratorsController.instance.generators.Find(x => x.generatorData.id == generator.generatorData.id).generatorData;
        if (cost <= PlayerPrefs.GetFloat(AppConstants.EnergyTag))
        {
            levelupConverterButton.alpha = 0.5f;
            levelupConverterButton.interactable = false;
            levelupConverterButton.GetComponent<Button>().interactable = false;
            Utils.Web.PostValues(
                AppConstants.LevelUpConverterUrl,
                new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("ID", generator.generatorData.id.ToString())
                }),
                (code, response) =>
                {
                    if (code == 200)
                    {
                        ResourcesController.instance.AddEnergy(-cost);
                        generatorData.converterLevel++;
                    }
                    else
                    {
                        BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                    }

                    levelupConverterButton.alpha = 1f;
                    levelupConverterButton.interactable = true;
                    levelupConverterButton.GetComponent<Button>().interactable = true;
                    UpdatePopup();
                }
            );
        }
        else
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało energii aby ulepszyć konwerter", true);
        }
    }

    private void ChangeConversion(int cost)
    {
        Generator generatorData = GeneratorsController.instance.generators.Find(x => x.generatorData.id == generator.generatorData.id).generatorData;
        if (cost <= PlayerPrefs.GetFloat(AppConstants.EnergyTag))
        {
            int fromResource = conversionButtonFrom.sprite == energySprite ? (int)AppConstants.ResourceType.ENERGY : conversionButtonFrom.sprite == biomassSprite ? (int)AppConstants.ResourceType.BIOMASS : conversionButtonFrom.sprite == gadgetsSprite ? (int)AppConstants.ResourceType.GADGETS : (int)AppConstants.ResourceType.FUEL;
            int toResource = conversionButtonTo.sprite == energySprite ? (int)AppConstants.ResourceType.ENERGY : conversionButtonTo.sprite == biomassSprite ? (int)AppConstants.ResourceType.BIOMASS : conversionButtonTo.sprite == gadgetsSprite ? (int)AppConstants.ResourceType.GADGETS : (int)AppConstants.ResourceType.FUEL;
            if (fromResource != toResource)
            {
                if (fromResource != generatorData.converterFromResource || toResource != generatorData.converterToResource)
                {
                    changeConversionButton.alpha = 0.5f;
                    changeConversionButton.interactable = false;
                    changeConversionButton.GetComponent<Button>().interactable = false;
                    Utils.Web.PostValues(
                        AppConstants.ChangeGeneratorConversionUrl,
                        new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("ID", generator.generatorData.id.ToString()),
                        new KeyValuePair<string, string>("FROM_RESOURCE",fromResource.ToString()),
                        new KeyValuePair<string, string>("TO_RESOURCE", toResource.ToString())
                        }),
                        (code, response) =>
                        {
                            if (code == 200)
                            {
                                ResourcesController.instance.AddEnergy(-cost);
                                generatorData.converterChangesCount++;
                                generatorData.converterFromResource = fromResource;
                                generatorData.converterToResource = toResource;
                            }
                            else
                            {
                                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                            }

                            changeConversionButton.alpha = 1f;
                            changeConversionButton.interactable = true;
                            changeConversionButton.GetComponent<Button>().interactable = true;
                            UpdatePopup();
                        }
                    );
                }
                else
                {
                    BannerController.instance.showBannerWithText(true, "Żaden zasób nie został zmieniony", true);
                }
            }
            else
            {
                BannerController.instance.showBannerWithText(true, "Wybierz różne zasoby", true);
            }
        }
        else
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało energii aby zmienić typ konwersji", true);
        }
    }
}