using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandChangeColor : ICommand
{
    Vector3Int coord;
    Color oldColor;
    Color newColor;
    public CommandChangeColor(Vector3Int coord, Color newColor, Color oldColor)
    {
        this.coord = coord;
        this.newColor = newColor;
        this.oldColor = oldColor;
    }

    static string _name = "Change Color";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.SetVoxelColor(coord, newColor);
    }

    public void Undo()
    {
        VoxelManager.instance.SetVoxelColor(coord, oldColor);
    }
}