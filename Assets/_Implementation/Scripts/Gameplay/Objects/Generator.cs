using UnityEngine;

[System.Serializable]
public class Generator
{

    public long id;
    public Vector3 position;
    public string nick;
    public int level;
    public int batteryLevel;
    public int converterLevel;
    public int converterFromResource;
    public int converterToResource;
    public int converterChangesCount;

    public Generator(long id, Vector3 position, string nick, int level, int batteryLevel, int converterLevel, int converterFromResource, int converterToResource, int converterChangesCount)
    {
        this.id = id;
        this.position = position;
        this.nick = nick;
        this.level = level;
        this.batteryLevel = batteryLevel;
        this.converterLevel = converterLevel;
        this.converterFromResource = converterFromResource;
        this.converterToResource = converterToResource;
        this.converterChangesCount = converterChangesCount;
    }

}
