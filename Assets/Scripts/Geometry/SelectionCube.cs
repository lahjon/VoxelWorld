using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;

public class SelectionCube : MonoBehaviour
{
    public ProceduralMesh mesh;
    public Vector3Int coord;
    public Material material;
    public bool occupied;
    public void MoveToCoord(Vector3Int newCoord, bool occupied)
    {
        coord = newCoord;
        this.occupied = occupied;
        if (occupied)
            gameObject.SetActive(false);
        else
        {
            gameObject.SetActive(true);
            transform.position = (Vector3)newCoord + new Vector3(.5f, .5f, .5f);
        }
    }

}