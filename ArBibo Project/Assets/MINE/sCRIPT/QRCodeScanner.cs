 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Networking;
using System.IO;
namespace UnityEngine.XR.ARFoundation.JohnBui
{
    
    public class QRCodeScanner : MonoBehaviour
{
    int trackstate;
    public bool isLoad = true;
    int imagebtnstate;
    GameObject content;

    ARTrackedImageManager trackedImageManager;
    void Start()
    {
        trackstate = PlayerPrefs.GetInt("trackstate");
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        imagebtnstate = PlayerPrefs.GetInt("imagebtnstate", imagebtnstate);
    }

    // Update is called once per frame
    void Update() 
    {
        
    }

    private void OnEnable()
      {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
      }

       private void OnDisable()
       {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
       }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            // Check if the tracked image is a QR code
            if (trackedImage.referenceImage.name.StartsWith("asset1"))
            {
             
                trackstate = 1;
                PlayerPrefs.SetInt("trackstate", trackstate);
            
            }
            else if (trackedImage.referenceImage.name.StartsWith("asset2"))
            {
                trackstate = 2;
                PlayerPrefs.SetInt("trackstate", trackstate);
            }
        }
    }

      public void image_btn(int btn)
        {
            imagebtnstate = btn;
            PlayerPrefs.SetInt("imagebtnstate", imagebtnstate);
            PlayerPrefs.Save();
        }

   
 }
}

