using UnityEditor.Experimental.GraphView;
using UnityEngine;

// 此文件绑在碰撞箱，真正实现进入碰撞箱后链接JSON文件并弹出对话内容
public class DialogueTrigger : MonoBehaviour
{
    [Header("对话JSON文件")]
    [Tooltip("直接拖入对应的对话JSON文件")]
    public TextAsset dialogueJSON;

    [Header("触发后要关闭的提示光/物体（可选）")]
    [Tooltip("比如发光的Point Light，对话开始后就会自动隐藏")]
    public GameObject objectToDisableOnTrigger;

    private CollideTriggerDialogues triggerDialoguesBox;
    private bool dialogueTriggered = false;

    // 游戏存档
    private ProgressManager progressManager;
    public DialogueNode nodeID;  // 在Inspector中为每个Cube赋值对应节点（如Init、Huahuo1）

    private void Start()
    {
        // ========== 修复：初始化 ProgressManager ==========
        progressManager = ProgressManager.Instance;

        // 获取同一物体上的CollideTriggerDialogues组件
        triggerDialoguesBox = GetComponent<CollideTriggerDialogues>();

        if (triggerDialoguesBox == null)
        {
            Debug.LogError($"在 {gameObject.name} 上未找到 CollideTriggerDialogues 组件，请添加该组件！");
        }

        if (dialogueJSON == null)
        {
            Debug.LogWarning($"请在 {gameObject.name} 的 DialogueTrigger 组件中拖入对话JSON文件！");
        }
    }

    private void Update()  // 不断判断CollideTriggerDialogues里的IsTriggerDialogue是否被触发
    {
        if (triggerDialoguesBox != null && triggerDialoguesBox.IsTriggerDialogue && !dialogueTriggered)
        {
            if (dialogueJSON != null)
            {
                TriggerDialogue();

                // 更新进度并存档
                NewDialogueData data = JsonUtility.FromJson<NewDialogueData>(dialogueJSON.text);  // 解析JSON
                if (data != null && !string.IsNullOrEmpty(data.nodeID))  // 更换字符串到枚举类型并赋值给：枚举类型的nodeID
                {
                    try
                    {
                        // 将字符串转换为枚举（忽略大小写）
                        nodeID = (DialogueNode)System.Enum.Parse(typeof(DialogueNode), data.nodeID, true);
                        Debug.Log($"成功转换: {nodeID}");

                        // ========== 确保 progressManager 不为空再调用 ==========
                        if (progressManager != null)
                        {
                            progressManager.UpdateProgress(nodeID, triggerDialoguesBox.transform.position);  // 更新进度（保存当前节点、玩家所在碰撞箱位置）
                        }
                        else
                        {
                            Debug.LogError("ProgressManager 实例为空，无法保存进度！");
                        }
                    }
                    catch (System.ArgumentException ex)
                    {
                        Debug.LogError($"无法转换 '{data.nodeID}' 为 DialogueNode 枚举: {ex.Message}");
                        nodeID = DialogueNode.Start; // 设置默认值
                    }
                }
            }
            else
            {
                Debug.LogError($"{gameObject.name} 的 DialogueTrigger 没有指定JSON文件！");
                dialogueTriggered = true; // 防止一直报错
            }
        }
    }

    public void TriggerDialogue()
    {
        if (dialogueTriggered) return;

        if (DialogueManager.Instance != null)
        {
            // ========== 新增：如果是 Huahuo3 剧情，先订阅对话结束的事件 ==========
            if (dialogueJSON.name == "Huahuo3")
            {
                DialogueManager.Instance.OnDialogueFinished += OnDialogueFinishedHandler;
            }

            // 开启对话
            DialogueManager.Instance.StartDialogue(dialogueJSON, triggerDialoguesBox);
            dialogueTriggered = true;

            // 触发后隐藏指定的发光物体或提示
            if (objectToDisableOnTrigger != null)
            {
                objectToDisableOnTrigger.SetActive(false);
            }

            // 通知触发器对话已触发 (锁定玩家和视角)
            if (triggerDialoguesBox != null)
            {
                triggerDialoguesBox.OnDialogueTriggered();
            }

            Debug.Log($"{gameObject.name} 触发了对话: {dialogueJSON.name}");
        }
        else
        {
            Debug.LogError("找不到 DialogueManager 实例！");
        }
    }

    // ========== 新增：当监听到任意对话结束时触发 ==========
    private void OnDialogueFinishedHandler(string finishedDialogueName)
    {
        // 确认结束的确实是 Huahuo3
        if (finishedDialogueName == "Huahuo3")
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.StartQuest();
                Debug.Log("Huahuo3 对话已完全结束，任务正式开启！");
            }
            else
            {
                Debug.LogError("找不到 QuestManager 实例！");
            }

            // 在Huahuo3结束后开启20分钟(1200秒)的倒计时
            if (CountdownTimer.Instance != null)
            {
                CountdownTimer.Instance.StartCountdown(1200f);
            }

            // 任务触发后注销事件，避免重复触发或内存泄漏
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueFinished -= OnDialogueFinishedHandler;
            }
        }
    }

    public void ResetTrigger()
    {
        dialogueTriggered = false;
    }

    // 安全起见，如果物体被销毁，确保注销事件监听
    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished -= OnDialogueFinishedHandler;
        }
    }
}