using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// attach to the landmark gameobject and the landmark replicas
/// turn off the xray vision after a period of time
/// </summary>
public class XRayToggle : MonoBehaviour
{
    public GazeCue gazeCue;

    public string newLayerName = "XRayVision";
    public string originalLayerName = "TargetLandmark";

    private float deactivateTime;
    private int newLayer;
    private int originalLayer;
    private bool isLayerSwitched = false;
    private float timer;
    internal object xRayToggle;
    private bool countdownActive = false;


    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to the 'OnDwellDetectedChanged' event
        if (gazeCue != null)
        {
            // gazeCue.OnDwellDetectedChanged += OnDwellDetectedChanged;
            gazeCue.OnSaccadeDetectedChanged += OnSaccadeDetectedChanged;
        }
        
        deactivateTime = gazeCue.deactivateTime;

        // Get the layer indices based on the layer names
        newLayer = LayerMask.NameToLayer(newLayerName);
        originalLayer = LayerMask.NameToLayer(originalLayerName);

        // Ensure the layers exist
        if (newLayer == -1 || originalLayer == -1)
        {
            Debug.LogError("One or both of the specified layers do not exist.");
            return;
        }
    }

    // Unsubscribe the 'OnDwellDetectedChanged' event for avoiding memory leakage
    private void OnDestroy()
    {
        if (gazeCue != null)
        {
            // gazeCue.OnDwellDetectedChanged -= OnDwellDetectedChanged;
            gazeCue.OnSaccadeDetectedChanged -= OnSaccadeDetectedChanged;
        }
    }
    
    // Starts the countdown when isDwellDetected becomes false.
    private void OnDwellDetectedChanged(bool isDwellDetected)
    {
        if (!isDwellDetected)
        {
            countdownActive = true; // start the countdown when dwell ends
            timer = 0f; // reset the timer
            
        }
    }

    // Starts the countdown when isSaccadeDetected becomes true.
    private void OnSaccadeDetectedChanged(bool isSaccadeDetected)
    {
        if (isSaccadeDetected)
        {
            countdownActive = true; // start the countdown when saccade starts
            timer = 0f; // reset the timer

        }
    }

    void FixedUpdate()
    {
        
        // turn off the xray after delay
        if (countdownActive && isLayerSwitched)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= deactivateTime)
            {
                // deactivate xray gameoject rendering
                ChangeLayerRecursively(gameObject, originalLayer);
                isLayerSwitched = false;
                timer = 0f;

                // deactivate the contour outline
                DeactivateOutlineHighlight(gameObject);

                // stop the countdown
                countdownActive = false;
            }
        }
    }

    public bool IsXRayVisionOn()
    {
        return isLayerSwitched;
    }

    public void TurnOnXRayVision()
    {
        isLayerSwitched = true;
        timer = 0f;
    }

    public void TurnOffXRayVision()
    {
        isLayerSwitched = false;
        timer = 0f;
    }

    /// <summary>
    /// Recursively changes the layer of the GameObject and all its children.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="layer"></param>
    public void ChangeLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        
        foreach (Transform child in obj.transform)
        {
            ChangeLayerRecursively(child.gameObject, layer);
            Debug.Log($"child.gameObject: {child.gameObject} changed to layer {layer}");
        }

    }

    public void ActivateOutlineHighlight(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            if (child.gameObject.name == "Contour")
            {
                child.gameObject.SetActive(true);
                break;
            }
        }
    }
    public void DeactivateOutlineHighlight(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            if (child.gameObject.name == "Contour")
            {
                child.gameObject.SetActive(false);
                break;
            }
        }
    }
}
