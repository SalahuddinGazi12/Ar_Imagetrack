using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneDetect : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject modelPrefab;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool hasInstantiatedModel = false;
    private GameObject instantiatedModel;

    void Update()
    {
        if (!hasInstantiatedModel && raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            // Surface detected
            Pose hitPose = hits[0].pose;

            // Instantiate the model prefab
            instantiatedModel = Instantiate(modelPrefab, hitPose.position, hitPose.rotation);
            hasInstantiatedModel = true;
        }
        else if (hasInstantiatedModel && !raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            // No surface detected, destroy the instantiated model
            DestroyModel();
        }
    }

    private void DestroyModel()
    {
        Destroy(instantiatedModel);
        hasInstantiatedModel = false;
    }
}
