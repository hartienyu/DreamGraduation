using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 此文件实现对话功能逻辑

[System.Serializable]
public class DialogueOption
{
    public string optionText;      // 选项显示的文字
    public string nextFileToLoad;  // 选择后加载的下一个剧情文件名称(无后缀)
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

    // ========== 定义一个对话结束的事件通知 ==========
    public event Action<string> OnDialogueFinished;
    private string currentDialogueName;

    [Header("UI组件引用")]
    public GameObject dialogueUI;
    public TextMeshProUGUI combinedText;

    [Header("选项UI引用(可选)")]
    public GameObject optionsContainer; // 一个容纳按钮的空物体(建议挂载 Vertical Layout Group)
    public GameObject optionButtonPrefab; // 带有Button组件和Text(TMP)组件的预制体

    // 该对话的分支文件暂存
    private List<TextAsset> currentBranchDialogues;

    private NewDialogueLine[] currentLines;
    private int currentLineIndex = 0;  // 当前对话文件的行下标
    private bool isTalking = false;
    private bool isWaitingForOption = false;

    // 添加列表以记录生成的按钮，用于键盘控制
    private List<Button> activeOptionButtons = new List<Button>();
    private int currentOptionIndex = 0;

    // 用于记住当前是哪个触发器开启的对话
    private CollideTriggerDialogues activeTrigger;

    // 防止同帧按F触发剧情并直接跳过对话
    private bool justStartedDialogue = false;

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
        if (isWaitingForOption && activeOptionButtons.Count > 0)
        {
            // 通过上/W 和 下/S 键进行选项导航
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                currentOptionIndex--;
                if (currentOptionIndex < 0) currentOptionIndex = activeOptionButtons.Count - 1;
                activeOptionButtons[currentOptionIndex].Select();
                
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                currentOptionIndex++;
                if (currentOptionIndex >= activeOptionButtons.Count) currentOptionIndex = 0;
                activeOptionButtons[currentOptionIndex].Select();
            }
            // F键 确认当前选中的选项
            else if (Input.GetKeyDown(KeyCode.F))
            {
                activeOptionButtons[currentOptionIndex].onClick.Invoke();
            }
        }
        else if (isTalking && !isWaitingForOption && !justStartedDialogue && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.F)))
        {
            ShowNextLine();
        }
    }

    private void LateUpdate()
    {
        justStartedDialogue = false;
    }

    public void StartDialogue(TextAsset jsonFile, CollideTriggerDialogues triggerBox = null, List<TextAsset> branches = null)
    {
        if (isTalking || jsonFile == null) return;

        // 防止立刻跳过
        justStartedDialogue = true;

        // 记录传过来的触发器
        activeTrigger = triggerBox;
        
        // 记录传过来的分支字典
        currentBranchDialogues = branches;

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

        // ========== 新增：拦截并提取Splash内容（即系统提示），把它转化为Splash文字而不在对话框显示 ==========
        if (line.content.Contains("【系统提示】") || (!string.IsNullOrEmpty(line.speaker) && line.speaker.Contains("系统提示")))
        {
            if (EventTester.Instance != null)
            {
                // 将"【系统提示】"替换为空，并去除首尾多余空格
                string cleanSplashText = line.content.Replace("【系统提示】", "").Trim();
                EventTester.Instance.ShowSplash(cleanSplashText);
            }

            // 解析结束后，把这行无声息地跳过去并且马上加载下一行，形成对话不打断的效果！
            currentLineIndex++;
            ShowNextLine();
            return;
        }

        // 把人名和对话内容拼在一起
        if (combinedText != null)
        {
            if (!string.IsNullOrEmpty(line.speaker))
            {
                combinedText.text = $"<b><color=#FFD700>[{line.speaker}]</color></b>\n{line.content}";
            }
            else
            {
                combinedText.text = line.content;
            }
        }

        // 检查这一句话是否有附带的选项
        if (line.options != null && line.options.Length > 0)
        {
            isWaitingForOption = true;
            if (optionsContainer != null) optionsContainer.SetActive(true);

            // 清空旧的按钮
            if (optionsContainer != null)
            {
                activeOptionButtons.Clear();
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

                    Button btn = btnObj.GetComponent<Button>();
                    activeOptionButtons.Add(btn);
                    
                    btn.onClick.AddListener(() => 
                    {
                        OnOptionClicked(nextFileForBtn);
                    });
                }

                // 默认强制选中第一个按钮
                if (activeOptionButtons.Count > 0)
                {
                    currentOptionIndex = 0;
                    activeOptionButtons[0].Select();
                }
            }
            else
            {
                Debug.LogWarning("选项配置了但未设置 optionsContainer!");
                isWaitingForOption = false; // 继续让他正常走
                currentLineIndex++;
            }
        }
        else
        {
            // 如果没有配置选项，代表这就是一句普通的对话
            currentLineIndex++;
        }
    }

    private void OnOptionClicked(string nextFile)
    {
        isWaitingForOption = false;
        activeOptionButtons.Clear();

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
            // 从设定的分支列表中查找对应名字的 JSON 剧本
            TextAsset newJson = currentBranchDialogues?.Find(x => x != null && x.name == nextFile);
            
            if (newJson != null)
            {
                // 先将标志位置否以让它能重新Start
                isTalking = false; 
                // 继续接力传递当前的分支列表
                StartDialogue(newJson, activeTrigger, currentBranchDialogues);
            }
            else
            {
                Debug.LogError($"无法在分支剧情文件列表中找到名称为 [{nextFile}] 的剧情文件！请检查是不是忘记把 {nextFile}.json 拖入 DialogueTrigger 的 Branch Dialogues 列表里了。");
                EndDialogue(); // 找不到就结束对话
            }
        }
        else
        {
            // 如果选项没配，说明就当闲聊跳过，继续当前 JSON 的下一句话
            currentLineIndex++;
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

        // ========== 对话结束(UI已关闭)，广播通知当前对话已结束 ==========
        OnDialogueFinished?.Invoke(currentDialogueName);
    }

    public bool IsTalking()
    {
        return isTalking;
    }
}


