using Unity.Mathematics;
using UnityEngine;

public struct BoundsInt 
{
    public BoundsInt(Vector3Int min, Vector3Int max)
    {
        this.min = min;
        this.max = max;
    }
    public Vector3Int min;
    public Vector3Int max;
}
