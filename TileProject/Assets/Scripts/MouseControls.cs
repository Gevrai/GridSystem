using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControls : MonoBehaviour
{
    private bool isTargetValid;
    private GridManager tileMan;
    private Vector3 mouseDownPos;
    private Vector3 currMousePos;
    private Vector3 tilePos;
    private GameObject activeTile;
    private Vector2 gridPos;
    private Vector2 currGridPos;
    private AudioSource audioSource;
    public AudioClip mouseLeftDown;
    public AudioClip mouseLeftUp;

    [HideInInspector] public float sprWidth;
    [HideInInspector] public float sprHeight;

    [HideInInspector] public Vector3 tilePivotPos; // Needed bc TimeManager has a function to find it's neighbours
    private SpriteRenderer spriteAlpha;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        tileMan = GetComponent<GridManager>();

        sprHeight = tileMan.spriteHeight;
        sprWidth = tileMan.spriteWidth;
    }

    void Update()
    {
        OnClick(Input.GetMouseButtonDown(0));
        if (isTargetValid)
        {
            OnMouseHold(Input.GetMouseButton(0));
            OnRelease(Input.GetMouseButtonUp(0));
        }
    }

    private void OnClick(bool mouseDown)
    {
        if(mouseDown)
        {
            // Find in what position in the grid array our mouse position correlate to
            mouseDownPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float gridCol = Mathf.Floor(mouseDownPos.x / sprWidth);
            float gridRow = Mathf.Abs(Mathf.Ceil(mouseDownPos.z / sprHeight));
            gridPos = new Vector2(gridCol, gridRow);

            // If array tile val not null, set valid && get tile GO.pos
            if (tileMan.gridPoints[(int)gridPos.x, (int)gridPos.y].tile != null)
            {
                isTargetValid = true;
                activeTile = tileMan.gridPoints[(int)gridPos.x, (int)gridPos.y].tile;
                tilePivotPos = activeTile.GetComponent<Transform>().position;

                ChangeAlpha(0.5f);
                PlaySound(mouseLeftDown);
            }
        }
    }

    private void OnMouseHold(bool mouseHeld)
    {
        if(mouseHeld)
        {
            currMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Get mouse pos
            tilePos = currMousePos + (tilePivotPos - mouseDownPos); // Get && apply tile offset
            activeTile.GetComponent<Transform>().position = new Vector3(tilePos.x, 1, tilePos.z); // Put tile on top
        }
    }

    private void OnRelease(bool mouseReleased)
    {
        if(mouseReleased)
        {
            isTargetValid = false;

            currMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float gridCol = Mathf.Floor(currMousePos.x / sprWidth);
            float gridRow = Mathf.Abs(Mathf.Ceil(currMousePos.z / sprHeight));
            // Find in what position in the grid array our mouse position correlate to
            currGridPos = new Vector2(gridCol, gridRow);
            if (tileMan.gridPoints[(int)currGridPos.x, (int)currGridPos.y].tile == null)
            {
                tileMan.gridPoints[(int)currGridPos.x, (int)currGridPos.y].tile = activeTile;
                tileMan.gridPoints[(int)gridPos.x, (int)gridPos.y].tile = null;

                ChangeAlpha(0.5f);
                PlaySound(mouseLeftDown);

                Vector3 gridCoordinate = new Vector3(Mathf.Floor(currMousePos.x / sprWidth) * sprWidth, 0, Mathf.Ceil(currMousePos.z / sprHeight) * sprHeight);
                activeTile.GetComponent<Transform>().position = gridCoordinate;
            }
            else
            {
                Vector3 gridCoordinate = new Vector3(Mathf.Floor(mouseDownPos.x / sprWidth) * sprWidth, 0, Mathf.Ceil(mouseDownPos.z / sprHeight) * sprHeight);
                activeTile.GetComponent<Transform>().position = gridCoordinate;
            }

            ChangeAlpha(1.0f);
            PlaySound(mouseLeftUp);
        }
    }

    private void ChangeAlpha(float alpha)
    {
        SpriteRenderer rend = activeTile.GetComponent<SpriteRenderer>();
        Color c = rend.material.color;
        c.a = alpha;
        rend.material.color = c;
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}

// Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow, 3.0f);
// hit.collider.GetComponent<SpriteRenderer>().material.SetColor("_Color", Color.red);