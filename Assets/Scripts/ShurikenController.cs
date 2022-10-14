using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShurikenController : MonoBehaviour
{
    [SerializeField] private GameObject shurikenPrefab;
    [SerializeField] private Transform fingerPos;
    [HideInInspector] public float forceImpulse;
    [HideInInspector] public float shurikenMass;
    [HideInInspector] public float shurikenDamage;
    [HideInInspector] public bool stunned = false;

    public void Start()
    {
        StartCoroutine(WorldRotationUpdate());
    }
    public void PushShuriken(float force,Vector2 direction, PointerEventData eventData)
    {
        if(eventData.delta != Vector2.zero && !stunned)
        {
            var rot = Quaternion.Euler(0, 0, 0);
            var shuriken = Instantiate(shurikenPrefab, transform.position, rot);
            shuriken.GetComponent<Shuriken>().damage = shurikenDamage;
            shuriken.GetComponent<Rigidbody2D>().mass = shurikenMass;
            shuriken.GetComponent<Rigidbody2D>().AddForce(direction.normalized * forceImpulse, ForceMode2D.Force);
        }
    }
    /// <summary>
    /// Нужен чтоб не сбивался угол поворота джойстика
    /// </summary>
    /// <returns></returns>
    private IEnumerator WorldRotationUpdate()
    {
        while (true)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            yield return null;
        }
    }

}
