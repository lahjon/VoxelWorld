using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry 
{
    public struct Face
    {
        public float3 x, y, z, w;
        public int direction;
		public float3 color;
        public Face(int direction, float3 position, float3 color)
        {
            this.x = (FaceDirections[direction][0] + position) * VoxelManager.instance.VoxelSize;
            this.y = (FaceDirections[direction][1] + position) * VoxelManager.instance.VoxelSize;
            this.w = (FaceDirections[direction][2] + position) * VoxelManager.instance.VoxelSize;
            this.z = (FaceDirections[direction][3] + position) * VoxelManager.instance.VoxelSize;
            this.direction = direction;
			this.color = color;
        }
		public static float3[][] FaceDirections = new float3[6][]
		{
			new float3[4]
			{
				new float3(1f, 1f, 1f),
				new float3(1f, 0f, 1f),
				new float3(1f, 1f, 0f),
				new float3(1f, 0f, 0f),
			},
			new float3[4]
			{
				new float3(1f, 1f, 0f),
				new float3(0f, 1f, 0f),
				new float3(1f, 1f, 1f),
				new float3(0f, 1f, 1f),
			},
			new float3[4]
			{
				new float3(0f, 1f, 1f),
				new float3(0f, 0f, 1f),
				new float3(1f, 1f, 1f),
				new float3(1f, 0f, 1f),
			},
			new float3[4]
			{
				new float3(0f, 1f, 0f),
				new float3(0f, 0f, 0f),
				new float3(0f, 1f, 1f),
				new float3(0f, 0f, 1f),
			},
			new float3[4]
			{
				new float3(0f, 0f, 1f),
				new float3(0f, 0f, 0f),
				new float3(1f, 0f, 1f),
				new float3(1f, 0f, 0f),
			},
			new float3[4]
			{
				new float3(1f, 0f, 0f),
				new float3(0f, 0f, 0f),
				new float3(1f, 1f, 0f),
				new float3(0f, 1f, 0f),
			},

		};
    }
}
public struct DirectionStruct
{
	public static readonly Vector3Int[] Normals = new Vector3Int[6]
	{
		Vector3Int.right,
		Vector3Int.up,
		Vector3Int.forward,
		Vector3Int.left,
		Vector3Int.down,
		Vector3Int.back
	};

	public static readonly Dictionary<Vector3Int, int> INormals = new Dictionary<Vector3Int, int>
	{
		{Vector3Int.right, 0},
		{Vector3Int.up, 1},
		{Vector3Int.forward, 2},
		{Vector3Int.left, 3},
		{Vector3Int.down, 4},
		{Vector3Int.back, 5}
	};


	// public static readonly HashSet<Vector3Int> hNormals = new HashSet<Vector3Int>
	// {
	// 	Vector3Int.right,
	// 	Vector3Int.up,
	// 	Vector3Int.forward,
	// 	Vector3Int.left,
	// 	Vector3Int.down,
	// 	Vector3Int.back
	// };
	// public static readonly Direction[] Directions = new Direction[6]
	// {
	// 	Direction.XPos,
	// 	Direction.YPos,
	// 	Direction.ZPos,
	// 	Direction.XNeg,
	// 	Direction.YNeg,
	// 	Direction.ZNeg,
	// };
	// public static Direction NormalToDirection(Vector3Int normal)
	// {
	// 	for (int i = 0; i < Normals.Length; i++)
	// 	{
	// 		if (Normals[i] == normal)
	// 		{
	// 			return Directions[i];
	// 		}s
	// 	}
	// 	return Direction.XPos;
	// }
	public static Vector3Int[] AvailableDirections(Vector3Int direction)
	{
		Vector3Int[] dirs = new Vector3Int[4];
		if (Normals[0] == direction || Normals[3] == direction)
		{
			dirs[1] = Normals[1];
			dirs[2] = Normals[2];
			dirs[0] = Normals[4];
			dirs[3] = Normals[5];
		}
		else if (Normals[1] == direction || Normals[4] == direction)
		{
			dirs[0] = Normals[0];
			dirs[1] = Normals[2];
			dirs[2] = Normals[3];
			dirs[3] = Normals[5];
		}
		else
		{
			dirs[0] = Normals[0];
			dirs[1] = Normals[1];
			dirs[2] = Normals[3];
			dirs[3] = Normals[4];
		}
		return dirs;
	}
}

// public static class DirectionExtensions 
// {

//     public static float4 ToColor(this Direction direction) {
// 		switch (direction)
// 		{
// 			case Direction.XPos:
//                 // red !
// 				return new float4(1f,0f,0f, 1f);
// 			case Direction.XNeg:
//                 // green !
// 				return new float4(0f,1f,0f, 1f);
// 			case Direction.YPos:
//                 // blue !
// 				return new float4(0f,0f,1f, 1f);
// 			case Direction.YNeg:
//                 // cyan ?
// 				return new float4(1f,1f,0f, 1f);
// 			case Direction.ZPos:
//                 // puple ?
// 				return new float4(1f,0f,1f, 1f);
// 			case Direction.ZNeg:
//                 // yellow ?
// 				return new float4(0f,1f,1f, 1f);
// 			default:
// 				return new float4(0,0,0,0);
// 		}
// 	}
// }