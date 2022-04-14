using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Formatters;

[MessagePackObject]
public class SaveData
{   
    [Key(0)]
    public VoxelData[] VoxelDatas{get;set;}
    public void Save()
    {
        // Do this once and store it for reuse.
    var resolver = MessagePack.Resolvers.CompositeResolver.Create(
    MessagePack.Unity.Extension.UnityBlitResolver.Instance,
    MessagePack.Unity.UnityResolver.Instance,

    // finally use standard resolver
    StandardResolver.Instance
);
var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

        byte[] bytes = MessagePackSerializer.Serialize(this);
        FileManager.WriteToFile("SaveData01.dat", bytes);
    }
}

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