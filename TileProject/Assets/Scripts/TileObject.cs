using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    public int UID;
    public Vector3 worldPos;

    private void Start()
    {
        worldPos = transform.position;
    }
}
