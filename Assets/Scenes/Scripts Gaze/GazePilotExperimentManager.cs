using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR;
using TMPro;


public enum DisplayMechanismType
{
    Concurrent,
    Exclusive_Map,
    Exclusive_All
}

public enum DwellTime
{
    Short = 200,
    Medium = 500,
    Long = 800
}

public enum DwelledObject
{
    Near,
    Far
}




public class GazePilotExperimentManager : MonoBehaviour
{
    [SerializeField] Transform mainCamera;    
    
    /// <summary>
    /// Display Mechanism Type will be counterbalanced by the experimenter. 
    /// </summary>
    public DisplayMechanismType displayMechanismType;

    /// <summary>
    /// trialID 0-5: represents the repetition of each dwell time combination.
    /// </summary>
    public int trialID { get; set; }
    /// <summary>
    /// trialID_accumulated 1-108: represents the accumulated trial number.
    /// </summary>
    public int trialID_accumulated { get; set; }

    public int trialRepetition;
    [SerializeField] TMP_Text trialIDText; // display the slider's value
    [SerializeField] LandmarkPlacement landmarkPlacement;
    [SerializeField] Transform miniMap;


    /// <summary>
    /// Questionnaire
    /// </summary>
    [SerializeField] GameObject mainComponents;
    [SerializeField] GameObject questionnaireComponents;
    [SerializeField] QuestionnaireDataManager questionnaireDataManager;

    [SerializeField] TeleportToRoute teleportToFunc;
    [SerializeField] Transform lobbyPosition;
    [SerializeField] Transform startPosition;
    [SerializeField] GameObject miniMapDisplay;
    public bool isInUrbanEnvironment { get; set; }

    /// <summary>
    /// XR Input
    /// </summary>
    InputDevice righthand;
    bool triggerValue;
    bool buttonDown_XRInput;

    /// <summary>
    /// DwellObject & DwellTime Combination
    /// </summary>
    public List<List<float>> combinations { get; set; }
    public int combinationID { get; set; }
    [SerializeField] GazeCue gazeCue;

    /// <summary>
    /// Task Completion Time
    /// </summary>
    public long trialStartLogTime { get; set; }
    public long trialEndLogTime { get; set; }

    /// <summary>
    /// Data Recording
    /// </summary>
    [SerializeField] DataManager dataManager;
    string Path;
    string FileName;

    /// <summary>
    /// Audios
    /// </summary>
    [SerializeField] AudioSource ClickAudio;
    [SerializeField] AudioSource FinishAudio;

    /// <summary>
    /// Accuracy
    /// </summary>
    public Vector3 targetLandmarkGroundTruth { get; set; }
    Vector3 gazeDirection;
    float gazeAngleError;

    /// <summary>
    /// Environment
    /// </summary>
    public string currentTargetLandmark { get; set; }
    public string currentLandmarkPlacement { get; set; }


    private System.Random random = new System.Random();

    void Start()
    {
        // Find the right controller
        FindRightController();

        // Record Data - "TrialCompletionTime"
        Path = dataManager.folderPath;
        FileName = "TrialCompletionTime";
        LogTrialDataTitle(Path, FileName);       

        // The 3(dwell time on Near Objects) x 3(dwell time on Far Objects) will be randomized by the program. 
        combinations = GenerateCombinations();
        ShuffleList(combinations);
        combinationID = 0;
        gazeCue.SetDwellTimeThreshold(combinations[combinationID][0], combinations[combinationID][1]);
        Debug.Log("combinationID: " + combinationID + "--" + combinations[combinationID][0] + "--" + combinations[combinationID][1]);

        // Print the random combinations
        PrintCombinations(combinations);

        // Set the active camera
        mainComponents.SetActive(true);
        questionnaireComponents.SetActive(false);
        questionnaireDataManager.ResetQuestionIndex();

        // Hide the miniMapDisplay when in the lobby
        miniMapDisplay.SetActive(false);

        // Set trialID
        trialID = 0;
        trialID_accumulated = 0;
        UpdateTrialIDText();

        // Set the initial trial
        landmarkPlacement.GenerateLandmarkPlacement();
        landmarkPlacement.ActiveLandmarkPlacement(trialID);
        landmarkPlacement.ActiveTargetLandmarkImage(trialID);
        // set the current environment info
        currentTargetLandmark = landmarkPlacement.GetTargetLandmarkImageName(trialID);
        currentLandmarkPlacement = landmarkPlacement.GetLandmarkPlacementName(trialID);

        // Reset VisualFeedbackCounter
        ResetVisualFeedbackCounter();
    }


    void Update()
    {
        // ************************************* Tracking the Trigger Pressing *************************************
        if (righthand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue)
        {
            if (!buttonDown_XRInput)
            {
                // Status 1: Button is pressed
                Debug.Log("buttonDown_XRInput is pressed");
                buttonDown_XRInput = true;

            }
            else
            {
                // Status 2: Button is held down
                Debug.Log("buttonDown_XRInput is held");
            }
        }
        else if (!(righthand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue) && buttonDown_XRInput)
        {
            // Status 3: Button is released
            Debug.Log("buttonDown_XRInput is released");
            buttonDown_XRInput = false;


            // Only Enable the Trigger Press when Teleporting into the Urban Environment
            if (isInUrbanEnvironment)
            {
                // Audio for trigger press
                ClickAudio.Play(0);

                // Hide the miniMapDisplay when in the lobby
                miniMapDisplay.SetActive(false);

                // Proceed with the experimental protocol
                if (combinationID < combinations.Count)
                {
                    // If trialID >= 5, start the questinnaire & move to the next combination
                    if (trialID >= (trialRepetition - 1))
                    {
                        // Destroy all current landmark replicas
                        landmarkPlacement.DestroyAllChildren(miniMap);

                        // Log Data Here
                        // Accuracy
                        targetLandmarkGroundTruth = GetTargetLandmarkGroundTruth(trialID);
                        gazeDirection = gazeCue.GetGazeOrigin() + gazeCue.GetGazeDirection();
                        gazeAngleError = GetGazeAngleError(targetLandmarkGroundTruth, gazeDirection);
                        Debug.Log("gazeAngleError: " + gazeAngleError + ";" + "targetLandmarkGroundTruth" + targetLandmarkGroundTruth + ";" + "gazeVector" + gazeDirection + ";");
                        // Time
                        trialEndLogTime = GetCurrentLogTime();
                        LogTrialData(Path, FileName);

                        // Reset VisualFeedbackCounter after the log
                        ResetVisualFeedbackCounter();

                        // Reset trialID
                        trialID = 0;
                        UpdateTrialIDText();

                        // Increment the trialID_accumulated
                        trialID_accumulated++;
                        Debug.Log("trialID_accumulated: " + trialID_accumulated);

                        // Start the questionnaire session
                        if (!questionnaireDataManager.isQuestionnaireFinish)
                        {
                            isInUrbanEnvironment = false;
                            
                            Debug.Log("swith to questionnaire!!!!!!!!!!!!!!!!!!!!!!");
                            mainComponents.SetActive(false);
                            questionnaireComponents.SetActive(true);
                        }
                    }

                    // If the mainComponents is active, increment the trial#
                    if (mainComponents.activeSelf)
                    {
                        // ** Keep the order of the code below for the correct logs of trialID **
                        // Teleport back to the lobby after the trigger release
                        teleportToFunc.TeleportTo(lobbyPosition);
                        isInUrbanEnvironment = false;

                        // Log Data Here
                        // Accuracy
                        targetLandmarkGroundTruth = GetTargetLandmarkGroundTruth(trialID);
                        gazeDirection = gazeCue.GetGazeOrigin() + gazeCue.GetGazeDirection();
                        gazeAngleError = GetGazeAngleError(targetLandmarkGroundTruth, gazeDirection);
                        Debug.Log("gazeAngleError: " + gazeAngleError + ";" + "targetLandmarkGroundTruth" + targetLandmarkGroundTruth + ";" + "gazeVector" + gazeDirection + ";");
                        // Time
                        trialEndLogTime = GetCurrentLogTime();
                        LogTrialData(Path, FileName);

                        // Reset VisualFeedbackCounter after the log
                        ResetVisualFeedbackCounter();

                        // Increment the trialID
                        trialID++;
                        Debug.Log("trialID: " + trialID);
                        UpdateTrialIDText();

                        // Increment the trialID_accumulated
                        trialID_accumulated++;
                        Debug.Log("trialID_accumulated: " + trialID_accumulated);


                        // Destroy all current landmark replicas and activate new one
                        landmarkPlacement.DestroyAllChildren(miniMap);
                        landmarkPlacement.ActiveLandmarkPlacement(trialID);
                        landmarkPlacement.ActiveTargetLandmarkImage(trialID);
                    }
                }
                else
                {
                    // Add Audio Here!
                    Debug.Log("You have finish all combinations of dwell times!");
                    FinishAudio.Play(0);
                    StartCoroutine(Quit.WaitQuit(6));
                }
            }


        }

        // ************************************* Tracking if the Questionnaire Finish or Not*************************************
        // After finishing the questionnaire
        if (questionnaireDataManager.isQuestionnaireFinish)
        {
            // Reset questionnaire
            questionnaireComponents.SetActive(false);
            questionnaireDataManager.ResetQuestionIndex();
            
            // Switch back to main component
            mainComponents.SetActive(true);
            Debug.Log("swith back to main!!!!!!!!!!!!!!!!!!!!!!");
            
            // Generate new landmark placement
            landmarkPlacement.GenerateLandmarkPlacement();

            // Increment combinationID for the next DwellObject & DwellTime Combination
            combinationID++;
            gazeCue.SetDwellTimeThreshold(combinations[combinationID][0], combinations[combinationID][1]);
            Debug.Log("combinationID: " + combinationID + "--" + combinations[combinationID][0] + "--" + combinations[combinationID][1]);

            // Make sure in the lobby after the questionnaire finished
            teleportToFunc.TeleportTo(lobbyPosition);
            isInUrbanEnvironment = false;
        }



        // change back to T for the formal experiment
        if (Input.GetKeyUp(KeyCode.DownArrow)) 
        {
            if (!isInUrbanEnvironment)
            {
                teleportToFunc.TeleportTo(startPosition);
                isInUrbanEnvironment = true;
                trialStartLogTime = GetCurrentLogTime();
                miniMapDisplay.SetActive(true);
            }
        }
    }


    public void LogTrialData(string path, string fileName)
    {
        // set the current environment info
        currentTargetLandmark = landmarkPlacement.GetTargetLandmarkImageName(trialID);
        currentLandmarkPlacement = landmarkPlacement.GetLandmarkPlacementName(trialID);

        // calculate the trial completion time
        long duration = trialEndLogTime - trialStartLogTime;
        Debug.Log("duration~~~~~~~~~~~~~~~~~~~~~~: " + duration);

        RecordData.SaveData(path, fileName,
          trialID_accumulated + ","
          + trialID + ","
          + displayMechanismType + ","
          + combinations[combinationID][0] + ","
          + combinations[combinationID][1] + ","
          + currentTargetLandmark + ","
          + currentLandmarkPlacement + ","
          // + "TargetLandmarkOcclusionRatio" + ","
          + trialStartLogTime.ToString() + ","
          + trialEndLogTime.ToString() + ","
          + duration.ToString() + ","
          + gazeCue.visualFeedbackCount_near + ","
          + gazeCue.visualFeedbackCount_far + ","
          + targetLandmarkGroundTruth + ","
          + gazeDirection + ","
          + gazeAngleError + ","
          + '\n');
    }

    public void LogTrialDataTitle(string path, string fileName)
    {
        RecordData.SaveData(path, fileName,
          "trialID_accumulated" + ","
          + "trialID" + ","
          + "DisplayMechanism" + ","
          + "DwellTimeThreshold_Near" + ","
          + "DwellTimeThreshold_Far" + ","
          + "TargetLandmark_ID" + ","
          + "LandmarkPlacement_ID" + ","
          // + "TargetLandmarkOcclusionRatio" + ","
          + "trialStartLogTime" + ","
          + "trialEndLogTime" + ","
          + "UsedTime" + ","
          + "VisualEffectCount_Near" + ","
          + "VisualEffectCount_Far" + ","
          + "TargetLandmarkGroundTruth " + ","
          + "GazeDirection" + ","
          + "GazeAngleError" + ","
          + '\n');
    }


    public void ResetVisualFeedbackCounter()
    {
        gazeCue.visualFeedbackCount_near = 0;
        gazeCue.visualFeedbackCount_far = 0;
    }


    public long GetCurrentLogTime()
    {
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }


    public Vector3 GetTargetLandmarkGroundTruth(int trialID)
    {
        Vector3 targetLandmarkPosition = landmarkPlacement.GetTargetLandmarkTransform(trialID);
        return (targetLandmarkPosition - mainCamera.position).normalized; // give you a vector pointing from the main camera to the target object.

    }


    public float GetGazeAngleError(Vector3 targetLandmarkGroundTruth, Vector3 gazeVector)
    {
        // Cast on the xz plane first
        // targetLandmarkGroundTruth = targetLandmarkGroundTruth - mainCamera.position;
        var targetLandmarkGroundTruthOnXZ = Vector3.ProjectOnPlane(targetLandmarkGroundTruth, Vector3.up).normalized;//xz plane
        // gazeVector = gazeVector - mainCamera.position;
        var gazeVectorOnXZ = Vector3.ProjectOnPlane(gazeVector, Vector3.up).normalized;//xz plane 
        
        float angle = Vector3.Angle(gazeVectorOnXZ, targetLandmarkGroundTruthOnXZ);
        Debug.Log("After xz-plane Casting -- gazeAngleError: " + angle + ";" + "targetLandmarkGroundTruth" + targetLandmarkGroundTruthOnXZ + ";" + "gazeVector" + gazeVectorOnXZ + ";");
        return angle;
    }

    private void UpdateTrialIDText()
    {
        if (trialIDText != null)
        {
            trialIDText.text = "Trial #: " + (trialID+1).ToString() + "/" + trialRepetition.ToString();
        }
    }



    private void FindRightController()
    {
        // Find Controllers
        var RightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, RightHandDevices);

        if (RightHandDevices.Count == 1)
        {
            righthand = RightHandDevices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", righthand.name, righthand.role.ToString()));
        }
        else if (RightHandDevices.Count > 1)
        {
            Debug.Log("Found more than one right hand!");
        }
    }

    private List<List<float>> GenerateCombinations()
    {
        List<List<float>> combinations = new List<List<float>>();

        foreach (DwellTime nearTime in Enum.GetValues(typeof(DwellTime)))
        {
            foreach (DwellTime farTime in Enum.GetValues(typeof(DwellTime)))
            {
                List<float> combination = new List<float> { (float)nearTime, (float)farTime };
                combinations.Add(combination);
            }
        }

        return combinations;
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int rnd = random.Next(i, n);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    private void PrintCombinations(List<List<float>> combinations)
    {
        Debug.Log("The length of the combinations is:" + combinations.Count);
        Debug.Log("Randomized combinations of DwellTime and DwelledObject:");
        foreach (var combination in combinations)
        {
            Debug.Log($"Near: {combination[0]}ms, Far: {combination[1]}ms");
        }
    }
}
