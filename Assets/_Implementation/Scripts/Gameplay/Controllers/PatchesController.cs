using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PatchesController : MonoBehaviour
{

    public static PatchesController instance;
    PatchesController() { instance = this; }

    public List<PatchController> patches = new List<PatchController>();

    public GameObject PatchBorder;

    public Material natureWallMaterial, commercyWallMaterial, industryWallMaterial, ownWallMaterial;

    public Sprite natureSprite, commercySprite, industrySprite;

    public GameObject fader;
    public Animation faderAnim;

    public GameObject downMenuButton, edgeChooseModeButton;

    public GameObject patchHighlightPrefab;

    // Draw patch method
    public void AddPatch(Patch patch, float overrideY = 0, bool nullParent = false)
    {
        PatchController foundPatch = patches.Find(x => x.patchData.nick.Equals(patch.nick));
        if (foundPatch != null)
        {
            foundPatch.patchData = patch;
            return;
        }

        GameObject patchObject = new GameObject(patch.nick + "_" + patch.fraction);
        if (nullParent)
        {
            patchObject.transform.parent = null;
        }
        else
        {
            patchObject.transform.parent = this.transform;
            PatchController patchController = patchObject.AddComponent<PatchController>();
            patchController.patchData = patch;
            patches.Add(patchController);
        }

        for (int i = 1; i <= patch.flags.Count; i++)
        {
            int vertex1 = (i - 1) % patch.flags.Count;
            int vertex2 = i % patch.flags.Count;
            Vector3 particlePosition = new Vector3((patch.flags[vertex1].x + patch.flags[vertex2].x) / 2, overrideY, (patch.flags[vertex1].z + patch.flags[vertex2].z) / 2);
            float particleRadius = Vector3.Distance(patch.flags[vertex1], patch.flags[vertex2]) / 2;

            bool condition = patch.flags[vertex1].x < patch.flags[vertex2].x;
            Vector3 particleRotation = new Vector3(
                0,
                Vector3.Angle(
                    condition ? Vector3.forward : Vector3.back,
                    patch.flags[vertex2] - patch.flags[vertex1]
                ) + 90,
                0
            );

            GameObject patchWall = Instantiate(PatchBorder, patchObject.transform);
            patchWall.transform.position = new Vector3(particlePosition.x, 0, particlePosition.z);
            patchWall.transform.eulerAngles = particleRotation;
            patchWall.transform.GetChild(0).localScale = new Vector3(particleRadius * 2f, patch.borders[i - 1] ? 10f : 3f, 0.5f);
            Material material = patch.fraction == (int)AppConstants.Fraction.NATURE ? natureWallMaterial : patch.fraction == (int)AppConstants.Fraction.COMMERCY ? commercyWallMaterial : industryWallMaterial;
            patchWall.transform.GetChild(0).GetComponent<MeshRenderer>().material = material;
            patchWall.transform.GetChild(0).GetComponent<MeshRenderer>().material = patch.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)) ? ownWallMaterial : material;

        }

    }

    // Remove own patch method
    public void RemoveOwnPatch(bool confirmed = false)
    {
        if (DownButtonController.instance.standardMenuButtonsVisible)
            DownButtonController.instance.ToggleStandardMenu();

        PatchController userPatch = PatchesController.instance.patches.Find(x => x.patchData.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));
        List<Vector2> patchPolygon = new List<Vector2>();
        foreach (Vector3 patchPolygonVertex in userPatch.patchData.flags)
            patchPolygon.Add(new Vector2(patchPolygonVertex.x, patchPolygonVertex.z));
        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { RemoveOwnPatch(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz usunąć swój rewir?");
            return;
        }

        Utils.Web.PostValues(AppConstants.RemovePatchUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID)
        }), (code, response) =>
        {
            if (code == 200)
            {
                PlayerPrefs.DeleteKey("PatchCreatedTag");
                PlayerPrefs.DeleteKey("GeneratorCreatedTag");
                PlayerPrefs.DeleteKey("GeneratorVisitTimestampTag");
                PlayerPrefs.DeleteKey("ConverterCreatedTag");
                PlayerPrefs.DeleteKey("BatteryCreatedTag");
                PlayerPrefs.DeleteKey("ProjectObjectsCountTag");
                PlayerPrefs.Save();

                List<GeneratorController> generators = GeneratorsController.instance.generators.FindAll(x => x.generatorData.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));
                List<ProjectObjectController> projectObjects = ProjectObjectsController.instance.projectObjects.FindAll(x => x.projectObjectData.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));
                List<AutomationController> automations = AutomationsController.instance.automations.FindAll(x => x.automationData.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));
                List<TreeController> trees = TreesController.instance.trees.FindAll(x => x.treeData.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));
                List<WellController> wells = WellsController.instance.wells.FindAll(x => x.wellData.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));

                int energySpent = 0;
                int biomassSpent = 0;
                int gadgetsSpent = 0;
                int fuelSpent = 0;

                foreach (GeneratorController generator in generators)
                {
                    if (PlayerPrefs.GetInt(AppConstants.GeneratorCreatedTag) == 1)
                    {
                        for (int i = 1; i < generator.generatorData.level; i++)
                        {
                            energySpent += (i * AppConstants.GeneratorUpgradeConstant);
                        }
                    }
                    if (PlayerPrefs.GetInt(AppConstants.BatteryCreatedTag) == 1)
                    {
                        energySpent += AppConstants.GeneratorBatteryPurchaseCost;
                        for (int i = 1; i < generator.generatorData.batteryLevel; i++)
                        {
                            energySpent += (i * AppConstants.GeneratorBatteryUpgradeConstant);
                        }
                    }
                    if (PlayerPrefs.GetInt(AppConstants.ConverterCreatedTag) == 1)
                    {
                        energySpent += AppConstants.GeneratorConverterPurchaseCost;
                        for (int i = 1; i < generator.generatorData.converterLevel; i++)
                        {
                            energySpent += (i * AppConstants.GeneratorConverterUpgradeConstant);
                        }
                    }
                }

                gadgetsSpent += (automations.Count * AppConstants.AutomationPurchaseCost);

                biomassSpent += (trees.Count * AppConstants.TreePurchaseCost);

                fuelSpent += (wells.Count * AppConstants.WellPurchaseCost);
                foreach (WellController well in wells)
                {
                    for (int i = 1; i < well.wellData.level; i++)
                    {
                        fuelSpent += (i * AppConstants.WellUpgradeCostConstant);
                    }
                }

                int projectObjectsTotalCostSingleResource = 0;
                for (int i = 0; i < projectObjects.Count; i++)
                {
                    projectObjectsTotalCostSingleResource += ((i + 1) * AppConstants.ProjectObjectPurchaseCost);
                }
                if (PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE)
                {
                    biomassSpent += projectObjectsTotalCostSingleResource;
                    gadgetsSpent += projectObjectsTotalCostSingleResource;
                }
                else if (PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY)
                {
                    gadgetsSpent += projectObjectsTotalCostSingleResource;
                    fuelSpent += projectObjectsTotalCostSingleResource;
                }
                else
                {
                    fuelSpent += projectObjectsTotalCostSingleResource;
                    biomassSpent += projectObjectsTotalCostSingleResource;
                }

                int energyAdded = Mathf.RoundToInt((float)energySpent / AppConstants.PatchRemoveReturnedResourcesCoeff);
                ResourcesController.instance.AddEnergy(energyAdded);
                int biomassAdded = Mathf.RoundToInt((float)biomassSpent / AppConstants.PatchRemoveReturnedResourcesCoeff);
                ResourcesController.instance.AddBiomass(biomassAdded);
                int gadgetsAdded = Mathf.RoundToInt((float)gadgetsSpent / AppConstants.PatchRemoveReturnedResourcesCoeff);
                ResourcesController.instance.AddGadgets(gadgetsAdded);
                int fuelAdded = Mathf.RoundToInt((float)fuelSpent / AppConstants.PatchRemoveReturnedResourcesCoeff);
                ResourcesController.instance.AddFuel(fuelAdded);

                InfoPopup.instance.Open(() => { StartCoroutine(RestartSceneCoroutine()); InfoPopup.instance.Close(); }, "Usunięto wszystkie Twoje obiekty. Dodano " + energyAdded + " energii, " + biomassAdded + " biomasy, " + gadgetsAdded + " gadżetów i " + fuelAdded + " paliwa");
            }
            else
            {
                Debug.Log(response);
                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
            }
        });
    }

    // Restart scene coroutine
    IEnumerator RestartSceneCoroutine()
    {
        fader.GetComponentInChildren<Image>().fillAmount = 0;
        faderAnim.Play("fadeIn");
        yield return new WaitWhile(() => faderAnim.isPlaying);
        Scene loadedLevel = SceneManager.GetActiveScene();
        SceneManager.LoadScene(loadedLevel.buildIndex);
    }

    // Choose patch edge mode
    bool choosePatchModeEnded = false;
    public void ChoosePatchEdgeModeToggle()
    {
        if (DownButtonController.instance.standardMenuButtonsVisible)
            DownButtonController.instance.ToggleStandardMenu();

        PatchController userPatch = PatchesController.instance.patches.Find(x => x.patchData.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        List<Vector2> patchPolygon = new List<Vector2>();
        foreach (Vector3 patchPolygonVertex in userPatch.patchData.flags)
            patchPolygon.Add(new Vector2(patchPolygonVertex.x, patchPolygonVertex.z));
        if (userPatch == null || !Geometry.PolygonContainsPoint(patchPolygon, new Vector2(PlayerColliderController.instance.transform.position.x, PlayerColliderController.instance.transform.position.z)))
        {
            BannerController.instance.showBannerWithText(true, "Możesz postawić granicę będąc w obrębie własnego rewiru", true);
            return;
        }
        if (userPatch.patchData.borders.FindIndex(x => !x) == -1)
        {
            BannerController.instance.showBannerWithText(true, "Nie możesz postawić już więcej granic", true);
            return;
        }

        Patch patch = userPatch.patchData;
        Vector3 centroid = Geometry.CalculateCentroid(patch.flags);
        float maxDiagonalLength = float.MinValue;
        foreach (Vector3 flagPosition1 in patch.flags)
        {
            foreach (Vector3 flagPosition2 in patch.flags)
            {
                if (Vector3.Distance(flagPosition1, flagPosition2) > maxDiagonalLength)
                    maxDiagonalLength = Vector3.Distance(flagPosition1, flagPosition2);
            }
        }
        StartCoroutine(SmoothPatchViewController(centroid, maxDiagonalLength));
    }

    // SkyView camera animation on patch
    IEnumerator SmoothPatchViewController(Vector3 centroid, float maxDiagonalLength)
    {
        fader.GetComponentInChildren<Image>().fillAmount = 0;
        faderAnim.Play("fadeIn");
        yield return new WaitWhile(() => faderAnim.isPlaying);

        Camera.main.gameObject.GetComponent<GoShared.GOOrbit>().enabled = false;

        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraRotation = Camera.main.transform.eulerAngles;
        float fieldOfView = Camera.main.fieldOfView;
        Transform parent = Camera.main.transform.parent;

        Camera.main.transform.position = new Vector3(centroid.x, 100, centroid.z);
        Camera.main.transform.eulerAngles = new Vector3(90, 0, 0);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = maxDiagonalLength * AppConstants.PatchTopViewCameraOrthographicSizeConstant;
        Camera.main.transform.parent = null;

        downMenuButton.SetActive(false);
        edgeChooseModeButton.SetActive(true);
        SwitchToNextEdge();

        faderAnim.Play("fadeOut");

        yield return new WaitUntil(() => choosePatchModeEnded);
        choosePatchModeEnded = false;

        fader.GetComponentInChildren<Image>().fillAmount = 0;
        faderAnim.Play("fadeIn");
        yield return new WaitWhile(() => faderAnim.isPlaying);

        Camera.main.orthographic = false;
        Camera.main.fieldOfView = fieldOfView;
        Camera.main.transform.parent = parent;

        Camera.main.transform.position = cameraPosition;
        Camera.main.transform.eulerAngles = cameraRotation;
        Camera.main.gameObject.GetComponent<GoShared.GOOrbit>().enabled = true;

        downMenuButton.SetActive(true);
        edgeChooseModeButton.SetActive(false);
        DestroyPatchHighlight();

        faderAnim.Play("fadeOut");
    }

    // Highlighting next patch edge (border purchasing)
    public void SwitchToNextEdge()
    {
        PatchController userPatch = PatchesController.instance.patches.Find(x => x.patchData.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        userPatch.HighlightNextEdge();
    }

    // Destroying patch edge highlight
    public void DestroyPatchHighlight()
    {
        PatchController userPatch = PatchesController.instance.patches.Find(x => x.patchData.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        userPatch.RemoveHighlight();
    }

    // Accepting border purchase
    public void AddBorder(bool confirmed = false)
    {
        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { AddBorder(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz postawić granicę na tej krawędzi za " + AppConstants.BorderPurchaseCost + " jednostek wszystkich zasobów?");
            return;
        }
        if (PlayerPrefs.GetFloat(AppConstants.EnergyTag) < AppConstants.BorderPurchaseCost || PlayerPrefs.GetFloat(AppConstants.BiomassTag) < AppConstants.BorderPurchaseCost || PlayerPrefs.GetFloat(AppConstants.GadgetsTag) < AppConstants.BorderPurchaseCost || PlayerPrefs.GetFloat(AppConstants.FuelTag) < AppConstants.BorderPurchaseCost)
        {
            BannerController.instance.showBannerWithText(true, "Masz zbyt mało zasobów", true);
            return;
        }

        PatchController userPatch = PatchesController.instance.patches.Find(x => x.patchData.nick == PlayerPrefs.GetString(AppConstants.NickTag));
        int vertexIndex = userPatch.GetHighlightedEdgeIndex();

        Utils.Web.PostValues(AppConstants.AddBorderUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
                new KeyValuePair<string, string>("VERTEX_INDEX", (vertexIndex + 1).ToString())
        }), (code, response) =>
        {
            if (code == 200)
            {
                userPatch.patchData.borders[vertexIndex] = true;
                choosePatchModeEnded = true;

                ResourcesController.instance.AddEnergy(-AppConstants.BorderPurchaseCost);
                ResourcesController.instance.AddBiomass(-AppConstants.BorderPurchaseCost);
                ResourcesController.instance.AddGadgets(-AppConstants.BorderPurchaseCost);
                ResourcesController.instance.AddFuel(-AppConstants.BorderPurchaseCost);
            }
            else
            {
                Debug.Log(response);
                BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
            }
        });
    }

    // Exit button
    public void ExitButton()
    {
        choosePatchModeEnded = true;
    }

}