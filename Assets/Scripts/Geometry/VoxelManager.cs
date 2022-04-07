using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using System.Linq;
public class VoxelManager : MonoBehaviour
{
    //public List<Face> faces;
    public Dictionary<Vector3Int, Voxel> voxels = new Dictionary<Vector3Int, Voxel>();
    public float voxelSize;
    public int gridLevel;
    //public int Operation = 1;
    public ProceduralMesh proceduralMesh;
    public SelectionCube selectionCube;
    public Vector3Int coord;
    public Vector3Int coordOffset;
    public Vector3Int latestAddedCoord;
    bool isDrawing;
    Vector3Int currentCoord;
    public static Direction[] AllDirections = new Direction[6]{
                                                Direction.XPos,
                                                Direction.XNeg,
                                                Direction.YPos,
                                                Direction.YNeg,
                                                Direction.ZPos,
                                                Direction.ZNeg,
                                            };
    public float3 color;
    void Start()
    {

    }

    public void ButtonClear()
    {
        voxels.Clear();
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.faces).ToArray());
    }

    void CreateMesh()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 newPos = hit.point + hit.normal;

            int yOffset = hit.normal == Vector3.up ? -1 : 0;
            int zOffset = hit.normal == Vector3.forward ? -1 : 0;
            int xOffset = hit.normal == Vector3.right ? -1 : 0;
            
            Vector3Int tempCoord = new Vector3Int(Mathf.FloorToInt(newPos.x) + xOffset, Mathf.FloorToInt(newPos.y) + yOffset, Mathf.FloorToInt(newPos.z) + zOffset);
            AddVoxel(tempCoord);
        }
        else
        {
            AddVoxel(coord);
        }

        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.faces).ToArray());
    }

    void AddVoxel(Vector3Int coord)
    {
        if (!voxels.ContainsKey(coord))
        {
            latestAddedCoord = coord;
            voxels.Add(coord, new Voxel(GetAllNeighbours(coord), coord, color));
        }
    }
    void RemoveVoxel(Vector3Int coord)
    {
        voxels.Remove(coord);
    }

    void Update()
    {
        GetCoord();

        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
        }
        if (Input.GetMouseButton(0) && currentCoord != coord)
        {
            currentCoord = coord;
            CreateMesh();
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }
    }

    (bool success, Voxel neighbour) GetNeighbour(Vector3Int coord, Direction direction)
    {
        if (voxels.ContainsKey(coord + direction.ToCoord()) && voxels[coord + direction.ToCoord()] is Voxel voxel)
            return (true, voxel);
        else
            return (false, new Voxel());
    }   
    Voxel[] GetAllNeighbours(Vector3Int coord)
    {
        (bool, Voxel) neighbour;
        List<Voxel> neighbours = new List<Voxel>();

        for (int i = 0; i < AllDirections.Length; i++)
        {
            neighbour = GetNeighbour(coord, AllDirections[i]);
            if (neighbour.Item1)
                neighbours.Add(neighbour.Item2);
        }   

        return neighbours.ToArray();
    }

    

    void GetCoord()
    {
        Plane plane = new Plane(Vector3.up, gridLevel);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 newPos = hit.point + hit.normal;

            int yOffset = hit.normal == Vector3.up ? -1 : 0;
            int zOffset = hit.normal == Vector3.forward ? -1 : 0;
            int xOffset = hit.normal == Vector3.right ? -1 : 0;
            
            Vector3Int tempCoord = new Vector3Int(Mathf.FloorToInt(newPos.x) + xOffset, Mathf.FloorToInt(newPos.y) + yOffset, Mathf.FloorToInt(newPos.z) + zOffset);

            Vector3Int selectedCoord = new Vector3Int(Mathf.FloorToInt(hit.point.x) + xOffset, Mathf.FloorToInt(hit.point.y) + yOffset, Mathf.FloorToInt(hit.point.z) + zOffset);

            Debug.Log(selectedCoord);
            Debug.Log(selectedCoord);

            coord = tempCoord;
            if (selectionCube.coord != coord)
            {
                selectionCube.MoveToCoord(coord, voxels.ContainsKey(coord));
            }
            
        }
        else if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);
            coord = new Vector3Int(Mathf.FloorToInt(pos.x), gridLevel, Mathf.FloorToInt(pos.z));
            if (selectionCube.coord != coord)
            {
                selectionCube.MoveToCoord(coord, voxels.ContainsKey(coord));
            }
        }
    }
}
