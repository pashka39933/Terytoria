using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WellPopup : MonoBehaviour
{

    public static WellPopup instance;
    WellPopup() { instance = this; }

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

    private WellController well;

    public CanvasGroup ownerView, commonView;
    public Text ownerLevel, commonLevel, commonNick;
    public Text upgradeAmount;
    public CanvasGroup levelupButton;

    public void Open(Well well)
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        commonView.alpha = (well.nick != PlayerPrefs.GetString(AppConstants.NickTag)) ? 1 : 0;
        commonView.interactable = (well.nick != PlayerPrefs.GetString(AppConstants.NickTag));
        commonView.blocksRaycasts = (well.nick != PlayerPrefs.GetString(AppConstants.NickTag));
        commonView.transform.localScale = Vector3.one;

        ownerView.alpha = (well.nick == PlayerPrefs.GetString(AppConstants.NickTag)) ? 1 : 0;
        ownerView.interactable = (well.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        ownerView.blocksRaycasts = (well.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        ownerView.transform.localScale = Vector3.one;

        StartCoroutine(UpdateCoroutine(WellsController.instance.wells.FindIndex(x => x.wellData.id == well.id)));

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

    IEnumerator UpdateCoroutine(int wellIndex)
    {
        while (true)
        {
            well = WellsController.instance.wells[wellIndex];
            UpdatePopup();
            yield return new WaitForEndOfFrame();
            if (Input.GetKey(KeyCode.Escape))
                Close();
        }

    }

    private void UpdatePopup()
    {
        float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => x.wellData.id != well.wellData.id && Vector3.Distance(x.wellData.position, well.wellData.position) < AppConstants.WellsBonusDistance).Count;
        wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
        wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;

        ownerLevel.text = "Poziom " + well.wellData.level + System.Environment.NewLine + "Wydajność ~ " + ResourcesController.FormatNumber((well.wellData.level * wellsBonus)) + " P/s" + (well.TerritoryBoosted > 1 ? " (<color=lime>x" + well.TerritoryBoosted + "</color>)" : "");
        commonLevel.text = "Poziom " + well.wellData.level + System.Environment.NewLine + "Wydajność ~" + ResourcesController.FormatNumber((AppConstants.WellCommonFuelConstant * well.wellData.level * wellsBonus)) + " P/s" + (well.TerritoryBoosted > 1 ? " (<color=lime>x" + well.TerritoryBoosted + "</color>)" : "");
        upgradeAmount.text = ResourcesController.FormatNumber(AppConstants.WellUpgradeCostConstant * well.wellData.level);

        levelupButton.GetComponent<Button>().onClick.RemoveAllListeners();
        levelupButton.GetComponent<Button>().onClick.AddListener(() => { LevelUpWell(AppConstants.WellUpgradeCostConstant * well.wellData.level); });

        commonNick.text = well.wellData.nick;
    }

    private void LevelUpWell(float cost)
    {
        Well wellData = WellsController.instance.wells.Find(x => x.wellData.id == well.wellData.id).wellData;
        if (cost <= PlayerPrefs.GetFloat(AppConstants.FuelTag))
        {
            levelupButton.alpha = 0.5f;
            levelupButton.interactable = false;
            levelupButton.GetComponent<Button>().interactable = false;
            Utils.Web.PostValues(
                AppConstants.LevelUpWellUrl,
                new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("ID", well.wellData.id.ToString())
                }),
                (code, response) =>
                {
                    if (code == 200)
                    {
                        ResourcesController.instance.AddFuel(-cost);
                        wellData.level++;
                    }
                    else
                    {
                        BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                    }

                    levelupButton.alpha = 1f;
                    levelupButton.interactable = true;
                    levelupButton.GetComponent<Button>().interactable = true;
                    UpdatePopup();
                }
            );
        }
        else
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało paliwa aby ulepszyć tę studnię", true);
        }
    }

}