using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTime : MonoBehaviour
{   
    private float elapsedTime = 0;
    private GameObject[] tanks;
    private bool noneInRange = true;

    // Start is called before the first frame update
    void Start()
    {
        tanks = GameObject.FindGameObjectsWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        noneInRange = true;
        foreach (GameObject tank in tanks){
            float dist = Vector3.Distance(this.gameObject.transform.position,tank.transform.position);
           
            if (dist < 5 && elapsedTime > 0.5){
                Time.timeScale = 0.5f;
                noneInRange = false;
            }
        }

        if (noneInRange){
            Time.timeScale = 1f;
        }
    }

   
    
}
