using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Analytics;

public class SDKController : MonoBehaviour, IUnityAdsListener
{
    private string gameId = "4055159";
    private bool testMode = false;
    public int AdPoints { get; set; }
    private DateTime startTime;
    private int secondsForShow = 150;

    private GameManager gameManager;
    public bool IsRewardReady
    {
        get
        {
            Debug.Log($"RewardVideoReady:{Advertisement.IsReady("rewardedVideo")}");
            return Advertisement.IsReady("rewardedVideo");
        }
    }
    public bool IsTimeForShow
    {
        get 
        {
           var currentTime = DateTime.Now - startTime;
           return currentTime.TotalSeconds > secondsForShow;
        }
    }

    public void Start()
    {
        gameManager = GetComponent<GameManager>();
        Advertisement.Initialize(gameId, testMode);
        Advertisement.AddListener(this);
        secondsForShow = 150;
        startTime = DateTime.Now;

        
    }
    public bool TryShowAd(int levelPassed)
    {
        if (Advertisement.IsReady())
        {
            if(levelPassed > 1 && (AdPoints > 7 || IsTimeForShow))
            {
                Advertisement.Show("video");
                Debug.Log($"Show Inter");
                startTime = DateTime.Now;
                AdPoints = -1;
                return true;
            }
            else
            {
                Debug.Log($"Inter ready BUT_{levelPassed}>1_{AdPoints}>1_{(DateTime.Now - startTime).TotalSeconds}>{secondsForShow}\nCONDITION:{(levelPassed > 1 && (AdPoints > 1 || IsTimeForShow))}");
            }
            return false;
        }
        else
        {
            Debug.Log($"Inter isNOT ready BUT_{levelPassed}>1_{AdPoints}>1_{(DateTime.Now - startTime).TotalSeconds}>{secondsForShow}\nCONDITION:{(levelPassed > 1 && (AdPoints > 1 || IsTimeForShow))}");
            return false;
        }
    }

    public void ShowRewardAd()
    {
        Advertisement.Show("rewardedVideo");
        AdPoints = 0;
        startTime = DateTime.Now.AddSeconds(40);

    }



    #region EVENTS
    public void OnUnityAdsReady(string placementId)
    {
       
    }

    public void OnUnityAdsDidError(string message)
    {
        
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if(placementId == "rewardedVideo")
        {
            if(showResult == ShowResult.Finished)
            {
                gameManager.Jumpers += 2;
                Debug.Log("AD_REWARD_FINISH_Jumpers +2");
            }
        }
    }
    #endregion
}
