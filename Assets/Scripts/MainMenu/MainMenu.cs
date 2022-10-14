using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Animation mainMenuPanel;
    [SerializeField] private Animation startGamePanel;
    [SerializeField] private Animation shopPanel;
    [SerializeField] private Animation howPlayPanel;
    [SerializeField] private Animation downloadPanel;

    [SerializeField] private GameObject lvlCardPrefab;
    [SerializeField] private Transform cardContent;
    [SerializeField] private AudioSource mainAudio;
    [SerializeField] private TextMeshProUGUI levelNameText;


    private int currentCard = 0;


    /// <param name="IsOpenStartPanel"> Открыть ли самое главное меню, либо сразу открыть выбор с уровнями</param>
    public void InitMenu(int levelPassed, bool IsOpenStartPanel)
    {
        this.currentCard = levelPassed;
        levelNameText.text = $"Level {currentCard + 1}";
        EnableLvlCards(levelPassed);

        if (IsOpenStartPanel)
        {
            startGamePanel.Play("Up");
        }
        else
        {
            mainMenuPanel.Play("Up");
        }

    }
    /// <summary>
    /// Событие кнопки Start
    /// </summary>
    public void StartGame()
    {
        startGamePanel.Play("Down");

       // if (GameManager.instance.IsGameJustTurnedOn)
       // {
       //     downloadPanel.Play("FullText");
       //     StartCoroutine(delayClick());
       //     GameManager.instance.IsGameJustTurnedOn = false;
       // }
       // else
       // {
       //     GameManager.instance.GoToNextLevel(cardSelected);
       // }
        downloadPanel.Play("FullText");
        StartCoroutine(delayClick());


        IEnumerator delayClick()
        {
            yield return new WaitUntil(()=>Input.GetMouseButtonDown(0));
            GameManager.instance.GoToNextLevel(currentCard);
        }
    }

    #region menuButtons
    public void GoToHowPlayPanel()
    {
        howPlayPanel.Play("Up");
        mainMenuPanel.Play("Down");
    }
    public void GoToShopPanel()
    {
        shopPanel.Play("Up");
        mainMenuPanel.Play("Down");
    }
    public void GoToStartGamePanel()
    {
        startGamePanel.Play("Up");
        mainMenuPanel.Play("Down");
    }
    public void BackFromStartGamePanel()
    {
        startGamePanel.Play("Down");
        mainMenuPanel.Play("Up");
    }
    public void BackFromShopPanel()
    {
        shopPanel.Play("Down");
        mainMenuPanel.Play("Up");
    }
    public void BackFromHowPlayPanel()
    {
        howPlayPanel.Play("Down");
        mainMenuPanel.Play("Up");
    }
    public void GoToBogdanInstagram()
    {
        Application.OpenURL("https://www.instagram.com/hodajmodjuja/?hl=ru");
    }
    public void GoToKsuInstagram()
    {
        Application.OpenURL("https://www.instagram.com/rallevski/?hl=ru");
    }
    public void GoToOlegInstagram()
    {
        Application.OpenURL("https://www.instagram.com/0re_one/?hl=ru");
    }
    #endregion

    public void ClickCardLvl(int lvlSelected)
    {
        currentCard = lvlSelected;
        levelNameText.text = $"Level {currentCard + 1}";
    }

    public void OffMainAudio()
    {
        StartCoroutine(upd());
        IEnumerator upd()
        {
            while (mainAudio.volume > 0.1f)
            {
                mainAudio.volume -= Time.deltaTime * 0.1f;
                yield return null;
            }
            mainAudio.volume = 0;
        }
    }
    private void EnableLvlCards(int levelActiveCount)
    {
        int i = 0;
        foreach (Transform card in cardContent)
        {
            card.gameObject.SetActive(true);
            i++;
            if (levelActiveCount < i)
            {
                return;
            }
        }
    }

    #region Editor
    public void InstanceAllCards()
    {
        for(int i = 0; i < GameManager.instance.lvlBase.Length; i++)
        {
            var card = Instantiate(lvlCardPrefab, cardContent).GetComponent<LevelCard>();

            card.InstanceCard(this, i, GameManager.instance.lvlBase[i].sceneName, GameManager.instance.lvlBase[i].levelMode);
        }
    }
    public void OffCards()
    {
        bool active = cardContent.GetChild(0).gameObject.activeSelf;
        foreach (Transform card in cardContent)
        {
            card.gameObject.SetActive(!active);
        }
    }
    #endregion

}
