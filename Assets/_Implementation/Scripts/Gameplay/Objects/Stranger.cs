using UnityEngine;

[System.Serializable]
public class Stranger
{

    public string id;
    public Vector3 position;
    public string nick;
    public int fraction;
    public int updateTimestamp;

    public Stranger(string id, Vector3 position, string nick, int fraction, int updateTimestamp)
    {
        this.id = id;
        this.position = position;
        this.nick = nick;
        this.fraction = fraction;
        this.updateTimestamp = updateTimestamp;
    }

}