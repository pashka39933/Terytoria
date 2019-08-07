using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomationsController : MonoBehaviour
{

    public static AutomationsController instance;
    AutomationsController() { instance = this; }

    public GameObject automationPrefab;

    public List<AutomationController> automations = new List<AutomationController>();

    // Adding automation method
    public void AddAutomation(Automation automation)
    {
        if (automations.FindIndex(x => x.automationData.id.Equals(automation.id)) > -1)
        {
            automations.Find(x => x.automationData.id.Equals(automation.id)).automationData = automation;
            return;
        }

        AutomationController automationController = Instantiate(automationPrefab, this.transform).GetComponent<AutomationController>();
        automationController.automationData = automation;
        automations.Add(automationController);
        automationController.name = automation.nick;
        automationController.transform.position = new Vector3(automation.position.x, 2, automation.position.z);
        automationController.GetComponent<CircleLineRenderer>().CreatePoints(16, AppConstants.Object1Range / 6f, 0.1f);
        automationController.GetComponent<SphereCollider>().enabled = true;
    }

    // Creating automation method
    public void CreateAutomationAtUserPosition(bool confirmed = false)
    {
        if (DownButtonController.instance.standardMenuButtonsVisible)
            DownButtonController.instance.ToggleStandardMenu();

        PatchController userPatch = PatchesController.instance.patches.Find(x => x.patchData.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        List<Vector2> patchPolygon = new List<Vector2>();
        foreach (Vector3 patchPolygonVertex in userPatch.patchData.flags)
            patchPolygon.Add(new Vector2(patchPolygonVertex.x, patchPolygonVertex.z));
        if (userPatch == null || !Geometry.PolygonContainsPoint(patchPolygon, new Vector2(PlayerColliderController.instance.transform.position.x, PlayerColliderController.instance.transform.position.z)))
        {
            BannerController.instance.showBannerWithText(true, "Możesz umieścić automat tylko w obrębie własnego rewiru", true);
            return;
        }
        if (!PlayerColliderController.instance.CanPlaceObjectHere())
        {
            BannerController.instance.showBannerWithText(true, "Zbyt blisko innego obiektu! Oddal się kilka metrów", true);
            return;
        }
        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { CreateAutomationAtUserPosition(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz tutaj umieścić automat za " + AppConstants.AutomationPurchaseCost + " gadżetów ?");
            return;
        }
        if (PlayerPrefs.GetFloat(AppConstants.GadgetsTag) < AppConstants.AutomationPurchaseCost)
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało gadżetów", true);
            return;
        }
        ResourcesController.instance.AddGadgets(-AppConstants.AutomationPurchaseCost);

        Utils.Web.PostValues(AppConstants.AddAutomationUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
            new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
                new KeyValuePair<string, string>("LAT", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).latitude.ToString()),
                new KeyValuePair<string, string>("LON", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).longitude.ToString())
        }), (code, response) =>
        {
            if (code == 200)
            {
                int ts = (int)Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "ts");
                long id = Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "id");
                Automation automation = new Automation(id, PlayerColliderController.instance.transform.position, PlayerPrefs.GetString(AppConstants.NickTag), (int)AppConstants.ResourceType.ENERGY, ts, 0, 0, 0);
                AddAutomation(automation);
            }
            else
            {
                Debug.Log(response);
                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
            }
        });
    }

}