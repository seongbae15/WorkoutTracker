using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;

    public string videoPath;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PickVideoFromGallery()
    {
        NativeGallery.GetVideoFromGallery((path) =>
        {
            Debug.Log("Video path: " + path);
            if (path != null)
            {
                Debug.Log("Video selected: " + path);
                videoPath = path;
                SceneManager.LoadScene("SelectedVideoScene");

            }
        }, "Select a video");
    }
}
