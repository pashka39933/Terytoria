using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreesController : MonoBehaviour
{

    public static TreesController instance;
    TreesController() { instance = this; }

    public GameObject treePrefab;

    public List<TreeController> trees = new List<TreeController>();

    // Adding tree method
    public void AddTree(Tree tree)
    {
        if (trees.FindIndex(x => x.treeData.id.Equals(tree.id)) > -1)
        {
            trees.Find(x => x.treeData.id.Equals(tree.id)).treeData = tree;
            return;
        }

        TreeController treeController = Instantiate(treePrefab, this.transform).GetComponent<TreeController>();
        treeController.treeData = tree;
        trees.Add(treeController);
        treeController.name = tree.nick;
        treeController.transform.position = new Vector3(tree.position.x, 0, tree.position.z);
        treeController.GetComponent<CircleLineRenderer>().CreatePoints(16, AppConstants.Object1Range / 6f, 0.1f);
        treeController.GetComponent<SphereCollider>().enabled = true;
    }

    // Creating tree method
    public void CreateTreeAtUserPosition(bool confirmed = false)
    {
        if (DownButtonController.instance.standardMenuButtonsVisible)
            DownButtonController.instance.ToggleStandardMenu();

        PatchController userPatch = PatchesController.instance.patches.Find(x => x.patchData.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        List<Vector2> patchPolygon = new List<Vector2>();
        foreach (Vector3 patchPolygonVertex in userPatch.patchData.flags)
            patchPolygon.Add(new Vector2(patchPolygonVertex.x, patchPolygonVertex.z));
        if (userPatch == null || !Geometry.PolygonContainsPoint(patchPolygon, new Vector2(PlayerColliderController.instance.transform.position.x, PlayerColliderController.instance.transform.position.z)))
        {
            BannerController.instance.showBannerWithText(true, "Możesz umieścić drzewo tylko w obrębie własnego rewiru", true);
            return;
        }
        if (!PlayerColliderController.instance.CanPlaceObjectHere())
        {
            BannerController.instance.showBannerWithText(true, "Zbyt blisko innego obiektu! Oddal się kilka metrów", true);
            return;
        }
        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { CreateTreeAtUserPosition(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz tutaj umieścić drzewo za " + AppConstants.TreePurchaseCost + " biomasy ?");
            return;
        }
        if(PlayerPrefs.GetFloat(AppConstants.BiomassTag) < AppConstants.TreePurchaseCost)
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało biomasy", true);
            return;
        }
        ResourcesController.instance.AddBiomass(-AppConstants.TreePurchaseCost);

        Utils.Web.PostValues(AppConstants.AddTreeUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
                new KeyValuePair<string, string>("LAT", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).latitude.ToString()),
                new KeyValuePair<string, string>("LON", GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).longitude.ToString())
        }), (code, response) =>
        {
            if (code == 200)
            {
                int ts = (int)Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "ts");
                long id = Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "id");
                Tree tree = new Tree(id, PlayerColliderController.instance.transform.position, PlayerPrefs.GetString(AppConstants.NickTag), ts, ts, ts);
                AddTree(tree);
            }
            else
            {
                Debug.Log(response);
                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
            }
        });
    }

}