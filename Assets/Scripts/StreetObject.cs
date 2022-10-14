using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Объекты могут наносить вред и врагам и игроку, определяют их по тэгу "Player"  "Enemy"
/// </summary>
public class StreetObject : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rigidBody;
    [SerializeField] protected float damage;
    [SerializeField] protected float rbSpeedForActive = 3;
    [SerializeField] public bool isActive;

    protected Coroutine controlFalseActive;


    /// <summary>
    /// Для предскозаний джампера шайбы игрока
    /// </summary>
    /// <returns></returns>
    public float TryGetDamage()
    {
        if (isActive)
        {
            return damage;
        }
        return 0;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        var puck = collision.gameObject.GetComponent<PuckManager>();
        if (puck && !(puck is HealPuck))
        {
            puck.GetDamaged(damage);
            puck.rigidBody.AddForce(new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) * 250, ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// Выключит активность наносить дамаг когда скорость РБ будет меньше rbSpeedForActive
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator AutoControlActive()
    {
         yield return new WaitForSeconds(0.1f);
        isActive = true;

         yield return new WaitUntil(() => rigidBody.velocity.magnitude <= rbSpeedForActive);
        isActive = false;
    }

}
