using System.Collections;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;
    private SaveData currentSaveData;
    private const string SaveKey = "GameProgress";  // PlayerPrefs存储键

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
            Destroy(gameObject);
        }

        // 初始化存档数据
        LoadProgress();
    }

    // 更新进度（触发对话后调用）
    public void UpdateProgress(DialogueNode completedNode)
    {
        currentSaveData.lastCompletedNode = completedNode;
        currentSaveData.currentLevel = GetNodeLevel(completedNode);
        currentSaveData.isLevel1Completed = IsLevel1Completed(completedNode);

        // ========== 添加调试日志 ==========
        Debug.Log($"保存进度 - 节点: {completedNode}, 等级: {currentSaveData.currentLevel}, Level1完成: {currentSaveData.isLevel1Completed}");

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
            currentSaveData = JsonUtility.FromJson<SaveData>(json);

            // ========== 添加调试日志 ==========
            Debug.Log($"加载存档成功: {json}");
        }
        else
        {
            // 首次进入游戏，初始化进度
            currentSaveData = new SaveData()
            {
                currentLevel = ProgressLevel.Level1,
                lastCompletedNode = DialogueNode.Start,
                isLevel1Completed = false
            };

            // ========== 添加调试日志 ==========
            Debug.Log("首次进入游戏，初始化存档");
        }
    }

    // 获取当前进度数据（供其他系统调用，如UI显示、场景跳转）
    public SaveData GetCurrentSaveData()
    {
        return currentSaveData;
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
    private ProgressLevel GetNodeLevel(DialogueNode node)
    {
        switch (node)
        {
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

            //    return ProgressLevel.Level2;

            default:
                return ProgressLevel.Level1;
        }
    }

    // ========== 修复：判断第一等级是否完成（以Huahuo3为终点） ==========
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