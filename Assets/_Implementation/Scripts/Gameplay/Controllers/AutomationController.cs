using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomationController : MonoBehaviour
{

    // Automation data
    public Automation automationData;

    // Resource particle
    public ParticleSystem resourceParticle;

    // Resource particle materials
    public Material energyParticle, biomassParticle, gadgetsParticle, fuelParticle;

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
                AutomationPopup.instance.Open(automationData);
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

    // Converting resources
    int conversionTimeInSeconds = 0;
    public void ConvertResource()
    {
        //if (ResetConversionInputAmountCoroutineVariable != null)
        //    StopCoroutine(ResetConversionInputAmountCoroutineVariable);
        //ResetConversionInputAmountCoroutineVariable = StartCoroutine(ResetConversionInputAmountCoroutine());

        float inputAmount = ((conversionTimeInSeconds / AppConstants.AutomationConversionDecreaseRatio) + AppConstants.AutomationConversionDefaultInputValue);
        float outputAmount = AppConstants.AutomationConversionDefaultOutputValue;

        if (PlayerPrefs.GetFloat(automationData.convertedResourceType == (int)AppConstants.ResourceType.ENERGY ? AppConstants.EnergyTag : automationData.convertedResourceType == (int)AppConstants.ResourceType.BIOMASS ? AppConstants.BiomassTag : automationData.convertedResourceType == (int)AppConstants.ResourceType.GADGETS ? AppConstants.GadgetsTag : AppConstants.FuelTag) >= inputAmount)
        {
            conversionTimeInSeconds++;

            if (this.transform.parent.gameObject.activeSelf)
                StartCoroutine(ConvertResourceCoroutine(inputAmount, outputAmount));
            else
            {
                float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => Vector3.Distance(x.wellData.position, automationData.position) < AppConstants.WellsBonusDistance).Count;
                wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
                wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;
                inputAmount = (inputAmount * wellsBonus);
                switch (automationData.convertedResourceType)
                {
                    case (int)AppConstants.ResourceType.ENERGY:
                        ResourcesController.instance.AddEnergy(-inputAmount);
                        PlayerPrefs.SetFloat(AppConstants.AutomationsEnergyDelta + "_" + automationData.id, PlayerPrefs.GetFloat(AppConstants.AutomationsEnergyDelta + "_" + automationData.id, 0) + inputAmount);
                        PlayerPrefs.Save();
                        break;
                    case (int)AppConstants.ResourceType.BIOMASS:
                        ResourcesController.instance.AddBiomass(-inputAmount);
                        PlayerPrefs.SetFloat(AppConstants.AutomationsBiomassDelta + "_" + automationData.id, PlayerPrefs.GetFloat(AppConstants.AutomationsBiomassDelta + "_" + automationData.id, 0) + inputAmount);
                        PlayerPrefs.Save();
                        break;
                    case (int)AppConstants.ResourceType.FUEL:
                        ResourcesController.instance.AddFuel(-inputAmount);
                        PlayerPrefs.SetFloat(AppConstants.AutomationsFuelDelta + "_" + automationData.id, PlayerPrefs.GetFloat(AppConstants.AutomationsFuelDelta + "_" + automationData.id, 0) + inputAmount);
                        PlayerPrefs.Save();
                        break;
                }
                ResourcesController.instance.AddGadgets(outputAmount * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
            }
        }
    }
    IEnumerator ConvertResourceCoroutine(float inputAmount, float outputAmount)
    {
        Transform player = PlayerColliderController.instance.transform;
        Transform converterInput = this.transform;
        Transform converterOutput = this.transform;

        ParticleSystem particleObject1 = Instantiate(resourceParticle);
        particleObject1.transform.parent = converterInput;
        particleObject1.transform.localScale = Vector3.one;
        particleObject1.transform.position = player.position;
        particleObject1.GetComponent<Renderer>().material = automationData.convertedResourceType == (int)AppConstants.ResourceType.ENERGY ? energyParticle : automationData.convertedResourceType == (int)AppConstants.ResourceType.BIOMASS ? biomassParticle : automationData.convertedResourceType == (int)AppConstants.ResourceType.GADGETS ? gadgetsParticle : automationData.convertedResourceType == (int)AppConstants.ResourceType.FUEL ? fuelParticle : energyParticle;

        ParticleSystem.ShapeModule shape = particleObject1.shape;
        shape.radius = 0.5f;
        ParticleSystem.EmissionModule emission = particleObject1.emission;
        emission.rateOverTime = 5f;
        ParticleSystem.MainModule main = particleObject1.main;
        ParticleSystem.MinMaxGradient color = main.startColor;
        color = particleObject1.GetComponent<Renderer>().material.GetColor("_TintColor");
        main.startColor = color;

        ParticleSystem particleObject2 = Instantiate(resourceParticle);
        particleObject2.transform.parent = converterOutput;
        particleObject2.transform.localScale = Vector3.one;
        particleObject2.transform.position = converterOutput.position;
        particleObject2.GetComponent<Renderer>().material = gadgetsParticle;

        ParticleSystem.ShapeModule shape2 = particleObject2.shape;
        shape2.radius = 0.5f;
        ParticleSystem.EmissionModule emission2 = particleObject2.emission;
        emission2.rateOverTime = 5f;
        ParticleSystem.MainModule main2 = particleObject2.main;
        ParticleSystem.MinMaxGradient color2 = main2.startColor;
        color2 = particleObject2.GetComponent<Renderer>().material.GetColor("_TintColor");
        main2.startColor = color2;

        while (Vector3.Distance(converterInput.position, particleObject1.transform.position) > 0.5f || Vector3.Distance(player.position, particleObject2.transform.position) > 0.5f)
        {
            particleObject1.transform.position = Vector3.Lerp(particleObject1.transform.position, converterInput.position, Time.deltaTime * 3f);
            particleObject2.transform.position = Vector3.Lerp(particleObject2.transform.position, player.position, Time.deltaTime * 3f);
            yield return new WaitForEndOfFrame();
        }

        float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => Vector3.Distance(x.wellData.position, automationData.position) < AppConstants.WellsBonusDistance).Count;
        wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
        wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;
        inputAmount = (inputAmount * wellsBonus);
        switch (automationData.convertedResourceType)
        {
            case (int)AppConstants.ResourceType.ENERGY:
                ResourcesController.instance.AddEnergy(-inputAmount);
                PlayerPrefs.SetFloat(AppConstants.AutomationsEnergyDelta + "_" + automationData.id, PlayerPrefs.GetFloat(AppConstants.AutomationsEnergyDelta + "_" + automationData.id, 0) + inputAmount);
                PlayerPrefs.Save();
                break;
            case (int)AppConstants.ResourceType.BIOMASS:
                ResourcesController.instance.AddBiomass(-inputAmount);
                PlayerPrefs.SetFloat(AppConstants.AutomationsBiomassDelta + "_" + automationData.id, PlayerPrefs.GetFloat(AppConstants.AutomationsBiomassDelta + "_" + automationData.id, 0) + inputAmount);
                PlayerPrefs.Save();
                break;
            case (int)AppConstants.ResourceType.FUEL:
                ResourcesController.instance.AddFuel(-inputAmount);
                PlayerPrefs.SetFloat(AppConstants.AutomationsFuelDelta + "_" + automationData.id, PlayerPrefs.GetFloat(AppConstants.AutomationsFuelDelta + "_" + automationData.id, 0) + inputAmount);
                PlayerPrefs.Save();
                break;
        }
        ResourcesController.instance.AddGadgets(outputAmount * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));

        particleObject1.Stop();
        particleObject2.Stop();
        yield return new WaitWhile(() => particleObject1.isPlaying);
        yield return new WaitWhile(() => particleObject2.isPlaying);
        Destroy(particleObject1.gameObject);
        Destroy(particleObject2.gameObject);
    }

    // Coroutine resetting current automation's conversion input amount of resource
    //Coroutine ResetConversionInputAmountCoroutineVariable;
    //IEnumerator ResetConversionInputAmountCoroutine()
    //{
    //    yield return new WaitForSeconds(AppConstants.AutomationConversionRatioResetTime);
    //}

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
        AutomationsController.instance.automations.Remove(this);
        Destroy(this.gameObject);
    }

}
