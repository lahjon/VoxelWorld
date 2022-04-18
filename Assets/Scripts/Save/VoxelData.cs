using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
public class SaveData
{   
    public VoxelData[] voxelDatas;
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
    public void LoadFromJson(string a_Json)
    {
        JsonUtility.FromJsonOverwrite(a_Json, this);
    }
}

// public class SaveDataVoxel : SaveData
// {
//     public VoxelData[] voxelDatas;
// }

[System.Serializable]
public struct VoxelData
{
    public VoxelData(Vector3Int coord, float3 color)
    {
        this.coord = coord;
        this.color = color;
    }
    public Vector3Int coord; 
    public float3 color;
}