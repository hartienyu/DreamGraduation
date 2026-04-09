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
            MemoryManager.Instance.OnNormalEndingTriggered += ShowNormalEnding;
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
            MemoryManager.Instance.OnNormalEndingTriggered -= ShowNormalEnding;
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

    private bool isEndingTriggered = false; // 用于判定是否已经进入结算状态
    
    public enum EndingState { None, FinalTrueEnding, FinalNormalEnding }
    private EndingState currentEndingState = EndingState.None;

    /// <summary>
    /// 点击关闭按钮时的逻辑
    /// </summary>
    private void CloseMemory()
    {
        if (memoryPanel != null)
        {
            if (currentEndingState == EndingState.FinalTrueEnding)
            {
                Time.timeScale = 1f;
                // 点击了真结局弹窗上的关闭按钮 -> 跳转到Reality场景
                UnityEngine.SceneManagement.SceneManager.LoadScene("Reality");
                return;
            }
            else if (currentEndingState == EndingState.FinalNormalEnding)
            {
                Time.timeScale = 1f;
                // 点击了普通结局的弹窗 -> 这里可以按需跳转，或者仅仅关闭UI继续新流程
                memoryPanel.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                return;
            }

            memoryPanel.SetActive(false);

            // 恢复游戏时间
            Time.timeScale = 1f;

            // 重新锁定鼠标（如果你是第一人称FPS）
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // ===== 触发后续剧情 =====
            // 如果玩家刚刚读完了第二段记忆，且没触发过结局，就在关闭后"重新弹出"结局面板
            if (MemoryManager.Instance != null && MemoryManager.Instance.isMemory2Unlocked && !isEndingTriggered)
            {
                Debug.Log("【UI】读者关闭了最后的记忆碎片，开始进行结局结算！");
                isEndingTriggered = true;
                
                // 判断勇气值来决定结局走向
                if (PlayerHealth.Instance != null && PlayerHealth.Instance.currentCourage >= 100f)
                {
                    Debug.Log("【结局判定】当前勇气值 >= 100，触发真结局！");
                    MemoryManager.Instance.TriggerTrueEnding();
                }
                else
                {
                    Debug.Log("【结局判定】当前勇气值 < 100，触发普通结局！");
                    MemoryManager.Instance.TriggerNormalEnding();
                }
                return;
            }

            Debug.Log("【UI】读者关闭记忆碎片，准备继续游戏流程。");
        }
    }

    /// <summary>
    /// 显示真结局弹窗
    /// </summary>
    private void ShowTrueEnding()
    {
        currentEndingState = EndingState.FinalTrueEnding;
        if (memoryPanel != null && memoryTextDisplay != null)
        {
            memoryTextDisplay.text = "【真结局】\n\n那水盆里的影子不再陌生。这一次，你终于直面了恐惧。\n你拉着她的手跑出了那间幽闭的校舍。\n\n\n> 点击确认后将回到现实...";
            memoryPanel.SetActive(true);
            
            // 保证按钮处于显示状态，可以点击进入下一场景
            if (closeButton != null) closeButton.gameObject.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// 显示普通结局弹窗
    /// </summary>
    private void ShowNormalEnding()
    {
        currentEndingState = EndingState.FinalNormalEnding;
        if (memoryPanel != null && memoryTextDisplay != null)
        {
            memoryTextDisplay.text = "【普通结局】\n\n你没能直面这一切的勇气，但是这所校园里，似乎还有其他的求救声......\n\n\n> 准备进入下一个“被救赎者”的新剧情";
            memoryPanel.SetActive(true);
            
            // 保证按钮可见，点击关闭UI
            if (closeButton != null) closeButton.gameObject.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
    }
}