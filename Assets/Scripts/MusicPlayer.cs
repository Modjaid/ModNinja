using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource[] music;
    [SerializeField] private float minVolume;
    [SerializeField] private float minPitch;
    [SerializeField] private float increaseValue;
    public int currentTrack;

    private float currentPitch;
    public float State
    {
        get
        {
            return currentPitch;
        }
        set
        {
            if(value > 1)
            {
                currentPitch = 1;
                music[currentTrack].pitch = 1;
                music[currentTrack].volume = 1;
            }
            else
            {
                currentPitch = value;
                music[currentTrack].pitch = value;
                music[currentTrack].volume = value;
            }
        }
    }

    private Coroutine stunnig;

    public void Start()
    {
        Mix();
    }

    public void StartPlay()
    {
        if (stunnig != null) StopCoroutine(stunnig);
        State = 0;
        music[currentTrack].Play();

    }

    public void IncreasePitch(float countDamage)
    {
        State += countDamage * (increaseValue * 0.001f);

        if (stunnig != null) StopCoroutine(stunnig);
        stunnig = StartCoroutine(Stunning(12, 0.05f));
    }
    


    private void Mix()
    {
        System.Random rand = new System.Random();
        AudioSource[] arr = music;
        for (int i = arr.Length - 1; i >= 1; i--)
        {
            int j = rand.Next(i + 1);

            AudioSource tmp = arr[j];
            arr[j] = arr[i];
            arr[i] = tmp;
        }
        music = arr;
    }

    public void Stop(float delay, float speed = 0.3f)
    {
        if(stunnig != null) StopCoroutine(stunnig);
        stunnig = StartCoroutine(Stunning(delay, speed));
    }

    public void Pause(bool IsPause)
    {
        if (IsPause) music[currentTrack].Pause();
        else music[currentTrack].Play();
    }

    public void NextTrack()
    {
        StartCoroutine(OffPrevTrack(music[currentTrack]));
        if (currentTrack >= music.Length - 1)
        {
            currentTrack = 0;
            Mix();
        }
        else
        {
            currentTrack++;
        }

    }


    private IEnumerator Stunning(float delay,float stunSpeed)
    {
        yield return new WaitForSeconds(delay);
        while (State > 0.1f)
        {
            State -= Time.deltaTime * stunSpeed;
            yield return null;
        }
        State = 0;
        stunnig = null;
    }
    private IEnumerator OffPrevTrack(AudioSource prevTrack)
    {
        while (prevTrack.volume > 0.1f)
        {
            prevTrack.volume -= Time.deltaTime / 0.2f;
            yield return null;
        }
        prevTrack.Stop();
    }

}
