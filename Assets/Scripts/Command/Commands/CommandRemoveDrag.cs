using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandRemoveDrag : ICommand
{
    List<Vector3Int> coords;
    List<float3> colors;
    public CommandRemoveDrag(List<Vector3Int> coords, List<float3> colors)
    {
        this.coords = coords;
        this.colors = colors;
    }

    static string _name = "Remove Drag";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.RemoveVoxels(coords);
    }

    public void Undo()
    {
        VoxelManager.instance.AddVoxels(coords, colors);
    }
}