using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridPoints
{
    public GridManager gridManager;
    public int gridUID;
    public Vector3 position;
    public enum PointState { walkable, blocked }
    public PointState state;
    public int identity;
    
    public GameObject tile;
    public bool hasTile;

    public GridPoints(int gridUID, int col, int row, float sprWidth, float sprHeight, GridManager gridManager, int identity)
    {
        this.gridManager = gridManager;
        this.gridUID = gridUID;
        position = FindWorldPosition(col, row, sprWidth, sprHeight);
        this.identity = identity;
        
    }

    private Vector3 FindWorldPosition(int col, int row, float sprWidth, float sprHeight)
    {
        int xPos = gridUID % col;
        int zPos = (int)Mathf.Floor(gridUID / col);
        return position = new Vector3(xPos * sprWidth, 0.0f, zPos * -sprHeight);
    }
}
