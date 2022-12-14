using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkManager))]
public class NeytworkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        NetworkManager manager = (NetworkManager)target;
        if (GUILayout.Button("ει"))
        {
            manager.Send();
        }
        if (GUILayout.Button("ιθΏ"))
        {
            manager.ConnectedToServer();
        }
    }
}
