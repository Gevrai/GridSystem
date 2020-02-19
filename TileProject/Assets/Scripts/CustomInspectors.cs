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
        if (GUILayout.Button("Generate Grid", GUILayout.ExpandWidth(false)))
        {
            gridManager.GenerateGrid();
        }
        if (GUILayout.Button("Generate Tiles", GUILayout.ExpandWidth(false)))
        {
            gridManager.GenerateTiles();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Grid", GUILayout.ExpandWidth(false)))
        {
            gridManager.ClearGrid();
        }
        if (GUILayout.Button("Save Grid", GUILayout.ExpandWidth(false)))
        {
            gridManager.SaveGridDatatoJson();
        }
        if (GUILayout.Button("Load Grid", GUILayout.ExpandWidth(false)))
        {
            gridManager.LoadGridDataFromJson();
        }
        GUILayout.EndHorizontal();

    }
}

[CustomEditor(typeof(ScriptableTile))]
public class CustomScriptableTileInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ScriptableTile scriptableTile = (ScriptableTile)target;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<<", GUILayout.ExpandWidth(false)))
        {
            scriptableTile.DecrMovCost();
        }
        EditorGUILayout.LabelField("Movement Cost: " + scriptableTile.moveCost, GUILayout.ExpandWidth(false));
        if (GUILayout.Button(">>", GUILayout.ExpandWidth(false)))
        {
            scriptableTile.IncrMovCost();
        }
        GUILayout.EndHorizontal();
    }
}
