using UnityEngine;

[System.Serializable]
public class Well
{

    public long id;
    public Vector3 position;
    public string nick;
    public int level;

    public Well(long id, Vector3 position, string nick, int level)
    {
        this.id = id;
        this.position = position;
        this.nick = nick;
        this.level = level;
    }

}
