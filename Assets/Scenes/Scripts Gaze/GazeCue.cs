using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class GazeCue : MonoBehaviour
{
    [SerializeField] LayerMask layermask;

    /// <summary>
    /// dwell implementation
    /// </summary>
    public Transform xrCamera; // The camera representing the head direction
    public float dwellTimeThreshold = 0.2f; // Threshold for dwell time
    public float deviationThreshold = 0.3f; // Threshold for angular deviation (in degrees) to consider it as a dwell

    public float deactivateTime = 4f;

    public float sampleRate = 200; // The frequency of eye tracker
    int n; // Number of samples to consider for dwell detection = dwellTimeThreshold * sampleRate

    Queue<Vector3> gazeDirections;
    Queue<Vector3> headDirections;

    EyeTrackingExploration gaze;
    Vector3 gazeOrigin;
    Vector3 gazeDirection; // The eye gaze direction
    Vector3 headDirection; // The head forward direction

    // bool isDwellDetected = false;

    /// <summary>
    /// find the counterpart of a given landmark or landmarkReplica
    /// </summary>
    [SerializeField] CounterpartFinder counterpartFinder;
    GameObject counterpart_gameobject;


    /// <summary>
    /// check the occlusion ratio of a landmark
    /// </summary>
    [SerializeField] OcclusionChecker occlusionChecker;
    public float occlusionThreshold = 0.6f;

    // save the previous gaze hit objects
    RaycastHit[] prevHits = new RaycastHit[0];


    /// <summary>
    /// Event-driven Approach
    /// efficiently monitor the isDwellDetected variable from a single GazeCues instance
    /// </summary>
    // A private backing field _isDwellDetected.
    private bool _isDwellDetected = false;
    // When the isDwellDetected property changes, it raises the OnDwellDetectedChanged event if the value is different.
    public bool isDwellDetected
    {
        get { return _isDwellDetected; }
        set
        {
            if (_isDwellDetected != value)
            {
                _isDwellDetected = value;
                OnDwellDetectedChanged?.Invoke(_isDwellDetected);
                Debug.Log("OnDwellDetectedChanged event invoked. New value: " + _isDwellDetected);
            }
        }
    }
    // The OnDwellDetectedChanged event passes the new value of isDwellDetected to subscribers.
    public event Action<bool> OnDwellDetectedChanged;


    /// <summary>
    /// Event-driven Approach
    /// efficiently monitor the isSaccadeDetected variable from a single GazeCues instance
    /// </summary>
    // A private backing field _isSaccadeDetected.
    private bool _isSaccadeDetected = false;
    // When the isSaccadeDetected property changes, it raises the OnDwellDetectedChanged event if the value is different.
    public bool isSaccadeDetected
    {
        get { return _isSaccadeDetected; }
        set
        {
            if (_isSaccadeDetected != value)
            {
                _isSaccadeDetected = value;
                OnSaccadeDetectedChanged?.Invoke(_isSaccadeDetected);
            }
        }
    }
    // The OnDwellDetectedChanged event passes the new value of isSaccadeDetected to subscribers.
    public event Action<bool> OnSaccadeDetectedChanged;






    // Start is called before the first frame update
    void Start()
    {
        gaze = GetComponent<EyeTrackingExploration>();
        n = (int)(dwellTimeThreshold * sampleRate);
        gazeDirections = new Queue<Vector3>(n);
        headDirections = new Queue<Vector3>(n);

    }

    // Update is called once per frame
    void Update()
    {
        // Read in the gaze origin and gaze direction (world coordinate) from EyeTrackingExample
        gazeOrigin = gaze.getRayOrigin();
        gazeDirection = gaze.getDirection();
        
        // Get the head forward direction
        headDirection = xrCamera.transform.forward;

        // Store the current gaze direction and head direction
        if (gazeDirections.Count >= n)
        {
            gazeDirections.Dequeue();
            headDirections.Dequeue();
        }
        gazeDirections.Enqueue(gazeDirection);
        headDirections.Enqueue(headDirection);

        // Check for dwell only after we have enough samples
        if (gazeDirections.Count == n && headDirections.Count == n)
        {
            float angularDeviation = CalculateAngularDeviation(gazeDirections, headDirections);

            if (angularDeviation <= deviationThreshold)
            {
                isSaccadeDetected = false;
                
                if (!isDwellDetected)
                {
                    isDwellDetected = true;
                    Debug.Log("Gaze dwell detected!" + isDwellDetected);
                    Vector3 gazeDirectionCentroid = GetCentroid(gazeDirections);
                    DetectCollidedObjects(gazeDirectionCentroid);
                }   
            }
            else
            {
                isSaccadeDetected = true;
                isDwellDetected = false;
                Debug.Log("Saccades! Start the countdown of the visual effect deactivation! " + isSaccadeDetected);

            }
        }
    }



    void DetectCollidedObjects(Vector3 gazeDirectionCentroid)
    {
        // Perform a RaycastAll from the gaze origin in the direction of the gaze
        RaycastHit[] hits = Physics.RaycastAll(gazeOrigin, gazeDirectionCentroid, Mathf.Infinity, layermask);

        // Process the detected game objects & Trigger Visual Effect
        foreach (RaycastHit hit in hits)
        {
            TriggerVisualEffect(hit);
        }

        // Save the previous gaze hit objects (for EXCLUSIVE visual effect when the gaze on the map)
        if (hits.Length > 0)
        {
            prevHits = new RaycastHit[hits.Length];
            Array.Copy(hits, prevHits, hits.Length);
        }

        // Detect New Dwell Again
        isDwellDetected = false;
        gazeDirections.Clear();
        headDirections.Clear();

    }

    void TriggerVisualEffect(RaycastHit hit)
    {
        // 1. Remove previous visual effect
        // EXCLUSIVE visual effect when the gaze on the map 
        // Check if the previous gaze hit on the map
        foreach (RaycastHit prevHit in prevHits)
        {
            if (prevHit.collider != null && prevHit.transform.name.Contains("(Clone)"))
            {
                var prevHit_counterpart_gameobject = counterpartFinder.FindCounterpart(prevHit.transform.gameObject);
                RemoveHighlight(prevHit.transform);
                RemoveHighlight(prevHit_counterpart_gameobject.transform);
                RemoveXRayVision(prevHit.transform);
                RemoveXRayVision(prevHit_counterpart_gameobject.transform);
            }
        }
        

        // 2. Trigger latest visual effect
        // Find the counterpart
        counterpart_gameobject = counterpartFinder.FindCounterpart(hit.transform.gameObject);
        if (counterpart_gameobject != null)
        {
            Debug.Log("Counterpart found: " + counterpart_gameobject.name);
            
            // Activate the visual feedback on the hit gameobject & its counterpart gameobject
            // Check occlusion ratio to determine which visual feedback to be activated
            if (occlusionChecker.CheckOcclusion(hit.transform.gameObject) < occlusionThreshold && occlusionChecker.CheckOcclusion(counterpart_gameobject) < occlusionThreshold)
            {
                TriggerHighlight(hit.transform);
                TriggerHighlight(counterpart_gameobject.transform);
            }
            else
            {
                TriggerXRayVision(hit.transform);
                TriggerXRayVision(counterpart_gameobject.transform);
            }            

        }
        else
        {
            Debug.Log("Counterpart not found");
        }
    }


    void TriggerHighlight(Transform transform)
    {
        var gameobj = transform.GetChild(0).gameObject;
        if(gameobj.activeSelf == false)
            gameobj.SetActive(true);
    }
    void RemoveHighlight(Transform transform)
    {
        var gameobj = transform.GetChild(0).gameObject;
        if (gameobj.activeSelf == true)
            gameobj.SetActive(false);
    }

    void TriggerXRayVision(Transform transform)
    {
        XRayToggle xRayToggle = transform.GetComponent<XRayToggle>();
        if (xRayToggle != null)
        {
            if (!xRayToggle.IsXRayVisionOn())
            {
                xRayToggle.TurnOnXRayVision();
                xRayToggle.ActivateOutlineHighlight(transform.gameObject);

                // For landmarks in the environment
                if (!transform.gameObject.name.Contains("(Clone)"))
                {
                    Debug.Log("Only change layer for the landmark in the environment! No need for landmark replicas on the map!");
                    xRayToggle.ChangeLayerRecursively(transform.gameObject, LayerMask.NameToLayer("XRayVision"));
                }
            }
        }
    }
    void RemoveXRayVision(Transform transform)
    {
        XRayToggle xRayToggle = transform.GetComponent<XRayToggle>();
        if (xRayToggle != null)
        {
            if (xRayToggle.IsXRayVisionOn())
            {
                xRayToggle.TurnOffXRayVision();
                xRayToggle.DeactivateOutlineHighlight(transform.gameObject);

                // For landmarks in the environment
                if (!transform.gameObject.name.Contains("(Clone)"))
                {
                    Debug.Log("Only change layer for the landmark in the environment! No need for landmark replicas on the map!");
                    xRayToggle.ChangeLayerRecursively(transform.gameObject, LayerMask.NameToLayer("TargetLandmark"));
                }
            }
        }
    }

    float CalculateAngularDeviation(Queue<Vector3> gazeDirections, Queue<Vector3> headDirections)
    {
        float sum = 0f;
        Vector3[] gazeArray = gazeDirections.ToArray();
        Vector3[] headArray = headDirections.ToArray();

        for (int t = 1; t < gazeArray.Length; t++)
        {
            // Calculate angles in degrees
            float angle1 = Vector3.Angle(gazeArray[t], headArray[t]);
            float angle2 = Vector3.Angle(gazeArray[t - 1], headArray[t - 1]);

            // Compute the absolute difference of angles
            float difference = Mathf.Abs(angle1 - angle2);
            sum += difference;

            // Debug.Log($"Angle1: {angle1}, Angle2: {angle2}, Difference: {difference}");
        }

        // Return the average angular deviation
        float averageDeviation = sum / gazeArray.Length;
        Debug.Log($"Average Angular Deviation: {averageDeviation}");
        return averageDeviation;
    }

    Vector3 GetCentroid(Queue<Vector3> vectors)
    {
        if (vectors.Count == 0)
        {
            Debug.LogError("Queue is empty, cannot calculate centroid.");
            return Vector3.zero;
        }

        Vector3 sum = Vector3.zero;

        foreach (Vector3 vector in vectors)
        {
            sum += vector;
        }

        Vector3 centroid = sum / vectors.Count;

        return centroid;
    }




}
