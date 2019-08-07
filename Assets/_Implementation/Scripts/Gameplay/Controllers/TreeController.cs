using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour
{

    // Generator data
    public Tree treeData;

    // Territory boost flag
    public int TerritoryBoosted = 0;

    // Boost Text
    public TextMesh boostText;

    // Inspect window
    void OnMouseDown()
    {
        if (!UIController.instance.uiActive)
        {
            if (Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), new Vector2(PlayerColliderController.instance.transform.position.x, PlayerColliderController.instance.transform.position.z)) < AppConstants.Object1Range)
            {
                TreePopup.instance.Open(treeData);
            }
            else
            {
                BannerController.instance.showBannerWithText(true, "Podejdź bliżej tego obiektu aby zobaczyć jego szczegóły", true);
            }
        }
    }

    // Setting territory boost
    public void SetupTerritoryBoostText()
    {
        if (this.TerritoryBoosted > 1)
        {
            this.boostText.text = "x" + (this.TerritoryBoosted).ToString();
            this.boostText.GetComponent<Animation>().Play();
        }
        else
        {
            this.boostText.text = "";
            this.boostText.GetComponent<Animation>().Stop();
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
        TreesController.instance.trees.Remove(this);
        Destroy(this.gameObject);
    }

}
