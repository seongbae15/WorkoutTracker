using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    double limitDuration = 15.0; // 15 seconds
    void Start()
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

        if (CheckVideoDuration(videoPlayer))
        {
            videoPlayer.Play();
        }
        else
        {
            SceneManager.LoadScene("StartScene");
            yield break;
        }

    }

    private bool CheckVideoFileFormat(string videoPath)
    {
        string extension = System.IO.Path.GetExtension(videoPath);
        if (extension != ".mp4" && extension != ".mov" && extension != ".MP4" && extension != ".MOV")
        {
            return false;
        }
        return true;
    }
    
    private bool CheckVideoDuration(VideoPlayer videoPlayer)
    {
        if (videoPlayer.length > limitDuration)
        {
            return false;
        }
        return true;
    }
}
