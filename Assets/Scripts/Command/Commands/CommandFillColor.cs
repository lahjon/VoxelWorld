using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandFillColor : ICommand
{
    Vector3Int[] coords;
    Color oldColor;
    Color newColor;
    public CommandFillColor(Vector3Int[] coords, Color newColor, Color oldColor)
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