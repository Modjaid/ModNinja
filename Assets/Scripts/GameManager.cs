using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static SDKController sdk;

    [SerializeField] private int saveKey;
    [SerializeField] private int levelPassed;
    [SerializeField] private int jumpers;
    [SerializeField] public LvlData[] lvlBase;
    [SerializeField] public MusicPlayer music;

    public int CurrentLvl;

    /// <summary>
    /// Включена ли игра только что? (Нужно для панели загрузки в меню с подсказками)
    /// </summary>
    public bool OpenStartPanel { get; set; }

    public int LevelPassed
    {
        get
        {
            if (levelPassed == -1)
            {
                return PlayerPrefs.GetInt("LevelPassed", 0);
            }
            else
            {
                return levelPassed;
            }
        }
        set
        {
            PlayerPrefs.SetInt("LevelPassed", value);
        }
    }
    public int Jumpers
    {
        get
        {
            return PlayerPrefs.GetInt("Jumpers", 0);
        }
        set
        {
            PlayerPrefs.SetInt("Jumpers", value);
        }
    }

    private FieldManager LvlManager
    {
        get
        {
           return GameObject.Find("LevelManager").GetComponent<FieldManager>();
        }
    }

    private Difficult currentLevelMode;

    public void Awake()
    {
        if (!instance)
        {
            sdk = gameObject.AddComponent<SDKController>();
            instance = this;
            DontDestroyOnLoad(transform.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        InitGame();
    }


    public void GoToNextLevel(int level)
    {
        currentLevelMode = lvlBase[level].levelMode;
        CurrentLvl = level;
        SceneManager.LoadScene(lvlBase[level].sceneName);
    }


    /// <returns>True если записан новый рекорд</returns>
    public bool SaveLevelCompleted(int starCount)
    {
       int oldStarCount = PlayerPrefs.GetInt("Fill_" + CurrentLvl, 0);
      if(CurrentLvl == LevelPassed)
       {
           LevelPassed++;
           Analytics.SendEvent("LevelPassed",LevelPassed);
        }


        if (oldStarCount >= starCount)
        {
            return false;
        }
        else
        {
            PlayerPrefs.SetInt("Fill_" + CurrentLvl, starCount);
            return true;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelGameScene;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelGameScene;
    }

    private void OnLevelGameScene(Scene scene, LoadSceneMode sceneMode)
    {
        if(scene.name == "Menu")
        {
            GameObject.Find("Menu").GetComponent<MainMenu>().InitMenu(LevelPassed, OpenStartPanel);
        }
        else
        {
            LvlManager.StartGame(currentLevelMode);
            if (sdk.TryShowAd(LevelPassed))
            {
                FieldManager.instance.InvokePusePanel();
            }
        }
    }
    private void InitGame()
    {
        //TODO: Отключить, когда потребуется рабочее сохранение
        CurrentLvl = LevelPassed;
        OpenStartPanel = false;
        int key = PlayerPrefs.GetInt("SaveKey", 0);
        if(saveKey != key)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("SaveKey", saveKey);
        }
    }




    [Serializable]
    public struct LvlData
    {
        public string description;
        public string sceneName;
        public Difficult levelMode;
    }

}
[Serializable]
public enum Difficult
{
    Easy,
    Norm,
    Hard
}

