using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/*todo :
 * Make it impossible to generate grids multiple time.
 * Integrate scriptable object to tile generation
 * Preload all tile resources for tile generation and clean all resources afterward
 * Add character controller and wasd movements on grid
 * Add fire emblem mouvements with ranges.
 * (Almost)Terrain editor exporting to json for save files.
 * Add ennemies
 * Add pathfinding AI
 * */

public class GridManager : MonoBehaviour
{
    public GridManager gridManager;
    public int col;
    public int row;
    [HideInInspector] 
    public float spriteWidth;
    [HideInInspector] 
    public float spriteHeight;

    public GridPoints[] gridPoints;
    public bool drawGrid;
    [Range(0.0f, 1.0f)] 
    public float spawnChance = 1.0f;
    public List<ScriptableTile> tileTypes;
    [HideInInspector] 
    public GameObject[] referenceTile;

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

    public void GenGrid()
    {
        gridPoints = new GridPoints[col * row];
        for (int i = 0; i < gridPoints.Length; i++) 
        {
            gridPoints[i] = new GridPoints(i, col, row, spriteWidth, spriteHeight, gridManager);
            gridPoints[i].hasTile = (Random.Range(0.0f, 1.0f) < spawnChance)? true: false;
        }
        if (drawGrid) DrawGrid();
    }
    
    public void GenTiles()
    {
        GenerateAllRefTiles();
        for (int i = 0; i < (gridPoints.Length); i++) 
        {
            if(gridPoints[i].hasTile)
            {
                gridPoints[i].tile = (GameObject)Instantiate(referenceTile, gridPoints[i].position, referenceTile.transform.rotation, transform);
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
        referenceTile[0] = (GameObject)Instantiate(Resources.Load("Tile32White"));
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
            gridPoints[i].state = GridPoints.PointState.walkable;
            Destroy(gridPoints[i].tile);
        }
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

        //GenTiles();
    }
}

