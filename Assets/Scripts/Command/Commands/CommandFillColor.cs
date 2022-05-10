using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandFillColor : ICommand
{
    List<Vector3Int> coords;
    float3 oldColor;
    float3 newColor;
    public CommandFillColor(List<Vector3Int> coords, float3 newColor, float3 oldColor)
    {
        this.coords = coords;
        this.newColor = newColor;
        this.oldColor = oldColor;
    }

    static string _name = "Fill Color";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.SetVoxelsColor(coords, newColor);
    }

    public void Undo()
    {
        VoxelManager.instance.SetVoxelsColor(coords, oldColor);
    }
}