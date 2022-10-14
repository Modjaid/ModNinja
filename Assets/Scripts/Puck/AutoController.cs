using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Автоматическое управление ботами (Мой ИИ)
/// </summary>
public class AutoController : MonoBehaviour
{
    #region Inspector
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private PuckManager puck;
    #endregion

    private void Start()
    {
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
    }


    /// <summary>
    /// Активируется когда цель достигла диапазона, затем всегда ее преследует, при нужной дистанции пушит
    /// </summary>
    /// <param name="pusher">Сам пушер</param>
    /// <param name="pursueTarget">цель которую надо преследовать</param>
    /// <param name="distanceForPursue">Нужная дистанция для пуша по цели</param>
    public void StartMoveAndPush(IPushable pusher, Transform pursueTarget, float distanceForPursue, float timerForPush)
    {
        StartCoroutine(Update());

        IEnumerator Update()
        {
            yield return StartCoroutine(WaitNeededDistance(pursueTarget, distanceForPursue));

            while (true)
            {

                yield return StartCoroutine(PursueTarget(pursueTarget));

                yield return StartCoroutine(pusher.Pushing());

                yield return new WaitForSeconds(timerForPush);
            }
        }
    }
    public void StartKungFu(KungFuPuck thisPuck,Transform targetPuck)
    {
        float waitTimeForPush = 0;
        StartCoroutine(SwitchPush());
        StartCoroutine(DistanceControl());
        StartCoroutine(ThrowShurikens());

        IEnumerator SwitchPush()
        {
            while (true)
            {
                float modeTime = Random.Range(2, 5);
                Coroutine pushing = StartCoroutine(PushInToRandomDirection());
                yield return new WaitForSeconds(modeTime);
                StopCoroutine(pushing);
                pushing = StartCoroutine(PushInToTarget());
                modeTime = Random.Range(2, 5);
                yield return new WaitForSeconds(modeTime);
                StopCoroutine(pushing);
            }
        }

        IEnumerator PushInToTarget()
        {
            while (true)
            {
                yield return new WaitForSeconds(waitTimeForPush);

                thisPuck.PushByEvent(2,targetPuck.position - transform.position);
                Debug.Log("ATTACK");
            }
        }

        IEnumerator DistanceControl()
        {
            while (true)
            {
                var distance = Vector2.Distance(transform.position, targetPuck.position);
                if(distance < 50)
                {
                    waitTimeForPush = 0.2f;
                }
                else
                {
                    waitTimeForPush = Random.Range(0.6f, 1.5f);
                }
                yield return null;
            }
        }

        IEnumerator PushInToRandomDirection()
        {
            while (true)
            {
                yield return new WaitForSeconds(waitTimeForPush);
                thisPuck.PushByEvent(2, Random.insideUnitCircle.normalized);
                Debug.Log("WALK");
            }
        }

        IEnumerator ThrowShurikens()
        {
            while (true)
            {
                if (thisPuck.elements.shurikens >= 3)
                {
                    while (thisPuck.elements.shurikens > 0)
                    {
                        thisPuck.Throw(targetPuck.position - transform.position);
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                yield return null;
            }
        }
    }

    /// <summary>
    /// Временное отключение автоуправления (с автовключением)
    /// </summary>
    public void Stop()
    {
        StopAllCoroutines();
        navMeshAgent.isStopped = false;
        navMeshAgent.enabled = false;
    }
        

    public bool TryRunAwayFrom(Collider2D target)
    {
        var targetPos = Vector3.zero;

        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * 300.1f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1, NavMesh.AllAreas))
            {

                if (target.bounds.Contains(hit.position) == false)
                {
                    targetPos = hit.position;
                    navMeshAgent.SetDestination(targetPos);
                    return true;
                }
            }
        }
        return false;
    }


    private IEnumerator WaitNeededDistance(Transform target, float neededDistance)
    {
        var currentDistance = Vector3.Distance(target.position, this.transform.position);
        while (currentDistance > neededDistance)
        {
            currentDistance = Vector3.Distance(target.position, this.transform.position);
            yield return null;
        }
    }

    private IEnumerator PursueTarget(Transform target)
    {
        // while (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance + 1)
        if (target == null) yield break;
        navMeshAgent.SetDestination(target.position);
        navMeshAgent.isStopped = false;

        while (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance || navMeshAgent.remainingDistance == 0)
        {

            navMeshAgent.SetDestination(target.position);

            puck.spriteRotation.LookAt(navMeshAgent.velocity.normalized);
            yield return null;
        }

        navMeshAgent.isStopped = true;
    }



}
