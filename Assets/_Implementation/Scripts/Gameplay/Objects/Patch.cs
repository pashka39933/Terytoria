using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Patch {

    public long id;
    public List<Vector3> flags;
    public List<bool> borders;
    public int fraction;
    public string nick;

    public Patch(long id, List<Vector3> flags, List<bool> borders, int fraction, string nick)
    {
        this.id = id;
        this.flags = flags;
        this.borders = borders;
        this.fraction = fraction;
        this.nick = nick;
    }

}
