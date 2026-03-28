using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

//*****************************************
//创建人： Trigger 
//功能说明：玩家生命值与UI管理核心
//*****************************************
public class PlayerHealth : MonoBehaviour
{
    [Header("血量设置")]
    public int maxHealth = 200;
    public int currentHealth;

    [Header("UI 绑定")]
    public Slider healthBarSlider;
    public TextMeshProUGUI healthText;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarSlider != null)
            healthBarSlider.maxValue = maxHealth;

        UpdateHealthUI();
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.J)) TakeDamage(15);
        if (Input.GetKeyDown(KeyCode.H)) Heal(20);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

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

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;

        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }

    private void Die()
    {
        isDead = true;
        Invoke("Replay", 5f);
    }

    private void Replay()
    {
        SceneManager.LoadScene(0);
    }

    public void UpgradeMaxHealth(int bonusHealth)
    {
        maxHealth += bonusHealth;
        currentHealth += bonusHealth;

        if (healthBarSlider != null)
            healthBarSlider.maxValue = maxHealth;

        UpdateHealthUI();
    }
}