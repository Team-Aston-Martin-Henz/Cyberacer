using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(TCDocumentation))]
public class TCDocumentationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Open Documentation"))
        {
            Application.OpenURL("https://ilonion.com/transcity_documentation");
        }
    }
}
