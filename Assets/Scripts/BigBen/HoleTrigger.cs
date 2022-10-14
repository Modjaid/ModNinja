using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleTrigger : MonoBehaviour
{
    [SerializeField] private float scaleSpeed;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        var puck = collision.GetComponent<PuckManager>();
        if (puck && !(puck is HealPuck))
        {
            if(puck is PlayerPuck && !puck.stunned)
            {
                (puck as PlayerPuck).TryToJump(0);
            }
            else
            {
                puck.GetDamaged(5000);
                puck.transform.parent = null;
                puck.animator.GetComponent<SpriteRenderer>().sortingOrder = 0;
                StartCoroutine(ResizeObj(puck.transform));
            }
        }else if(collision.gameObject.tag == "StreetObj")
        {
            StartCoroutine(ResizeObjWithDestroy(collision.transform));
        }
    }

    private IEnumerator ResizeObj(Transform obj)
    {
        while (obj)
        {
            obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, new Vector3(0.02f, 0.02f, 0.02f), scaleSpeed * Time.deltaTime);
            yield return null;
        }
    }
    private IEnumerator ResizeObjWithDestroy(Transform obj)
    {
        while (obj.transform.localScale.x >= 0.03)
        {
            obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, new Vector3(0.02f, 0.02f, 0.02f), scaleSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(obj.gameObject);
    }
}
