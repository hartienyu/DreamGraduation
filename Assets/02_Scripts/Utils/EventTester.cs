using UnityEngine;
using TMPro;

// 【测试专用脚本】正式实装霸凌者AI后可直接删除挂载或此脚本
public class EventTester : MonoBehaviour
{
    public static EventTester Instance;

    [Header("测试剧本列表")]
    [Tooltip("把你想要测试的剧本文件(如Global_Bully)全拖这里")]
    public System.Collections.Generic.List<TextAsset> testDialogues;

    [Header("状态记录")]
    [Tooltip("记录玩家是否选择了撕碎画纸")]
    public bool isPaperTorn = false;

    [Header("Splash Canvas (警告提示)")]
    public GameObject splashCanvas;
    public TextMeshProUGUI splashText;

    // 是否显示测试作弊面板
    private bool showCheatMenu = false;

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

#if UNITY_EDITOR
        // 自动加载 Assets/04_Art/Dialogues 下的所有对话文件，免去手动拖拽
        if (testDialogues == null) testDialogues = new System.Collections.Generic.List<TextAsset>();
        testDialogues.Clear();

        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/04_Art/Dialogues" });
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (textAsset != null && !testDialogues.Contains(textAsset))
            {
                testDialogues.Add(textAsset);
            }
        }
        Debug.Log($"[EventTester] 已自动扫描并加载了 {testDialogues.Count} 个剧情文件！不用手动拖拽了。");
#endif
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
        if (dialogueName == "Task2_BranchA_TearPaper")
        {
            isPaperTorn = true;
            // ShowSplash("警告：霸凌者即将出现！");  // —— 根据要求取消结束时直接弹警告，改为由 CountdownTimer 倒计时触发
        }
        // 2. 玩家选择了保留画纸的反馈剧情结束了
        else if (dialogueName == "Task2_BranchB_KeepPaper")
        {
            isPaperTorn = false;
            // ShowSplash("警告：霸凌者即将出现！");  // —— 同上取消
        }
        // 3. 玩家被抓到了 (BadEnding结束)
        else if (dialogueName == "Global_BadEnding")
        {
            Debug.Log("【GAME OVER】对话结束，直接触发游戏失败逻辑！");
        }
    }

    // 显示屏幕提示
    public void ShowSplash(string message)
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
        // F1键开关作弊面板
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showCheatMenu = !showCheatMenu;

            // 切换作弊面板时解放或锁定鼠标
            Cursor.visible = showCheatMenu;
            Cursor.lockState = showCheatMenu ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // 如果没有开启作弊面板，直接返回，不监听其它按键
        if (!showCheatMenu) return;

        // 【剧本测试】按键快速测试
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayBranch("HuaHuo1");
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayBranch("HuaHuo3");
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayBranch("Task2_Start");

        // 结局/霸凌者
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (isPaperTorn) PlayBranch("Task2_BranchA_TearPaper");
            else PlayBranch("Task2_BranchB_KeepPaper");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlayBranch("Global_BullyApproach");
        if (Input.GetKeyDown(KeyCode.Alpha6)) PlayBranch("Global_BullyLeave");
        if (Input.GetKeyDown(KeyCode.Alpha7)) PlayBranch("Global_BullySpot");
        if (Input.GetKeyDown(KeyCode.Alpha8)) PlayBranch("Global_BullySpotLeave");
        if (Input.GetKeyDown(KeyCode.Alpha9)) PlayBranch("Global_BadEnding");
    }

    // ========== 生成左上角的测试 GUI 按钮 ==========
    private void OnGUI()
    {
        // 仅当开启了作弊模式时才显示UI
        if (!showCheatMenu) return;

        // 增大面版去容纳所有剧情
        GUILayout.BeginArea(new Rect(10, 10, 220, 600), GUI.skin.box);
        GUILayout.Label("<b><color=red>=== CheatCode 面板 (F1开关) ===</color></b>");

        GUILayout.Space(10);
        GUILayout.Label("【主线 & Task剧情测试】");

        if (GUILayout.Button("1. 测试: 花火1 (按 1)")) PlayBranch("HuaHuo1");
        if (GUILayout.Button("2. 测试: 花火3 (按 2)")) PlayBranch("HuaHuo3");
        if (GUILayout.Button("3. 测试: Task 2 开始交互 (按 3)")) PlayBranch("Task2_Start");

        GUILayout.Space(10);
        GUILayout.Label($"<b>当前画纸追踪状态 (被撕): {isPaperTorn}</b>");
        
        if (GUILayout.Button("4. 触发 Task 2 结算路线 (按 4)"))
        {
            if (isPaperTorn) PlayBranch("Task2_BranchA_TearPaper");
            else PlayBranch("Task2_BranchB_KeepPaper");
        }

        GUILayout.Space(10);
        GUILayout.Label("【全局霸凌事件模拟】");
        if (GUILayout.Button("5. 霸凌者靠近 (按 5)")) PlayBranch("Global_BullyApproach");
        if (GUILayout.Button("6. 霸凌者离开 (按 6)")) PlayBranch("Global_BullyLeave");
        if (GUILayout.Button("7. 发现玩家 (按 7)")) PlayBranch("Global_BullySpot");
        if (GUILayout.Button("8. 逃脱 (SpotLeave) (按 8)")) PlayBranch("Global_BullySpotLeave");
        if (GUILayout.Button("9. 处决被抓 (BadEnding) (按 9)")) PlayBranch("Global_BadEnding");

        GUILayout.Space(10);
        if (GUILayout.Button("0. 【测试】强制触发五分钟警告"))
        {
            ShowSplash("警告：他发现你了……仅剩五分钟！");
        }

        GUILayout.Space(10);
        GUILayout.Label("【时间控制】");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-1分钟"))
        {
            if (CountdownTimer.Instance != null)
                CountdownTimer.Instance.AdjustTime(-60f);
        }
        if (GUILayout.Button("+1分钟"))
        {
            if (CountdownTimer.Instance != null)
                CountdownTimer.Instance.AdjustTime(60f);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    // 工具方法：播放特定的分支剧本
    private void PlayBranch(string dialogueName)
    {
        if (DialogueManager.Instance == null) return;
        
        // 现在从自身的列表里找
        TextAsset json = testDialogues?.Find(x => x.name == dialogueName);
        if (json != null)
        {
            DialogueManager.Instance.StartDialogue(json, null, testDialogues);
        }
        else
        {
            Debug.LogError($"未在 BullyEventTester (Cheat面板) 的 Test Dialogues 中找到剧情文件: {dialogueName} ！请在这挂载物体的面板上把它拖入。");
        }
    }
}