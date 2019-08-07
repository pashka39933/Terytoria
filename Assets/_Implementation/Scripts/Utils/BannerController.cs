using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BannerController : MonoBehaviour
{

    public static BannerController instance;
    BannerController() { instance = this; }

    public void showBannerWithText(bool show, string text, bool autoHide = false)
    {

        if (this == null || this.GetComponentInChildren<Text>() == null)
        {
            return;
        }

        this.GetComponentInChildren<Text>().text = text;

        RectTransform bannerRect = this.GetComponent<RectTransform>();
        bool alreadyOpen = Mathf.Abs(bannerRect.anchoredPosition.y - bannerRect.sizeDelta.y) > 1;

        if (show != alreadyOpen)
        {
            StopAllCoroutines();
            StartCoroutine(Slide(show, 0.25f, autoHide));
        }

    }

    private IEnumerator Slide(bool show, float time, bool autoHide)
    {

        Vector2 newPosition;
        RectTransform bannerRect = this.GetComponent<RectTransform>();

        if (show)
        {
            newPosition = new Vector2(bannerRect.anchoredPosition.x, 0);
        }
        else
        {
            newPosition = new Vector2(bannerRect.anchoredPosition.x, bannerRect.sizeDelta.y);
        }

        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            bannerRect.anchoredPosition = Vector2.Lerp(bannerRect.anchoredPosition, newPosition, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        bannerRect.anchoredPosition = newPosition;

        if (show && autoHide)
        {
            yield return new WaitForSeconds(AppConstants.TopInfoBannerAutoHideTimeInSeconds);
            StartCoroutine(Slide(false, 0.25f, false));
        }

    }

}
