using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 进度等级
public enum ProgressLevel
{
    Level1,  // 第一等级：DialogueCube_Init → DialogueCube_Huahuo1
    Level2   // 第二等级：剩余对话节点
}

// 对话触发节点
public enum DialogueNode
{
    None, // 新增：用于永远可以触发、固定不变的NPC对话，不被存档拦截
    Start,
    Stadium,
    Staircase,
    Corridor,
    ClassroomDoor,
    Huahuo1,
    Huahuo3,
    Task1_1,
    Task1_2,
    Task1_3,
    Task1_4,
    Task1_5,
    Task2_Start,
    Task2_BranchA_TearPaper,
    Task2_BranchB_KeepPaper,
    Global_BullyApproach,
    Global_BullyLeave,
    Global_BullySpot,
    Global_BullySpotLeave,
    Global_BadEnding,
    TrueEnding1,
    TrueEnding2,
    TrueEnding3,
    TrueEnding4,
    TrueEnding5,
    TrueEnding6
}

// --- 1. 游戏存档核心数据（每个存档槽对应一个） ---
[Serializable]
public class GameSaveData
{
    // 基础信息
    public string sceneName = "GameScene";       // 当前所在的场景名称
    public Vector3 lastPlayerPosition;           // 玩家最后位置

    // 角色状态
    public float currentHealth = 100f;           // 当前血量
    public int courageValue = 0;                 // 勇气值

    // 资源与收集品
    public int memoryFragments = 2000;             // 记忆碎片数量
    public int gems = 0;                         // 宝石数量

    // 商店购买记录
    public List<int> remainingShopQuantities = new List<int>(); // 记录商店各商品的剩余库存

    // 倒计时
    public float remainingTimer = -1f; // 倒计时剩余时间，-1表示未开启
    public bool hasTriggered5MinSplash = false; // 是否已经触发过5分钟警告

    // 世界与剧情状态
    public ProgressLevel currentLevel;           // 当前所在等级
    public DialogueNode lastCompletedNode;       // 最后完成的对话节点
    public bool isLevel1Completed;               // 第一等级是否完成
    
    // 只凭 DialogueNode 记录即可，精简并删除了原来的字符串ID List
    public List<DialogueNode> triggeredNodes = new List<DialogueNode>(); // 【替换了】原来那个繁琐的 triggeredDialogues
    
    // 场景散落物/状态仍保留用字符串（如果宝石啥的不用节点配的话）
    public List<string> finishedSceneStates = new List<string>(); 
}

// --- 2. 全局设置数据（独立于存档槽位，全局共用，如音量等） ---
[Serializable]
public class GlobalSettingsData
{
    public float masterVolume = 1.0f;
    public float bgmVolume = 1.0f;
    public float sfxVolume = 1.0f;
}