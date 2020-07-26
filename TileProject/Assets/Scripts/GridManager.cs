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
 * Gevrai's comments
 *  - Separation of concern:
 *         - Grid management logic should be separated from displaying logic
 *              - Stuff such as sprites metadata, textures, materials, etc probably has no place in grid manager class
 *         - Grid should be an easy to use object with methods such as Get(i,j) or Set(i,j, tile). Underlying logic (2D matrix as a 1D array) should be hidden
 *         - If logic repeats, should probably be a function
 *         - If block is hard to understand, should probably be a function with explicit name
 *         - Use 'this' for member func/fields ? (maybe not classic in CSharp, I don't know...)
 *  - Cache stuff you will reuse
 *  - Try to do some Unit testing! it is well worth it IMHO (good luck :P)
 * */

public class GridManager : MonoBehaviour
{
    private static Material MATERIAL_UNLIT_BLACKLINE = new Material(Shader.Find("Unlit/UnlitBlackLine"));

    public enum GridPointDirection {
        UP, DOWN, LEFT, RIGHT, UP_LEFT, UP_RIGHT, DOWN_LEFT, DOWN_RIGHT 
    }

    private bool gridExist;
    private List<GameObject> gridLines = new List<GameObject>();
    private List<GameObject> gridTMPro = new List<GameObject>();
    public int col, row;

    private GameObject[] referenceTiles;
    private float spriteWidth, spriteHeight;
    private Camera cam;
    private UnityEvent shareGridMetrics;

    [Header("A prefab name in the ressource folder")]
    public GameObject baseTile;
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

    // NOTE: GenerateTiles and LoadTiles are too similar, should refactor

    public void GenerateTiles()
    {
        DestroyTiles();
        GenerateRefTiles();
        for (int i = 0; i < gridPoints.Length; i++)
        {
            gridPoints[i].containsTile = tileSpawnChance > Random.Range(0.0f, 1.0f)
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
            for (int i = 0; i < this.col + 1; i++)
            {
                ID++;
                var orig = new Vector3(i * spriteWidth, -1.0f, 0.0f)
                var dest = new Vector3(i * spriteWidth, -1.0f, row * -spriteHeight)
                GameObject newLine = DrawLine(orig, dest, Color.black, 0.020f, ID, GridLineHolder.transform);
                gridLines.Add(newLine);
            }
            for (int i = 0; i < this.row + 1; i++)
            {
                ID++;
                var orig = new Vector3(0.0f, -1.0f, i * -spriteHeight)
                var dest = new Vector3(col * spriteWidth, -1.0f, i * -spriteHeight)
                GameObject newLine = DrawLine( orig, dest, Color.black, 0.020f, ID, GridLineHolder.transform);
                gridLines.Add(newLine);
            }
        }
    }

    private GameObject DrawLine(Vector3 start, Vector3 end, Color color, float width, Transform parent)
    {
        var newLine = new GameObject();
        newLine.transform.SetParent(parent);

        newLine.transform.position = start;
        newLine.AddComponent<LineRenderer>();
        var lr = newLine.GetComponent<LineRenderer>();
        lr.material = MATERIAL_UNLIT_BLACKLINE;
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
                // TODO cache textmesh
                var TMPro = (GameObject)Instantiate(Resources.Load("TextMesh"), GridTMProHolder.transform);
                TMPro.transform.position = new Vector3(gridPoints[i].position.x + spriteWidth / 2, 1.0f, gridPoints[i].position.z - spriteHeight / 2); ;
                TMPro.GetComponent<TextMeshPro>().text = i.ToString();
                gridTMPro.Add(TMPro);
            }
        }
    }

    /// <summary> Toggle TextMeshPro and GridLines objects</summary>
    public void ToggleGridHelper()
    {
        gridlines.forEach( el => el.SetActive(!el.activeInHierarchy) );
        gridTMPro.forEach( el => el.SetActive(!el.activeInHierarchy) );
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

    public int FindGridPointNeighbour(int i, int j, GridPointDirection direction)
    {
        // Grid points is nice to be a 1D array, but should be wrapped in an easy to use (and understand) class.
        if (gridpoints.IsOutside(i)) {
            return null;
        }

        switch (direction)
        {
            case UP:
                if (gridPoints[i].UID - col >= 0)
                    // Why return the UID, and not the gripoint itself ??
                    return gridPoints[i - col].UID;
                break;
            case DOwN:
                if (gridPoints[i].UID + col <= gridPoints.Length)
                    return gridPoints[i + col].UID;
                break;
            case LEFT:
                if (gridPoints[i].UID - 1 >= 0 && i % col != 0)
                    return gridPoints[i - 1].UID;
                break;
            case RIGHT:
                if (gridPoints[i].UID + 1 <= gridPoints.Length && i % col != col - 1)
                    return gridPoints[i + 1].UID;
                break;
            case UP_LEFT:
                if (gridPoints[i].UID - col - 1 >= 0 && i % col != 0)
                    return gridPoints[i - col - 1].UID;
                break;
            case UP_RIGHT:
                if (gridPoints[i].UID - col + 1 >= 0 && i % col != col - 1)
                    return gridPoints[i - col + 1].UID;
                break;
            case DOWN_LEFT:
                if (gridPoints[i].UID + col - 1 <= gridPoints.Length && i % col != 0)
                    return gridPoints[i + col - 1].UID;
                break;
            case DOWN_RIGHT:
                if (gridPoints[i].UID + col + 1 <= gridPoints.Length && i % col != col - 1)
                    return gridPoints[i + col + 1].UID;
                break;
        }
        return -1;
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(Application.dataPath + "/saveFile.json")) 
        {
            File.Delete(Application.dataPath + "/saveFile.json");
            File.Delete(Application.dataPath + "/saveFile.json.meta");
        }
    }


    // While there are definitely some ways better fitted for exporting this grid data, I must say I like that Json is very easy to use and maintainable.
    // However, I would look into some way to save a binary in the future (near project completion) since json isn't well fitted for repeating structures (array of similar objects)
    // CSharp (or mono/Unity) most probably has some binary formatter easy to use
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