using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TableController : MonoBehaviour
{
    [Header("需要弹出的UI面板")]
    public GameObject uiPanel;

    // 用来记录玩家是否在交互范围内
    private bool canInteract = false;

    // Start is called before the first frame update
    void Start()
    {
        // 游戏开始时确保UI是关闭的
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 只有当玩家在范围内，并且按下了 F 键时，才触发UI
        if (canInteract && Input.GetKeyDown(KeyCode.F))
        {
            if (uiPanel != null)
            {
                // 切换UI的显示/隐藏状态
                bool isActive = uiPanel.activeSelf;
                uiPanel.SetActive(!isActive);
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
        }
    }


    // 当有物体进入触发器范围时调用
    private void OnTriggerEnter(Collider other)
    {
        // 检查进入范围的是不是玩家
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            Debug.Log("玩家进入了交互范围，可以按F键！");
            // 【可选】你可以在这里显示一个悬浮的“按F交互”的小提示UI
        }
    }


    // 当有物体离开触发器范围时调用
    private void OnTriggerExit(Collider other)
    {
        // 检查离开范围的是不是玩家
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            Debug.Log("玩家离开了交互范围。");

            // 玩家离开时，自动把弹出的UI关掉
            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
            }
        }
    }
}
