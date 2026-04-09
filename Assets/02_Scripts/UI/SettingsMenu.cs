using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [Header("核心配置")]
    [Tooltip("记得挂载你的 AudioMixer 混音器")]
    public AudioMixer audioMixer;
    [Tooltip("主菜单的场景名称，用于判断当前场景和返回主菜单")]
    public string mainMenuSceneName = "MainMenu";

    [Header("UI 容器与组件引用")]
    [Tooltip("设置面板的本体(放在里面的背景、滑动条等)。这里通过代码控制它的显示/隐藏。")]
    public GameObject settingsPanel; 

    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;
    [Tooltip("返回主菜单按钮")]
    public Button returnToMainMenuButton;
    [Tooltip("返回游戏按钮 (关闭设置面板)")]
    public Button returnToGameButton;

    // 是否位于主菜单（自动判定）
    private bool isInMainMenu;

    private void Start()
    {
        // 自动判定当前是不是在主菜单场景
        isInMainMenu = SceneManager.GetActiveScene().name == mainMenuSceneName;

        // 自动绑定事件，不用在外面一个个分配 Method 了
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (returnToMainMenuButton != null) returnToMainMenuButton.onClick.AddListener(GoToMainMenu);
        if (returnToGameButton != null) returnToGameButton.onClick.AddListener(CloseSettingsMenu); // 返回游戏即关闭设置

        // ==== 核心视觉逻辑判断 ====
        // 1. 根据当前是否在主菜单，决定这两个按钮的显示状态
        if (returnToMainMenuButton != null)
        {
            // 在主菜单时，不需要“返回主菜单”，隐藏它
            returnToMainMenuButton.gameObject.SetActive(!isInMainMenu);
        }
        if (returnToGameButton != null)
        {
            // 无论是在主菜单还是游戏里，这个按钮都可以作为“关闭设置/返回”的按钮，所以保持显示！
            returnToGameButton.gameObject.SetActive(true);
        }

        // 2. 如果是在游戏里，加载出来时必须隐藏面板（等玩家按 ESC 才呼出）。
        if (!isInMainMenu && settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 初始化读取保存的设置
        LoadSettings();
    }

    private void Update()
    {
        // === ESC 监听逻辑 ===
        // 仅仅在游戏场景（而非主菜单）中，才允许按下 Escape 呼出和关闭菜单
        if (!isInMainMenu && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsPanel();
        }
    }

    // ================= 面板显示逻辑 (游戏内ESC调用) =================
    private void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;

        bool isOpening = !settingsPanel.activeSelf;
        settingsPanel.SetActive(isOpening);

        if (isOpening)
        {
            // 呼出设置时：暂停时间，放出鼠标以供点击
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // 关闭设置时：恢复时间，锁定鼠标回到游戏
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // ================= 给主菜单暴露的点击事件 =================
    // 主菜单的 "点击设置" 按钮可以拖拽这个事件，或者继续用你原本的 mainmenumanager。只要能调出面板就行。
    public void OpenSettingsMenu()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    // 设置界面上的 "X 关闭按钮" 拖拽这个事件
    public void CloseSettingsMenu()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // ================= 声音设置 =================
    public void SetMasterVolume(float volume)
    {
        if (volume <= 0.0001f) volume = 0.0001f; // 防止Log10无穷大报错
        if (audioMixer != null) audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetBGMVolume(float volume)
    {
        if (volume <= 0.0001f) volume = 0.0001f;
        if (audioMixer != null) audioMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (volume <= 0.0001f) volume = 0.0001f;
        if (audioMixer != null) audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    // ================= 画面设置 =================
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    // ================= 导航设置 (回到主菜单) =================
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 

        if (!isInMainMenu)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // ================= 读取存档 =================
    private void LoadSettings()
    {
        bool isFull = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFull;
        Screen.fullScreen = isFull;

        float mVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (masterSlider != null) masterSlider.value = mVol;
        SetMasterVolume(mVol);

        float bVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        if (bgmSlider != null) bgmSlider.value = bVol;
        SetBGMVolume(bVol);

        float sVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (sfxSlider != null) sfxSlider.value = sVol;
        SetSFXVolume(sVol);
    }
}
