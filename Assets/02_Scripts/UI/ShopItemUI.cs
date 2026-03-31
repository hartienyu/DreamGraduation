using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 挂载在商品卡片预制体上
public class ShopItemUI : MonoBehaviour
{
    [Header("UI 组件绑定")]
    public Image itemIconImage; // 【新增】用于显示物品图片的 Image 组件
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI stockText;
    public Button buyButton;

    private ShopManager shopManager;
    private int itemIndex;

    // 初始化卡片数据
    public void SetupCard(ShopManager manager, int index, ShopManager.ItemData data)
    {
        shopManager = manager;
        itemIndex = index;

        nameText.text = data.itemName;
        descText.text = data.description;
        priceText.text = data.price + " 碎片";

        // 【新增】配置图片显示逻辑
        if (data.itemIcon != null)
        {
            itemIconImage.sprite = data.itemIcon;
            itemIconImage.gameObject.SetActive(true); // 如果有图片就显示
        }
        else
        {
            itemIconImage.gameObject.SetActive(false); // 如果没配置图片就隐藏，防止显示白块
        }

        // 判断是否可销售与库存控制
        if (!data.isAvailable)
        {
            stockText.text = "暂未解锁";
            stockText.color = Color.red;
            SetButtonState(false, "锁定");
        }
        else if (data.maxQuantity <= 0)
        {
            stockText.text = "已售罄";
            stockText.color = Color.gray;
            SetButtonState(false, "售罄");
        }
        else
        {
            stockText.text = data.maxQuantity >= 999 ? "库存: 无限" : "剩余: " + data.maxQuantity;
            stockText.color = Color.white;
            SetButtonState(true, "献祭");
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void SetButtonState(bool interactable, string btnText)
    {
        buyButton.interactable = interactable;
        buyButton.GetComponentInChildren<TextMeshProUGUI>().text = btnText;
    }

    private void OnBuyClicked()
    {
        shopManager.TryBuyItem(itemIndex);
    }
}