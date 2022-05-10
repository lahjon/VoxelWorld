using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandRemoveLine : ICommand
{
    List<Vector3Int> coords;
    float3 color;
    public CommandRemoveLine(List<Vector3Int> coords, float3 color)
    {
        this.coords = coords;
        this.color = color;
    }

    static string _name = "Remove Line";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.RemoveVoxels(coords);
    }

    public void Undo()
    {
        VoxelManager.instance.AddVoxels(coords, color);
    }
}