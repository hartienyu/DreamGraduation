using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("玩家数据")]
    public int playerPoints = 2000;

    [Header("系统引用")]
    public PlayerHealth playerHealth; // 【新增】引用玩家的血量脚本
    public PlayerMovement playerMovement; // 【新增】引用玩家的移动脚本

    [Header("UI 引用")]
    public GameObject shopPanel;
    public TextMeshProUGUI currentPointsText;

    [Header("商品复选框 (Toggles)")]
    public Toggle breadToggle;
    public Toggle chickenToggle;
    public Toggle armorToggle;
    public Toggle shoesToggle;

    // 商品价格设定
    private int priceBread = 20;
    private int priceChicken = 100;
    private int priceArmor = 1500;
    private int priceShoes = 500;

    // 商品效果设定
    private int healAmountBread = 30;   // 面包回血量
    private int healAmountChicken = 80; // 烤鸡回血量

    void Start()
    {
        UpdatePointsDisplay();
    }

    private void UpdatePointsDisplay()
    {
        if (currentPointsText != null)
        {
            currentPointsText.text = playerPoints.ToString();
        }
    }

    public void OnBuyButtonClicked()
    {
        int totalCost = 0;

        // 计算总价
        if (breadToggle.isOn) totalCost += priceBread;
        if (chickenToggle.isOn) totalCost += priceChicken;
        if (armorToggle.isOn) totalCost += priceArmor;
        if (shoesToggle.isOn) totalCost += priceShoes;

        if (totalCost == 0)
        {
            Debug.Log("商店提示：你没有选择任何商品！");
            return;
        }

        // 检查余额
        if (playerPoints >= totalCost)
        {
            // 扣钱
            playerPoints -= totalCost;
            UpdatePointsDisplay();

            if (breadToggle.isOn)
            {
                // 调用 PlayerHealth 里的 Heal 函数
                if (playerHealth != null) playerHealth.Heal(healAmountBread);
                Debug.Log("购买并吃下了面包，恢复了体力！");
            }

            if (chickenToggle.isOn)
            {
                if (playerHealth != null) playerHealth.Heal(healAmountChicken);
                Debug.Log("购买并吃下了烤鸡，大量恢复了体力！");
            }

            if (armorToggle.isOn)
            {
                if (playerHealth != null) playerHealth.UpgradeMaxHealth(50); // 比如永久增加 50 点生命上限
                Debug.Log("获得了防护服！永久增加了生命上限。");

                // 【可选优化】买过一次后，禁止重复购买
                armorToggle.interactable = false;
            }

            if (shoesToggle.isOn)
            {
                // 走路增加 2，跑步增加 3，冲刺增加 5 (数值你可以自己改)
                if (playerMovement != null) playerMovement.UpgradeSpeed(2f, 3f, 5f);
                Debug.Log("获得了冲刺鞋！永久增加了移动速度。");

                // 【可选优化】买过一次后，禁止重复购买
                shoesToggle.interactable = false;
            }

            // 购买完成后重置勾选
            ResetToggles();
        }
        else
        {
            Debug.Log("商店提示：穷鬼，点数不足！");
        }
    }

    public void OnExitButtonClicked()
    {
        shopPanel.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void ResetToggles()
    {
        breadToggle.isOn = false;
        chickenToggle.isOn = false;
        armorToggle.isOn = false;
        shoesToggle.isOn = false;
    }
}