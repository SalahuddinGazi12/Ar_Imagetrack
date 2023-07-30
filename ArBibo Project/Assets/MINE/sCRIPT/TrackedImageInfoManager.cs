using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using System.IO;

using System.Collections;
using UnityEngine.Networking;

namespace UnityEngine.XR.ARFoundation.JohnBui
{

    [RequireComponent(typeof(ARTrackedImageManager))]
    public class TrackedImageInfoManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The camera to set on the world space UI canvas for each instantiated image info.")]
        Camera m_WorldSpaceCanvasCamera;

        /// <summary>
        /// The prefab has a world space UI canvas,
        /// which requires a camera to function properly.
        /// </summary>
        public Camera worldSpaceCanvasCamera
        {
            get { return m_WorldSpaceCanvasCamera; }
            set { m_WorldSpaceCanvasCamera = value; }
        }

        [SerializeField]
        [Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
        Texture2D m_DefaultTexture;

        public string assetName = "Cube";
        public string bundleName = "model01";
        public GameObject rootGameObject;
        public bool isLoad = true;

        GameObject content;

        public string[] imageAPIUrl; 
        public string[] modelAPIUrl; 
        public string[] id;
        string baseUrl = "https://dev.customerapp.bibomart.net/api/admin/ar/file/image/";
        string baseurlmodel = "https://dev.customerapp.bibomart.net/api/admin/ar/file/assets/";
        public Texture2D defaultTexture
        {
            get { return m_DefaultTexture; }
            set { m_DefaultTexture = value; }
        }

        ARTrackedImageManager m_TrackedImageManager;

        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
            
            imageAPIUrl = new string[id.Length];
            modelAPIUrl = new string[id.Length];

            for (int i = 0; i < id.Length; i++)
            {
                imageAPIUrl[i] = baseUrl + id[i] + ".bin";
                modelAPIUrl[i] = baseurlmodel + id[i] + ".android.bin";
            }

              
            StartCoroutine(LoadModels(imageAPIUrl, modelAPIUrl, this.gameObject.transform));
        }

        void OnEnable()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        void UpdateInfo(ARTrackedImage trackedImage)
        {
            string nameImg = trackedImage.referenceImage.name;
            var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
            var planeGo = planeParentGo.transform.GetChild(0).gameObject;
            
            //switch (nameImg)
            //{
            //    case "One":
            //        // Handle image "One"
            //        // Instantiate the corresponding model or perform other actions
            //        // Example: Instantiate model01 prefab
            //        modelAPIUrl[0] = baseurlmodel + id[0] + ".android.bin";
            //        StartCoroutine(LoadModel(modelAPIUrl, this.gameObject.transform));
            //        break;
            
            //    case "Two":
            //        // Handle image "Two"
            //        // Instantiate the corresponding model or perform other actions
            //        // Example: Instantiate model02 prefab
            //        modelAPIUrl[1] = baseurlmodel + id[1] + ".android.bin";
            //        StartCoroutine(LoadModel(modelAPIUrl, this.gameObject.transform));
            //        break;
            
            //    // Add more cases for other image names as needed
            
            //    default:
            //        // Default behavior
            //        break;
            //}
            if (nameImg == "One")
            {
                if (!isLoad)
                {
                    content.transform.position = planeParentGo.transform.position;
                    content.transform.rotation = planeParentGo.transform.rotation;
                    content.transform.localScale = planeParentGo.transform.localScale;
                    isLoad = true;
                }
            }
            var canvas = trackedImage.GetComponentInChildren<Canvas>();
            canvas.worldCamera = worldSpaceCanvasCamera;
            
            
            if (trackedImage.trackingState != TrackingState.None)
            {
                planeGo.SetActive(true);
            
                // The image extents is only valid when the image is being tracked
                trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);
            
                // Set the texture
                var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
                material.mainTexture = (trackedImage.referenceImage.texture == null) ? defaultTexture : trackedImage.referenceImage.texture;
                
                Debug.Log("Tracked: " + trackedImage.referenceImage.name);
            }
            else
            {
                Debug.Log("Not Tracked: " + trackedImage.referenceImage.name);
                planeGo.SetActive(false);
            }
        }

        //void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        //{
        //    foreach (var trackedImage in eventArgs.added)
        //    {
        //        // Give the initial image a reasonable default scale
        //        trackedImage.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //        UpdateInfo(trackedImage);
        //    }
        //    foreach (var trackedImage in eventArgs.updated)
        //        UpdateInfo(trackedImage);
        //}
        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                trackedImage.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                
                UpdateInfo(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
            {
                UpdateInfo(trackedImage);
            }
        }
        
        private IEnumerator LoadModels(string[] imageUrls, string[] modelUrls, Transform parentTransform)
        {
            for (int i = 0; i < imageUrls.Length; i++)
            {
                string imageUrl = imageUrls[i];
                string modelUrl = modelUrls[i];

                // Download the image texture from the API
                UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageUrl);
                yield return imageRequest.SendWebRequest();

                if (imageRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download image: " + imageRequest.error);
                    yield break;
                }
                
                Debug.Log("Image Downloaded");
                
                Texture2D imageTexture = DownloadHandlerTexture.GetContent(imageRequest);

                // Download the asset bundle from the API
                using (UnityWebRequest modelRequest = UnityWebRequestAssetBundle.GetAssetBundle(modelUrl))
                {
                    yield return modelRequest.SendWebRequest();

                    // Check if the request encountered an error
                    if (modelRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Failed to download asset bundle: " + modelRequest.error);
                        yield break;
                    }

                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(modelRequest);
                    GameObject obj = bundle.LoadAsset<GameObject>(bundle.GetAllAssetNames()[0]);
                    content = Instantiate(obj, parentTransform.transform, true);
                    isLoad = false;
                    content.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    //content.gameObject.name = "---Downloaded Content---";

                    // Set the downloaded texture to the content object
                    var material = content.GetComponentInChildren<MeshRenderer>().material;
                    material.mainTexture = imageTexture;
                    
                    Debug.Log("Asset Bundle Loaded ");
                }
            }
        }
        
        //private IEnumerator LoadModel(string modelUrls, Transform parentTransform)
        //{

        //        // Download the asset bundle from the API
        //        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(modelUrls))
        //        {
        //            yield return request.SendWebRequest();

        //            // Check if the request encountered an error
        //            if (request.result != UnityWebRequest.Result.Success)
        //            {
        //                Debug.LogError("Failed to download asset bundle: " + request.error);
        //                yield break;
        //            }

        //            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        //            GameObject obj = bundle.LoadAsset<GameObject>(bundle.GetAllAssetNames()[0]);
        //            content = Instantiate(obj, parentTransform.transform, true);
        //            isLoad = false;
        //            bundle.Unload(false);
        //            //content.transform.localScale = new Vector3(0, 0, 0);

        //        }
        //    }
        //}
    }
    
}

