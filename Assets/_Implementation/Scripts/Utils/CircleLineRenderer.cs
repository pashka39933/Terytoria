using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class CircleLineRenderer : MonoBehaviour
{

    public void CreatePoints(int segments, float radius, float yPos)
    {
        LineRenderer line = this.GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = segments + 1;

        float x;
        float z;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            line.SetPosition(i, new Vector3(x, yPos, z));
            angle += (360f / segments);
        }
    }

}