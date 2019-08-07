using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Territory
{
    
    public int id;
    public List<Vector2> corners;
    public int fraction;
    public int patchesJoined;

    public Territory(int id, List<Vector2> corners, int fraction, int patchesJoined)
    {
        this.id = id;
        this.corners = corners;
        this.fraction = fraction;
        this.patchesJoined = patchesJoined;
    }

}
