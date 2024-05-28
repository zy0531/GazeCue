using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// attach to the landmark gameobject and the landmark replicas
/// turn off the xray vision after a period of time
/// </summary>
public class XRayToggle : MonoBehaviour
{
    public string newLayerName = "XRayVision";
    public string originalLayerName = "TargetLandmark";
    public float deactivateTime = 2.0f; // Time in seconds before switching back
    // public float xRayVisionLength = 2.0f; // Time in seconds before switching back

    private int newLayer;
    private int originalLayer;
    private bool isLayerSwitched = false;
    private float timer;
    internal object xRayToggle;


    // Start is called before the first frame update
    void Start()
    {
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

    void FixedUpdate()
    {
        
        // turn off the xray after delay
        if (isLayerSwitched)
        {
            timer += Time.deltaTime;
            if (timer >= deactivateTime)
            {
                ChangeLayerRecursively(gameObject, originalLayer);
                isLayerSwitched = false;
                timer = 0f;

                DeactivateOutlineHighlight(gameObject);
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
