using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerritoriesController : MonoBehaviour {

    public static TerritoriesController instance;
    TerritoriesController() { instance = this; }

    public GameObject TerritoryBorderParticle;

    public Material natureParticleMaterial, commercyParticleMaterial, industryParticleMaterial;
    public Color natureParticleColor, commercyParticleColor, industryParticleColor;

    public List<TerritoryController> territories = new List<TerritoryController>();

    // Initialization
    private void Start()
    {
        StartCoroutine(SearchForTerritories());
    }

    // Territories searching coroutine
    IEnumerator SearchForTerritories()
    {
        while (true)
        {
            // Getting all nature patches corners
            List<PatchController> naturePatches = PatchesController.instance.patches.FindAll(x => x.patchData.fraction == (int)AppConstants.Fraction.NATURE);
            List<List<Vector2>> naturePatchesCorners = new List<List<Vector2>>();
            foreach (PatchController naturePatch in naturePatches)
            {
                List<Vector2> naturePatchCorners = new List<Vector2>();
                foreach (Vector3 flag in naturePatch.patchData.flags)
                    naturePatchCorners.Add(new Vector2(flag.x, flag.z));
                naturePatchesCorners.Add(naturePatchCorners);
            }
            yield return new WaitForEndOfFrame();

            // Counting nature territories
            List<KeyValuePair<List<Vector2>, int>> natureTerritories = Geometry.GetAllPolygonsMinimalIntersections(naturePatchesCorners);
            yield return new WaitForEndOfFrame();

            // Removing territories without borders
            for (int i = 0; i < natureTerritories.Count; i++)
            {
                foreach (Vector2 territoryCorner in natureTerritories[i].Key)
                {
                    bool removed = false;
                    foreach (PatchController naturePatch in naturePatches)
                    {
                        for (int j = 1; j < naturePatch.patchData.flags.Count; j++)
                        {
                            Vector2 lineA = new Vector2(naturePatch.patchData.flags[j - 1].x, naturePatch.patchData.flags[j - 1].z);
                            Vector2 lineB = new Vector2(naturePatch.patchData.flags[j].x, naturePatch.patchData.flags[j].z);
                            if (Geometry.IsPointOnLine(lineA, lineB, territoryCorner))
                            {
                                if (!naturePatch.patchData.borders[j - 1])
                                {
                                    natureTerritories.RemoveAt(i);
                                    i--;
                                    removed = true;
                                }
                            }
                            if (removed)
                                break;
                        }
                        if (removed)
                            break;
                    }
                    if (removed)
                        break;
                }
            }
            yield return new WaitForEndOfFrame();

            // Creating territories
            foreach (KeyValuePair<List<Vector2>, int> natureTerritory in natureTerritories)
            {
                int territoryId = GetOrderIndependentHashCode(natureTerritory.Key);
                TerritoryController tc = territories.Find(x => x.territoryData.id.Equals(territoryId));
                if (tc == null)
                {
                    GameObject territoryObject = new GameObject("NatureTerritory");
                    territoryObject.transform.parent = this.transform;
                    TerritoryController controller = territoryObject.AddComponent<TerritoryController>();
                    controller.territoryData = new Territory(territoryId, natureTerritory.Key, (int)AppConstants.Fraction.NATURE, natureTerritory.Value);
                    territories.Add(controller);
                }
                else
                {
                    tc.territoryData.id = territoryId;
                    tc.territoryData.corners = natureTerritory.Key;
                    tc.territoryData.fraction = (int)AppConstants.Fraction.NATURE;
                    tc.territoryData.patchesJoined = natureTerritory.Value;
                }
            }
            yield return new WaitForEndOfFrame();

            // Getting all commercy patches corners
            List<PatchController> commercyPatches = PatchesController.instance.patches.FindAll(x => x.patchData.fraction == (int)AppConstants.Fraction.COMMERCY);
            List<List<Vector2>> commercyPatchesCorners = new List<List<Vector2>>();
            foreach (PatchController commercyPatch in commercyPatches)
            {
                List<Vector2> commercyPatchCorners = new List<Vector2>();
                foreach (Vector3 flag in commercyPatch.patchData.flags)
                    commercyPatchCorners.Add(new Vector2(flag.x, flag.z));
                commercyPatchesCorners.Add(commercyPatchCorners);
            }
            yield return new WaitForEndOfFrame();

            // Counting commercy territories
            List<KeyValuePair<List<Vector2>, int>> commercyTerritories = Geometry.GetAllPolygonsMinimalIntersections(commercyPatchesCorners);
            yield return new WaitForEndOfFrame();

            // Removing territories without borders
            for (int i = 0; i < commercyTerritories.Count; i++)
            {
                foreach (Vector2 territoryCorner in commercyTerritories[i].Key)
                {
                    bool removed = false;
                    foreach (PatchController commercyPatch in commercyPatches)
                    {
                        for (int j = 1; j < commercyPatch.patchData.flags.Count; j++)
                        {
                            Vector2 lineA = new Vector2(commercyPatch.patchData.flags[j - 1].x, commercyPatch.patchData.flags[j - 1].z);
                            Vector2 lineB = new Vector2(commercyPatch.patchData.flags[j].x, commercyPatch.patchData.flags[j].z);
                            if (Geometry.IsPointOnLine(lineA, lineB, territoryCorner))
                            {
                                if (!commercyPatch.patchData.borders[j - 1])
                                {
                                    commercyTerritories.RemoveAt(i);
                                    i--;
                                    removed = true;
                                }
                            }
                            if (removed)
                                break;
                        }
                        if (removed)
                            break;
                    }
                    if (removed)
                        break;
                }
            }
            yield return new WaitForEndOfFrame();

            // Creating territories
            foreach (KeyValuePair<List<Vector2>, int> commercyTerritory in commercyTerritories)
            {
                int territoryId = GetOrderIndependentHashCode(commercyTerritory.Key);
                TerritoryController tc = territories.Find(x => x.territoryData.id.Equals(territoryId));
                if (tc == null)
                {
                    GameObject territoryObject = new GameObject("CommercyTerritory");
                    territoryObject.transform.parent = this.transform;
                    TerritoryController controller = territoryObject.AddComponent<TerritoryController>();
                    controller.territoryData = new Territory(territoryId, commercyTerritory.Key, (int)AppConstants.Fraction.COMMERCY, commercyTerritory.Value);
                    territories.Add(controller);
                }
                else
                {
                    tc.territoryData.id = territoryId;
                    tc.territoryData.corners = commercyTerritory.Key;
                    tc.territoryData.fraction = (int)AppConstants.Fraction.COMMERCY;
                    tc.territoryData.patchesJoined = commercyTerritory.Value;
                }
            }
            yield return new WaitForEndOfFrame();

            // Getting all industry patches corners
            List<PatchController> industryPatches = PatchesController.instance.patches.FindAll(x => x.patchData.fraction == (int)AppConstants.Fraction.INDUSTRY);
            List<List<Vector2>> industryPatchesCorners = new List<List<Vector2>>();
            foreach (PatchController industryPatch in industryPatches)
            {
                List<Vector2> industryPatchCorners = new List<Vector2>();
                foreach (Vector3 flag in industryPatch.patchData.flags)
                    industryPatchCorners.Add(new Vector2(flag.x, flag.z));
                industryPatchesCorners.Add(industryPatchCorners);
            }
            yield return new WaitForEndOfFrame();

            // Counting industry territories
            List<KeyValuePair<List<Vector2>, int>> industryTerritories = Geometry.GetAllPolygonsMinimalIntersections(industryPatchesCorners);
            yield return new WaitForEndOfFrame();

            // Removing territories without borders
            for (int i = 0; i < industryTerritories.Count; i++)
            {
                foreach (Vector2 territoryCorner in industryTerritories[i].Key)
                {
                    bool removed = false;
                    foreach (PatchController industryPatch in industryPatches)
                    {
                        for (int j = 1; j < industryPatch.patchData.flags.Count; j++)
                        {
                            Vector2 lineA = new Vector2(industryPatch.patchData.flags[j - 1].x, industryPatch.patchData.flags[j - 1].z);
                            Vector2 lineB = new Vector2(industryPatch.patchData.flags[j].x, industryPatch.patchData.flags[j].z);
                            if (Geometry.IsPointOnLine(lineA, lineB, territoryCorner))
                            {
                                if (!industryPatch.patchData.borders[j - 1])
                                {
                                    industryTerritories.RemoveAt(i);
                                    i--;
                                    removed = true;
                                }
                            }
                            if (removed)
                                break;
                        }
                        if (removed)
                            break;
                    }
                    if (removed)
                        break;
                }
            }
            yield return new WaitForEndOfFrame();

            // Creating territories
            foreach (KeyValuePair<List<Vector2>, int> industryTerritory in industryTerritories)
            {
                int territoryId = GetOrderIndependentHashCode(industryTerritory.Key);
                TerritoryController tc = territories.Find(x => x.territoryData.id.Equals(territoryId));
                if (tc == null)
                {
                    GameObject territoryObject = new GameObject("IndustryTerritory");
                    territoryObject.transform.parent = this.transform;
                    TerritoryController controller = territoryObject.AddComponent<TerritoryController>();
                    controller.territoryData = new Territory(territoryId, industryTerritory.Key, (int)AppConstants.Fraction.INDUSTRY, industryTerritory.Value);
                    territories.Add(controller);
                }
                else
                {
                    tc.territoryData.id = territoryId;
                    tc.territoryData.corners = industryTerritory.Key;
                    tc.territoryData.fraction = (int)AppConstants.Fraction.INDUSTRY;
                    tc.territoryData.patchesJoined = industryTerritory.Value;
                }
            }
            yield return new WaitForEndOfFrame();

            // Removing inactive teritorries
            for (int i = 0; i < territories.Count; i++)
            {
                bool toDelete = true;
                if (natureTerritories.FindIndex(x => GetOrderIndependentHashCode(x.Key).Equals(territories[i].territoryData.id)) > -1)
                {
                    toDelete = false;
                }
                if (toDelete)
                {
                    if (commercyTerritories.FindIndex(x => GetOrderIndependentHashCode(x.Key).Equals(territories[i].territoryData.id)) > -1)
                    {
                        toDelete = false;
                    }
                }
                if (toDelete)
                {
                    if (industryTerritories.FindIndex(x => GetOrderIndependentHashCode(x.Key).Equals(territories[i].territoryData.id)) > -1)
                    {
                        toDelete = false;
                    }
                }
                if (toDelete)
                {
                    territories[i].Destroy();
                }
            }
            yield return new WaitForEndOfFrame();


            // Setting objects boosts
            // Generators
            foreach (GeneratorController obj in GeneratorsController.instance.generators)
            {
                obj.TerritoryBoosted = 0;
                Vector2 position = new Vector2(obj.generatorData.position.x, obj.generatorData.position.z);
                foreach (TerritoryController territory in territories)
                    if (Geometry.PolygonContainsPoint(territory.territoryData.corners, position))
                        obj.TerritoryBoosted += territory.territoryData.patchesJoined;
                obj.SetupTerritoryBoostText();
            }

            yield return new WaitForEndOfFrame();

            // Trees
            foreach (TreeController obj in TreesController.instance.trees)
            {
                obj.TerritoryBoosted = 0;
                Vector2 position = new Vector2(obj.treeData.position.x, obj.treeData.position.z);
                foreach (TerritoryController territory in territories)
                    if (Geometry.PolygonContainsPoint(territory.territoryData.corners, position))
                        obj.TerritoryBoosted += territory.territoryData.patchesJoined;
                obj.SetupTerritoryBoostText();
            }

            yield return new WaitForEndOfFrame();

            // Automations
            foreach (AutomationController obj in AutomationsController.instance.automations)
            {
                obj.TerritoryBoosted = 0;
                Vector2 position = new Vector2(obj.automationData.position.x, obj.automationData.position.z);
                foreach (TerritoryController territory in territories)
                    if (Geometry.PolygonContainsPoint(territory.territoryData.corners, position))
                        obj.TerritoryBoosted += territory.territoryData.patchesJoined;
                obj.SetupTerritoryBoostText();
            }

            yield return new WaitForEndOfFrame();

            // Wells
            foreach (WellController obj in WellsController.instance.wells)
            {
                obj.TerritoryBoosted = 0;
                Vector2 position = new Vector2(obj.wellData.position.x, obj.wellData.position.z);
                foreach (TerritoryController territory in territories)
                    if (Geometry.PolygonContainsPoint(territory.territoryData.corners, position))
                        obj.TerritoryBoosted += territory.territoryData.patchesJoined;
                obj.SetupTerritoryBoostText();
            }

            yield return new WaitForEndOfFrame();

            // Project Objects
            foreach (ProjectObjectController obj in ProjectObjectsController.instance.projectObjects)
            {
                obj.TerritoryBoosted = 0;
                Vector2 position = new Vector2(obj.projectObjectData.position.x, obj.projectObjectData.position.z);
                foreach (TerritoryController territory in territories)
                    if (Geometry.PolygonContainsPoint(territory.territoryData.corners, position))
                        obj.TerritoryBoosted += territory.territoryData.patchesJoined;
                obj.SetupTerritoryBoostText();
            }

            yield return new WaitForSeconds(AppConstants.ServerSynchronizationIntervalInSeconds);
        }
    }

    static int GetOrderIndependentHashCode<T>(IEnumerable<T> source)
    {
        int hash = 0;
        foreach (T element in source)
        {
            hash = hash ^ EqualityComparer<T>.Default.GetHashCode(element);
        }
        return hash;
    }

}
