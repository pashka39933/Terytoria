using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WellController : MonoBehaviour
{

    // Well data
    public Well wellData;

    // Resource particle
    public ParticleSystem resourceParticle;

    // Resource particle materials
    public Material fuelParticle;

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
                WellPopup.instance.Open(wellData);
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

    // Transmitting fuel from well to player
    public void TransmitFuel()
    {
        if (this.transform.parent.gameObject.activeSelf)
            StartCoroutine(TransmitFuelCoroutine());
        else
        {
            float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => x.wellData.id != wellData.id && Vector3.Distance(x.wellData.position, wellData.position) < AppConstants.WellsBonusDistance).Count;
            wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
            wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;
            ResourcesController.instance.AddFuel((wellData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? (wellData.level * wellsBonus * AppConstants.WellOwnerFuelConstant) : (wellData.level * wellsBonus * AppConstants.WellCommonFuelConstant)) * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
        }
    }
    IEnumerator TransmitFuelCoroutine()
    {
        ParticleSystem particleObject = Instantiate(resourceParticle);
        particleObject.transform.parent = this.transform;
        particleObject.transform.localScale = Vector3.one;
        particleObject.transform.position = new Vector3(this.transform.position.x, 4f, this.transform.position.z);
        particleObject.GetComponent<Renderer>().material = fuelParticle;

        ParticleSystem.ShapeModule shape = particleObject.shape;
        shape.radius = 1.5f;
        ParticleSystem.EmissionModule emission = particleObject.emission;
        emission.rateOverTime = 15f;
        ParticleSystem.MainModule main = particleObject.main;
        ParticleSystem.MinMaxGradient color = main.startColor;
        color = particleObject.GetComponent<Renderer>().material.GetColor("_TintColor");
        main.startColor = color;

        Transform player = PlayerColliderController.instance.transform;

        while (Vector3.Distance(player.position, particleObject.transform.position) > 0.5f)
        {
            particleObject.transform.position = Vector3.Lerp(particleObject.transform.position, player.position, Time.deltaTime * 2f);
            yield return new WaitForEndOfFrame();
        }

        float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => x.wellData.id != wellData.id && Vector3.Distance(x.wellData.position, wellData.position) < AppConstants.WellsBonusDistance).Count;
        wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
        wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;
        ResourcesController.instance.AddFuel((wellData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? (wellData.level * wellsBonus * AppConstants.WellOwnerFuelConstant) : (wellData.level * wellsBonus * AppConstants.WellCommonFuelConstant)) * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));

        particleObject.Stop();
        yield return new WaitWhile(() => particleObject.isPlaying);
        Destroy(particleObject.gameObject);
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
        WellsController.instance.wells.Remove(this);
        Destroy(this.gameObject);
    }

}
