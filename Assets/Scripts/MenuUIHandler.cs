using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuUIHandler : MonoBehaviour
{
    public void CheckVideo()
    {
        SceneManager.LoadScene("SelectedVideoScene");
    }

    public void Move2StartScene()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void Move2Inferecne()
    {
        SceneManager.LoadScene("InferenceScene");
    }
}
