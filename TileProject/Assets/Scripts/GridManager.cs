using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/*todo :
 * Change grid visual element to a button working as a toggle with chaning text (on/off).
 * Support tiles that cover multiple grid spaces && use new find neighbour function.
 * Integrate mouse control based grid edition.
 * Integrate multiple save profile
 * Support tetranimos, compatible with drag and drop.
 * 
 * Add character controller and wasd movements on grid
 * Add fire emblem mouvements with ranges.
 * Add ennemies
 * Add pathfinding AI
 * */

public class GridManager : MonoBehaviour
{
    public GridManager gridManager;
    [HideInInspector] public bool isGridGenerated;
    public int col, row;
    [HideInInspector] public float spriteWidth, spriteHeight;

    public GridPoints[] gridPoints;
    public bool drawGrid;
    [Range(0.0f, 1.0f)] 
    public float spawnChance = 1.0f;
    public List<ScriptableTile> tileTypes;
    [HideInInspector] public GameObject[] referenceTile;

    public GameObject topDownCam;

    private void Awake()
    {
        gridManager = this;
        referenceTile = new GameObject[tileTypes.Count];
    }

    private void Start()
    {
        GetTileSize();
        SetCam();
    }

    public void GenerateGrid()
    {
        if(!isGridGenerated)
        {
            isGridGenerated = true;
            gridPoints = new GridPoints[col * row];
            for (int i = 0; i < gridPoints.Length; i++)
            {
                gridPoints[i] = new GridPoints(gridManager, i, 0, col, row, spriteWidth, spriteHeight);
            }
            if (drawGrid) DrawGrid();
        }
        else Debug.Log("Grid already exist, aborting grid generation");
    }

    public void GenerateTiles()
    {
        GenerateAllRefTiles();
        for (int i = 0; i < (gridPoints.Length); i++)
        {
            gridPoints[i].hasTile = (Random.Range(0.0f, 1.0f) < spawnChance) ? true : false;
            if (gridPoints[i].hasTile)
            {
                gridPoints[i].identity = Random.Range(0, 3); // TEMPORARY, AUTOMATICALLY GEN VARIED TILE
                gridPoints[i].tile = (GameObject)Instantiate(referenceTile[gridPoints[i].identity], gridPoints[i].position, referenceTile[gridPoints[i].identity].transform.rotation, transform);
                gridPoints[i].tile.layer = 8;
            }
        }
        DestroyAllRefTiles();
    }

    public void LoadTiles()
    {
        GenerateAllRefTiles();
        for (int i = 0; i < (gridPoints.Length); i++)
        {
            if (gridPoints[i].hasTile)
            {
                gridPoints[i].tile = (GameObject)Instantiate(referenceTile[gridPoints[i].identity], gridPoints[i].position, referenceTile[gridPoints[i].identity].transform.rotation, transform);
                gridPoints[i].tile.layer = 8;
            }
        }
        DestroyAllRefTiles();
    }

    public void GenerateAllRefTiles()
    {
        for (int i = 0; i < tileTypes.Count; i++)
        {
            referenceTile[i] = (GameObject)Instantiate(Resources.Load("BlankTile32"));
            referenceTile[i].GetComponent<SpriteRenderer>().sprite = tileTypes[i].tileSprite;
        }
    }

    public void DestroyAllRefTiles()
    {
        for (int i = 0; i < tileTypes.Count; i++)
        {
            Destroy(referenceTile[i]);
        }
    }

    private void GetTileSize()
    {
        referenceTile[0] = (GameObject)Instantiate(Resources.Load("BlankTile32"));
        referenceTile[0].GetComponent<SpriteRenderer>().sprite = tileTypes[0].tileSprite;

        SpriteRenderer sprRend = referenceTile[0].GetComponent<SpriteRenderer>();
        spriteWidth = sprRend.sprite.rect.width / sprRend.sprite.pixelsPerUnit;
        spriteHeight = sprRend.sprite.rect.height / sprRend.sprite.pixelsPerUnit;

        Destroy(referenceTile[0]);
    }

    private void SetCam()
    {
        // set camera
        topDownCam.GetComponent<Transform>().position = new Vector3((col / 2.0f * spriteWidth), 5.0f, (row / 2.0f * -spriteHeight));
        topDownCam.GetComponent<Camera>().orthographicSize = col / 4.0f;
    }

    private void DrawGrid()
    {
        for (int i = 0; i < col + 1; i++)
        {
            DrawGridLine(new Vector3(i * spriteWidth, 0, 0), new Vector3(i * spriteWidth, 0, row * -spriteHeight), Color.black, 0.020f);
        }
        for (int i = 0; i < row + 1; i++)
        {
            DrawGridLine(new Vector3(0, 0, i * -spriteHeight), new Vector3(col * spriteWidth, 0, i * -spriteHeight), Color.black, 0.020f);
        }
    }

    public void ClearGrid()
    {
        for (int i = 0; i < col * row; i++) 
        {
            gridPoints[i].tileState = GridPoints.TileState.walkable;
            gridPoints[i].moveCost = 0;
            Destroy(gridPoints[i].tile);
        }
    }

    public int FindGridPointNeighbour(int UID, string direction)
    {
        int targetUID = 0;
        switch (direction)
        {
            case "up":
                if (gridPoints[UID].gridUID - col >= 0)
                    targetUID = gridPoints[UID - col].gridUID;
                break;

            case "down":
                if (gridPoints[UID].gridUID + col <= gridPoints.Length)
                    targetUID = gridPoints[UID + col].gridUID;
                break;

            case "left":
                if (gridPoints[UID].gridUID - 1 >= 0 && UID % col != 0)
                    targetUID = gridPoints[UID - 1].gridUID;
                break;

            case "right":
                if (gridPoints[UID].gridUID + 1 <= gridPoints.Length && UID % col != col - 1)
                    targetUID = gridPoints[UID + 1].gridUID;
                break;

            default:
                targetUID = -1;
                break;
        }
        return targetUID;
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
    
    public void SaveGridDatatoJson()
    {
        if (File.Exists(Application.dataPath + "/saveFile.json")) File.Delete(Application.dataPath + "/saveFile.json");
        string toJson = JsonHelper.ToJson(gridPoints, true);
        File.WriteAllText(Application.dataPath + "/saveFile.json", toJson);
    }

    public void LoadGridDataFromJson()
    {
        string fromJson = File.ReadAllText(Application.dataPath + "/saveFile.json");
        GridPoints[] gridPoints = JsonHelper.FromJson<GridPoints>(fromJson);

        ClearGrid();
        LoadTiles();
    }
}

