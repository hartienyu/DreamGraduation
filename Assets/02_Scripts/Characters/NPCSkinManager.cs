using UnityEngine;

//*****************************************
// 功能说明：NPC皮肤管理器，用于切换火花/花火外观
//*****************************************
public class NPCSkinManager : MonoBehaviour
{
    public static NPCSkinManager Instance { get; private set; }

    [Header("皮肤模型引用")]
    public GameObject defaultSkinObj; // 默认外观 (火花)
    public GameObject Skin2Obj;  // 新皮肤外观 (花火)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 游戏开始时读取PlayerData同步皮肤状态
        if (PlayerData.Instance != null)
        {
            ToggleSkin(PlayerData.Instance.isHuohuaSkinEquipped);
        }
        else
        {
            ToggleSkin(false); // 默认原皮
        }
    }

    // 切换皮肤的公共方法
    public void ToggleSkin(bool useHuohuaSkin)
    {
        if (defaultSkinObj != null) defaultSkinObj.SetActive(!useHuohuaSkin);
        if (Skin2Obj != null) Skin2Obj.SetActive(useHuohuaSkin);

        Debug.Log($"[NPCSkinManager] 皮肤已切换。当前使用花火皮肤: {useHuohuaSkin}");
    }
}