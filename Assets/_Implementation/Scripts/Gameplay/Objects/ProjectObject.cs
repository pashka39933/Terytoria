using UnityEngine;

[System.Serializable]
public class ProjectObject
{

    public long id;
    public Vector3 position;
    public string nick;
    public int fraction;

    public ProjectObject(long id, Vector3 position, string nick, int fraction)
    {
        this.id = id;
        this.position = position;
        this.nick = nick;
        this.fraction = fraction;
    }

}