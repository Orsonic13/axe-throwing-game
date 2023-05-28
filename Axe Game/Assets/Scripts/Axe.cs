using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    GameObject playerObj;
    AxeThrow axeThrow;
    
    void Start() 
    {
        playerObj = GameObject.FindWithTag("Player");
        axeThrow = playerObj.GetComponent<AxeThrow>();
    }

    /*
    void OnTriggerEnter2D(Collider2D other) 
    {
        if(axeThrow.axeState == "withdraw" && other.gameObject.tag == "player") 
        {
            axeThrow.axeState = "held";
            Destroy(axeThrow.axeObj);
        }
    }
    */
}
