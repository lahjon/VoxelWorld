using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry 
{
    public struct Face
    {
        public float3 x, y, z, w;
        public Direction direction;
		public float3 color;
        public Face(float3 x, float3 y, float3 z, float3 w, Direction direction, float3 color)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.z = z;
            this.direction = direction;
			this.color = color;
        }
    }
}

public enum Direction
{
	XPos,
	XNeg,
	YPos,
	YNeg,
	ZPos,
	ZNeg
}

public enum GridAxis
{
	XZ,
	YZ,
	YX
}

public struct DirectionStruct
{
	public static readonly Vector3Int[] Normals = new Vector3Int[6]
	{
		Vector3Int.right,
		Vector3Int.left,
		Vector3Int.up,
		Vector3Int.down,
		Vector3Int.forward,
		Vector3Int.back
	};
	public static readonly Direction[] Directions = new Direction[6]
	{
		Direction.XPos,
		Direction.XNeg,
		Direction.YPos,
		Direction.YNeg,
		Direction.ZPos,
		Direction.ZNeg,
	};
	public static readonly Direction[] InvertDirections = new Direction[6]
	{
		Direction.XPos,
		Direction.YPos,
		Direction.ZPos,
		Direction.XNeg,
		Direction.YNeg,
		Direction.ZNeg,
	};
	public static Direction NormalToDirection(Vector3Int normal)
	{
		for (int i = 0; i < Normals.Length; i++)
		{
			if (Normals[i] == normal)
			{
				return Directions[i];
			}
		}
		return Direction.XPos;
	}

	public static Direction[] AvailableDirections(Direction direction)
	{
		Direction[] dirs = new Direction[4];
		if (Direction.XNeg == direction || Direction.XPos == direction)
		{
			dirs[0] = Direction.YNeg;
			dirs[1] = Direction.YPos;
			dirs[2] = Direction.ZNeg;
			dirs[3] = Direction.ZPos;
		}
		else if (Direction.YNeg == direction || Direction.YPos == direction)
		{
			dirs[0] = Direction.XPos;
			dirs[1] = Direction.XNeg;
			dirs[2] = Direction.ZNeg;
			dirs[3] = Direction.ZPos;
		}
		else if (Direction.ZNeg == direction || Direction.ZPos == direction)
		{
			dirs[0] = Direction.XPos;
			dirs[1] = Direction.XNeg;
			dirs[2] = Direction.YNeg;
			dirs[3] = Direction.YPos;
		}
		return dirs;
	}
}

public static class DirectionExtensions {

	public static float3 FloatToNormal(this Direction direction) {
		switch (direction)
		{
			case Direction.XPos:
				return new float3(1,0,0);
			case Direction.XNeg:
				return new float3(-1,0,0);
			case Direction.YPos:
				return new float3(0,1,0);
			case Direction.YNeg:
				return new float3(0,-1,0);
			case Direction.ZPos:
				return new float3(0,0,-1);
			case Direction.ZNeg:
				return new float3(0,0,1);
			default:
				return new float3(0,0,0);
		}
	}

	public static Direction Invert(this Direction direction)
	{
		switch (direction)
		{
			case Direction.XPos:
				return Direction.XNeg;
			case Direction.XNeg:
				return Direction.XPos;
			case Direction.YPos:
				return Direction.YNeg;
			case Direction.YNeg:
				return Direction.YPos;
			case Direction.ZPos:
				return Direction.ZNeg;
			case Direction.ZNeg:
				return Direction.ZPos;
			default:
				return Direction.XNeg;
		}
	}


	public static Vector3Int ToCoord(this Direction direction) {
		switch (direction)
		{
			case Direction.XPos:
				return new Vector3Int(1,0,0);
			case Direction.XNeg:
				return new Vector3Int(-1,0,0);
			case Direction.YPos:
				return new Vector3Int(0,1,0);
			case Direction.YNeg:
				return new Vector3Int(0,-1,0);
			case Direction.ZPos:
				return new Vector3Int(0,0,1);
			case Direction.ZNeg:
				return new Vector3Int(0,0,-1);
			default:
				return new Vector3Int(0,0,0);
		}
	}

    public static float4 ToTangent(this Direction direction) {
		switch (direction)
		{
			case Direction.XPos:
                // red !
				return new float4(0f,0f,1f, 1f);
			case Direction.XNeg:
                // green !
				return new float4(0f,0f,-1f, -1f);
			case Direction.YPos:
                // blue !
				return new float4(-1f,0f,0f, 1f);
			case Direction.YNeg:
                // cyan ?
				return new float4(1f,0f,0f, -1f);
			case Direction.ZPos:
                // puple ?
				return new float4(0f,1f,0f, -1f);
			case Direction.ZNeg:
                // yellow ?
				return new float4(0f,-1f,0f, 1f);
			default:
				return new float4(0f,0f,0f,0f);
		}
	}

    public static float4 ToColor(this Direction direction) {
		switch (direction)
		{
			case Direction.XPos:
                // red !
				return new float4(1f,0f,0f, 1f);
			case Direction.XNeg:
                // green !
				return new float4(0f,1f,0f, 1f);
			case Direction.YPos:
                // blue !
				return new float4(0f,0f,1f, 1f);
			case Direction.YNeg:
                // cyan ?
				return new float4(1f,1f,0f, 1f);
			case Direction.ZPos:
                // puple ?
				return new float4(1f,0f,1f, 1f);
			case Direction.ZNeg:
                // yellow ?
				return new float4(0f,1f,1f, 1f);
			default:
				return new float4(0,0,0,0);
		}
	}

    public static Face CreateFace(this Direction direction, float3 position, float3 color) {
		switch (direction)
		{
			case Direction.XPos:
                return new Face(
                    new float3(1f, 0f, 0f) + position, 
                    new float3(1f, 1f, 0f) + position,
                    new float3(1f, 1f, 1f) + position,
                    new float3(1f, 0f, 1f) + position,
                    direction,
					color
                );
			case Direction.XNeg:
				return new Face(
                    new float3(0f, 0f, 0f) + position, 
                    new float3(0f, 0f, 1f) + position,
                    new float3(0f, 1f, 1f) + position,
                    new float3(0f, 1f, 0f) + position,
                    direction,
					color
                );
			case Direction.YPos:
				return new Face(
                    new float3(0f, 1f, 0f) + position, 
                    new float3(0f, 1f, 1f) + position,
                    new float3(1f, 1f, 1f) + position,
                    new float3(1f, 1f, 0f) + position,
                    direction,
					color
                );
			case Direction.YNeg:
				return new Face(
                    new float3(0f, 0f, 0f) + position, 
                    new float3(1f, 0f, 0f) + position,
                    new float3(1f, 0f, 1f) + position,
                    new float3(0f, 0f, 1f) + position,
                    direction,
					color
                );
			case Direction.ZPos:
				return new Face(
                    new float3(0f, 0f, 0f) + position, 
                    new float3(0f, 1f, 0f) + position,
                    new float3(1f, 1f, 0f) + position,
                    new float3(1f, 0f, 0f) + position,
                    direction,
					color
                );
			case Direction.ZNeg:
				return new Face(
                    new float3(0f, 0f, 1f) + position, 
                    new float3(1f, 0f, 1f) + position,
                    new float3(1f, 1f, 1f) + position,
                    new float3(0f, 1f, 1f) + position,
                    direction,
					color
                );
			default:
				return new Face();
		}
	}
}