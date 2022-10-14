using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LvlIntroFinishTrigger : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
       var player = collision.GetComponent<PlayerPuck>();
        if (player)
        {
            GameManager.instance.SaveLevelCompleted(3);
            GameManager.instance.OpenStartPanel = true;
            SceneManager.LoadScene("Menu");
        }
    }
}
