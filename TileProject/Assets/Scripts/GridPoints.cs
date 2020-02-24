using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridPoints
{
    private GridManager gridManager;
    public int UID;
    public int identity;

    public bool containsTile;
    public GameObject tile;
    public Vector3 position;

    // From scriptable tile
    public enum TileState { walkable, blocked }
    public TileState tileState;
    public float moveCost;

    public GridPoints(GridManager gridManager, int gridUID, int col, int row, float sprWidth, float sprHeight)
    {
        this.gridManager = gridManager;
        this.UID = gridUID;
        position = InferWorldPosition(col, row, sprWidth, sprHeight);
    }

    /// <summary> Given a column, a row and the size of the grid space, infer a world position coordinate</summary>
    private Vector3 InferWorldPosition(int col, int row, float sprWidth, float sprHeight)
    {
        int xPos = UID % col;
        int zPos = (int)Mathf.Floor(UID / col);
        return position = new Vector3(xPos * sprWidth, 0.0f, zPos * -sprHeight);
    }

    /// <summary> Get scriptable tile script and load up all values to grid point object</summary>
    public void SetupScriptableTileParams()
    {
        ScriptableTile scriptableTile = gridManager.ReferenceTiles[identity].GetComponent<ScriptableTile>();
        tileState = (GridPoints.TileState)scriptableTile.tileState;
        moveCost = scriptableTile.moveCost;
    }

    
}
