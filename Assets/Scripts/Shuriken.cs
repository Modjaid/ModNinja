using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuriken : MonoBehaviour
{
    [SerializeField] private int collisionForDestroy;
    [SerializeField] private Layer targetLayer;
    [SerializeField] public float damage;

    public void OnCollisionEnter2D(Collision2D collision)
    {
        Layer layer = (Layer)collision.gameObject.layer;

        if(layer == Layer.StaticObj)
        {
            collisionForDestroy--;
            if (collisionForDestroy < 0)
            {
                Liquidation();
            }
        }
        else if (layer == targetLayer)
        {
            collision.gameObject.GetComponent<PuckManager>().GetDamaged(damage);
            FieldManager.instance.TrySlowMotion(collision.transform);
            Liquidation();
        }
    }
    public void Liquidation()
    {
        Destroy(this.gameObject);
    }
}
