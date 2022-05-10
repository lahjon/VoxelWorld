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
        public Voxel(Vector3Int coord, float3 color, Dictionary<Vector3Int, Voxel> owner)
        {
            this.coord = coord;
            this.color = color;
            Voxel voxel;

            for (int i = 0; i < DirectionStruct.Normals.Length; i++)
            {
                if (owner.TryGetValue(coord + DirectionStruct.Normals[i], out voxel))
                {
                    neighbours[i] = voxel;
                    voxel.neighbours[(6 + i + 3) % 6] = this;
                }
            }
        }

        // public Voxel(VoxelData data, Dictionary<Vector3Int, Voxel> owner)
        // {
        //     this.coord = data.coord;
        //     this.color = data.color;
        //     Voxel voxel;
            
        //     for (int i = 0; i < DirectionStruct.Normals.Length; i++)
        //     {
        //         if (owner.TryGetValue(coord + DirectionStruct.Normals[i], out voxel))
        //         {
        //             neighbours[i] = voxel;
        //             voxel.neighbours[(6 + i + 3) % 6] = this;
        //         }
        //     }
        // }

        public List<Face> GetFaces()
        {
            List<Face> faces = new List<Face>();
            for (int i = 0; i < 6; i++)
            {
                if (neighbours[i] != null)
                {
                    continue;
                }
                faces.Add(new Face(i, new float3(coord.x, coord.y, coord.z), color));
            }
            return faces;
        }
    }

}