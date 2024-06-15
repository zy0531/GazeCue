using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmarkPlacement : MonoBehaviour
{
    public List<GameObject> ImageOfTargetLandmarks;
    public List<GameObject> landmarkPlacements;

    private System.Random random = new System.Random();

    void Start()
    {

    }



    // Call it after each dwellTime combination
    public void GenerateLandmarkPlacement()
    {
        ShuffleList(ImageOfTargetLandmarks);
        ShuffleList(landmarkPlacements);
    }

    public void ActiveLandmarkPlacement(int trialID)
    {
        // Check if trialID is within the bounds of the list
        if (trialID < 0 || trialID >= landmarkPlacements.Count)
        {
            Debug.LogWarning("Invalid trialID");
            return;
        }

        // Loop through the list and set active state
        for (int i = 0; i < landmarkPlacements.Count; i++)
        {
            landmarkPlacements[i].SetActive(i == trialID);
        }
    }

    public void ActiveTargetLandmarkImage(int trialID)
    {
        // Check if trialID is within the bounds of the list
        if (trialID < 0 || trialID >= ImageOfTargetLandmarks.Count)
        {
            Debug.LogWarning("Invalid trialID");
            return;
        }

        // Loop through the list and set active state
        for (int i = 0; i < ImageOfTargetLandmarks.Count; i++)
        {
            ImageOfTargetLandmarks[i].SetActive(i == trialID);
        }
    }
    public string GetLandmarkPlacementName(int trialID)
    {
        return landmarkPlacements[trialID].name;
    }

    public string GetTargetLandmarkImageName(int trialID)
    {
        return ImageOfTargetLandmarks[trialID].name;
    }

    public Vector3 GetTargetLandmarkTransform(int trialID)
    {
        // Check if trialID is within bounds
        if (trialID < 0 || trialID >= ImageOfTargetLandmarks.Count || trialID >= landmarkPlacements.Count)
        {
            Debug.LogWarning("Invalid trialID");
            return Vector3.zero;
        }

        // Get the name of the target image
        string TargetImageName = ImageOfTargetLandmarks[trialID].name;

        // Get the parent transform to search through its children
        Transform parent = landmarkPlacements[trialID].transform;
        if (parent == null)
        {
            Debug.LogWarning("Parent transform is null for landmarkPlacements[" + trialID + "]");
            return Vector3.zero;
        }

        // Iterate through children to find the matching name
        foreach (Transform child in parent)
        {
            if (CompareNamesByLastTwoChars(child.name, TargetImageName))
            {
                Debug.Log(child.name + " matches " + TargetImageName);
                return child.position;
            }
        }

        // if no matching child found
        Debug.LogWarning("No child found with matching name pattern for " + TargetImageName);
        return Vector3.zero;
    }

    public void DestroyAllChildren(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }


    public bool CompareNamesByLastTwoChars(string name1, string name2)
    {
        if (name1.Length < 2 || name2.Length < 2)
        {
            Debug.LogWarning("One or both names are too short to compare the last two characters.");
            return false;
        }

        string lastTwoChars1 = name1.Substring(name1.Length - 2);
        string lastTwoChars2 = name2.Substring(name2.Length - 2);

        return lastTwoChars1 == lastTwoChars2;
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

    private void GetCombinations()
    {
        Debug.Log("Combinations of targetLandmarks and landmarkPlacements after randomization:");
        for (int i = 0; i < ImageOfTargetLandmarks.Count; i++)
        {
            Debug.Log($"Landmark: {ImageOfTargetLandmarks[i].name} -> Placement: {landmarkPlacements[i].name}");
        }
    }



}
