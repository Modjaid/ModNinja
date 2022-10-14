using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextDamage : MonoBehaviour
{
    private Animation anim;

    void Start()
    {
        anim = GetComponent<Animation>();
    }

    
    void Update()
    {
        if (!anim.isPlaying)
        {
            Destroy(this.gameObject, 1);
        }
    }
}
