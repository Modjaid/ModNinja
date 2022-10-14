using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MainCamera : MonoBehaviour
{
    [SerializeField] private float defaultMoveSpeed = 2f;
    [SerializeField] private float defaultZoomSpeed = 2f;
    private Coroutine cameraParking;
    private Coroutine cameraZooming;

    private void Update()
    {
        
    }

    /// <summary>
    /// Если distanceForNext = 0 - Камера всегда будет преследовать цель
    /// </summary>
    public void SetCameraParking(Transform newTarget, float distanceForNext = 0, float speed = 4)
    {
        if (cameraParking != null) StopCoroutine(cameraParking);

        if (distanceForNext == 0)
        {
            cameraParking = StartCoroutine(CameraParkingAlways(newTarget, speed));
        }
        else
        {
            cameraParking = StartCoroutine(CameraParking(newTarget, speed, distanceForNext));
        }
    }
    public void SetCameraZoom(float zoomSize, float speed = 2)
    {

        Camera camera = GetComponent<Camera>();
        if (cameraZooming != null) StopCoroutine(cameraZooming);

        if(zoomSize > camera.orthographicSize)
        {
            cameraZooming = StartCoroutine(CameraUnZoom(camera,zoomSize, speed));
        }
        else
        {
            cameraZooming = StartCoroutine(CameraZoom(camera,zoomSize, speed));
        }

    }

    private IEnumerator CameraParking(Transform target, float currentSpeed, float distanceForStop)
    {

        float distance = float.MaxValue;
        while (distance > distanceForStop)
        {
            transform.position = Vector3.Lerp(transform.position, Get3DPos(target), Time.unscaledDeltaTime * currentSpeed);

            distance = Vector2.Distance(transform.position, target.position);
            yield return null;
        }
    }
    private IEnumerator CameraParkingAlways(Transform target, float currentSpeed)
    {

        float distance = float.MaxValue;
        while (true)
        {
            transform.position = Vector3.Lerp(transform.position, Get3DPos(target), Time.unscaledDeltaTime * currentSpeed);

            distance = Vector2.Distance(transform.position, target.position);
            yield return null;
        }
    }

    private IEnumerator CameraUnZoom(Camera camera,float zoomSize, float speed)
    {
        float distance = float.MaxValue;
        while (distance > 0.1f)
        {
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, zoomSize, speed * Time.unscaledDeltaTime);
            distance = Mathf.DeltaAngle(camera.orthographicSize, zoomSize);
            yield return null;
        }
    }
    private IEnumerator CameraZoom(Camera camera, float zoomSize, float speed)
    {
        float distance = -float.MaxValue;
        while (distance < 0.1f)
        {
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, zoomSize, speed * Time.unscaledDeltaTime);
            distance = Mathf.DeltaAngle(camera.orthographicSize,zoomSize);
            yield return null;
        }
    }

    private Vector3 Get3DPos(Transform transform)
    {
        if (transform != null)
        {
            return new Vector3(transform.position.x, transform.position.y, -10);
        }
        else
        {
            return Vector3.zero;
        }
    }


}

public enum Layer
{
    Default = 0,
    Player = 8,
    StaticObj = 9,
    Enemy = 10
}
