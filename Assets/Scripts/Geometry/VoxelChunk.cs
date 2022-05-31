using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Collections;


namespace Geometry 
{
	public class VoxelChunk : MonoBehaviour
	{
		public Dictionary<Vector3Int, Voxel> voxels = new Dictionary<Vector3Int, Voxel>();
		public List<VoxelChunk> neighbours = new List<VoxelChunk>();
		public Vector3Int coord;
		public ProceduralMesh mesh;
		public void Init(Vector3Int aCoord)
		{
			coord = aCoord;
			Debug.Log(coord);
		}
	}

}
