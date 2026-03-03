using UnityEngine;
using UnityEngine.SceneManagement; // 必须引入场景管理命名空间

public class MainMenuManager : MonoBehaviour
{
    // 假设你的游戏主场景名字叫 "GameScene"
    public string gameSceneName = "GameScene";

    // 1. 开始游戏
    public void StartGame()
    {
        Debug.Log("开始新游戏！");
        // 加载游戏场景
        SceneManager.LoadScene(gameSceneName);
    }

    // 2. 继续游戏
    public void ContinueGame()
    {
        // 这里的逻辑通常是读取存档数据，然后再加载场景
        // 目前先做个打印演示
        Debug.Log("加载存档并继续游戏！");
        // 读取存档逻辑...
        // SceneManager.LoadScene(gameSceneName);
    }

    // 3. 设置界面
    public void OpenSettings()
    {
        // 通常这里会激活一个包含各种设置（音量、画质）的 UI Panel
        Debug.Log("打开设置面板！");
    }

    // 4. 退出游戏
    public void QuitGame()
    {
        Debug.Log("退出游戏！");

        // 注意：Application.Quit 在 Unity 编辑器里运行是没效果的，只有打包出游戏后才有效
        Application.Quit();

        // 加上这句可以让在编辑器测试时也能停止运行
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}