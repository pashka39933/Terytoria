using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ARCloudTagDetectionController : MonoBehaviour, ICloudRecoEventHandler {

    // Cloud Reco instance
    CloudRecoBehaviour mCloudRecoBehaviour;

    // Searched deposit
    Deposit deposit = null;

    // Waiting for initialization
    IEnumerator Start()
    {
        yield return new WaitUntil(() => this.GetComponent<CloudRecoBehaviour>() != null);
        mCloudRecoBehaviour = this.GetComponent<CloudRecoBehaviour>();
        mCloudRecoBehaviour.CloudRecoEnabled = false;
    }

    // Enabling CloudReco deposit's search
    public void StartSearching(Deposit deposit)
    {
        if (mCloudRecoBehaviour)
        {
            this.deposit = deposit;
            mCloudRecoBehaviour.RegisterEventHandler(this);
            mCloudRecoBehaviour.CloudRecoEnabled = true;
        }
    }

    // Disabling CloudReco search
    public void StopSearching()
    {
        if (mCloudRecoBehaviour)
        {
            mCloudRecoBehaviour.CloudRecoEnabled = false;
            mCloudRecoBehaviour.UnregisterEventHandler(this);
            this.deposit = null;
        }
    }

    // Callback - OnNewSearchResult
    public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
    {
        if (targetSearchResult.UniqueTargetId.Equals(this.deposit.id))
        {
            OpenedDepositPopup.instance.Open(deposit);
        }
        else
        {
            BannerController.instance.showBannerWithText(true, "Błędny depozyt! Znajdź " + this.deposit.name, true);
        }
    }

    // Callback - OnStateChanged
    public void OnStateChanged(bool scanning)
    {
        if (scanning)
        {
            var tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            tracker.TargetFinder.ClearTrackables(false);
        }
    }

    // Callback - OnInitialized
    public void OnInitialized(TargetFinder targetFinder)
    {
        Debug.Log("Cloud Reco initialized");
    }
    // Callback - OnInitError
    public void OnInitError(TargetFinder.InitState initError)
    {
        Debug.Log("Cloud Reco init error " + initError.ToString());
    }
    // Callback - OnUpdateError
    public void OnUpdateError(TargetFinder.UpdateState updateError)
    {
        Debug.Log("Cloud Reco update error " + updateError.ToString());
    }


}
