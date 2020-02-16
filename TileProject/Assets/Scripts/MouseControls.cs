using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* MERGE WITH GRID MANAGER?
 * Shold this be merged with the grid manager? I split those up because they have inputs check within them.
 * If I wa using the new Unity input system which uses action and remove the need to check for input within the update loop,
 * I would have definitely never made up two classes.
*/

public class MouseControls : MonoBehaviour
{
    private GridManager gridManager;
    private bool isWithinGrid;
    private bool isHoldingTile;
    private Vector3 clickPosition;
    private Vector3 initialTilePos;
    private Vector3 currMousePos;
    private GameObject selectedTile;
    private int initialGridPoint;
    private int targetGridPoint;
    private AudioSource audioSource;
    public AudioClip mouseLeftDown;
    public AudioClip mouseLeftUp;

    [HideInInspector] public float gridUnitWidth;
    [HideInInspector] public float gridUnitHeight;

    private SpriteRenderer spriteAlpha;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gridManager = GetComponent<GridManager>();

        gridUnitWidth = gridManager.spriteWidth;
        gridUnitHeight = gridManager.spriteHeight;
    }

    void Update()
    {
        OnClick(Input.GetMouseButtonDown(0));
        OnMouseHold(Input.GetMouseButton(0));
        OnRelease(Input.GetMouseButtonUp(0));
    }

    private void OnClick(bool mouseDown)
    {
        if(mouseDown)
        {
            clickPosition = GetMousePos(Camera.main, gridUnitWidth, gridUnitHeight, out isWithinGrid);
            if(isWithinGrid)
            {
                initialGridPoint = WorldPosToGridPoint(clickPosition, gridUnitWidth, gridUnitHeight);
                if (gridManager.gridPoints[initialGridPoint].tile != null)
                {
                    isHoldingTile = true;
                    selectedTile = gridManager.gridPoints[initialGridPoint].tile;
                    initialTilePos = selectedTile.GetComponent<Transform>().position;

                    ChangeTileAlpha(0.5f);
                    PlayAudioClip(mouseLeftDown);
                }
            }
        }
    }

    private void OnMouseHold(bool mouseHeld)
    {
        if(mouseHeld && isHoldingTile)
        {
            Vector3 currMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPos = currMousePos + (initialTilePos - clickPosition); // Apply ofset so tiles are not held by corner
            selectedTile.GetComponent<Transform>().position = new Vector3(targetPos.x, 1, targetPos.z); // Drag tile and put in front of others
        }
    }

    private void OnRelease(bool mouseReleased)
    {
        if(mouseReleased && isHoldingTile)
        {
            currMousePos = GetMousePos(Camera.main, gridUnitWidth, gridUnitHeight, out isWithinGrid);
            ChangeTileAlpha(1.0f);
            PlayAudioClip(mouseLeftUp);
            if (isWithinGrid)
            {
                targetGridPoint = WorldPosToGridPoint(currMousePos, gridUnitWidth, gridUnitHeight);
                if (gridManager.gridPoints[targetGridPoint].tile == null)
                {
                    // Swap tile objects
                    gridManager.gridPoints[targetGridPoint].tile = selectedTile;
                    gridManager.gridPoints[initialGridPoint].tile = null;

                    GridPointToWorldPos(targetGridPoint);
                }
                else GridPointToWorldPos(initialGridPoint);
            }
            else GridPointToWorldPos(initialGridPoint);
            // Reset
            isHoldingTile = false;
            selectedTile = null;
            // Reset for safety. How should I handle this?
            isWithinGrid = false;
            initialTilePos = new Vector3(0.0f, 0.0f, 0.0f);
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
        float row = Mathf.Abs(Mathf.Ceil(worldPos.z / gridHeight));
        int position = (int)(col + (row * gridManager.row));
        Debug.Log(position);

        return position;
    }

    /// <summary> Return the world position of a grid point</summary>
    /// <param name="gridPoint"> GridPoint object</param>
    private void GridPointToWorldPos(int gridPoint)
    {
        selectedTile.GetComponent<Transform>().position = gridManager.gridPoints[gridPoint].position;
    }

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