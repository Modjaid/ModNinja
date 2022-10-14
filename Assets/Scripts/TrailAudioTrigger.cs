using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource trailAudio;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == (int)Layer.Player)
        {
           if(!trailAudio.isPlaying) trailAudio.Play();
        }
    }
}
