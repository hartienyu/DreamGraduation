using UnityEngine;

// Require a Collider component to detect the player
[RequireComponent(typeof(Collider))]
public class InteractableItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "神秘物品";

    // ========== 新增：对话配置 ==========
    [Header("剧情设置")]
    [Tooltip("捡起该物品后触发的对话文件。如果不需要触发剧情，留空即可。")]
    public TextAsset dialogueJSON;

    private bool isPlayerNear = false;

    private void Update()
    {
        // If the player is near, the quest is active, and the player presses F
        if (isPlayerNear && QuestManager.Instance.isQuestActive && Input.GetKeyDown(KeyCode.F))
        {
            PickUpItem();
        }
    }

    private void PickUpItem()
    {
        // Tell the QuestManager we collected an item
        QuestManager.Instance.CollectItem();

        // Hide the prompt UI because the item is about to disappear
        QuestUIManager.Instance.HideInteractPrompt();

        // ========== 新增：如果配置了剧情文件，则触发对话 ==========
        if (dialogueJSON != null)
        {
            if (DialogueManager.Instance != null)
            {
                // 因为是直接捡起物品，没有碰撞触发器，所以第二个参数传 null
                DialogueManager.Instance.StartDialogue(dialogueJSON, null);
                Debug.Log($"捡起物品 {itemName}，触发剧情：{dialogueJSON.name}");
            }
            else
            {
                Debug.LogError("找不到 DialogueManager 实例！");
            }
        }

        // Destroy the item object from the scene
        Destroy(gameObject);
    }

    // Triggered when the player enters the item's collider radius
    private void OnTriggerEnter(Collider other)
    {
        // Make sure your player object has the tag "Player"
        if (other.CompareTag("Player") && QuestManager.Instance.isQuestActive)
        {
            isPlayerNear = true;
            QuestUIManager.Instance.ShowInteractPrompt(itemName);
        }
    }

    // Triggered when the player walks away from the item
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            QuestUIManager.Instance.HideInteractPrompt();
        }
    }
}