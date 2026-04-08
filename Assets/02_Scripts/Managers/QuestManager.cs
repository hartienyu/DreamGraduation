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
    // 事件：专门为代码监听用的任务开始事件
    public System.Action onQuestStartedEvent;

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

    public void StartQuest()
    {
        if (isQuestActive) return; // Prevent multiple start calls

        isQuestActive = true;
        currentItemsFound = 0;

        Debug.Log("Quest Started: Find the items.");
        onQuestStartedEvent?.Invoke(); // 通知所有的 InteractableItem 亮光
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

        // 【新增逻辑】：羁绊任务完成后触发记忆碎片解锁
        if (MemoryManager.Instance != null && !MemoryManager.Instance.isMemory1Unlocked)
        {
            MemoryManager.Instance.UnlockMemory1_Task1Completed();
        }
        else if (MemoryManager.Instance != null && MemoryManager.Instance.isMemory1Unlocked)
        {
            // 如果片段1已经解锁了，说明这次完成的是任务2
            MemoryManager.Instance.UnlockMemory2_Task2Completed();
        }

        OnQuestCompleted?.Invoke(); // Trigger UI update，也用来激活后续物体或剧情
    }
}