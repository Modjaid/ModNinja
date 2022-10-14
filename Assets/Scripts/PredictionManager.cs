using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PredictionManager : Singleton<PredictionManager>
{
    public GameObject obstacles;
    public int maxIterations;

    Scene currentScene;
    Scene predictionScene;

    PhysicsScene currentPhysicsScene;
    PhysicsScene predictionPhysicsScene;

    List<GameObject> dummyObstacles = new List<GameObject>();

    LineRenderer lineRenderer;
    GameObject dummy;

    public GameObject TEST;

    void Start(){
        Physics.autoSimulation = false;

        currentScene = SceneManager.GetActiveScene();
        currentPhysicsScene = currentScene.GetPhysicsScene();

        CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        predictionScene = SceneManager.CreateScene("Prediction", parameters);
        predictionPhysicsScene = predictionScene.GetPhysicsScene();

        lineRenderer = GetComponent<LineRenderer>();
    }

    void FixedUpdate(){
        if (currentPhysicsScene.IsValid()){
            currentPhysicsScene.Simulate(Time.fixedDeltaTime);
        }

        predict(TEST, Vector2.zero, Vector2.up * 20);
    }

    public void copyAllObstacles(){
        foreach(Transform t in obstacles.transform){
            if(t.gameObject.GetComponent<Collider>() != null){
                GameObject fakeT = Instantiate(t.gameObject);
                fakeT.transform.position = t.position;
                fakeT.transform.rotation = t.rotation;
                Renderer fakeR = fakeT.GetComponent<Renderer>();
                if(fakeR){
                    fakeR.enabled = false;
                }
                SceneManager.MoveGameObjectToScene(fakeT, predictionScene);
                dummyObstacles.Add(fakeT);
            }
        }
    }

    void killAllObstacles(){
        foreach(var o in dummyObstacles){
            Destroy(o);
        }
        dummyObstacles.Clear();
    }

    public void predict(GameObject subject, Vector2 currentPosition, Vector2 force){
        if (currentPhysicsScene.IsValid() && predictionPhysicsScene.IsValid()){
            subject.SetActive(false);
            if (dummy == null){
                dummy = Instantiate(subject);
                SceneManager.MoveGameObjectToScene(dummy, predictionScene);
                Debug.Log($"dummy: {subject}");
            }

            dummy.transform.position = currentPosition;
            dummy.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
            lineRenderer.positionCount = 0;
            lineRenderer.positionCount = maxIterations;


            for (int i = 0; i < maxIterations; i++){
                predictionPhysicsScene.Simulate(Time.fixedDeltaTime);
                lineRenderer.SetPosition(i, dummy.transform.position);
            }
            subject.SetActive(true);
            Destroy(dummy);
        }
    }

    void OnDestroy(){
        killAllObstacles();
    }
   


}
