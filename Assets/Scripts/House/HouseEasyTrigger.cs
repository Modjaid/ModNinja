using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HouseEasyTrigger : MonoBehaviour
{
    [SerializeField] private Transform posForTeleport;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == (int)Layer.Player)
        {
            foreach(GameObject enemy in FieldManager.instance.enemyPucks)
            {
                enemy.transform.position = posForTeleport.position;
                enemy.GetComponent<FatPuck>().enabled = true;
                enemy.GetComponent<NavMeshAgent>().enabled = true;
               // fat.autoController.Stop();
               // fat.autoController.StartMoveAndPush(fat, fat.playerPuck, 1000, fat.timerForPush);
            }
            Destroy(this.gameObject);
        }
    }
}
