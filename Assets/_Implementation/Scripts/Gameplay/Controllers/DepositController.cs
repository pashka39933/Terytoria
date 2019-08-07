using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepositController : MonoBehaviour
{

    // Deposit data
    public Deposit depositData;

    // Deposit preview
    public Sprite previewSprite;

    // Inspect window
    void OnMouseDown()
    {
        if (!UIController.instance.uiActive)
        {
            if (Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), new Vector2(PlayerColliderController.instance.transform.position.x, PlayerColliderController.instance.transform.position.z)) < AppConstants.DepositRange)
            {
                if (this.previewSprite == null)
                {
                    Utils.Web.GetSprite(depositData.previewUrl, (success, downloadedSprite) =>
                    {
                        if (success)
                            this.previewSprite = downloadedSprite;
                    });
                }
                ClosedDepositPopup.instance.Open(depositData);
            }
            else
            {
                BannerController.instance.showBannerWithText(true, "Podejdź bliżej tego obiektu aby zobaczyć jego szczegóły", true);
            }
        }
    }

    // Destroy smoothly
    public void Destroy()
    {
        StartCoroutine(SmoothDestroy());
    }
    IEnumerator SmoothDestroy()
    {
        Animation anim = this.GetComponent<Animation>();
        anim[anim.clip.name].time = anim[anim.clip.name].length;
        anim[anim.clip.name].speed = -1;
        anim.Play();
        yield return new WaitWhile(() => anim.isPlaying);
        DepositsController.instance.deposits.Remove(this);
        Destroy(this.gameObject);
    }
}
