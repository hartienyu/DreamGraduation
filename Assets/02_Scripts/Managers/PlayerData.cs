using UnityEngine;

//*****************************************
// 功能说明：全局玩家数据管理（处理宝石、皮肤解锁等跨模块静态数据）
//*****************************************
public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("货币数据")]
    public int gemCount = 0; // 宝石数量

    [Header("皮肤解锁进度")]
    public bool hasHuohuaSkin = false; // 是否购买了花火皮肤
    public bool isHuohuaSkinEquipped = false; // 当前是否装备了花火皮肤

    private void Awake()
    {
        // 经典的单例模式，确保跨场景不被销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 增加宝石的公共方法
    public void AddGem(int amount)
    {
        gemCount += amount;
        Debug.Log($"[PlayerData] 获得了 {amount} 个宝石！当前宝石数量: {gemCount}");
    }
}