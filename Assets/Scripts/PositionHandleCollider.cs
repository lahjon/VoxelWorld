using UnityEngine;

[ExecuteInEditMode]
public class PositionHandleCollider : MonoBehaviour
{
    public PositionHandle positionHandle;
    Vector3 startPos, lastPos, delta;
    public System.Action move;
    [SerializeField] HandlePositions handlePositions;
    Color startColor;
    bool _moving;
    bool tracking;
    bool _selected;
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            if (_selected)
            {
                transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
    }
    [SerializeField] Material material;
    public bool Moving
    {
        get => _moving;
        set
        {
            _moving = value;
            if (!_moving) 
            {
                tracking = false;
                material.SetColor("_Color", startColor);
            }
            else
            {
                material.SetColor("_Color", Color.white);
                Selected = false;
            }
        }
    }
    void Awake()
    {
        material = GetComponent<MeshRenderer>().sharedMaterial;
        startColor = material.GetColor("_Color");
        if (positionHandle == null)
        {
            transform.parent.parent.GetComponent<PositionHandle>();
        }

        switch (handlePositions)
        {
            case HandlePositions.BlueArrow:
                move = MoveBA;
                break;
            case HandlePositions.GreenArrow:
                move = MoveGA;
                break;
            case HandlePositions.RedArrow:
                move = MoveRA;
                break;
            case HandlePositions.BluePlane:
                move = MoveBP;
                break;
            case HandlePositions.GreenPlane:
                move = MoveGP;
                break;
            case HandlePositions.RedPlane:
                move = MoveRP;
                break;
            default:
                break;
        }
    }
    void Update()
    {
        if (Moving)
        {
            move.Invoke();
        }
    }
    void MoveBA()
    {
        Plane plane = new Plane(Vector3.up, 0);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);

            if (!tracking)
            {
                lastPos = pos;
                tracking = true;
            }
            delta = pos - lastPos;
            positionHandle.transform.localPosition += new Vector3(0, 0, delta.z);
            lastPos = pos;
        }
    }

    void MoveGA()
    {
        Plane plane = new Plane(Vector3.forward, 0);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);

            if (!tracking)
            {
                lastPos = pos;
                tracking = true;
            }
            delta = pos - lastPos;
            positionHandle.transform.localPosition += new Vector3(0, delta.y, 0);
            lastPos = pos;
        }
    }

    void MoveRA()
    {
        Plane plane = new Plane(Vector3.up, 0);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);

            if (!tracking)
            {
                lastPos = pos;
                tracking = true;
            }
            delta = pos - lastPos;
            positionHandle.transform.localPosition += new Vector3(delta.x, 0, 0);
            lastPos = pos;
        }
    }

    void MoveBP()
    {
        Plane plane = new Plane(Vector3.forward, 0);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);

            if (!tracking)
            {
                lastPos = pos;
                tracking = true;
            }
            delta = pos - lastPos;
            positionHandle.transform.localPosition += new Vector3(delta.x, delta.y, 0);
            lastPos = pos;
        }
    }

    void MoveGP()
    {
        Plane plane = new Plane(Vector3.up, 0);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);

            if (!tracking)
            {
                lastPos = pos;
                tracking = true;
            }
            delta = pos - lastPos;
            positionHandle.transform.localPosition += new Vector3(delta.x, 0, delta.z);
            lastPos = pos;
        }
    }

    void MoveRP()
    {
        Plane plane = new Plane(Vector3.left, 0);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);

            if (!tracking)
            {
                lastPos = pos;
                tracking = true;
            }
            delta = pos - lastPos;
            positionHandle.transform.localPosition += new Vector3(0, delta.y, delta.z);
            lastPos = pos;
        }
    }
}


public enum HandlePositions
{
    BlueArrow,
    GreenArrow,
    RedArrow,
    BluePlane,
    GreenPlane,
    RedPlane

}