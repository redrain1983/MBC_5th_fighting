using System.Collections;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeleteNoticeUI : MonoBehaviour
{
    [Header("UI ����")]
    public Text noticeText;          // ������ �Ϸ�!�� ����
    public float displayTime = 1.2f; // ǥ�� �ð� (��)
    public Color fadeColor = new Color(1, 1, 1, 0); // ����� �� �����

    private Color originalColor;
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (noticeText != null)
        {
            originalColor = noticeText.color;
            noticeText.enabled = false;
        }
    }

    /// <summary>
    /// ������ �Ϸ�!�� �޽��� ǥ�� �Լ�
    /// </summary>
    public void ShowMessage(string message = "���� �Ϸ�!")
    {
        if (noticeText == null) return;

        // ���� ���̵� ���̸� �ߴ�
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(DisplayMessage(message));
    }

    private IEnumerator DisplayMessage(string message)
    {
        noticeText.text = message;
        noticeText.enabled = true;
        noticeText.color = originalColor;

        // ���� �ð� ���
        yield return new WaitForSeconds(displayTime);

        // ���̵�ƿ�
        float fadeTime = 0.5f;
        float elapsed = 0f;
        Color startColor = noticeText.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            noticeText.color = Color.Lerp(startColor, fadeColor, elapsed / fadeTime);
            yield return null;
        }

        noticeText.enabled = false;
    }
}