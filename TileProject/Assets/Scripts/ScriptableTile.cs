using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewtTile", menuName = "Create tile")]
public class ScriptableTile : ScriptableObject
{
    public Sprite tile;
    public enum State { walkable, blocked }
    public State tileState;
}
