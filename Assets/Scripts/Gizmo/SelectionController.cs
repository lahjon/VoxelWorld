using System.Linq;
using UnityEngine;
public class SelectionController : MonoBehaviour
{
    public Vector3Int selectedCoord, startCoord, deltaCoord;
    Plane plane, planeUp, PlaneForward, PlaneRight;
    SelectionPlane _selectionPlane;
    public SelectionPlane SelectionPlane
    {
        get => _selectionPlane;
        set
        {
            _selectionPlane = value;
            if (_selectionPlane == SelectionPlane.Up)
            {
                plane = planeUp;
            }
            else if (_selectionPlane == SelectionPlane.Forward)
            {
                plane = PlaneForward;
            }
            else
            {
                plane = PlaneRight;
            }
        }
    }
    void Awake()
    {
        planeUp = new Plane(Vector3.up, 0);
        PlaneForward = new Plane(Vector3.forward, 0);
        PlaneRight = new Plane(Vector3.right, 0);
        plane = planeUp;
        gameObject.SetActive(false);
    }
    
    void Update()
    {        
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            Vector3 pos = ray.GetPoint(distance);
            selectedCoord = Vector3Int.FloorToInt(pos / VoxelManager.instance.VoxelSize);
        }
        if (Input.GetMouseButtonDown(0))
        {
            startCoord = selectedCoord; 
            deltaCoord = Vector3Int.zero;
            plane.Translate(VoxelManager.instance.SelectCoordToWorldSpace());
            if (VoxelManager.instance.placementNormal == Vector3Int.forward || VoxelManager.instance.placementNormal == Vector3Int.back)
            {
                SelectionPlane = SelectionPlane.Forward;
            }
            else if (VoxelManager.instance.placementNormal == Vector3Int.up || VoxelManager.instance.placementNormal == Vector3Int.down)
            {
                SelectionPlane = SelectionPlane.Up;
            }
            else
            {
                SelectionPlane = SelectionPlane.Right;
            }
        }
        
        if (Input.GetMouseButton(0))
        {
            if (startCoord != selectedCoord)
            {
                // if (SelectionPlane == SelectionPlane.Up)
                // {
                //     deltaCoord = new Vector3Int(startCoord.x - selectedCoord.x, 0, startCoord.z - selectedCoord.z);
                // }
                // else if (SelectionPlane == SelectionPlane.Forward)
                // {
                //     deltaCoord = new Vector3Int(startCoord.x - selectedCoord.x, startCoord.y - selectedCoord.y, 0);
                // }
                // else
                // {
                //     deltaCoord = new Vector3Int(0, startCoord.y - selectedCoord.y, startCoord.z - selectedCoord.z);
                // }

                int signX = 0;
                int signY = 0;
                int signZ = 0;
                if (startCoord.x - selectedCoord.x > 0)
                    signX = 1;
                else if (startCoord.x - selectedCoord.x < 0)
                    signX = -1;

                if (startCoord.y - selectedCoord.y > 0)
                    signY = 1;
                else if (startCoord.y - selectedCoord.y < 0)
                    signY = -1;

                if (startCoord.z - selectedCoord.z > 0)
                    signZ = 1;
                else if (startCoord.z - selectedCoord.z < 0)
                    signZ = -1;

                if (SelectionPlane == SelectionPlane.Up)
                {
                    deltaCoord = new Vector3Int(signX, 0, signZ);
                }
                else if (SelectionPlane == SelectionPlane.Forward)
                {
                    deltaCoord = new Vector3Int(signX, signY, 0);
                }
                else
                {
                    deltaCoord = new Vector3Int(0, signY, signZ);
                }

                var t = VoxelManager.instance.voxels.Values.ToList();
                foreach (var item in t)
                {
                    item.coord = item.coord - deltaCoord;
                }
                VoxelManager.instance.CreateMesh();

                //VoxelManager.instance.deltaCoord = new Vector3Int(startCoord.x - selectedCoord.x, startCoord.y - selectedCoord.y, startCoord.z - selectedCoord.z);
                startCoord = selectedCoord;
            }
        }
            
    }
}

public enum SelectionPlane
{
    Up, 
    Forward, 
    Right
}

// int signX = 0;
// int signY = 0;
// int signZ = 0;
// if (startCoord.x - selectedCoord.x > 0)
//     signX = 1;
// else if (startCoord.x - selectedCoord.x < 0)
//     signX = -1;

// if (startCoord.y - selectedCoord.y > 0)
//     signY = 1;
// else if (startCoord.y - selectedCoord.y < 0)
//     signY = -1;

// if (startCoord.z - selectedCoord.z > 0)
//     signZ = 1;
// else if (startCoord.z - selectedCoord.z < 0)
//     signZ = -1;

// if (SelectionPlane == SelectionPlane.Up)
// {
//     deltaCoord = new Vector3Int(signX, 0, signZ);
// }
// else if (SelectionPlane == SelectionPlane.Forward)
// {
//     deltaCoord = new Vector3Int(signX, signY, 0);
// }
// else
// {
//     deltaCoord = new Vector3Int(0, signY, signZ);
// }