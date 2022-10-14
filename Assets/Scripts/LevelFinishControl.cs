using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFinishControl : MonoBehaviour
{
    private FieldManager fieldManager;
    void Start()
    {
        fieldManager = GetComponent<FieldManager>();
        fieldManager.OnChangeSpecialPucks += WatchForChangesInPucks;
    }

   public void WatchForChangesInPucks()
    {
        if (!fieldManager.player)
        {
            fieldManager.EndGame(false,1.5f);
            fieldManager.OnChangeSpecialPucks -= WatchForChangesInPucks;
        }
        if(fieldManager.enemyPucks.Count == 0)
        {
            fieldManager.EndGame(true,1f);
            fieldManager.OnChangeSpecialPucks -= WatchForChangesInPucks;
        }
    }
}
