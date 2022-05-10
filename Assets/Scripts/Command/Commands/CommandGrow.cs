using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandGrow : ICommand
{
    List<Vector3Int> coords;
    float3 color;
    public CommandGrow(List<Vector3Int> coords, float3 color)
    {
        this.coords = coords;
        this.color = color;
    }

    static string _name = "Grow";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.AddVoxels(coords, color);
    }

    public void Undo()
    {
        VoxelManager.instance.RemoveVoxels(coords);
    }
}