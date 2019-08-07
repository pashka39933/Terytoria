using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AutomationPopup : MonoBehaviour
{

    public static AutomationPopup instance;
    AutomationPopup() { instance = this; }

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

    private AutomationController automation;

    public CanvasGroup ownerView, commonView;
    public Text gadgetsAmount, biomassAmount, energyAmount, fuelAmount, commonNick;
    public Image ownerConversionFrom, commonConversionFrom;
    public Sprite energySprite, biomassSprite, gadgetsSprite, fuelSprite;
    public CanvasGroup conversionButton, gadgetsButton, biomassButton, energyButton, fuelButton;

    public void Open(Automation automation)
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        commonView.alpha = (automation.nick != PlayerPrefs.GetString(AppConstants.NickTag)) ? 1 : 0;
        commonView.interactable = (automation.nick != PlayerPrefs.GetString(AppConstants.NickTag));
        commonView.blocksRaycasts = (automation.nick != PlayerPrefs.GetString(AppConstants.NickTag));
        commonView.transform.localScale = Vector3.one;

        ownerView.alpha = (automation.nick == PlayerPrefs.GetString(AppConstants.NickTag)) ? 1 : 0;
        ownerView.interactable = (automation.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        ownerView.blocksRaycasts = (automation.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        ownerView.transform.localScale = Vector3.one;

        StartCoroutine(UpdateCoroutine(AutomationsController.instance.automations.FindIndex(x => x.automationData.id == automation.id)));

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

    IEnumerator UpdateCoroutine(int automationIndex)
    {
        while (true)
        {
            automation = AutomationsController.instance.automations[automationIndex];
            UpdatePopup();
            yield return new WaitForEndOfFrame();
            if (Input.GetKey(KeyCode.Escape))
                Close();
        }

    }

    private void UpdatePopup()
    {
        float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => Vector3.Distance(x.wellData.position, automation.automationData.position) < AppConstants.WellsBonusDistance).Count;
        wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
        wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;

        int ts = (int)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        float gadgets = wellsBonus * Mathf.Abs(automation.automationData.ownerGadgetsConsumptionTimestamp - ts) * AppConstants.AutomationGadgetsProductionConstant;
        gadgets = gadgets < AppConstants.AutomationSingleResourceCapacity ? gadgets : AppConstants.AutomationSingleResourceCapacity;

        gadgetsAmount.text = ResourcesController.FormatNumber(gadgets) + (automation.TerritoryBoosted > 1 ? " (<color=lime>x" + automation.TerritoryBoosted + "</color>)" : "");
        gadgetsButton.GetComponent<Button>().onClick.RemoveAllListeners();
        gadgetsButton.GetComponent<Button>().onClick.AddListener(() => { CollectGadgets(gadgets); });

        biomassAmount.text = ResourcesController.FormatNumber(automation.automationData.convertedBiomassAmount) + (automation.TerritoryBoosted > 1 ? " (<color=lime>x" + automation.TerritoryBoosted + "</color>)" : "");
        biomassButton.GetComponent<Button>().onClick.RemoveAllListeners();
        biomassButton.GetComponent<Button>().onClick.AddListener(() => { CollectBiomass(automation.automationData.convertedBiomassAmount); });

        energyAmount.text = ResourcesController.FormatNumber(automation.automationData.convertedEnergyAmount) + (automation.TerritoryBoosted > 1 ? " (<color=lime>x" + automation.TerritoryBoosted + "</color>)" : "");
        energyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        energyButton.GetComponent<Button>().onClick.AddListener(() => { CollectEnergy(automation.automationData.convertedEnergyAmount); });

        fuelAmount.text = ResourcesController.FormatNumber(automation.automationData.convertedFuelAmount) + (automation.TerritoryBoosted > 1 ? " (<color=lime>x" + automation.TerritoryBoosted + "</color>)" : "");
        fuelButton.GetComponent<Button>().onClick.RemoveAllListeners();
        fuelButton.GetComponent<Button>().onClick.AddListener(() => { CollectFuel(automation.automationData.convertedFuelAmount); });

        ownerConversionFrom.sprite = automation.automationData.convertedResourceType == (int)AppConstants.ResourceType.ENERGY ? energySprite : automation.automationData.convertedResourceType == (int)AppConstants.ResourceType.BIOMASS ? biomassSprite : automation.automationData.convertedResourceType == (int)AppConstants.ResourceType.GADGETS ? gadgetsSprite : fuelSprite;
        commonConversionFrom.sprite = automation.automationData.convertedResourceType == (int)AppConstants.ResourceType.ENERGY ? energySprite : automation.automationData.convertedResourceType == (int)AppConstants.ResourceType.BIOMASS ? biomassSprite : automation.automationData.convertedResourceType == (int)AppConstants.ResourceType.GADGETS ? gadgetsSprite : fuelSprite;
        conversionButton.GetComponent<Button>().onClick.RemoveAllListeners();
        conversionButton.GetComponent<Button>().onClick.AddListener(() => { ChangeConversion(); });

        commonNick.text = automation.automationData.nick;
    }

    private void ChangeConversion()
    {
        Automation automationData = AutomationsController.instance.automations.Find(x => x.automationData.id == automation.automationData.id).automationData;
        int resourceType = ownerConversionFrom.sprite == energySprite ? (int)AppConstants.ResourceType.BIOMASS : ownerConversionFrom.sprite == biomassSprite ? (int)AppConstants.ResourceType.FUEL : (int)AppConstants.ResourceType.ENERGY;
        conversionButton.alpha = 0.5f;
        conversionButton.interactable = false;
        conversionButton.GetComponent<Button>().interactable = false;
        Utils.Web.PostValues(
            AppConstants.ChangeAutomationConversionUrl,
            new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("ID", automation.automationData.id.ToString()),
                new KeyValuePair<string, string>("CONVERTED_RESOURCE_TYPE", resourceType.ToString())
            }),
            (code, response) =>
            {
                if (code == 200)
                {
                    ownerConversionFrom.sprite = ownerConversionFrom.sprite == energySprite ? biomassSprite : ownerConversionFrom.sprite == biomassSprite ? fuelSprite : energySprite;
                    automationData.convertedResourceType = resourceType;
                }
                else
                {
                    BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                }

                conversionButton.alpha = 1f;
                conversionButton.interactable = true;
                conversionButton.GetComponent<Button>().interactable = true;
                UpdatePopup();
            }
        );
    }

    private void CollectGadgets(float amount)
    {
        Automation automationData = AutomationsController.instance.automations.Find(x => x.automationData.id == automation.automationData.id).automationData;
        if (amount.Equals(0))
            return;
        gadgetsButton.alpha = 0.5f;
        gadgetsButton.interactable = false;
        gadgetsButton.GetComponent<Button>().interactable = false;
        Utils.Web.PostValues(
            AppConstants.CollectAutomationGadgetsUrl,
            new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("ID", automation.automationData.id.ToString())
            }),
            (code, response) =>
            {
                if (code == 200)
                {
                    ResourcesController.instance.AddGadgets(amount * (automation.TerritoryBoosted > 1 ? automation.TerritoryBoosted : 1));
                    automationData.ownerGadgetsConsumptionTimestamp = (int)Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "ts");
                }
                else
                {
                    BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                }

                gadgetsButton.alpha = 1f;
                gadgetsButton.interactable = true;
                gadgetsButton.GetComponent<Button>().interactable = true;
                UpdatePopup();
            }
        );
    }

    private void CollectBiomass(float amount)
    {
        Automation automationData = AutomationsController.instance.automations.Find(x => x.automationData.id == automation.automationData.id).automationData;
        if (amount.Equals(0))
            return;
        biomassButton.alpha = 0.5f;
        biomassButton.interactable = false;
        biomassButton.GetComponent<Button>().interactable = false;
        Utils.Web.PostValues(
            AppConstants.CollectAutomationBiomassUrl,
            new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("ID", automation.automationData.id.ToString())
            }),
            (code, response) =>
            {
                if (code == 200)
                {
                    ResourcesController.instance.AddBiomass(amount * (automation.TerritoryBoosted > 1 ? automation.TerritoryBoosted : 1));
                    automationData.convertedBiomassAmount = 0;
                }
                else
                {
                    BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                }

                biomassButton.alpha = 1f;
                biomassButton.interactable = true;
                biomassButton.GetComponent<Button>().interactable = true;
                UpdatePopup();
            }
        );
    }

    private void CollectEnergy(float amount)
    {
        Automation automationData = AutomationsController.instance.automations.Find(x => x.automationData.id == automation.automationData.id).automationData;
        if (amount.Equals(0))
            return;
        energyButton.alpha = 0.5f;
        energyButton.interactable = false;
        energyButton.GetComponent<Button>().interactable = false;
        Utils.Web.PostValues(
            AppConstants.CollectAutomationEnergyUrl,
            new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("ID", automation.automationData.id.ToString())
            }),
            (code, response) =>
            {
                if (code == 200)
                {
                    ResourcesController.instance.AddEnergy(amount * (automation.TerritoryBoosted > 1 ? automation.TerritoryBoosted : 1));
                    automationData.convertedEnergyAmount = 0;
                }
                else
                {
                    BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                }

                energyButton.alpha = 1f;
                energyButton.interactable = true;
                energyButton.GetComponent<Button>().interactable = true;
                UpdatePopup();
            }
        );
    }

    private void CollectFuel(float amount)
    {
        Automation automationData = AutomationsController.instance.automations.Find(x => x.automationData.id == automation.automationData.id).automationData;
        if (amount.Equals(0))
            return;
        fuelButton.alpha = 0.5f;
        fuelButton.interactable = false;
        fuelButton.GetComponent<Button>().interactable = false;
        Utils.Web.PostValues(
            AppConstants.CollectAutomationFuelUrl,
            new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("ID", automation.automationData.id.ToString())
            }),
            (code, response) =>
            {
                if (code == 200)
                {
                    ResourcesController.instance.AddFuel(amount * (automation.TerritoryBoosted > 1 ? automation.TerritoryBoosted : 1));
                    automationData.convertedFuelAmount = 0;
                }
                else
                {
                    BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                }

                fuelButton.alpha = 1f;
                fuelButton.interactable = true;
                fuelButton.GetComponent<Button>().interactable = true;
                UpdatePopup();
            }
        );
    }

}