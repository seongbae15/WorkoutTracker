using UnityEngine;
using TMPro;
using System.Collections;

public class TMPBlinkingText : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    private Color originalColor;
    private float blinkSpeed = 1.0f; // Time in seconds between blinks

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalColor = tmpText.color;
        StartCoroutine(BlinkText());
    }

    IEnumerator BlinkText()
    {
        while (true)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }
}
