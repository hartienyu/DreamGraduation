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

        // 检查是否已指定JSON文件
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
            DialogueManager.Instance.StartDialogue(dialogueJSON);
            dialogueTriggered = true;

            // 通知触发器对话已触发
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

    // 重置触发状态（如果需要重新触发）
    public void ResetTrigger()
    {
        dialogueTriggered = false;
    }
}