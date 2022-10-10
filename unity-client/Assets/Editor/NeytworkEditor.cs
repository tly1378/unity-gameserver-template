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
        if (GUILayout.Button("发送"))
        {
            manager.Send();
        }
        if (GUILayout.Button("重连"))
        {
            manager.ConnectedToServer();
        }
    }
}
