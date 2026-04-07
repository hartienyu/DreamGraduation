using UnityEngine;
using UnityEngine.Events;

// 任务管理器（每个任务的定义、其他外部调用接口）
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

    private GameObject bully;
    private bool hasBullyAppeared = false;  // 防止重复激活
    private bool setBullyAppeared = false;  // 是否激活Bully出现
    public void Start()
    {
        bully = GameObject.FindWithTag("Bully");
        if (bully == null)
        {
            Debug.LogError("找不到 Tag 为 'Bully' 的物体！请检查 Tag 设置");
        }
        else
        {
            // 确保一开始是隐藏的
            bully.SetActive(false);
            Debug.Log($"Bully 初始状态已设置为隐藏");
        }
    }

    private void Update()
    {
        // Press R to start the quest if it hasn't started yet
        if (!isQuestActive && currentItemsFound == 0 && Input.GetKeyDown(KeyCode.R))
        {
            StartQuest();
        }

        // 对于所有任务，只要时间少于10分钟，就让bully出现开始巡逻
        if (!hasBullyAppeared && CountdownTimer.Instance.GetCurrentTime() <= 600f && CountdownTimer.Instance.GetCurrentTime() > 0)
        {
            if (bully != null)
            {
                bully.SetActive(true);
                hasBullyAppeared = true;
                Debug.Log("霸凌者出现");
            }
        }
        else if (hasBullyAppeared && CountdownTimer.Instance.GetCurrentTime() <= 0f)  // 只在已出现且时间≤0时隐藏
        {
            if (bully != null)
            {
                bully.SetActive(false);
                hasBullyAppeared = false;
                Debug.Log("霸凌者消失");
            }
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


    // ========== 当监听到任意对话结束时触发任务开始 ==========
    public void OnDialogueFinishedHandler(string finishedDialogueName)
    {
        // 结束对话的是 Huahuo3
        if (finishedDialogueName == "Huahuo3")
        {
            StartQuest();
            Debug.Log("Huahuo3 对话已完全结束，任务正式开启！");

            // 在Huahuo3结束后开启20分钟(1200秒)的倒计时
            if (CountdownTimer.Instance != null)
            {
                CountdownTimer.Instance.StartCountdown(630f);
            }

            // 任务触发后注销事件，避免重复触发或内存泄漏
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueFinished -= OnDialogueFinishedHandler;
            }
        }
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