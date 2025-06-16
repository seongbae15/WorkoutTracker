using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarnPopupWIndow : MonoBehaviour
{
    public Button confirmButton;
    private Action onConfirm;

    void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            gameObject.SetActive(false);
        });
    }

    public void Show(string message, Action onConfirmAction)
    {
        TextMeshProUGUI messageText = GetComponentInChildren<TextMeshProUGUI>();
        messageText.text = message;

        onConfirm = onConfirmAction;
        gameObject.SetActive(true);
    }
}
