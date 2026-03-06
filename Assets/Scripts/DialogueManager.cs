using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string content;
}

[System.Serializable]
public class DialogueData
{
    public DialogueLine[] dialogueLines; // 变量名必须和 JSON 里的键名一模一样
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI 组件引用")]
    public GameObject dialogueUI;
    // 现在只需要一个 Text 组件了
    public TextMeshProUGUI combinedText;

    private DialogueLine[] currentLines;
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

        // 解析 JSON 文本为 C# 对象
        DialogueData data = JsonUtility.FromJson<DialogueData>(jsonFile.text);
        currentLines = data.dialogueLines;
        currentLineIndex = 0;
        isTalking = true;

        dialogueUI.SetActive(true);
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentLines[currentLineIndex];

        // 【核心修改点】将名字和内容合并，并使用富文本给名字上色
        // 这里用了金黄色(#FFD700)并将名字加粗(<b>)，你可以根据需要修改
        combinedText.text = $"<b><color=#FFD700>[{line.speaker}]</color></b>\n{line.content}";

        currentLineIndex++;
    }

    private void EndDialogue()
    {
        isTalking = false;
        dialogueUI.SetActive(false);

        // 【新增】对话一结束，立刻启动 60 秒倒计时
        if (CountdownTimer.Instance != null)
        {
            CountdownTimer.Instance.StartCountdown(60f); // 这里的 60f 可以改成你想要的秒数
        }
    }
}