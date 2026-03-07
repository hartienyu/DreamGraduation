using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Reflection;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string content;
}

[System.Serializable]
public class DialogueData
{
    public DialogueLine[] dialoguePlot2_1;  // Plot 2.1 dialogue
    public DialogueLine[] dialoguePlot2_2;
    public DialogueLine[] dialoguePlot2_3;
    public DialogueLine[] dialoguePlot2_4;

    public DialogueLine[] dialoguePlot3_1;
    public DialogueLine[] dialoguePlot3_2;
    public DialogueLine[] dialoguePlot3_3;
    public DialogueLine[] dialoguePlot3_4;

    // 可以继续添加更多剧情...
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI组件引用")]
    public GameObject dialogueUI;
    public TextMeshProUGUI combinedText;

    // 使用列表来存储所有要播放的剧情字段名
    private List<string> plotFieldNames = new List<string>();
    private int currentPlotListIndex = 0;  // 当前在列表中的索引

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

    // 初始化所有要播放的剧情顺序
    private void InitializePlotSequence()
    {
        plotFieldNames.Clear();

        // 手动添加你想要的剧情顺序
        // 你可以自由组合和排序，不需要按照数字顺序

        // 剧情2的所有部分
        plotFieldNames.Add("dialoguePlot2_1");
        plotFieldNames.Add("dialoguePlot2_2");
        plotFieldNames.Add("dialoguePlot2_3");
        plotFieldNames.Add("dialoguePlot2_4");

        // 剧情3的所有部分
        plotFieldNames.Add("dialoguePlot3_1");
        plotFieldNames.Add("dialoguePlot3_2");
        plotFieldNames.Add("dialoguePlot3_3");
        plotFieldNames.Add("dialoguePlot3_4");

        // 如果还有更多剧情，继续添加
        // plotFieldNames.Add("dialoguePlot4_1");
        // plotFieldNames.Add("dialoguePlot4_2");

        currentPlotListIndex = 0;
    }

    // 接收 TextAsset (JSON文件) 并开始播放
    public void StartDialogue(TextAsset jsonFile)
    {
        if (isTalking || jsonFile == null) return;

        // 如果还没有初始化剧情顺序，或者想重新初始化
        if (plotFieldNames.Count == 0)
        {
            InitializePlotSequence();
        }

        DialogueData data = JsonUtility.FromJson<DialogueData>(jsonFile.text);

        // 检查是否还有更多剧情要播放
        if (currentPlotListIndex >= plotFieldNames.Count)
        {
            Debug.Log("所有剧情已播放完毕");
            return;
        }

        // 获取当前要播放的剧情字段名
        string fieldName = plotFieldNames[currentPlotListIndex];
        Debug.Log($"正在加载: {fieldName}");

        var field = typeof(DialogueData).GetField(fieldName);
        if (field != null)
        {
            currentLines = field.GetValue(data) as DialogueLine[];

            if (currentLines != null && currentLines.Length > 0)
            {
                currentLineIndex = 0;
                isTalking = true;
                dialogueUI.SetActive(true);
                ShowNextLine();

                // 移动到下一个剧情索引
                currentPlotListIndex++;
            }
            else
            {
                Debug.LogWarning($"{fieldName} 没有对话内容");
                // 如果没有内容，尝试播放下一个
                currentPlotListIndex++;
                StartDialogue(jsonFile); // 递归调用播放下一个
            }
        }
        else
        {
            Debug.LogError($"找不到字段: {fieldName}");
            // 如果找不到字段，尝试播放下一个
            currentPlotListIndex++;
            StartDialogue(jsonFile); // 递归调用播放下一个
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

        DialogueLine line = currentLines[currentLineIndex];

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

        // 对话一结束，立即启动 60 秒倒计时
        if (CountdownTimer.Instance != null)
        {
            CountdownTimer.Instance.StartCountdown(60f);
        }
    }

    // 重置所有进度（如果需要重新开始）
    public void ResetProgress()
    {
        currentPlotListIndex = 0;
        isTalking = false;
        dialogueUI.SetActive(false);
        currentLines = null;
        currentLineIndex = 0;
    }

    // 方法2：自动扫描所有剧情字段（更灵活但需要约定命名规则）
    private void AutoScanPlotFields()
    {
        plotFieldNames.Clear();

        FieldInfo[] fields = typeof(DialogueData).GetFields();
        foreach (FieldInfo field in fields)
        {
            // 筛选出以 "dialoguePlot" 开头的字段
            if (field.Name.StartsWith("dialoguePlot") &&
                field.FieldType == typeof(DialogueLine[]))
            {
                plotFieldNames.Add(field.Name);
            }
        }

        // 按名称排序（如果需要）
        plotFieldNames.Sort();

        currentPlotListIndex = 0;

        Debug.Log($"自动扫描到 {plotFieldNames.Count} 个剧情字段");
    }

    // 使用自动扫描版本的方法
    public void StartDialogueAuto(TextAsset jsonFile)
    {
        if (isTalking || jsonFile == null) return;

        if (plotFieldNames.Count == 0)
        {
            AutoScanPlotFields();
        }

        DialogueData data = JsonUtility.FromJson<DialogueData>(jsonFile.text);

        if (currentPlotListIndex >= plotFieldNames.Count)
        {
            Debug.Log("所有剧情已播放完毕");
            return;
        }

        string fieldName = plotFieldNames[currentPlotListIndex];
        Debug.Log($"正在加载: {fieldName}");

        var field = typeof(DialogueData).GetField(fieldName);
        if (field != null)
        {
            currentLines = field.GetValue(data) as DialogueLine[];

            if (currentLines != null && currentLines.Length > 0)
            {
                currentLineIndex = 0;
                isTalking = true;
                dialogueUI.SetActive(true);
                ShowNextLine();
                currentPlotListIndex++;
            }
            else
            {
                Debug.LogWarning($"{fieldName} 没有对话内容");
                currentPlotListIndex++;
                StartDialogueAuto(jsonFile);
            }
        }
        else
        {
            Debug.LogError($"找不到字段: {fieldName}");
            currentPlotListIndex++;
            StartDialogueAuto(jsonFile);
        }
    }
}