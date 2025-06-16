using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuUIHandler : MonoBehaviour
{
    public GameObject savePopupModal;
    void Start()
    {
        if (savePopupModal != null)
        {
            savePopupModal.SetActive(false);
        }
    }
    public void Move2SelectedVideoScene()
    {
        bool isVideoPicked = MainManager.Instance.PickVideoFromGallery();
        if (isVideoPicked)
        {
            SceneManager.LoadScene("SelectedVideoScene");
        }
        else
        {
            Move2StartScene();
        }
    }

    public void Move2StartScene()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void Move2Inference()
    {
        SceneManager.LoadScene("InferenceScene");
    }

    public void ClickDoneButton()
    {
        if (savePopupModal != null)
        {
            savePopupModal.SetActive(true);
        }
    }

    public void MoveToStartScene(bool isSave)
    {
        if (isSave)
        {
            Debug.Log("Save the video");
        }
        else
        {
            Debug.Log("Do not save the video");
        }
        SceneManager.LoadScene("StartScene");
    }

    public void SelectVideo()
    {
        bool isVideoPicked = MainManager.Instance.PickVideoFromGallery();
        if (isVideoPicked)
        {
            SceneManager.LoadScene("SelectedVideoScene");
        }
        else
        {
            SceneManager.LoadScene("StartScene");
        }
    }

}
