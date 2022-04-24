using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

namespace Geometry {

    public class Voxel
    {
        public Voxel[] neighbours = new Voxel[6];
        public Vector3Int coord;
        public float3 color;
        public Voxel(Vector3Int coord, float3 color)
        {
            this.coord = coord;
            this.color = color;
            Voxel voxel;
            //Voxel newNeighbour;

            Debug.Log("My Coord: " + coord);
            for (int i = 0; i < DirectionStruct.Normals.Length; i++)
            {
                if (VoxelManager.instance.voxels.TryGetValue(coord + DirectionStruct.Normals[i], out voxel))
                {
                    Debug.Log("Found Neighbour: " + voxel.coord);
                    neighbours[i] = voxel;
                    voxel.neighbours[(6 + i + 3) % 6] = this;
                    //newNeighbour = GetNeighbour(DirectionStruct.Directions[i].Invert().ToCoord()); 
                }
            }
            // for (int i = 0; i < 6; i++)
            // {
            //     if (neighbours[i] != null)
            //     {
            //         for (int v = 0; v < 6; v++)
            //         {
            //             if (neighbours[i].neighbours[v] != null)
            //             {
            //                 Debug.Log("N Neighbours: " + neighbours[i].neighbours[v].coord); 
            //             }
            //         }
            //         Debug.Log("Neighbour: " + neighbours[i].coord); 
            //     }
            // }
        }

        public Voxel(VoxelData data)
        {
            this.coord = data.coord;
            this.color = data.color;
        }

        public List<Face> GetFaces()
        {
            List<Face> faces = new List<Face>();
            for (int i = 0; i < 6; i++)
            {
                if (neighbours[i] != null)
                {
                    continue;
                }
                faces.Add(DirectionStruct.Directions[i].CreateFace(new float3(coord.x, coord.y, coord.z), color));
            }
            return faces;
        }
        public Voxel GetNeighbour(Direction direction)
        {
            Voxel voxel;
            VoxelManager.instance.voxels.TryGetValue(coord + DirectionStruct.Normals[(int)direction], out voxel);
            return voxel;
        }
        public Voxel GetNeighbour(Vector3Int direction)
        {
            Voxel voxel;
            VoxelManager.instance.voxels.TryGetValue(coord + direction, out voxel);
            return voxel;
        }
    }

}