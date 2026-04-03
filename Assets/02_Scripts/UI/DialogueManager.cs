using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 此文件实现对话功能逻辑

[System.Serializable]
public class NewDialogueLine  // 对话格式
{
    public string speaker;
    public string content;
}

[System.Serializable]
public class NewDialogueData  // JSON文件读取后会自动按成员变量名赋值
{
    public string nodeID;
    public NewDialogueLine[] dialogue;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    // ========== 新增：定义一个对话结束的事件通知 ==========
    public event Action<string> OnDialogueFinished;
    private string currentDialogueName;

    [Header("UI组件引用")]
    public GameObject dialogueUI;
    public TextMeshProUGUI combinedText;

    private NewDialogueLine[] currentLines;
    private int currentLineIndex = 0;  // 当前对话文件的行下标
    private bool isTalking = false;

    // 用于记住当前是哪个触发器开启的对话
    private CollideTriggerDialogues activeTrigger;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // 玩家按下enter键跳下一行对话文本
        if (isTalking && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(TextAsset jsonFile, CollideTriggerDialogues triggerBox = null)
    {
        if (isTalking || jsonFile == null) return;

        // 记录传过来的触发器
        activeTrigger = triggerBox;

        // ========== 新增：记录当前正在进行的对话名称 ==========
        currentDialogueName = jsonFile.name;

        // 解析JSON
        NewDialogueData data = JsonUtility.FromJson<NewDialogueData>(jsonFile.text);

        if (data != null && data.dialogue != null && data.dialogue.Length > 0)
        {
            currentLines = data.dialogue;  // 一开始就存入整个dialogue内容
            currentLineIndex = 0;  // 当前对话文件的行下标
            isTalking = true;
            dialogueUI.SetActive(true);
            ShowNextLine();

            Debug.Log($"开始对话: {jsonFile.name}，共 {currentLines.Length} 句");
        }
        else
        {
            Debug.LogError($"JSON文件 {jsonFile.name} 格式错误或没有对话内容");
        }
    }

    private void ShowNextLine()
    {
        // 如果一个对话json文件的内容读完了
        if (currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        NewDialogueLine line = currentLines[currentLineIndex];  //当前行内容

        if (string.IsNullOrEmpty(line.speaker))
        {
            combinedText.text = $"{line.content}";  // Thinking...
        }
        else
        {
            combinedText.text = $"<b><color=#FFD700>[{line.speaker}]</color></b>\n{line.content}";  // Speaking...
        }

        currentLineIndex++;
    }

    private void EndDialogue()
    {
        isTalking = false;
        dialogueUI.SetActive(false);

        Debug.Log("对话结束");

        // 对话结束，通知触发器恢复视角并解锁玩家
        if (activeTrigger != null)
        {
            activeTrigger.OnDialogueEnded();
            activeTrigger = null; // 用完清空，防止影响下次对话
        }

        // ========== 新增：对话结束(UI已关闭)，广播通知当前对话已结束 ==========
        OnDialogueFinished?.Invoke(currentDialogueName);

        // 对话结束后启动倒计时（如果需要）
        if (CountdownTimer.Instance != null)
        {
            CountdownTimer.Instance.StartCountdown(60f);
        }
    }

    public bool IsTalking()
    {
        return isTalking;
    }
}