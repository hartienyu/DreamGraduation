using UnityEngine;

public class GameEndingManager : MonoBehaviour
{
    [Header("引用的UI面板")]
    public GameObject exitPanel;

    private void Start()
    {
        // 游戏开始时确保面板是隐藏的
        if (exitPanel != null) 
            exitPanel.SetActive(false);

        // 订阅对话结束的广播事件
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished += OnDialogueFinished;
        }
    }

    private void OnDestroy()
    {
        // 销毁时注销事件，防止报错
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished -= OnDialogueFinished;
        }
    }

    // 当任何对话结束时，都会自动运行这个方法
    private void OnDialogueFinished(string dialogueName)
    {
        // 检查结束的剧本名字是不是 "TrueEnding6"（注意：要和你的 JSON 文件名一模一样，不要加 .json）
        if (dialogueName == "TrueEnding6")
        {
            ShowExitPanel();
        }
    }

    private void ShowExitPanel()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(true);

            // 解锁并显示鼠标指针，否则玩家没法点击 Quit 按钮
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // 如果你想，这里还可以加上暂停游戏时间的代码：Time.timeScale = 0f;
        }
    }

    // 提供给 Quit 按钮绑定的退出游戏方法
    public void QuitGame()
    {
        Debug.Log("退出游戏！");
        
        // 打包后真正退出游戏的代码
        Application.Quit();

#if UNITY_EDITOR
        // 这一段是为了在 Unity 编辑器里测试时也能模拟退出（会自动停止 Play 模式）
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}