using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VideoManager : MonoBehaviour
{
    public WarnPopupWIndow warnPopupModal;
    public VideoPlayer videoPlayer;
    double limitDuration = 15.0; // 15 seconds
    public RawImage videoDisplay;


    void Start()
    {
        ClearRenderTexture();

        MainManager.Instance.PickVideoFromGallery((path) =>
        {
            if (string.IsNullOrEmpty(path))
            {
                SceneManager.LoadScene("StartScene");
                return;
            }

            if (!CheckVideoFileFormat(path))
            {
                return;
            }

            StartCoroutine(PlayVideo(path));
        });
    }

    private IEnumerator PlayVideo(string videoPath)
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        if (videoPlayer.targetTexture != null && videoDisplay != null)
        {
            videoDisplay.texture = videoPlayer.targetTexture;
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
        videoPlayer.Stop();
        RenderTexture rt = videoPlayer.targetTexture;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
        videoPlayer.targetTexture.Release();
        videoPlayer.clip = null;
        videoPlayer.url = null;
    }
}
