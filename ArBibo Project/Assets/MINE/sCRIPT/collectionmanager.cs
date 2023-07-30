using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class collectionmanager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void bktomain()
    {
        SceneManager.LoadScene(0);
    }
    public void qrcodescanner()
    {
        SceneManager.LoadScene(2);
    }
    public void surfacemaanger()
    {
        SceneManager.LoadScene(3);
    }
}
