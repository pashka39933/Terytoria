using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectObjectsController : MonoBehaviour
{

    public static ProjectObjectsController instance;
    ProjectObjectsController() { instance = this; }

    public GameObject projectObjectPrefabNature, projectObjectPrefabCommercy, projectObjectPrefabIndustry;

    public List<ProjectObjectController> projectObjects = new List<ProjectObjectController>();

    // Adding project object method
    public void AddProjectObject(ProjectObject projectObject)
    {
        if (projectObjects.FindIndex(x => x.projectObjectData.id.Equals(projectObject.id)) > -1)
        {
            projectObjects.Find(x => x.projectObjectData.id.Equals(projectObject.id)).projectObjectData = projectObject;
            return;
        }

        ProjectObjectController projectObjectController = Instantiate(projectObject.fraction == (int)AppConstants.Fraction.NATURE ? projectObjectPrefabNature : projectObject.fraction == (int)AppConstants.Fraction.COMMERCY ? projectObjectPrefabCommercy : projectObjectPrefabIndustry, this.transform).GetComponent<ProjectObjectController>();
        projectObjectController.projectObjectData = projectObject;
        projectObjects.Add(projectObjectController);
        projectObjectController.name = projectObject.nick;
        projectObjectController.transform.position = new Vector3(projectObject.position.x, 4, projectObject.position.z);
        projectObjectController.GetComponent<CircleLineRenderer>().CreatePoints(16, AppConstants.ProjectObjectRange / 4f, -0.75f);
        projectObjectController.GetComponent<SphereCollider>().enabled = true;
    }

    // Creating project object method
    public void CreateProjectObjectAtUserPosition(bool confirmed = false)
    {
        if (DownButtonController.instance.standardMenuButtonsVisible)
            DownButtonController.instance.ToggleStandardMenu();

        PatchController userPatch = PatchesController.instance.patches.Find(x => x.patchData.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        List<Vector2> patchPolygon = new List<Vector2>();
        foreach (Vector3 patchPolygonVertex in userPatch.patchData.flags)
            patchPolygon.Add(new Vector2(patchPolygonVertex.x, patchPolygonVertex.z));
        if (userPatch == null || !Geometry.PolygonContainsPoint(patchPolygon, new Vector2(PlayerColliderController.instance.transform.position.x, PlayerColliderController.instance.transform.position.z)))
        {
            string objectName = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? "regulator klimatu" : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? "sztuczną inteligencję" : "rafinerię";
            BannerController.instance.showBannerWithText(true, "Możesz umieścić " + objectName + " tylko w obrębie własnego rewiru", true);
            return;
        }
        if (!PlayerColliderController.instance.CanPlaceObjectHere())
        {
            BannerController.instance.showBannerWithText(true, "Zbyt blisko innego obiektu! Oddal się kilka metrów", true);
            return;
        }

        int projectObjectCost = (PlayerPrefs.GetInt(AppConstants.ProjectObjectsCountTag) + 1) * AppConstants.ProjectObjectPurchaseCost;
        string costResource1 = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? "biomasy" : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? "gadżetów" : "paliwa";
        string costResource2 = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? "gadżetów" : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? "paliwa" : "biomasy";
        if (!confirmed)
        {
            string objectName = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? "regulator klimatu" : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? "sztuczną inteligencję" : "rafinerię";
            ConfirmationPopup.instance.Open(() => { CreateProjectObjectAtUserPosition(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz tutaj umieścić  " + objectName + " za " + projectObjectCost + " " + costResource1 + " oraz " + projectObjectCost + " " + costResource2 + " ?");
            return;
        }
        if(PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE && (PlayerPrefs.GetFloat(AppConstants.BiomassTag) < projectObjectCost || PlayerPrefs.GetFloat(AppConstants.GadgetsTag) < projectObjectCost))
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało zasobów", true);
            return;
        }
        if (PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY && (PlayerPrefs.GetFloat(AppConstants.GadgetsTag) < projectObjectCost || PlayerPrefs.GetFloat(AppConstants.FuelTag) < projectObjectCost))
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało zasobów", true);
            return;
        }
        if (PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.INDUSTRY && (PlayerPrefs.GetFloat(AppConstants.FuelTag) < projectObjectCost || PlayerPrefs.GetFloat(AppConstants.BiomassTag) < projectObjectCost))
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało zasobów", true);
            return;
        }
        if (PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE)
        {
            ResourcesController.instance.AddBiomass(-projectObjectCost);
            ResourcesController.instance.AddGadgets(-projectObjectCost);
        }
        if (PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY)
        {
            ResourcesController.instance.AddGadgets(-projectObjectCost);
            ResourcesController.instance.AddFuel(-projectObjectCost);
        }
        if (PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.INDUSTRY)
        {
            ResourcesController.instance.AddFuel(-projectObjectCost);
            ResourcesController.instance.AddBiomass(-projectObjectCost);
        }

        Utils.Web.PostValues(AppConstants.AddProjectObjectUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
                new KeyValuePair<string, string>("LAT", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).latitude.ToString()),
                new KeyValuePair<string, string>("LON", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).longitude.ToString())
        }), (code, response) =>
        {
            if (code == 200)
            {
                long id = Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "id");
                ProjectObject projectObject = new ProjectObject(id, PlayerColliderController.instance.transform.position, PlayerPrefs.GetString(AppConstants.NickTag), PlayerPrefs.GetInt(AppConstants.FractionTag));
                AddProjectObject(projectObject);
                PlayerPrefs.SetInt(AppConstants.ProjectObjectsCountTag, (PlayerPrefs.GetInt(AppConstants.ProjectObjectsCountTag) + 1));
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log(response);
                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
            }
        });
    }

}