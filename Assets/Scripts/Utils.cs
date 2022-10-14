using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public static class Utils
{
    public const float RESET_FORCE = 3;
    public const float SPEED_ROTATION = 0.1f;

    public static IEnumerator StartTimer(float timer, Action callback)
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        callback();
    }
    public static IEnumerator MoveToTarget(NavMeshAgent agent, Vector3 targetPos)
    {
        while (true)
        {
            agent.SetDestination(targetPos);
            yield return null;
        }
    }
    public static IEnumerator MoveToTarget(NavMeshAgent agent, Transform target)
    {
        while (true)
        {
            agent.SetDestination(target.position);
            yield return null;
        }
    }
}
