using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Geometry;
using System;


public class TerrainGenerator : MonoBehaviour
{
    //Queue<>

    public void RequestMapData(Action<VoxelChunk> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<VoxelChunk> callback)
    {
        //VoxelChunk voxel
    }
}