using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер шайбы игрока
/// </summary>
public class PlayerPuck : PuckManager, IPushable, IJumpable, IThrowable
{
    public event Action<float> OnPlayerHit;

    [Header("Player")]
    [SerializeField] private float pushForce;
    [SerializeField] private float pushMass;
    [SerializeField] private float pushDrag;

    [Header("Shuriken")]
    [SerializeField] private float shurikenImpulse;
    [SerializeField] private float shurikenMass;
    [SerializeField] private float shurikenDamage;
    [Range(0, 100)]
    [SerializeField] private int shurikenCount;
    [Range(0,20)]
    [Tooltip("Время производство одного сюрикена")]
    [SerializeField] public float shurikenGenerationTimer;
    [SerializeField] private GameObject shurikenPrefab;

    [Header("For Jumper")]
    [SerializeField] private TrailRenderer defaultTrail1;
    [SerializeField] private TrailRenderer defaultTrail2;
    [SerializeField] private TrailRenderer coolTrail;
    [SerializeField] private TrailRenderer fireTrail;
    [SerializeField] private Collider2D triggerCollider;

    [SerializeField] private GameObject flashEffect;
    [SerializeField] private GameObject collisionEffect;
    [SerializeField] private GameObject boomCollisionEffect;
    [SerializeField] private GameObject jumpActiveIndicator;

    [Header("For AutoJoystick")]
    [SerializeField] private float rayOffsetAngle;
    [SerializeField] private float normalizeLength;
    [SerializeField] private int rayCount;
    [SerializeField] private LayerMask layerMask;

    [Header("От удара будет больше прибавлятся джамперов")]
    [SerializeField] private float produceJumpersPotential;

    [Header("Скорость преследования до побежденного (Для эффектности при добивании в замедленной съемки)")]
    [SerializeField] private float speedReachingForTheDead = 10;

    [SerializeField] public PuckElements elements;
    [Header("Звуки")]
    [SerializeField] private AudioSource killEnemy;
    [SerializeField] private AudioSource pushAudio;
    [SerializeField] private AudioSource racketAudio;
    [SerializeField] private AudioSource[] hitAudio;
    [SerializeField] private AudioSource[] collisionAudio;


    private Coroutine pushing; 
    private Coroutine joystickClickCounting;
    private Coroutine trailFlaming;

    public new void Start()
    {
        base.Start();
        elements.shurikens = shurikenCount;
        elements.OnChangeJumpers += TriggerSwitcher;
        elements.jumpers += GameManager.instance.Jumpers;
        StartCoroutine(GenerateShurikens());
    }

    public new void Update()
    {
        base.Update();
    }
    /// <summary>
    /// Подписан на события OnFingerUp Joystick
    /// </summary>
    public void PushByEvent(float force, Vector2 direction, PointerEventData eventData)
    {

        Debug.DrawRay(transform.position, direction.normalized * pushForce * force,Color.red);
        if (!stunned)
        {
            if (pushing != null) StopCoroutine(pushing); 
            pushing = StartCoroutine(Pushing());
            pushAudio.Play();

            Vector2 autoDirection;
            if(TryGetNearEnemy(direction,out autoDirection))
            {
                rigidBody.AddForce(autoDirection.normalized * pushForce * force, ForceMode2D.Impulse);
            //    Debug.Log("AUTOCORRECT DIRECTION");
            }
            else
            {
            //    Debug.Log("NOCORRECT");
                rigidBody.AddForce(GetStraightDir(direction, pushForce * force), ForceMode2D.Impulse);
            }
            CountQuickClicks();
        }
    }

    #region Hide
    public IEnumerator Pushing()
    {
        rigidBody.mass = pushMass;
        rigidBody.drag = pushDrag;
        yield return new WaitForSeconds(.2f);
        yield return new WaitUntil(() => rigidBody.velocity.magnitude < Utils.RESET_FORCE);
        RigidBodyInspectorUpd();
    }


    public void OnCollisionEnter2D(Collision2D collision)
    {
        var enemyPuck = collision.gameObject.GetComponent<PuckManager>();
       // if (collision.gameObject.layer == (int)Layer.Enemy)
       if(enemyPuck)
        {
            Instantiate(collisionEffect, null).transform.position = collision.contacts[0].point;
            float playerDamage = CountDamage(collision.relativeVelocity.magnitude);
            float enemyDamage = enemyPuck.CountDamage(collision.relativeVelocity.magnitude);

            if(playerDamage > enemyDamage)
            {
                PlayHitAudio();
                OnPlayerHit?.Invoke(playerDamage);
                PlayHitAnimation();
                RecordDamage(playerDamage);
                bool isEnemyDied = enemyPuck.GetDamaged(playerDamage);
                spriteRotation.TakeLookAtEnemyByTime(collision.transform,100,100);
                
                if (isEnemyDied)
                {
                    OnPlayerHit?.Invoke(5000);
                    killEnemy?.Play();
                    FieldManager.instance.SlowMotion(enemyPuck.transform);
                    StartCoroutine(ReachingForTheDead(enemyPuck.transform));
                }
                else
                {
                    FieldManager.instance.TrySlowMotion(enemyPuck.transform);
                }
                elements.IncreaseJumpers(playerDamage, produceJumpersPotential);
                BoomEffect(collision.contacts[0].point, playerDamage);
            }
            else
            {
                hitAudio[UnityEngine.Random.Range(7, 10)].Play();
                GetDamaged(enemyDamage);
              //  enemyPuck.PlayHitAnimation();
                BoomEffect(collision.contacts[0].point, enemyDamage);
            }
        }

       if(collision.collider.tag == "StreetObj")
        {
            collisionAudio[UnityEngine.Random.Range(0, 4)].Play();
        }

    }

    /// <summary>
    /// Предварительный триггер с рассчетом, следует ли джампироваться
    /// </summary>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        var enemyPuck = collision.gameObject.GetComponent<PuckManager>();
        if (enemyPuck)
        {
            float playerDamage = CountDamage();
            float enemyDamage = enemyPuck.CountDamage();

            if (enemyDamage > playerDamage)
            {
                TryToJump(enemyDamage);
            }
        }
        else
        {
            var streetObj = collision.GetComponent<StreetObject>();
            if (streetObj)
            {
                float damage = streetObj.TryGetDamage();
                if(damage > 1)
                {
                    TryToJump(damage);
                }
            }
        }
        
    }
    public override void Stop()
    {
        spriteRotation.StopAllCoroutines();
        StopAllCoroutines();
    }

    protected override void Liquidation()
    {
        FieldManager.instance.PlayerIsDied();
        stunned = true;
        collider.enabled = false;
        base.Liquidation();
        triggerCollider.enabled = false;
    }

    private IEnumerator ReachingForTheDead(Transform enemy)
    {
        while (Time.timeScale < 0.9f)
        {
            transform.position = Vector2.Lerp(transform.position, enemy.position, speedReachingForTheDead * Time.deltaTime);
            yield return null;
        }
    }

    public void TryToJump(float cost)
    {
        Vector2 newPos;
        if (elements.TryToGetJumpPos(cost, transform.position, out newPos))
        {
            untouchable = true;
            Instantiate(flashEffect, null).transform.position = transform.position;
            Instantiate(flashEffect, null).transform.position = newPos;
            pushAudio.Play();
            SwitchTrails(false);
            collider.enabled = false;
            transform.position = newPos;

            StartCoroutine(Return());

        }

        IEnumerator Return()
        {
            yield return new WaitForSeconds(0.01f);
            collider.enabled = true;
            yield return new WaitForSeconds(0.99f);
            untouchable = false;
            yield return new WaitForSeconds(1f);
            SwitchTrails(true);
        }

        void SwitchTrails(bool isActive)
        {
            defaultTrail1.emitting = isActive;
            defaultTrail2.emitting = isActive;
            coolTrail.emitting = !isActive;
        }
    }

    public void JumpToPos(Vector3 newPos)
    {

        untouchable = true;
        Instantiate(flashEffect, null).transform.position = transform.position;
        Instantiate(flashEffect, null).transform.position = newPos;
        SwitchTrails(false);
        collider.enabled = false;
        transform.position = newPos;

        StartCoroutine(Return());

        IEnumerator Return()
        {
            yield return new WaitForSeconds(0.01f);
            collider.enabled = true;
            yield return new WaitForSeconds(0.99f);
            untouchable = false;
            yield return new WaitForSeconds(1f);
            SwitchTrails(true);
        }

        void SwitchTrails(bool isActive)
        {
            defaultTrail1.emitting = isActive;
            defaultTrail2.emitting = isActive;
            coolTrail.emitting = !isActive;
        }
    }

    /// <summary>
    /// Данный метод подписан на elements.OnChangeJumpers
    /// отключае триггер отвечающий за джампер
    /// </summary>
    public void TriggerSwitcher(float currentJumpers)
    {

        if (currentJumpers >= 1f)
        {
            triggerCollider.enabled = true;
            jumpActiveIndicator.SetActive(true);
        }
        else
        {
            triggerCollider.enabled = false;
            jumpActiveIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Считывает молниеносные клики на джойстик для уведомления что пуш приумножается, таймер между
    /// кликами слишком короткий потому функция рассчитана на замедление времени
    /// </summary>
    private void CountQuickClicks()
    {
        if (joystickClickCounting == null) // Старт счетчика быстрых кликов
        {
            joystickClickCounting = StartCoroutine(Utils.StartTimer(0.1f, () => joystickClickCounting = null));
            FieldManager.instance.PushTextActive(true);
        }
        else // счетчик быстрых кликов продолжает принимать новые значения
        {
            if (trailFlaming != null) StopCoroutine(trailFlaming);

            //Перезапуск таймера в целях чтобы счетчик не сбрасывался на исходную
            StopCoroutine(joystickClickCounting);
            joystickClickCounting = StartCoroutine(Utils.StartTimer(0.1f, () => joystickClickCounting = null));


            fireTrail.emitting = true;
            if (!racketAudio.isPlaying) racketAudio.Play();
            StartCoroutine(Utils.StartTimer(3, () => fireTrail.emitting = false));
            FieldManager.instance.PushTextActive();
            FieldManager.instance.FocusCameraOnPlayer();
        }
    }

    private void BoomEffect(Vector2 pos,float damage)
    {
        if(damage > 200)
        {
            Instantiate(boomCollisionEffect).transform.position = pos;
        }
    }

    public void PushShuriken(float force, Vector2 direction, PointerEventData eventData)
    {
        if (eventData.delta != Vector2.zero && !stunned)
        {
            Throw(direction);
        }
    }
    public void Throw(Vector2 direction)
    {
        if (elements.TryGetOneShuriken())
        {
            animator.SetTrigger("Shuriken");
            var rot = Quaternion.Euler(0, 0, 0);
            var shuriken = Instantiate(shurikenPrefab, transform.position, rot);
            shuriken.GetComponent<Shuriken>().damage = shurikenDamage;
            shuriken.GetComponent<Rigidbody2D>().mass = shurikenMass;
            shuriken.GetComponent<Rigidbody2D>().AddForce(direction.normalized * shurikenImpulse, ForceMode2D.Impulse);
        }
    }
    private IEnumerator GenerateShurikens()
    {
        while (true)
        {
            yield return new WaitForSeconds(shurikenGenerationTimer);
            elements.IncreaseShuriken();

        }
    }

    private void RecordDamage(float damage)
    {
        FieldManager.instance.CommonGameDamage += (int)damage;
    }
    #endregion Hide

    private bool TryGetNearEnemy(Vector2 direction, out Vector2 newDirection)
    {
        newDirection = Vector2.zero;
        if (FieldManager.instance.SlowMotionIsGoing())
        {
            return false;
        }

        float angleOfBetweenRays = (rayOffsetAngle * 2) / rayCount;
        float currentOffset = -rayOffsetAngle;
        List<Vector2> targets = new List<Vector2>();
        for (int i = 0; i < rayCount; i++)
        {
            Vector2 currentRayDir = Quaternion.Euler(new Vector3(0, 0, currentOffset)) * direction * normalizeLength;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentRayDir, 90, layerMask);
            if(hit.collider != null)
            {
                if (hit.collider.gameObject.layer == (int)Layer.Enemy)
                {
                    targets.Add(hit.transform.position);
                }
            }

            //Debug.DrawRay(transform.position, Quaternion.Euler(new Vector3(0, 0, currentOffset)) * dir * normalizeLength, Color.red, 0.1f);
            currentOffset += angleOfBetweenRays;
        }



        float minDistance = 1000;
        Vector2 selectedTarget = Vector2.zero;
        foreach (Vector2 target in targets)
        {
            var distance = Vector2.Distance(transform.position, target);
            if (distance < minDistance)
            {
                selectedTarget = target;
                minDistance = distance;
            }
        }


        if (selectedTarget != Vector2.zero)
        {
            newDirection = selectedTarget - (Vector2)transform.position;
            Debug.DrawRay(transform.position, selectedTarget - (Vector2)transform.position, Color.red, 3f);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void PlayHitAudio()
    {
        hitAudio[UnityEngine.Random.Range(0, 5)].Play();
        int tryVocal = UnityEngine.Random.Range(0, 7);
        if (tryVocal > 4)
        {
            hitAudio[tryVocal].Play();
        }
    }

    private Vector2 GetStraightDir(Vector2 dir,float addForce)
    {
        var enemies = Physics2D.OverlapCircleAll(transform.position, 100, LayerMask.GetMask("Enemy"));

      //  var normal = new Vector2(Mathf.Round(dir.x), Mathf.Round(dir.y));
      //
      //  if (normal.x == 0) normal.x = (Math.Abs(dir.x) - (Math.Abs(dir.x) - 0.05f)) * Math.Sign(dir.x);
      //  if (normal.y == 0) normal.y =(Math.Abs(dir.y) - (Math.Abs(dir.y) - 0.05f)) * Math.Sign(dir.y);

      //  Debug.Log($"Length Vector ={normal.magnitude * addForce}");

        if (enemies.Length == 0)
        {
            return dir.normalized * (addForce/1.5f);
        }

        return dir.normalized * addForce;

    }

}
