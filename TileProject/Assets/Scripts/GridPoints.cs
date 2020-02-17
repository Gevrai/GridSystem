using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridPoints
{
    public int gridUID;
    public GridManager gridManager;
    public enum PointState { walkable, blocked }
    public PointState state;
    public Vector3 position;
    public bool hasTile;
    public GameObject tile;

    public GridPoints(int gridUID, int col, int row, float sprWidth, float sprHeight, GridManager gridManager)
    {
        this.gridUID = gridUID;
        this.position = FindWorldPosition(col, row, sprWidth, sprHeight);
        this.gridManager = gridManager;
    }

    private Vector3 FindWorldPosition(int col, int row, float sprWidth, float sprHeight)
    {
        int xPos = gridUID % col;
        int zPos = (int)Mathf.Floor(gridUID / col);
        return position = new Vector3(xPos * sprWidth, 0.0f, zPos * -sprHeight);
    }
}
