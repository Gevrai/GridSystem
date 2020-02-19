using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridPoints
{
    public GridManager gridManager;
    public int gridUID;
    public int identity; // TEMPORARY, AUTOMATICALLY GEN VARIED TILE

    public bool hasTile;
    public GameObject tile;
    public Vector3 position;

    // From scriptable tile
    public enum TileState { walkable, blocked }
    public TileState tileState;
    public float moveCost;

    public GridPoints(GridManager gridManager, int gridUID, int identity, int col, int row, float sprWidth, float sprHeight)
    {
        this.gridManager = gridManager;
        this.gridUID = gridUID;
        this.identity = identity;
        position = InferWorldPosition(col, row, sprWidth, sprHeight);
    }


    /// <summary> Given a column, a row and the size of the grid space, infer a world position coordinate</summary>
    private Vector3 InferWorldPosition(int col, int row, float sprWidth, float sprHeight)
    {
        int xPos = gridUID % col;
        int zPos = (int)Mathf.Floor(gridUID / col);
        return position = new Vector3(xPos * sprWidth, 0.0f, zPos * -sprHeight);
    }

    /// <summary> Get scriptable tile script and load up all values to grid point object</summary>
    private void SetupScriptableTileParams()
    {
        ScriptableTile scriptableTile = gridManager.referenceTile[identity].GetComponent<ScriptableTile>();
        tileState = (GridPoints.TileState)scriptableTile.tileState;
        moveCost = scriptableTile.moveCost;
    }
}
