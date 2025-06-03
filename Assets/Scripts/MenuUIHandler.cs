using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuUIHandler : MonoBehaviour
{
    public void Move2SelectedVideoScene()
    {
        MainManager.Instance.PickVideoFromGallery();
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
