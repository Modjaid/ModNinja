using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(GameManager))]
public class TestOnInspector : Editor
{
    GameManager manager;
    SerializedProperty sceneList;


    public override void OnInspectorGUI()
    {
        manager = target as GameManager;
        DrawDefaultInspector();
        for (int i = 0; i < manager.lvlBase.Length; i++)
        {
            manager.lvlBase[i].description = $"{i}_{manager.lvlBase[i].sceneName}_{manager.lvlBase[i].levelMode.ToString()}";
        }

        EditorGUILayout.HelpBox("ALL FREE LEVELS", MessageType.Info, true);
        for (int i = 2; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string currentName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));

            string easyScene = currentName + " Easy\n";
            string normScene = currentName + " Norm\n";
            string hardScene = currentName + " Hard\n";

            for (int y = 0; y < manager.lvlBase.Length; y++)
            {
                if (manager.lvlBase[y].sceneName == currentName)
                {
                    switch (manager.lvlBase[y].levelMode)
                    {
                        case Difficult.Easy:
                            easyScene = ""; break;
                        case Difficult.Norm: normScene = ""; break;
                        case Difficult.Hard: hardScene = ""; break;
                    }
                }
            }
            EditorGUILayout.HelpBox(easyScene + normScene + hardScene + "\n", MessageType.None, true);
        }

        if (GUILayout.Button("ClearAllPlayerPrefs",GUILayout.Height(50)))
        {
            PlayerPrefs.DeleteAll();
        }

    }
}
