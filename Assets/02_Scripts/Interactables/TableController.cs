using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************
//创建人： Trigger 
//功能说明：桌子/祭坛交互触发器
//*****************************************
public class TableController : MonoBehaviour
{
    [Header("绑定 UI 管理器")]
    public ShopManager shopManager; // 【关键修改】：不再直接拖入 Panel，而是拖入整个 ShopManager 脚本

    // 用来记录玩家是否在交互范围内
    private bool canInteract = false;

    void Update()
    {
        // 只有当玩家在范围内，并且按下了 F 键时，才触发UI
        if (canInteract && Input.GetKeyDown(KeyCode.F))
        {
            // 确保 ShopManager 存在，并且当前商店没有处于打开状态
            if (shopManager != null && !shopManager.shopPanel.activeSelf)
            {
                // 【核心修改】：呼叫 ShopManager 专属的打开方法！所有的视角锁定都在这里面执行
                shopManager.OpenShop();
            }
        }
    }

    // 当有物体进入触发器范围时调用
    private void OnTriggerEnter(Collider other)
    {
        // 检查进入范围的是不是玩家
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            Debug.Log("玩家进入了潜意识祭坛范围，可以按F键交互！");
        }
    }

    // 当有物体离开触发器范围时调用
    private void OnTriggerExit(Collider other)
    {
        // 检查离开范围的是不是玩家
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            Debug.Log("玩家离开了祭坛范围。");

            // 安全机制：如果玩家离开了（比如被敌人击飞），自动调用退出方法，恢复视角和移动
            if (shopManager != null && shopManager.shopPanel.activeSelf)
            {
                shopManager.OnExitButtonClicked();
            }
        }
    }
}