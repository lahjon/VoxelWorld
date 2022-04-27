using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// //Debug
// using System.Diagnostics;

public class VoxelManager : MonoBehaviour, ISaveable
{
    public Dictionary<Vector3Int, Voxel> voxels = new Dictionary<Vector3Int, Voxel>();
    public PositionHandle positionHandle;
    //public List<Vector3Int> selectedVoxels = new List<Vector3Int>();
    public HashSet<Vector3Int> selectedVoxels = new HashSet<Vector3Int>();
    public Dictionary<Vector3Int, float3> paintedVoxels = new Dictionary<Vector3Int, float3>();
    public float voxelSize;
    public GameObject gridPlane;
    bool NoKeys;
    int gridLevelLock;
    bool _selectMode, _drawMode, _paintMode;
    public bool SelectMode
    {
        get => _selectMode;
        set
        {
            _selectMode = value;
            if (_selectMode)
            {
                DrawMode = false;
                PaintMode = false;
            }
            positionHandle.gameObject.SetActive(_selectMode);
        }
    }
    public bool DrawMode
    {
        get => _drawMode;
        set
        {
            _drawMode = value;
            if (_drawMode)
            {
                SelectMode = false;
                PaintMode = false;
            }
            selectionCube.gameObject.SetActive(_drawMode);
        }
    }

    public bool PaintMode
    {
        get => _paintMode;
        set
        {
            _paintMode = value;
            if (_paintMode)
            {
                SelectMode = false;
                DrawMode = false;
            }
        }
    }
    BoundsInt bounds;

    int _gridLevel;
    [SerializeField] int gridLevelMax = 50;
    int gridLevelMin = 0;
    public int GridLevel
    {
        get => _gridLevel;
        set
        {
            _gridLevel = value;
            gridPlane.transform.localPosition = new Vector3(gridPlane.transform.localPosition.x, GridLevel + -10.01f, gridPlane.transform.localPosition.z);
        }
    }
    public ProceduralMesh proceduralMesh;
    public SelectionCube selectionCube;
    Vector3Int placementCoord; // the coord in which the next voxel will be placed
    Vector3Int selectedCoord; // the coord currently selected
    Vector3Int? latestAddedCoord = null; // used to stop drawing when holding down mb
    //Vector3Int? latestRemoveCoord = null; // used to stop drawing when holding down mb
    Vector3Int latestPlacementCoord; // used to draw lines
    Vector3Int latestSelectedCoord; // used to draw lines
    Vector3Int latestPaintCoord; // used to draw lines
    Vector3Int placementNormal; // the normal direction of the latest placement
    Vector3Int lockedPlacementNormal; // the normal direction of the latest placement
    public static Color color;
    public CommandManager commandManager;
    public CommandPanel commandPanel;
    public ValuePicker valuePicker;
    public ColorWheel colorWheel;
    public Palette palette;
    public int brushSize;
    public Slider brushSlider;
    public static VoxelManager instance;
    #region standard
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        commandManager = new CommandManager(commandPanel);
    }

    void Start()
    {
        DrawMode = true;
        SelectMode = false;
        PaintMode = false;
        bounds = new BoundsInt(Vector3Int.one * -14, Vector3Int.one * 14);
    }
    void Update()
    {
        if (DrawMode && !EventSystem.current.IsPointerOverGameObject())
        {
            SetCoordMouseHover();

            // MB0
            if (Input.GetMouseButtonDown(0))
            {

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    TryGrowVoxels();
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    TryDrawLine();
                }
                else
                {
                    selectedVoxels.Clear();
                    gridLevelLock = placementCoord.y;
                    lockedPlacementNormal = placementNormal;
                    NoKeys = true;
                }
            }

            if (Input.GetMouseButton(0) && !(Input.GetKey(KeyCode.LeftControl)) && !(Input.GetKey(KeyCode.LeftShift)))
            {
                TryAddVoxels();
            }

            if (Input.GetMouseButtonUp(0) )
            {
                latestAddedCoord = null;
                if (NoKeys && selectedVoxels.Count > 0)
                {
                    commandManager.AddCommand(new CommandAdd(selectedVoxels.ToArray(), color), false);
                }
                NoKeys = false;
            }

            // MB2
            if (Input.GetMouseButtonDown(2) && Input.GetKey(KeyCode.LeftShift))
            {
                TryRemoveLine();
            }
            else if (Input.GetMouseButtonDown(2) && Input.GetKey(KeyCode.LeftControl))
            {
                TryShrinkVoxel();
            }
            else if (Input.GetMouseButtonDown(2))
            {
                selectedVoxels.Clear();
                lockedPlacementNormal = placementNormal;
                gridLevelLock = selectedCoord.y;
                NoKeys = true;
            }

            if (Input.GetMouseButton(2) && NoKeys)
            {
                TryRemoveVoxels();
            }

            if (Input.GetMouseButtonUp(2))
            {
                if (NoKeys && selectedVoxels.Count > 0)
                {
                    commandManager.AddCommand(new CommandRemove(selectedVoxels.ToArray(), color), false);
                }
                NoKeys = false;
            }


            UpdateGridLevel();
        }
        else if (PaintMode && !EventSystem.current.IsPointerOverGameObject())
        {
            SetCoordMouseHover();
            if (Input.GetMouseButtonDown(0))
            {

            }
            if (Input.GetMouseButton(0))
            {
                TrySetVoxelColor();
            }
            if (Input.GetMouseButtonUp(0))
            {
                commandManager.AddCommand(new CommandChangeColor(paintedVoxels.Keys.ToArray(), color, paintedVoxels.Values.ToArray()), false);
                paintedVoxels.Clear();
            }
            // Keys
            if (Input.GetKeyDown(KeyCode.I))
            {
                SampleColor();
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                TrySetVoxelColor();
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                TryFillColor();
            }
        }
    }
    void CreateMesh()
    {   
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }

    #endregion

    #region button
    public void UpdateBrushSlider()
    {
        brushSize = (int)brushSlider.value;
    }
    public void ButtonClear()
    {
        voxels.Clear();
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    public void ButtonUndo()
    {
        commandManager.UndoCommand();
    }
    public void ButtonRedo()
    {
        commandManager.RedoCommand();
    }
    public void ButtonClearHistory()
    {
        commandManager.ClearHistory();
    }
    public void ButtonSave()
    {
        SaveDataManager.SaveJsonData(Helpers.FindInterfacesOfType<ISaveable>());
    }
    public void ButtonLoad()
    {
        SaveDataManager.LoadJsonData(Helpers.FindInterfacesOfType<ISaveable>());
    }
    #endregion

    #region input

    void UpdateGridLevel()
    {
        if (Input.mouseScrollDelta.y > 0 && GridLevel < gridLevelMax)
        {
            GridLevel++;
        }
        else if (Input.mouseScrollDelta.y < 0 && GridLevel > gridLevelMin)
        {
            GridLevel--;
        }
    }
    void TryDrawLine()
    {

        Vector3Int[] coords = DrawLine(latestPlacementCoord, placementCoord);
        if (coords.Length > 0)
        {
            latestPlacementCoord = coords[coords.Length - 1];
        }
        commandManager.AddCommand(new CommandDrawLine(coords, color));

    }

    void TryRemoveLine()
    {
        Vector3Int[] coords = DrawLine(latestSelectedCoord, selectedCoord);
        if (coords.Length > 0)
        {
            latestSelectedCoord = coords[coords.Length - 1];
        }
        commandManager.AddCommand(new CommandRemoveLine(coords, color));
    }

    void TryShrinkVoxel()
    {
        if (voxels.ContainsKey(selectedCoord))
        {
            commandManager.AddCommand(new CommandShrink(SelectInByNormal(voxels[selectedCoord], DirectionStruct.NormalToDirection(placementNormal)), color));
        }
    }
    void TryRemoveVoxels()
    {
        VoxelManager.instance.RemoveVoxels(
                    GrowSelectionCubic(new Vector3Int(selectedCoord.x, gridLevelLock, selectedCoord.z),
                    DirectionStruct.NormalToDirection(lockedPlacementNormal),
                    brushSize));
    }

    void TryAddVoxels()
    {
        latestSelectedCoord = selectedCoord;
        if (latestAddedCoord != selectedCoord && !voxels.ContainsKey(placementCoord))
        {
            latestAddedCoord = placementCoord;
            latestPlacementCoord = placementCoord;

            VoxelManager.instance.AddVoxels(
                GrowSelectionCubic(new Vector3Int(placementCoord.x, gridLevelLock, placementCoord.z), 
                DirectionStruct.NormalToDirection(lockedPlacementNormal), 
                brushSize,
                0), 
                color);

        }
    }
    void TryGrowVoxels()
    {
        if (voxels.ContainsKey(selectedCoord))
        {
            commandManager.AddCommand(new CommandGrow(SelectOutByNormal(voxels[selectedCoord], DirectionStruct.NormalToDirection(placementNormal)), color));
        }
    }

    void TrySetVoxelColor()
    {
        Voxel voxel;
        if (voxels.TryGetValue(selectedCoord, out voxel) && selectedCoord != latestPaintCoord)
        {
            latestPaintCoord = voxel.coord;

            Vector3Int[] coords = GrowSelectionCubicPaint(voxel.coord, DirectionStruct.NormalToDirection(placementNormal), brushSize);
            SetVoxelsColor(coords, color);
        }
    }

    void TryFillColor()
    {
        Voxel voxel;
        if (voxels.TryGetValue(selectedCoord, out voxel))
        {
            commandManager.AddCommand(new CommandFillColor(SelectAllConnected(voxel), color, new Color(voxel.color.x, voxel.color.y, voxel.color.z)));
        }
    }
    #endregion

    #region commands
    public void AddMoveCommand(Vector3Int offset)
    {
        commandManager.AddCommand(new CommandMove(selectedVoxels.ToArray(), offset), false);
    }
    public void TransformVoxels(Vector3Int offset)
    {
        MoveVoxels(selectedVoxels.ToArray(), offset);
    }
    public void SetVoxelColor(Vector3Int coord, float3 aColor)
    {
        Voxel voxel;
        if (voxels.TryGetValue(coord, out voxel))
        {
            voxel.color = aColor;
            CreateMesh();
        }
    }
    public void SetVoxelColors(Vector3Int[] coords, float3[] colors)
    {
        Voxel voxel;
        for (int i = 0; i < coords.Length; i++)
        {
            if (voxels.TryGetValue(coords[i], out voxel))
            {
                voxel.color = colors[i];
            }
        }
        CreateMesh();
    }
    public void SetVoxelsColor(Vector3Int[] coords, Color aColor)
    {
        Voxel voxel;
        for (int i = 0; i < coords.Length; i++)
        {
            if (voxels.TryGetValue(coords[i], out voxel))
            {
                voxel.color = new float3(aColor.r, aColor.g, aColor.b);
            }
        }
        CreateMesh();
    }
    public void AddSingleVoxel(Vector3Int coord, Color aColor)
    {
        AddVoxel(coord, aColor);
        CreateMesh();
    }
    Voxel AddVoxel(Vector3Int coord, Color aColor)
    {
        if (!voxels.ContainsKey(coord) && InBounds(coord))
        {
            Voxel voxel = new Voxel(coord, new float3(aColor.r, aColor.g, aColor.b));
            voxels.Add(coord, voxel);
            return voxel;
        }
        return null;
    }
    bool InBounds(Vector3Int coord)
    {
        return coord.x >= bounds.min.x && coord.y >= bounds.min.y && coord.z >= bounds.min.z && coord.x < bounds.max.x && coord.y < bounds.max.y && coord.z < bounds.max.z;
    }
    public void AddVoxels(Vector3Int[] addVoxels, Color aColor)
    {
        int ct = 0;
        for (int i = 0; i < addVoxels.Length; i++)
        {
            if (AddVoxel(addVoxels[i], aColor) != null)
            {
                ct++;
            }
            AddVoxel(addVoxels[i], aColor);
        }
        if (ct > 0)
            CreateMesh();
    }
    public void RemoveSingleVoxel(Vector3Int coord)
    {
        RemoveVoxel(coord);
        CreateMesh();
    }
    public void RemoveVoxel(Vector3Int coord)
    {
        Voxel voxel;
        if (voxels.TryGetValue(coord, out voxel))
        {
            voxels.Remove(coord);
            for (int i = 0; i < voxel.neighbours.Length; i++)
            {
                if (voxel.neighbours[i] != null)
                {
                    voxel.neighbours[i].neighbours[(6 + i + 3) % 6] = null;
                }
            }
        }
    }
    public void RemoveVoxels(Vector3Int[] removeVoxels)
    {
        for (int i = 0; i < removeVoxels.Length; i++)
        {
            RemoveVoxel(removeVoxels[i]);
        }
        CreateMesh();
    }


    Vector3Int[] DrawLine(Vector3Int c0, Vector3Int c1)
    {
        // less function call
        List<Vector3Int> points = new List<Vector3Int>();
        int dx = c1.x - c0.x;
        int dy = c1.y - c0.y;
        int dz = c1.z - c0.z;
        int n = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy), Mathf.Abs(dz));
        float divN = n == 0 ? 0.0f : 1.0f / n;
        float xstep = dx * divN;
        float ystep = dy * divN;
        float zstep = dz * divN;
        float x = c0.x, y = c0.y, z = c0.z;
        for (int step = 0; step <= n; step++, x += xstep, y += ystep, z += zstep) 
        {
            points.Add(new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z)));
        }
        return points.ToArray();
    }
    Vector3Int[] DrawLineWalk(Vector3Int p0, Vector3Int p1)
    {
        // FAILS WALK
        int dx = p1.x - p0.x;
        int dy = p1.y - p0.y;
        int dz = p1.z - p0.z;
        int nx = Mathf.Abs(dx);
        int ny = Mathf.Abs(dy);
        int nz = Mathf.Abs(dz);
        int sign_x = dx > 0 ? 1 : -1;
        int sign_y = dy > 0 ? 1 : -1;
        int sign_z = dz > 0 ? 1 : -1;

        Vector3Int p = new Vector3Int(p0.x, p0.y, p0.z);
        List<Vector3Int> points = new List<Vector3Int>();
        points.Add(p);

        int counter = 0;

        for (int ix = 0, iy = 0, iz = 0; ix < nx || iy < ny || iz < nz;)
        {
            counter++;
            if ((.5f + ix) / nx < (.5f + iy) / ny && (.5f + ix) / nx < (.5f + iz) / nz)
            {
                // if ( (1 + 2 * ix) * ny < (1 + 2 * iy) * nx && (1 + 2 * ix) * nz < (1 + 2 * iz) * nx)
                p.x += sign_x;
                ix += 1;
            }
            else if ((.5f + iy) / ny < (.5f + ix) / nx && (.5f + iy) / ny < (.5f + iz) / nz)
            {
                // else if ( (1 + 2 * iy) * nx < (1 + 2 * ix) * ny && (1 + 2 * iy) * nz < (1 + 2 * iz) * ny)
                p.y += sign_y;
                iy += 1;
            }
            else
            {
                p.z += sign_z;
                iz += 1;
            }

            points.Add(new Vector3Int(p.x, p.y, p.z));

            if (counter > 50) return points.ToArray();
        }
        return points.ToArray();
    }
    Vector3Int[] DrawLineInvert(Vector3Int c1, Vector3Int c2)
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
    public void MoveVoxels(Vector3Int[] coords, Vector3Int offset)
    {
        Voxel voxel;
        for (int i = 0; i < coords.Length; i++)
        {
            if (voxels.TryGetValue(coords[i], out voxel))
            {
                voxel.coord += offset;
            }
        }
        CreateMesh();
    }

    Vector3Int[] GrowSelectionCubic(Vector3Int aCoord, Direction direction, int steps, int operation)
    {
        // used for add and paint
        HashSet<Vector3Int> results = new HashSet<Vector3Int>();
        int xMinStep = direction != Direction.XPos ? -steps : 0;
        int xMaxStep = direction != Direction.XNeg ? steps : 0;
        int yMinStep = direction != Direction.YPos ? -steps : 0;
        int yMaxStep = direction != Direction.YNeg ? steps : 0;
        int zMinStep = direction != Direction.ZPos ? -steps : 0;
        int zMaxStep = direction != Direction.ZNeg ? steps : 0;
        Vector3Int coord;
        for (int x = xMinStep; x <= xMaxStep; x++)
        {
            for (int y = yMinStep; y <= yMaxStep; y++)
            {
                for (int z = zMinStep; z <= zMaxStep; z++)
                {
                    coord = new Vector3Int(aCoord.x + x, aCoord.y + y, aCoord.z + z);
                    if (!voxels.ContainsKey(coord))
                    {
                        results.Add(coord);
                    }
                }
            }
        }

        if (operation == 0)
        {
            selectedVoxels.UnionWith(results);
        }

        return results.ToArray();
    }

    Vector3Int[] GrowSelectionCubicPaint(Vector3Int aCoord, Direction direction, int steps)
    {
        // used for paint
        int xMinStep = direction != Direction.XPos ? -steps : 0;
        int xMaxStep = direction != Direction.XNeg ? steps : 0;
        int yMinStep = direction != Direction.YPos ? -steps : 0;
        int yMaxStep = direction != Direction.YNeg ? steps : 0;
        int zMinStep = direction != Direction.ZPos ? -steps : 0;
        int zMaxStep = direction != Direction.ZNeg ? steps : 0;
        Vector3Int coord;
        for (int x = xMinStep; x <= xMaxStep; x++)
        {
            for (int y = yMinStep; y <= yMaxStep; y++)
            {
                for (int z = zMinStep; z <= zMaxStep; z++)
                {
                    coord = new Vector3Int(aCoord.x + x, aCoord.y + y, aCoord.z + z);
                    if (voxels.ContainsKey(coord) && !paintedVoxels.ContainsKey(coord))
                    {
                        paintedVoxels.Add(coord, voxels[coord].color);
                    }
                }
            }
        }
        return paintedVoxels.Keys.ToArray();
    }

    Vector3Int[] GrowSelectionCubic(Vector3Int aCoord, Direction direction, int steps)
    {
        // used for paint
        int xMinStep = direction != Direction.XPos ? -steps : 0;
        int xMaxStep = direction != Direction.XNeg ? steps : 0;
        int yMinStep = direction != Direction.YPos ? -steps : 0;
        int yMaxStep = direction != Direction.YNeg ? steps : 0;
        int zMinStep = direction != Direction.ZPos ? -steps : 0;
        int zMaxStep = direction != Direction.ZNeg ? steps : 0;
        Vector3Int coord;
        for (int x = xMinStep; x <= xMaxStep; x++)
        {
            for (int y = yMinStep; y <= yMaxStep; y++)
            {
                for (int z = zMinStep; z <= zMaxStep; z++)
                {
                    coord = new Vector3Int(aCoord.x + x, aCoord.y + y, aCoord.z + z);
                    if (voxels.ContainsKey(coord) && !paintedVoxels.ContainsKey(coord))
                    {
                        selectedVoxels.Add(coord);
                    }
                }
            }
        }
        return selectedVoxels.ToArray();
    }

    List<Vector3Int> GetCoords(Vector3Int coord, Direction direction, Direction[] directions)
    {
        List<Vector3Int> coords = new List<Vector3Int>();
        for (int i = 0; i < directions.Length; i++)
        {
            coords.Add(coord + directions[i].ToCoord());
        }
        return coords;
    }
    Vector3Int[] SelectOutByNormal(Voxel voxel, Direction direction)
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
                    if (GetVoxelByCoord(aVoxel.coord + placementNormal) == null)
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

    Vector3Int[] SelectInByNormal(Voxel voxel, Direction direction)
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
                if (GetVoxelByCoord(searchVoxels[0].coord + directions[i].ToCoord()) is Voxel aVoxel && !removeVoxels.Contains(aVoxel.coord))
                {
                    if (GetVoxelByCoord(aVoxel.coord + placementNormal) == null)
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

    Vector3Int[] SelectAllConnected(Voxel voxel)
    {
        HashSet<Vector3Int> foundVoxels = new HashSet<Vector3Int>();
        List<Voxel> searchVoxels = new List<Voxel>();
        foundVoxels.Add(voxel.coord);
        searchVoxels.Add(voxel);
        while (searchVoxels.Count > 0)
        {
            for (int i = 0; i < DirectionStruct.Directions.Length; i++)
            {
                if (GetVoxelByCoord(searchVoxels[0].coord + DirectionStruct.Directions[i].ToCoord()) is Voxel aVoxel && !foundVoxels.Contains(aVoxel.coord))
                {
                    foundVoxels.Add(aVoxel.coord);
                    searchVoxels.Add(aVoxel);
                }
            }
            searchVoxels.RemoveAt(0);
        }
        return foundVoxels.ToArray();
    }

    void SampleColor()
    {
        Voxel voxel;
        if (voxels.TryGetValue(selectedCoord, out voxel))
        {
            valuePicker.SetColor(new Color(voxel.color.x, voxel.color.y, voxel.color.z));
        }
    }
    #endregion

    #region voxels
    Voxel GetVoxelByCoord(Vector3Int coord)
    {
        if (voxels.ContainsKey(coord) && voxels[coord] is Voxel voxel)
        {
            return voxel;
        }
        return null;
    }
    int DiagonalDistance(Vector3Int c1, Vector3Int c2) => Mathf.Max(Mathf.Abs(c2.x - c1.x), Mathf.Abs(c2.y - c1.y), Mathf.Abs(c2.z - c1.z));
    Vector3Int RoundCoord(Vector3 coord)
    {
        return new Vector3Int(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y), Mathf.RoundToInt(coord.z));
    }
    #endregion

    #region coords
    void SetCoordMouseHover()
    {
        Plane plane = new Plane(Vector3.up, GridLevel);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3Int normal = Vector3Int.FloorToInt(hit.normal);
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
            placementCoord = new Vector3Int(Mathf.FloorToInt(pos.x), GridLevel, Mathf.FloorToInt(pos.z));
            selectedCoord = placementCoord;
            placementNormal = Vector3Int.up;
        }

        selectionCube.MoveToCoord(placementCoord, voxels.ContainsKey(placementCoord));
    }
    #endregion

    #region save

    public void PopulateSaveData(SaveData a_SaveData)
    {
        // SaveDataVoxel saveDataVoxel = (SaveDataVoxel)a_SaveData;
        Voxel[] allVoxels = voxels.Values.ToArray();
        VoxelData[] voxelDatas = new VoxelData[allVoxels.Length];
        for (int i = 0; i < allVoxels.Length; i++)
        {
            voxelDatas[i] = new VoxelData(allVoxels[i].coord, allVoxels[i].color);
        }
        a_SaveData.voxelDatas = voxelDatas;
    }

    public void LoadFromSaveData(SaveData a_SaveData)
    {
        voxels.Clear();
        Dictionary<Vector3Int, Voxel> newVoxels = new Dictionary<Vector3Int, Voxel>();
        for (int i = 0; i < a_SaveData.voxelDatas.Length; i++)
        {
            newVoxels.Add(a_SaveData.voxelDatas[i].coord, new Voxel(a_SaveData.voxelDatas[i]));
        }
        voxels = newVoxels;
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    #endregion
}