using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/*todo :
 * Integrate scriptable object to tile generation
 * Preload all tile resources for tile generation and clean all resources afterward
 * Add character controller and wasd movements on grid
 * Add fire emblem mouvements with ranges.
 * Terrain editor exporting to json for save files.
 * Add ennemies
 * Add pathfinding AI
 * */

public class GridManager : MonoBehaviour
{
    public int col;
    public int row;
    [HideInInspector] public float spriteWidth;
    [HideInInspector] public float spriteHeight;

    public GridPoints[,] gridPoints;
    public bool drawGrid;
    public bool genTiles;
    [Range(0.0f, 1.0f)] public float spawnChance = 1.0f;
    private GameObject referenceTile; // TODO : Must be made into an array

    public GameObject topDownCam;

    public List<ScriptableTile> tiles;

    [System.Serializable]
    public class GridPoints
    {
        public string gridUID; // Used to find the good array entry when loading back from json
        public int col, row; // Not sure if necessary
        public enum PointState { walkable, blocked }
        public PointState state;
        public Vector3 position;
        public GameObject tile;
        public Vector2[] neighbours;

        public GridPoints(int col, int row, Vector3 position)
        {
            gridUID = SetGridPointUID(col, row);
            this.col = col;
            this.row = row;
            FindNeighbours();
            this.position = position;
            
        }

        private string SetGridPointUID(int row, int col)
        {
            string UID = "GridPoint:" + row + ":" + col;
            return UID;
        }

        private void FindNeighbours()
        {
            neighbours = new Vector2[4];
            neighbours[0] = new Vector2(col, row - 1);  // Up
            neighbours[1] = new Vector2(col, row + 1); // Down
            neighbours[2] = new Vector2(col - 1, row); // Left
            neighbours[3] = new Vector2(col + 1, row); // Right
        }
    }

    private void Start()
    {
        LoadReference();
        GenGrid();
        if(genTiles) GenTiles();
        //SaveGridDatatoJson();
        SetCam();

        Destroy(referenceTile);
    }

    public void LoadReference()
    {
        referenceTile = (GameObject)Instantiate(Resources.Load("Tile32White"));
        GetTileSize();
    }

    public void GenGrid()
    {
        gridPoints = new GridPoints[col, row];
        for (int i = 0; i < col; i++) {
            for (int j = 0; j < row; j++)
            {
                gridPoints[i, j] = new GridPoints(i, j, new Vector3(i * spriteWidth, 0.0f, j * -spriteHeight));
            }
        }
        if(drawGrid) DrawGrid();
    }

    public void GenTiles()
    {
        for (int i = 0; i < col; i++) {
            for (int j = 0; j < row; j++)
            {
                float a = Random.Range(0.0f, 1.0f);
                if(a < spawnChance)
                {
                    Vector3 worldPos = gridPoints[i, j].position;
                    gridPoints[i, j].tile = (GameObject)Instantiate(referenceTile, worldPos, referenceTile.transform.rotation, transform);
                    gridPoints[i, j].tile.layer = 8;
                    SetTileColor(i, j); // temporary, for testing
                }
            }
        }
    }

    private void GetTileSize()
    {
        SpriteRenderer sprRend = referenceTile.GetComponent<SpriteRenderer>();
        spriteWidth = sprRend.sprite.rect.width / sprRend.sprite.pixelsPerUnit;
        spriteHeight = sprRend.sprite.rect.height / sprRend.sprite.pixelsPerUnit;
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

    private void SetTileColor(int i, int j) // Temporary, for testing.
    {
        int rnd = Random.Range(0, 2);
        if (rnd == 0)
        {
            gridPoints[i, j].tile.GetComponentInChildren<SpriteRenderer>().material.SetColor("_Color", Color.green);
            gridPoints[i, j].state = GridPoints.PointState.walkable;
        }
        else
        {
            gridPoints[i, j].tile.GetComponentInChildren<SpriteRenderer>().material.SetColor("_Color", Color.gray);
            gridPoints[i, j].state = GridPoints.PointState.blocked;
        }
    }

    public void ClearGrid()
    {
        for (int i = 0; i < col; i++) {
            for (int j = 0; j < row; j++)
            {
                gridPoints[i, j].state = GridPoints.PointState.walkable;
                Destroy(gridPoints[i, j].tile);
            }
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

    /*
    private void SaveGridDatatoJson()
    {
        string toJson;
        string fromJson = "";
        if (File.Exists(Application.dataPath + "/saveFile.json")) File.Delete(Application.dataPath + "/saveFile.json");
        toJson = JsonHelper.ToJson(gridPoints, true);

        
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                if(File.Exists(Application.dataPath + "/saveFile.json")) 
                {
                    fromJson = File.ReadAllText(Application.dataPath + "/saveFile.json");
                }
                toJson = JsonUtility.ToJson(gridPoints[i, j], true);
                toJson = fromJson + toJson;
                File.WriteAllText(Application.dataPath + "/saveFile.json", toJson);
            }
        }
    }*/

    /*private void LoadGridDataFromJson()
    {
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
            }
        }
        string json = File.ReadAllText(Application.dataPath + "/saveFile.json");
        GridPoints loadedGridPointsData = JsonUtility.FromJson<GridPoints>(json);
    }


    private static string text = "This is an example string and my data is here";
    string data = getBetween(text, "string", "is");

    public static string getBetween(string strSource, string strStart, string strEnd)
    {
        int Start, End;
        if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        {
            Start = strSource.IndexOf(strStart, 0) + strStart.Length;
            End = strSource.IndexOf(strEnd, Start);
            Debug.Log(strSource.Substring(Start, End - Start));
            return strSource.Substring(Start, End - Start);
        }
        else
        {
            return "";
        }
    }*/


    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}

