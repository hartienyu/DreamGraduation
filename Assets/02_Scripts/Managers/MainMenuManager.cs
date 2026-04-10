using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // 必须引入 EventSystems 才能控制焦点取消

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "GameScene";

    [Header("UI 面板引用")]
    public GameObject titlePanel;
    public GameObject buttonPanel;
    public GameObject settingsPanel;
    public GameObject loadingPanel;
    public GameObject quitConfirmPanel;

    [Header("加载进度组件")]
    public Slider loadingSlider;
    public TMP_Text progressText;

    [Header("键盘导航设置")]
    [Tooltip("按从上到下的顺序拖入主界面的按钮（开始、继续、设置、退出）")]
    public Button[] mainButtons;
    private int currentButtonIndex = 0;

    [Header("系统引用")]
    public AudioManager audioManager; // 确保这里是你的 UIAudioManager

    private bool isTransitioning = false; // 新增：是否正在切换场景的安全锁

    void Start()
    {
        // 初始状态清理
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);

        if (titlePanel != null) titlePanel.SetActive(true);
        if (buttonPanel != null) buttonPanel.SetActive(true);

        // ========== 新增：如果有存档记录，点亮“继续游戏”按钮 ==========
        if (mainButtons.Length > 1) 
        {
            if (ProgressManager.Instance.HasSaveGame())
            {
                mainButtons[1].interactable = true; // 假设列表第2个(Index:1)是Continue
            }
            else
            {
                mainButtons[1].interactable = false;
            }
        }

        // 初始自动选中第一个“可点击”的按钮
        SelectFirstInteractableButton();
    }

    void Update()
    {
        // 如果正在切换场景，彻底屏蔽按键检测以防物体销毁后报空引用
        if (isTransitioning) return;

        // 如果正在加载，屏蔽所有输入
        if (loadingPanel != null && loadingPanel.activeSelf) return;

        HandleEscInput();
        HandleNavigation();
    }

    // ==========================================
    // Esc 键逻辑分配
    // ==========================================
    private void HandleEscInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (quitConfirmPanel.activeSelf)
            {
                audioManager.PlaySFX(audioManager.errorSound); // 提示必须做选择
            }
            else
            {
                OnClickQuitButton();
            }
        }
    }

    // ==========================================
    // 键盘/手柄/滚轮 上下选择逻辑
    // ==========================================
    private void HandleNavigation()
    {
        // 只有主菜单激活且没有任何弹窗时，才允许上下切换
        if (!buttonPanel.activeSelf || settingsPanel.activeSelf || quitConfirmPanel.activeSelf)
            return;

        if (mainButtons == null || mainButtons.Length == 0) return;

        int moveDirection = 0;

        // 向上：按上箭头、W键、或鼠标滚轮向上滚 (y > 0)
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.mouseScrollDelta.y > 0.1f)
        {
            moveDirection = -1;
        }
        // 向下：按下箭头、S键、或鼠标滚轮向下滚 (y < 0)
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || Input.mouseScrollDelta.y < -0.1f)
        {
            moveDirection = 1;
        }

        // 如果玩家触发了移动操作
        if (moveDirection != 0)
        {
            // 寻找下一个可用的按钮
            int nextIndex = FindNextInteractableButton(currentButtonIndex, moveDirection);

            if (nextIndex != -1) // 找到了可用按钮
            {
                SelectButton(nextIndex);
                audioManager.PlaySFX(audioManager.hoverSound);
            }
            else // 没找到（到顶了或者到底了）
            {
                audioManager.PlaySFX(audioManager.errorSound);
            }
        }

        // 回车或空格键确认
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            // 确保当前按钮是可点击的才触发
            if (mainButtons[currentButtonIndex].interactable)
            {
                mainButtons[currentButtonIndex].onClick.Invoke();
            }
            else
            {
                audioManager.PlaySFX(audioManager.errorSound);
            }
        }
    }

    // 核心算法：跳过不可点击的按钮，寻找下一个可用的目标
    private int FindNextInteractableButton(int startIndex, int direction)
    {
        int checkIndex = startIndex + direction;

        // 只要没越界，就一直顺着方向找
        while (checkIndex >= 0 && checkIndex < mainButtons.Length)
        {
            if (mainButtons[checkIndex].interactable)
            {
                return checkIndex; // 找到了，返回下标
            }
            checkIndex += direction; // 没找到，继续往下/上找
        }

        return -1; // 越界了都没找到，返回 -1 表示触顶/触底
    }

    // 选择指定按钮
    private void SelectButton(int index)
    {
        if (mainButtons == null || mainButtons.Length == 0) return;
        currentButtonIndex = index;
        mainButtons[currentButtonIndex].Select();
    }

    // 自动寻找并选中列表中的第一个可用按钮
    private void SelectFirstInteractableButton()
    {
        // 从 -1 开始往下找（方向为1），相当于找列表里的第一个可用按钮
        int firstIndex = FindNextInteractableButton(-1, 1);
        if (firstIndex != -1)
        {
            SelectButton(firstIndex);
        }
    }

    // 取消所有按钮的选中状态（用于进入弹窗时）
    private void ClearSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // ==========================================
    // 按钮功能区 (接入 ProgressManager)
    // ==========================================
    public void StartGame()
    {
        // 调用重置并创建新存档
        ProgressManager.Instance.StartNewGame();
        StartCoroutine(LoadSceneAsync(ProgressManager.Instance.GetCurrentSaveData().sceneName));
    }

    public void ContinueGame()
    {
        // 提取存档，进入保存中的那个场景
        ProgressManager.Instance.ContinueGame();
        StartCoroutine(LoadSceneAsync(ProgressManager.Instance.GetCurrentSaveData().sceneName));
    }

    private IEnumerator LoadSceneAsync(string targetScene)
    {
        isTransitioning = true; // 锁定：标记正在离开当前场景，拦截所有按键 Update

        if (titlePanel != null) titlePanel.SetActive(false);
        if (buttonPanel != null) buttonPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        loadingPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        float displayProgress = 0f;

        while (!operation.isDone)
        {
            float targetProgress = operation.progress / 0.9f;
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, 1.5f * Time.deltaTime);

            if (loadingSlider != null) loadingSlider.value = displayProgress;
            if (progressText != null) progressText.text = "加载中...\t\t\t" + (displayProgress * 100f).ToString("F0") + "%";

            if (operation.progress >= 0.9f && displayProgress >= 1f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        ClearSelection(); // 进入设置，取消主界面按钮的高亮聚焦
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        audioManager.PlaySFX(audioManager.clickSound);
        SelectFirstInteractableButton(); // 返回主界面，默认选中第一个
    }

    public void OnClickQuitButton()
    {
        quitConfirmPanel.SetActive(true);
        ClearSelection(); // 进入确认弹窗，取消主界面按钮的高亮聚焦
    }

    public void ConfirmQuit()
    {
        audioManager.PlaySFX(audioManager.clickSound);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CancelQuit()
    {
        quitConfirmPanel.SetActive(false);
        audioManager.PlaySFX(audioManager.clickSound);
        SelectFirstInteractableButton(); // 取消退出，默认选中第一个
    }
}

