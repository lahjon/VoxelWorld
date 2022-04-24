using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandAdd : ICommand
{
    Vector3Int[] coords;
    Color color;
    public CommandAdd(Vector3Int[] coords, Color color)
    {
        this.coords = coords;
        this.color = color;
    }

    static string _name = "Add";
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