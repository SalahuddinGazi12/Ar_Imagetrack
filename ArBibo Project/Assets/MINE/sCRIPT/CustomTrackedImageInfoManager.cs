using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using System.IO;

using System.Collections;
using UnityEngine.Networking;

namespace SKB
{
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class CustomTrackedImageInfoManager : MonoBehaviour
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

        /// <summary>
        /// If an image is detected but no source texture can be found,
        /// this texture is used instead.
        /// </summary>
        public Texture2D defaultTexture
        {
            get { return m_DefaultTexture; }
            set { m_DefaultTexture = value; }
        }
        
        public string[] imageAPIUrl; 
        public string[] modelAPIUrl; 
        public string[] id;
        string baseUrl = "https://dev.customerapp.bibomart.net/api/admin/ar/file/image/";
        string baseurlmodel = "https://dev.customerapp.bibomart.net/api/admin/ar/file/assets/";

        ARTrackedImageManager m_TrackedImageManager;
        public GameObject[] content;

        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
            
            content = new GameObject[id.Length];
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
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                Debug.Log("Tracking : " + trackedImage.referenceImage.name);
            }

            // Disable the visual plane if it is not being tracked
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);
                
                for (int i = 0; i < id.Length; i++)
                {
                    if (trackedImage.referenceImage.name == id[i])
                    {
                        content[i].SetActive(true);
                        content[i].transform.position = trackedImage.transform.position;
                    }
                }
            }
            else
            {
                for (int i = 0; i < id.Length; i++)
                {
                    if (trackedImage.referenceImage.name == id[i])
                    {
                        content[i].SetActive(false);
                    }
                }
            }
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
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
                string modelUrl = modelUrls[i];

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
                    //GameObject obj = bundle.LoadAsset<GameObject>(bundle.GetAllAssetNames()[0]);
                    GameObject obj = bundle.LoadAsset<GameObject>(bundle.GetAllAssetNames()[0]);

                    content[i] = Instantiate(obj, parentTransform.transform, true);
                    
                    bundle.Unload(false);
                    
                    content[i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    
                    content[i].SetActive(false);

                    Debug.Log("Asset Bundle Loaded " + i);
                }
            }
        }
    }
}

