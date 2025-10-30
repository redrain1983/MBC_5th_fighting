using System.Collections;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeleteNoticeUI : MonoBehaviour
{
    [Header("UI 설정")]
    public Text noticeText;          // “삭제 완료!” 문구
    public float displayTime = 1.2f; // 표시 시간 (초)
    public Color fadeColor = new Color(1, 1, 1, 0); // 사라질 때 투명색

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
    /// “삭제 완료!” 메시지 표시 함수
    /// </summary>
    public void ShowMessage(string message = "삭제 완료!")
    {
        if (noticeText == null) return;

        // 이전 페이드 중이면 중단
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(DisplayMessage(message));
    }

    private IEnumerator DisplayMessage(string message)
    {
        noticeText.text = message;
        noticeText.enabled = true;
        noticeText.color = originalColor;

        // 일정 시간 대기
        yield return new WaitForSeconds(displayTime);

        // 페이드아웃
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