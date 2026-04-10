using Cinemachine;
using UnityEngine;

// 此文件只控制对话碰撞箱（检测是否进入碰撞箱、处理锁定镜头）
public class CollideTriggerDialogues : MonoBehaviour
{
    // 在unity拖入object
    [Header("触发设置")]
    public bool triggerOnce = true;  // 是否只触发一次
    public bool hideOnExit = true;   // 对话结束后/离开时是否隐藏自身（例如隐形碰撞方块应该隐藏，实体如垃圾桶则不隐藏）

    public string playerTag = "Player";
    public GameObject nextColliderCube;  // 下一个对话的碰撞箱，实现链式出现及销毁
    [Header("触发NPC出现/隐藏")]
    public GameObject NPC;  // 待触发行为的人物对象

    [Header("强制看物品设置 (主视角版)")]
    [Tooltip("勾选后，主相机会自动看向该物品并锁死鼠标")]
    public bool forceLookAtItem = false;
    [Tooltip("相机需要强制看向的物品 (拖入场景里的物品)")]
    public Transform itemToLookAt;

    private bool isPlayerInTriggerBox = false;
    private bool hasTriggered = false;

    // 缓存玩家的移动脚本和相机的原始状态
    private PlayerMovement currentPlayer;
    private Transform originalLookAt;
    private string originalXAxisName;
    private string originalYAxisName;


    public bool IsTriggerDialogue  // 告诉DialogueTrigger.cs是否触发对话
    {
        get
        {
            return isPlayerInTriggerBox && (!triggerOnce || !hasTriggered);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInTriggerBox = true;  // 玩家进入碰撞箱
            currentPlayer = other.GetComponent<PlayerMovement>(); // 获取玩家脚本

            if (!triggerOnce || !hasTriggered)
            {
                if (NPC != null)
                {
                    if (NPC.activeSelf)
                    {
                        NPC.SetActive(false);  // 让火花（鬼魂）先消失
                    }
                    else
                    {
                        NPC.SetActive(true);
                    }
                }

                if (nextColliderCube != null)
                {
                    nextColliderCube.SetActive(true);  // 激活下一个对话碰撞箱
                }
            }

            Debug.Log($"玩家进入 {gameObject.name} 的触发区域");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInTriggerBox = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInTriggerBox = false;
            currentPlayer = null;
            
            // 通过开关控制是否隐藏自身（隐形触发器一般勾选，实体如垃圾桶则取消勾选）
            if (hideOnExit)
            {
                gameObject.SetActive(false);  
                Debug.Log($"玩家离开 {gameObject.name} 的触发区域并隐藏自身");
            }
            else
            {
                Debug.Log($"玩家离开 {gameObject.name} 的触发区域，保留自身显示");
            }
        }
    }

    // 当对话被触发时调用
    public void OnDialogueTriggered()
    {
        hasTriggered = true;
        Debug.Log($"{gameObject.name} 的对话已触发");

        if (forceLookAtItem)
        {
            LockPlayerAndFocusMainCamera();
        }
    }

    // 当对话结束时调用 (务必让你的对话系统在结束时调用这个方法)
    public void OnDialogueEnded()
    {
        if (forceLookAtItem)
        {
            UnlockPlayerAndRestoreMainCamera();
        }
        Debug.Log($"{gameObject.name} 的对话已结束，恢复自由");
    }

    // ================== 核心逻辑：锁定并操控主相机 ==================

    private void LockPlayerAndFocusMainCamera()
    {
        if (currentPlayer == null) return;

        // 1. 禁止玩家移动 (使用上一步在PlayerMovement中加的开关)
        currentPlayer.canMove = false;

        // 2. 接管 Cinemachine FreeLook 相机
        CinemachineFreeLook cam = currentPlayer.freeLookCamera;
        if (cam != null && itemToLookAt != null)
        {
            // 记录相机原本在看谁，以及原本的输入轴名称（通常是 "Mouse X" 和 "Mouse Y"）
            originalLookAt = cam.LookAt;
            originalXAxisName = cam.m_XAxis.m_InputAxisName;
            originalYAxisName = cam.m_YAxis.m_InputAxisName;

            // 把相机的注视目标改成物品！
            cam.LookAt = itemToLookAt;

            // 【关键点】清空输入轴名称，这样玩家甩鼠标就不会导致镜头抖动了
            cam.m_XAxis.m_InputAxisName = "";
            cam.m_YAxis.m_InputAxisName = "";
            // 将当前的输入值归零，防止切入瞬间有残留的滑动惯性
            cam.m_XAxis.m_InputAxisValue = 0f;
            cam.m_YAxis.m_InputAxisValue = 0f;
        }
    }

    private void UnlockPlayerAndRestoreMainCamera()
    {
        if (currentPlayer == null) return;

        // 1. 恢复玩家移动
        currentPlayer.canMove = true;

        // 2. 将主相机恢复原样
        CinemachineFreeLook cam = currentPlayer.freeLookCamera;
        if (cam != null)
        {
            // 把注视目标换回原本的（通常是玩家的某个骨骼点）
            cam.LookAt = originalLookAt;

            // 把鼠标控制权还给 Cinemachine
            cam.m_XAxis.m_InputAxisName = originalXAxisName;
            cam.m_YAxis.m_InputAxisName = originalYAxisName;
        }
    }

    // 重置触发器状态
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log($"{gameObject.name} 的触发器已重置");
    }
}