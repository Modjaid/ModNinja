using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IntroPanelJostick : MonoBehaviour, IPointerDownHandler
{
    public Animation introPanelJostick;
    public int maxClick;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!introPanelJostick.isPlaying)
        {
            maxClick--;
        }
        introPanelJostick.Play("introPanelJostick");
        if(maxClick <= 0)
        {
            this.enabled = false;
        }
    }
}
