using UnityEngine;
using Cinemachine; // 必须引入

public class CollideTriggerDialogues : MonoBehaviour
{
    [Header("触发设置")]
    public bool triggerOnce = true;  // 是否只触发一次
    public string playerTag = "Player";
    public GameObject nextColliderCube;  // 下一个对话的碰撞箱
    [Header("触发NPC出现/隐藏")]
    public GameObject NPC;  // 待触发行为的人物对象

    [Header("强制看物品设置 (主视角版)")]
    [Tooltip("勾选后，主相机会自动看向该物品并锁死鼠标")]
    public bool forceLookAtItem = false;
    [Tooltip("相机需要强制看向的物品 (拖入场景里的物品)")]
    public Transform itemToLookAt;

    private bool isPlayerInTrigger = false;
    private bool hasTriggered = false;

    // 缓存玩家的移动脚本和相机的原始状态
    private PlayerMovement currentPlayer;
    private Transform originalLookAt;
    private string originalXAxisName;
    private string originalYAxisName;

    // 属性：是否应该触发对话
    public bool IsTriggerDialogue
    {
        get
        {
            return isPlayerInTrigger && (!triggerOnce || !hasTriggered);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInTrigger = true;
            currentPlayer = other.GetComponent<PlayerMovement>(); // 获取玩家脚本

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
                nextColliderCube.SetActive(true);  // 激活下一个对话
            }
            Debug.Log($"玩家进入 {gameObject.name} 的触发区域");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInTrigger = false;
            currentPlayer = null;
            gameObject.SetActive(false);  // 隐藏自己
            Debug.Log($"玩家离开 {gameObject.name} 的触发区域");
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