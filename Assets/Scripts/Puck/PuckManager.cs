using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Родитель всех шайб менеджеров: PlayerPuck, FatPuck....
/// </summary>
public abstract class PuckManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Animator animator;
    [SerializeField] public SpriteRotation spriteRotation;
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] public new Collider2D collider;
    [SerializeField] public PuckCanvas canvas;
    [SerializeField] public GameObject dieEffect;
    [SerializeField] public GameObject dieExposionEffect;

    [Header("Physics")]
    [SerializeField] protected float mass;
    [SerializeField] private float drag;
    [Header("Puck params")]
    [SerializeField] public float maxHealth;
    [SerializeField] public float damage;
    [Tooltip("Множитель хилки для самого Пака")]
    [SerializeField] public float treatBonuses;



    protected float health;

    public bool stunned = false;
    /// <summary>
    /// для отключения получения какого либо урона
    /// </summary>
    protected bool untouchable = false;
    private float force;

    public void Start()
    {
        health = maxHealth;
        RigidBodyInspectorUpd();
    }

    public void Update()
    {
        animator.SetFloat("SpeedMove", rigidBody.velocity.magnitude);
    }

    public void FixedUpdate()
    {
        UpdateLastForce();
    }
    public virtual void RigidBodyInspectorUpd()
    {
        rigidBody.mass = mass;
        rigidBody.drag = drag;
    }
    

    /// <returns>True- Если убило наповал к чертям собачьим</returns>
    public virtual bool GetDamaged(float damage)
    {
        if (untouchable) return false;
        health -= damage;
        canvas.UpdSlideBar(health);
        canvas.ActiveInfoText(true, (int)damage);

        if (health > 0)
        {
            StartCoroutine(Stunning());
            PlayFallAnimation(false);
            return false;
        }
        else
        {
            PlayFallAnimation(true);
            Liquidation();
            return true;
        }
    }

    /// <summary>
    /// Рандомно вызывает клипы удара у текущей шайбы
    /// </summary>
    public void PlayHitAnimation()
    {
        var randFallAnimTrack = UnityEngine.Random.Range(0, 100);
        animator.SetInteger("HitSelector", randFallAnimTrack);
        animator.SetTrigger("Hit");
    }

    public float CountDamage(float collisionForce)
    {
        if (stunned)
        {
            return 0;
        }
     //   var stabilizeMass = Math.Abs(newMass - mass) + mass;
        return (force * damage * collisionForce) / 1000;
    }
    public float CountDamage()
    {
        if (stunned)
        {
            return 0;
        }
        //   var stabilizeMass = Math.Abs(newMass - mass) + mass;
        return force * damage * 0.7f;
    }

    public abstract void Stop();

    public virtual void ConvertDamageToHealth(float damage)
    {
        float newHealths = damage * (treatBonuses / 300); 

        if (this.health + newHealths > maxHealth)
        {
            this.health = maxHealth;
        }
        canvas.UpdSlideBar(this.health);
        canvas.ActiveHealText((int)newHealths);
    }
    protected virtual void Liquidation()
    {
        collider.enabled = false;
        Instantiate(dieEffect, null).transform.position = transform.position;
        canvas.ActiveSlideBar(false);
        FieldManager.instance.TrySlowMotion(this.transform, 15, 0.1f, 1.5f, 30);
        StopAllCoroutines();
        StartCoroutine(DelayForDestroy());

        IEnumerator DelayForDestroy()
        {
            yield return new WaitForSeconds(3);
            Instantiate(dieExposionEffect, null).transform.position = transform.position;
            spriteRotation.StopAllCoroutines();
            Destroy(this.gameObject);
        }
    }

    protected IEnumerator Stunning()
    {
        stunned = true;
        // canvas.ActiveInfoText(true, "BLYATb");
        yield return new WaitForSeconds(.2f);

        yield return new WaitUntil(() => rigidBody.velocity.magnitude < Utils.RESET_FORCE);

        //  canvas.ActiveInfoText(false);
        stunned = false;
    }

    /// <summary>
    /// Триггер анимации падения
    /// </summary>
    /// <param name="isDeadFall">Либо анимация окончательная, либо с переходом на стойку</param>
    protected void PlayFallAnimation(bool isDeadFall)
    {
        string triggerName = (isDeadFall) ? "Death" : "Fall";


        var randFallAnimTrack = UnityEngine.Random.Range(1, 3);
        animator.SetInteger("FallSelector", randFallAnimTrack);
        animator.SetTrigger(triggerName);
    }



    /// <summary>
    /// чем выше force тем больше шанс отразить, а также нанести удар
    /// Контроль последнего форса нужен для правильной оценки коллизии, когда у всех значения будут по нулям
    /// </summary>
    private void UpdateLastForce()
    {
        if (rigidBody.velocity.magnitude > Utils.RESET_FORCE)
        {
            force = rigidBody.velocity.magnitude;
        }
    }


    // public static float CountDamageByForce(float force)
    // {
    //
    // }


}

/// <summary>
/// Дает шайбе возможность пуша (Данный интерфейс позволяет работать с AutoController)
/// </summary>
public interface IPushable
{
    IEnumerator Pushing();
}
/// <summary>
/// Дает шайбе возможность телепортироваться
/// </summary>
public interface IJumpable 
{
    /// <summary>
    /// Вызывать в OnTriggerEnter2D
    /// </summary>
    /// <param name="cost">Стоимость необходимая для прыжка, предпологается что это будет дамаг врага</param>
    void TryToJump(float cost);

    /// <summary>
    /// вызывать в апдейте для контроля Элемента jumpers, также может взять роль автогенератора
    /// </summary>
    /// <returns></returns>
    void TriggerSwitcher(float currentJumpers);
}
/// <summary>
/// Дает шайбе возможность выбрасывать предметы (Данный интерфейс позволяет работать с AutoController)
/// </summary>
public interface IThrowable 
{
    /// <summary>
    /// Бросает предмет (Сюрикен в заданном направлении)
    /// </summary>
    /// <param name="direction"></param>
    void Throw(Vector2 direction);
}


[Serializable]
public struct PuckElements
{
    public event Action<float> OnChangeJumpers;
    public event Action<int> OnChangeShurikens;
    [SerializeField] public float jumpers;
    [SerializeField] public int shurikens;

    /// <summary>
    /// За одно и убавляет очки jumpers если удачно нашлась позиция
    /// </summary>
    /// <returns>true - удачно ли нашлась позиция</returns>
    public bool TryToGetJumpPos(float enemyDamage, Vector2 currentPos, out Vector2 result)
    {
        result = Vector3.zero;

        float tax = enemyDamage / 500;
        if(jumpers < tax)
        {
            return false;
        }

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = currentPos + new Vector2(10,10) + UnityEngine.Random.insideUnitCircle * 35.1f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                jumpers -= tax;
                OnChangeJumpers?.Invoke(jumpers);
                Debug.Log("Удачно");
                return true;
            }
        }
        Debug.Log("Нет");
        return false;
    }

    public void IncreaseJumpers(float damage,float potential)
    {
        jumpers += damage * potential / 100;
        OnChangeJumpers?.Invoke(jumpers);
    }

    public bool TryGetOneShuriken()
    {
        if(shurikens > 0)
        {
            shurikens--;
            OnChangeShurikens?.Invoke(shurikens);
            return true;
        }
        else
        {
            return false;
        }
    }
    public void IncreaseShuriken()
    {
        shurikens++;
        OnChangeShurikens?.Invoke(shurikens);
    }
}

