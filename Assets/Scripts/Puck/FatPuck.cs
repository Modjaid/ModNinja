using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// Менеджер шайбы жирдяя
/// </summary>
public class FatPuck : PuckManager, IPushable
{
    [Header("Fat")]
    [Space(10)]
    [SerializeField] private float pushForce;
    [SerializeField] private float pushMass;
    [SerializeField] private float pushDrag;

    [Header("AI params")]
    [SerializeField] public AutoController autoController;
    [Tooltip("Дистанция необходимая до врага для преследования")]
    [SerializeField] public float distanceForPursue;
    [Tooltip("Цикличный таймер через какое время будет пуш ( 0 - таймер офф)")]
    [SerializeField] public float timerForPush;
    [Tooltip("Время для Разгона на старте")]
    [SerializeField] private float startPushTime;

    [HideInInspector] public Transform playerPuck;
    private Vector3 targetPos;


    public new void Start()
    {
        base.Start();
        playerPuck = FieldManager.instance.player.transform;

        autoController.StartMoveAndPush(this, playerPuck, distanceForPursue, timerForPush);
    }

    public new void Update()
    {
        base.Update();
    }

    public IEnumerator Pushing()
    {
        yield return new WaitUntil(() => stunned == false);
        spriteRotation.TakeLookAtEnemyByTime(playerPuck,startPushTime,5f);

        yield return new WaitForSeconds(startPushTime);


        rigidBody.mass = pushMass;
        rigidBody.drag = pushDrag;
        rigidBody.AddForce(((Vector2)spriteRotation.transform.right) * pushForce, ForceMode2D.Impulse);
        spriteRotation.SwitchRBRotator(true);

        yield return new WaitForSeconds(.2f);

        yield return new WaitUntil(() => rigidBody.velocity.magnitude < Utils.RESET_FORCE);

        RigidBodyInspectorUpd();
    }

    protected override void Liquidation()
    {
        autoController.Stop();
        FieldManager.instance.IncreaseEnemyDied(this.gameObject);
        base.Liquidation();
    }

    public override void Stop()
    {
        autoController.Stop();
        spriteRotation.StopAllCoroutines();
        StopAllCoroutines();
    }
}
