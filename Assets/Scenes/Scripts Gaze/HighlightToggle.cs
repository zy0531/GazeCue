using UnityEngine;
using System.Collections;

public class HighlightToggle : MonoBehaviour
{
    public GazeCue gazeCue;
    
    private IEnumerator coroutine;
    private bool defaultStatus = false;
    private float deactivateTime;

    void Start()
    {
        if (gazeCue != null)
        {
            // gazeCue.OnDwellDetectedChanged += OnDwellDetectedChanged;
            gazeCue.OnSaccadeDetectedChanged += OnSaccadeDetectedChanged;
        }

        deactivateTime = gazeCue.deactivateTime;
    }

    void OnDestroy()
    {
        if (gazeCue != null)
        {
            // gazeCue.OnDwellDetectedChanged -= OnDwellDetectedChanged;
            gazeCue.OnSaccadeDetectedChanged -= OnSaccadeDetectedChanged;
        }
    }

    void OnDwellDetectedChanged(bool isDwellDetected)
    {
        if (!isDwellDetected && this.gameObject.activeSelf != defaultStatus)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = WaitAndDeactivate(deactivateTime);
            StartCoroutine(coroutine);
        }
    }

    void OnSaccadeDetectedChanged(bool isSaccadeDetected)
    {
        if (isSaccadeDetected && this.gameObject.activeSelf != defaultStatus)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = WaitAndDeactivate(deactivateTime);
            StartCoroutine(coroutine);
        }
    }

    public IEnumerator WaitAndDeactivate(float deactivateTime)
    {
        yield return new WaitForSeconds(deactivateTime);
        defaultStatus = false; // Set status manually
        this.gameObject.SetActive(false);
    }
}
