using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class KungFuPuck : PuckManager , IPushable, IJumpable, IThrowable
{
    [SerializeField] private AutoController autoController;
    [Header("KungFu")]
    [SerializeField] private float pushForce;
    [SerializeField] private float pushMass;
    [SerializeField] private float pushDrag;

    [Header("Shuriken")]
    [SerializeField] private float shurikenImpulse;
    [SerializeField] private float shurikenMass;
    [SerializeField] private float shurikenDamage;
    [Range(0, 100)]
    [SerializeField] private int shurikenCount;
    [Range(0, 20)]
    [Tooltip("Время производство одного сюрикена")]
    [SerializeField] public float shurikenGenerationTimer;
    [SerializeField] private GameObject shurikenPrefab;

    [Header("For Jumper")]
    [SerializeField] private TrailRenderer defaultTrail1;
    [SerializeField] private TrailRenderer defaultTrail2;
    [SerializeField] private TrailRenderer coolTrail;
    [SerializeField] private Collider2D triggerCollider;

    [SerializeField] private GameObject flashEffect;
    [SerializeField] private GameObject jumpActiveIndicator;

    [Header("От удара будет больше прибавлятся джамперов")]
    [SerializeField] private float produceJumpersPotential;


    [SerializeField] public PuckElements elements;

    private Coroutine pushing;
    private Transform targetPuck;

    public new void Start()
    {
        base.Start();
        targetPuck = FieldManager.instance.player.transform;
        elements.shurikens = shurikenCount;
        elements.OnChangeJumpers += TriggerSwitcher;
        StartCoroutine(GenerateShurikens());
        autoController.StartKungFu(this, targetPuck);
    }


    public void PushByEvent(float force, Vector2 direction)
    {
        if (!stunned)
        {
            if (pushing != null) StopCoroutine(pushing);
            pushing = StartCoroutine(Pushing());
            rigidBody.AddForce(direction.normalized * pushForce * force, ForceMode2D.Impulse);
            spriteRotation.SwitchRBRotator(true);
        }

    }


    public IEnumerator Pushing()
    {
        rigidBody.mass = pushMass;
        rigidBody.drag = pushDrag;
        yield return new WaitForSeconds(.2f);
        yield return new WaitUntil(() => rigidBody.velocity.magnitude < Utils.RESET_FORCE);
        RigidBodyInspectorUpd();
    }


    /// <summary>
    /// Предварительный триггер с рассчетом, следует ли джампироваться
    /// </summary>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == (int)Layer.Player)
        {
            var enemyPuck = collision.gameObject.GetComponent<PuckManager>();
            float thisDamage = CountDamage();
            float playerDamage = enemyPuck.CountDamage();

            if (thisDamage < playerDamage)
            {
                TryToJump(thisDamage);
            }
        }
        else
        {
            var streetObj = collision.GetComponent<StreetObject>();
            if (streetObj)
            {
                float damage = streetObj.TryGetDamage();
                if (damage > 1)
                {
                    TryToJump(damage);
                }
            }
        }

    }

    protected override void Liquidation()
    {
        autoController.Stop();
        FieldManager.instance.IncreaseEnemyDied(this.gameObject);
        stunned = true;
        collider.enabled = false;
        base.Liquidation();
        triggerCollider.enabled = false;
    }


    public void TryToJump(float cost)
    {
        Vector2 newPos;
        if (elements.TryToGetJumpPos(cost, transform.position, out newPos))
        {
            untouchable = true;
            Instantiate(flashEffect, null).transform.position = transform.position;
            Instantiate(flashEffect, null).transform.position = newPos;
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

    public override void Stop()
    {
        autoController.Stop();
        spriteRotation.StopAllCoroutines();
        StopAllCoroutines();
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
        if (elements.TryGetOneShuriken() && !stunned)
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

}
