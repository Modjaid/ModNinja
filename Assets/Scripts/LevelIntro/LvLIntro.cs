using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LvLIntro : FieldManager
{
    [HideInInspector] public Animation anim;
    public string[] cutScenes;
    public int startCutSceneIndex;
    public bool IsStopTimeOnDead = false; // Для тренировки комбо, при первом убийстве время замедляется до полной остановки
    public int countEnemyForOffStopTime = 5; // Требуется для отключения режима полного остановления времени
    public Transform finishJumpPos;
    public bool NextFinishCutScene;

    private Coroutine slowMotion;
    private PlayerPuck playerPuck;

    public void Start()
    {
        playerPuck = player.GetComponent<PlayerPuck>();
        anim = GetComponent<Animation>();
        anim.Play(cutScenes[startCutSceneIndex]);
    }

    public void Update()
    {

        if (countEnemyForOffStopTime == 0)
        {
            Invoke("NextCutScene",3);
            IntroFasterMotion(1f);
            countEnemyForOffStopTime--;
        }
        if (NextFinishCutScene)
        {
            if(playerPuck.elements.jumpers < 1)
            {
                playerPuck.JumpToPos(finishJumpPos.position);
                NextFinishCutScene = false;
            }
        }
    }
    public void UpdJumperSlideBar()
    {
        canvas.UpdateJumperSlideBar(playerPuck.elements.jumpers);
    }

    public override void SlowMotion(Transform target)
    {
        if (IsStopTimeOnDead)
        {
            IntroSlowMotion();
            if(countEnemyForOffStopTime == 1)
            {
                IsStopTimeOnDead = false;
            }
            countEnemyForOffStopTime--;
            return;
        }
        base.SlowMotion(target);
    }
    public override void TrySlowMotion(Transform target)
    {
        if (IsStopTimeOnDead)
        {
            return;
        }
        base.TrySlowMotion(target);
    }

    /// <summary>
    /// (Для смерти) Вызов обязательного слоу мошн
    /// </summary>
    /// <param name="target">Цель для камеры</param>
    public void IntroSlowMotion()
    {
        if (slowMotion != null) StopCoroutine(slowMotion);



        slowMotion = StartCoroutine(Slower());


        IEnumerator Slower()
        {
          //  Time.timeScale = 0.5f;
          //  Time.fixedDeltaTime = 0.02f * Time.timeScale;

            while (Time.timeScale > 0.01f)
            {
                Time.timeScale -= Time.deltaTime;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                yield return null;
            }
            Time.timeScale = 0.01f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }
    public void IntroFasterMotion(float endTimeScale)
    {
        if (slowMotion != null) StopCoroutine(slowMotion);



        slowMotion = StartCoroutine(Faster());


        IEnumerator Faster()
        {
            Time.timeScale = 0.1f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            while (Time.timeScale < endTimeScale)
            {
                Time.timeScale += Time.deltaTime * 0.2f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                yield return null;
            }
            Time.timeScale = endTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }
    public override void PushTextActive(bool isReset = false)
    {

        if (isReset)
        {
            pushXComboNum = 2;
        }
        else
        {
            if(IsStopTimeOnDead)
            {
                IntroFasterMotion(0.2f);
            }
            canvas.PlayPushText(pushXComboNum);
            pushXComboNum++;
        }

    }
    /// <summary>
    /// Четвертая кат сцена " где враги берут в окружение "
    /// </summary>
    private void NextCutScene()
    {
        anim.Play(cutScenes[3]);
    }
}
