using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    [Header("Joysticks")]
    [SerializeField] private GameObject pushJoystick;
    [SerializeField] private GameObject shurikenJoystick;

    [Header("jumpBar")]
    [SerializeField] private TextMeshProUGUI jumpText;
    [SerializeField] private Image jumpSlideImage;
    [SerializeField] private Image jumpBackgroundImage;
    [SerializeField] private Image jumpAvatarImage;

    [Header("ShurikenBar")]
    [SerializeField] private TextMeshProUGUI shurikenText;
    [SerializeField] private Image shurikenSlideImage;
    [SerializeField] private Image shurikenBackgroundImage;
    [SerializeField] private Image shurikenAvatarImage;

    [Header("KillBar")]
    [SerializeField] private TextMeshProUGUI killText;

    [Header("PushText")]
    [SerializeField] private Animation pushText_Anim;
    [SerializeField] private TextMeshProUGUI pushText_Up;
    [SerializeField] private TextMeshProUGUI pushText_Under;

    [Header("PausePanel")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject pausePanel;

    [Header("EndGamePanels")]
    [SerializeField] private Animation victoryPanel;
    [SerializeField] private GameObject failPanel;
    [SerializeField] private TextMeshProUGUI victoryDamageText;
    [SerializeField] private TextMeshProUGUI victoryTimeText;
    [SerializeField] private TextMeshProUGUI failTimeText;
    [SerializeField] private TextMeshProUGUI failDamageText;
    [SerializeField] private TextMeshProUGUI xBonusesShadows;
    [SerializeField] private TextMeshProUGUI xBonuses;
    [SerializeField] private TextMeshProUGUI newRecordText;
    [SerializeField] private Image victoryFillImage;
    [SerializeField] private GameObject rewardButton;

    [SerializeField] private AudioSource clickAudio;

    private SlideBar jumpSlideBar;
    private SlideBar shurikenSlideBar;


    private void Start()
    {
        jumpSlideBar = new SlideBar(1);
        shurikenSlideBar = new SlideBar(FieldManager.instance.player.GetComponent<PlayerPuck>().shurikenGenerationTimer,1);
        shurikenSlideBar.sliding = StartCoroutine(shurikenSlideBar.SlidingWithRepeat(shurikenSlideImage));
        UpdateShurikenBar(0);
        UpdateJumperSlideBar(GameManager.instance.Jumpers);
    }



    public void PlayPushText(int XComboNum)
    {
         pushText_Up.text = "X" + XComboNum.ToString();
         pushText_Under.text = "X" + XComboNum.ToString();
         pushText_Anim.Play("pushText");
    }
    public void UpdateJumperSlideBar(float currentJumpers)
    {
        if (jumpSlideBar.sliding != null) StopCoroutine(jumpSlideBar.sliding);

        if (currentJumpers >= 1f)
        {
            jumpText.text = "X" + Convert.ToInt32(currentJumpers).ToString();
            jumpSlideImage.fillAmount = 1;
            jumpAvatarImage.color = Color.blue;
            jumpBackgroundImage.color = Color.cyan;
        }
        else
        {
            jumpAvatarImage.color = Color.black;
            jumpBackgroundImage.color = Color.grey;

            jumpSlideBar.sliding = StartCoroutine(jumpSlideBar.Sliding(jumpSlideImage, currentJumpers));
            jumpText.text = "";
        }

    }

    public void UpdateShurikenBar(int currentShurikens)
    {
        shurikenText.text = currentShurikens.ToString();
        if(currentShurikens == 0)
        {
            shurikenAvatarImage.color = Color.black;
            shurikenBackgroundImage.color = Color.grey;
        }
        else
        {
            shurikenAvatarImage.color = Color.red;
            shurikenBackgroundImage.color = new Color(1, 0, 0.2f, 1);

        }
    }

    public void UpdateKillInfo(int diedEnemyCount, int commonEnemyCount)
    {

        killText.text = diedEnemyCount + "/" + commonEnemyCount;
    }

    public void ActiveFinishPanel(int matchDamage, TimeSpan timeRecods,int maxNeededRecord, float bonus, int conclude, bool isNewRecord)
    {
        GameManager.sdk.AdPoints+=3;
        pauseButton.SetActive(false);
        pushJoystick.SetActive(false);
        shurikenJoystick.SetActive(false);

        victoryPanel.gameObject.SetActive(true);
        victoryDamageText.text = "Common damage:\n" + (0).ToString();
        victoryTimeText.text = $"Time:\n00:00";

        bool PlayingQueue = true;
        Coroutine animationQueue = StartCoroutine(PlayAnimationQueue());
        StartCoroutine(ClickControlForStopAnimationQueue());

        IEnumerator PlayAnimationQueue()
        {


            yield return new WaitUntil(() => !victoryPanel.isPlaying);
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(PlayDamageNumbers());
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(PlayXbonuses());
            PlayingQueue = false;
            if (isNewRecord)
            {
                StartCoroutine(NewRecordIndicator());
            }
      
        }

        //TODO: Добавить клик для ускорения анимаций
        IEnumerator ClickControlForStopAnimationQueue()
        {
            yield return new WaitUntil(() => !victoryPanel.isPlaying);

            while (PlayingQueue)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    StopCoroutine(animationQueue);
                    victoryDamageText.text = "Common damage:\n" + matchDamage.ToString();
                    victoryTimeText.text = $"Time:\n{timeRecods.Minutes}:{timeRecods.Seconds}";
                    victoryFillImage.fillAmount = conclude / (float)maxNeededRecord;
                    if (bonus != 1)
                    {
                        xBonuses.text = "X" + bonus.ToString();
                        xBonusesShadows.text = xBonuses.text;
                        victoryPanel.Play("XBonuses");
                    }
                    if (isNewRecord)
                    {
                        StartCoroutine(NewRecordIndicator());
                    }
                    victoryPanel.Play("UpButtons");
                    break;
                }
                yield return null;
            }
        }

        IEnumerator PlayDamageNumbers()
        {
            float currentNumber = 0;
     
            while (currentNumber < matchDamage - 100)
            {
                currentNumber = Mathf.Lerp(currentNumber, matchDamage, 2 * Time.deltaTime);
                victoryDamageText.text = "Common damage:\n" + ((int)currentNumber).ToString();

                victoryFillImage.fillAmount = currentNumber / maxNeededRecord;
                yield return null;
            }
         //   victoryFillImage.fillAmount = damageRecord / maxNeededRecord;
            victoryDamageText.text = "Common damage:\n" + matchDamage.ToString();
        }
        IEnumerator PlayXbonuses()
        {
            float currentMinuts = 0;
            float currentSeconds = 0;

            while (currentMinuts < timeRecods.Minutes - 0.4f || currentSeconds < timeRecods.Seconds - 0.4f)
            {
                currentMinuts = Mathf.Lerp(currentMinuts, timeRecods.Minutes, 2f * Time.deltaTime);
                currentSeconds = Mathf.Lerp(currentSeconds, timeRecods.Seconds, 2f * Time.deltaTime);
                victoryTimeText.text = $"Time:\n{(int)currentMinuts}:{(int)currentSeconds}";
                yield return null;
            }
            victoryTimeText.text = $"Time:\n{timeRecods.Minutes}:{timeRecods.Seconds}";


            if (bonus != 1)
            {
                xBonuses.text = "X" + bonus.ToString();
                xBonusesShadows.text = xBonuses.text;
                victoryPanel.Play("XBonuses");
            }

            yield return new WaitUntil(() => !victoryPanel.isPlaying);

            float currentNumber = matchDamage;
           
            
            while (currentNumber < conclude - 400)
            {
                currentNumber = Mathf.Lerp(currentNumber, conclude, 2 * Time.deltaTime);

                victoryFillImage.fillAmount = currentNumber / maxNeededRecord;
                yield return null;
            }
            victoryFillImage.fillAmount = conclude / (float)maxNeededRecord;
            victoryPanel.Play("UpButtons");
        }
        IEnumerator NewRecordIndicator()
        {
            while (true)
            {
                newRecordText.gameObject.SetActive(true);
                yield return new WaitForSeconds(1.5f);
                newRecordText.gameObject.SetActive(false);
                yield return new WaitForSeconds(1.5f);
            }
        }

    }

    public void ActiveFailPanel(int damageRecord, TimeSpan timeRecods)
    {
        PausePanel(false);
        var damageText = $"Common damage:\n" + damageRecord.ToString();
        var timeText = $"Time:\n{timeRecods.Minutes}:{timeRecods.Seconds}";

        pauseButton.SetActive(false);
        pushJoystick.SetActive(false);
        shurikenJoystick.SetActive(false);

        failPanel.SetActive(true);
        failDamageText.text = damageText;
        failTimeText.text = timeText;
        GameManager.sdk.AdPoints++;
        if (GameManager.sdk.IsRewardReady)
        {
            rewardButton.SetActive(true);
        }
        else
        {
            rewardButton.SetActive(false);
        }
    }
    public void PausePanel(bool isActive)
    {
        pushJoystick.SetActive(!isActive);
        shurikenJoystick.SetActive(!isActive);
        pausePanel.SetActive(isActive);
        pauseButton.SetActive(!isActive);
        GameManager.instance.music.Pause(isActive);

        Time.timeScale = (isActive) ? 0 : 1;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
    public void ExitToMenu(bool OpenStartPanel)
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        GameManager.instance.OpenStartPanel = OpenStartPanel;
        GameManager.instance.music.NextTrack();
        SceneManager.LoadScene("Menu");
        GameManager.sdk.AdPoints++;
    }
    public void Restart()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        GameManager.sdk.AdPoints++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ClickRewardButton()
    {
        GameManager.sdk.ShowRewardAd();
        rewardButton.SetActive(false);
    }

}
