using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* MERGE WITH GRID MANAGER?
 * Shold this be merged with the grid manager? I split those up because they have inputs check within them.
 * If I was using the new Unity input system which uses action and remove the need to check for input within the update loop,
 * I would have definitely never made up two classes.
*/

public class MouseControls : MonoBehaviour
{
    private GridManager gridManager;
    private bool isWithinGrid, isHoldingTile;
    private Vector3 clickPos, initialTilePos, currMousePos;
    private GameObject selectedTile;
    private int originUID, targetUID;
    private float gridPointWidth, gridPointHeight;

    private AudioSource audioSource;
    public AudioClip mouseLeftDown, mouseLeftUp;
    private SpriteRenderer spriteAlpha;

    private void Awake()
    {
        gridManager = GetComponent<GridManager>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        OnMouseDown(Input.GetMouseButtonDown(0));
        OnPaint(Input.GetKeyDown(KeyCode.Space));
        OnMouseHold(Input.GetMouseButton(0));
        OnMouseRelease(Input.GetMouseButtonUp(0));
    }

    private void OnMouseDown(bool mouseDown)
    {
        if(mouseDown)
        {
            // WOW in, out, ref paramaters are pretty nice! go csharp!
            clickPos = GetMousePos(Camera.main, gridPointWidth, gridPointHeight, out isWithinGrid);
            if(isWithinGrid)
            {
                originUID = WorldPosToGridPoint(clickPos, gridPointWidth, gridPointHeight);
                if (gridManager.gridPoints[originUID].tile != null)
                {
                    isHoldingTile = true;
                    selectedTile = gridManager.gridPoints[originUID].tile;
                    initialTilePos = selectedTile.GetComponent<Transform>().position;

                    ChangeTileAlpha(0.5f);
                    PlayAudioClip(mouseLeftDown);
                }
            }
        }
    }

    private void OnPaint(bool spaceDown)
    {
        if(spaceDown)
        {
            clickPos = GetMousePos(Camera.main, gridPointWidth, gridPointHeight, out isWithinGrid);
            if(isWithinGrid)
            {
                int targetUID = WorldPosToGridPoint(clickPos, gridPointWidth, gridPointHeight);
                gridManager.GenerateNewTile(1, targetUID);
            }
        }
    }

    private void OnMouseHold(bool mouseHeld)
    {
        if(mouseHeld && isHoldingTile)
        {
            Vector3 currMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPos = currMousePos + (initialTilePos - clickPos); // Apply ofset so tiles are not held by corner
            selectedTile.GetComponent<Transform>().position = new Vector3(targetPos.x, 1, targetPos.z); // Drag tile and put in front of others
        }
    }

    private void OnMouseRelease(bool mouseReleased)
    {
        if(mouseReleased && isHoldingTile)
        {
            currMousePos = GetMousePos(Camera.main, gridPointWidth, gridPointHeight, out isWithinGrid);

            ChangeTileAlpha(1.0f);
            PlayAudioClip(mouseLeftUp);

            if (isWithinGrid)
            {
                targetUID = WorldPosToGridPoint(currMousePos, gridPointWidth, gridPointHeight);
                if (gridManager.gridPoints[targetUID].tile == null)
                {
                    // Move tile object
                    gridManager.gridPoints[targetUID].tile = selectedTile;
                    gridManager.gridPoints[targetUID].identity = gridManager.gridPoints[originUID].identity;
                    gridManager.gridPoints[originUID].tile = null;

                    TiletoGridPos(selectedTile, targetUID);
                }
                else 
                {
                    // Store target UID tile
                    GameObject temp = gridManager.gridPoints[targetUID].tile;
                    // Set target UID tile equal current UID tile
                    gridManager.gridPoints[targetUID].tile = gridManager.gridPoints[originUID].tile;
                    TiletoGridPos(gridManager.gridPoints[targetUID].tile, targetUID);
                    // Set current UID tile equal as stored UID tile
                    gridManager.gridPoints[originUID].tile = temp;
                    TiletoGridPos(gridManager.gridPoints[originUID].tile, originUID);
                    // Destroy stored UID tile GO
                    // Call setup on each grid point
                }
            }
            else TiletoGridPos(selectedTile, originUID);

            isHoldingTile = false;
            selectedTile = null;
            isWithinGrid = false;
        }
    }

    /// <summary> Get mouse cursor world position and determin if position is valid(on the grid)</summary>
    /// <param name="worldPos"> The world position to search from</param>
    /// <param name="gridWidth"> The width in meter of one grid space</param>
    /// <param name="gridHeight"> The height in meter of one grid space</param>
    /// <returns> Vector3</returns>
    private Vector3 GetMousePos(Camera cam, float gridWidth, float gridHeight, out bool isMousePosValid)
    {
        Vector3 position = cam.ScreenToWorldPoint(Input.mousePosition);
        if(position.x < 0 || position.z > 0 || position.x > gridManager.col * gridWidth || position.z < -(gridManager.row * gridHeight))
        {
            Debug.Log("Mouse position is outside of the grid: " + position);
            isMousePosValid = false;
        }
        else
        {
            isMousePosValid = true;
        }
        return position;
    }

    /// <summary> Find which grid array element correspond to target world pos</summary>
    /// <param name="worldPos"> The world position to search from</param>
    /// <param name="gridWidth"> The width in meter of one grid space</param>
    /// <param name="gridHeight"> The height in meter of one grid space</param>
    /// <returns> Integer</returns>
    private int WorldPosToGridPoint(Vector3 worldPos, float gridWidth, float gridHeight)
    {
        float col = Mathf.Floor(worldPos.x / gridWidth);
        float row = Mathf.Floor(Mathf.Abs(worldPos.z / gridHeight));
        int position = (int)(col + (row * gridManager.row));

        return position;
    }

    /// <summary> Move a tile GO to a grid point position</summary>
    private void TiletoGridPos(GameObject tile, int UID)
    {
        tile.GetComponent<Transform>().position = gridManager.gridPoints[UID].position;
    }

    public void RefreshGridMetrics()
    {
        gridPointWidth = gridManager.SpriteWidth;
        gridPointHeight = gridManager.SpriteHeight;
    }

    /// <summary> Make tile semi-transparent when selected</summary>
    private void ChangeTileAlpha(float alpha)
    {
        SpriteRenderer rend = selectedTile.GetComponent<SpriteRenderer>();
        Color c = rend.material.color;
        c.a = alpha;
        rend.material.color = c;
    }

    private void PlayAudioClip(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}