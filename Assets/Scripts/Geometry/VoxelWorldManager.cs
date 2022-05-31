using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Collections;
using Geometry;
using System.Linq;
using System.Threading;

public class VoxelWorldManager : MonoBehaviour
{
    public Dictionary<Vector3Int, VoxelChunk> voxelChunks = new Dictionary<Vector3Int, VoxelChunk>();
    public List<MapData> mapDatas = new List<MapData>();
    public int chunkSize = 50;
    public static VoxelWorldManager instance;
    public Transform voxelChunkParent;
    public VoxelChunk voxelChunkPrefab;
    public Vector3Int playerPos;
    Queue<VoxelChunk> renderChunks;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void Clear()
    {
        foreach (VoxelChunk chunk in voxelChunks.Values)
        {
            chunk.voxels.Clear();
            chunk.mesh.GenerateMesh(chunk.voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
        }
    }
    public void CreateWorldMesh()
    {   
        mapDatas.Clear();
        foreach (VoxelChunk chunk in voxelChunks.Values)
        {
            //var t = chunk.voxels.Values.SelectMany(x => x.GetFaces(chunk.coord.x, chunk.coord.y, chunk.coord.z));
            //mapDatas.Add(new MapData(t, chunk));
            chunk.mesh.GenerateMesh(chunk.voxels.Values.SelectMany(x => x.GetFaces(chunk.coord.x, chunk.coord.y, chunk.coord.z)).ToArray());
        }
        // foreach (var data in mapDatas)
        // {
            
        // }
    }
    
	// public void AddVoxels(List<Vector3Int> coords, List<float3> colors)
    // {
    //     Voxel voxel;
    //     for (int i = 0; i < coords.Count; i++)
    //     {
    //         VoxelChunk chunk = GetChunk(coords[i]);
    //         chunk.voxels.TryGetValue(coords[i], out voxel);

    //         if (voxel == null)
    //         {
    //             chunk.voxels.Add(coords[i], new Voxel(coords[i], colors[i], chunk.voxels));
    //         }
    //     }

    //     //CreateWorldMesh();
    // }

    public void CreateWorld(List<Vector3Int> coords, List<float3> colors)
    {
        Voxel voxel;
        for (int i = 0; i < coords.Count; i++)
        {
            VoxelChunk chunk = GetChunk(coords[i]);
            chunk.voxels.TryGetValue(coords[i], out voxel);

            if (voxel == null)
            {
                chunk.voxels.Add(coords[i], new Voxel(coords[i], colors[i], chunk.voxels, null));
            }
        }

        CreateWorldMesh();
    }

    // public void AddVoxel(Vector3Int coord, float3 color)
    // {
        
    // }

    public Voxel GetVoxel(Vector3Int coord)
    {
        Voxel voxel;
        GetChunk(coord).voxels.TryGetValue(coord, out voxel);
        return voxel;
    }

    // Voxel CreateVoxelByChunk(Vector3Int coord)
    // {
    //     return null;
    // }

    VoxelChunk GetChunk(Vector3Int coord)
    {
        VoxelChunk voxelChunk = null;
        Vector3Int chunkCoord = new Vector3Int(coord.x / chunkSize, coord.y / chunkSize, coord.z / chunkSize);
        voxelChunks.TryGetValue(chunkCoord, out voxelChunk);

        if (voxelChunk == null)
        {
            VoxelChunk chunk = Instantiate(voxelChunkPrefab, voxelChunkParent);
            chunk.Init(chunkCoord);
            chunk.name = string.Format("chunk_{0}", coord);
            voxelChunks.Add(chunkCoord, chunk);
            return chunk;  
        }

        return voxelChunk;
    }
}

public class MapData
{
    public IEnumerable<Face> faces;
    public VoxelChunk voxelChunk;
    public MapData(IEnumerable<Face> faces, VoxelChunk voxelChunk = null)
    {
        this.faces = faces;
        this.voxelChunk = voxelChunk;
    }
}
