using UnityEngine;

// 直接把数据结构写在同一个文件里，加上 [System.Serializable] 就能在 Unity 面板里看到了
[System.Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(2, 5)] // 让在 Inspector 里的输入框大一点，方便写长句子
    public string content;
}

public class DialogueTrigger : MonoBehaviour
{
    [Header("在这里配置这组对话的内容")]
    public DialogueLine[] dialogueLines;

    [Header("触发设置")]
    public bool playOnStart = false; // 勾选则游戏运行直接播放

    private void Start()
    {
        if (playOnStart)
        {
            TriggerDialogue();
        }
    }

    // 调用 Manager 播放这组对话
    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogueLines);
    }

    // 举例：碰撞触发
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }
}