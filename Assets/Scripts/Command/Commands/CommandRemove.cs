using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandRemove : ICommand
{
    Vector3Int placementCoord;
    Color color;
    public CommandRemove(Vector3Int placementCoord, Color color)
    {
        this.placementCoord = placementCoord;
        this.color = color;
    }

    static string _name = "Remove";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.RemoveSingleVoxel(placementCoord);
    }

    public void Undo()
    {
        VoxelManager.instance.AddSingleVoxel(placementCoord, color);
    }
}