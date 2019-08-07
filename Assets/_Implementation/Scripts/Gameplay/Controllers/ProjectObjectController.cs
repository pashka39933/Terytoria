using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectObjectController : MonoBehaviour
{

    // Project object data
    public ProjectObject projectObjectData;

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
            if (Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), new Vector2(PlayerColliderController.instance.transform.position.x, PlayerColliderController.instance.transform.position.z)) < AppConstants.ProjectObjectRange)
            {
                string objectName = projectObjectData.fraction == (int)AppConstants.Fraction.NATURE ? "Regulator klimatu" : projectObjectData.fraction == (int)AppConstants.Fraction.COMMERCY ? "Sztuczna inteligencja" : "Rafineria";
                string projectName = projectObjectData.fraction == (int)AppConstants.Fraction.NATURE ? "Wieczna Wiosna" : projectObjectData.fraction == (int)AppConstants.Fraction.COMMERCY ? "Monopol" : "Statek Kosmiczny";
                string objectHint = "<b><size=45>" + objectName + " (by " + projectObjectData.nick + ")" + "</size></b>[n][n]<i>Zbliż się do niego aby rozpocząć generację punktów projektu " + projectName + ". Tempo generacji wynosi " + AppConstants.ProjectObjectOutputPoints + (TerritoryBoosted > 1 ? ("(<color=lime>x" + TerritoryBoosted + ")</color>") : "") + " punktów projektu za " + AppConstants.ProjectObjectInputEnergy + " jednostek energii.</i>";
                InfoPopup.instance.OpenWithoutAction(objectHint);
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

    // Generating Project Points
    public void GenerateProjectPoints()
    {
        if (PlayerPrefs.GetFloat(AppConstants.EnergyTag) >= AppConstants.ProjectObjectInputEnergy)
        {
            if (this.transform.parent.gameObject.activeSelf)
            {
                StartCoroutine(GenerateProjectPointsCoroutine());
            }
            else
            {
                ResourcesController.instance.AddEnergy(-AppConstants.ProjectObjectInputEnergy);
                switch (PlayerPrefs.GetInt(AppConstants.FractionTag))
                {
                    case (int)AppConstants.Fraction.NATURE:
                        ProjectPointsController.instance.AddNatureProjectPoints(AppConstants.ProjectObjectOutputPoints * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                        break;
                    case (int)AppConstants.Fraction.COMMERCY:
                        ProjectPointsController.instance.AddCommercyProjectPoints(AppConstants.ProjectObjectOutputPoints * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                        break;
                    case (int)AppConstants.Fraction.INDUSTRY:
                        ProjectPointsController.instance.AddIndustryProjectPoints(AppConstants.ProjectObjectOutputPoints * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                        break;
                }
            }
        }
    }
    IEnumerator GenerateProjectPointsCoroutine()
    {
        Transform player = PlayerColliderController.instance.transform;
        Transform converterInput = this.transform;
        Transform converterOutput = this.transform;

        ParticleSystem particleObject1 = Instantiate(resourceParticle);
        particleObject1.transform.parent = converterInput;
        particleObject1.transform.localScale = Vector3.one;
        particleObject1.transform.position = player.position;
        particleObject1.GetComponent<Renderer>().material = energyParticle;

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
        particleObject2.GetComponent<Renderer>().material = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? biomassParticle : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? gadgetsParticle : fuelParticle;

        ParticleSystem.ShapeModule shape2 = particleObject2.shape;
        shape2.radius = 4f;
        ParticleSystem.EmissionModule emission2 = particleObject2.emission;
        emission2.rateOverTime = 40f;
        ParticleSystem.MainModule main2 = particleObject2.main;
        ParticleSystem.MinMaxGradient color2 = main2.startColor;
        color2 = particleObject2.GetComponent<Renderer>().material.GetColor("_TintColor");
        main2.startColor = color2;

        Vector3 converterOutputDestPosition = new Vector3(converterOutput.transform.position.x, converterOutput.transform.position.y + 15f, converterOutput.transform.position.z);
        while (Vector3.Distance(converterInput.position, particleObject1.transform.position) > 0.5f || Vector3.Distance(converterOutputDestPosition, particleObject2.transform.position) > 0.5f)
        {
            particleObject1.transform.position = Vector3.Lerp(particleObject1.transform.position, converterInput.position, Time.deltaTime * 2.5f);
            particleObject2.transform.position = Vector3.Lerp(particleObject2.transform.position, converterOutputDestPosition, Time.deltaTime * 2.5f);
            yield return new WaitForEndOfFrame();
        }

        ResourcesController.instance.AddEnergy(-AppConstants.ProjectObjectInputEnergy);
        switch (PlayerPrefs.GetInt(AppConstants.FractionTag))
        {
            case (int)AppConstants.Fraction.NATURE:
                ProjectPointsController.instance.AddNatureProjectPoints(AppConstants.ProjectObjectOutputPoints * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                break;
            case (int)AppConstants.Fraction.COMMERCY:
                ProjectPointsController.instance.AddCommercyProjectPoints(AppConstants.ProjectObjectOutputPoints * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
                break;
            case (int)AppConstants.Fraction.INDUSTRY:
                ProjectPointsController.instance.AddIndustryProjectPoints(AppConstants.ProjectObjectOutputPoints * (TerritoryBoosted > 1 ? TerritoryBoosted : 1));
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
        ProjectObjectsController.instance.projectObjects.Remove(this);
        Destroy(this.gameObject);
    }

}