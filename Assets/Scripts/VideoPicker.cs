using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
public class VideoPicker : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Button selectButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectButton.onClick.AddListener(PickVideoFromGallery);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PickVideoFromGallery()
    {
        NativeGallery.GetVideoFromGallery((path) =>
        {
            Debug.Log("Video path: " + path);
            if (path != null)
            {
                Debug.Log("Video selected: " + path);
            }
        }, "Select a video");
    }

}
