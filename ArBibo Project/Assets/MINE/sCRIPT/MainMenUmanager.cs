using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenUmanager : MonoBehaviour
{
    public GameObject pointcamera;
    void Start()
    {
        Invoke("pointcameratoimage", 3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void collectionmenu()
    {
        SceneManager.LoadScene(1);
    }
    public void takess()
    {
        ScreenCapture.CaptureScreenshot("screenshot " + System.DateTime.Now.ToString("MM-dd-yy (HH-mm-ss)") + ".png");
    }
    public void crossthegame()
    {
        Application.Quit();
    }
    public void pointcameratoimage()
    {
        pointcamera.SetActive(false);
    }
}
