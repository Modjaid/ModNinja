using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlideBar
{
    public Coroutine sliding;
    public float slideBarDiapason;
    private float speedUp;
    private float currentPoints;
    private float targetPoints;

    public SlideBar(float slideBarDiapason, float speedUp = 4)
    {
        targetPoints = slideBarDiapason;
        this.slideBarDiapason = slideBarDiapason;
        currentPoints = slideBarDiapason;
        this.speedUp = speedUp;
    }
    public float GetBarNormalized()
    {
        return currentPoints / slideBarDiapason;
    }
 
    public IEnumerator Sliding(Image imageBar, float currentHealth)
    {
        if(currentHealth < 0)
        {
            this.targetPoints = 0;
        }
        else if(currentHealth > slideBarDiapason)
        {
            this.targetPoints = slideBarDiapason;
        }
        else
        {
            this.targetPoints = currentHealth;
        }

        if (currentPoints < targetPoints)
        {
            return Increase();
        }
        else
        {
            return Decrease();
        }

        IEnumerator Increase()
        {
            while (currentPoints < targetPoints)
            {
                currentPoints += Time.deltaTime * speedUp;
                imageBar.fillAmount = GetBarNormalized();
                yield return null;
            }
            
        }
        IEnumerator Decrease()
        {
            while (currentPoints > targetPoints)
            {
                currentPoints -= Time.deltaTime * speedUp;
                imageBar.fillAmount = GetBarNormalized();
                yield return null;
            }
        }
    }
    public IEnumerator SlidingWithRepeat(Image imageBar)
    {
        return Update();

        IEnumerator Update()
        {
            while (true)
            {
                currentPoints = 0;
                while (currentPoints < slideBarDiapason)
                {
                    currentPoints += Time.deltaTime * speedUp;
                    imageBar.fillAmount = GetBarNormalized();
                    yield return null;
                }
            }

        }
    }
}
