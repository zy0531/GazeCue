using UnityEngine;
using System.Collections.Generic;

public class CounterpartFinder : MonoBehaviour
{
    // Name Template
    public string nameLandmark = "LandmarkGaze_";
    public string nameLandmarkReplica = "LandmarkReplicasGaze_";

    // Method to find the counterpart GameObject based on the naming convention
    public GameObject FindCounterpart(GameObject obj)
    {
        string counterpart_name;

        if (obj.name.StartsWith(nameLandmark))
        {
            counterpart_name = obj.name.Replace(nameLandmark, nameLandmarkReplica) + "(Clone)";
        }
        else  // if (obj.name.StartsWith("LandmarkReplicasGaze_"))
        {
            counterpart_name = obj.name.Replace(nameLandmarkReplica, nameLandmark).Replace("(Clone)", "");
        }
        // Find the counterpart GameObject in the scene
        GameObject counterpart = GameObject.Find(counterpart_name);

        return counterpart;
    }


}
