using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.EventSystems;

public class VoxelManager : MonoBehaviour
{
    public Dictionary<Vector3Int, Voxel> voxels = new Dictionary<Vector3Int, Voxel>();
    public float voxelSize;
    public int gridLevel;
    public ProceduralMesh proceduralMesh;
    public SelectionCube selectionCube;
    Vector3Int placementCoord; // the coord in which the next voxel will be placed
    Vector3Int selectedCoord; // the coord currently selected
    Vector3Int? latestAddedCoord = null; // used to stop drawing when holding down mb
    Vector3Int latestSelectedCoord; // used to draw lines
    Vector3Int placementNormal; // the normal direction of the latest placement
    public static Color color;
    public static Direction[] AllDirections = new Direction[6]{
                                                Direction.XPos,
                                                Direction.XNeg,
                                                Direction.YPos,
                                                Direction.YNeg,
                                                Direction.ZPos,
                                                Direction.ZNeg,
                                            };

    public CommandManager commandManager;
    public CommandPanel commandPanel;
    public static VoxelManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        commandManager = new CommandManager(commandPanel);
    }
    public void ButtonClear()
    {
        voxels.Clear();
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    public void ButtonRedo()
    {
        commandManager.RedoCommand();
    }
    public void ButtonUndo()
    {
        commandManager.UndoCommand();
    }

    public void ButtonClearHistory()
    {
        commandManager.ClearHistory();
    }

    public void CreateMesh()
    {
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    void TryAddVoxel()
    {
        if (latestAddedCoord != selectedCoord && !voxels.ContainsKey(placementCoord))
        {
            latestAddedCoord = placementCoord;
            latestSelectedCoord = placementCoord;
            commandManager.AddCommand(new CommandAdd(placementCoord, color));
        }
    }
    void TryGrowVoxels()
    {
        if (voxels.ContainsKey(selectedCoord))
        {
            commandManager.AddCommand(new CommandGrow(ExtrudeByNormal(voxels[selectedCoord], DirectionStruct.NormalToDirection(placementNormal)), color));
        }
    }
    public void AddVoxel(Vector3Int coord)
    {
        Voxel voxel = new Voxel(new List<Voxel>(), coord, new float3(color.r, color.g, color.b));
        voxels.Add(coord, voxel);
        voxel.Neighbours = GetAllNeighbours(coord, true);
        CreateMesh();
    }
    public void AddVoxel(Vector3Int coord, Color aColor)
    {
        Voxel voxel = new Voxel(new List<Voxel>(), coord, new float3(aColor.r, aColor.g, aColor.b));
        voxels.Add(coord, voxel);
        voxel.Neighbours = GetAllNeighbours(coord, true);
        CreateMesh();
    }
    public void AddVoxels(Vector3Int[] addVoxels)
    {
        for (int i = 0; i < addVoxels.Length; i++)
        {
            if (!voxels.ContainsKey(addVoxels[i]))
            {
                Voxel voxel = new Voxel(new List<Voxel>(), addVoxels[i], new float3(color.r, color.g, color.b));
                voxels.Add(addVoxels[i], voxel);
                voxel.Neighbours = GetAllNeighbours(addVoxels[i], true);
            }
        }
        if (addVoxels.Length > 0)
        {
            latestSelectedCoord = addVoxels[addVoxels.Length - 1];
        }
        CreateMesh();
    }
    public void AddVoxels(Vector3Int[] addVoxels, Color aColor)
    {
        for (int i = 0; i < addVoxels.Length; i++)
        {
            if (!voxels.ContainsKey(addVoxels[i]))
            {
                Voxel voxel = new Voxel(new List<Voxel>(), addVoxels[i], new float3(aColor.r, aColor.g, aColor.b));
                voxels.Add(addVoxels[i], voxel);
                voxel.Neighbours = GetAllNeighbours(addVoxels[i], true);
            }
        }
        if (addVoxels.Length > 0)
        {
            latestSelectedCoord = addVoxels[addVoxels.Length - 1];
        }
        CreateMesh();
    }
    public void RemoveVoxel(Vector3Int coord)
    {
        if(voxels.ContainsKey(coord) && voxels[coord] is Voxel voxel)
        {
            for (int i = 0; i < voxel.Neighbours.Count; i++)
            {
                voxel.Neighbours[i].Neighbours.Remove(voxel);
            }
            voxels.Remove(coord);
            CreateMesh();
        }
    }
    void TryRemoveVoxel()
    {
        if(voxels.ContainsKey(selectedCoord) && voxels[selectedCoord] is Voxel voxel)
        {
            commandManager.AddCommand(new CommandRemove(selectedCoord, new Color(voxel.color.x, voxel.color.y, voxel.color.z)));
        }
    }
    void TryDrawLine()
    {
        if (latestSelectedCoord != null)
        {
            commandManager.AddCommand(new CommandDrawLine(DrawLine(latestSelectedCoord, placementCoord), color));
        }
    }
    void TryShrinkVoxel()
    {
        if (voxels.ContainsKey(selectedCoord))
        {
            commandManager.AddCommand(new CommandShrink(RemoveByNormal(voxels[selectedCoord], DirectionStruct.NormalToDirection(placementNormal)), color));
        }
    }
    public void RemoveVoxels(Vector3Int[] removeVoxels)
    {
        for (int i = 0; i < removeVoxels.Length; i++)
        {
            if(voxels.ContainsKey(removeVoxels[i]) && voxels[removeVoxels[i]] is Voxel voxel)
            {
                for (int k = 0; k < voxel.Neighbours.Count; k++)
                {
                    voxel.Neighbours[k].Neighbours.Remove(voxel);
                }
                voxels.Remove(removeVoxels[i]);
            }
        }
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
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            SetCoordMouseHover();
            if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
            {
                TryGrowVoxels();
            }
            else if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
            {
                TryDrawLine();
            }

            if (Input.GetMouseButton(0) && !(Input.GetKey(KeyCode.LeftControl)) && !(Input.GetKey(KeyCode.LeftShift)))
            {
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl))
                {
                    TryAddVoxel();
                }
            }   
            if (Input.GetMouseButtonDown(2) && !Input.GetKey(KeyCode.LeftControl))
            {
                TryRemoveVoxel();
            }
            else if (Input.GetMouseButtonDown(2) && Input.GetKey(KeyCode.LeftControl))
            {
                TryShrinkVoxel();
            }
            if (Input.GetMouseButtonUp(0))
            {
                latestAddedCoord = null;
            }

        }
    }

    public Voxel GetNeighbour(Vector3Int coord, Direction direction)
    {
        if (voxels.ContainsKey(coord + direction.ToCoord()) && voxels[coord + direction.ToCoord()] is Voxel voxel)
            return voxel;
        else
            return null;
    }   
    public List<Voxel> GetAllNeighbours(Vector3Int coord, bool add)
    {
        List<Voxel> neighbours = new List<Voxel>();

        for (int i = 0; i < AllDirections.Length; i++)
        {
            if (GetNeighbour(coord, AllDirections[i]) is Voxel voxel)
            {
                if (add)
                {
                    voxel.Neighbours.Add(voxels[coord]);
                }
                neighbours.Add(voxel);            }
        }   
        return neighbours;
    }
    Vector3Int[] ExtrudeByNormal(Voxel voxel, Direction direction)
    {
        HashSet<Vector3Int> extrudeVoxels = new HashSet<Vector3Int>();
        List<Voxel> searchVoxels = new List<Voxel>();
        extrudeVoxels.Add(voxel.coord + direction.ToCoord());
        searchVoxels.Add(voxel);
        Direction[] directions = DirectionStruct.AvailableDirections(direction);
        while (searchVoxels.Count > 0)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                if (GetVoxelByCoord(searchVoxels[0].coord + directions[i].ToCoord()) is Voxel aVoxel && !extrudeVoxels.Contains(aVoxel.coord + direction.ToCoord()))
                {
                    if (aVoxel.GetNeighbour(aVoxel.coord + placementNormal) == null)
                    {
                        extrudeVoxels.Add(aVoxel.coord + direction.ToCoord());
                        searchVoxels.Add(aVoxel);
                    }
                }
            }
            searchVoxels.RemoveAt(0);
        }
        return extrudeVoxels.ToArray();
    }

    Vector3Int[] RemoveByNormal(Voxel voxel, Direction direction)
    {
        HashSet<Vector3Int> removeVoxels = new HashSet<Vector3Int>();
        List<Voxel> searchVoxels = new List<Voxel>();
        removeVoxels.Add(voxel.coord);
        searchVoxels.Add(voxel);
        Direction[] directions = DirectionStruct.AvailableDirections(direction);
        while (searchVoxels.Count > 0)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                if (GetVoxelByCoord(searchVoxels[0].coord + directions[i].ToCoord()) is Voxel aVoxel &&!removeVoxels.Contains(aVoxel.coord))
                {
                    if (aVoxel.GetNeighbour(aVoxel.coord + placementNormal) == null)
                    {
                        removeVoxels.Add(aVoxel.coord);
                        searchVoxels.Add(aVoxel);
                    }
                }
            }
            searchVoxels.RemoveAt(0);
        }
        return removeVoxels.ToArray();
    }

    Voxel GetVoxelByCoord(Vector3Int coord)
    {
        if (voxels.ContainsKey(coord) && voxels[coord] is Voxel voxel)
        {
            return voxel;
        }
        return null;
    }
    HashSet<Voxel> GetConnectedVoxelsByPlame(Voxel voxel, Direction dir)
    {
        HashSet<Voxel> allVoxels = new HashSet<Voxel>();
        return allVoxels;
    }
    

    void SetCoordMouseHover()
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
            
            placementNormal = normal;
            placementCoord = selCoord + normal;
            selectedCoord = selCoord;
        }
        else if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);
            placementCoord = new Vector3Int(Mathf.FloorToInt(pos.x), gridLevel, Mathf.FloorToInt(pos.z));
            selectedCoord = placementCoord;
            placementNormal = Vector3Int.up;
        }

        selectionCube.MoveToCoord(placementCoord, voxels.ContainsKey(placementCoord));
    }
}
