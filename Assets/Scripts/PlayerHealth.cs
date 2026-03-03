using UnityEngine;
using UnityEngine.UI;
using TMPro; // 【新增】必须引入这个才能控制 TextMeshPro 文本

public class PlayerHealth : MonoBehaviour
{
    [Header("血量设置")]
    public int maxHealth = 200;      // 把最大血量改成 200
    public int currentHealth;

    [Header("UI 绑定")]
    public Slider healthBarSlider;
    public TextMeshProUGUI healthText; // 【新增】用于显示 200/200 的文本框

    void Start()
    {
        // 游戏开始时，满血
        currentHealth = maxHealth;

        // 初始化血条的最大值
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
        }

        // 刷新一次 UI 显示
        UpdateHealthUI();
    }

    void Update()
    {
        // 测试代码：按 J 扣血，按 H 回血
        if (Input.GetKeyDown(KeyCode.J)) TakeDamage(15);
        if (Input.GetKeyDown(KeyCode.H)) Heal(20);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // 每次扣血后，更新UI
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // 每次回血后，更新UI
        UpdateHealthUI();
    }

    // 【新增】统管所有血量 UI 更新的函数
    private void UpdateHealthUI()
    {
        // 更新滑动条的进度
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        // 更新数字文本显示，格式为 "当前血量 / 最大血量"
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log("玩家死亡！游戏结束！");
    }

    // ========== 商店升级接口 ==========
    public void UpgradeMaxHealth(int bonusHealth)
    {
        maxHealth += bonusHealth;
        currentHealth += bonusHealth; // 上限增加的同时，也给玩家补上这部分血量

        // 必须更新滑动条的最大值
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
        }

        UpdateHealthUI();
        Debug.Log($"最大生命值已永久提升！当前上限:{maxHealth}");
    }
}