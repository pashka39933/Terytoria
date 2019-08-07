using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorController : MonoBehaviour
{

    // Generator data
    public Generator generatorData;

    // Resources materials
    public Material generatorEmpty, generatorEnergy, generatorBiomass, generatorFuel, generatorGadgets;

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
            if (Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), new Vector2(PlayerColliderController.instance.transform.position.x, PlayerColliderController.instance.transform.position.z)) < AppConstants.GeneratorRange)
            {
                GeneratorPopup.instance.Open(generatorData);
            }
            else
            {
                BannerController.instance.showBannerWithText(true, "Podejdź bliżej tego obiektu aby zobaczyć jego szczegóły", true);
            }
        }
    }

    // Setting visial state depending on player distance
    public void SetupVisualState(bool playerClose)
    {
        this.transform.GetComponent<MeshRenderer>().material = !playerClose ? generatorEmpty : generatorEnergy;
        this.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = !playerClose ? generatorEnergy : generatorEmpty;
        this.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = !playerClose ? generatorEnergy : generatorEmpty;
        this.transform.GetChild(0).GetChild(2).GetComponent<MeshRenderer>().material = !playerClose ? generatorEnergy : generatorEmpty;
        this.transform.GetChild(0).GetChild(3).GetComponent<MeshRenderer>().material = !playerClose ? generatorEnergy : generatorEmpty;
        this.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material = !playerClose ? generatorEmpty : generatorData.converterFromResource == (int)AppConstants.ResourceType.ENERGY ? generatorEnergy : generatorData.converterFromResource == (int)AppConstants.ResourceType.BIOMASS ? generatorBiomass : generatorData.converterFromResource == (int)AppConstants.ResourceType.GADGETS ? generatorGadgets : generatorFuel;
        this.transform.GetChild(1).GetChild(1).GetComponent<MeshRenderer>().material = !playerClose ? generatorEmpty : generatorData.converterToResource == (int)AppConstants.ResourceType.ENERGY ? generatorEnergy : generatorData.converterToResource == (int)AppConstants.ResourceType.BIOMASS ? generatorBiomass : generatorData.converterToResource == (int)AppConstants.ResourceType.GADGETS ? generatorGadgets : generatorFuel;
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

    // Transmitting energy from generator to player
    public void TransmitEnergy()
    {
        if (this.transform.parent.gameObject.activeSelf)
            StartCoroutine(TransmitEnergyCoroutine());
        else
        {
            float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => Vector3.Distance(x.wellData.position, generatorData.position) < AppConstants.WellsBonusDistance).Count;
            wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
            wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;
            ResourcesController.instance.AddEnergy((generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? (AppConstants.GeneratorOwnerEnergyConstant * generatorData.level * wellsBonus) : (AppConstants.GeneratorCommonEnergyConstant * generatorData.level * wellsBonus)) * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
        }
    }
    IEnumerator TransmitEnergyCoroutine()
    {
        ParticleSystem particleObject = Instantiate(resourceParticle);
        particleObject.transform.parent = this.transform;
        particleObject.transform.localScale = Vector3.one;
        particleObject.transform.position = new Vector3(this.transform.position.x, 10f, this.transform.position.z);

        ParticleSystem.ShapeModule shape = particleObject.shape;
        shape.radius = generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? 1f : 0.5f;
        ParticleSystem.EmissionModule emission = particleObject.emission;
        emission.rateOverTime = generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? 10f : 5f;
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

        float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => Vector3.Distance(x.wellData.position, generatorData.position) < AppConstants.WellsBonusDistance).Count;
        wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
        wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;
        float addedEnergy = (generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? (AppConstants.GeneratorOwnerEnergyConstant * generatorData.level * wellsBonus) : (AppConstants.GeneratorCommonEnergyConstant * generatorData.level * wellsBonus));
        ResourcesController.instance.AddEnergy(addedEnergy * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));

        AnalyticsController.instance.OwnGeneratorEnergy += (generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? addedEnergy : 0);
        AnalyticsController.instance.OwnGeneratorTime += (generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? 1 : 0);
        AnalyticsController.instance.OtherGeneratorsEnergy += (generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? 0 : addedEnergy);
        AnalyticsController.instance.OtherGeneratorsTime += (generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag) ? 0 : 1);

        particleObject.Stop();
        yield return new WaitWhile(() => particleObject.isPlaying);
        Destroy(particleObject.gameObject);
    }

    // Transmitting energy from generator battery to player
    public void TransmitBatteryEnergy(int amount)
    {
        if (this.transform.parent.gameObject.activeSelf)
            StartCoroutine(TransmitBatteryEnergyCoroutine(amount));
        else
            ResourcesController.instance.AddEnergy(amount * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
    }
    IEnumerator TransmitBatteryEnergyCoroutine(float amount)
    {
        if (amount.Equals(0))
            yield break;

        float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => Vector3.Distance(x.wellData.position, generatorData.position) < AppConstants.WellsBonusDistance).Count;
        wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
        wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;
        amount = (amount * wellsBonus);

        ParticleSystem particleObject = Instantiate(resourceParticle);
        particleObject.transform.parent = this.transform;
        particleObject.transform.localScale = Vector3.one;
        particleObject.transform.position = new Vector3(this.transform.position.x, 10f, this.transform.position.z);

        ParticleSystem.ShapeModule shape = particleObject.shape;
        shape.radius = 3f;
        ParticleSystem.EmissionModule emission = particleObject.emission;
        emission.rateOverTime = 30f;
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

        ResourcesController.instance.AddEnergy(amount * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));

        particleObject.Stop();
        yield return new WaitWhile(() => particleObject.isPlaying);
        Destroy(particleObject.gameObject);
    }

    // Converting resources
    public void ConvertResource()
    {
        if (generatorData.converterLevel > 0 && PlayerPrefs.GetFloat(generatorData.converterFromResource == (int)AppConstants.ResourceType.ENERGY ? AppConstants.EnergyTag : generatorData.converterFromResource == (int)AppConstants.ResourceType.BIOMASS ? AppConstants.BiomassTag : generatorData.converterFromResource == (int)AppConstants.ResourceType.GADGETS ? AppConstants.GadgetsTag : AppConstants.FuelTag) >= generatorData.converterLevel)
        {
            if (this.transform.parent.gameObject.activeSelf)
                StartCoroutine(ConvertResourceCoroutine(generatorData.converterLevel * AppConstants.GeneratorConverterEfficiencyConstant));
            else
            {
                switch (generatorData.converterFromResource)
                {
                    case (int)AppConstants.ResourceType.ENERGY:
                        ResourcesController.instance.AddEnergy(-generatorData.converterLevel);
                        break;
                    case (int)AppConstants.ResourceType.BIOMASS:
                        ResourcesController.instance.AddBiomass(-generatorData.converterLevel);
                        break;
                    case (int)AppConstants.ResourceType.GADGETS:
                        ResourcesController.instance.AddGadgets(-generatorData.converterLevel);
                        break;
                    case (int)AppConstants.ResourceType.FUEL:
                        ResourcesController.instance.AddFuel(-generatorData.converterLevel);
                        break;
                }
                switch (generatorData.converterToResource)
                {
                    case (int)AppConstants.ResourceType.ENERGY:
                        ResourcesController.instance.AddEnergy(generatorData.converterLevel * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                        break;
                    case (int)AppConstants.ResourceType.BIOMASS:
                        ResourcesController.instance.AddBiomass(generatorData.converterLevel * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                        break;
                    case (int)AppConstants.ResourceType.GADGETS:
                        ResourcesController.instance.AddGadgets(generatorData.converterLevel * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                        break;
                    case (int)AppConstants.ResourceType.FUEL:
                        ResourcesController.instance.AddFuel(generatorData.converterLevel * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                        break;
                }
            }
        }
    }
    IEnumerator ConvertResourceCoroutine(float amount)
    {
        float wellsBonus = 1f + AppConstants.WellSingleBonusValue * WellsController.instance.wells.FindAll(x => Vector3.Distance(x.wellData.position, generatorData.position) < AppConstants.WellsBonusDistance).Count;
        wellsBonus = wellsBonus > AppConstants.WellsBonusMinValue ? wellsBonus : AppConstants.WellsBonusMinValue;
        wellsBonus = wellsBonus < AppConstants.WellsBonusMaxValue ? wellsBonus : AppConstants.WellsBonusMaxValue;
        amount = (amount * wellsBonus);

        Transform player = PlayerColliderController.instance.transform;
        Transform converterInput = this.transform.GetChild(1).GetChild(0);
        Transform converterOutput = this.transform.GetChild(1).GetChild(1);

        ParticleSystem particleObject1 = Instantiate(resourceParticle);
        particleObject1.transform.parent = converterInput;
        particleObject1.transform.localScale = Vector3.one;
        particleObject1.transform.position = player.position;
        particleObject1.GetComponent<Renderer>().material = generatorData.converterFromResource == (int)AppConstants.ResourceType.ENERGY ? energyParticle : generatorData.converterFromResource == (int)AppConstants.ResourceType.BIOMASS ? biomassParticle : generatorData.converterFromResource == (int)AppConstants.ResourceType.GADGETS ? gadgetsParticle : generatorData.converterFromResource == (int)AppConstants.ResourceType.FUEL ? fuelParticle : energyParticle;

        ParticleSystem.ShapeModule shape = particleObject1.shape;
        shape.radius = 1f;
        ParticleSystem.EmissionModule emission = particleObject1.emission;
        emission.rateOverTime = 10f;
        ParticleSystem.MainModule main = particleObject1.main;
        ParticleSystem.MinMaxGradient color = main.startColor;
        color = particleObject1.GetComponent<Renderer>().material.GetColor("_TintColor");
        main.startColor = color;

        ParticleSystem particleObject2 = Instantiate(resourceParticle);
        particleObject2.transform.parent = converterOutput;
        particleObject2.transform.localScale = Vector3.one;
        particleObject2.transform.position = converterOutput.position;
        particleObject2.GetComponent<Renderer>().material = generatorData.converterToResource == (int)AppConstants.ResourceType.ENERGY ? energyParticle : generatorData.converterFromResource == (int)AppConstants.ResourceType.BIOMASS ? biomassParticle : generatorData.converterToResource == (int)AppConstants.ResourceType.GADGETS ? gadgetsParticle : generatorData.converterToResource == (int)AppConstants.ResourceType.FUEL ? fuelParticle : energyParticle;

        ParticleSystem.ShapeModule shape2 = particleObject2.shape;
        shape2.radius = 1f;
        ParticleSystem.EmissionModule emission2 = particleObject2.emission;
        emission2.rateOverTime = 10f;
        ParticleSystem.MainModule main2 = particleObject2.main;
        ParticleSystem.MinMaxGradient color2 = main2.startColor;
        color2 = particleObject2.GetComponent<Renderer>().material.GetColor("_TintColor");
        main2.startColor = color2;

        while (Vector3.Distance(converterInput.position, particleObject1.transform.position) > 0.5f || Vector3.Distance(player.position, particleObject2.transform.position) > 0.5f)
        {
            particleObject1.transform.position = Vector3.Lerp(particleObject1.transform.position, converterInput.position, Time.deltaTime * 2.5f);
            particleObject2.transform.position = Vector3.Lerp(particleObject2.transform.position, player.position, Time.deltaTime * 2.5f);
            yield return new WaitForEndOfFrame();
        }

        switch (generatorData.converterFromResource)
        {
            case (int)AppConstants.ResourceType.ENERGY:
                ResourcesController.instance.AddEnergy(-amount);
                break;
            case (int)AppConstants.ResourceType.BIOMASS:
                ResourcesController.instance.AddBiomass(-amount);
                break;
            case (int)AppConstants.ResourceType.GADGETS:
                ResourcesController.instance.AddGadgets(-amount);
                break;
            case (int)AppConstants.ResourceType.FUEL:
                ResourcesController.instance.AddFuel(-amount);
                break;
        }

        switch (generatorData.converterToResource)
        {
            case (int)AppConstants.ResourceType.ENERGY:
                ResourcesController.instance.AddEnergy(amount * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                break;
            case (int)AppConstants.ResourceType.BIOMASS:
                ResourcesController.instance.AddBiomass(amount * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                break;
            case (int)AppConstants.ResourceType.GADGETS:
                ResourcesController.instance.AddGadgets(amount * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                break;
            case (int)AppConstants.ResourceType.FUEL:
                ResourcesController.instance.AddFuel(amount * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                break;
        }

        particleObject1.Stop();
        particleObject2.Stop();
        yield return new WaitWhile(() => particleObject1.isPlaying);
        yield return new WaitWhile(() => particleObject2.isPlaying);
        Destroy(particleObject1.gameObject);
        Destroy(particleObject2.gameObject);
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
        GeneratorsController.instance.generators.Remove(this);
        Destroy(this.gameObject);
    }

}
