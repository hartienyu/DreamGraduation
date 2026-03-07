using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Settings")]
    public int totalItemsToFind = 3;

    // Internal state variables
    private int currentItemsFound = 0;
    public bool isQuestActive { get; private set; } = false;

    [Header("Quest Events")]
    // We use UnityEvents so we can easily connect UI updates in the Inspector or via code
    public UnityEvent OnQuestStarted;
    public UnityEvent<int, int> OnItemCollected;
    public UnityEvent OnQuestCompleted;

    private void Awake()
    {
        // Simple Singleton pattern for easy access
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Press R to start the quest if it hasn't started yet
        if (!isQuestActive && currentItemsFound == 0 && Input.GetKeyDown(KeyCode.R))
        {
            StartQuest();
        }
    }

    private void StartQuest()
    {
        isQuestActive = true;
        currentItemsFound = 0;

        Debug.Log("Quest Started: Find the items.");
        OnQuestStarted?.Invoke(); // Trigger UI update
    }

    // Called by the items when the player picks them up
    public void CollectItem()
    {
        if (!isQuestActive) return;

        currentItemsFound++;
        OnItemCollected?.Invoke(currentItemsFound, totalItemsToFind); // Trigger UI update

        // Check if quest is finished
        if (currentItemsFound >= totalItemsToFind)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        isQuestActive = false;
        Debug.Log("Quest Completed!");
        OnQuestCompleted?.Invoke(); // Trigger UI update
    }
}