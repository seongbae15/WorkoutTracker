using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoManager : MonoBehaviour
{
    public WarnPopupWIndow warnPopupModal;
    public VideoPlayer videoPlayer;
    double limitDuration = 15.0; // 15 seconds
    void Start()
    {
        ClearRenderTexture();
        string videoPath = MainManager.Instance.videoPath;
        if (!string.IsNullOrEmpty(videoPath))
        {
            if (!CheckVideoFileFormat(videoPath))
            {
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
            yield break;
        }

    }

    private bool CheckVideoFileFormat(string videoPath)
    {
        string extension = System.IO.Path.GetExtension(videoPath).ToLower();
        if (extension != ".mp4" && extension != ".mov")
        {
            warnPopupModal.Show("Unsupported video format. Please select an MP4 or MOV file.", () =>
            {
                SceneManager.LoadScene("StartScene");
            });
            return false;
        }
        return true;
    }

    private bool CheckVideoDuration(VideoPlayer videoPlayer)
    {
        if (videoPlayer.length > limitDuration)
        {
            warnPopupModal.Show("Video duration exceeds the limit of 15 seconds.", () =>
            {
                SceneManager.LoadScene("StartScene");
            });
            return false;
        }
        return true;
    }

    private void ClearRenderTexture()
    {
        if (videoPlayer.targetTexture != null)
        {
            RenderTexture rt = videoPlayer.targetTexture;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }
    }
}
