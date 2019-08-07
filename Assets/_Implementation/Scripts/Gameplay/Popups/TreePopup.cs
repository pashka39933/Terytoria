using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TreePopup : MonoBehaviour
{

    public static TreePopup instance;
    TreePopup() { instance = this; }

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

    private TreeController tree;

    public Text levelText;
    public Text biomassAmount;
    public Text nickText;
    public CanvasGroup biomassButton;

    public void Open(Tree tree)
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        StartCoroutine(UpdateCoroutine(TreesController.instance.trees.FindIndex(x => x.treeData.id == tree.id)));

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

    IEnumerator UpdateCoroutine(int treeIndex)
    {
        while (true)
        {
            tree = TreesController.instance.trees[treeIndex];
            UpdatePopup();
            yield return new WaitForEndOfFrame();
            if (Input.GetKey(KeyCode.Escape))
                Close();
        }
    }

    private void UpdatePopup()
    {
        float fruitSize = 0f;
        int ts = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        int initialTreeLifetime = Mathf.Abs((tree.treeData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? tree.treeData.ownerFruitCollectionTimestamp : tree.treeData.commonFruitCollectionTimestamp) - tree.treeData.creationTimestamp);
        int currentTreeLifetime = Mathf.Abs(ts - tree.treeData.creationTimestamp);
        int level = 0;
        int nextLevelLifetime = 0;
        int nextLevelLifetimeMem = 0;
        for (int i = 0; i < currentTreeLifetime + AppConstants.TreeUpgradeTimeConstantInSeconds; i += AppConstants.TreeUpgradeTimeConstantInSeconds)
        {
            if (i == nextLevelLifetime || i >= currentTreeLifetime)
            {
                if (i > initialTreeLifetime)
                {
                    if (fruitSize.Equals(0f))
                        fruitSize = fruitSize + (Mathf.Abs((i > currentTreeLifetime ? currentTreeLifetime : i) - initialTreeLifetime) * level * AppConstants.TreeFruitGrowConstant);
                    else
                        fruitSize = fruitSize + (Mathf.Abs((i > currentTreeLifetime ? currentTreeLifetime : i) - nextLevelLifetimeMem) * level * AppConstants.TreeFruitGrowConstant);
                    nextLevelLifetimeMem = i;
                }

                if (i < currentTreeLifetime)
                {
                    level++;
                    nextLevelLifetime = nextLevelLifetime + level * AppConstants.TreeUpgradeTimeConstantInSeconds;
                }
            }
        }

        float biomassBonus = 1f + AppConstants.TreeSingleBonusValue * TreesController.instance.trees.FindAll(x => x.treeData.id != tree.treeData.id && Vector3.Distance(x.treeData.position, tree.treeData.position) < AppConstants.TreesBonusDistance).Count;
        biomassBonus = biomassBonus > AppConstants.TreesBonusMinValue ? biomassBonus : AppConstants.TreesBonusMinValue;
        biomassBonus = biomassBonus < AppConstants.TreesBonusMaxValue ? biomassBonus : AppConstants.TreesBonusMaxValue;

        float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => Vector3.Distance(x.wellData.position, tree.treeData.position) < AppConstants.WellsBonusDistance).Count;
        wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
        wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;
        biomassBonus = biomassBonus + wellsBonus;

        float biomass = (fruitSize * biomassBonus);
        float maxBiomass = AppConstants.TreeFruitMaximumSizeConstant * level;
        biomass = (tree.treeData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? biomass : biomass > maxBiomass ? maxBiomass : biomass);

        levelText.text = "Poziom " + level;
        biomassAmount.text = ResourcesController.FormatNumber((biomass)) + (tree.TerritoryBoosted > 1 ? " (<color=lime>x" + tree.TerritoryBoosted + "</color>)" : "");
        biomassButton.alpha = (tree.treeData.nick == PlayerPrefs.GetString(AppConstants.NickTag) || biomass.Equals(maxBiomass)) ? 1f : 0.5f;
        biomassButton.interactable = (tree.treeData.nick == PlayerPrefs.GetString(AppConstants.NickTag) || biomass.Equals(maxBiomass));
        biomassButton.GetComponent<Button>().onClick.RemoveAllListeners();
        biomassButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (tree.treeData.nick == PlayerPrefs.GetString(AppConstants.NickTag))
                CollectOwnerFruit(biomass);
            else
                CollectCommonFruit(biomass);
        });

        nickText.text = tree.treeData.nick;
    }

    private void CollectOwnerFruit(float amount)
    {
        Tree treeData = TreesController.instance.trees.Find(x => x.treeData.id == tree.treeData.id).treeData;
        if (amount.Equals(0))
            return;
        biomassButton.alpha = 0.5f;
        biomassButton.interactable = false;
        biomassButton.GetComponent<Button>().interactable = false;
        Utils.Web.PostValues(
            AppConstants.CollectOwnerFruitUrl,
            new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
            new KeyValuePair<string, string>("ID", tree.treeData.id.ToString())
            }),
            (code, response) =>
            {
                if (code == 200)
                {
                    ResourcesController.instance.AddBiomass(amount * (tree.TerritoryBoosted > 1 ? tree.TerritoryBoosted : 1));
                    treeData.ownerFruitCollectionTimestamp = (int)Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "ts");
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

    private void CollectCommonFruit(float amount)
    {
        Tree treeData = TreesController.instance.trees.Find(x => x.treeData.id == tree.treeData.id).treeData;
        if (amount.Equals(0))
            return;
        biomassButton.alpha = 0.5f;
        biomassButton.interactable = false;
        biomassButton.GetComponent<Button>().interactable = false;
        Utils.Web.PostValues(
            AppConstants.CollectCommonFruitUrl,
            new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
            new KeyValuePair<string, string>("ID", tree.treeData.id.ToString()),
                new KeyValuePair<string, string>("PREVIOUS_CONSUMPTION_TIMESTAMP", treeData.commonFruitCollectionTimestamp.ToString()),
            }),
            (code, response) =>
            {
                if (code == 200)
                {
                    ResourcesController.instance.AddBiomass(amount * (tree.TerritoryBoosted > 1 ? tree.TerritoryBoosted : 1));
                    treeData.commonFruitCollectionTimestamp = (int)Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "ts");
                }
                else if (code == 599)
                {
                    BannerController.instance.showBannerWithText(true, "Ktoś był szybszy", true);
                    treeData.commonFruitCollectionTimestamp = (int)Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "ts");
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

}