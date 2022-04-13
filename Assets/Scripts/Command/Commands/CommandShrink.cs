using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandShrink : ICommand
{
    Vector3Int[] coords;
    Color color;
    public CommandShrink(Vector3Int[] coords, Color color)
    {
        this.coords = coords;
        this.color = color;
    }

    static string _name = "Shrink";
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