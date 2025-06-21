using UnityEngine;
using UnityEngine.Video;
using Unity.Sentis;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

public class YoloManager : MonoBehaviour
{
    public RunYOLO8nPose yoloPoseModel;

    public BackendType backendType = BackendType.GPUCompute;

    public RawImage displayImage;

    private VideoPlayer videoPlayer;

    public Button playPauseButton;
    private bool isPaused = false;

    private List<Texture2D> processedFrames = new();
    private bool isProcessingDone = false;
    private float fps = 30f;

    void Start()
    {
        // Screen.orientation = ScreenOrientation.Portrait;

        yoloPoseModel.Initialize(backendType, displayImage);

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.url = MainManager.Instance.videoPath;
        videoPlayer.Prepare();

        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        playPauseButton.onClick.AddListener(TogglePlayPause);
    }
    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared. Frame count: " + videoPlayer.frameCount);
        _ = ProcessAllFrames();
    }
    async Task ProcessAllFrames()
    {
        int totalFrames = (int)videoPlayer.frameCount;
        double fps = videoPlayer.frameRate;
        Debug.Log($"Starting frame-by-frame processing: {fps} rate...");

        if (fps <= 1f)
        {
            Debug.LogWarning("FrameRate too low or unknown. Defaulting to 30fps.");
            fps = 30f;
        }

        for (int i = 0; i < totalFrames; i++)
        {
            double time = i / fps;
            videoPlayer.time = time;
            videoPlayer.Play();
            
            // 프레임이 실제로 적용됐는지 대기
            while (videoPlayer.time < time)
            {
                Debug.Log(videoPlayer.frame + "|| " + i + " / " + totalFrames);
                await Task.Yield();
            }

            videoPlayer.Pause();

            // 비디오 텍스처가 준비됐는지 체크
            if (videoPlayer.texture == null)
            {
                Debug.LogWarning($"[Skipped] Texture is null at frame {i}");
                continue;
            }

            await Task.Delay(30); // GPU 렌더링 마무리 대기

            var tex = videoPlayer.texture;
            var rendered = await yoloPoseModel.ExecuteModelAndRenderToTexture(tex);
            processedFrames.Add(rendered);
        }

        videoPlayer.Stop();
        videoPlayer.enabled = false;

        isProcessingDone = true;
        Debug.Log("All frames processed. Starting playback...");
        StartCoroutine(PlayProcessedFrames((float)fps));
    }

    IEnumerator PlayProcessedFrames(float fps=30f)
    {
        float delay = 1f / fps;
        Debug.Log(processedFrames.Count);
        while (true)
        {
            for (int i = 0; i < processedFrames.Count; i++)
            {
                displayImage.texture = processedFrames[i];
                yield return new WaitForSeconds(delay);
            }
        }
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
