using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HealPuck : PuckManager
{
    [SerializeField] private AutoController autoController;

    public new void Start()
    {
        base.Start();
        spriteRotation.SwitchRBForwardAlways(true);
    }
    public new void Update()
    {
        
    }

    /// <summary>
    /// Получает урон, и хилит игрока
    /// </summary>
    public override bool GetDamaged(float damage)
    {
        FieldManager.instance.player.GetComponent<PlayerPuck>().ConvertDamageToHealth(damage);
       return base.GetDamaged(damage);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == (int)Layer.Player)
        {
           var isGoing = autoController.TryRunAwayFrom(collision);
        }
    }
    public override void Stop()
    {
        autoController.Stop();
        spriteRotation.StopAllCoroutines();
        StopAllCoroutines();
    }



}
