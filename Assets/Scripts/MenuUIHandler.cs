using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuUIHandler : MonoBehaviour
{
    public void UploadVideo()
    {
        SceneManager.LoadScene("VideoUploadScene");
    }

    public void Move2StartScene()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void Move2VedioEditScene()
    {
        SceneManager.LoadScene("VideoEditScene");
    }

    public void Move2Inferecne()
    {
        SceneManager.LoadScene("InferenceScene");
    }
}
