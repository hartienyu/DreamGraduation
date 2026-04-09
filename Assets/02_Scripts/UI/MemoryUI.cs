using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MemoryUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("记忆碎片的整个画板/背景面板")]
    public GameObject memoryPanel;
    
    [Tooltip("显示记忆内容的文字组件")]
    public TextMeshProUGUI memoryTextDisplay;
    
    [Tooltip("关闭按钮")]
    public Button closeButton;

    private void Start()
    {
        // 开始时先隐藏面板
        if (memoryPanel != null) memoryPanel.SetActive(false);

        // 监听MemoryManager的解锁事件
        if (MemoryManager.Instance != null)
        {
            MemoryManager.Instance.OnMemoryUnlocked += ShowMemory;
            MemoryManager.Instance.OnTrueEndingTriggered += ShowTrueEnding;
        }
        else
        {
            Debug.LogWarning("[MemoryUI] 场景中没有找到 MemoryManager 实例！");
        }

        // 绑定关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMemory);
        }
    }

    private void OnDestroy()
    {
        if (MemoryManager.Instance != null)
        {
            MemoryManager.Instance.OnMemoryUnlocked -= ShowMemory;
            MemoryManager.Instance.OnTrueEndingTriggered -= ShowTrueEnding;
        }
    }

    /// <summary>
    /// 当接收到记忆碎片解锁信号时，弹出UI并显示文字
    /// </summary>
    private void ShowMemory(string text)
    {
        if (memoryPanel != null && memoryTextDisplay != null)
        {
            memoryTextDisplay.text = text;
            memoryPanel.SetActive(true);

            // 弹出回忆时，解锁鼠标方便点击关闭按钮
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 暂停游戏时间（可选，如果你的回忆是在安全时间读的可以不加）
            Time.timeScale = 0f;
            Debug.Log("【UI】成功弹出记忆碎片画面！");
        }
    }

    private bool isEndingTriggered = false; // 用于判定是否已经进入真结局

    /// <summary>
    /// 点击关闭按钮时的逻辑
    /// </summary>
    private void CloseMemory()
    {
        if (memoryPanel != null)
        {
            memoryPanel.SetActive(false);

            // 恢复游戏时间
            Time.timeScale = 1f;

            // 重新锁定鼠标（如果你是第一人称FPS）
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // ===== 触发后续剧情 =====
            // 如果玩家刚刚刚读完了第二段记忆，且没触发过真结局，就在关闭后触发真结局
            if (MemoryManager.Instance != null && MemoryManager.Instance.isMemory2Unlocked && !isEndingTriggered)
            {
                Debug.Log("【UI】读者关闭了最后的记忆碎片，直接切入真结局结算！");
                isEndingTriggered = true;
                MemoryManager.Instance.TriggerTrueEnding();
                return;
            }

            Debug.Log("【UI】读者关闭记忆碎片，准备继续游戏流程。");
        }
    }

    /// <summary>
    /// 显示真结局（如果你想用纯UI结算就在这里做）
    /// </summary>
    private void ShowTrueEnding()
    {
        if (memoryPanel != null && memoryTextDisplay != null)
        {
            memoryTextDisplay.text = "【真结局】\n\n你终于直面了恐惧，救下了小白......\n\n(游戏结束)";
            memoryPanel.SetActive(true);
            
            // 隐藏关闭按钮，或者把关闭按钮的功能改成退出游戏/回主菜单
            if (closeButton != null) closeButton.gameObject.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
    }
}