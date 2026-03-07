using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************
//创建人： Trigger 
//功能说明：门脚本（门的开关） - 已添加按F交互功能
//*****************************************
public class DoorController : MonoBehaviour
{
    public float openAngle = 90f;   // 开门的角度（欧拉角y值） 
    public float closeAngle = 0f;   // 关门的角度 (修复了拼写错误: closeAnlge -> closeAngle)
    public float smoothSpeed = 2f;  // 门开关运动的平滑速度

    private Quaternion openRotation;  // 开门的角度（四元数）
    private Quaternion closeRotation; // 关门的角度
    public bool isOpen;               // 门当前开关状态

    private SphereCollider sc;
    private MeshCollider mc;
    private AudioSource audioSource;

    private AudioClip openClip;  // 为保持命名一致性，修改为 openClip
    private AudioClip closeClip;

    private bool isClosing = false; // 标记门是否正在执行关闭动作，替代原有的 playCloseSound

    // 判断玩家是否在门附近的标志位
    private bool canInteract = false;

void Start()
{
    openRotation = Quaternion.Euler(0, openAngle, 0);
    closeRotation = Quaternion.Euler(0, closeAngle, 0);

    // 动态添加碰撞器并设置为触发器
    sc = gameObject.AddComponent<SphereCollider>();
    sc.isTrigger = true;
    gameObject.tag = "Door";

    // 将触发半径稍微改大一点为2，方便玩家站在门前按F
    if (sc.radius <= 1f)
    {
        sc.radius = 2f;
    }

    mc = GetComponentInChildren<MeshCollider>();
    audioSource = gameObject.AddComponent<AudioSource>();

    // 加载音效
    openClip = Resources.Load<AudioClip>("AudioClips/OpenDoor");
    closeClip = Resources.Load<AudioClip>("AudioClips/CloseDoor");

    // 提示信息：如果音效没有正确放在 Resources 文件夹下，给出警告
    if (openClip == null || closeClip == null)
    {
        Debug.LogWarning("【DoorController】音效加载失败！请检查项目中是否存在路径：Resources/AudioClips/OpenDoor 或 CloseDoor");
    }
}

void Update()
{
    // 如果玩家在触发范围内，并且按下了 F 键，就调用开/关门方法
    if (canInteract && Input.GetKeyDown(KeyCode.F))
    {
        ToggleDoor();
    }

    // 门的状态插值运算
    if (isOpen)
    {
        // 开门动画：当前值跟目标值之间的差值，每次旋转一点，最终旋转到目标值
        transform.localRotation = Quaternion.Slerp(transform.localRotation, openRotation, smoothSpeed * Time.deltaTime);
    }
    else
    {
        // 关门动画：关门速度设为原来的5倍
        transform.localRotation = Quaternion.Slerp(transform.localRotation, closeRotation, smoothSpeed * 5f * Time.deltaTime);

        // 关门音效逻辑：当处于正在关门状态，且角度差极小时，播放关门音效
        if (isClosing && Quaternion.Angle(transform.localRotation, closeRotation) < 0.1f)
        {
            if (closeClip != null)
            {
                audioSource.PlayOneShot(closeClip);
            }
            isClosing = false; // 播放完毕后重置状态
        }
    }
}

/// <summary>
/// 切换门的状态（开或者关）
/// </summary>
public void ToggleDoor()
{
    isOpen = !isOpen;

    // 如果有 MeshCollider，控制其开关。
    // 注意：开门瞬间关闭碰撞器可能会导致玩家在门摆动时穿模。如果不希望穿模，可以保留碰撞器开启。
    if (mc != null)
    {
        mc.enabled = !isOpen;
    }

    if (isOpen)
    {
        if (openClip != null)
        {
            audioSource.PlayOneShot(openClip);
        }
        isClosing = false; // 如果在关门途中突然开门，打断关门状态
    }
    else
    {
        isClosing = true;  // 标记开始关门，准备在 Update 中判定播放音效
    }
}

// 当有物体进入这个门的球形触发器时执行
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        canInteract = true;
    }
}

// 当玩家离开这个门的球形触发器范围时执行
private void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Player"))
    {
        canInteract = false;
    }
}
}