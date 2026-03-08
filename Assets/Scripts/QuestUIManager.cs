using UnityEngine;
using TMPro;

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("Quest Tracker UI")]
    public GameObject questPanel;       // The background panel for the quest tracker
    public TextMeshProUGUI questText;   // The text showing progress (e.g., "Items: 1/3")

    [Header("Splash Screen UI")]
    public GameObject splashCanvas;

    [Header("Interaction Prompt UI")]
    public GameObject interactPanel;    // The panel that pops up near the item
    public TextMeshProUGUI interactText;// The text showing "[F] Apple"

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Hide UI elements at the start of the game
        questPanel.SetActive(false);
        splashCanvas.SetActive(false);
        interactPanel.SetActive(false);

        // Subscribe to QuestManager events
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted.AddListener(ShowQuestStarted);
            QuestManager.Instance.OnItemCollected.AddListener(UpdateQuestProgress);
            QuestManager.Instance.OnQuestCompleted.AddListener(ShowQuestCompleted);
        }
    }

    // --- Quest Tracker Methods ---

    private void ShowQuestStarted()
    {
        questPanel.SetActive(true);
        if (splashCanvas != null)
        {
            splashCanvas.SetActive(true);
            // 显示 3 秒后，自动调用 HideSplashCanvas 方法把它关掉
            Invoke(nameof(HideSplashCanvas), 3f);
        }
        questText.text = $"找到隐藏的物品： 0/{QuestManager.Instance.totalItemsToFind}";
    }

    private void UpdateQuestProgress(int current, int total)
    {
        questText.text = $"找到隐藏的物品： {current}/{total}";
    }

    private void ShowQuestCompleted()
    {
        questText.text = "任务成功！";
        // Hide the quest panel after 3 seconds
        Invoke(nameof(HideQuestPanel), 3f);
    }

    private void HideQuestPanel()
    {
        questPanel.SetActive(false);
    }

    // --- Interaction Prompt Methods ---

    public void ShowInteractPrompt(string itemName)
    {
        interactPanel.SetActive(true);
        // Format the text like Genshin Impact: "[F] ItemName"
        interactText.text = $"[F] {itemName}";
    }

    public void HideInteractPrompt()
    {
        interactPanel.SetActive(false);
    }

    private void HideSplashCanvas()
{
    if (splashCanvas != null) splashCanvas.SetActive(false);
}
}