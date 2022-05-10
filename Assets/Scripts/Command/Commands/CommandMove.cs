using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class CommandMove : ICommand
{
    List<Vector3Int> coords;
    Vector3Int offset;    
    public CommandMove(List<Vector3Int> coords, Vector3Int offset)
    {
        this.coords = coords;
        this.offset = offset;    
    }

    static string _name = "Move";
    public string Name { get =>  _name; }

    public void Execute()
    {
        VoxelManager.instance.MoveVoxels(coords, offset);
    }

    public void Undo()
    {
        VoxelManager.instance.MoveVoxels(coords, -offset);
    }
}