using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandGrow : ICommand
{
    Vector3Int[] coords;
    Color color;
    public CommandGrow(Vector3Int[] coords, Color color)
    {
        this.coords = coords;
        this.color = color;
    }

    static string _name = "Grow";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.AddVoxels(coords);
    }

    public void Undo()
    {
        VoxelManager.instance.RemoveVoxels(coords);
    }
}