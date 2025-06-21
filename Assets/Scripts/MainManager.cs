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

    public void PickVideoFromGallery(System.Action<string> onVideoPicked)
    {
        NativeGallery.GetVideoFromGallery((path) =>
        {
            videoPath = path;
            onVideoPicked?.Invoke(path);
        }, "Select a video");
    }
}
