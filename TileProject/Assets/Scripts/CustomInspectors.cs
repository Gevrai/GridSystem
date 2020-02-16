using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class CustomInspectors : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GridManager gridManager = (GridManager)target;

        GUILayout.BeginHorizontal();
        /*if (GUILayout.Button("Find Neighbours", GUILayout.ExpandWidth(false)))
        {
            gridManager.FindNeighbours();
        }*/
        if (GUILayout.Button("Clear Grid", GUILayout.ExpandWidth(false)))
        {
            gridManager.ClearGrid();
        }
        GUILayout.EndHorizontal();

    }
}
