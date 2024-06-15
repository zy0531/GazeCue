using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;




public class GazeCue : MonoBehaviour
{
    [SerializeField] LayerMask layermask;

    /// <summary>
    /// dwell implementation
    /// </summary>
    public Transform xrCamera; // The camera representing the head direction
    // public float dwellTimeThreshold = 0.2f; // Threshold for dwell time
    public float nearFarThreshold = 5f;
    public float dwellTimeThreshold_near;
    public float dwellTimeThreshold_far;
    public float deviationThreshold = 0.3f; // Threshold for angular deviation (in degrees) to consider it as a dwell

    public float deactivateTime = 4f; // Lasting time of visual effect

    /// <summary>
    /// Number of samples to consider for dwell detection ---> n = dwellTimeThreshold * sampleRate   
    /// </summary>
    public float sampleRate = 200; // The frequency of GAZE SAMPLE
    
    int n_near; 
    int n_far;
    int n; // n_max
    int n_min;

    Queue<Vector3> gazeDirections;
    Queue<Vector3> headDirections;

    [SerializeField] EyeTrackingGazePilot gaze;
    Vector3 gazeOrigin;
    Vector3 gazeDirection; // The eye gaze direction
    Vector3 headDirection; // The head forward direction

    public int visualFeedbackCount_near { get; set; }
    public int visualFeedbackCount_far { get; set; }


    /// <summary>
    /// find the counterpart of a given landmark or landmarkReplica
    /// </summary>
    [SerializeField] CounterpartFinder counterpartFinder;
    GameObject counterpart_gameobject;

    /// <summary>
    /// define the mechanisms of the AR visual effect display
    /// </summary>
    public DisplayMechanismType displayMechanismType;


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
                // Debug.Log("OnDwellDetectedChanged event invoked. New value: " + _isDwellDetected);
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







    public Vector3 GetGazeOrigin()
    {
        return gazeOrigin;
    }
    public Vector3 GetGazeDirection()
    {
        return gazeDirection;
    }


    public void SetDwellTimeThreshold(float dwellTimeThreshold_near_input, float dwellTimeThreshold_far_input)
    {
        dwellTimeThreshold_near = dwellTimeThreshold_near_input/1000f;
        dwellTimeThreshold_far = dwellTimeThreshold_far_input/1000f;

        n_near = (int)(dwellTimeThreshold_near * sampleRate);
        n_far = (int)(dwellTimeThreshold_far * sampleRate);
        n = Mathf.Max(n_near, n_far);
        gazeDirections = new Queue<Vector3>(n);
        headDirections = new Queue<Vector3>(n);
        
    }



    // Start is called before the first frame update
    void Awake()
    {
        // n = (int)(dwellTimeThreshold * sampleRate);
        /*n_near = (int)(dwellTimeThreshold_near * sampleRate);
        n_far = (int)(dwellTimeThreshold_far * sampleRate);
        n = Mathf.Max(n_near, n_far);
        n_min = Mathf.Min(n_near, n_far);
        gazeDirections = new Queue<Vector3>(n);
        headDirections = new Queue<Vector3>(n);*/
    }

    // Fixed Timestamp 0.005s 
    void FixedUpdate()
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
        if (n_near <= n_far)
        {
            if (gazeDirections.Count == n_near && headDirections.Count == n_near)
            {
                float angularDeviation = CalculateAngularDeviation(gazeDirections, headDirections);

                if (angularDeviation <= deviationThreshold)
                {
                    isSaccadeDetected = false;

                    if (!isDwellDetected)
                    {
                        isDwellDetected = true;
                        Vector3 gazeDirectionCentroid = GetCentroid(gazeDirections);
                        // Debug.Log($"Near gaze dwell detected! The sample num is {gazeDirections.Count}");
                        if (DetectCollidedObjects(gazeDirectionCentroid, DwelledObject.Near))
                        {
                            // Debug.Log($"Near gaze dwell detected! Near Visual Effect Triggered!!!!");
                            visualFeedbackCount_near++;
                            gazeDirections.Clear();
                            headDirections.Clear();
                        }
                    }
                }
                else
                {
                    isDwellDetected = false;
                    isSaccadeDetected = true;
                    // Debug.Log("Saccades! Start the countdown of the visual effect deactivation!");
                }
            }

            if (gazeDirections.Count == n_far && headDirections.Count == n_far)
            {
                float angularDeviation = CalculateAngularDeviation(gazeDirections, headDirections);

                if (angularDeviation <= deviationThreshold)
                {
                    isSaccadeDetected = false;

                    if (!isDwellDetected)
                    {
                        isDwellDetected = true;
                        Vector3 gazeDirectionCentroid = GetCentroid(gazeDirections);
                        // Debug.Log($"Far gaze dwell detected! The sample num is {gazeDirections.Count}");
                        if (DetectCollidedObjects(gazeDirectionCentroid, DwelledObject.Far))
                        {
                            // Debug.Log($"Far gaze dwell detected! Far Visual Effect Triggered!!!!");
                            visualFeedbackCount_far++;
                        }
                        gazeDirections.Clear();
                        headDirections.Clear();
                    }
                }
                else
                {
                    isDwellDetected = false;
                    isSaccadeDetected = true;
                    // Debug.Log("Saccades! Start the countdown of the visual effect deactivation!");
                }
            }

        }
        else
        {
            if (gazeDirections.Count == n_far && headDirections.Count == n_far)
            {
                float angularDeviation = CalculateAngularDeviation(gazeDirections, headDirections);

                if (angularDeviation <= deviationThreshold)
                {
                    isSaccadeDetected = false;

                    if (!isDwellDetected)
                    {
                        isDwellDetected = true;
                        Vector3 gazeDirectionCentroid = GetCentroid(gazeDirections);
                        // Debug.Log($"Far gaze dwell detected! The sample num is {gazeDirections.Count}");
                        if (DetectCollidedObjects(gazeDirectionCentroid, DwelledObject.Far))
                        {
                            // Debug.Log($"Far gaze dwell detected! Far Visual Effect Triggered!!!!");
                            visualFeedbackCount_far++;
                            gazeDirections.Clear();
                            headDirections.Clear();
                        }
                    }
                }
                else
                {
                    isDwellDetected = false;
                    isSaccadeDetected = true;
                    // Debug.Log("Saccades! Start the countdown of the visual effect deactivation!");
                }
            }

            if (gazeDirections.Count == n_near && headDirections.Count == n_near)
            {
                float angularDeviation = CalculateAngularDeviation(gazeDirections, headDirections);

                if (angularDeviation <= deviationThreshold)
                {
                    isSaccadeDetected = false;

                    if (!isDwellDetected)
                    {
                        isDwellDetected = true;
                        Vector3 gazeDirectionCentroid = GetCentroid(gazeDirections);
                        // Debug.Log($"Near gaze dwell detected! The sample num is {gazeDirections.Count}");
                        if (DetectCollidedObjects(gazeDirectionCentroid, DwelledObject.Near))
                        {
                            // Debug.Log($"Near gaze dwell detected! Near Visual Effect Triggered!!!!");
                            visualFeedbackCount_near++;
                        }
                        gazeDirections.Clear();
                        headDirections.Clear();
                    }
                }
                else
                {
                    isDwellDetected = false;
                    isSaccadeDetected = true;
                    // Debug.Log("Saccades! Start the countdown of the visual effect deactivation!");
                }
            }  
        }

        // if we have enough samples for n
        /*if (gazeDirections.Count == n && headDirections.Count == n)
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
        }*/
    }



    bool DetectCollidedObjects(Vector3 gazeDirectionCentroid, DwelledObject dwelledObject)
    {
        // Perform a RaycastAll from the gaze origin in the direction of the gaze
        RaycastHit[] hits = Physics.RaycastAll(gazeOrigin, gazeDirectionCentroid, Mathf.Infinity, layermask);

        // Filter hits
        if (dwelledObject == DwelledObject.Near)
        {
            hits = hits.Where(hit => hit.distance <= nearFarThreshold).ToArray();
        }  
        else
        {
            hits = hits.Where(hit => hit.distance > nearFarThreshold).ToArray();
        }
        
        // Process the detected game objects & Trigger Visual Effect
        foreach (RaycastHit hit in hits)
        {
            TriggerVisualEffect(hit);
        }

        // Save the previous gaze hit objects (for EXCLUSIVE visual effect)
        if (hits.Length > 0)
        {
            prevHits = new RaycastHit[hits.Length];
            Array.Copy(hits, prevHits, hits.Length);
        }

        // continue checking the dwell
        isDwellDetected = false;

        return hits.Length > 0;
    }

    void TriggerVisualEffect(RaycastHit hit)
    {
        // 1. Remove previous visual effect based on the DisplayMechanismType
        foreach (RaycastHit prevHit in prevHits)
        {
            // if apply the EXCLUSIVE on map
            if(displayMechanismType == DisplayMechanismType.Exclusive_Map)
            {
                // Check if the previous gaze hit on the map
                if (prevHit.collider != null && prevHit.transform.name.Contains("(Clone)"))
                {
                    var prevHit_counterpart_gameobject = counterpartFinder.FindCounterpart(prevHit.transform.gameObject);
                    RemoveHighlight(prevHit.transform);
                    RemoveHighlight(prevHit_counterpart_gameobject.transform);
                    RemoveXRayVision(prevHit.transform);
                    RemoveXRayVision(prevHit_counterpart_gameobject.transform);
                }
            }
            // if apply the EXCLUSIVE on any triggerable objects
            else if (displayMechanismType == DisplayMechanismType.Exclusive_All)
            {
                if (prevHit.collider != null)
                {
                    if(prevHit.transform.name.Contains("(Clone)"))
                    {
                        var prevHit_counterpart_gameobject = counterpartFinder.FindCounterpart(prevHit.transform.gameObject);
                        RemoveHighlight(prevHit.transform);
                        RemoveHighlight(prevHit_counterpart_gameobject.transform);
                        RemoveXRayVision(prevHit.transform);
                        RemoveXRayVision(prevHit_counterpart_gameobject.transform);
                    }
                    else
                    {
                        var prevHit_counterpart_gameobject = counterpartFinder.FindCounterpart(prevHit.transform.gameObject);
                        RemoveHighlight(prevHit.transform);
                        RemoveHighlight(prevHit_counterpart_gameobject.transform);
                        RemoveXRayVision(prevHit.transform);
                        RemoveXRayVision(prevHit_counterpart_gameobject.transform);
                    }
                }
                
            }
            // if apply concurrent machanism
            else
            {
                // do nothing with previous triggered visual effects
            }
        }
        
        // 2. clear the prevHits after remove the visual effects on them
        prevHits = new RaycastHit[0];


        // 3. Trigger latest visual effect
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
        // Debug.Log($"Average Angular Deviation: {averageDeviation}");
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
