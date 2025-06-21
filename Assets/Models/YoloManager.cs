using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Unity.Sentis;
using UnityEngine.UI;
using Unity.VisualScripting;

public class YoloManager : MonoBehaviour
{
    public RunYOLO8nPose yoloPoseModel;

    public BackendType backendType = BackendType.GPUCompute;

    public RawImage displayImage;

    private VideoPlayer videoPlayer;

    public bool isLiveCamera = false;
    public bool isYoloPoseModel = false;

    public Button playPauseButton;
    private bool isPaused = false;

    private bool isProcessingAllFrames = true;
    private bool isReplayMode = false;
    private int replayFrameIndex = 0;
    private float frameTimer = 0f;
    private float frameInterval = 0f;

    private long totalFrame = 0;

    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.url = MainManager.Instance.videoPath;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += Prepared;

        playPauseButton.onClick.AddListener(TogglePlayPause);

        yoloPoseModel.Initialize(backendType, displayImage);
    }
    // async void Update()
    // {
    //     Texture inputTexture;

    //     if (videoPlayer.texture == null)
    //         return;

    //     inputTexture = videoPlayer.texture;

    //     //calculate fps
    //     float deltaTime = Time.deltaTime;
    //     float fpsValue = 1.0f / deltaTime;

    //     int result = 0;
    //     result = await yoloPoseModel.ExecuteModel(inputTexture);
    // }

    async void Update()
    {
        if (videoPlayer.texture == null)
            return;

        // Loop 중 처음 실행 시 프레임 간 시간 설정

        if (isProcessingAllFrames)
        {
            Texture inputTexture = videoPlayer.texture;

            await yoloPoseModel.ExecuteModel(inputTexture);

            // 다음 프레임으로 이동
            videoPlayer.StepForward();
            
            Debug.Log(videoPlayer.frameCount + " / " + videoPlayer.frame + " / " + totalFrame);
            // 마지막 프레임 도달 시
            if (videoPlayer.frame >= totalFrame - 1)
            {
                Debug.Log(1);
                videoPlayer.Pause();
                isProcessingAllFrames = false;
                isReplayMode = true;
                replayFrameIndex = 0;
                Debug.Log("isReplayMode: " + isReplayMode);
            }
        }
        else if (isReplayMode)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameInterval)
            {
                frameTimer = 0f;

                if (replayFrameIndex < yoloPoseModel.poseFrames.Count)
                {
                    displayImage.texture = yoloPoseModel.poseFrames[replayFrameIndex];
                    replayFrameIndex++;
                }
                else
                {
                    replayFrameIndex = 0; // 다시 처음부터 보여주기
                }
            }
        }
    }
    public void RestartInference()
    {
        isProcessingAllFrames = true;
        isReplayMode = false;
        replayFrameIndex = 0;
        frameTimer = 0f;
        frameInterval = 0f;
        yoloPoseModel.poseFrames.Clear();
        videoPlayer.frame = 0;
        videoPlayer.Play();
    }


    void Prepared(VideoPlayer vp)
    {
        frameInterval = 1f / (float)videoPlayer.frameRate;
        totalFrame = (long)videoPlayer.frameCount;

        videoPlayer.frame = 0;   // 이제 유효함
        videoPlayer.Play();
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
