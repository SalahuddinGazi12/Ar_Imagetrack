 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace UnityEngine.XR.ARFoundation.JohnBui
{
    public class ImageManager : MonoBehaviour
    {
        public Button[] buttons;
        public string[] id;
        public string baseUrl = "https://dev.customerapp.bibomart.net/api/admin/ar/file/image/";
        public bool isLoad = true;
        GameObject content;

        void Start()
        {
            //for (int i = 0; i < buttons.Length; i++)
            //{
            //    //if (ShouldDownloadImage(i))
            //    //{

            if (ShouldDownloadImage(0))
            {

                PlayerPrefs.SetInt("state1", 1);
                imagettrack(0);
            }

            else if (ShouldDownloadImage(1))
            {

                PlayerPrefs.SetInt("state2", 1);
                imagettrack(1);
            }


            if (PlayerPrefs.GetInt("state1") == 1)
            {
                imagettrack(0);
            }
            else if (PlayerPrefs.GetInt("state2") == 1)
            {
                imagettrack(1);
            }
            //}
        }

        public void imagettrack(int i)
        {
            string imageUrl = baseUrl + id[i] + ".bin";
            StartCoroutine(DownloadImage(imageUrl, sprite =>
            {
                buttons[i].image.sprite = sprite;
                buttons[i].enabled = true;
            }));
        }

        bool ShouldDownloadImage(int index)
        {
            // Customize the condition based on your requirements
            // Example condition: Only download image if trackstate is equal to index + 1
            return PlayerPrefs.GetInt("trackstate") == index + 1;
        }

        IEnumerator DownloadImage(string imageUrl, System.Action<Sprite> callback)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download image: " + request.error);
                    yield break;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                callback?.Invoke(sprite);
            }
        }
        private IEnumerator LoadModel(string modelUrls, Transform parentTransform)
        {

            // Download the asset bundle from the API
            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(modelUrls))
            {
                yield return request.SendWebRequest();

                // Check if the request encountered an error
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download asset bundle: " + request.error);
                    yield break;
                }

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                GameObject obj = bundle.LoadAsset<GameObject>(bundle.GetAllAssetNames()[0]);
                content = Instantiate(obj, parentTransform.transform, true);
                isLoad = false;
                bundle.Unload(false);
                //content.transform.localScale = new Vector3(0, 0, 0);

            }
        }
    }
}
