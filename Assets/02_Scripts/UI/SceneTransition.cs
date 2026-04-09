using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("拖入你的 BlackScreenPanel (Image组件)")]
    public Image blackScreenPanel;

    [Header("转场设置")]
    [Tooltip("淡入持续时间（秒）")]
    public float fadeDuration = 1.5f;

    private void Start()
    {
        // 场景加载完毕后自动开始淡入效果（黑幕逐渐消失）
        if (blackScreenPanel != null)
        {
            StartCoroutine(FadeInRoutine());
        }
        else
        {
            Debug.LogWarning("未指定 BlackScreenPanel！");
        }
    }

    private IEnumerator FadeInRoutine()
    {
        // 确保黑幕一开始是激活状态且完全不透明 (Alpha = 1)
        blackScreenPanel.gameObject.SetActive(true);
        Color fadeColor = blackScreenPanel.color;
        fadeColor.a = 1f;
        blackScreenPanel.color = fadeColor;

        float elapsedTime = 0f;

        // 在指定时间内将 Alpha 从 1 插值到 0
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            blackScreenPanel.color = fadeColor;

            // 等待下一帧
            yield return null;
        }

        // 确保最终 Alpha 为 0
        fadeColor.a = 0f;
        blackScreenPanel.color = fadeColor;

        // 禁用黑幕 UI，防止它在透明状态下阻挡鼠标/触屏点击事件 (Raycast)
        blackScreenPanel.gameObject.SetActive(false);
    }
}
