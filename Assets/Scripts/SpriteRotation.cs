using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRotation : MonoBehaviour
{
    [SerializeField] private Rigidbody2D puckRB;

    /// <summary>
    /// ВКЛ/ВЫКЛ поворот вперед в сторону движения (rigidBody), есть автовыключение при скорости == 0
    /// </summary>
    /// <param name="IsActive"></param>
    public void SwitchRBRotator(bool IsActive)
    {
        StopAllCoroutines();

        if (IsActive)
        {
            StartCoroutine(RigidBodyForward());
        }
    }
    /// <summary>
    /// ВКЛ/ВЫКЛ поворот вперед в сторону движения (rigidBody), автовыключение отсутствует
    /// </summary>
    public void SwitchRBForwardAlways(bool IsActive)
    {
        StopAllCoroutines();

        if (IsActive)
        {
            StartCoroutine(RigidBodyForwardAlways());
        }
    }

    /// <summary>
    /// ВКЛ/ВЫКЛ поворот на цель (Transform), автовыключение отсутствует
    /// </summary>
    public void SwitchTargetAlways(Transform target = null)
    {
        StopAllCoroutines();

        if (target != null)
        {
            StartCoroutine(TargetAlways(target));
        }
    }

    public void TakeLookAtEnemyByTime(Transform target, float endTimeForLook, float speedRotation = 0.1f)
    {
        StopAllCoroutines();

        StartCoroutine(TargetAlways(target, speedRotation));
        StartCoroutine(Delay());
        IEnumerator Delay()
        {
            yield return new WaitForSeconds(endTimeForLook);
            SwitchRBForwardAlways(true);
        }
    }

    private IEnumerator RigidBodyForward()
    {
        var velocity = puckRB.velocity;

        yield return new WaitForSeconds(.01f);

     //   Debug.Log("Пошла корутина " + velocity);
        while (velocity != Vector2.zero)
        {
            // Скорость смещения объекта
            velocity = puckRB.velocity;

            //AIM
            var target = new Vector3(velocity.x, velocity.y, 0);

            //ROT
            float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
            var rot = Quaternion.AngleAxis(angle, Vector3.forward);

            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Utils.SPEED_ROTATION);
            yield return null;
        }
    }

    private IEnumerator RigidBodyForwardAlways()
    {

        while (true)
        {
            // Скорость смещения объекта
            var velocity = puckRB.velocity;

            //AIM
            var target = new Vector2(velocity.x, velocity.y);

            //ROT
            float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
            var rot = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * Utils.SPEED_ROTATION);
            yield return null;
        }
    }

    private IEnumerator TargetAlways(Transform target,float speedRotation = 0.1f)
    {
        while (target != null)
        {
            transform.right = Vector2.Lerp(transform.right, (target.position - transform.position).normalized, Time.deltaTime * speedRotation);
            yield return null;
        }
    }
    public void LookAt(Vector3 target)
    {
        transform.right = target;
    }
    private IEnumerator Centr()
    {

        float restRotation = 1;
        while (restRotation > 0.2f)
        {
            var targetDir = new Vector3(-transform.position.x, -transform.position.y, 0);
            //ROT
            float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
            var rot = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * Utils.SPEED_ROTATION);
            restRotation = Quaternion.Angle(transform.rotation, rot);
            yield return null;
        }
    }



}
