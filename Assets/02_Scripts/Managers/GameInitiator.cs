using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitiator : MonoBehaviour
{
    private ProgressManager progressManager;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");  // 获取玩家位置，以便更新其位置

        progressManager = ProgressManager.Instance;
        LoadGameProgress(player);
    }

    private void LoadGameProgress(GameObject player)
    {
        SaveData saveData = progressManager.GetCurrentSaveData();

        Debug.Log($"加载游戏进度 - 上次节点: {saveData.lastCompletedNode}, 等级: {saveData.currentLevel}, Level1完成: {saveData.isLevel1Completed}，玩家上次位置：{saveData.lastPlayerPosition}");

        // 如果不是新游戏（位置不是 Vector3.zero），才更新玩家位置
        if (saveData.lastPlayerPosition != Vector3.zero)
        {
            player.transform.position = saveData.lastPlayerPosition;
        }
        else
        {
            Debug.Log("新游戏（未建立新坐标），保留玩家在场景中布置的初始位置。");
        }

        // 禁用所有比当前等级低的关卡节点
        DeactivateAllPreviousLevels(saveData.currentLevel);

        // 决定激活哪个节点
        DialogueNode nodeToActivate;

        // 如果还没有完成任何节点（新游戏）
        if (saveData.lastCompletedNode == DialogueNode.Start &&
            saveData.currentLevel == ProgressLevel.Level1 &&
            !saveData.isLevel1Completed)
        {
            // 新游戏：激活 Start 节点本身
            nodeToActivate = DialogueNode.Start;
            Debug.Log("新游戏 - 激活 Start 节点");
        }
        else
        {
            // 已有进度：激活下一个节点
            nodeToActivate = GetNextNode(saveData.lastCompletedNode);
            Debug.Log($"继续游戏 - 上次完成 {saveData.lastCompletedNode}，激活下一个节点 {nodeToActivate}");
        }

        ActivateNode(nodeToActivate);
    }


    // 获取下一个应该激活的节点
    private DialogueNode GetNextNode(DialogueNode completedNode)
    {
        // 定义对话流程顺序（根据你的场景实际顺序）
        switch (completedNode)
        {
            case DialogueNode.Start:
                return DialogueNode.Stadium;
            case DialogueNode.Stadium:
                return DialogueNode.Staircase;
            case DialogueNode.Staircase:
                return DialogueNode.Corridor;
            case DialogueNode.Corridor:
                return DialogueNode.ClassroomDoor;
            case DialogueNode.ClassroomDoor:
                return DialogueNode.Huahuo1;
            case DialogueNode.Huahuo1:
                return DialogueNode.Huahuo3;
            case DialogueNode.Huahuo3:
                return DialogueNode.Task1_1;
            case DialogueNode.Task1_1:
                return DialogueNode.Task1_2;
            case DialogueNode.Task1_2:
                return DialogueNode.Task1_3;
            case DialogueNode.Task1_3:
                return DialogueNode.Task1_4;
            case DialogueNode.Task1_4:
                return DialogueNode.Task1_5;
            case DialogueNode.Task1_5:
                return DialogueNode.Task2_Start;
            case DialogueNode.Task2_Start:
                return DialogueNode.Task2_BranchA_TearPaper;
            // 添加更多...
            default:
                // 如果已经是最后一个节点，返回自身（没有下一个）
                Debug.Log($"节点 {completedNode} 是最后一个，没有下一个节点");
                return completedNode;
        }
    }

    // 激活指定节点（支持名称映射）
    private void ActivateNode(DialogueNode node)
    {
        // 获取场景中的实际物体名称（映射）
        string actualCubeName = GetActualCubeName(node);

        Debug.Log($"尝试激活节点: {node} -> 查找物体: {actualCubeName}");

        // 通过名称查找
        GameObject targetCube = GameObject.Find(actualCubeName);

        // 如果找不到，尝试在所有 DialogueTrigger 中查找
        if (targetCube == null)
        {
            Debug.Log("找不到StartCube");
            DialogueTrigger[] allTriggers = FindObjectsOfType<DialogueTrigger>(true);
            foreach (var trigger in allTriggers)
            {
                if (trigger.nodeID == node)
                {
                    targetCube = trigger.gameObject;
                    Debug.Log($"通过枚举找到节点: {targetCube.name}");
                    break;
                }
            }
        }

        if (targetCube != null)
        {
            // 确保物体激活
            if (!targetCube.activeSelf)
            {
                targetCube.SetActive(true);
            }

            // 激活对话触发器
            DialogueTrigger trigger = targetCube.GetComponent<DialogueTrigger>();
            if (trigger != null)
            {
                trigger.enabled = true;
            }

            // 激活碰撞触发器
            CollideTriggerDialogues collide = targetCube.GetComponent<CollideTriggerDialogues>();
            if (collide != null)
            {
                collide.enabled = true;
                collide.ResetTrigger();
            }

            Debug.Log($"✅ 激活对话节点: {targetCube.name} (枚举: {node})");
        }
        else
        {
            Debug.LogError($"❌ 未找到节点: 枚举={node}, 名称={actualCubeName}");

            // 输出场景中所有 DialogueCube 供调试
            GameObject[] allCubes = GameObject.FindGameObjectsWithTag("DialogueCube");
            Debug.Log($"场景中的 DialogueCube 列表: {string.Join(", ", System.Linq.Enumerable.Select(allCubes, c => c.name))}");
        }
    }

    // 映射枚举到场景中的实际物体名称
    private string GetActualCubeName(DialogueNode node)
    {
        switch (node)
        {
            case DialogueNode.Huahuo1:
                return "DialogueCube_Huahuo1";
            case DialogueNode.Huahuo3:
                return "DialogueCube_Huahuo3";
            case DialogueNode.Start:
                return "DialogueCube_Start";
            case DialogueNode.Stadium:
                return "DialogueCube_Stadium";
            case DialogueNode.Corridor:
                return "DialogueCube_Corridor";
            case DialogueNode.Staircase:
                return "DialogueCube_Staircase";
            case DialogueNode.ClassroomDoor:
                return "DialogueCube_ClassroomDoor";
            case DialogueNode.Task1_2:
                return "DialogueCube_Task1_2";
            default:
                return $"DialogueCube_{node}";
        }
    }

    // 禁用所有比当前等级低的关卡
    private void DeactivateAllPreviousLevels(ProgressLevel currentLevel)
    {
        int currentLevelValue = (int)currentLevel;

        for (int i = 1; i < currentLevelValue; i++)
        {
            string tag = $"Level{i}Dialogue";
            DeactivateLevelByTag(tag);
        }
    }

    private void DeactivateLevelByTag(string tag)
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag(tag);
        Debug.Log($"查找 Tag '{tag}'，找到 {cubes.Length} 个物体");

        foreach (var cube in cubes)
        {
            DialogueTrigger trigger = cube.GetComponent<DialogueTrigger>();
            if (trigger != null)
            {
                trigger.enabled = false;
            }

            CollideTriggerDialogues collide = cube.GetComponent<CollideTriggerDialogues>();
            if (collide != null)
            {
                collide.enabled = false;
            }
        }

        Debug.Log($"已禁用所有 {tag} 节点");
    }
}