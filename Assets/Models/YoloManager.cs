using System.Collections.Generic;
using TMPro;
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

    public bool isLiveCamera = false;
    public bool isYoloPoseModel = false;

    public Button playPauseButton;
    private bool isPaused = false;
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
    async void Update()
    {
        Texture inputTexture;

        if (videoPlayer.texture == null)
            return;

        inputTexture = videoPlayer.texture;

        //calculate fps
        float deltaTime = Time.deltaTime;
        float fpsValue = 1.0f / deltaTime;

        int result = 0;
        result = await yoloPoseModel.ExecuteModel(inputTexture);
    }

    void Prepared(VideoPlayer vp)
    {
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
