using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine; // 必须引入，用于精准控制和锁定视角

//*****************************************
//创建人： Trigger 
//功能说明：动态商店系统（潜意识祭坛），控制商品生成、购买逻辑与视角/移动锁定
//*****************************************
public class ShopManager : MonoBehaviour
{
    // 供其他脚本检查商店是否打开的全局状态标记
    public static bool IsShopOpen = false;
    public static int LastShopCloseFrame = -1; // 记录商店关闭时所在的帧数

    // 定义商品的数据结构
    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public Sprite itemIcon;    // 物品的图标/图片
        public string description;
        public int price;
        public int maxQuantity;
        public bool isAvailable;
        public ItemEffect effectType;
        public int effectValue;
    }

    // 物品效果枚举
    public enum ItemEffect { Heal, MaxHealthUp, SpeedUp, Skin }

    [Header("潜意识数据")]
    public int playerPoints = 2000;

    [Header("系统引用")]
    public PlayerHealth playerHealth;
    public PlayerMovement playerMovement;

    [Header("UI 引用")]
    public GameObject shopPanel;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI gemText; // 【新增】宝石显示UI


    [Header("预制体生成配置")]
    public GameObject itemPrefab; // 拖入你刚做好的商品预制体 (ItemCard_Template)
    public Transform itemContainer; // 拖入 Scroll View 里的 Content 父物体

    [Header("商品列表配置")]
    public List<ItemData> shopItems; // 在 Unity 面板里直接配置商品

    // 记录生成的 UI 卡片，方便后续刷新
    private List<ShopItemUI> spawnedCards = new List<ShopItemUI>();

    void Start()
    {
        UpdatePointsDisplay();
        GenerateShopUI();

        // 保险机制：确保开局时商店 UI 是关闭的，鼠标是锁定的
        if (shopPanel != null && shopPanel.activeSelf)
        {
            OnExitButtonClicked();
        }
    }

    // 监听 Esc 键关闭商店
    void Update()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnExitButtonClicked();
            }
        }
    }

    private void UpdatePointsDisplay()
    {
        if (pointsText != null) pointsText.text = "裂痕碎片: " + playerPoints;
        
        if (gemText != null)
        {
            if (PlayerData.Instance != null)
            {
                gemText.text = "宝石: " + PlayerData.Instance.gemCount;
            }
            else
            {
                // 如果找不到 PlayerData，至少让它显示 0，而不是不覆盖原本的占位符 "11111"
                gemText.text = "宝石: 0";
                Debug.LogWarning("警告：场景中没有找到 PlayerData 实例！请确保创建了一个空物体并挂载了 PlayerData 脚本。");
            }
        }
    }

    // 根据数据动态生成商店 UI
    private void GenerateShopUI()
    {
        // 先清空旧的卡片（防止重复打开商店时叠加）
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedCards.Clear();

        // 遍历商品列表，生成预制体
        for (int i = 0; i < shopItems.Count; i++)
        {
            GameObject newObj = Instantiate(itemPrefab, itemContainer);
            ShopItemUI cardUI = newObj.GetComponent<ShopItemUI>();

            // 将数据传递给卡片自身进行初始化
            cardUI.SetupCard(this, i, shopItems[i]);
            spawnedCards.Add(cardUI);
        }
    }

    // ==========================================
    // 商店开关与玩家状态控制逻辑
    // ==========================================

    // 打开商店的方法（由场景里的桌子/触发器呼叫）
    public void OpenShop()
    {
        IsShopOpen = true;
        shopPanel.SetActive(true);
        UpdatePointsDisplay(); // 每次打开商店时刷新文本显示

        // 1. 解锁并显示鼠标指针，以便点击商品
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        if (playerMovement != null)
        {
            // 2. 停止玩家走路/跳跃/冲刺
            playerMovement.canMove = false;

            // 3. 【终极锁定法】：直接把 Cinemachine 读取鼠标输入的通道给掐断
            if (playerMovement.freeLookCamera != null)
            {
                playerMovement.freeLookCamera.m_XAxis.m_InputAxisName = "";
                playerMovement.freeLookCamera.m_YAxis.m_InputAxisName = "";
                playerMovement.freeLookCamera.m_XAxis.m_InputAxisValue = 0;
                playerMovement.freeLookCamera.m_YAxis.m_InputAxisValue = 0;
            }
        }
    }

    // 退出商店的方法（绑定给 UI 右上角的关闭按钮，或由 Esc 键触发）
    public void OnExitButtonClicked()
    {
        IsShopOpen = false;
        LastShopCloseFrame = Time.frameCount; // 标记关闭的这一帧
        shopPanel.SetActive(false);

        // 1. 锁定并隐藏鼠标指针
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        if (playerMovement != null)
        {
            // 2. 恢复玩家移动
            playerMovement.canMove = true;

            // 3. 【恢复输入】：把读取通道的名字还给 Cinemachine
            if (playerMovement.freeLookCamera != null)
            {
                playerMovement.freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";
                playerMovement.freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
            }
        }
    }

    // ==========================================
    // 购买与效果应用逻辑
    // ==========================================

    // 尝试购买商品（由 ShopItemUI 中的按钮点击后调用）
    public void TryBuyItem(int index)
    {
        ItemData item = shopItems[index];

        if (item.effectType == ItemEffect.Skin)
        {
            // 如果玩家已经拥有了这套皮肤，则进行切换
            if (PlayerData.Instance != null && PlayerData.Instance.hasHuohuaSkin)
            {
                PlayerData.Instance.isHuohuaSkinEquipped = !PlayerData.Instance.isHuohuaSkinEquipped;
                if (NPCSkinManager.Instance != null)
                {
                    NPCSkinManager.Instance.ToggleSkin(PlayerData.Instance.isHuohuaSkinEquipped);
                }
                // 刷新卡片 UI 显示新的切换状态
                spawnedCards[index].SetupCard(this, index, item);
                return;
            }

            // 购买皮肤逻辑（消耗宝石）
            if (PlayerData.Instance != null && PlayerData.Instance.gemCount >= item.price)
            {
                PlayerData.Instance.gemCount -= item.price;
                PlayerData.Instance.hasHuohuaSkin = true;
                PlayerData.Instance.isHuohuaSkinEquipped = true; // 购买后默认装备
                
                if (NPCSkinManager.Instance != null)
                {
                    NPCSkinManager.Instance.ToggleSkin(true);
                }
                
                UpdatePointsDisplay();
                spawnedCards[index].SetupCard(this, index, item);
            }
            else
            {
                Debug.Log("提示：宝石不足，无法兑换皮肤...");
            }
            return;
        }

        // 常规商品逻辑（消耗碎片）
        if (playerPoints >= item.price)
        {
            // 扣钱与扣库存
            playerPoints -= item.price;
            item.maxQuantity--;

            UpdatePointsDisplay();
            ApplyItemEffect(item);

            // 刷新该卡片的 UI（同步数量或变成售罄状态）
            spawnedCards[index].SetupCard(this, index, item);
        }
        else
        {
            Debug.Log("提示：碎片不足，无法交换...");
        }
    }

    // 执行物品的实际效果
    private void ApplyItemEffect(ItemData item)
    {
        switch (item.effectType)
        {
            case ItemEffect.Heal:
                if (playerHealth != null) playerHealth.Heal(item.effectValue);
                Debug.Log($"吞下了{item.itemName}，恢复了{item.effectValue}点理智。");
                break;

            case ItemEffect.MaxHealthUp:
                if (playerHealth != null) playerHealth.UpgradeMaxHealth(item.effectValue);
                Debug.Log($"筑起心理防线，上限增加{item.effectValue}。");
                break;

            case ItemEffect.SpeedUp:
                if (playerMovement != null) playerMovement.UpgradeSpeed(2f, 3f, 5f);
                Debug.Log("逃避的本能被激发，移动速度提升。");
                break;
        }
    }
}