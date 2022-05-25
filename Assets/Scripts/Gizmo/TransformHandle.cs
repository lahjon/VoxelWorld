using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransformHandle : MonoBehaviour
{
    // public LayerMask lm;

    // void Update()
    // {
    //     if (VoxelManager.instance.builderController.isLMBPressed)
    //     {
    //         RaycastHit hit;
    //         Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    //         if (Physics.Raycast(ray, out hit, Mathf.Infinity, lm))
    //         {
    //             //transform.position = hit.point;
    //             Vector3Int selCoord = Vector3Int.FloorToInt(hit.point / VoxelManager.instance.VoxelSize);
    //             transform.position = selCoord;
    //         }
    //     }
    // }   
}
