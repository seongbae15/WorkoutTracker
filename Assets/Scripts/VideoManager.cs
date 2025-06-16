using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Awake()
    {
        string videoPath = MainManager.Instance.videoPath;
        if (!string.IsNullOrEmpty(videoPath))
        {
            if (!CheckVideoFileFormat(videoPath))
            {
                // Create a popup window.
                SceneManager.LoadScene("StartScene");
                return;
            }
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

    private bool CheckVideoFileFormat(string videoPath)
    {
        string extension = System.IO.Path.GetExtension(videoPath);
        if (extension != ".mp4" && extension != ".mov" && extension != ".MP4" && extension != ".MOV")
        {
            Debug.Log("Unsupported video format: " + extension);
            return false;
        }
        return true;
    }
}
