using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrangersController : MonoBehaviour
{

    public static StrangersController instance;
    StrangersController() { instance = this; }

    public GameObject strangerPrefab;

    public List<StrangerController> strangers = new List<StrangerController>();

    // Adding stranger method
    public void AddStranger(Stranger stranger)
    {
        if (stranger.nick == PlayerPrefs.GetString(AppConstants.NickTag))
            return;
        Int32 currentTs = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        if (Mathf.Abs(stranger.updateTimestamp - currentTs) < AppConstants.ServerSynchronizationIntervalInSeconds * 3)
        {
            if (strangers.FindIndex(x => x.strangerData.id.Equals(stranger.id)) > -1)
            {
                StrangerController foundStranger = strangers.Find(x => x.strangerData.id.Equals(stranger.id));
                foundStranger.SetData(stranger);
                return;
            }

            StrangerController strangerController = Instantiate(strangerPrefab, this.transform).GetComponent<StrangerController>();
            strangerController.SetData(stranger);
            strangers.Add(strangerController);
            strangerController.name = stranger.nick;
            strangerController.transform.position = stranger.position;
        }
    }

}