using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionController : MonoBehaviour
{
    public Vector3Int selectedCoord, startCoord, previousCoord, deltaCoord, placementDirection, placementNormal;
    Vector3 startMouse;

    public int offsetSensitivity;
    List<Vector3Int> coords = new List<Vector3Int>();
    bool moveActive;
    public LayerMask lm;
    void Awake()
    {
        gameObject.SetActive(false);
    }
    public void StartMove()
    {
        //moveActive = true;

        //float d = Vector3.Distance(transform.position, Camera.main.transform.position).Remap(1, 100, .5f, 10);
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, lm))
        {
            startMouse = hit.point;
        }

        startCoord = Vector3Int.FloorToInt(Mouse.current.position.ReadValue() / (offsetSensitivity * VoxelManager.instance.VoxelSize));
        previousCoord = startCoord;
        selectedCoord = startCoord;
        if (VoxelManager.instance.placementNormal == Vector3Int.down || VoxelManager.instance.placementNormal == Vector3Int.up)
        {
            placementDirection = Vector3Int.up;
        }
        else if (VoxelManager.instance.placementNormal == Vector3Int.left || VoxelManager.instance.placementNormal == Vector3Int.right)
        {
            placementDirection = Vector3Int.right;
        }
        else
        {
            placementDirection = Vector3Int.forward;
        }
    }

    Vector3Int GetStartDirection(Vector3 aDirection)
    {
        Vector3 tempVector = new Vector3(Mathf.Abs(aDirection.x), Mathf.Abs(aDirection.y), Mathf.Abs(aDirection.z));
        Vector3Int direction = Vector3Int.zero;

        int signX = aDirection.x < 0 ? 1 : -1;
        int signY = aDirection.y < 0 ? 1 : -1;
        int signZ = aDirection.z < 0 ? 1 : -1;
        
        if (tempVector.x > tempVector.y && tempVector.x > tempVector.z)
        {
            //direction = Vector3Int.right;
            direction = new Vector3Int(signX, 0, 0);
        }
        else if (tempVector.y > tempVector.x && tempVector.y > tempVector.z)
        {
            //direction = Vector3Int.up;
            direction = new Vector3Int(0, signY, 0);
        }
        else
        {
           // direction = Vector3Int.forward;
           direction = new Vector3Int(0, 0, signZ);
        }

        return direction;
    }

    public void PerformMove()
    {
        //float d = Vector3.Distance(transform.position, Camera.main.transform.position).Remap(1, 100, .5f, 10);
        selectedCoord = Vector3Int.FloorToInt(Mouse.current.position.ReadValue() / (offsetSensitivity * VoxelManager.instance.VoxelSize));
        if (previousCoord != selectedCoord)
        {
            previousCoord = selectedCoord;
            deltaCoord = selectedCoord - startCoord;

            if (!moveActive)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, lm))
                {
                    placementNormal = GetStartDirection(startMouse - hit.point);
                }

                // if (placementDirection == Vector3Int.up && placementNormal == Vector3Int.up)
                // {
                //     placementNormal = Vector3Int.forward;
                // }
                // else if (placementDirection == Vector3Int.right && placementNormal == Vector3Int.right)
                // {
                //     placementNormal = Vector3.right; 
                // }
                // else if (placementDirection == Vector3Int.forward && placementNormal == Vector3Int.forward)
                // {
                //     placementNormal = Vector3Int.up;
                // }

                Debug.Log(placementNormal);
                Debug.Log(deltaCoord);
                if (placementNormal == Vector3Int.left && deltaCoord.x < 0)
                {
                    Debug.Log("SWAP");
                    placementNormal = Vector3Int.right;
                }
                else if (placementNormal == Vector3Int.back && deltaCoord.x < 0)
                {
                    Debug.Log("SWAP");
                    placementNormal = Vector3Int.forward;
                }

                coords = VoxelManager.instance.voxels.Keys.ToList();
                moveActive = true;
            }

            // if (placementNormal == Vector3Int.left)
            // {
            //     deltaCoord *= -1;
            // }
            // else if (placementDirection == Vector3Int.left)
            // {
            //     deltaCoord *= -1;
            // }
            //transform.position = new Vector3(deltaCoord.x, 0, 0) * VoxelManager.instance.VoxelSize;

            var voxels = VoxelManager.instance.voxels.Values.ToList();
            
            //Debug.Log(Mathf.Abs(deltaCoord.x));
            for (int i = 0; i < voxels.Count; i++)
            {
                voxels[i].coord = coords[i] + placementNormal * (deltaCoord.x + deltaCoord.y);
            }

            VoxelManager.instance.CreateMesh();
        }
        
    }

    public void StopMove()
    {
        moveActive = false;
        var colors = VoxelManager.instance.voxels.Values.Select(x => x.color).ToList();
        for (int i = 0; i < coords.Count; i++)
        {
            coords[i] += placementNormal * (deltaCoord.x + deltaCoord.y);
        }
        VoxelManager.instance.ButtonClear();
        if (coords.Count > 0 && coords.Count == colors.Count)
        {
            VoxelManager.instance.TryAddVoxels(coords, colors);
        }

        //VoxelManager.instance.aLm = VoxelManager.instance.lmvg;
    }
    
    void Update()
    {        
        // if (moveActive)
        // {
        //     RaycastHit hit;
        //     Ray ray = Camera.main.ScreenPointToRay();
        //     if (Physics.Raycast(ray, out hit, Mathf.Infinity, lm))
        //     {
        //         selectedCoord = 
        //         Debug.Log(hit.point);
        //     }
        // }   
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