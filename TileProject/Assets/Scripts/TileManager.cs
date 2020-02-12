using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*todo :
 * Integrate scriptable object to tile generation
 * Preload all tile resources for tile generation and clean all resources afterward
 * Add character controller and wasd movements on grid
 * Add fire emblem mouvements with ranges.
 * Terrain editor exporting to json for save files.
 * Add ennemies
 * Add pathfinding AI
 * */

public class TileManager : MonoBehaviour
{
    public int col;
    public int row;
    private float unitWidth;
    private float unitHeight;

    public GridPoints[,] grid;
    public bool drawGrid;
    public bool genTiles;
    private GameObject referenceTile;

    public GameObject topDownCam;

    public List<ScriptableTile> tiles;

    public class GridPoints
    {
        public int UID;
        public enum PointState { walkable, blocked }
        public PointState state;
        public Vector3 worldPos;
        public GameObject tile;

        public GridPoints(int UID, Vector3 worldPos)
        {
            this.UID = UID;
            this.worldPos = worldPos;
        }
    }

    private void Start()
    {
        GenReference();
        GenGrid();
        if(genTiles) GenTiles();
        SetCam();

        // Kill ref
        Destroy(referenceTile);
    }

    public void GenReference()
    {
        referenceTile = (GameObject)Instantiate(Resources.Load("Tile32White"));
        GetTileSize();
    }

    public void GenGrid()
    {
        grid = new GridPoints[col, row];
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                grid[i, j] = new GridPoints(i * col + j + 1, new Vector3(i * unitWidth, 0, row * unitHeight));
            }
        }

        if (drawGrid)
        {
            for (int i = 0; i < col + 1; i++)
            {
                DrawGridLine(new Vector3(i * unitWidth, 0, 0), new Vector3(i * unitWidth, 0, row * -unitHeight), Color.black, 0.020f);
            }
            for (int i = 0; i < row + 1; i++)
            {
                DrawGridLine(new Vector3(0, 0, i * -unitHeight), new Vector3(col * unitWidth, 0, i * -unitHeight), Color.black, 0.020f);
            }
        }
    }

    public void GenTiles()
    {
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                grid[i, j].tile = (GameObject)Instantiate(referenceTile, new Vector3(i * unitWidth, 0.0f, j * -unitHeight), referenceTile.transform.rotation, transform);
                grid[i, j].tile.GetComponentInChildren<TileObject>().UID = i * 10 + j;

                int rnd = Random.Range(0, 2);
                if(rnd == 0) 
                {
                    grid[i, j].tile.GetComponentInChildren<SpriteRenderer>().material.SetColor("_Color", Color.green);
                    grid[i, j].state = GridPoints.PointState.walkable;
                }
                else 
                {
                    grid[i, j].tile.GetComponentInChildren<SpriteRenderer>().material.SetColor("_Color", Color.gray);
                    grid[i, j].state = GridPoints.PointState.blocked;
                }
            }
        }
    }

    private void GetTileSize()
    {
        SpriteRenderer sprRend = referenceTile.GetComponent<SpriteRenderer>();
        unitWidth = sprRend.sprite.rect.width / sprRend.sprite.pixelsPerUnit;
        unitHeight = sprRend.sprite.rect.height / sprRend.sprite.pixelsPerUnit;
    }

    private void SetCam()
    {
        // set camera
        topDownCam.GetComponent<Transform>().position = new Vector3((col / 2.0f * unitWidth), 5.0f, (row / 2.0f * -unitHeight));
        topDownCam.GetComponent<Camera>().orthographicSize = col / 4.0f;
    }

    private void DrawGridLine(Vector3 start, Vector3 end, Color color, float width)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/UnlitBlackLine"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    public void FindNeighbours()
    {
        Vector3 temp = this.gameObject.GetComponent<MouseControls>().selectedTile;
        int i = (int)Mathf.Abs(temp.x / unitWidth);
        int j = (int)Mathf.Abs(temp.z / unitHeight);

        Debug.Log("Selected tile: [" + i + "," + j + "]" + " and UID: " + grid[i, j].UID);

        if (j - 1 < 0) Debug.Log("Nothing North");
        else Debug.Log("North: [" + i + "," + (j - 1) + "]" + " and UID: " + grid[i, j - 1].UID);

        if (j + 1 >= row) Debug.Log("Nothing South");
        else Debug.Log("South: [" + i + "," + (j + 1) + "]" + " and UID: " + grid[i, j + 1].UID);

        if (i + 1 >= col) Debug.Log("Nothing east");
        else Debug.Log("East: [" + (i + 1) + "," + j + "]" + " and UID: " + grid[i + 1, j].UID);

        if(i - 1 < 0) Debug.Log("Nothing west");
        else Debug.Log("West: [" + (i - 1) + "," + j + "]" + " and UID: " + grid[i - 1, j].UID);
    }
}


