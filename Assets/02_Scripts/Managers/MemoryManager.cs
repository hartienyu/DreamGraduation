using UnityEngine;
using System;

public class MemoryManager : MonoBehaviour
{
    public static MemoryManager Instance;

    [Header("回忆文字内容")]
    [TextArea(10, 20)]
    public string memoryText1 = "她们把我绑在厕所的隔间里。\n透过门缝，我看见小白被拖到洗手台前。领头的那个人抓着她的头发，把她的头按进了装满水的洗手盆里。\n一下。\n小白的手在乱挥。\n两下。\n水花溅到了我的鞋上。\n三下。\n气泡从水底冒上来，一串一串的。\n我想喊住手。我想冲出去。可是我的嘴张不开，我的腿动不了。我整个人像被钉在了地上，什么都做不了。\n她们松开手的时候，小白已经不动了。\n她们慌了，跑了。\n我从隔间里爬出来，把小白从水里拉起来。她的脸是湿的，凉的，眼睛半睁着，嘴角带着一点笑意。\n我叫她的名字。一直叫。她再也没有回答。\n后来所有人都说是意外。没有人受罚。\n我从那天起再也没有交过朋友。";
    
    [TextArea(10, 25)]
    public string memoryText2 = "我站在旧校舍的女厕里。\n水龙头开着，水从洗手盆里溢出来，淌了一地。\n她们把小白的头按进水里。一下，两下，三下。小白在挣扎，水花溅到我脸上。我想喊住手，可是我的嘴张不开。我想冲过去，可是我的腿动不了。\n我站在那里，什么都没有做。\n等她们终于松开手，小白已经不动了。她的眼睛半睁着，嘴角带着一点笑意，像是看见了什么很好的东西。\n后来所有人都说是意外。没有人受罚。我转学了，再也没有提起过那天的事。\n十九岁那年我离开了家。继母不让我住下去，父亲没有说话。我去了陌生的城市，一个人活到现在。我换过很多工作，因为每个地方的人都让我害怕。她们一皱眉我就觉得要被打，她们一靠近我就想蹲下去抱住头。\n我以为只要不回想，事情就会过去。\n可是每天夜里我都回到那间厕所。水龙头永远开着，水永远在流，好似小白的眼永远在水里看着我。\n我终于明白了。\n我在这里建了这座旧校舍，不是因为我想逃。\n是因为我想回去。\n回到那天，回到那个洗手盆前面，抓住小白的手，带她跑出去。\n哪怕只有一次。\n哪怕只是在梦里。";

    [Header("记忆解锁状态")]
    public bool isMemory1Unlocked = false;
    public bool isMemory2Unlocked = false;

    // 事件：通知UI系统弹出对应的记忆碎片片段
    public event Action<string> OnMemoryUnlocked;
    // 事件：通知结局触发
    public event Action OnTrueEndingTriggered;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 完成羁绊任务1时触发
    /// </summary>
    public void UnlockMemory1_Task1Completed()
    {
        if (isMemory1Unlocked) return;
        isMemory1Unlocked = true;
        Debug.Log("【记忆系统】羁绊任务1完成！解锁记忆碎片1。");
        OnMemoryUnlocked?.Invoke(memoryText1);
    }

    /// <summary>
    /// 完成羁绊任务2时触发，并触发真·结局
    /// </summary>
    public void UnlockMemory2_Task2Completed()
    {
        if (isMemory2Unlocked) return;
        isMemory2Unlocked = true;
        Debug.Log("【记忆系统】羁绊任务2完成！解锁完整记忆碎片2。");
        OnMemoryUnlocked?.Invoke(memoryText2);
        
        // 判定真结局：回忆全都凑齐了，进入救出小白的真结局
        TriggerTrueEnding();
    }

    private void TriggerTrueEnding()
    {
        Debug.Log("【结局系统】记忆完全解锁，直面恐惧。触发救赎真结局！");
        OnTrueEndingTriggered?.Invoke();
        // 在这里可以接入过场动画、白屏、回到主菜单等结算逻辑
    }
}