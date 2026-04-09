using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [Header("UI 绑定")]
    public GameObject gameOverPanel; // 失败弹窗背景与面板
    public TextMeshProUGUI reasonText; // 显示失败原因的文本 (倒计时结束/生命值归零)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 初始隐藏失败面板
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // 触发游戏失败
    public void TriggerGameOver(string reason)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (reasonText != null) 
                reasonText.text = reason;

            // 暂停时间，解锁鼠标
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // "回到主菜单" 按钮调用的方法
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // 恢复时间
        SceneManager.LoadScene(0); // 假定场景索引 0 是主菜单
    }
}
