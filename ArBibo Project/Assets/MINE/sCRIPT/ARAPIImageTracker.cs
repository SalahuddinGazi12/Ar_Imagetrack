using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Networking;

public class ARAPIImageTracker : MonoBehaviour
{
    public string imageAPIUrl = "https://dev.customerapp.bibomart.net/api/admin/ar/file/image/6421c4369412c22620e988cf.bin";
    public string modelAPIUrl = "https://dev.customerapp.bibomart.net/api/admin/ar/file/assets/6421c4369412c22620e988cf.android.bin";
    public ARTrackedImageManager trackedImageManager;
   // public GameObject loadingIndicator;

    private Dictionary<string, GameObject> loadedModels = new Dictionary<string, GameObject>();
    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();

    }
    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    [System.Obsolete]
    private void Start()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        }
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        StartCoroutine(LoadReferenceImage());
    }

    [System.Obsolete]
    private IEnumerator LoadReferenceImage()
    {
        // Download the image from the API
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageAPIUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download image: " + www.error);
            yield break;
        }

        // Convert the image data to a texture
        Texture2D imageTexture = DownloadHandlerTexture.GetContent(www);
        if (trackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            mutableLibrary.ScheduleAddImageJob(imageTexture, "my new image", 0.5f /* 50 cm */);
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // If the tracked image is not yet associated with a model, associate it with the loaded model prefab
                if (trackedImage.referenceImage.name == "my new image" && trackedImage.transform.childCount == 0)
                {
                    StartCoroutine(LoadModel(modelAPIUrl, trackedImage.transform));
                }
            }
            else if (trackedImage.trackingState == TrackingState.None)
            {
                // If the tracked image is lost, destroy the associated model
                if (trackedImage.transform.childCount > 0)
                {
                    Destroy(trackedImage.transform.GetChild(0).gameObject);
                }
            }
        }
    }

    private IEnumerator LoadModel(string modelName, Transform parentTransform)
    {
        // Download the asset bundle from the API
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(modelAPIUrl))
        {
            yield return request.SendWebRequest();

            // Check if the request encountered an error
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download asset bundle: " + request.error);
                yield break;
            }

            // Get the loaded asset bundle from the UnityWebRequest
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);

            // Load the model from the asset bundle
            GameObject modelPrefab = assetBundle.LoadAsset<GameObject>(modelName);

            // Instantiate the model in the scene
            GameObject modelObject = Instantiate(modelPrefab, parentTransform.position, parentTransform.rotation);

            // Add the loaded model to the dictionary
            loadedModels.Add(modelName, modelObject);

            // Unload the asset bundle
            assetBundle.Unload(false);

            // Hide the loading indicator
            //  loadingIndicator.SetActive(false);
        }
    }
}