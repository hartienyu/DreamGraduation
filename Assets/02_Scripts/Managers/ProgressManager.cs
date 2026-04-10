using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;
    private GameSaveData currentSaveData;
    private GlobalSettingsData globalSettings; // 全局设置数据
    private const string SaveKey = "GameProgress";  // PlayerPrefs存储键
    private const string SettingsKey = "GlobalSettings"; // PlayerPrefs设置键

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }

        // 初始化存档数据和全局设置
        LoadProgress();
        LoadSettings();
    }

    // 更新进度（触发对话后调用）
    public void UpdateProgress(DialogueNode completedNode, Vector3 position)
    {
        currentSaveData.lastCompletedNode = completedNode;
        if (completedNode != DialogueNode.None && !currentSaveData.triggeredNodes.Contains(completedNode))
        {
            currentSaveData.triggeredNodes.Add(completedNode);
        }
        currentSaveData.currentLevel = GetNodeLevel(completedNode);
        currentSaveData.isLevel1Completed = IsLevel1Completed(completedNode);
        currentSaveData.lastPlayerPosition = position;

        // ========== 添加调试日志 ==========
        Debug.Log($"保存进度 - 节点: {completedNode}, 等级: {currentSaveData.currentLevel}, Level1完成: {currentSaveData.isLevel1Completed}，当前位置：{currentSaveData.lastPlayerPosition}");

        // 自动存档
        SaveProgress();
    }

    // 存档：将数据序列化为JSON存储到PlayerPrefs（也可改用File/PlayerPrefsX/云存档）
    public void SaveProgress()
    {
        string json = JsonUtility.ToJson(currentSaveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        // ========== 添加调试日志 ==========
        Debug.Log($"存档已保存: {json}");
    }

    // 读档：加载存储的进度数据
    public void LoadProgress()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            currentSaveData = JsonUtility.FromJson<GameSaveData>(json);

            // ========== 添加调试日志 ==========
            Debug.Log($"加载存档成功: {json}");
        }
        else
        {
            // 首次进入游戏，初始化进度
            currentSaveData = new GameSaveData()
            {
                currentLevel = ProgressLevel.Level1,
                lastCompletedNode = DialogueNode.Start,
                isLevel1Completed = false,
                lastPlayerPosition = Vector3.zero // 我们在 GameInitiator 里判断如果是 zero 就不强制移动玩家
            };

            // ========== 添加调试日志 ==========
            Debug.Log("首次进入游戏，初始化存档");
        }
    }

    // ========== 主菜单系统专用接口 ==========

    // 检查是否有已经存在的存档
    public bool HasSaveGame()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    // 开始新游戏（删除旧存档，重置状态）
    public void StartNewGame()
    {
        // 1. 删除本地旧存档记录
        PlayerPrefs.DeleteKey(SaveKey);

        // 2. 初始化全新、干净的游戏数据
        currentSaveData = new GameSaveData()
        {
            currentLevel = ProgressLevel.Level1,
            lastCompletedNode = DialogueNode.Start,
            isLevel1Completed = false,
            lastPlayerPosition = Vector3.zero, // 代表走初始生成点
            sceneName = "GameScene", // 你的第一关场景名
            triggeredNodes = new List<DialogueNode>(),
            finishedSceneStates = new List<string>()
        };

        // 3. 立刻保存这个全新状态，覆盖并真正创立存档
        SaveProgress();
    }

    // 继续游戏（读取最后一次的进度信息）
    public void ContinueGame()
    {
        // 1. 获取现有进度
        LoadProgress();
    }

    // 获取当前进度数据（供其他系统调用，如UI显示、场景初始化）
    public GameSaveData GetCurrentSaveData()
    {
        return currentSaveData;
    }

    // ========== 全局设置管理（音量等） ==========

    // 获取当前设置
    public GlobalSettingsData GetSettings()
    {
        return globalSettings;
    }

    // 保存全局设置
    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(globalSettings);
        PlayerPrefs.SetString(SettingsKey, json);
        PlayerPrefs.Save();
        ApplyVolumeSettings();
    }

    // 读取全局设置
    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(SettingsKey))
        {
            string json = PlayerPrefs.GetString(SettingsKey);
            globalSettings = JsonUtility.FromJson<GlobalSettingsData>(json);
        }
        else
        {
            // 默认设置
            globalSettings = new GlobalSettingsData()
            {
                masterVolume = 1.0f,
                bgmVolume = 1.0f,
                sfxVolume = 1.0f
            };
        }
        ApplyVolumeSettings();
    }

    // 应用音量设置到游戏 (这里简化为通过AudioListener控制主音量，具体可扩展到AudioMixer)
    public void ApplyVolumeSettings()
    {
        AudioListener.volume = globalSettings.masterVolume;
        // BGM 和 SFX 可以需要根据你的其他 AudioManager/AudioSource 进行专门设置
    }

    // 重置进度（测试/重玩时用）
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        LoadProgress();

        // ========== 添加调试日志 ==========
        Debug.Log("进度已重置");
    }


    // 判断当前对话节点属于哪个等级
    public ProgressLevel GetNodeLevel(DialogueNode node)
    {
        switch (node)
        {
            // Level1 节点
            case DialogueNode.Start:
            case DialogueNode.Stadium:
            case DialogueNode.Corridor:
            case DialogueNode.Staircase:
            case DialogueNode.ClassroomDoor:
            case DialogueNode.Huahuo1:
            case DialogueNode.Huahuo3:
            case DialogueNode.Task1_1:
            case DialogueNode.Task1_2:
            case DialogueNode.Task1_3:
            case DialogueNode.Task1_4:
            case DialogueNode.Task1_5:
                return ProgressLevel.Level1;

            // Level2 节点（补全）
            case DialogueNode.Task2_Start:
            case DialogueNode.Task2_BranchA_TearPaper:
            case DialogueNode.Task2_BranchB_KeepPaper:
            case DialogueNode.Global_BullyApproach:
            case DialogueNode.Global_BullyLeave:
            case DialogueNode.Global_BullySpot:
            case DialogueNode.Global_BullySpotLeave:
            case DialogueNode.Global_BadEnding:
                return ProgressLevel.Level2;

            default:
                Debug.LogWarning($"未识别的节点: {node}，默认返回 Level1");
                return ProgressLevel.Level1;
        }
    }

    // ========== 判断第一等级是否完成（以Huahuo3为终点） ==========
    private bool IsLevel1Completed(DialogueNode lastNode)
    {
        // 根据你的需求：只要Huahuo3对话结束就完成任务
        if (lastNode == DialogueNode.Huahuo3)
        {
            return true;
        }
        // 其他所有情况都返回 false
        else
        {
            return false;
        }
    }
}


