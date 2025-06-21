using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices; // WebGL JavaScript Interop을 위해 추가
using UnityEngine.UI; // RawImage를 사용하기 위해 추가

public class VideoManager : MonoBehaviour
{
    public WarnPopupWIndow warnPopupModal; // 경고 팝업 모달
    public VideoPlayer videoPlayer;        // 비디오 플레이어 컴포넌트
    public RawImage videoDisplay;         // 비디오를 표시할 UI RawImage (인스펙터에서 연결)
    public double limitDuration = 15.0;    // 비디오 길이 제한 (15초)

    // 비디오를 렌더링할 RenderTexture. 코드에서 생성하고 연결합니다.
    private RenderTexture videoRenderTexture; 

    // JavaScript 함수를 C#에서 호출하기 위한 선언.
    // 이 함수는 'Assets/Plugins/WebGLFileUploader.jslib' 파일에 정의됩니다.
    [DllImport("__Internal")]
    private static extern void OpenFileSelection();

    void Awake()
    {
        // Inspector에서 컴포넌트들이 연결되었는지 다시 한번 확인합니다.
        if (videoPlayer == null || videoDisplay == null || warnPopupModal == null)
        {
            Debug.LogError("Required components (VideoPlayer, RawImage, WarnPopupWIndow) are not assigned in the Inspector or found on this GameObject. Returning to StartScene.");
            warnPopupModal?.Show("Initialization error: Missing components.", () => SceneManager.LoadScene("StartScene"));
            return;
        }

        // RenderTexture 초기화 (씬 로드 시 한 번만 수행)
        if (videoRenderTexture == null)
        {
            videoRenderTexture = new RenderTexture(1920, 1080, 0);
            videoPlayer.targetTexture = videoRenderTexture;
            videoDisplay.texture = videoRenderTexture; // RawImage에 RenderTexture 연결
            videoPlayer.renderMode = VideoRenderMode.RenderTexture; // 렌더 모드 설정
        }
        
        // VideoPlayer의 이벤트 구독
        videoPlayer.prepareCompleted += OnVideoPrepared; // 비디오 준비 완료 시
        videoPlayer.errorReceived += OnVideoError;       // 비디오 오류 발생 시
    }

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Invoke("RequestVideoUpload", 1.0f); // 1초 후 JS 호출
#endif
        ClearRenderTexture(); // 새로운 비디오 로드 전에 기존 내용 (검은 화면) 지우기
        Debug.Log("VideoScene loaded. Automatically prompting user to select video...");
        SelectLocalVideo(); // 씬 로드 즉시 비디오 선택 대화상자 띄우기
    }

    void RequestVideoUpload()
    {
        OpenFileSelection(); // JS 함수 호출
    }

    // --- 사용자가 비디오를 선택할 때 호출될 함수 ---
    public void SelectLocalVideo()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenFileSelection(); // WebGL 빌드에서는 JavaScript 함수 호출 (파일 선택 대화상자)
#else
        // Unity Editor 환경에서 테스트를 위한 더미 비디오 로직
        Debug.LogWarning("Local file selection via web browser is only available in WebGL builds. Running Editor test mode.");

        // 1. 테스트용 비디오 파일 경로 설정
        // 스트리밍 에셋 폴더에 'IMG_3888.mov' 파일을 넣어두세요.
        string testVideoPath = Application.streamingAssetsPath + "/IMG_3888.mov";
        Debug.Log($"Editor test video path: {testVideoPath}");

        // 2. 파일 존재 여부 확인
        if (System.IO.File.Exists(testVideoPath))
        {
            Debug.Log($"Loading dummy video in Editor: {testVideoPath}");
            // 파일 경로 앞에 "file://" 접두사를 붙여야 VideoPlayer가 로컬 파일로 인식합니다.
            StartCoroutine(PlayVideo("file://" + testVideoPath));
        }
        else
        {
            Debug.LogError($"Test video not found at: {testVideoPath}. Please place video file in your Assets/StreamingAssets folder.");
            warnPopupModal?.Show("Test video file not found. Please place 'IMG_3888.mov' in Assets/StreamingAssets.", () => SceneManager.LoadScene("StartScene"));
        }
#endif
    }

    // --- JavaScript에서 Blob URL을 받아오는 함수 (함수명 수정) ---
    // JavaScript의 Unity.SendMessage에서 'OnVideoSelected'라는 이름으로 호출됩니다.
    public void OnVideoSelected(string blobUrl) // 함수명 'OpenFileSelection'에서 'OnVideoSelected'로 변경
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is null. Cannot set video URL from JavaScript.");
            warnPopupModal?.Show("Failed to load video: Video player not ready.", () => SceneManager.LoadScene("StartScene"));
            return;
        }

        Debug.Log("Received video URL from JavaScript: " + blobUrl);
        StartCoroutine(PlayVideo(blobUrl)); // 받은 Blob URL로 비디오 재생 코루틴 시작
    }

    // --- 비디오 재생 로직 (기존 코루틴 활용) ---
    private IEnumerator PlayVideo(string path)
    {
        ClearRenderTexture(); // 새 비디오 로드 전에 RenderTexture 초기화
        
        // 파일 형식 체크는 여기서 수행
        if (!CheckVideoFileFormat(path))
        {
            Debug.LogError("Video file format check failed for path: " + path);
            yield break;
        }

        if (path != null)
        {
            MainManager.Instance.videoPath = path; // 비디오 경로 저장
            Debug.Log(MainManager.Instance.videoPath);
        }

        videoPlayer.url = path;
        videoPlayer.Prepare(); // 비디오 로드 및 준비 시작


        // 비디오 준비 완료까지 대기 (최대 10초 타임아웃)
        float prepareTimeout = 10.0f; 
        float timer = 0f;
        while (!videoPlayer.isPrepared && timer < prepareTimeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 타임아웃 발생 시 처리
        if (!videoPlayer.isPrepared)
        {
            Debug.LogError($"Video preparation failed or timed out after {prepareTimeout:F1}s for: {path}");
            warnPopupModal?.Show("Video preparation failed or timed out. Please try again.", () => SceneManager.LoadScene("StartScene"));
            yield break; // 코루틴 종료
        }
        // 비디오 재생은 OnVideoPrepared에서 처리됩니다.
    }
    
    // --- 비디오 준비 완료 이벤트 핸들러 ---
    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log($"Video prepared successfully: {vp.url}, Duration: {vp.length:F1} seconds");

        if (CheckVideoDuration(vp)) // 비디오 길이 체크
        {
            vp.Play(); // 길이 제한을 통과하면 비디오 재생
        }
        else
        {
            vp.Stop(); // 길이 제한에 걸리면 재생 중지
            // warnPopupModal.Show()는 CheckVideoDuration 내부에서 호출됨
        }
    }

    // --- 비디오 오류 발생 시 호출되는 핸들러 (수정된 부분) ---
    // OnVideoError 함수 시그니처를 VideoPlayer.ErrorEventHandler 델리게이트와 일치시킴
    private void OnVideoError(VideoPlayer vp, string message) // 'VideoClip clip' 파라미터 추가
    {
        Debug.LogError($"Video Playback Error: {message} for URL: {vp.url}");
        warnPopupModal?.Show($"Video playback error: {message}. Please select another file.", () => SceneManager.LoadScene("StartScene"));
    }

    // --- 비디오 파일 포맷 체크 (Blob URL 처리 추가) ---
    private bool CheckVideoFileFormat(string videoPath)
    {
        // Blob URL은 확장자가 없으므로, 'blob:'으로 시작하면 유효한 것으로 간주
        if (videoPath.StartsWith("blob:")) return true;
        
        // Editor 테스트를 위한 로컬 파일 경로 처리 (file:// 접두사 제거)
        string processedPath = videoPath;
        if (processedPath.StartsWith("file:///")) // Windows
        {
            processedPath = processedPath.Substring("file:///".Length);
        }
        else if (processedPath.StartsWith("file://")) // macOS/Linux
        {
            processedPath = processedPath.Substring("file://".Length);
        }

        string extension = System.IO.Path.GetExtension(processedPath)?.ToLower(); 
        if (extension != ".mp4" && extension != ".mov")
        {
            warnPopupModal?.Show("Unsupported video format. Please select an MP4 or MOV file.", () => SceneManager.LoadScene("StartScene"));
            return false;
        }
        return true;
    }

    // --- 비디오 길이 체크 ---
    private bool CheckVideoDuration(VideoPlayer videoPlayer)
    {
        if (videoPlayer.length > limitDuration)
        {
            warnPopupModal?.Show($"Video duration ({videoPlayer.length:F1}s) exceeds the limit of {limitDuration:F1} seconds.", () => SceneManager.LoadScene("StartScene"));
            return false;
        }
        return true;
    }

    // --- RenderTexture 초기화 (화면을 검게 지움) ---
    private void ClearRenderTexture()
    {
        if (videoPlayer != null && videoPlayer.targetTexture != null)
        {
            RenderTexture rt = videoPlayer.targetTexture as RenderTexture;
            if (rt != null)
            {
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = null;
            }
        }
    }

    void OnDestroy()
    {
        // 스크립트가 파괴될 때 이벤트 구독 해제 및 RenderTexture 메모리 해제
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError; 
        }
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
            Destroy(videoRenderTexture);
        }
    }
}