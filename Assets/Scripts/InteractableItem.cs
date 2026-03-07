using UnityEngine;

// Require a Collider component to detect the player
[RequireComponent(typeof(Collider))]
public class InteractableItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "…Ò√ÿŒÔ∆∑";

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