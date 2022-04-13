using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandAdd : ICommand
{
    Vector3Int placementCoord;
    Color color;
    public CommandAdd(Vector3Int placementCoord, Color color)
    {
        this.placementCoord = placementCoord;
        this.color = color;
    }

    static string _name = "Add";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.AddVoxel(placementCoord, color);
    }

    public void Undo()
    {
        VoxelManager.instance.RemoveVoxel(placementCoord);
    }
}