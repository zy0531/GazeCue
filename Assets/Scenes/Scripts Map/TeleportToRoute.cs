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
        if (Input.GetKeyUp(KeyCode.T))
        {
            if(!hasStarted)
            {
                xrRig.transform.position = startPosition.position;
                locomotion.enabled = true;
                hasStarted = true;
                SharedVariables.hasStarted = hasStarted;
            }  
        }
    }
}
