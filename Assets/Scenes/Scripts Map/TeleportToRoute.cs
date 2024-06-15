using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;



public class TeleportToRoute : MonoBehaviour
{
    [SerializeField] Transform startPosition;
    [SerializeField] GameObject xrRig;
    [SerializeField] Locomotion locomotion;

    bool hasStarted;


    // Start is called before the first frame update
    void Start()
    {
        locomotion.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        // use "T" on the keyboard to teleport to the start position
/*        if (Input.GetKeyUp(KeyCode.T))
        {
            if(!hasStarted)
            {
                TeleportTo(startPosition);
                locomotion.enabled = true;
                hasStarted = true;
                SharedVariables.hasStarted = hasStarted;
            }  
        }*/
    }

    

    public void TeleportTo(Transform transform)
    {
        xrRig.transform.position = transform.position;
    }

}
