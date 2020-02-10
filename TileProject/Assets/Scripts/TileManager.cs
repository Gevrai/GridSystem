using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{

    public int col;
    public int row;
    private float tileWidth;
    private float tileHeight;

    public GameObject[,] grid;
    private GameObject referenceTile;
    private SpriteRenderer sprRend;

    public GameObject topDownCam;

    private void Start()
    {
        //GenerateTiles();

    }

    public void GenerateTiles()
    {
        // Prep for grid
        grid = new GameObject[col, row];
        referenceTile = (GameObject)Instantiate(Resources.Load("Tile32"));
        GetTileSize();

        //Set tiles
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                if(grid[i, j] == null)
                {
                    GameObject newTile = Instantiate(referenceTile, new Vector3(i * tileWidth, 0.0f, j * -tileHeight), referenceTile.transform.rotation, transform);
                    newTile.GetComponentInChildren<TileObject>().UID = i * 10 + j;
                    grid[i, j] = newTile;
                }
                else
                {
                    Debug.Log("The grid position: " + i * j + " is already occupied");
                }
            }
        }
        // set camera
        topDownCam.GetComponent<Transform>().position = new Vector3((col / 2.0f * tileWidth), 5.0f, (row / 2.0f * -tileHeight));
        topDownCam.GetComponent<Camera>().orthographicSize = col / 4.0f;
        // Kill ref
        Destroy(referenceTile);
    }

    public void GenerateGrid()
    {

    }

    private void GetTileSize()
    {
        sprRend = referenceTile.GetComponent<SpriteRenderer>();
        tileWidth = sprRend.sprite.rect.width / sprRend.sprite.pixelsPerUnit;
        tileHeight = sprRend.sprite.rect.height / sprRend.sprite.pixelsPerUnit;
    }

    private void DrawGridLine(Vector3 start, Vector3 end, Color color, float width, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        //GameObject.Destroy(myLine, duration);
    }
}


