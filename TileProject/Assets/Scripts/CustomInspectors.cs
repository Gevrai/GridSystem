using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GridManager))]
public class CustomInspectors : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GridManager gridManager = (GridManager)target;

        #region Scriptable tile selector
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.Space(58);
        if (GUILayout.Button("<<", GUILayout.ExpandWidth(false)))
        {
            //TODO
        }
        GUILayout.Space(58);
        GUILayout.EndVertical();

        gridManager.texture = (Texture)EditorGUILayout.ObjectField("", gridManager.texture, typeof(Texture), false, GUILayout.Height(128));

        GUILayout.BeginVertical();
        GUILayout.Space(58);
        if (GUILayout.Button(">>", GUILayout.ExpandWidth(false)))
        {
            //TODO
        }
        GUILayout.Space(58);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        #endregion

        #region Grid generation controls
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Grid", GUILayout.ExpandWidth(false)))
        {
            gridManager.GenerateGrid();
        }
        if (GUILayout.Button("Toggle Grid Helper", GUILayout.ExpandWidth(false)))
        {
            gridManager.ToggleGridHelper();
        }
        if (GUILayout.Button("Generate Tiles", GUILayout.ExpandWidth(false)))
        {
            gridManager.GenerateTiles();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Destroy Tiles", GUILayout.ExpandWidth(false)))
        {
            gridManager.DestroyTiles();
        }
        if (GUILayout.Button("Dlt Save",GUILayout.ExpandWidth(false)))
        {
            gridManager.DeleteSaveFile();
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
        #endregion
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
