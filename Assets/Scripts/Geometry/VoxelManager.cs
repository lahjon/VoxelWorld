using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// //Debug
// using System.Diagnostics;

public class VoxelManager : MonoBehaviour, ISaveable
{
    public Dictionary<Vector3Int, Voxel> voxels = new Dictionary<Vector3Int, Voxel>();
    public Dictionary<Vector3Int, Voxel> processingVoxels = new Dictionary<Vector3Int, Voxel>();
    public Material normalMaterial, transparentMaterial;
    List<List<Vector3Int>> Brushes = new List<List<Vector3Int>>();
    public PositionHandle positionHandle;
    //public List<Vector3Int> selectedVoxels = new List<Vector3Int>();
    public List<Vector3Int> selectedVoxels = new List<Vector3Int>();
    public Dictionary<Vector3Int, float3> paintedVoxels = new Dictionary<Vector3Int, float3>();
    public float voxelSize;
    public TMP_Text brushSizeText;
    public GameObject gridPlane;
    public LayerMask lm, lme;
    
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
    public ProceduralMesh proceduralMesh, boundryMesh, processingMesh;
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
    public static float3 color;    
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
        BuildBrushSizes();
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
                if (NoKeys && processingVoxels.Count > 0)
                {
                    commandManager.AddCommand(new CommandAdd(processingVoxels.Keys.ToList(), color), true);
                }
                NoKeys = false;
                ClearProcessingMesh();
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
                processingMesh.SetMaterial(transparentMaterial);
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
                if (NoKeys && processingVoxels.Count > 0)
                {
                    commandManager.AddCommand(new CommandRemove(processingVoxels.Keys.ToList(), color), false);
                }
                NoKeys = false;

                processingMesh.SetMaterial(normalMaterial);
                ClearProcessingMesh();
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
                commandManager.AddCommand(new CommandChangeColor(paintedVoxels.Keys.ToList(), color, paintedVoxels.Values.ToList()), false);
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
    void CreateProcessingMesh()
    {   
        Debug.Log("Generate");
        processingMesh.GenerateMesh(processingVoxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    void ClearProcessingMesh()
    {
        processingVoxels.Clear();
        processingMesh.GenerateMesh(processingVoxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }

    #endregion

    #region button
    public void UpdateBrushSlider()
    {
        brushSize = (int)brushSlider.value;
        brushSizeText.text = ((int)brushSlider.value + 1).ToString();
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

        List<Vector3Int> coords = DrawLine(latestPlacementCoord, placementCoord);
        if (coords.Count > 0)
        {
            latestPlacementCoord = coords[coords.Count - 1];
        }
        commandManager.AddCommand(new CommandDrawLine(coords, color));

    }

    void TryRemoveLine()
    {
        List<Vector3Int> coords = DrawLine(latestSelectedCoord, selectedCoord);
        if (coords.Count > 0)
        {
            latestSelectedCoord = coords[coords.Count - 1];
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
        List<Vector3Int> brush = GetBrush(selectedCoord);
        if (!processingVoxels.ContainsKey(selectedCoord))
        {
            VoxelManager.instance.AddProcessingVoxels(brush);
            VoxelManager.instance.RemoveVoxels(brush);
        }
    }

    List<Vector3Int> GetBrush(Vector3Int coord)
    {
        List<Vector3Int> newCoords = new List<Vector3Int>();
        for (int i = 0; i < Brushes[brushSize].Count(); i++)
        {
            Vector3Int aCoord = Brushes[brushSize][i] + coord;
            newCoords.Add(aCoord);
        }
        return newCoords;
    }

    void TryAddVoxels()
    {
        latestSelectedCoord = selectedCoord;
        if (latestAddedCoord != selectedCoord)
        {
            latestAddedCoord = placementCoord;
            latestPlacementCoord = placementCoord;

            VoxelManager.instance.AddProcessingVoxels(GetBrush(placementCoord), color);

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

            List<Vector3Int> coords = GrowSelectionCubicPaint(voxel.coord, DirectionStruct.NormalToDirection(placementNormal), brushSize);
            SetVoxelsColor(coords, color);
        }
    }

    void TryFillColor()
    {
        Voxel voxel;
        if (voxels.TryGetValue(selectedCoord, out voxel))
        {
            commandManager.AddCommand(new CommandFillColor(SelectAllConnected(voxel), color, voxel.color));
        }
    }
    #endregion

    #region commands
    public void AddMoveCommand(Vector3Int offset)
    {
        commandManager.AddCommand(new CommandMove(selectedVoxels, offset), false);
    }
    public void TransformVoxels(Vector3Int offset)
    {
        MoveVoxels(selectedVoxels.ToList(), offset);
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
    public void SetVoxelColors(List<Vector3Int> coords, List<float3>  colors)
    {
        Voxel voxel;
        for (int i = 0; i < coords.Count; i++)
        {
            if (voxels.TryGetValue(coords[i], out voxel))
            {
                voxel.color = colors[i];
            }
        }
        CreateMesh();
    }
    public void SetVoxelsColor(List<Vector3Int> coords, float3 aColor)
    {
        Voxel voxel;
        for (int i = 0; i < coords.Count; i++)
        {
            if (voxels.TryGetValue(coords[i], out voxel))
            {
                voxel.color = aColor;
            }
        }
        CreateMesh();
    }
    // public void AddSingleVoxel(Vector3Int coord, Color aColor)
    // {
    //     AddVoxel(coord, aColor);
    //     CreateMesh();
    // }
    Voxel AddVoxel(Vector3Int coord, float3 aColor)
    {
        if (!voxels.ContainsKey(coord) && InBounds(coord))
        {
            Voxel voxel = new Voxel(coord, aColor, voxels);
            voxels.Add(coord, voxel);
            return voxel;
        }
        return null;
    }
    Voxel AddProcessingVoxel(Vector3Int coord, float3 aColor)
    {
        if (!processingVoxels.ContainsKey(coord) && !voxels.ContainsKey(coord) &&InBounds(coord))
        {
            Voxel voxel = new Voxel(coord, aColor, processingVoxels);
            processingVoxels.Add(coord, voxel);
            return voxel;
        }
        return null;
    }

    Voxel AddProcessingVoxel(Vector3Int coord)
    {
        if (!processingVoxels.ContainsKey(coord) && voxels.ContainsKey(coord) && InBounds(coord))
        {
            Voxel voxel = new Voxel(coord, float3.zero, processingVoxels);
            processingVoxels.Add(coord, voxel);
            return voxel;
        }
        return null;
    }
    bool InBounds(Vector3Int coord)
    {
        return coord.x >= bounds.min.x && coord.y >= 0 && coord.z >= bounds.min.z && coord.x < bounds.max.x && coord.y < bounds.max.y * 2 && coord.z < bounds.max.z;
    }
    public void AddVoxels(List<Vector3Int> addVoxels, float3 aColor)
    {
        int ct = 0;
        for (int i = 0; i < addVoxels.Count; i++)
        {
            if (AddVoxel(addVoxels[i], aColor) != null)
            {
                ct++;
            }
            //AddVoxel(addVoxels[i], aColor);
        }
        if (ct > 0)
            CreateMesh();
    }
    public void AddProcessingVoxels(List<Vector3Int> addVoxels, float3 aColor)
    {
        int ct = 0;
        for (int i = 0; i < addVoxels.Count; i++)
        {
            if (AddProcessingVoxel(addVoxels[i], aColor) != null)
            {
                ct++;
            }
        }
        if (ct > 0)
            CreateProcessingMesh();
    }
    public void AddProcessingVoxels(List<Vector3Int> addVoxels)
    {
        int ct = 0;
        for (int i = 0; i < addVoxels.Count; i++)
        {
            if (AddProcessingVoxel(addVoxels[i]) != null)
            {
                ct++;
            }
        }
        if (ct > 0)
            CreateProcessingMesh();
    }
    // public void RemoveSingleVoxel(Vector3Int coord)
    // {
    //     RemoveVoxel(coord);
    //     CreateMesh();
    // }
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
    public void RemoveVoxels(List<Vector3Int> removeVoxels)
    {
        for (int i = 0; i < removeVoxels.Count; i++)
        {
            RemoveVoxel(removeVoxels[i]);
        }
        CreateMesh();
    }


    Vector3Int[] DrawLineWalk(Vector3Int c0, Vector3Int c1)
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
    List<Vector3Int> DrawLine(Vector3Int p0, Vector3Int p1)
    {
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
            if ( (1 + 2 * ix) * ny < (1 + 2 * iy) * nx )
            {
                if ( (1 + 2 * ix) * nz < (1 + 2 * iz) * nx )
                {
                    p.x += sign_x;
                    ix += 1;
                }
                else
                {
                    p.z += sign_z;
                    iz += 1;
                }
            }
            else if ( (1 + 2 * iy) * nx < (1 + 2 * ix) * ny )
            {
                if ( (1 + 2 * iy) * nz < (1 + 2 * iz) * ny )
                {
                    p.y += sign_y;
                    iy += 1;
                }
                else
                {
                    p.z += sign_z;
                    iz += 1;
                }
            }
            else
            {
                if ( (1 + 2 * iz) * ny > (1 + 2 * iy) * nz )
                {
                    p.y += sign_y;
                    iy += 1;
                }
                else
                {
                    p.z += sign_z;
                    iz += 1;
                }
            }

            points.AddRange(GetBrush(new Vector3Int(p.x, p.y, p.z)));

            if (counter > 50) return points;
        }
        return points;
    }
    List<Vector3Int> DrawLineInvert(Vector3Int c1, Vector3Int c2)
    {
        int n = DiagonalDistance(c1, c2);
        List<Vector3Int> aVoxels = new List<Vector3Int>();
        for (int i = 0; i <= n; i++)
        {
            float t = n == 0 ? 0f : (float)i / n;
            aVoxels.Add(RoundCoord(Vector3.Lerp(c1, c2, t)));
        }
        return aVoxels;
    }
    public void MoveVoxels(List<Vector3Int> coords, Vector3Int offset)
    {
        Voxel voxel;
        for (int i = 0; i < coords.Count; i++)
        {
            if (voxels.TryGetValue(coords[i], out voxel))
            {
                voxel.coord += offset;
            }
        }
        CreateMesh();
    }

    List<Vector3Int> GrowSelectionCubic(Vector3Int aCoord, Direction direction, int steps, int operation)
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
                    if (!processingVoxels.ContainsKey(coord))
                    {
                        results.Add(coord);
                    }
                }
            }
        }

        if (operation == 0)
        {
            selectedVoxels.AddRange(results);
        }

        return results.ToList();
    }

    void BuildBrushSizes()
    {
        int min = 1;
        int max = 6;
        List<Vector3Int> brush = new List<Vector3Int>();
        for (int n = min; n < max; n++)
        {
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    for (int z = 0; z < n; z++)
                    {
                        brush.Add(new Vector3Int(x - n / 2, y, z - n / 2));
                    }
                }
            }
            Brushes.Add(brush.ToList());
            brush.Clear();
        }
    }

    List<Vector3Int>  GrowSelectionCubicPaint(Vector3Int aCoord, Direction direction, int steps)
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
        return paintedVoxels.Keys.ToList();
    }

    List<Vector3Int> GrowSelectionCubic(Vector3Int aCoord, Direction direction, int steps)
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
        return selectedVoxels;
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
    List<Vector3Int> SelectOutByNormal(Voxel voxel, Direction direction)
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
        return extrudeVoxels.ToList();
    }

    List<Vector3Int>  SelectInByNormal(Voxel voxel, Direction direction)
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
        return removeVoxels.ToList();
    }

    List<Vector3Int> SelectAllConnected(Voxel voxel)
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
        return foundVoxels.ToList();
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
        LayerMask aLm = Input.GetMouseButton(2) ? lme : lm;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, aLm))
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
            newVoxels.Add(a_SaveData.voxelDatas[i].coord, AddVoxel(a_SaveData.voxelDatas[i].coord, a_SaveData.voxelDatas[i].color));
        }
        voxels = newVoxels;
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    #endregion
}