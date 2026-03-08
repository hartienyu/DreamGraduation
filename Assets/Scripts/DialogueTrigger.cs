using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("对话JSON文件")]
    [Tooltip("直接拖入对应的对话JSON文件")]
    public TextAsset dialogueJSON;

    private CollideTriggerDialogues triggerDialogues;
    private bool dialogueTriggered = false;

    private void Start()
    {
        // 获取同一物体上的CollideTriggerDialogues组件
        triggerDialogues = GetComponent<CollideTriggerDialogues>();

        if (triggerDialogues == null)
        {
            Debug.LogError($"在 {gameObject.name} 上未找到 CollideTriggerDialogues 组件，请添加该组件！");
        }

        if (dialogueJSON == null)
        {
            Debug.LogWarning($"请在 {gameObject.name} 的 DialogueTrigger 组件中拖入对话JSON文件！");
        }
    }

    private void Update()
    {
        if (triggerDialogues != null && triggerDialogues.IsTriggerDialogue && !dialogueTriggered)
        {
            if (dialogueJSON != null)
            {
                TriggerDialogue();
            }
            else
            {
                Debug.LogError($"{gameObject.name} 的 DialogueTrigger 没有指定JSON文件！");
                dialogueTriggered = true; // 防止一直报错
            }
        }
    }

    public void TriggerDialogue()
    {
        if (dialogueTriggered) return;

        if (DialogueManager.Instance != null)
        {
            // ========== 新增：如果是 Huahuo3 剧情，先订阅对话结束的事件 ==========
            if (dialogueJSON.name == "Huahuo3")
            {
                DialogueManager.Instance.OnDialogueFinished += OnDialogueFinishedHandler;
            }

            // 开启对话
            DialogueManager.Instance.StartDialogue(dialogueJSON, triggerDialogues);
            dialogueTriggered = true;

            // 通知触发器对话已触发 (锁定玩家和视角)
            if (triggerDialogues != null)
            {
                triggerDialogues.OnDialogueTriggered();
            }

            Debug.Log($"{gameObject.name} 触发了对话: {dialogueJSON.name}");
        }
        else
        {
            Debug.LogError("找不到 DialogueManager 实例！");
        }
    }

    // ========== 新增：当监听到任意对话结束时触发 ==========
    private void OnDialogueFinishedHandler(string finishedDialogueName)
    {
        // 确认结束的确实是 Huahuo3
        if (finishedDialogueName == "Huahuo3")
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.StartQuest();
                Debug.Log("Huahuo3 对话已完全结束，任务正式开启！");
            }
            else
            {
                Debug.LogError("找不到 QuestManager 实例！");
            }

            // 任务触发后注销事件，避免重复触发或内存泄漏
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueFinished -= OnDialogueFinishedHandler;
            }
        }
    }

    public void ResetTrigger()
    {
        dialogueTriggered = false;
    }

    // 安全起见，如果物体被销毁，确保注销事件监听
    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished -= OnDialogueFinishedHandler;
        }
    }
}