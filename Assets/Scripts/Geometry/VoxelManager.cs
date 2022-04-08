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
    public Vector3Int placementCoord;
    Vector3Int? latestAddedCoord = null;
    Vector3Int latestSelectedCoord;
    bool isDrawing;
    Vector3Int selectedCoord;
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
        //latestAddedCoord = null;
    }

    public void ButtonClear()
    {
        voxels.Clear();
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.faces).ToArray());
    }

    void CreateMesh()
    {
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.faces).ToArray());
    }

    void AddVoxel(Vector3Int coord)
    {
        if (latestAddedCoord == selectedCoord)
            return;
        if (!voxels.ContainsKey(coord))
        {
            latestAddedCoord = coord;
            latestSelectedCoord = coord;
            voxels.Add(coord, new Voxel(GetAllNeighbours(coord), coord, color));
            CreateMesh();
        }
    }

    void AddVoxels(Vector3Int[] addVoxels)
    {
        for (int i = 0; i < addVoxels.Length; i++)
        {
            if (!voxels.ContainsKey(addVoxels[i]))
            {
                voxels.Add(addVoxels[i], new Voxel(GetAllNeighbours(addVoxels[i]), addVoxels[i], color));
            }
        }
        if (addVoxels.Length > 0)
        {
            latestSelectedCoord = addVoxels[addVoxels.Length - 1];
        }
        CreateMesh();
    }

    void RemoveVoxel(Vector3Int coord)
    {
        if(voxels.Remove(coord))
            CreateMesh();
    }

    Vector3Int[] DrawLine(Vector3Int c1, Vector3Int c2)
    {
        int n = DiagonalDistance(c1, c2);
        Vector3Int[] voxels = new Vector3Int[n + 1];
        for (int i = 0; i <= n; i++)
        {
            float t = n == 0 ? 0f : (float)i / n;
            voxels[i] = (RoundCoord(Vector3.Lerp(c1, c2, t)));
        }
        return voxels;
    }
    int DiagonalDistance(Vector3Int c1, Vector3Int c2) => Mathf.Max(Mathf.Abs(c2.x - c1.x), Mathf.Abs(c2.y - c1.y), Mathf.Abs(c2.z - c1.z));
    Vector3Int RoundCoord(Vector3 coord)
    {
        return new Vector3Int(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y), Mathf.RoundToInt(coord.z));
    }

    void Update()
    {
        GetCoord();

        if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftShift))
        {
            AddVoxel(placementCoord);
        }
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
        {
            if (latestSelectedCoord != null)
            {
                AddVoxels(DrawLine(latestSelectedCoord, placementCoord));
            }
        }
        if (Input.GetMouseButtonDown(2))
        {
            RemoveVoxel(selectedCoord);
        }
        if (Input.GetMouseButtonUp(0))
        {
            latestAddedCoord = null;
        }
        Debug.Log(selectedCoord);
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
            Vector3Int normal =  Vector3Int.FloorToInt(hit.normal);
            Vector3Int offsetCoord = new Vector3Int(hit.normal == Vector3.right ? 1 : 0, hit.normal == Vector3.up ? 1 : 0, hit.normal == Vector3.forward ? 1 : 0);
            Vector3Int selCoord = Vector3Int.FloorToInt(hit.point) - offsetCoord;

            if (hit.normal == Vector3.up || hit.normal == Vector3.down)
            {
                selCoord.y = Mathf.RoundToInt(hit.point.y) - offsetCoord.y;
            }
            else if (hit.normal == Vector3.left || hit.normal == Vector3.right)
            {
                selCoord.x = Mathf.RoundToInt(hit.point.x) - offsetCoord.x;
            }
            else
            {
                selCoord.z = Mathf.RoundToInt(hit.point.z) - offsetCoord.z;
            }
            
            placementCoord = selCoord + normal;
            selectedCoord = selCoord;
        }
        else if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);
            placementCoord = new Vector3Int(Mathf.FloorToInt(pos.x), gridLevel, Mathf.FloorToInt(pos.z));
            selectedCoord = placementCoord;
        }

        selectionCube.MoveToCoord(placementCoord, voxels.ContainsKey(placementCoord));
    }
}
