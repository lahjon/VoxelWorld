using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandChangeColor : ICommand
{
    List<Vector3Int> coords;
    List<float3> oldColors;
    float3 newColor;
    public CommandChangeColor(List<Vector3Int> coords, float3 newColor, List<float3> oldColors)
    {
        this.coords = coords;
        this.newColor = newColor;
        this.oldColors = oldColors;
    }

    static string _name = "Change Color";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.SetVoxelsColor(coords, newColor, true);
    }

    public void Undo()
    {

        VoxelManager.instance.SetVoxelColors(coords, oldColors);
        
    }
}