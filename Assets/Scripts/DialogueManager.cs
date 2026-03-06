using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI 组件引用")]
    public GameObject dialogueUI;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI contentText;

    private DialogueLine[] currentLines;
    private int currentLineIndex = 0;
    private bool isTalking = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- 新增：每帧检测按键输入 ---
    private void Update()
    {
        // 如果当前正在对话，并且玩家按下了 Enter 键（Return 是主键盘回车，KeypadEnter 是小键盘回车）
        if (isTalking && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            ShowNextLine();
        }
    }
    // ------------------------------

    public void StartDialogue(DialogueLine[] lines)
    {
        if (isTalking) return;

        currentLines = lines;
        currentLineIndex = 0;
        isTalking = true;

        dialogueUI.SetActive(true);
        ShowNextLine();
    }

    // 这个方法保留，如果你的 UI 上还有透明按钮，依然可以支持鼠标点击
    public void OnClickNext()
    {
        if (!isTalking) return;
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
        speakerText.text = line.speaker;
        contentText.text = line.content;

        currentLineIndex++;
    }

    private void EndDialogue()
    {
        isTalking = false;
        dialogueUI.SetActive(false);
    }
}