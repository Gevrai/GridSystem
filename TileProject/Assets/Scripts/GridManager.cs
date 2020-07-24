using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using TMPro;

/*todo :
 * Finish making sure tile draging, swapping and loading setup properly their respective scriptableTiles information.
 * Support tiles that cover multiple grid spaces && use new find neighbour function.
 * Integrate multiple save profile
 * Support tetranimos, compatible with drag and drop.
 * Support multiple select
 * Add character controller and wasd movements on grid
 *
 *
 *
 * 
 * */

public class GridManager : MonoBehaviour
{
    private List<GameObject> gridLines = new List<GameObject>();
    private List<GameObject> gridTMPro = new List<GameObject>();
    private GameObject[] referenceTiles;
    private bool gridExist;
    private float spriteWidth, spriteHeight;
    private Camera cam;
    private UnityEvent shareGridMetrics;
    private bool gridLinesExist;
    private bool gridTMProExist;

    [Header("A prefab name in the ressource folder")]
    public GameObject baseTile;
    public int col, row;
    [Range(0.0f, 1.0f)] public float tileSpawnChance = 1.0f;
    [Header("Scriptable tile objects for grid generation")]
    public List<ScriptableTile> scriptableTiles;
    public GridPoints[] gridPoints;

    [HideInInspector]
    public Texture texture;

    private void Start()
    {
        referenceTiles = new GameObject[scriptableTiles.Count];

        shareGridMetrics = new UnityEvent();
        shareGridMetrics.AddListener(GetComponent<MouseControls>().RefreshGridMetrics);
        InferGridPointSize(baseTile.name, 0);

        cam = Camera.main;
        SetupCamera();
    }

    public void GenerateGrid()
    {
        if(!gridExist)
        {
            gridExist = true;
            gridPoints = new GridPoints[col * row];
            for (int i = 0; i < gridPoints.Length; i++)
            {
                gridPoints[i] = new GridPoints(this, i, col, row, spriteWidth, spriteHeight);
            }
            GenerateGridLines();
            GenerateGridTMPro();
        }
        else Debug.Log("Grid already exist, aborting grid generation");
    }

    public void GenerateTiles()
    {
        DestroyTiles();
        GenerateRefTiles();
        for (int i = 0; i < gridPoints.Length; i++)
        {
            gridPoints[i].containsTile = (Random.Range(0.0f, 1.0f) < tileSpawnChance) ? true : false;
            if (gridPoints[i].containsTile)
            {
                gridPoints[i].identity = Random.Range(0, 3); // TEMPORARY, AUTOMATICALLY GEN VARIED TILE
                gridPoints[i].tile = (GameObject)Instantiate(referenceTiles[gridPoints[i].identity], gridPoints[i].position, referenceTiles[gridPoints[i].identity].transform.rotation, transform);
                gridPoints[i].tile.name = "Tile" + gridPoints[i].UID;
                gridPoints[i].tile.layer = 8;
            }
        }
        DestroyRefTiles();
    }

    public void LoadTiles()
    {
        DestroyTiles();
        GenerateRefTiles();
        for (int i = 0; i < gridPoints.Length; i++)
        {
            if (gridPoints[i].containsTile)
            {
                gridPoints[i].tile = (GameObject)Instantiate(referenceTiles[gridPoints[i].identity], gridPoints[i].position, referenceTiles[gridPoints[i].identity].transform.rotation, transform);
                gridPoints[i].tile.name = "Tile" + gridPoints[i].UID;
                gridPoints[i].tile.layer = 8;
            }
        }
        DestroyRefTiles();
    }

    /// <summary> Generate a specific tile object and assign it to a gridpoint</summary>
    /// <param name="identity"> Which tile type to use for generation</param>
    /// <param name="UID"> The unique identifier of the gridpoint that recieve the generated tile</param>
    public void GenerateNewTile(int identity, int UID)
    {
        Destroy(gridPoints[UID].tile);

        gridPoints[UID].containsTile = true;
        gridPoints[UID].identity = identity;
        GenerateRefTile(identity);

        gridPoints[UID].tile = (GameObject)Instantiate(referenceTiles[gridPoints[UID].identity], gridPoints[UID].position, referenceTiles[gridPoints[UID].identity].transform.rotation, transform);
        gridPoints[UID].tile.name = "Tile" + UID;
        gridPoints[UID].tile.layer = 8;
        // gridPoints[UID].SetupScriptableTileParams(); // TODO, find why it does not work

        Destroy(referenceTiles[identity]);
    }

    public void GenerateRefTile(int x)
    {
        referenceTiles[x] = (GameObject)Instantiate(Resources.Load(baseTile.name));
        referenceTiles[x].GetComponent<SpriteRenderer>().sprite = scriptableTiles[x].sprite;
    }

    public void GenerateRefTiles()
    {
        for (int i = 0; i < scriptableTiles.Count; i++)
        {
            referenceTiles[i] = (GameObject)Instantiate(Resources.Load(baseTile.name));
            referenceTiles[i].GetComponent<SpriteRenderer>().sprite = scriptableTiles[i].sprite;
        }
    }

    public void DestroyRefTiles()
    {
        for (int i = 0; i < scriptableTiles.Count; i++)
        {
            Destroy(referenceTiles[i]);
        }
    }

    /// <summary> Infer the width and height of all grid points based on a sprite size </summary>
    /// <param name="prefabName"> The prefab to use in ressource folder</param>
    /// <param name="refTile"> The Scriptable tile to get the sprite size from</param>
    private void InferGridPointSize(string prefabName, int refTile)
    {
        referenceTiles[refTile] = (GameObject)Instantiate(Resources.Load(prefabName));
        referenceTiles[refTile].GetComponent<SpriteRenderer>().sprite = scriptableTiles[refTile].sprite;

        SpriteRenderer sprRend = referenceTiles[0].GetComponent<SpriteRenderer>();
        spriteWidth = sprRend.sprite.rect.width / sprRend.sprite.pixelsPerUnit;
        spriteHeight = sprRend.sprite.rect.height / sprRend.sprite.pixelsPerUnit;

        shareGridMetrics.Invoke();

        Destroy(referenceTiles[0]);
    }

    /// <summary> Set camera position and other params to frame created grid size</summary>
    private void SetupCamera()
    {
        cam.orthographicSize = col / 4.0f;
        cam.GetComponent<Transform>().position = new Vector3((col / 2.0f * spriteWidth), 5.0f, (row / 2.0f * -spriteHeight));
    }

    private void GenerateGridLines()
    {
        if(!gridLinesExist)
        {
            gridLinesExist = true;
            GameObject GridLineHolder = new GameObject();
            GridLineHolder.transform.SetParent(this.gameObject.transform);
            GridLineHolder.name = "GridLineHolder";

            int ID = 0;
            for (int i = 0; i < col + 1; i++)
            {
                ID++;
                GameObject newLine = DrawLine(new Vector3(i * spriteWidth, -1.0f, 0.0f), new Vector3(i * spriteWidth, -1.0f, row * -spriteHeight), Color.black, 0.020f, ID, GridLineHolder.transform);
                gridLines.Add(newLine);
            }
            for (int i = 0; i < row + 1; i++)
            {
                ID++;
                GameObject newLine = DrawLine(new Vector3(0.0f, -1.0f, i * -spriteHeight), new Vector3(col * spriteWidth, -1.0f, i * -spriteHeight), Color.black, 0.020f, ID, GridLineHolder.transform);
                gridLines.Add(newLine);
            }
        }
    }

    private GameObject DrawLine(Vector3 start, Vector3 end, Color color, float width, int ID, Transform parent)
    {
        GameObject newLine = new GameObject();
        newLine.transform.SetParent(parent);

        // TODO : To function
        if (ID < 10)
            newLine.name = "GridLine" + "_" + "00" + ID.ToString();
        else if (ID < 100)
            newLine.name = "GridLine" + "_" + "0" + ID.ToString();
        else
            newLine.name = "GridLine" + "_" + ID.ToString();

        newLine.transform.position = start;
        newLine.AddComponent<LineRenderer>();
        LineRenderer lr = newLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/UnlitBlackLine"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        return newLine;
    }

    /// <summary> Create TextMeshPro object to display numbers in each grid squares</summary>
    private void GenerateGridTMPro()
    {
        if (!gridTMProExist)
        {
            gridTMProExist = true;
            GameObject GridTMProHolder = new GameObject();
            GridTMProHolder.transform.SetParent(this.gameObject.transform);
            GridTMProHolder.name = "GridTMProHolder";

            for (int i = 0; i < gridPoints.Length; i++)
            {
                GameObject TMPro = (GameObject)Instantiate(Resources.Load("TextMesh"), GridTMProHolder.transform);

                // TODO : To function
                if(i < 10)
                    TMPro.name = "TMPro" + "_" + "00" + i.ToString();
                else if(i < 100)
                    TMPro.name = "TMPro" + "_" + "0" + i.ToString();
                else 
                    TMPro.name = "TMPro" + "_" + i.ToString();

                TMPro.transform.position = new Vector3(gridPoints[i].position.x + spriteWidth / 2, 1.0f, gridPoints[i].position.z - spriteHeight / 2); ;
                TMPro.GetComponent<TextMeshPro>().text = i.ToString();

                gridTMPro.Add(TMPro);
            }
        }
    }

    /// <summary> Toggle TextMeshPro and GridLines objects</summary>
    public void ToggleGridHelper()
    {
        if(gridLinesExist && gridTMProExist)
        {
            if(gridLines[0].activeInHierarchy == true)
            {
                foreach (GameObject line in gridLines) line.SetActive(false);
                foreach (GameObject TMPro in gridTMPro) TMPro.SetActive(false);
            }
            else
            {
                foreach (GameObject line in gridLines) line.SetActive(true);
                foreach (GameObject TMPro in gridTMPro) TMPro.SetActive(true);
            }
        }
    }

    /// <summary> For each gridpoints, destroy it's tile"/> </summary>
    public void DestroyTiles()
    {
        for (int i = 0; i < col * row; i++) 
        {
            if(gridPoints[i].tile != null) 
            {
                Destroy(gridPoints[i].tile);
            }
        }
    }

    public int FindGridPointNeighbour(int i, string direction)
    {
        int targetUID = 0;
        switch (direction)
        {
            case "up":
                if (gridPoints[i].UID - col >= 0)
                    targetUID = gridPoints[i - col].UID;
                break;

            case "down":
                if (gridPoints[i].UID + col <= gridPoints.Length)
                    targetUID = gridPoints[i + col].UID;
                break;

            case "left":
                if (gridPoints[i].UID - 1 >= 0 && i % col != 0)
                    targetUID = gridPoints[i - 1].UID;
                break;

            case "right":
                if (gridPoints[i].UID + 1 <= gridPoints.Length && i % col != col - 1)
                    targetUID = gridPoints[i + 1].UID;
                break;

            case "upLeft":
                if (gridPoints[i].UID - col - 1 >= 0 && i % col != 0)
                    targetUID = gridPoints[i - col - 1].UID;
                break;

            case "upRight":
                if (gridPoints[i].UID - col + 1 >= 0 && i % col != col - 1)
                    targetUID = gridPoints[i - col + 1].UID;
                break;

            case "downLeft":
                if (gridPoints[i].UID + col - 1 <= gridPoints.Length && i % col != 0)
                    targetUID = gridPoints[i + col - 1].UID;
                break;

            case "downRight":
                if (gridPoints[i].UID + col + 1 <= gridPoints.Length && i % col != col - 1)
                    targetUID = gridPoints[i + col + 1].UID;
                break;

            default:
                targetUID = -1;
                break;
        }
        return targetUID;
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(Application.dataPath + "/saveFile.json")) 
        {
            File.Delete(Application.dataPath + "/saveFile.json");
            File.Delete(Application.dataPath + "/saveFile.json.meta");
        }
    }

    public void SaveGridDatatoJson()
    {
        File.Delete(Application.dataPath + "/saveFile.json");
        File.Delete(Application.dataPath + "/saveFile.json.meta");

        string toJson = JsonHelper.ToJson(gridPoints, true);
        File.WriteAllText(Application.dataPath + "/saveFile.json", toJson);
    }

    public void LoadGridDataFromJson()
    {
        string fromJson = File.ReadAllText(Application.dataPath + "/saveFile.json");
        GridPoints[] gridPoints = JsonHelper.FromJson<GridPoints>(fromJson);

        DestroyTiles();
        LoadTiles();
    }

    #region Class Proprieties
    public float SpriteWidth { get { return spriteWidth; } }
    public float SpriteHeight { get { return spriteHeight; } }
    public GameObject[] ReferenceTiles { get { return referenceTiles; } }
    #endregion
}

/*
GameObject textMesh = (GameObject)Instantiate(Resources.Load("TextMesh"), transform);
textMesh.name = "TMPro" + i.ToString();
textMesh.transform.position = new Vector3(gridPoints[i].position.x + spriteWidth/2, 0.0f, gridPoints[i].position.z - spriteHeight/2);;
textMesh.GetComponent<TextMeshPro>().text = i.ToString();
*/