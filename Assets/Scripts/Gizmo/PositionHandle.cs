using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PositionHandle : MonoBehaviour
{
    PositionHandleCollider _positionHandleCollider;
    PositionHandleCollider PositionHandleCollider
    {
        get => _positionHandleCollider;
        set
        {
            PositionHandleCollider previousHandle = _positionHandleCollider;
            _positionHandleCollider = value;
            if (_positionHandleCollider != null)
            {
                _positionHandleCollider.Selected = true;
            }
            if (previousHandle != null)
            {
                previousHandle.Selected = false;
            }
        }
    }
    [SerializeField] Transform positionHandleMesh;
    [SerializeField] LayerMask lm;
    bool _mouseDown;
    public bool MouseDown
    {
        get => _mouseDown;
        set
        {
            _mouseDown = value;
            if (_mouseDown)
            {
                if (PositionHandleCollider != null)
                {
                    PositionHandleCollider.Moving = true;
                    VoxelManager.instance.selectedVoxels = VoxelManager.instance.voxels.Keys.ToList();
                }
            }
            else
            {
                if (PositionHandleCollider != null)
                {
                    PositionHandleCollider.Moving = false;
                    if (_offset != Vector3.zero)
                    {
                        VoxelManager.instance.AddMoveCommand(Offset);
                    }
                }

                VoxelManager.instance.selectedVoxels.Clear();
                Position = Vector3.zero;
                Offset = Vector3Int.zero;
            }
        }
    }
    Vector3Int _offset;
    public Vector3Int Offset
    {
        get => _offset;
        set
        {
            if (_offset != value && MouseDown)
            {
                VoxelManager.instance.TransformVoxels(-(_offset - value));
            }
            _offset = value;
        }
    }
    public Vector3 Position
    {
        get => positionHandleMesh.localPosition;
        set
        {
            positionHandleMesh.transform.localPosition = value;
        }
    }
    void OnEnable()
    {
        Position = Vector3.zero;
    }
    void FixedUpdate()
    {
        if (VoxelManager.instance.SelectMode)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!MouseDown)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, lm))
                {
                    PositionHandleCollider aPositionHandleCollider = hit.collider.GetComponent<PositionHandleCollider>();
                    if (PositionHandleCollider != aPositionHandleCollider)
                    {
                        PositionHandleCollider = aPositionHandleCollider;
                    }
                }
                else if (PositionHandleCollider != null)
                {
                    PositionHandleCollider = null;
                }
            }
            positionHandleMesh.transform.localScale = Vector3.Distance(transform.position, Camera.main.transform.position).Remap(0, 100, .5f, 10) * Vector3.one;  
            Offset = new Vector3Int(Mathf.RoundToInt(Position.x), Mathf.RoundToInt(Position.y), Mathf.RoundToInt(Position.z)); 
        }    
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PositionHandleCollider != null)
            {
                MouseDown = true;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            MouseDown = false;
        }
    }
}