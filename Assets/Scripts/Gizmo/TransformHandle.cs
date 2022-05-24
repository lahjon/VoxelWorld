using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransformHandle : MonoBehaviour
{
    public LayerMask lm;

    void Update()
    {
        if (VoxelManager.instance.builderController.isLMBPressed)
        {
            // RaycastHit[] hit = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, Mathf.Infinity, lm);
            // for (int i = 0; i < hit.Length; i++)
            // {
            //     Debug.Log(hit[i]);
            // }

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, lm))
            {
                transform.position = hit.point;
            }
        }
    }   
}
