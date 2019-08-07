using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsController : MonoBehaviour
{

    // Instance
    public static AnalyticsController instance;
    AnalyticsController() { instance = this; }

    // Help vars
    public float OwnGeneratorEnergy = 0, OtherGeneratorsEnergy = 0;
    public float OwnGeneratorTime = 0, OtherGeneratorsTime = 0;
    public Vector3 latestReportedPosition = Vector3.zero;

    // Initialization
    private IEnumerator Start()
    {
        // Waiting for user params
        yield return new WaitUntil(() => PlayerPrefs.HasKey(AppConstants.NickTag));
        yield return new WaitUntil(() => PlayerPrefs.HasKey(AppConstants.FractionTag));

        // DevToDev init
        DevToDev.Analytics.UserId = PlayerPrefs.GetString(AppConstants.NickTag);
        DevToDev.Analytics.ApplicationVersion = Application.version;
        DevToDev.Analytics.Initialize("1eb135a1-8cde-0f86-93db-3122cdd6b478", "O25aGkNz6ZFW3wPfXseQEm9qbu0AnxCU");
        DevToDev.Analytics.SetActiveLog(true);

        // Fabric init
        Fabric.Crashlytics.Crashlytics.SetUserIdentifier(PlayerPrefs.GetString(AppConstants.NickTag));
        Fabric.Crashlytics.Crashlytics.SetKeyValue("fraction", PlayerPrefs.GetInt(AppConstants.FractionTag).ToString());

        // Unity Analytics init
        UnityEngine.Analytics.Analytics.SetUserId(PlayerPrefs.GetString(AppConstants.NickTag));

        // Events sending coroutine
        StartCoroutine(AnalyticsCoroutine());
    }

    // Analytics coroutine
    IEnumerator AnalyticsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(20f / 3f);

            // User's distances
            foreach (StrangerController stranger in StrangersController.instance.strangers)
            {
                Report("UsersDistances", new Dictionary<string, object>() {
                    { "user1", PlayerPrefs.GetString(AppConstants.NickTag) },
                    { "user2", stranger.strangerData.nick },
                    { "distance", Vector3.Distance(PlayerColliderController.instance.transform.position, stranger.transform.position) }
                });
            }

            yield return new WaitForSecondsRealtime(20f / 3f);

            // Generators stats
            Report("GeneratorStats", new Dictionary<string, object>() {
                { "ownEnergy", OwnGeneratorEnergy },
                { "ownTime", OwnGeneratorTime },
                { "otherEnergy", OtherGeneratorsEnergy },
                { "otherTime", OtherGeneratorsTime }
            });
            OwnGeneratorEnergy = 0;
            OwnGeneratorTime = 0;
            OtherGeneratorsEnergy = 0;
            OtherGeneratorsTime = 0;

            yield return new WaitForSecondsRealtime(20f / 3f);

            // Single user walked distance
            if (latestReportedPosition != Vector3.zero)
            {
                Report("UserMovementDistance", new Dictionary<string, object>() {
                    { "distance", Vector3.Distance(PlayerColliderController.instance.transform.position, latestReportedPosition) }
                });
            }
            latestReportedPosition = PlayerColliderController.instance.transform.position;
        }
    }

    // Reporting event
    // usage:
    // AnalyticsController.instance.Report("EventName", new Dictionary<string, object>(){{"Media Type", "Image"}});
    public void Report(string eventName, Dictionary<string, object> eventParams)
    {

        // Adding timestamp
        eventParams.Add("nick", PlayerPrefs.GetString(AppConstants.NickTag));
        eventParams.Add("uuid", PlayerPrefs.GetString(AppConstants.UniqueIdentifier));
        eventParams.Add("fraction", PlayerPrefs.GetInt(AppConstants.FractionTag));
        eventParams.Add("patchCreated", PlayerPrefs.GetInt(AppConstants.PatchCreatedTag));
        eventParams.Add("generatorCreated", PlayerPrefs.GetInt(AppConstants.GeneratorCreatedTag));
        eventParams.Add("batteryCreated", PlayerPrefs.GetInt(AppConstants.BatteryCreatedTag));
        eventParams.Add("converterCreated", PlayerPrefs.GetInt(AppConstants.ConverterCreatedTag));
        eventParams.Add("projectObjectsCount", PlayerPrefs.GetInt(AppConstants.ProjectObjectsCountTag));
        eventParams.Add("coords", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).toLatLongString());
        eventParams.Add("timestamp", (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        eventParams.Add("version", Application.version);

        // DevToDev event
        DevToDev.CustomEventParams devtodevEventParams = new DevToDev.CustomEventParams();
        foreach (KeyValuePair<string, object> eventParam in eventParams)
        {
            try { devtodevEventParams.AddParam(eventParam.Key, (int)eventParam.Value); }
            catch (Exception)
            {
                try { devtodevEventParams.AddParam(eventParam.Key, (long)eventParam.Value); }
                catch (Exception)
                {
                    try { devtodevEventParams.AddParam(eventParam.Key, (float)eventParam.Value); }
                    catch (Exception)
                    {
                        try { devtodevEventParams.AddParam(eventParam.Key, (double)eventParam.Value); }
                        catch (Exception)
                        {
                            try { devtodevEventParams.AddParam(eventParam.Key, (string)eventParam.Value); }
                            catch (Exception)
                            {
                                Debug.Log("[DevToDev] Unknown type of param: " + eventParam.Key);
                            }
                        }
                    }
                }
            }
        }
        DevToDev.Analytics.CustomEvent(eventName, devtodevEventParams);

        // Fabric event
        Fabric.Answers.Answers.LogCustom(eventName, eventParams);

        // Unity Analytics event
        UnityEngine.Analytics.Analytics.CustomEvent(eventName, eventParams);
    }

}