using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 新的对话数据类，匹配你的JSON格式
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

    // 接收 TextAsset (JSON文件) 并开始播放
    public void StartDialogue(TextAsset jsonFile)
    {
        if (isTalking || jsonFile == null) return;

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

    // 显示下一行对话
    private void ShowNextLine()
    {
        if (currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        NewDialogueLine line = currentLines[currentLineIndex];

        // Dialogue's format
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

        // 对话结束后启动倒计时（如果需要）
        if (CountdownTimer.Instance != null)
        {
            CountdownTimer.Instance.StartCountdown(60f);
        }
    }

    // 检查是否正在对话
    public bool IsTalking()
    {
        return isTalking;
    }
}