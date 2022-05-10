using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandDrawLine : ICommand
{
    List<Vector3Int> coords;
    float3 color;
    public CommandDrawLine(List<Vector3Int> coords, float3 color)
    {
        this.coords = coords;
        this.color = color;
    }

    static string _name = "Draw Line";
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