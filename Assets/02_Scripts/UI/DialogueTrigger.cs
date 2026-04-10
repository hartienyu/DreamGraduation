using UnityEditor.Experimental.GraphView;
using UnityEngine;

// 链接碰撞箱及对话功能（此文件绑在碰撞箱，真正实现进入碰撞箱后链接JSON文件并弹出对话内容）
public class DialogueTrigger : MonoBehaviour
{
    [Header("对话JSON文件")]
    [Tooltip("直接拖入对应的对话JSON文件")]
    public TextAsset dialogueJSON;

    [Header("触发后要关闭的提示光/物体（可选）")]
    [Tooltip("比如发光的Point Light，对话开始后就会自动隐藏")]
    public GameObject objectToDisableOnTrigger;

    [Header("该剧情的分支文件（如果有选项）")]
    [Tooltip("如果该剧情存在分歧选项，将所有选项导向的JSON剧本拖入此列表即可")]
    public System.Collections.Generic.List<TextAsset> branchDialogues;

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
            // 开启对话，将自身配置的 branchDialogues 传给 DialogueManager
            DialogueManager.Instance.StartDialogue(dialogueJSON, triggerDialoguesBox, branchDialogues);
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

    public void ResetTrigger()
    {
        dialogueTriggered = false;

    }
}


