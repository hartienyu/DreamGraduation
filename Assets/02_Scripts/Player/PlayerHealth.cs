using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

//*****************************************
//创建人： Trigger 
//功能说明：玩家生命值与勇气值管理核心 (已整合 Courage 机制)
//*****************************************
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance; // 单例模式方便其他脚本调用勇气剧情接口

    [Header("Health 玩家血量设置")]
    public int maxHealth = 100;
    public int currentHealth;
    private const float BASE_HP = 100f; // 基础血量

    [Header("Health 玩家血量UI绑定")]
    public Slider healthBarSlider;
    public TextMeshProUGUI healthText;

    [Header("Courage 勇气机制设置")]
    public int courageLevel = 0;         // 始终保持在Lv0
    public float currentCourage = 0f;    // 当前勇气值
    public float maxCourage = 100f;      // 当前勇气值上限, Lv0上限为100, 之后不再增加
    public bool isBondingActive = false; // 是否处于羁绊剧情
    
    private float courageTimer = 0f;     // 用来计时，每10秒+1

    [Header("Courage 勇气UI绑定")]
    [Tooltip("显示勇气值比例的滑动条")]
    public Slider courageSlider;
    [Tooltip("显示“勇气值Lv0”这类文本的组件")]
    public TextMeshProUGUI courageLevelText; // 对应 Courage
    [Tooltip("显示“0/100”这类数值的组件")]
    public TextMeshProUGUI courageText;      // 对应 CourageText

    private bool isDead = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // 游戏开始，初始化Lv0状态
        InitLevel0();

        // 监听对话系统，处理勇气升级逻辑
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished += HandleDialogueFinished;
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished -= HandleDialogueFinished;
        }
    }

    // 核心剧情对话结束处理
    private void HandleDialogueFinished(string dialogueName)
    {
        if (dialogueName == "Task1.2") // 如果这是烧笔记本
        {
            UnlockLevel1_BurnNotebook();
        }
        else if (dialogueName.Equals("Huahuo3", System.StringComparison.OrdinalIgnoreCase))
        {
            StartBondingPhase();
            
            if (QuestManager.Instance != null) QuestManager.Instance.StartQuest();
            if (CountdownTimer.Instance != null) CountdownTimer.Instance.StartCountdown(1200f); // 20分钟
        }
        else if (dialogueName == "Task2_BranchA_TearPaper" || dialogueName == "Task2_BranchB_KeepPaper") // 假设这是处理画纸
        {
            UnlockLevel2_DisposePaper();
        }
    }

    void Update()
    {
        if (isDead) return;

        // 如果进入羁绊剧情，随存活时间流逝增加勇气值
        if (isBondingActive && currentCourage < maxCourage)
        {
            courageTimer += Time.deltaTime;
            if (courageTimer >= 10f) // 每 10 秒钟增加 1 点勇气值
            {
                AddCourage(1f);
                courageTimer = 0f;
                Debug.Log("[Courage] 生存满 10 秒，勇气值自动 +1");
            }
        }

        // 测试功能按键
        if (Input.GetKeyDown(KeyCode.J)) TakeDamage(15);
        if (Input.GetKeyDown(KeyCode.H)) Heal(20);
    }

    // ================= 勇气剧情触发接口 =================

    public void InitLevel0()
    {
        courageLevel = 0;
        maxCourage = 100f;
        currentCourage = 0f;
        
        UpdateMaxHealth();
        currentHealth = maxHealth; // 初始化满血

        UpdateAllUI();
    }

    public void StartBondingPhase()
    {
        isBondingActive = true;
        Debug.Log("[Courage] 进入羁绊剧情，勇气值每秒提升！");
    }

    public void StopBondingPhase()
    {
        isBondingActive = false;
    }

    public void UnlockLevel1_BurnNotebook()
    {
        // 不再提升等级，保持在 Lv.0
        // maxCourage = 200f; // 移除：完成 Task 1.2 不再增加上限
        AddCourage(50f);
        Debug.Log($"[Courage] 烧毁笔记本，获取 50 勇气值！继续保留在 Lv.0");

        // 发放1个宝石
        if (PlayerData.Instance != null) 
        {
            PlayerData.Instance.AddGem(1);
        }

        // 剧情Task1.2结束后，触发记忆碎片1
        if (MemoryManager.Instance != null && !MemoryManager.Instance.isMemory1Unlocked)
        {
            MemoryManager.Instance.UnlockMemory1_Task1Completed();
        }
    }

    private bool hasTriggeredTask2 = false; 

    public void UnlockLevel2_DisposePaper()
    {
        if (hasTriggeredTask2) return;
        hasTriggeredTask2 = true;
        
        // 移除：不再增加上限和等级
        AddCourage(50f);
        Debug.Log($"[Courage] 处理画纸，获取 50 勇气值！继续保留在 Lv.0");

        // 剧情Task2结束后，触发记忆碎片2
        if (MemoryManager.Instance != null && !MemoryManager.Instance.isMemory2Unlocked)
        {
            MemoryManager.Instance.UnlockMemory2_Task2Completed();
        }
    }

    public void AddCourage(float amount)
    {
        currentCourage += amount;
        // 限制在 0 和 maxCourage 之间
        currentCourage = Mathf.Clamp(currentCourage, 0f, maxCourage);

        UpdateMaxHealth();  // 因勇气改变而更新最大血量
        UpdateAllUI();
    }

    // ================= 生命值计算与受击 =================

    // 公式：Max HP = 基础血量 (100) + (当前勇气值 × 0.5)
    private void UpdateMaxHealth()
    {
        int oldMaxHealth = maxHealth;
        maxHealth = Mathf.RoundToInt(BASE_HP + currentCourage * 0.5f);

        // 如果血量上限被拉伸/提高了，也要等比回复当前血量
        int difference = maxHealth - oldMaxHealth;
        if (difference > 0)
        {
            currentHealth += difference;
        }

        // 防溢出保护
        if (currentHealth > maxHealth) currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateAllUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateAllUI();
    }

    // ================= 物理碰撞与受击 =================

    private float damageCooldown = 0f;
    private float damageInterval = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bully"))
        {
            TakeDamage(15);         // 扣生命值 15
            AddCourage(-50f);       // 扣勇气值 50
            damageCooldown = 0f;
            Debug.Log("Player is under attack! Loses HP and Courage.");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bully"))
        {
            damageCooldown += Time.deltaTime;
            if (damageCooldown >= damageInterval)
            {
                TakeDamage(15);         // 扣生命值 15
                AddCourage(-50f);       // 扣勇气值 50
                damageCooldown = 0f;
                Debug.Log("Player is under continuous attack!");
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bully"))
        {
            damageCooldown = 0f;
            Debug.Log("Player escaped from Bully");
        }
    }

    // ================= 统一 UI 更新 =================

    private void UpdateAllUI()
    {
        // 1. 生命UI
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }

        // 2. 勇气UI
        if (courageSlider != null)
        {
            courageSlider.maxValue = maxCourage;
            courageSlider.value = currentCourage;
        }
        // 显示: "勇气值Lv0"
        if (courageLevelText != null)
        {
            courageLevelText.text = $"勇气值 Lv{courageLevel}";
        }
        // 显示: "0/100" (四舍五入到整数)
        if (courageText != null)
        {
            courageText.text = $"{currentCourage:F0}/{maxCourage:F0}";
        }
    }

    private void Die()
    {
        isDead = true;
        // 如果 GameOverUI 存在，则直接弹出结束画面
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.TriggerGameOver("被霸凌者抓住了...\n生命值耗尽");
        }
        else
        {
            // 作为安全垫背：五秒后重载
            Invoke("Replay", 5f);
        }
    }

    private void Replay()
    {
        SceneManager.LoadScene(0);
    }

    public void UpgradeMaxHealth(int bonusHealth)
    {
        maxHealth += bonusHealth;
        currentHealth += bonusHealth;
        UpdateAllUI();
    }
}