using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

namespace Geometry {

    public class Voxel
    {
        public List<Face> faces;
        List<Voxel> _neighbours;
        public List<Voxel> Neighbours
        {
            get => _neighbours;
            set
            {
                _neighbours = value;
                SetFaces();
            }
        }
        public Vector3Int coord;
        public float3 color;

        public Voxel(List<Voxel> neighbours, Vector3Int coord, float3 color)
        {
            this.Neighbours = neighbours;
            this.coord = coord;
            this.color = color;
        }

        void SetFaces()
        {
            faces = new List<Face>();
            for (int i = 0; i < VoxelManager.AllDirections.Length; i++)
            {
                if (GetNeighbour(coord + VoxelManager.AllDirections[i].ToCoord()) is Voxel voxel)
                {
                    continue;
                }
                faces.Add(VoxelManager.AllDirections[i].CreateFace(new float3(coord.x, coord.y, coord.z), color));
            }
            
            // faces.Add(Direction.XPos.CreateFace(new float3(coord.x, coord.y, coord.z), color));
            // faces.Add(Direction.XNeg.CreateFace(new float3(coord.x, coord.y, coord.z), color));
            // faces.Add(Direction.YPos.CreateFace(new float3(coord.x, coord.y, coord.z), color));
            // faces.Add(Direction.YNeg.CreateFace(new float3(coord.x, coord.y, coord.z), color));
            // faces.Add(Direction.ZPos.CreateFace(new float3(coord.x, coord.y, coord.z), color));
            // faces.Add(Direction.ZNeg.CreateFace(new float3(coord.x, coord.y, coord.z), color));
        }

        (bool, Direction) NeighbourDirection(Voxel voxel)
        {
            for (int i = 0; i < VoxelManager.AllDirections.Length; i++)
            {
                if (VoxelManager.AllDirections[i].ToCoord() == coord - voxel.coord)
                {
                    return (true, VoxelManager.AllDirections[i]);
                }
                
            }
            // FIX
            return (false, Direction.XPos);
        }

        public Voxel GetNeighbour(Vector3Int coord)
        {
            return Neighbours.FirstOrDefault(x => x.coord == coord);
        }   
    }

}