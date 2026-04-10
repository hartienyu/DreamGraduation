using UnityEngine;

//*****************************************
// 功能说明：全局玩家数据管理（处理宝石、皮肤解锁等跨模块静态数据）
//*****************************************
public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("货币数据")]
    public int gemCount = 0; // 宝石数量
    public int memoryFragmentsCount = 0; // 记忆碎片数量

    [Header("皮肤解锁进度")]
    public bool hasHuohuaSkin = false; // 是否购买了花火皮肤
    public bool isHuohuaSkinEquipped = false; // 当前是否装备了花火皮肤

    private void Start()
    {
        if (ProgressManager.Instance != null && ProgressManager.Instance.GetCurrentSaveData() != null)
        {
            gemCount = ProgressManager.Instance.GetCurrentSaveData().gems;
            memoryFragmentsCount = ProgressManager.Instance.GetCurrentSaveData().memoryFragments;
        }
    }

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
            Destroy(this); 
        }
    }

    public void AddMemoryFragment(int amount)
    {
        memoryFragmentsCount += amount;
        if (ProgressManager.Instance != null && ProgressManager.Instance.GetCurrentSaveData() != null)
        {
            ProgressManager.Instance.GetCurrentSaveData().memoryFragments = memoryFragmentsCount;
            ProgressManager.Instance.SaveProgress();
        }
    }

    public void AddGem(int amount)
    {
        gemCount += amount;
        if (ProgressManager.Instance != null && ProgressManager.Instance.GetCurrentSaveData() != null)
        {
            ProgressManager.Instance.GetCurrentSaveData().gems = gemCount;
            ProgressManager.Instance.SaveProgress();
            Debug.Log($"[PlayerData] 获得了 {amount} 个宝石！当前宝石数量: {gemCount}");
        }
    }
}
