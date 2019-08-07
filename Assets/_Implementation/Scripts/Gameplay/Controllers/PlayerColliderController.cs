using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerColliderController : MonoBehaviour
{

    // Instance
    public static PlayerColliderController instance;
    PlayerColliderController() { instance = this; }

    // Nick Text
    public TextMesh nickText;

    // Character body
    public Transform character;

    // Initialization
    void Start()
    {
        StartCoroutine(BodyCompassRotation());
        StartCoroutine(SetNickCoroutine());
        StartCoroutine(GeneratorsCollisionListener());
        StartCoroutine(WellsCollisionListener());
        StartCoroutine(AutomationsCollisionListener());
        StartCoroutine(ProjectObjectsCollisionListener());
    }

    // Body compass rotation coroutine
    IEnumerator BodyCompassRotation()
    {
        yield return new WaitForSeconds(3);
        while (true)
        {
            Input.compass.enabled = true;
            character.eulerAngles = new Vector3(0, Mathf.LerpAngle(character.eulerAngles.y, Input.compass.trueHeading, Time.deltaTime * 5), 0);
            yield return new WaitForEndOfFrame();
        }
    }

    // Body create coroutine
    IEnumerator SetNickCoroutine()
    {
        nickText.text = PlayerPrefs.GetString(AppConstants.NickTag);
        yield return new WaitWhile(() => nickText.text.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));
        StartCoroutine(SetNickCoroutine());
    }

    // Battery materials
    public Material blueBattery, whiteBattery;

    // Listens for collision with generators
    IEnumerator GeneratorsCollisionListener()
    {
        while (true)
        {
            yield return new WaitUntil(() => PlayerPrefs.GetInt(AppConstants.PatchCreatedTag) == 1);

            foreach (GeneratorController generator in GeneratorsController.instance.generators)
            {
                if (Vector2.Distance(new Vector2(generator.transform.position.x, generator.transform.position.z), new Vector2(this.transform.position.x, this.transform.position.z)) < AppConstants.GeneratorRange)
                {
                    // Resources from battery
                    if (generator.generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag))
                    {
                        int currentTimestamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                        int previousVisitDiff = currentTimestamp - PlayerPrefs.GetInt(AppConstants.GeneratorVisitTimestampTag, currentTimestamp);
                        if (previousVisitDiff > 2)
                        {
                            int accumulatedValue = previousVisitDiff * AppConstants.GeneratorOwnerEnergyConstant * generator.generatorData.level;
                            accumulatedValue = (accumulatedValue > generator.generatorData.batteryLevel * AppConstants.GeneratorBatteryCapacityConstant) ? generator.generatorData.batteryLevel * AppConstants.GeneratorBatteryCapacityConstant : accumulatedValue;
                            generator.TransmitBatteryEnergy(accumulatedValue);
                        }
                        PlayerPrefs.SetInt(AppConstants.GeneratorVisitTimestampTag, currentTimestamp);
                        PlayerPrefs.Save();
                    }

                    // Resources conversion
                    if (generator.generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag))
                    {
                        generator.ConvertResource();
                    }

                    // Standard energy collection
                    generator.TransmitEnergy();

                    generator.SetupVisualState(true);
                }
                else
                {
                    generator.SetupVisualState(false);
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    // Listens for collision with wells
    IEnumerator WellsCollisionListener()
    {
        while (true)
        {
            yield return new WaitUntil(() => PlayerPrefs.GetInt(AppConstants.PatchCreatedTag) == 1);

            foreach (WellController well in WellsController.instance.wells)
            {
                if (Vector2.Distance(new Vector2(well.transform.position.x, well.transform.position.z), new Vector2(this.transform.position.x, this.transform.position.z)) < AppConstants.Object1Range)
                {
                    // Standard fuel collection
                    well.TransmitFuel();
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    // Listens for collision with automations
    IEnumerator AutomationsCollisionListener()
    {
        while (true)
        {
            yield return new WaitUntil(() => PlayerPrefs.GetInt(AppConstants.PatchCreatedTag) == 1);

            foreach (AutomationController automation in AutomationsController.instance.automations)
            {
                if (Vector2.Distance(new Vector2(automation.transform.position.x, automation.transform.position.z), new Vector2(this.transform.position.x, this.transform.position.z)) < AppConstants.Object1Range)
                {
                    // Resources conversion
                    if (automation.automationData.nick != PlayerPrefs.GetString(AppConstants.NickTag))
                    {
                        automation.ConvertResource();
                    }
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    // Listens for collision with project objects
    IEnumerator ProjectObjectsCollisionListener()
    {
        while (true)
        {
            yield return new WaitUntil(() => PlayerPrefs.GetInt(AppConstants.PatchCreatedTag) == 1);

            foreach (ProjectObjectController projectObject in ProjectObjectsController.instance.projectObjects)
            {
                if (Vector2.Distance(new Vector2(projectObject.transform.position.x, projectObject.transform.position.z), new Vector2(this.transform.position.x, this.transform.position.z)) < AppConstants.ProjectObjectRange)
                {
                    // Project points generation
                    if (projectObject.projectObjectData.fraction == PlayerPrefs.GetInt(AppConstants.FractionTag))
                    {
                        projectObject.GenerateProjectPoints();
                    }
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    // Checks if in own generator's range, returns null or own generator object. nearCondition == true -> returns own generator only if player is in it's range
    public GeneratorController GetOwnGenerator(bool nearCondition = false)
    {
        foreach (GeneratorController generator in GeneratorsController.instance.generators)
        {
            if (nearCondition)
            {
                if (generator.generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag) && Vector2.Distance(new Vector2(generator.transform.position.x, generator.transform.position.z), new Vector2(this.transform.position.x, this.transform.position.z)) < AppConstants.GeneratorRange)
                    return generator;
            }
            else
            {
                if (generator.generatorData.nick == PlayerPrefs.GetString(AppConstants.NickTag))
                    return generator;
            }
        }
        return null;
    }

    // Checks if player can place generator at current position
    public bool CanPlaceGeneratorHere()
    {
        if (GeneratorsController.instance.generators.FindIndex(x => Vector3.Distance(x.transform.position, this.transform.position) < AppConstants.MinGeneratorsDistance) > -1)
            return false;

        return true;
    }

    // Checks if player can place object at current position
    public bool CanPlaceObjectHere()
    {
        if (GeneratorsController.instance.generators.FindIndex(x => Vector3.Distance(x.transform.position, this.transform.position) < AppConstants.MinObjectsDistance) > -1)
            return false;
        if (TreesController.instance.trees.FindIndex(x => Vector3.Distance(x.transform.position, this.transform.position) < AppConstants.MinObjectsDistance) > -1)
            return false;
        if (AutomationsController.instance.automations.FindIndex(x => Vector3.Distance(x.transform.position, this.transform.position) < AppConstants.MinObjectsDistance) > -1)
            return false;
        if (WellsController.instance.wells.FindIndex(x => Vector3.Distance(x.transform.position, this.transform.position) < AppConstants.MinObjectsDistance) > -1)
            return false;

        return true;
    }

    // Checks if player can place object at current position
    public bool CanPlacePatchFlagHere()
    {
        foreach (PatchController patchController in PatchesController.instance.patches)
        {
            if (patchController.patchData.flags.FindIndex(x => Vector3.Distance(x, this.transform.GetChild(0).position) < AppConstants.PatchFlagsMinDistance) > -1)
                return false;
        }

        return true;
    }

}
