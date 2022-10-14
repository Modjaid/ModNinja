using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MainMenu))]
public class CardGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var menu = target as MainMenu;
        if(GUILayout.Button("Generate All Cards",GUILayout.Height(60)))
        {
            menu.InstanceAllCards();
        }
        if (GUILayout.Button("Off All Cards", GUILayout.Height(60)))
        {
            menu.OffCards();
        }
    }
}
