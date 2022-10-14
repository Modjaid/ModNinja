using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCard : MonoBehaviour
{
    [SerializeField] private Image difficultIndicator;
    [SerializeField] private GameObject[] starImages;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button button;
    [SerializeField] private Color easyColor;
    [SerializeField] private Color normColor;
    [SerializeField] private Color hardColor;

    [SerializeField] private int lvlNum;
    [SerializeField] private MainMenu menu;
    public void Start()
    {
        int stars = PlayerPrefs.GetInt("Fill_" + lvlNum, 0);
        for(int i = 0; i < stars; i++)
        {
            starImages[i].SetActive(true);
        }
        button.onClick.AddListener(() => menu.ClickCardLvl(lvlNum));
    }

    public void InstanceCard(MainMenu menu,int lvlNum, string sceneName, Difficult lvlDifficult)
    {
        this.lvlNum = lvlNum;
        this.menu = menu;
        levelText.text = "Level " + (lvlNum + 1).ToString();
        descriptionText.text = sceneName;
        button.onClick.AddListener(() => menu.ClickCardLvl(lvlNum));
        this.gameObject.name = $"button " + lvlNum;

        switch (lvlDifficult)
        {
            case Difficult.Easy:
                difficultIndicator.color = easyColor; break;
            case Difficult.Norm:
                difficultIndicator.color = normColor; break;
            case Difficult.Hard:
                difficultIndicator.color = hardColor; break;
        }
    }
}
