using UnityEngine;

public class CollideTriggerDialogues : MonoBehaviour
{
    [Header("触发设置")]
    public bool triggerOnce = true;  // 是否只触发一次
    public string playerTag = "Player";
    public GameObject nextColliderCube;  // 下一个对话的碰撞箱

    private bool isPlayerInTrigger = false;
    private bool hasTriggered = false;

    // 属性：是否应该触发对话
    public bool IsTriggerDialogue
    {
        get
        {
            // 玩家在触发器内，且（不是只触发一次 或 还没触发过）
            return isPlayerInTrigger && (!triggerOnce || !hasTriggered);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInTrigger = true;
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
            gameObject.SetActive(false);  // 隐藏自己
            Debug.Log($"玩家离开 {gameObject.name} 的触发区域");
        }
    }

    // 当对话被触发时调用
    public void OnDialogueTriggered()
    {
        hasTriggered = true;
        Debug.Log($"{gameObject.name} 的对话已触发");
    }

    // 重置触发器状态
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log($"{gameObject.name} 的触发器已重置");
    }
}