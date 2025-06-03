using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string videoPath = MainManager.Instance.videoPath;
        Debug.Log("SelectedScene: " + videoPath);
        if (!string.IsNullOrEmpty(videoPath))
        {
            StartCoroutine(PlayVideo(videoPath));
        }
    }
    private IEnumerator PlayVideo(string videoPath)
    {
        videoPlayer.url = videoPath;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        videoPlayer.Play();
    }
}
