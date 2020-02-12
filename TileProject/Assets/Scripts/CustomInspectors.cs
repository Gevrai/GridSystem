using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileManager))]
public class CustomInspectors : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileManager tileManager = (TileManager)target;
        if(GUILayout.Button("Find Neighbours"))
        {
            tileManager.FindNeighbours();
        }

    }
}
