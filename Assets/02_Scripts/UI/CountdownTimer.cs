using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance;

    [Header("UI 引用")]
    public TextMeshProUGUI timerText;

    [Header("霸凌者对象绑定")]
    [Tooltip("拖入场景中事先放好并隐藏的 Wukong/Bully 对象")]
    public GameObject bullyObject;

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
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CountdownTimer] TimerText 未绑定！请在 Inspector 中拖入 TextMeshProUGUI 组件。");
        }

        // 如果配置了霸凌者，初始阶段它应该是关闭的
        if (bullyObject != null)
        {
            bullyObject.SetActive(false);
        }

        // 读取存档中的倒计时状态
        LoadTimerFromSave();
    }

    private void LoadTimerFromSave()
    {
        if (ProgressManager.Instance != null && ProgressManager.Instance.GetCurrentSaveData() != null)
        {
            GameSaveData saveData = ProgressManager.Instance.GetCurrentSaveData();
            if (saveData.remainingTimer > 0)
            {
                currentTime = saveData.remainingTimer;
                hasTriggered5MinSplash = saveData.hasTriggered5MinSplash;
                isCountingDown = true;

                if (timerText != null) timerText.gameObject.SetActive(true);

                // 如果已经过了触发时间，唤醒霸凌者
                if (hasTriggered5MinSplash || currentTime <= 600f) 
                {
                    ActivateBully();
                    hasTriggered5MinSplash = true; // 确保布尔值正确同步
                }
            }
        }
    }

    // 可以在外部或保存系统调用此方法同步
    public void ForceSaveTimer()
    {
        if (ProgressManager.Instance != null && ProgressManager.Instance.GetCurrentSaveData() != null)
        {
            GameSaveData saveData = ProgressManager.Instance.GetCurrentSaveData();
            saveData.remainingTimer = isCountingDown ? currentTime : -1f;
            saveData.hasTriggered5MinSplash = hasTriggered5MinSplash;
        }
    }

    private void Update()
    {
        if (isCountingDown)
        {
            // Time.deltaTime 是上一帧到这一帧经过的时间（秒）
            currentTime -= Time.deltaTime;

            if (currentTime <= 600f && !hasTriggered5MinSplash)
            {
                // 当倒计时小于等于300秒（5分钟），触发一次警告并呼唤霸凌者
                hasTriggered5MinSplash = true;
                if (EventTester.Instance != null)
                {
                    EventTester.Instance.ShowSplash("警告：霸凌者来了！");
                }
                
                // 开启霸凌者(Wukong)对象
                ActivateBully();
            }

            if (currentTime <= 0)
            {
                currentTime = 0;
                isCountingDown = false;
                OnTimerEnded();
            }

            // 更新UI显示，将倒计时格式化为 分:秒
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
                timerText.text = $"倒计时: {minutes:D2}:{seconds:D2}";
            }
        }
    }

    // 开放给外部调用的方法：传入需要倒计时的秒数
    public void StartCountdown(float durationInSeconds)
    {
        currentTime = durationInSeconds; // 这里我们将会传 600f (10分钟)
        isCountingDown = true;
        hasTriggered5MinSplash = false; // 重置此警告旗标
        
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true); // 显示倒计时文本
        }
    }

    // 开放给外部调用的加减时间方法（用于测试或奖惩）
    public void AdjustTime(float seconds)
    {
        if (!isCountingDown) return;
        
        currentTime += seconds;
        
        // 防止时间被减到负数出现逻辑错误
        if (currentTime < 0) currentTime = 0.1f;
        
        // 如果把时间加到了大于10分钟，重置10分钟的警告旗帜，允许再次触发，并关闭霸凌者
        if (currentTime > 600f)
        {
            hasTriggered5MinSplash = false;
        }

        Debug.Log($"倒计时时间已调整，增减 {seconds} 秒，当前剩余 {currentTime} 秒。");
    }

    private void ActivateBully()
    {
        if (bullyObject != null)
        {
            bullyObject.SetActive(true);
            Debug.Log("[Bully Spawned!] 时间剩余 5 分钟，霸凌者 (Wukong) 已出动！");
        }
        else
        {
            // 通过查找Tag作为兜底
            GameObject bullyByTag = GameObject.FindGameObjectWithTag("Bully");
            if (bullyByTag != null)
            {
                bullyObject = bullyByTag;
                bullyObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[CountdownTimer]霸凌者物体未配置且按标签查找失败！请将 Wukong 拖入 CountdownTimer 槽位。");
            }
        }
    }

    // 倒计时结束时执行的逻辑
    private void OnTimerEnded()
    {
        if (timerText != null)
        {
            timerText.text = "Time's Up!";
        }
        
        // 当倒计时结束，即时间晚到被霸凌者抓到
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.TriggerGameOver("时间到了...\n未能逃脱！");
        }
        else
        {
            Debug.LogError("没有挂载 GameOverUI 对象，无法弹出失败画面。");
        }
        
        Debug.Log("倒计时结束，执行惩罚机制！");
    }
}