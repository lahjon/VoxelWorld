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
            if (_mouseDown && PositionHandleCollider != null)
            {
                PositionHandleCollider.Moving = true;
            }
            else if (!_mouseDown && PositionHandleCollider != null)
            {
                PositionHandleCollider.Moving = false;
            }
        }
    }
    [SerializeField] Vector3 mousePosition, hitPosition;

    public Vector3 Position
    {
        get => positionHandleMesh.localPosition;
    }
    void FixedUpdate()
    {
        if (VoxelManager.instance.selectMode)
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
        if (Input.GetMouseButton(0))
        {
            //Debug.Log("Mouse Down");
        }
        if (Input.GetMouseButtonUp(0))
        {
            MouseDown = false;
        }
        //Debug.Log(mousePosition);
    }
}