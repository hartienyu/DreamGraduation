using UnityEngine;
using System;

public class MemoryManager : MonoBehaviour
{
    public static MemoryManager Instance;

    [Header("回忆内容文件 (JSON)")]
    [Tooltip("记忆碎片1的 JSON 文件")]
    public TextAsset memoryJson1;
    
    [Tooltip("记忆碎片2的 JSON 文件")]
    public TextAsset memoryJson2;

    [Header("记忆解锁状态")]
    public bool isMemory1Unlocked = false;
    public bool isMemory2Unlocked = false;

    // 事件：通知UI系统弹出对应的记忆碎片片段
    public event Action<string> OnMemoryUnlocked;
    // 事件：通知结局触发
    public event Action OnTrueEndingTriggered;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 完成羁绊任务1时触发
    /// </summary>
    public void UnlockMemory1_Task1Completed()
    {
        if (isMemory1Unlocked) return;
        isMemory1Unlocked = true;
        Debug.Log("【记忆系统】羁绊任务1完成！解锁记忆碎片1。");

        string textToShow = memoryJson1 != null ? JsonUtility.FromJson<MemoryData>(memoryJson1.text).memoryText : "记忆1加载失败！";
        OnMemoryUnlocked?.Invoke(textToShow);
    }

    /// <summary>
    /// 完成羁绊任务2时触发
    /// </summary>
    public void UnlockMemory2_Task2Completed()
    {
        if (isMemory2Unlocked) return;
        isMemory2Unlocked = true;
        Debug.Log("【记忆系统】羁绊任务2完成！解锁完整记忆碎片2。");

        string textToShow = memoryJson2 != null ? JsonUtility.FromJson<MemoryData>(memoryJson2.text).memoryText : "记忆2加载失败！";
        OnMemoryUnlocked?.Invoke(textToShow);
        
        // 判定真结局：不再里面立刻触发，而是等玩家阅读完毕关闭UI后由 UI 触发挥发真结局
    }

    public void TriggerTrueEnding()
    {
        Debug.Log("【结局系统】片段2阅读完毕，记忆完全解锁，直面恐惧。触发救赎真结局！");
        OnTrueEndingTriggered?.Invoke();
        // 在这里可以接入过场动画、白屏、回到主菜单等结算逻辑
    }
}

// 用于解析 JSON 的数据结构
[Serializable]
public class MemoryData
{
    public string memoryText;
}