using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//创建人： Trigger 
//功能说明：门脚本（门的开关）
//***************************************** 
public class DoorController : MonoBehaviour
{
    public float openAngle = 90;//开门的角度（欧拉角y值） 
    public float closeAnlge = 0;//关门的角度
    public float smoothSpeed = 2;//门开关运动的平滑速度

    private Quaternion openRotation;//开门的角度（四元数）
    private Quaternion closeRotation;//关门的角度
    public bool isOpen;//门当前开关状态
    private SphereCollider sc;
    private MeshCollider mc;
    private AudioSource audioSource;
    private AudioClip audioClip;
    private AudioClip closeClip;
    private bool playCloseSound;

    void Start()
    {
        openRotation = Quaternion.Euler(0,openAngle,0);
        closeRotation = Quaternion.Euler(0,closeAnlge,0);
        //添加碰撞器并设置为触发器
        sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger=true;
        gameObject.tag = "Door";
        if (sc.radius<=1)
        {
            sc.radius = 1;
        }
        mc = GetComponentInChildren<MeshCollider>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioClip = Resources.Load<AudioClip>("AudioClips/OpenDoor");
        closeClip = Resources.Load<AudioClip>("AudioClips/CloseDoor");
    }

    void Update()
    {
        //如果门当前开门状态并且没有完全打开，执行开门动画
        if (isOpen&&Quaternion.Angle(transform.localRotation,openRotation)>0.01f)
        {
            //当前值跟目标值之间的差值，每次旋转一点，最终旋转到目标值
            transform.localRotation= Quaternion.Slerp(transform.localRotation,
                openRotation,smoothSpeed*Time.deltaTime);
        }
        else if (!isOpen && Quaternion.Angle(transform.localRotation, closeRotation) > 0.01f)
        {
            //当前值跟目标值之间的差值，每次旋转一点，最终旋转到目标值
            transform.localRotation = Quaternion.Slerp(transform.localRotation,
                closeRotation, smoothSpeed*5 * Time.deltaTime);
        }
        if (playCloseSound)
        {
            if (Quaternion.Angle(transform.localRotation, closeRotation) < 0.01f)
            {
                audioSource.PlayOneShot(closeClip);
                playCloseSound = false;
            }

        }
    }
    /// <summary>
    /// 切换门的状态（开或者关）
    /// </summary>
    public void ToggleDoor()
    {
        isOpen = !isOpen;
        mc.enabled = !isOpen;
        if (isOpen)
        {
            audioSource.PlayOneShot(audioClip);
            playCloseSound = true;
        }
    }
}
