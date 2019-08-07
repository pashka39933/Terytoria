using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinnerFractionPopupController : MonoBehaviour {

    public static WinnerFractionPopupController instance;
    WinnerFractionPopupController() { instance = this; }

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

    public Text winnerFractionText;
    public Image winnerFractionIcon;
    public Sprite natureIcon, commercyIcon, industryIcon;

    public void Open(AppConstants.Fraction winnerFraction)
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        winnerFractionText.text = winnerFractionText.text.Replace("[1]", winnerFraction == AppConstants.Fraction.NATURE ? "przyrodnicza" : winnerFraction == AppConstants.Fraction.COMMERCY ? "komercyjna" : "przemysłowa");
        winnerFractionIcon.sprite = winnerFraction == AppConstants.Fraction.NATURE ? natureIcon : winnerFraction == AppConstants.Fraction.COMMERCY ? commercyIcon : industryIcon;

        this.GetComponent<Animation>().Play("popupIn");
    }

    public void Close()
    {
        if (!opened)
            return;
        opened = false;

        UIController.instance.uiActive = false;

        this.GetComponent<Animation>().Play("popupOut");
    }

}
