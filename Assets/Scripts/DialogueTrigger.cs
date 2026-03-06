using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("拖入 JSON 对话文件")]
    public TextAsset dialogueJSON;

    [Header("触发设置")]
    public bool playOnStart = false;

    private void Start()
    {
        if (playOnStart)
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogueJSON);
    }
}