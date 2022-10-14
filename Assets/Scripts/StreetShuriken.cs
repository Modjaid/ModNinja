using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetShuriken : StreetObject
{
    [SerializeField] private Animation rotateAnim;
    [SerializeField] private SpriteRenderer colorShuriken;
    public new void OnCollisionEnter2D(Collision2D collision)
    {

        if (controlFalseActive != null) StopCoroutine(controlFalseActive);
        controlFalseActive = StartCoroutine(AutoControlActive());
        if (!isActive) return;

        var puck = collision.gameObject.GetComponent<PuckManager>();
        if (puck && !(puck is HealPuck))
        {
            puck.GetDamaged(damage);
            puck.rigidBody.AddForce(new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) * 250, ForceMode2D.Impulse);
        }
    }

    public override IEnumerator AutoControlActive()
    {
        yield return new WaitForSeconds(0.1f);
        isActive = true;
        rotateAnim.Play("ShurikenRotation");
        colorShuriken.color = Color.red;

        yield return new WaitUntil(() => rigidBody.velocity.magnitude <= rbSpeedForActive);
        isActive = false;
        rotateAnim.Stop();
        colorShuriken.color = Color.white;
    }
}
