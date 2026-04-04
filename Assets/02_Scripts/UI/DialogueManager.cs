using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 此文件实现对话功能逻辑

[System.Serializable]
public class DialogueOption
{
    public string optionText;      // 选项显示的文字
    public string nextFileToLoad;  // 选择后加载的下一个剧情文件名称(无后缀)，需放在 Resources/Dialogues 目录下
}

[System.Serializable]
public class NewDialogueLine  // 对话格式
{
    public string speaker;
    public string content;
    public DialogueOption[] options; // 剧情选项列表
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

    [Header("选项UI引用(可选)")]
    public GameObject optionsContainer; // 一个容纳按钮的空物体(建议挂载 Vertical Layout Group)
    public GameObject optionButtonPrefab; // 带有Button组件和Text(TMP)组件的预制体

    private NewDialogueLine[] currentLines;
    private int currentLineIndex = 0;  // 当前对话文件的行下标
    private bool isTalking = false;
    private bool isWaitingForOption = false;

    // 用于记住当前是哪个触发器开启的对话
    private CollideTriggerDialogues activeTrigger;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() 
    {
        if (optionsContainer != null) optionsContainer.SetActive(false);
    }

    private void Update()
    {
        // 只有在对话中且不是在等待选择选项时，才可以按下确定键继续
        if (isTalking && !isWaitingForOption && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
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
            combinedText.text = "{line.content}";  // Thinking...
        }
        else
        {
            combinedText.text = "<b><color=#FFD700>[{line.speaker}]</color></b>\n{line.content}";  // Speaking...
        }

        currentLineIndex++;

        // 检查这一句话是否有附带的选项
        if (line.options != null && line.options.Length > 0)
        {
            isWaitingForOption = true;
            if (optionsContainer != null) optionsContainer.SetActive(true);

            // 清空旧的按钮
            if (optionsContainer != null)
            {
                foreach (Transform child in optionsContainer.transform) 
                {
                    Destroy(child.gameObject);
                }

                // 实例化新按钮
                foreach (var opt in line.options)
                {
                    GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer.transform);
                    btnObj.GetComponentInChildren<TextMeshProUGUI>().text = opt.optionText;
                    
                    string nextFileForBtn = opt.nextFileToLoad;

                    btnObj.GetComponent<Button>().onClick.AddListener(() => 
                    {
                        OnOptionClicked(nextFileForBtn);
                    });
                }
            }
            else
            {
                Debug.LogWarning("选项配置了但未设置 optionsContainer!");
                isWaitingForOption = false; // 继续让他正常走
            }
        }
    }

    private void OnOptionClicked(string nextFile)
    {
        isWaitingForOption = false;

        // 隐藏并清空选项容器
        if (optionsContainer != null) 
        {
            optionsContainer.SetActive(false);
            foreach (Transform child in optionsContainer.transform) 
            {
                Destroy(child.gameObject);
            }
        }

        // 如果配置了下一个剧情文件，加载它，产生故事分支
        if (!string.IsNullOrEmpty(nextFile))
        {
            TextAsset newJson = Resources.Load<TextAsset>("Dialogues/" + nextFile);
            if (newJson != null)
            {
                // 先将标志位置否以让它能重新Start
                isTalking = false; 
                StartDialogue(newJson, activeTrigger);
            }
            else
            {
                Debug.LogError($"无法在 Resources/Dialogues/ 文件夹下找到剧情文件: {nextFile}");
                EndDialogue(); // 找不到就结束对话
            }
        }
        else
        {
            // 如果选项没配，说明就当闲聊跳过，继续当前 JSON 的下一句话
            ShowNextLine();
        }
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
    }

    public bool IsTalking()
    {
        return isTalking;
    }
}