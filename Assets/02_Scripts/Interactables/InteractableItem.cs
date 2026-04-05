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

    [Header("提示光设置")]
    [Tooltip("挂载在物品上的提示光(如Point Light)，任务开始时亮起，交互后熄灭。")]
    public GameObject pointLight;

    [Header("互动后操作")]
    [Tooltip("互动后是否销毁该物品？(如废纸应该销毁，而垃圾桶不应该销毁)")]
    public bool destroyOnInteract = true;
    private bool hasInteracted = false;

    private bool isPlayerNear = false;

    private void Start()
    {
        // 初始状态下隐藏发光提示
        if (pointLight != null)
        {
            pointLight.SetActive(false);
        }

        // 监听任务开始事件
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestStartedEvent += ShowLight;
            
            // 如果开始时任务已经在进行，直接亮灯
            if (QuestManager.Instance.isQuestActive && !hasInteracted)
            {
                ShowLight();
            }
        }
    }

    private void ShowLight()
    {
        if (pointLight != null && !hasInteracted)
        {
            pointLight.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // 注销事件防止内存泄漏
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestStartedEvent -= ShowLight;
        }
    }

    private void Update()
    {
        // If the player is near, the quest is active, and the player presses F
        if (isPlayerNear && !hasInteracted && QuestManager.Instance.isQuestActive && Input.GetKeyDown(KeyCode.F))
        {
            PickUpItem();
        }
    }

    private void PickUpItem()
    {
        hasInteracted = true; // 标记为已交互

        // 交互后关闭发光提示光
        if (pointLight != null)
        {
            pointLight.SetActive(false);
        }

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

        // 处理物品本身是否销毁（如果是实体大物件或者触发器，一般不销毁）
        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }
        else
        {
            // 如果不销毁，我们至少关掉碰撞体防止玩家连续刷剧情
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    // Triggered when the player enters the item's collider radius
    private void OnTriggerEnter(Collider other)
    {
        // Make sure your player object has the tag "Player"
        if (other.CompareTag("Player") && !hasInteracted && QuestManager.Instance.isQuestActive)
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