using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;

    public string videoPath = null;

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

    public bool PickVideoFromGallery()
    {
        NativeGallery.GetVideoFromGallery((path) =>
        {
            if (path != null)
            {
                videoPath = path;
            }
            else
            {
                videoPath = null;
            }
        }, "Select a video");

        return videoPath != null;
    }
}
