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
using UnityEngine.InputSystem;

// //Debug
// using System.Diagnostics;

public class VoxelManager : MonoBehaviour, ISaveable
{
    public Dictionary<Vector3Int, Voxel> voxels = new Dictionary<Vector3Int, Voxel>();
    public Dictionary<Vector3Int, Voxel> tempVoxels = new Dictionary<Vector3Int, Voxel>();
    public Dictionary<Vector3Int, Voxel> processingVoxels = new Dictionary<Vector3Int, Voxel>();
    public Dictionary<Vector3Int, Voxel> selectedVoxels = new Dictionary<Vector3Int, Voxel>();
    public bool canPlace;
    public PlayerInput playerInput;
    public BuilderController builderController;
    public PlayerController playerController;
    public Vector3Int deltaCoord;
    public Material normalMaterial, transparentMaterial;
    List<List<Vector3Int>> Brushes = new List<List<Vector3Int>>();
    public Dictionary<Vector3Int, float3> paintedVoxels = new Dictionary<Vector3Int, float3>();
    public SelectionController selectionController;
    int _voxelSize = 1;
    int[] voxelSizes = new int[6] {2,4,8,10,16,20};
    public int VoxelSize
    {
        get => voxelSizes[_voxelSize - 1];
        set
        {
            _voxelSize = value;
        }
    }
    public TMP_Text brushSizeText, voxelSizeText;
    public GameObject gridPlane;
    public LayerMask lmv, lmg, lmvg, lme;
    LayerMask aLm;
    
    bool NoKeys;
    int gridLevelLock;
    bool _transformMode, _drawMode, _paintMode, _selectMode;
    public bool TransformMode
    {
        get => _transformMode;
        set
        {
            _transformMode = value;
            if (_transformMode)
            {
                DrawMode = false;
                PaintMode = false;
                SelectMode = false;
            }
            
            selectionController.gameObject.SetActive(_transformMode);
        }
    }
    public bool SelectMode
    {
        get => _selectMode;
        set
        {
            _selectMode = value;
            if (_selectMode)
            {
                TransformMode = false;
                PaintMode = false;
                DrawMode = false;
            }
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
                TransformMode = false;
                PaintMode = false;
                SelectMode = false;
            }
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
                TransformMode = false;
                DrawMode = false;
                SelectMode = false;
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
    public ProceduralMesh proceduralMesh, boundryMesh, boundryVisMesh, processingMesh, cursorMesh, subtractProcessingMesh, selectionMesh;
    public SelectionCube selectionCube;
    Vector3Int placementCoord; // the coord in which the next voxel will be placed
    Vector3Int selectedCoord; // the coord currently selected
    Vector3Int? latestAddedCoord = null; // used to stop drawing when holding down mb
    Vector3Int latestPlacementCoord; // used to draw lines
    Vector3Int latestSelectedCoord; // used to draw lines
    Vector3Int latestPaintCoord; // used to draw lines
    Vector3Int startCoord; // used to draw lines
    public Vector3Int placementNormal; // the normal direction of the latest placement
    public static float3 color;    
    public int[] gridBounds = new int[3]
    {
        1, 1, 1
    };
    bool isSelecting;
    public CommandManager commandManager;
    public CommandPanel commandPanel;
    public ValuePicker valuePicker;
    public ColorWheel colorWheel;
    public Palette palette;
    public GameObject proceduralMeshPrefab, processingMeshPrefab, cursorMeshPrefab, boundryMeshPrefab, boundryMeshVisPrefab, subtractProcessingMeshPrefab, selectionMeshPrefab;
    public int brushSize;
    public Slider brushSlider, voxelSlider;
    public TMP_Dropdown[] boundsDropdown;
    public TMP_Dropdown actionDropdown;
    public static VoxelManager instance;
    #region standard
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        playerInput = new PlayerInput();
        playerController.playerInput = playerInput;
        builderController.playerInput = playerInput;

        commandManager = new CommandManager(commandPanel);
        proceduralMesh = Instantiate(proceduralMeshPrefab).GetComponent<ProceduralMesh>();
        boundryMesh = Instantiate(boundryMeshPrefab).GetComponent<ProceduralMesh>();
        boundryVisMesh = Instantiate(boundryMeshVisPrefab).GetComponent<ProceduralMesh>();
        processingMesh = Instantiate(processingMeshPrefab).GetComponent<ProceduralMesh>();
        subtractProcessingMesh = Instantiate(subtractProcessingMeshPrefab).GetComponent<ProceduralMesh>();
        cursorMesh = Instantiate(cursorMeshPrefab).GetComponent<ProceduralMesh>();
        selectionMesh = Instantiate(selectionMeshPrefab).GetComponent<ProceduralMesh>();

        selectionMesh.gameObject.name = "selectionMesh";
        boundryVisMesh.gameObject.name = "boundryVisMesh";
        subtractProcessingMesh.gameObject.name = "subtractProcessingMesh";
        cursorMesh.gameObject.name = "cursorMesh";
        processingMesh.gameObject.name = "processingMesh";
        boundryMesh.gameObject.name = "boundryMesh";
        proceduralMesh.gameObject.name = "proceduralMesh";

        aLm = lmvg;
    }

    void Start()
    {
        DrawMode = true;
        TransformMode = false;
        PaintMode = false;
        SelectMode = false;
        BuildBrushSizes();
        ButtonInitialize();
        CreateDropdown();
    }

    public void CreateDropdown()
    {
        string[] allCommands = System.Enum.GetNames(typeof(BuildCommand));
        for (int i = 0; i < allCommands.Length; i++)
        {
            actionDropdown.options.Add(new TMP_Dropdown.OptionData() {text = allCommands[i]});
        }
        actionDropdown.onValueChanged.AddListener((value) => builderController.BuildCommand = (BuildCommand)value);
    }



    public void StartRemovingVoxels()
    {
        processingMesh.SetMaterial(transparentMaterial);
    }

    public void StopRemovingVoxels()
    {
        if (processingVoxels.Count > 0)
        {
            commandManager.AddCommand(new CommandRemove(processingVoxels.Keys.ToList(), color), false);
        }

        processingMesh.SetMaterial(normalMaterial);
        ClearProcessingMesh();
    }

    public void StopPaintingVoxels()
    {
        commandManager.AddCommand(new CommandChangeColor(paintedVoxels.Keys.ToList(), color, paintedVoxels.Values.ToList()), false);
        paintedVoxels.Clear();
    }
    public void StartInteractiveDrawLine()
    {
        isSelecting = true;
        startCoord = selectedCoord + placementNormal;
    }
    public void PerformInteractiveDrawLine()
    {
        if (selectedCoord != latestSelectedCoord)
        {
            ClearProcessingMesh();
            TryAddVoxels(GetBrush(DrawLineDiagonal(startCoord, selectedCoord + placementNormal)));
            cursorMesh.GenerateMesh(new Face(DirectionStruct.INormals[placementNormal], new float3(selectedCoord.x, selectedCoord.y, selectedCoord.z), float3.zero));
        }
    }
    public void StopInteractiveDrawLine()
    {
        if (processingVoxels.Count > 0)
        {
            commandManager.AddCommand(new CommandDrawLine(processingVoxels.Keys.ToList(), color), true);
        }
        ClearProcessingMesh();
        isSelecting = false;
    }

    public void StartBoxDragAdd()
    {
        isSelecting = true;
        startCoord = selectedCoord + placementNormal;
    }

    public void PerformBoxDragAdd()
    {
        if (selectedCoord != latestSelectedCoord)
        {
            ClearProcessingMesh();
            TryAddVoxels(BoxDrag(startCoord, selectedCoord + placementNormal));
            cursorMesh.GenerateMesh(new Face(DirectionStruct.INormals[placementNormal], new float3(selectedCoord.x, selectedCoord.y, selectedCoord.z), float3.zero));
        }
    }
    
    public void StopBoxDragAdd()
    {
        if (processingVoxels.Count > 0)
        {
            commandManager.AddCommand(new CommandAdd(processingVoxels.Keys.ToList(), color), true);
        }
        ClearProcessingMesh();
        isSelecting = false;
    }

    public void StartBoxDragRemove()
    {
        isSelecting = true;
        if (startCoord.x > 0 || startCoord.y > 0 || startCoord.z > 0)
        {
            startCoord = selectedCoord + placementNormal;
        }
        else
        {
            startCoord = selectedCoord - placementNormal;
        }
    }

    public void PerformBoxDragRemove()
    {
        if (selectedCoord != latestSelectedCoord)
        {
            latestSelectedCoord = selectedCoord;

            // OPTIMIZE THIS
            voxels = voxels.Concat(processingVoxels)
            .GroupBy(kv => kv.Key)
            .ToDictionary(g => g.Key, g => g.First().Value);
            
            ClearProcessingMesh();
            TryRemoveVoxels(BoxDrag(startCoord, selectedCoord));
        
            cursorMesh.GenerateMesh(new Face(DirectionStruct.INormals[placementNormal], new float3(selectedCoord.x, selectedCoord.y, selectedCoord.z), float3.zero));
        }
    }
    public void StopBoxDragRemove()
    {
        if (processingVoxels.Count > 0)
        {
            commandManager.AddCommand(new CommandRemoveDrag(processingVoxels.Keys.ToList(), processingVoxels.Values.Select(x => x.color).ToList()), true);
        }
        ClearProcessingMesh();
        isSelecting = false;
    }

    public void StartBoxDragSelect()
    {
        isSelecting = true;
        if (startCoord.x > 0 || startCoord.y > 0 || startCoord.z > 0)
        {
            startCoord = selectedCoord + placementNormal;
        }
        else
        {
            startCoord = selectedCoord - placementNormal;
        }
    }
    public void PerformBoxDragSelect()
    {
        if (selectedCoord != latestSelectedCoord)
        {
            latestSelectedCoord = selectedCoord;

            // OPTIMIZE THIS
            voxels = voxels.Concat(selectedVoxels)
            .GroupBy(kv => kv.Key)
            .ToDictionary(g => g.Key, g => g.First().Value);
            
            ClearSelectionMesh();
            TrySelectVoxels(BoxDrag(startCoord, selectedCoord));

            cursorMesh.GenerateMesh(new Face(DirectionStruct.INormals[placementNormal], new float3(selectedCoord.x, selectedCoord.y, selectedCoord.z), float3.zero));
        }
    }
    public void StopBoxDragSelect()
    {
        if (processingVoxels.Count > 0)
        {
            // CREATE SELECT COMMAND
        }
        isSelecting = false;
    }

    void Update()
    {
        SetCoordMouseHover();
    }

    public Vector3 SelectCoordToWorldSpace() => selectedCoord * VoxelSize;

    public void StopAddingVoxels()
    {
        latestAddedCoord = null;
        if (processingVoxels.Count > 0)
        {
            commandManager.AddCommand(new CommandAdd(processingVoxels.Keys.ToList(), color), true);
        }
        ClearProcessingMesh();
    }

    List<Vector3Int> BoxDrag(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> selectedCoords = new List<Vector3Int>();
        int xStart;
        int yStart;
        int zStart;
        int xEnd;
        int yEnd;
        int zEnd;
        if (start.x >= end.x)
        {
            xStart = end.x;
            xEnd = start.x;
        }
        else
        {
            xStart = start.x;
            xEnd = end.x;
        }
        if (start.y >= end.y)
        {
            yStart = end.y;
            yEnd = start.y;
        }
        else
        {
            yStart = start.y;
            yEnd = end.y;
        }
        if (start.z >= end.z)
        {
            zStart = end.z;
            zEnd = start.z;
        }
        else
        {
            zStart = start.z;
            zEnd = end.z;
        }
        for (int x = xStart; x <= xEnd; x++)
        {
            for (int y = yStart; y <= yEnd; y++)
            {
                for (int z = zStart; z <= zEnd; z++)
                {
                    selectedCoords.Add(new Vector3Int(x,y,z));
                }
            }
        }
        return selectedCoords;
    }

    public void CreateMesh()
    {   
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    void CreateProcessingMesh()
    {   
        processingMesh.GenerateMesh(processingVoxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    void CreateSubtractProcessingMesh()
    {   
        subtractProcessingMesh.GenerateMesh(processingVoxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    void CreateSelectionMesh()
    {   
        selectionMesh.GenerateMesh(selectedVoxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    void ClearProcessingMesh()
    {
        processingVoxels.Clear();
        processingMesh.GenerateMesh(new Face[0]);
        subtractProcessingMesh.GenerateMesh(new Face[0]);
    }

    void ClearSelectionMesh()
    {
        selectedVoxels.Clear();
        selectionMesh.GenerateMesh(new Face[0]);
    }

    void ClearMesh()
    {
        voxels.Clear();
        proceduralMesh.GenerateMesh(new Face[0]);
    }

    void ClearSelection()
    {
        voxels = voxels.Concat(processingVoxels)
                        .GroupBy(kv => kv.Key)
                        .ToDictionary(g => g.Key, g => g.First().Value);

        selectionMesh.GenerateMesh(new Face[0]);
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }

    #endregion

    #region button

    void BuildBoundries()
    {
        Dictionary<Vector3Int, Voxel> boundryVoxels = new Dictionary<Vector3Int, Voxel>();
        List<Vector3Int> coords = new List<Vector3Int>();

        for (int x = 0; x < gridBounds[0]; x++)
        {
            for (int y = 0; y < gridBounds[1]; y++)
            {
                for (int z = 0; z < gridBounds[2]; z++)
                {
                    boundryVoxels.Add(new Vector3Int(x,y,z), new Voxel(new Vector3Int(x,y,z), float3.zero, boundryVoxels));
                }
            }
        }
        
        boundryMesh.GenerateMesh(boundryVoxels.Values.SelectMany(x => x.GetFaces(gridLevelMax)).ToArray());
        boundryVisMesh.GenerateMesh(boundryVoxels.Values.SelectMany(x => x.GetFaces(gridLevelMax, false)).ToArray());
    }

    public void ButtonSetPlayerMode()
    {
        playerController.gameObject.SetActive(true);
        builderController.gameObject.SetActive(false);
    }
    public void ButtonSetBuilderMode()
    {
        builderController.gameObject.SetActive(true);
        playerController.gameObject.SetActive(false);
    }

    public void ButtonInitialize()
    {
        ClearProcessingMesh();
        commandManager.ClearHistory();

        VoxelSize = (int)voxelSlider.value;
        bounds = new BoundsInt(Vector3Int.zero , new Vector3Int(gridBounds[0] * gridLevelMax / VoxelSize, gridBounds[1] * gridLevelMax / VoxelSize, gridBounds[2] * gridLevelMax / VoxelSize));
        BuildBoundries(); 

        List<Vector3Int> allVoxels = voxels.Keys.ToList();
        for (int i = 0; i < allVoxels.Count; i++)
        {
            if (!InBounds(allVoxels[i]))
            {
                voxels.Remove(allVoxels[i]);
            }
        }
        CreateMesh();

    }

    void InitializeFromSave()
    {
        ClearProcessingMesh();
        ClearMesh();
        BuildBoundries();
        commandManager.ClearHistory();
    }
    public void UpdateDropdown(int index)
    {
        gridBounds[index] = boundsDropdown[index].value + 1;
    }
    public void UpdateBrushSlider()
    {
        brushSize = (int)brushSlider.value;
        brushSizeText.text = ((int)brushSlider.value + 1).ToString();
    }
    public void UpdateVoxelSize()
    {
        voxelSizeText.text = ((int)voxelSlider.value).ToString();
    }
    public void ButtonClear()
    {
        ClearMesh();
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
    public void PerformDrawLine()
    {

        List<Vector3Int> coords = DrawLine(latestPlacementCoord, placementCoord);
        if (coords.Count > 0)
        {
            latestPlacementCoord = coords[coords.Count - 1];
        }
        commandManager.AddCommand(new CommandDrawLine(coords, color));

    }
    public void PerformRemoveLine()
    {
        List<Vector3Int> coords = DrawLine(latestSelectedCoord, selectedCoord);
        if (coords.Count > 0)
        {
            latestSelectedCoord = coords[coords.Count - 1];
        }
        commandManager.AddCommand(new CommandRemoveLine(coords, color));
    }

    public void PerformPullIn()
    {
        if (voxels.ContainsKey(selectedCoord))
        {
            commandManager.AddCommand(new CommandShrink(SelectInByNormal(voxels[selectedCoord], placementNormal), color));
        }
    }
    public void PerformRemoveVoxels()
    {
        if (!processingVoxels.ContainsKey(selectedCoord))
        {
            List<Vector3Int> brush = GetBrush(selectedCoord, -1);
            VoxelManager.instance.AddProcessingVoxels(brush);
            VoxelManager.instance.RemoveVoxels(brush);
        }
    }
    void TryRemoveVoxels(List<Vector3Int> coords)
    {
        if (!processingVoxels.ContainsKey(selectedCoord))
        {
            VoxelManager.instance.AddSubtractProcessingVoxels(coords);
            VoxelManager.instance.RemoveVoxels(coords);
        }
    }
    void TrySelectVoxels(List<Vector3Int> coords)
    {
        if (!processingVoxels.ContainsKey(selectedCoord))
        {
            VoxelManager.instance.AddSelectionVoxels(coords);
            VoxelManager.instance.RemoveVoxels(coords);
        }
    }
    List<Vector3Int> GetBrush(Vector3Int coord, int sign = 1)
    {
        List<Vector3Int> newCoords = new List<Vector3Int>();
        for (int i = 0; i < Brushes[brushSize].Count(); i++)
        {
            Vector3Int aCoord = Brushes[brushSize][i] + coord + (placementNormal * brushSize / 2) * sign;
            newCoords.Add(aCoord);
        }
        return newCoords;
    }
    List<Vector3Int> GetBrush(List<Vector3Int> coord, int sign = 1)
    {
        HashSet<Vector3Int> newCoords = new HashSet<Vector3Int>();
        for (int c = 0; c < coord.Count; c++)
        {
            for (int i = 0; i < Brushes[brushSize].Count(); i++)
            {
                Vector3Int aCoord = Brushes[brushSize][i] + coord[c] + (placementNormal * brushSize / 2) * sign;
                newCoords.Add(aCoord);
            }
        }
        return newCoords.ToList();
    }
    public void PerformAddingVoxels()
    {
        latestSelectedCoord = selectedCoord;
        if (latestAddedCoord != selectedCoord)
        {
            latestAddedCoord = placementCoord;
            latestPlacementCoord = placementCoord;
            AddProcessingVoxels(GetBrush(placementCoord), color);
        }
    }
    void TryAddVoxels(List<Vector3Int> coords)
    {
        latestSelectedCoord = selectedCoord;
        if (latestAddedCoord != selectedCoord)
        {
            latestAddedCoord = placementCoord;
            latestPlacementCoord = placementCoord;

            VoxelManager.instance.AddProcessingVoxels(coords, color);

        }
    }
    public void PerformExtrude()
    {
        if (voxels.ContainsKey(selectedCoord))
        {
            commandManager.AddCommand(new CommandGrow(SelectOutByNormal(voxels[selectedCoord], placementNormal), color));
        }
    }
    public void PerformPaintColor()
    {
        Voxel voxel;
        if (voxels.TryGetValue(selectedCoord, out voxel) && selectedCoord != latestPaintCoord)
        {
            latestPaintCoord = voxel.coord;
            SetVoxelsColor(GetBrush(voxel.coord), color, false);
        }
    }
    public void PerformFillColor()
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
        //commandManager.AddCommand(new CommandMove(selectedVoxels, offset), false);
    }
    public void TransformVoxels(Vector3Int offset)
    {
        //MoveVoxels(selectedVoxels.ToList(), offset);
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
    public void SetVoxelsColor(List<Vector3Int> coords, float3 aColor, bool reset = true)
    {
        Voxel voxel;
        for (int i = 0; i < coords.Count; i++)
        {
            if (voxels.TryGetValue(coords[i], out voxel) && !paintedVoxels.ContainsKey(voxel.coord))
            {
                paintedVoxels.Add(voxel.coord, voxel.color);
                voxel.color = aColor;
            }
        }
        if (reset)
        {
            paintedVoxels.Clear();
        }
        CreateMesh();
    }
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
        if (!processingVoxels.ContainsKey(coord) && !voxels.ContainsKey(coord) && InBounds(coord))
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
            Voxel voxel = new Voxel(coord, voxels[coord].color, processingVoxels);
            processingVoxels.Add(coord, voxel);
            return voxel;
        }
        return null;
    }
    Voxel AddSelectionVoxel(Vector3Int coord)
    {
        if (!selectedVoxels.ContainsKey(coord) && voxels.ContainsKey(coord) && InBounds(coord))
        {
            Voxel voxel = new Voxel(coord, voxels[coord].color, selectedVoxels);
            selectedVoxels.Add(coord, voxel);
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
        }
        if (ct > 0)
            CreateMesh();
    }
    public void AddVoxels(List<Vector3Int> addVoxels, List<float3> colors)
    {
        int ct = 0;
        for (int i = 0; i < addVoxels.Count; i++)
        {
            if (AddVoxel(addVoxels[i], colors[i]) != null)
            {
                ct++;
            }
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
    public void AddSubtractProcessingVoxels(List<Vector3Int> addVoxels)
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
            CreateSubtractProcessingMesh();
    }
    public void AddSelectionVoxels(List<Vector3Int> addVoxels)
    {
        int ct = 0;
        for (int i = 0; i < addVoxels.Count; i++)
        {
            if (AddSelectionVoxel(addVoxels[i]) != null)
            {
                ct++;
            }
        }
        if (ct > 0)
            CreateSelectionMesh();
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
    public void RemoveVoxels(List<Vector3Int> removeVoxels)
    {
        for (int i = 0; i < removeVoxels.Count; i++)
        {
            RemoveVoxel(removeVoxels[i]);
        }
        CreateMesh();
    }
    List<Vector3Int> DrawLineDiagonal(Vector3Int c0, Vector3Int c1)
    {
        // alternate draw line
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
        return points;
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
    // List<Vector3Int> DrawLineInvert(Vector3Int c1, Vector3Int c2)
    // {
    //     int n = DiagonalDistance(c1, c2);
    //     List<Vector3Int> aVoxels = new List<Vector3Int>();
    //     for (int i = 0; i <= n; i++)
    //     {
    //         float t = n == 0 ? 0f : (float)i / n;
    //         aVoxels.Add(RoundCoord(Vector3.Lerp(c1, c2, t)));
    //     }
    //     return aVoxels;
    // }
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
                        brush.Add(new Vector3Int(x - n / 2, y - n / 2, z - n / 2));
                    }
                }
            }
            Brushes.Add(brush.ToList());
            brush.Clear();
        }
    }

    List<Vector3Int> SelectOutByNormal(Voxel voxel, Vector3Int direction)
    {
        HashSet<Vector3Int> extrudeVoxels = new HashSet<Vector3Int>();
        List<Voxel> searchVoxels = new List<Voxel>();
        extrudeVoxels.Add(voxel.coord + direction);
        searchVoxels.Add(voxel);
        Vector3Int[] directions = DirectionStruct.AvailableDirections(direction);
        while (searchVoxels.Count > 0)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                if (GetVoxelByCoord(searchVoxels[0].coord + directions[i]) is Voxel aVoxel && !extrudeVoxels.Contains(aVoxel.coord + direction))
                {
                    if (GetVoxelByCoord(aVoxel.coord + placementNormal) == null)
                    {
                        extrudeVoxels.Add(aVoxel.coord + direction);
                        searchVoxels.Add(aVoxel);
                    }
                }
            }
            searchVoxels.RemoveAt(0);
        }
        return extrudeVoxels.ToList();
    }

    List<Vector3Int>  SelectInByNormal(Voxel voxel, Vector3Int direction)
    {
        HashSet<Vector3Int> removeVoxels = new HashSet<Vector3Int>();
        List<Voxel> searchVoxels = new List<Voxel>();
        removeVoxels.Add(voxel.coord);
        searchVoxels.Add(voxel);
        Vector3Int[] directions = DirectionStruct.AvailableDirections(direction);
        while (searchVoxels.Count > 0)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                if (GetVoxelByCoord(searchVoxels[0].coord + directions[i]) is Voxel aVoxel && !removeVoxels.Contains(aVoxel.coord))
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
            for (int i = 0; i < DirectionStruct.Normals.Length; i++)
            {
                if (GetVoxelByCoord(searchVoxels[0].coord + DirectionStruct.Normals[i]) is Voxel aVoxel && !foundVoxels.Contains(aVoxel.coord))
                {
                    foundVoxels.Add(aVoxel.coord);
                    searchVoxels.Add(aVoxel);
                }
            }
            searchVoxels.RemoveAt(0);
        }
        return foundVoxels.ToList();
    }

    public void SampleColor()
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
    // int DiagonalDistance(Vector3Int c1, Vector3Int c2) => Mathf.Max(Mathf.Abs(c2.x - c1.x), Mathf.Abs(c2.y - c1.y), Mathf.Abs(c2.z - c1.z));
    // Vector3Int RoundCoord(Vector3 coord)
    // {
    //     return new Vector3Int(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y), Mathf.RoundToInt(coord.z));
    // }
    #endregion

    #region coords
    void SetCoordMouseHover()
    {
        Plane plane = new Plane(Vector3.up, GridLevel);
        //float distance;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        RaycastHit hit;
        

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, aLm))
        {
            Vector3Int normal = Vector3Int.FloorToInt(hit.normal);
            Vector3Int offsetCoord = new Vector3Int(hit.normal == Vector3.right ? 1 : 0, hit.normal == Vector3.up ? 1 : 0, hit.normal == Vector3.forward ? 1 : 0);
            Vector3Int selCoord = Vector3Int.FloorToInt(hit.point / VoxelSize) - offsetCoord;

            canPlace = true;

            if (hit.normal == Vector3.up || hit.normal == Vector3.down)
            {
                selCoord.y = Mathf.RoundToInt(hit.point.y / VoxelSize) - offsetCoord.y;
            }
            else if (hit.normal == Vector3.left || hit.normal == Vector3.right)
            {
                selCoord.x = Mathf.RoundToInt(hit.point.x / VoxelSize) - offsetCoord.x;
            }
            else
            {
                selCoord.z = Mathf.RoundToInt(hit.point.z / VoxelSize) - offsetCoord.z;
            }

            if (selectedCoord != selCoord || placementNormal != normal)
            {
                placementNormal = normal;
                selectedCoord = selCoord; 
                placementCoord = (selCoord + normal);
                if (!isSelecting)
                {
                    cursorMesh.GenerateMesh(new Face[1] {new Face(DirectionStruct.INormals[placementNormal], new float3(selectedCoord.x, selectedCoord.y, selectedCoord.z), float3.zero)});
                }
            }
        }
        else if (canPlace)
        {
            canPlace = false;
            cursorMesh.GenerateMesh(new Face[0]);
        }

    }
    #endregion

    #region save

    public void PopulateSaveData(SaveData a_SaveData)
    {
        Voxel[] allVoxels = voxels.Values.ToArray();
        VoxelData[] voxelDatas = new VoxelData[allVoxels.Length];
        a_SaveData.voxelSize = _voxelSize;
        a_SaveData.bounds = bounds;
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
        bounds = a_SaveData.bounds;
        VoxelSize = a_SaveData.voxelSize;
        InitializeFromSave();
        for (int i = 0; i < a_SaveData.voxelDatas.Length; i++)
        {
            newVoxels.Add(a_SaveData.voxelDatas[i].coord, AddVoxel(a_SaveData.voxelDatas[i].coord, a_SaveData.voxelDatas[i].color));
        }
        voxels = newVoxels;
        proceduralMesh.GenerateMesh(voxels.Values.SelectMany(x => x.GetFaces()).ToArray());
    }
    #endregion
}