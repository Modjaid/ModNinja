using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestCanvas : MonoBehaviour
{
    public Text sceneName;
    public Text textButton;
    private bool infinity = false;
    void Start()
    {
       // sceneName.text = SceneManager.GetActiveScene().name + " <" + ((Difficult)(FieldManager.instance.levelMode)).ToString() + ">";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeJumper()
    {
        if (!infinity)
        {
            FieldManager.instance.player.GetComponent<PlayerPuck>().elements.jumpers = 100000.5f;
            textButton.text = "JumperOn";
            infinity = true;
        }
        else
        {
            FieldManager.instance.player.GetComponent<PlayerPuck>().elements.jumpers = 0;
            textButton.text = "JumperOff";
            infinity = false;
        }
    }
    
}
