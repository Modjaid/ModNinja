using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LvlIntroEnemyDestroy : MonoBehaviour
{
    private void OnDestroy()
    {
        var lvlIntro = FieldManager.instance.gameObject.GetComponent<LvLIntro>();
        lvlIntro.anim.Play(lvlIntro.cutScenes[2]);
    }
}
