using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class FieldManager : MonoBehaviour
{
    public static FieldManager instance;

    public event Action OnChangeSpecialPucks;

    [HideInInspector] public MainCamera camera;
    [HideInInspector] public GameObject player;
    [HideInInspector] public List<GameObject> enemyPucks;

    [Tooltip("Режимы и рекордные требования данного уровня")]
    [SerializeField] private LevelState[] lvlStates;

    [Header("SlowMotion_DeadEffect")]
    [SerializeField] private float slowMotion_Dead_Timer = 2;
    [SerializeField] private float slowMotion_Dead_Zoom = 25;
    [SerializeField] private float slowMotion_Dead_TimeScale = 0.1f;
    [SerializeField] private float slowMotion_Dead_CameraSpeed = 30;
    [Header("SlowMotion_CollisionEffect")]
    [SerializeField] private float slowMotion_Collision_Timer = 0.4f;
    [SerializeField] private float slowMotion_Collision_Zoom = 30;
    [SerializeField] private float slowMotion_Collision_TimeScale = 0.8f;
    [SerializeField] private float slowMotion_Collision_CameraSpeed = 1;

    [Header("CameraDefaultParams")]
    [SerializeField] private float defaultCameraZoom = 45f;
    [Tooltip("Distance for pursue the Camera To the Enemy")]
    [SerializeField] private float distancePursueCamera = 85f;

    [Header("GameCanvas")]
    [SerializeField] protected GameCanvas canvas;

    [Header("Audio")]
    [SerializeField] private AudioSource[] slowMotionAudio;

    public int levelMode;
    private float matchSeconds;
    private Coroutine timerRoutine;
    public int CommonGameDamage { get; set; } = 0;
    

    public int DiedEnemyCount { get; set; }
    public int StartEnemyCount { get; set; }

    protected int pushXComboNum = 2;


    private Coroutine slowMotion;

    private void Awake()
    {
        AutoInitLevel();
    }

    public void Update()
    {
        
    }

    private void AutoInitLevel()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
        UpdateAllActiveEnemy();
        camera = Camera.main.GetComponent<MainCamera>();
        player.GetComponent<PlayerPuck>().elements.OnChangeJumpers += canvas.UpdateJumperSlideBar;
        player.GetComponent<PlayerPuck>().elements.OnChangeShurikens += canvas.UpdateShurikenBar;
        player.GetComponent<PlayerPuck>().OnPlayerHit += GameManager.instance.music.IncreasePitch;
        FloatingJoystick.OnClickDown = StartGameTimer;
        camera.SetCameraParking(player.transform);
        camera.SetCameraZoom(defaultCameraZoom, 2f);

    }

    public void StartGame(Difficult levelMode)
    {
        this.levelMode = (int)levelMode;
        foreach (LevelState section in lvlStates)
        {
            section.enemySection.SetActive(false);
        }
        lvlStates[this.levelMode].enemySection.SetActive(true);
        UpdateAllActiveEnemy();

        GameManager.instance.music.StartPlay();
    }

    public void UpdateAllActiveEnemy()
    {
        enemyPucks = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        StartEnemyCount = enemyPucks.Count;
        canvas.UpdateKillInfo(DiedEnemyCount, StartEnemyCount);
        OnChangeSpecialPucks?.Invoke();
    }

    public void PlayerIsDied()
    {
        player = null;
        OnChangeSpecialPucks?.Invoke();
    }

    public bool SlowMotionIsGoing()
    {
        if (slowMotion == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    /// <summary>
    /// (Для смерти) Вызов обязательного слоу мошн
    /// </summary>
    /// <param name="target">Цель для камеры</param>
    public virtual void SlowMotion(Transform target)
    {
        if(slowMotion != null) StopCoroutine(slowMotion);
        slowMotion = StartCoroutine(Active());

       if(!slowMotionAudio[0].isPlaying) slowMotionAudio[0].Play();
      //  GameManager.instance.music.PlayOnDeadEnemy();
        camera.SetCameraParking(target,0,slowMotion_Dead_CameraSpeed);
        camera.SetCameraZoom(slowMotion_Dead_Zoom, slowMotion_Dead_CameraSpeed);
        Time.timeScale = slowMotion_Dead_TimeScale;


        Time.fixedDeltaTime = 0.02f * Time.timeScale;


        IEnumerator Active()
        {
            yield return new WaitForSecondsRealtime(slowMotion_Dead_Timer);
            Time.timeScale = 1;
            //
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            camera.SetCameraZoom(defaultCameraZoom);
            camera.SetCameraParking(player.transform, 0, 4);
            if (!slowMotionAudio[1].isPlaying) slowMotionAudio[1].Play();
            slowMotion = null;

        }
    }
    /// <summary>
    /// (Для удара) сработает при условиях если слоу мошн не активен, и если цель не слишком далека от героя
    /// </summary>
    /// <param name="target">Цель для камеры</param>
    public virtual void TrySlowMotion(Transform target)
    {
        if (!MayCameraPursue(target)) return;
        if (slowMotion != null) return;

        slowMotion = StartCoroutine(Active());
        //  camera.GetComponent<Camera>().orthographicSize = 15;
        camera.SetCameraParking(target, 0, slowMotion_Collision_CameraSpeed);
        camera.SetCameraZoom(slowMotion_Collision_Zoom, 2);
        Time.timeScale = slowMotion_Collision_TimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        IEnumerator Active()
        {
            yield return new WaitForSecondsRealtime(slowMotion_Collision_Timer);
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            camera.SetCameraZoom(defaultCameraZoom);
            camera.SetCameraParking(player.transform);
            slowMotion = null;
        }
    }
    public virtual void TrySlowMotion(Transform target,float zoom,float timeScale,float timer, float speed)
    {
        if (!MayCameraPursue(target)) return;
        if (slowMotion != null) return;

        slowMotion = StartCoroutine(Active());
        camera.SetCameraParking(target, 0, speed);
        camera.SetCameraZoom(zoom, 2);
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        IEnumerator Active()
        {
            yield return new WaitForSecondsRealtime(timer);
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            camera.SetCameraZoom(defaultCameraZoom);
            if(player != null) camera.SetCameraParking(player.transform);
            slowMotion = null;
        }
    }

    public void InvokePusePanel()
    {
        canvas.PausePanel(true);
    }

    /// <summary>
    /// Функция срабатывает только при дефолтном аргументе, true исключительно ради сброса XCombo в начало
    /// </summary>
    /// <param name="isReset"></param>
    public virtual void PushTextActive(bool isReset = false)
    {
        if (isReset)
        {
            pushXComboNum = 2;
        }
        else
        {
            canvas.PlayPushText(pushXComboNum);
            pushXComboNum++;
        }
    }

    public void EndGame(bool IsVictory,float delayForOpenPanel)
    {
        //TODO: Доделать запись игрового секундомера не от реального времени, ибо в игре есть замедлялки
        TimeSpan gameTime = TimeSpan.FromSeconds(matchSeconds);
        camera.SetCameraZoom(60,1);
        foreach (GameObject puck in enemyPucks)
        {
            puck.GetComponent<PuckManager>().Stop();
        }

        float bonus = 0;
        int conclude = 0;
        int countFullStars = 0;
        bool isNewRecord = false;

        if (IsVictory)
        {
            bonus = 0;
            conclude = 0;
            countFullStars = CountAllBonuses(gameTime, CommonGameDamage, ref bonus, ref conclude);
            isNewRecord = GameManager.instance.SaveLevelCompleted(countFullStars);
            GameManager.instance.Jumpers = 0;
            player.GetComponent<PlayerPuck>().OnPlayerHit -= GameManager.instance.music.IncreasePitch;
            GameManager.instance.music.Stop(30);
            Analytics.CustomEvent("Level Completed", new Dictionary<string, object> 
            {
                { "Level",GameManager.instance.CurrentLvl},
                { "Bonuses",CommonGameDamage * bonus}
            });
        }
        else
        {
            GameManager.instance.music.Stop(0,0.6f);
            Analytics.SendEvent("Level Fail", GameManager.instance.CurrentLvl);
        }


        StartCoroutine(Delay());
        IEnumerator Delay()
        {
            yield return new WaitForSeconds(delayForOpenPanel);
            if (IsVictory)
            {
                canvas.ActiveFinishPanel(CommonGameDamage,gameTime,lvlStates[levelMode].requierdRecord,bonus,conclude, isNewRecord);
            }
            else
            {
                canvas.ActiveFailPanel(CommonGameDamage, gameTime);
            }
        }

    }

    public void IncreaseEnemyDied(GameObject enemyDied)
    {
        DiedEnemyCount++;
        enemyPucks.Remove(enemyDied);
        canvas.UpdateKillInfo(DiedEnemyCount, StartEnemyCount);
        OnChangeSpecialPucks?.Invoke();
    }

    public void FocusCameraOnPlayer()
    {
        camera.SetCameraParking(player.transform);
        camera.SetCameraZoom(defaultCameraZoom);

    }


    private bool MayCameraPursue(Transform target)
    {
        float currentDistance = 0;
        if (player!=null) currentDistance = Vector2.Distance(player.transform.position, target.position);
        if (currentDistance > distancePursueCamera)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// После первого клика сам отписывается от джойстиков
    /// </summary>
    private void StartGameTimer()
    {
        matchSeconds = 0;
        timerRoutine = StartCoroutine(StartTimer());
        FloatingJoystick.OnClickDown -= StartGameTimer;
        IEnumerator StartTimer()
        {
            while (true)
            {
                matchSeconds += Time.deltaTime;
                yield return null;
            }
        }
    }

    [Serializable]
    public struct LevelState
    {
        public string modeName;
        [Tooltip("Интервальные бонусы в секундах , начиная с большого интервала времени (Минимальный бонус)")]
        public int[] bonusTimes;
        [Tooltip("В зависимости от LevelMode будет выбрана активная секция")]
        public GameObject enemySection;
        [Tooltip("Необходимое число очков для трех звезд")]
        public int requierdRecord;

    }



    /// <param name="timeRecods">Общая запись времени матча</param>
    /// <param name="damage">Урон полученный за матч</param>
    /// <param name="bonus">Множитель в соответствии с тайминговой секцией</param>
    /// <param name="conclude">сколько в итоге все посчитанно</param>
    /// <returns>Возвращает количество полностью заполненных звездочек</returns>
    public int CountAllBonuses(TimeSpan timeRecods, int damage, ref float bonus, ref int conclude)
    {

        for (int i = 0; i < lvlStates[levelMode].bonusTimes.Length; i++)
        {
            if (lvlStates[levelMode].bonusTimes[i] > timeRecods.TotalSeconds)
            {
                bonus = i;
            }

        }
        switch (bonus)
        {
            case -1: bonus = 1f; break;
            case 0: bonus = 1.3f; break;
            case 1: bonus = 1.6f; break;
            case 2: bonus = 2; break;
        }

         conclude = (int)(damage * bonus);
        if (conclude >= lvlStates[levelMode].requierdRecord)
        {
            //TODO: РЕАЛИЗОВАТЬ АЧИВКУ ЕСЛИ ОЧКИ ДОСТИГЛИ МАКСИМУМА В МАТЧЕ
            conclude = lvlStates[levelMode].requierdRecord;
        }

        return conclude / (lvlStates[levelMode].requierdRecord / 3);
    }

}


