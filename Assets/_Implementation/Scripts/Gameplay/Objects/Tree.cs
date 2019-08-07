using UnityEngine;

[System.Serializable]
public class Tree
{

    public long id;
    public Vector3 position;
    public string nick;
    public int creationTimestamp;
    public int ownerFruitCollectionTimestamp;
    public int commonFruitCollectionTimestamp;

    public Tree(long id, Vector3 position, string nick, int creationTimestamp, int ownerFruitCollectionTimestamp, int commonFruitCollectionTimestamp)
    {
        this.id = id;
        this.position = position;
        this.nick = nick;
        this.creationTimestamp = creationTimestamp;
        this.ownerFruitCollectionTimestamp = ownerFruitCollectionTimestamp;
        this.commonFruitCollectionTimestamp = commonFruitCollectionTimestamp;
    }

}
