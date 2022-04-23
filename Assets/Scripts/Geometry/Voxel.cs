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

            for (int i = 0; i < VoxelManager.instance.voxels.Count; i++)
            {
                if (true)
                {
                    
                }
            }
        }

        public Voxel(VoxelData data)
        {
            this.coord = data.coord;
            this.color = data.color;
        }

        public List<Face> GetFaces()
        {
            List<Face> faces = new List<Face>();
            for (int i = 0; i < DirectionStruct.Directions.Length; i++)
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