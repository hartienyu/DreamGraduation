using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("ÍÏÈë JSON ¶Ô»°ÎÄ¼þ")]
    public TextAsset dialogueJSON;

    private CollideTriggerDialogues triggerDialogues;
    private bool dialogueTriggered = false;  // 防止重复触发
    private void Start()
    {
        GameObject cube = GameObject.FindGameObjectWithTag("DialogueCube");
        triggerDialogues = cube.GetComponent<CollideTriggerDialogues>();

        if (triggerDialogues == null)
        {
            Debug.LogError("未找到 CollideTriggerDialogues 组件！");
        }
    }
    private void Update()
    {
        if (triggerDialogues != null && triggerDialogues.IsTriggerDialogue)
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogueJSON);
    }
}