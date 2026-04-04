using UnityEngine;

public class DialogueUnlocker : MonoBehaviour
{
    [Header("前提条件：哪个对话JSON文件结束？(填文件名，不带后缀)")]
    public string prerequisiteDialogueName;

    [Header("完成后要解锁/显示的游戏物体（比如垃圾桶）")]
    public GameObject objectToUnlock;

    private void Start()
    {
        // 游戏一开始，先隐藏物体，等待触发
        if (objectToUnlock != null)
        {
            objectToUnlock.SetActive(false);
        }

        // 监听对话结束的广播
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished += CompleteCondition;
        }
    }

    private void CompleteCondition(string finishedDialogue)
    {
        // 如果结束的对话名字和我们等待的名字一致
        if (finishedDialogue == prerequisiteDialogueName)
        {
            if (objectToUnlock != null)
            {
                objectToUnlock.SetActive(true);
                Debug.Log($"[{prerequisiteDialogueName}] 剧情完成，已解锁物体：{objectToUnlock.name}！");
            }

            // 解锁后注销该事件以节省性能，防止重复触发
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueFinished -= CompleteCondition;
            }
        }
    }

    private void OnDestroy()
    {
        // 安全起见，从场景销毁时注销事件
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueFinished -= CompleteCondition;
        }
    }
}