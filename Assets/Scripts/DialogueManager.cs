using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class NewDialogueLine
{
    public string speaker;
    public string content;
}

[System.Serializable]
public class NewDialogueData
{
    public NewDialogueLine[] dialogue;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI组件引用")]
    public GameObject dialogueUI;
    public TextMeshProUGUI combinedText;

    private NewDialogueLine[] currentLines;
    private int currentLineIndex = 0;
    private bool isTalking = false;

    // ========== 新增：用于记住当前是哪个触发器开启的对话 ==========
    private CollideTriggerDialogues activeTrigger;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isTalking && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            ShowNextLine();
        }
    }

    // ========== 修改：增加第二个参数 trigger ==========
    public void StartDialogue(TextAsset jsonFile, CollideTriggerDialogues trigger = null)
    {
        if (isTalking || jsonFile == null) return;

        // 记录传过来的触发器
        activeTrigger = trigger;

        // 解析JSON
        NewDialogueData data = JsonUtility.FromJson<NewDialogueData>(jsonFile.text);

        if (data != null && data.dialogue != null && data.dialogue.Length > 0)
        {
            currentLines = data.dialogue;
            currentLineIndex = 0;
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
        if (currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        NewDialogueLine line = currentLines[currentLineIndex];

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

        // ========== 新增：对话结束，通知触发器恢复视角并解锁玩家 ==========
        if (activeTrigger != null)
        {
            activeTrigger.OnDialogueEnded();
            activeTrigger = null; // 用完清空，防止影响下次对话
        }

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