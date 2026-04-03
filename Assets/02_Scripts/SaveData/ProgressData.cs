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
    Task1_5
}

// 存档数据结构（可序列化，用于存储）
[Serializable]
public class SaveData  //游戏所有需要存档的数据
{
    public ProgressLevel currentLevel;       // 当前所在等级
    public DialogueNode lastCompletedNode;   // 最后完成的对话节点
    public bool isLevel1Completed;           // 第一等级是否完成
}