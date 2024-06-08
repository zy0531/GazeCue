using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateItself : MonoBehaviour
{
    IEnumerator coroutine;
    bool oldStatus;
    float waitTime = 4f;

    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.activeSelf != oldStatus)
        {
            coroutine = WaitAndDeactivate(waitTime);
            StartCoroutine(coroutine);
        }
        oldStatus = this.gameObject.activeSelf;
    }

    public IEnumerator WaitAndDeactivate(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        oldStatus = false; // set status mannually
        this.gameObject.SetActive(false);
    }
}