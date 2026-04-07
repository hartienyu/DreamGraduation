using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance;

    [Header("UI 引用")]
    public TextMeshProUGUI timerText; // 拖入刚才做的 TimerText

    private float currentTime;
    private bool isCountingDown = false;
    private bool hasTriggered5MinSplash = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 游戏刚开始时，先隐藏倒计时UI
        timerText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isCountingDown)
        {
            // Time.deltaTime 是上一帧到这一帧经过的时间（秒）
            currentTime -= Time.deltaTime;

            if (currentTime <= 300f && !hasTriggered5MinSplash)
            {
                // 当倒计时小于等于300秒（5分钟），触发一次警告
                hasTriggered5MinSplash = true;
                if (EventTester.Instance != null)
                {
                    EventTester.Instance.ShowSplash("警告：他们来了！");
                }
            }

            if (currentTime <= 0)
            {
                currentTime = 0;
                isCountingDown = false;
                OnTimerEnded();
            }

            // 更新UI显示，将倒计时格式化为 分:秒
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = $"倒计时: {minutes:D2}:{seconds:D2}";
        }
    }

    // 开放给外部调用的方法：传入需要倒计时的秒数
    public void StartCountdown(float durationInSeconds)
    {
        currentTime = durationInSeconds; // 这里我们将会传 1200f (20分钟)
        isCountingDown = true;
        hasTriggered5MinSplash = false; // 重置此警告旗标
        timerText.gameObject.SetActive(true); // 显示倒计时文本
    }

    public float GetCurrentTime()  // 外部调用检查倒计时现在剩余时间
    {
        return currentTime;
    }

    // 开放给外部调用的加减时间方法（用于测试或奖惩）
    public void AdjustTime(float seconds)
    {
        if (!isCountingDown) return;
        
        currentTime += seconds;
        
        // 防止时间被减到负数出现逻辑错误
        if (currentTime < 0) currentTime = 0.1f;
        
        // 如果把时间加到了大于5分钟，重置5分钟的警告旗帜，允许再次触发
        if (currentTime > 300f)
        {
            hasTriggered5MinSplash = false;
        }

        Debug.Log($"倒计时时间已调整，增减 {seconds} 秒，当前剩余 {currentTime} 秒。");
    }

    // 倒计时结束时执行的逻辑
    private void OnTimerEnded()
    {
        timerText.text = "Time's Up!";
        // 这里可以触发后续事件，比如玩家死亡、强制切换场景等
        Debug.Log("倒计时结束，执行惩罚机制！");
    }
}