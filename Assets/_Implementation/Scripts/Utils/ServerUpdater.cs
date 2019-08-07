using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerUpdater : MonoBehaviour
{

    // Map
    public GoMap.GOMap map;

    // Initialization
    private void OnEnable()
    {
        StartCoroutine(Sync());
    }

    // Sync Coroutine
    IEnumerator Sync()
    {
        while (true)
        {
            yield return new WaitUntil(() => GameInitializer.loggedIn);
            yield return new WaitUntil(() => map.tiles.Count > 0);

            string automationsDeltas = "";
            foreach (AutomationController automation in AutomationsController.instance.automations)
            {
                long automationID = automation.automationData.id;
                if (PlayerPrefs.HasKey(AppConstants.AutomationsEnergyDelta + "_" + automationID) || PlayerPrefs.HasKey(AppConstants.AutomationsBiomassDelta + "_" + automationID) || PlayerPrefs.HasKey(AppConstants.AutomationsFuelDelta + "_" + automationID))
                {
                    automationsDeltas = automationsDeltas + (automationsDeltas.Length > 0 ? ";" : "") + automationID + ":";
                    automationsDeltas = automationsDeltas + PlayerPrefs.GetFloat(AppConstants.AutomationsEnergyDelta + "_" + automationID) + ",";
                    automationsDeltas = automationsDeltas + PlayerPrefs.GetFloat(AppConstants.AutomationsBiomassDelta + "_" + automationID) + ",";
                    automationsDeltas = automationsDeltas + PlayerPrefs.GetFloat(AppConstants.AutomationsFuelDelta + "_" + automationID);
                }
            }

            double LAT = GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).latitude;
            double LON = GoShared.Coordinates.convertVectorToCoordinates(PlayerColliderController.instance.transform.position).longitude;
            double RADIUS = map.tiles[0].goTile.diagonalLenght / AppConstants.SyncWorldDataDownloadRangeTileDivisionConstant;

            bool requestCompleted = false;
            Utils.Web.PostValues(AppConstants.UpdatePlayerStateUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {

                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
                new KeyValuePair<string, string>("RESOURCE0", PlayerPrefs.GetFloat(AppConstants.EnergyTag).ToString()),
                new KeyValuePair<string, string>("RESOURCE1", PlayerPrefs.GetFloat(AppConstants.BiomassTag).ToString()),
                new KeyValuePair<string, string>("RESOURCE2", PlayerPrefs.GetFloat(AppConstants.GadgetsTag).ToString()),
                new KeyValuePair<string, string>("RESOURCE3", PlayerPrefs.GetFloat(AppConstants.FuelTag).ToString()),
                new KeyValuePair<string, string>("PROJECT_POINTS1", PlayerPrefs.GetInt(AppConstants.NatureDeltaPoints).ToString()),
                new KeyValuePair<string, string>("PROJECT_POINTS2", PlayerPrefs.GetInt(AppConstants.CommercyDeltaPoints).ToString()),
                new KeyValuePair<string, string>("PROJECT_POINTS3", PlayerPrefs.GetInt(AppConstants.IndustryDeltaPoints).ToString()),
                new KeyValuePair<string, string>("GENERATOR_VISIT_TIMESTAMP", PlayerPrefs.GetInt(AppConstants.GeneratorVisitTimestampTag).ToString()),
                new KeyValuePair<string, string>("AUTOMATIONS_DELTAS", automationsDeltas),
                new KeyValuePair<string, string>("ONESIGNAL_ID", AppConstants.OneSignalID),
                new KeyValuePair<string, string>("LAT", LAT.ToString()),
                new KeyValuePair<string, string>("LON", LON.ToString()),
                new KeyValuePair<string, string>("RADIUS", RADIUS.ToString())

            }), (code, response) =>
            {
                if (code == 200)
                {
                    PlayerPrefs.DeleteKey(AppConstants.NatureDeltaPoints);
                    PlayerPrefs.DeleteKey(AppConstants.CommercyDeltaPoints);
                    PlayerPrefs.DeleteKey(AppConstants.IndustryDeltaPoints);

                    foreach (AutomationController automation in AutomationsController.instance.automations)
                    {
                        long automationID = automation.automationData.id;
                        PlayerPrefs.DeleteKey(AppConstants.AutomationsEnergyDelta + "_" + automationID);
                        PlayerPrefs.DeleteKey(AppConstants.AutomationsBiomassDelta + "_" + automationID);
                        PlayerPrefs.DeleteKey(AppConstants.AutomationsFuelDelta + "_" + automationID);
                    }

                    Dictionary<string, object> json = Utils.JSON.GetDictionary(response);
                    PlayerPrefs.SetInt(AppConstants.NaturePoints, (int)Utils.JSON.GetLong(json, "naturePoints"));
                    PlayerPrefs.SetInt(AppConstants.CommercyPoints, (int)Utils.JSON.GetLong(json, "commercyPoints"));
                    PlayerPrefs.SetInt(AppConstants.IndustryPoints, (int)Utils.JSON.GetLong(json, "industryPoints"));
                    PlayerPrefs.Save();

                    ProjectPointsController.instance.Start();

                    int winnerFraction = (int)Utils.JSON.GetLong(json, "winner");
                    if (winnerFraction > 0)
                    {
                        WinnerFractionPopupController.instance.Open(winnerFraction == 1 ? AppConstants.Fraction.NATURE : winnerFraction == 2 ? AppConstants.Fraction.COMMERCY : AppConstants.Fraction.INDUSTRY);
                    }
                    else
                    {
                        WinnerFractionPopupController.instance.Close();
                    }

                    ParseTileDataResponse(Utils.JSON.GetJSON(json, "worldData"));

                    Debug.Log("[ServerUpdater] Sync success");
                }
                else
                {
                    Debug.Log("[ServerUpdater] Sync fail - " + response);
                }

                requestCompleted = true;

            });
            yield return new WaitUntil(() => requestCompleted);
            yield return new WaitForSecondsRealtime(AppConstants.ServerSynchronizationIntervalInSeconds);
        }
    }

    // Method parsing tile data
    public void ParseTileDataResponse(Dictionary<string, object> json)
    {
        // Users
        List<string> checkListString = new List<string>();
        List<Dictionary<string, object>> strangers = Utils.JSON.GetArray<Dictionary<string, object>>(json, "users");
        foreach (Dictionary<string, object> stranger in strangers)
        {
            List<string> coords = Utils.JSON.GetArray<string>(stranger, "coords");
            GoShared.Coordinates coordinate = new GoShared.Coordinates(double.Parse(coords[0]), double.Parse(coords[1]), 0);
            Vector3 strangerPosition = coordinate.convertCoordinateToVector();
            StrangersController.instance.AddStranger(new Stranger(
                Utils.JSON.GetString(stranger, "id"),
                new Vector3(strangerPosition.x, 3, strangerPosition.z),
                Utils.JSON.GetString(stranger, "nick"),
                int.Parse(Utils.JSON.GetString(stranger, "fraction")),
                int.Parse(Utils.JSON.GetString(stranger, "update_timestamp"))
            ));
            checkListString.Add(Utils.JSON.GetString(stranger, "id"));
        }
        List<StrangerController> removeListStrangers = StrangersController.instance.strangers.FindAll(x => !checkListString.Contains(x.strangerData.id));
        foreach (StrangerController stranger in removeListStrangers)
            stranger.Destroy();

        // Patches
        List<long> checkListLong = new List<long>();
        List<Dictionary<string, object>> patches = Utils.JSON.GetArray<Dictionary<string, object>>(json, "patches");
        foreach (Dictionary<string, object> patch in patches)
        {
            List<string> coords = Utils.JSON.GetArray<string>(patch, "coords");
            List<Vector3> flagsPositions = new List<Vector3>();
            for (int i = 1; i < coords.Count; i += 2)
            {
                if (coords[i - 1] != "0" || coords[i] != "0")
                {
                    GoShared.Coordinates coordinate = new GoShared.Coordinates(double.Parse(coords[i - 1]), double.Parse(coords[i]), 0);
                    flagsPositions.Add(coordinate.convertCoordinateToVector());
                }
            }

            List<string> borders = Utils.JSON.GetArray<string>(patch, "borders");
            List<bool> bordersFlags = new List<bool>();
            for (int i = 0; i < flagsPositions.Count; i++)
                bordersFlags.Add(borders[i].Equals("1"));

            PatchesController.instance.AddPatch(new Patch(
                long.Parse(Utils.JSON.GetString(patch, "id")),
                flagsPositions,
                bordersFlags,
                int.Parse(Utils.JSON.GetString(patch, "fraction")),
                Utils.JSON.GetString(patch, "nick")
            ));
            checkListLong.Add(long.Parse(Utils.JSON.GetString(patch, "id")));
        }
        List<PatchController> removeListPatches = PatchesController.instance.patches.FindAll(x => !checkListLong.Contains(x.patchData.id));
        foreach (PatchController patch in removeListPatches)
            patch.Destroy();

        // Generators
        checkListLong = new List<long>();
        List<Dictionary<string, object>> generators = Utils.JSON.GetArray<Dictionary<string, object>>(json, "generators");
        foreach (Dictionary<string, object> generator in generators)
        {
            List<string> coords = Utils.JSON.GetArray<string>(generator, "coords");
            GoShared.Coordinates coordinate = new GoShared.Coordinates(double.Parse(coords[0]), double.Parse(coords[1]), 0);
            Vector3 generatorPosition = coordinate.convertCoordinateToVector();
            GeneratorsController.instance.AddGenerator(new Generator(
                long.Parse(Utils.JSON.GetString(generator, "id")),
                generatorPosition,
                Utils.JSON.GetString(generator, "nick"),
                int.Parse(Utils.JSON.GetString(generator, "level")),
                int.Parse(Utils.JSON.GetString(generator, "battery_level")),
                int.Parse(Utils.JSON.GetString(generator, "converter_level")),
                int.Parse(Utils.JSON.GetString(generator, "converter_from_resource")),
                int.Parse(Utils.JSON.GetString(generator, "converter_to_resource")),
                int.Parse(Utils.JSON.GetString(generator, "converter_changes_count"))
            ));
            checkListLong.Add(long.Parse(Utils.JSON.GetString(generator, "id")));
        }
        List<GeneratorController> removeListGenerators = GeneratorsController.instance.generators.FindAll(x => !checkListLong.Contains(x.generatorData.id));
        foreach (GeneratorController generator in removeListGenerators)
            generator.Destroy();


        // Trees
        checkListLong = new List<long>();
        List<Dictionary<string, object>> trees = Utils.JSON.GetArray<Dictionary<string, object>>(json, "trees");
        foreach (Dictionary<string, object> tree in trees)
        {
            List<string> coords = Utils.JSON.GetArray<string>(tree, "coords");
            GoShared.Coordinates coordinate = new GoShared.Coordinates(double.Parse(coords[0]), double.Parse(coords[1]), 0);
            Vector3 treePosition = coordinate.convertCoordinateToVector();
            TreesController.instance.AddTree(new Tree(
                long.Parse(Utils.JSON.GetString(tree, "id")),
                treePosition,
                Utils.JSON.GetString(tree, "nick"),
                int.Parse(Utils.JSON.GetString(tree, "creation_timestamp")),
                int.Parse(Utils.JSON.GetString(tree, "owner_fruit_consumption_timestamp")),
                int.Parse(Utils.JSON.GetString(tree, "common_fruit_consumption_timestamp"))
            ));
            checkListLong.Add(long.Parse(Utils.JSON.GetString(tree, "id")));
        }
        List<TreeController> removeListTrees = TreesController.instance.trees.FindAll(x => !checkListLong.Contains(x.treeData.id));
        foreach (TreeController tree in removeListTrees)
            tree.Destroy();

        // Automations
        checkListLong = new List<long>();
        List<Dictionary<string, object>> automations = Utils.JSON.GetArray<Dictionary<string, object>>(json, "automations");
        foreach (Dictionary<string, object> automation in automations)
        {
            List<string> coords = Utils.JSON.GetArray<string>(automation, "coords");
            GoShared.Coordinates coordinate = new GoShared.Coordinates(double.Parse(coords[0]), double.Parse(coords[1]), 0);
            Vector3 automationPosition = coordinate.convertCoordinateToVector();
            AutomationsController.instance.AddAutomation(new Automation(
                long.Parse(Utils.JSON.GetString(automation, "id")),
                automationPosition,
                Utils.JSON.GetString(automation, "nick"),
                int.Parse(Utils.JSON.GetString(automation, "converted_resource_type")),
                int.Parse(Utils.JSON.GetString(automation, "owner_gadgets_consumption_timestamp")),
                float.Parse(Utils.JSON.GetString(automation, "converted_energy_amount")),
                float.Parse(Utils.JSON.GetString(automation, "converted_biomass_amount")),
                float.Parse(Utils.JSON.GetString(automation, "converted_fuel_amount"))
            ));
            checkListLong.Add(long.Parse(Utils.JSON.GetString(automation, "id")));
        }
        List<AutomationController> removeListAutomations = AutomationsController.instance.automations.FindAll(x => !checkListLong.Contains(x.automationData.id));
        foreach (AutomationController automation in removeListAutomations)
            automation.Destroy();

        // Wells
        checkListLong = new List<long>();
        List<Dictionary<string, object>> wells = Utils.JSON.GetArray<Dictionary<string, object>>(json, "wells");
        foreach (Dictionary<string, object> well in wells)
        {
            List<string> coords = Utils.JSON.GetArray<string>(well, "coords");
            GoShared.Coordinates coordinate = new GoShared.Coordinates(double.Parse(coords[0]), double.Parse(coords[1]), 0);
            Vector3 wellPosition = coordinate.convertCoordinateToVector();
            WellsController.instance.AddWell(new Well(
                long.Parse(Utils.JSON.GetString(well, "id")),
                wellPosition,
                Utils.JSON.GetString(well, "nick"),
                int.Parse(Utils.JSON.GetString(well, "level"))
            ));
            checkListLong.Add(long.Parse(Utils.JSON.GetString(well, "id")));
        }
        List<WellController> removeListWells = WellsController.instance.wells.FindAll(x => !checkListLong.Contains(x.wellData.id));
        foreach (WellController well in removeListWells)
            well.Destroy();

        // Project objects
        checkListLong = new List<long>();
        List<Dictionary<string, object>> projectObjects = Utils.JSON.GetArray<Dictionary<string, object>>(json, "projectObjects");
        foreach (Dictionary<string, object> projectObject in projectObjects)
        {
            if (int.Parse(Utils.JSON.GetString(projectObject, "fraction")) == PlayerPrefs.GetInt(AppConstants.FractionTag))
            {
                List<string> coords = Utils.JSON.GetArray<string>(projectObject, "coords");
                GoShared.Coordinates coordinate = new GoShared.Coordinates(double.Parse(coords[0]), double.Parse(coords[1]), 0);
                Vector3 projectObjectPosition = coordinate.convertCoordinateToVector();
                ProjectObjectsController.instance.AddProjectObject(new ProjectObject(
                    long.Parse(Utils.JSON.GetString(projectObject, "id")),
                    projectObjectPosition,
                    Utils.JSON.GetString(projectObject, "nick"),
                    int.Parse(Utils.JSON.GetString(projectObject, "fraction"))
                ));
                checkListLong.Add(long.Parse(Utils.JSON.GetString(projectObject, "id")));
            }
        }
        List<ProjectObjectController> removeListProjectObjects = ProjectObjectsController.instance.projectObjects.FindAll(x => !checkListLong.Contains(x.projectObjectData.id));
        foreach (ProjectObjectController projectObject in removeListProjectObjects)
            projectObject.Destroy();

        // Deposits
        checkListString = new List<string>();
        List<Dictionary<string, object>> deposits = Utils.JSON.GetArray<Dictionary<string, object>>(json, "deposits");
        foreach (Dictionary<string, object> deposit in deposits)
        {
            List<string> coords = Utils.JSON.GetArray<string>(deposit, "coords");
            GoShared.Coordinates coordinate = new GoShared.Coordinates(double.Parse(coords[0]), double.Parse(coords[1]), 0);
            Vector3 depositPosition = coordinate.convertCoordinateToVector();
            DepositsController.instance.AddDeposit(new Deposit(
                Utils.JSON.GetString(deposit, "id"),
                depositPosition,
                Utils.JSON.GetString(deposit, "preview_url"),
                Utils.JSON.GetString(deposit, "name"),
                float.Parse(Utils.JSON.GetString(deposit, "receiverEnergy")),
                float.Parse(Utils.JSON.GetString(deposit, "receiverBiomass")),
                float.Parse(Utils.JSON.GetString(deposit, "receiverGadgets")),
                float.Parse(Utils.JSON.GetString(deposit, "receiverFuel"))
            ));
            checkListString.Add(Utils.JSON.GetString(deposit, "id"));
        }
        List<DepositController> removeListDeposits = DepositsController.instance.deposits.FindAll(x => !checkListString.Contains(x.depositData.id));
        foreach (DepositController deposit in removeListDeposits)
            deposit.Destroy();

    }

}