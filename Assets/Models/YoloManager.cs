using UnityEngine;
using UnityEngine.Video;
using Unity.Sentis;
using UnityEngine.UI;

public class YoloManager : MonoBehaviour
{
    public RunYOLO8nPose yoloPoseModel;

    public BackendType backendType = BackendType.GPUCompute;

    public RawImage displayImage;

    private VideoPlayer videoPlayer;

    public Button playPauseButton;
    private bool isPaused = false;

    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        yoloPoseModel.Initialize(backendType, displayImage);

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.url = MainManager.Instance.videoPath;

        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();

        // videoPlayer.prepareCompleted += Prepared;

        playPauseButton.onClick.AddListener(TogglePlayPause);

    }
    async void Update()
    {
        if (videoPlayer == null || !videoPlayer.isPrepared || videoPlayer.texture == null)
            return;

        Texture inputTexture = videoPlayer.texture;
        await yoloPoseModel.ExecuteModel(inputTexture);
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        videoPlayer.Play();
    }

    void OnVideoError(VideoPlayer vp, string message)
{
        Debug.LogError($"[YoloManager] VideoPlayer Error: {message} (url: {vp.url})");
#if UNITY_WEBGL
        // WebGL에서 오류 발생 시 알림 (예: 브라우저 지원 문제, blob URL 해석 실패 등)
        Application.ExternalEval($"alert('Video playback error: {message}')");
#endif
    }
    public void TogglePlayPause()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            isPaused = true;
        }
        else
        {
            videoPlayer.Play();
            isPaused = false;
        }
    }    
}
