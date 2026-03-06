using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("ÍÏÈë JSON ¶Ô»°ÎÄ¼þ")]
    public TextAsset dialogueJSON;

    [Header("´¥·¢ÉèÖÃ")]
    public bool playOnStart = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogueJSON);
    }
}