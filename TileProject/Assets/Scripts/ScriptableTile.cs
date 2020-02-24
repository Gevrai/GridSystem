using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewtTile", menuName = "Create tile")]
public class ScriptableTile : ScriptableObject
{
    public Sprite sprite;
    public enum TileState { walkable, blocked }
    public TileState tileState;
    [HideInInspector] public float moveCost;

    public ScriptableTile()
    {
        moveCost = 1.0f;
    }

    public void IncrMovCost()
    {
        moveCost += 0.5f;
        if(moveCost > 2.0f) moveCost = 2.0f;
    }

    public void DecrMovCost()
    {
        moveCost -= 0.5f;
        if (moveCost < 0.5f) moveCost = 0.5f;
    }
}
