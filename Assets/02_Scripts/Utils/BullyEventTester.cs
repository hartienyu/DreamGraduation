using UnityEngine;
using TMPro;

// 【测试专用脚本】正式实装霸凌者AI后可直接删除挂载或此脚本
public class BullyEventTester : MonoBehaviour
{
    public static BullyEventTester Instance;

    [Header("状态记录")]
    [Tooltip("记录玩家是否选择了撕碎画纸")]
    public bool isPaperTorn = false;

    [Header("Splash Canvas (警告提示)")]
    public GameObject splashCanvas;
    public TextMeshProUGUI splashText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 开始时隐藏警告UI
        if (splashCanvas != null) splashCanvas.SetActive(false);

        // 监听对话结束事件，以便判断玩家选项和结局
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished += OnDialogueFinished;
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished -= OnDialogueFinished;
        }
    }

    // ========== 监听核心关键对话的结束 ==========
    private void OnDialogueFinished(string dialogueName)
    {
        // 1. 玩家选择了把画撕碎的反馈剧情结束了
        if (dialogueName == "Task2_TearPaper")
        {
            isPaperTorn = true;
            ShowSplash("警告：霸凌者即将出现！");
        }
        // 2. 玩家选择了保留画纸的反馈剧情结束了
        else if (dialogueName == "Task2_KeepPaper")
        {
            isPaperTorn = false;
            ShowSplash("警告：霸凌者即将出现！");
        }
        // 3. 玩家被抓到了 (BadEnding结束)
        else if (dialogueName == "Task2_BadEnding")
        {
            Debug.Log("【GAME OVER】对话结束，直接触发游戏失败逻辑！");
            // 这里以后可以调用类似 GameManager.Instance.GameOver(); 
            // 或锁定玩家控制之类的功能
        }
        // 4. 清关对话结束
        else if (dialogueName == "Task2_Complete_A" || dialogueName == "Task2_Complete_B")
        {
            Debug.Log("【任务完成】羁绊等级提升，解锁下一个任务节点！");
            // 这里可以调用 ProgressManager.Instance.UpdateProgress(...)
        }
    }

    // 显示屏幕提示
    private void ShowSplash(string message)
    {
        if (splashCanvas != null)
        {
            splashCanvas.SetActive(true);
            if (splashText != null) splashText.text = message;
            
            // 3秒后自动隐藏提示
            Invoke("HideSplash", 3f);
        }
        else
        {
            Debug.LogWarning($"[缺少SplashCanvas] 应该在屏幕显示: {message}");
        }
    }

    private void HideSplash()
    {
        if (splashCanvas != null) splashCanvas.SetActive(false);
    }

    private void Update()
    {
        // 允许通过按键快速测试，因为游戏中可能隐藏了鼠标指针
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayBranch("Task2_BullyApproach");
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayBranch("Task2_BullyLeave");
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayBranch("Task2_BullySpot");
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayBranch("Task2_BullySpotLeave");
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlayBranch("Task2_BadEnding");
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            if (isPaperTorn) PlayBranch("Task2_Complete_A");
            else PlayBranch("Task2_Complete_B");
        }
    }

    // ========== 生成左上角的测试 GUI 按钮 ==========
    private void OnGUI()
    {
        // 设置一个背景框在屏幕左上角
        GUILayout.BeginArea(new Rect(10, 10, 220, 420), GUI.skin.box);
        GUILayout.Label("<b><color=red>=== 霸凌者事件测试面板 ===</color></b>");
        GUILayout.Label("<color=yellow>提示: 鼠标被隐藏时可直接按数字键</color>");

        GUILayout.Space(10);
        GUILayout.Label("【动态事件 - 视野检测模拟】");

        if (GUILayout.Button("1. 霸凌者靠近 (按 1)"))
        {
            PlayBranch("Task2_BullyApproach");
        }
        if (GUILayout.Button("2. 霸凌者离开 (按 2)"))
        {
            PlayBranch("Task2_BullyLeave");
        }
        if (GUILayout.Button("3. 发现玩家 (按 3)"))
        {
            PlayBranch("Task2_BullySpot");
        }
        if (GUILayout.Button("4. 玩家逃脱 (按 4)"))
        {
            PlayBranch("Task2_BullySpotLeave");
        }

        GUILayout.Space(10);
        GUILayout.Label("【处决与结局模拟】");

        if (GUILayout.Button("5. 被抓到 (按 5)"))
        {
            // 在真正实装时，这里是由于怪物物理碰撞到Player才调用 PlayBranch("Task2_BadEnding")
            PlayBranch("Task2_BadEnding");
        }

        GUILayout.Space(10);
        GUILayout.Label($"<b>当前画纸已被撕碎: {isPaperTorn}</b>");
        
        if (GUILayout.Button("6. 模拟存活/任务完成 (按 6)"))
        {
            // 结算时根据 isPaperTorn 状态动态给结局
            if (isPaperTorn)
            {
                PlayBranch("Task2_Complete_A");
            }
            else
            {
                PlayBranch("Task2_Complete_B");
            }
        }

        GUILayout.EndArea();
    }

    // 工具方法：播放特定的分支剧本
    private void PlayBranch(string dialogueName)
    {
        if (DialogueManager.Instance == null) return;
        
        // 我们利用之前添加的 branchDialogues 库去拿剧本
        TextAsset json = DialogueManager.Instance.branchDialogues.Find(x => x.name == dialogueName);
        if (json != null)
        {
            DialogueManager.Instance.StartDialogue(json);
        }
        else
        {
            Debug.LogError($"未在 DialogueManager 的 Branch Dialogues 中找到剧情文件: {dialogueName} ！请把它拖入列表。");
        }
    }
}