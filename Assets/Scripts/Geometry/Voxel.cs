using Unity.Mathematics;
using UnityEngine;

namespace Geometry {

    public struct Voxel
    {
        public Face[] faces;
        public Voxel[] neighbours;
        public Vector3Int coord;
        public float3 color;

        public Voxel(Voxel[] neighbours, Vector3Int coord, float3 color)
        {
            this.neighbours = neighbours;
            this.coord = coord;
            this.color = color;
            faces = new Face[6];
            faces[0] = Direction.XPos.CreateFace(new float3(coord.x, coord.y, coord.z), color);
            faces[1] = Direction.XNeg.CreateFace(new float3(coord.x, coord.y, coord.z), color);
            faces[2] = Direction.YPos.CreateFace(new float3(coord.x, coord.y, coord.z), color);
            faces[3] = Direction.YNeg.CreateFace(new float3(coord.x, coord.y, coord.z), color);
            faces[4] = Direction.ZPos.CreateFace(new float3(coord.x, coord.y, coord.z), color);
            faces[5] = Direction.ZNeg.CreateFace(new float3(coord.x, coord.y, coord.z), color);
        }
    }

}