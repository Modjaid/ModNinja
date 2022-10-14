using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionObject : StreetObject
{
    [SerializeField] private GameObject bagFire;
    [SerializeField] private GameObject collisionEffect;
    public float radius;
    public float force;
    [Tooltip("В точке соприкосновения шайба получает обычный дамаг, все кто в радиусе получают explosionDamage")]
    public float explosionDamage;



    public new void OnCollisionEnter2D(Collision2D collision)
    {

        if (isActive)
        {
            var puck = collision.gameObject.GetComponent<PuckManager>();
            if (puck)
            {
                puck.GetDamaged(damage);
            }

            Instantiate(collisionEffect).transform.position = collision.contacts[0].point;

            StartCoroutine(ExplosionForce(collision));
        }
        else
        {

            if (controlFalseActive != null) StopCoroutine(controlFalseActive);
            controlFalseActive = StartCoroutine(AutoControlActive());
        }
    }

    private IEnumerator ExplosionForce(Collision2D collision)
    {

        yield return new WaitForSeconds(0.2f);

        Collider2D[] overlapedColliders = Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < overlapedColliders.Length; i++)
        {
            var rb = overlapedColliders[i].attachedRigidbody;
            if (rb)
            {
                if (rb.gameObject.tag == "RbDynamic") rb.isKinematic = false;

                var direction = rb.transform.position - transform.position;
                rb.AddForce(direction * force);
                var puck = rb.GetComponent<PuckManager>();
                if (puck)
                {
                    puck.GetDamaged(explosionDamage);
                }
            }
        }
        Destroy(this.gameObject);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
