using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public class TrackImageFromAPI : MonoBehaviour
{
    public string imageUrl = "https://dev.customerapp.bibomart.net/api/admin/ar/file/image/6421c4369412c22620e988cf.bin";
    
    public string apiUrl = "https://dev.customerapp.bibomart.net/api/admin/ar/file/assets/6421c4369412c22620e988cf.android.bin";
    public GameObject[] models;
    [SerializeField]
    ARTrackedImageManager m_TrackedImageManager;

    // Start is called before the first frame update
    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    [System.Obsolete]
    void Start()
    {
       

        // Start a coroutine to download and track the image from the API
        StartCoroutine(LoadImage(imageUrl));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                StartCoroutine(LoadModel(trackedImage));
            }
            else if (trackedImage.trackingState == TrackingState.None)
            {
                Destroy(trackedImage.gameObject);
            }
        }
    }
    IEnumerator LoadModel(ARTrackedImage trackedImage)
    {
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(apiUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {

           




           // byte[] assetData = request.downloadHandler.data;

            // Load the asset from the downloaded data
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);

            string rootAssetPath = assetBundle.GetAllAssetNames()[0];
            GameObject arObject = Instantiate(assetBundle.LoadAsset(rootAssetPath) as GameObject);
            arObject.transform.SetParent(trackedImage.transform);
            arObject.transform.localPosition = Vector3.zero;
            arObject.transform.localRotation = Quaternion.identity;


            // Load the asset from the downloaded data

            //GameObject prefab = assetBundle.LoadAsset<GameObject>(assetBundle.GetAllAssetNames()[0]);
            //int modelIndex = int.Parse(request.downloadHandler.text);
            //prefab = Instantiate(models[modelIndex]);
            //prefab.transform.SetParent(trackedImage.transform);
            //prefab.transform.localPosition = Vector3.zero;
            //prefab.transform.localRotation = Quaternion.identity;
        }
    }

    [System.Obsolete]
    IEnumerator LoadImage(string url)
    {
        // Download the image from the API
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download image: " + www.error);
            yield break;
        }

        // Convert the image data to a texture
        Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
       // Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        // Create a new ARTrackedImage object
        if (m_TrackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            mutableLibrary.ScheduleAddImageJob(texture, "my new image", 0.5f /* 50 cm */);
        }
        // Start the AR session
        m_TrackedImageManager.enabled = true;
    }

    //[System.Obsolete]
    //void AddImage(Texture2D imageToAdd)
    //{
    //    if (m_TrackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
    //    {
    //        mutableLibrary.ScheduleAddImageJob(imageToAdd,"my new image",0.5f /* 50 cm */);
    //    }
    //}
    IEnumerator GetRequestimage1(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {


            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;


            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            //Button button = GameObject.Find("Button1").GetComponent<Button>();
            // button.image.sprite = sprite;




        }
        else
        {
            Debug.LogError($"Error: {www.error}");
        }


        //UnityWebRequest webRequest = UnityWebRequest.Get(url);
        //yield return webRequest.SendWebRequest();

        //if (webRequest.result == UnityWebRequest.Result.Success)
        //{// Get the downloaded asset data as bytes
        //    //canvs.SetActive(false);
        //    byte[] assetData = webRequest.downloadHandler.data;

        //    // Load the asset from the downloaded data
        //    AssetBundle assetBundle = AssetBundle.LoadFromMemory(assetData);
        //    GameObject prefab = assetBundle.LoadAsset<GameObject>(assetBundle.GetAllAssetNames()[0]);
        //}
    }
}
