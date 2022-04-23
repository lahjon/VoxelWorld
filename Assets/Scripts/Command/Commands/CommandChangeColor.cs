using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandChangeColor : ICommand
{
    Vector3Int[] coords;
    float3[] oldColors;
    Color newColor;
    public CommandChangeColor(Vector3Int[] coords, Color newColor, float3[] oldColors)
    {
        this.coords = coords;
        this.newColor = newColor;
        this.oldColors = oldColors;
    }

    static string _name = "Change Color";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.SetVoxelsColor(coords, newColor);
    }

    public void Undo()
    {

        VoxelManager.instance.SetVoxelColors(coords, oldColors);
        
    }
}