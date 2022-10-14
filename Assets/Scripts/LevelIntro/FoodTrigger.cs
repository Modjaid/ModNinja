using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.name == "Food")
        {
            FieldManager.instance.gameObject.GetComponent<Animation>().Play("CutScene_2");
        }
    }
}
