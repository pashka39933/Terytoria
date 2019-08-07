using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaController : MonoBehaviour {

    public static ArenaController instance;
    ArenaController() { instance = this; }

    // Spotlight prefab
    public GameObject spotlightPrefab;

    // Arena center
    public Vector3 arenaCenter;

    // Arena range
    public float arenaRadius;

    // Init method
    public void Init(Vector3 position, float range)
    {
        GameObject spotlight = Instantiate(spotlightPrefab, this.transform);
        spotlight.transform.position = new Vector3(position.x, 2000, position.z);
        spotlight.GetComponent<Light>().spotAngle = range / 8f;
        this.arenaCenter = position;
        this.arenaRadius = range;
    }

    // Check if in arena method
    public bool CheckIfWithinArena(Vector3 position)
    {
        return Vector3.Distance(arenaCenter, position) < arenaRadius * 2f;
    }

}
