using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance;

    [Header("UI 引用")]
    public TextMeshProUGUI timerText; // 拖入刚才做的 TimerText

    private float currentTime;
    private bool isCountingDown = false;

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
        timerText.gameObject.SetActive(true); // 显示倒计时文本
    }

    // 倒计时结束时执行的逻辑
    private void OnTimerEnded()
    {
        timerText.text = "Time's Up!";
        // 这里可以触发后续事件，比如玩家死亡、强制切换场景等
        Debug.Log("倒计时结束，执行惩罚机制！");
    }
}