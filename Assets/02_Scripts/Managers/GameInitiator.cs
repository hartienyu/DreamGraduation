using System.Collections;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;

public class GameInitiator : MonoBehaviour
{
    private ProgressManager progressManager;

    private void Start()
    {
        progressManager = ProgressManager.Instance;
        LoadGameProgress();
    }

    private void LoadGameProgress()
    {
        SaveData saveData = progressManager.GetCurrentSaveData();

        // ========== 添加调试日志 ==========
        Debug.Log($"加载游戏进度 - 上次节点: {saveData.lastCompletedNode}, 等级: {saveData.currentLevel}, Level1完成: {saveData.isLevel1Completed}");

        // 根据进度调整游戏状态
        switch (saveData.currentLevel)
        {
            case ProgressLevel.Level1:
                // 跳转到Level1的最近节点
                ActivateLastNode(saveData.lastCompletedNode);
                break;
            case ProgressLevel.Level2:
                // 隐藏Level1的对话Cube，激活Level2的最近节点
                DeactivateLevel1Nodes();
                ActivateLastNode(saveData.lastCompletedNode);
                break;
        }
    }

    // 激活最后完成节点的下一个节点（或定位到该节点）
    private void ActivateLastNode(DialogueNode lastNode)
    {
        // 示例：根据节点名找到对应的DialogueCube并设置状态
        GameObject targetCube = GameObject.Find($"DialogueCube_{lastNode}");
        if (targetCube != null)
        {
            // 可设置玩家位置到节点附近，或激活节点的交互状态
            DialogueTrigger trigger = targetCube.GetComponent<DialogueTrigger>();
            if (trigger != null)
            {
                trigger.enabled = true;

                // ========== 添加调试日志 ==========
                Debug.Log($"激活对话节点: DialogueCube_{lastNode}");
            }
        }
        else
        {
            // ========== 添加调试日志 ==========
            Debug.LogWarning($"未找到对话节点: DialogueCube_{lastNode}");
        }
    }

    // 禁用第一等级的所有对话节点（Level2时调用）
    private void DeactivateLevel1Nodes()
    {
        GameObject[] level1Cubes = GameObject.FindGameObjectsWithTag("Level1Dialogue");
        foreach (var cube in level1Cubes)
        {
            DialogueTrigger trigger = cube.GetComponent<DialogueTrigger>();
            if (trigger != null)
            {
                trigger.enabled = false;
            }
        }

        // ========== 添加调试日志 ==========
        Debug.Log("已禁用所有Level1对话节点");
    }
}