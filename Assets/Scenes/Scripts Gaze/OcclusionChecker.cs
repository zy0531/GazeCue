using System.Collections.Generic;
using UnityEngine;

public class OcclusionChecker : MonoBehaviour
{
    public Camera mainCamera; // Assign the main camera in the Inspector
    // public string targetObject_name; // Assign the target GameObject in the Inspector for testing
    public int gridResolution = 5; // Resolution of the grid -- The number of divisions along each axis of the bounds to generate sample points.

/*   // for testing
 *    void FixedUpdate()
    {
        # CheckOcclusion(GameObject.Find(targetObject_name));
    }*/

    /// <summary>
    /// check the occlusion ratio of a gameobject with uniformly random sampling
    /// </summary>
    /// <param name="targetObject"></param>
    /// <returns>occlusionRatio</returns>
    public float CheckOcclusion(GameObject targetObject)
    {
        Collider targetCollider = targetObject.GetComponent<Collider>();
        if (targetCollider == null)
        {
            Debug.LogError("No Collider attached to the target object.");
            return 0f;
        }

        Bounds bounds = targetCollider.bounds;
        int occludedPoints = 0;

        Vector3[] samplePoints = GetGridPointsInBounds(bounds, gridResolution);
        foreach (var point in samplePoints)
        {
            if (IsPointOccluded(point, targetObject))
            {
                occludedPoints++;
            }
        }

        // Debug.Log($"{occludedPoints} out of {samplePoints.Length} points are occluded.");
        float occlusionRatio = (float)occludedPoints / samplePoints.Length;
        // Debug.Log("The occlusionRatio is" + occlusionRatio.ToString("F2"));
        return occlusionRatio;
    }

    // Sample Method: Random sampling
    Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    // Sampling Method: Uniform sampling (Use a grid-based approach instead of random sampling)
    Vector3[] GetGridPointsInBounds(Bounds bounds, int resolution)
    {
        Vector3[] points = new Vector3[resolution * resolution * resolution];
        int index = 0;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    float fx = (float)x / (resolution - 1);
                    float fy = (float)y / (resolution - 1);
                    float fz = (float)z / (resolution - 1);

                    points[index] = new Vector3(
                        Mathf.Lerp(bounds.min.x, bounds.max.x, fx),
                        Mathf.Lerp(bounds.min.y, bounds.max.y, fy),
                        Mathf.Lerp(bounds.min.z, bounds.max.z, fz)
                    );
                    index++;
                }
            }
        }

        return points;
    }


    bool IsPointOccluded(Vector3 point, GameObject targetObject)
    {
        Vector3 direction = point - mainCamera.transform.position;
        float distance = Vector3.Distance(mainCamera.transform.position, point);

        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, direction, out hit, distance))
        {
            return hit.transform != targetObject.transform;
        }
        return false;
    }
}
