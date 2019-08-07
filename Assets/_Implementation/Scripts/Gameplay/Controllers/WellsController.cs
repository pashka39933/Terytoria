using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WellsController : MonoBehaviour
{

    public static WellsController instance;
    WellsController() { instance = this; }

    public GameObject wellPrefab;

    public List<WellController> wells = new List<WellController>();

    // Adding well method
    public void AddWell(Well well)
    {
        if (wells.FindIndex(x => x.wellData.id.Equals(well.id)) > -1)
        {
            wells.Find(x => x.wellData.id.Equals(well.id)).wellData = well;
            return;
        }

        WellController wellController = Instantiate(wellPrefab, this.transform).GetComponent<WellController>();
        wellController.wellData = well;
        wells.Add(wellController);
        wellController.name = well.nick;
        wellController.transform.position = new Vector3(well.position.x, 2, well.position.z);
        wellController.GetComponent<CircleLineRenderer>().CreatePoints(16, AppConstants.Object1Range / 8f, 0.1f);
        wellController.GetComponent<SphereCollider>().enabled = true;
    }

    // Creating well method
    public void CreateWellAtUserPosition(bool confirmed = false)
    {
        if (DownButtonController.instance.standardMenuButtonsVisible)
            DownButtonController.instance.ToggleStandardMenu();

        PatchController userPatch = PatchesController.instance.patches.Find(x => x.patchData.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        List<Vector2> patchPolygon = new List<Vector2>();
        foreach (Vector3 patchPolygonVertex in userPatch.patchData.flags)
            patchPolygon.Add(new Vector2(patchPolygonVertex.x, patchPolygonVertex.z));
        if (userPatch == null || !Geometry.PolygonContainsPoint(patchPolygon, new Vector2(PlayerColliderController.instance.transform.position.x, PlayerColliderController.instance.transform.position.z)))
        {
            BannerController.instance.showBannerWithText(true, "Możesz umieścić studnię tylko w obrębie własnego rewiru", true);
            return;
        }
        if (!PlayerColliderController.instance.CanPlaceObjectHere())
        {
            BannerController.instance.showBannerWithText(true, "Zbyt blisko innego obiektu! Oddal się kilka metrów", true);
            return;
        }
        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { CreateWellAtUserPosition(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz tutaj umieścić studnię za " + AppConstants.WellPurchaseCost + " paliwa ?");
            return;
        }
        if (PlayerPrefs.GetFloat(AppConstants.FuelTag) < AppConstants.WellPurchaseCost)
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało paliwa", true);
            return;
        }
        ResourcesController.instance.AddFuel(-AppConstants.WellPurchaseCost);

        Utils.Web.PostValues(AppConstants.AddWellUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
                new KeyValuePair<string, string>("LAT", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).latitude.ToString()),
                new KeyValuePair<string, string>("LON", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).longitude.ToString())
        }), (code, response) =>
        {
            if (code == 200)
            {
                long id = Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "id");
                Well well = new Well(id, PlayerColliderController.instance.transform.position, PlayerPrefs.GetString(AppConstants.NickTag), 1);
                AddWell(well);
            }
            else
            {
                Debug.Log(response);
                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
            }
        });
    }

}