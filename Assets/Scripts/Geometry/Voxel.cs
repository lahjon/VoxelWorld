using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

namespace Geometry {

    public class Voxel
    {
        public List<Voxel> Neighbours;
        public Vector3Int coord;
        public float3 color;
        public Voxel(List<Voxel> neighbours, Vector3Int coord, float3 color)
        {
            this.Neighbours = neighbours;
            this.coord = coord;
            this.color = color;
        }

        public Voxel(VoxelData data)
        {
            this.Neighbours = new List<Voxel>();
            this.coord = data.coord;
            this.color = data.color;
        }

        public List<Face> GetFaces()
        {
            List<Face> faces = new List<Face>();
            for (int i = 0; i < DirectionStruct.Directions.Length; i++)
            {
                if (GetNeighbour(coord + DirectionStruct.Directions[i].ToCoord()) is Voxel voxel)
                {
                    continue;
                }
                faces.Add(DirectionStruct.Directions[i].CreateFace(new float3(coord.x, coord.y, coord.z), color));
            }
            return faces;
        }
        public Voxel GetNeighbour(Vector3Int coord)
        {
            return Neighbours.FirstOrDefault(x => x.coord == coord);
        }
    }

}