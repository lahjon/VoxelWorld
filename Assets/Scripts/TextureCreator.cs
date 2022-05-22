using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TextureCreator : MonoBehaviour {

	[Range(2, 512)]
	public int resolution = 256;

	public float frequency = 1f;

	[Range(1, 8)]
	public int octaves = 1;

	[Range(1f, 4f)]
	public float lacunarity = 2f;

	[Range(0f, 1f)]
	public float persistence = 0.5f;

	[Range(1, 3)]
	public int dimensions = 3;

	public NoiseMethodType type;

	public Gradient coloring;

	private Texture2D texture;
	
	private void Start () {
		if (texture == null) {
			texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
			texture.name = "Procedural Texture";
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.filterMode = FilterMode.Trilinear;
			texture.anisoLevel = 9;
			GetComponent<MeshRenderer>().material.mainTexture = texture;
		}
		FillTexture();
	}

	private void Update () {
		if (transform.hasChanged) {
			transform.hasChanged = false;
			Debug.Log("Clear");
			VoxelManager.instance.ButtonClear();
			FillTexture();
		}
	}
	
	public void FillTexture () 
	{
		if (texture.width != resolution) {
			texture.Resize(resolution, resolution);
		}
		
		Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f,-0.5f));
		Vector3 point10 = transform.TransformPoint(new Vector3( 0.5f,-0.5f));
		Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
		Vector3 point11 = transform.TransformPoint(new Vector3( 0.5f, 0.5f));
		List<Vector3Int> coords = new List<Vector3Int>();
		List<float3> colors = new List<float3>();

		NoiseMethod method = Noise.methods[(int)type][dimensions - 1];
		float stepSize = 1f / resolution;
		for (int y = 0; y < resolution; y++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
			for (int x = 0; x < resolution; x++) {
				Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
				float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
				if (type != NoiseMethodType.Value) {
					sample = sample * 0.5f + 0.5f;
				}
				var z = coloring.Evaluate(sample);
				int max = 20;
				var zb = (int)sample.Remap(0, 1, 0, max);
				texture.SetPixel(x, y, z);
				for (int i = 0; i < max - zb; i++)
				{
					coords.Add(new Vector3Int(x, i, y));
					colors.Add(new float3(z.r, z.g, z.b));
				}
			}
		}
		VoxelManager.instance.TryAddVoxels(coords, colors, true);
		texture.Apply();
	}
}