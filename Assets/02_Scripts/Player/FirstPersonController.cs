using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//*****************************************
//创建人： Trigger 
//功能说明：第一人称控制器
//***************************************** 
public class FirstPersonController : MonoBehaviour
{
    private CharacterController controller;//角色控制器组件
    public float moveSpeed = 5;//移动速度
    public float mouseSensitivity = 100;//鼠标灵敏度
    private float verticalRotation;//垂直轴向旋转角度记录
    private float horizontalRotation;//水平轴向旋转角度记录
    public GameObject flashLightGo;//手电筒灯光的游戏物体引用
    private bool usingFlashLight;//是否打开手电筒 状态
    private float gravity = -9.8f;//重力
    private DoorController dc;//当前进入到具体哪个门的开关范围了（也就是你想开哪个门）
    public Transform caughtTrans;//玩家被抓住时鬼应该在的位置
    private bool beCaught;//玩家是否被抓住
    public bool lockMove;//锁定移动控制
    private WEAPONTYPE weaponType;//玩家当前使用的武器类型
    public GameObject[] weaponGos;//武器游戏物体
    public float attackCD;//使用武器的时间间隔
    private Animator animator;//动画控制器
    private Dictionary<WEAPONTYPE, int> bag = new Dictionary<WEAPONTYPE, int>();//背包里的子弹总数
    private Dictionary<WEAPONTYPE, int> clip = new Dictionary<WEAPONTYPE, int>();//弹夹里的子弹数量
    public int clipMaxSingleShootBullet=5;//点射枪弹夹可以容纳最大子弹数
    public int clipFlashLigtBattery=1;//手电筒需要安装的电池
    public int clipMaxAutoShootBullet=10;//机关枪弹夹可以容纳的最大数量
    private bool isReloading;//是否处于正在填充的状态 填充中不能做其他事
    private float attackTimer;//上一次攻击的时间，计时器
    public Transform attackEffectTrans;//攻击特效位置
    public GameObject attackEffectGo;//攻击特效
    //后坐力效果
    public float lerpSpeed;//速度
    public float moveDistance = -0.2f;//移动距离
    private float elapsedTime;//经过时间
    private bool movingForward;//朝前还是朝后
    private Vector3 initPos;//初始位置
    private Vector3 targetPos;//目标位置
    public Transform handTrans;//手部模型引用
    private bool isPlayingMoveAnimation;//是否正在播放攻击动画
    public AudioSource audioSource;//循环播放音效
    public AudioSource soundAudio;//播放音效
    public AudioClip flashLightSound;//手电筒音效
    public AudioClip singleShootSound;//点射枪音效
    public AudioClip autoShootSound;//自动射击音效
    public AudioClip reloadSound;//填充弹夹音效
    public int HP = 100;
    void Start()
    {
        controller=GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        weaponType = WEAPONTYPE.FLASHLIGHT;
        animator = GetComponentInChildren<Animator>();
        bag.Add(WEAPONTYPE.FLASHLIGHT,1);//背包里有一块电池
        bag.Add(WEAPONTYPE.SINGLESHOT,20);//点射枪子弹总数
        bag.Add(WEAPONTYPE.AUTO,40);//机关枪子弹总数
        clip.Add(WEAPONTYPE.FLASHLIGHT, clipFlashLigtBattery);//弹夹里可以容纳的子弹数量
        clip.Add(WEAPONTYPE.SINGLESHOT, clipMaxSingleShootBullet);//点射枪弹夹子弹容量
        clip.Add(WEAPONTYPE.AUTO, clipMaxAutoShootBullet);//机关枪弹夹子弹容量
        initPos = handTrans.localPosition;
        targetPos = initPos + new Vector3(0,0,moveDistance);
    }

    void Update()
    {
        if (beCaught)
        {
            return;
        }
        PlayerMove();
        PlayerRotate();
        Attack();
        ToggleDoor();
        ChangeWeapon();
        Reload();
        PlayGunAnimation();
    }
    /// <summary>
    /// 玩家移动
    /// </summary>
    private void PlayerMove()
    {
        if (lockMove)//移动锁定之后不能控制移动
        {
            return;
        }
        //玩家的输入控制
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        //移动方向 水平轴值方向叠加垂直轴值方向
        Vector3 moveDirection = transform.right * horizontal
            + transform.forward * vertical;
        if (moveDirection.magnitude>0)//玩家有输入
        {
            animator.SetBool("Move",true);
            if (!audioSource.isPlaying)//没有播放
            {
                audioSource.Play();//播放脚步声
            }
        }
        else
        {
            animator.SetBool("Move", false);
            if (audioSource.isPlaying)//在播放
            {
                audioSource.Stop();//停止播放
            }
        }
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        //上下楼梯浮空，给向下的速度
        controller.Move(Vector3.up*gravity*Time.deltaTime);
    }
    /// <summary>
    /// 玩家转向
    /// </summary>
    private void PlayerRotate()
    {
        //获取鼠标滑动输入 +-1 有时候鼠标特别灵敏时会越过边界值
        float mouseX= Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        //Y轴向上的旋转值 在之前的角度基础上加上水平轴向变化值
        horizontalRotation+=mouseX * Time.deltaTime * mouseSensitivity;
        //X轴向上的旋转值
        verticalRotation -= mouseY * mouseSensitivity * Time.deltaTime;
        transform.eulerAngles = new Vector3(verticalRotation, horizontalRotation, 0);
    }
    /// <summary>
    /// 使用手电筒
    /// </summary>
    private void UseFlashLight()
    {
        if (Input.GetMouseButtonDown(0))//如果按下鼠标左键
        {
            //把手电筒开关打开或者关闭
            usingFlashLight = !usingFlashLight;
            flashLightGo.SetActive(usingFlashLight);
            soundAudio.PlayOneShot(flashLightSound);
        }
    }
    /// <summary>
    /// 进入门范围
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            dc= other.GetComponent<DoorController>();
        }
    }
    /// <summary>
    /// 退出门范围
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            dc = null;
        }
    }
    /// <summary>
    /// 玩家开关门
    /// </summary>
    private void ToggleDoor()
    {
        if (Input.GetKeyDown(KeyCode.Space)&&dc)
        {
            dc.ToggleDoor();
        }
    }
    /// <summary>
    /// 玩家被抓住
    /// </summary>
    /// <returns></returns>
    public Vector3 BeCaught()
    {
        beCaught = true;
        return caughtTrans.position;
    }
    /// <summary>
    /// 切换武器
    /// </summary>
    private void ChangeWeapon()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            //把手电筒开关关闭
            usingFlashLight = false;
            flashLightGo.SetActive(usingFlashLight);
            weaponType++;
            weaponType=(WEAPONTYPE)(((int)weaponType)%3);
            ChangeWeaponGameObject();
            animator.SetTrigger("ChangeWeapon");
            switch (weaponType)
            {
                case WEAPONTYPE.FLASHLIGHT:
                    attackCD = 0;
                    animator.SetBool("UseGun",false);
                    break;
                case WEAPONTYPE.SINGLESHOT:
                    attackCD = 0.2f;
                    animator.SetBool("UseGun", true);
                    break;
                case WEAPONTYPE.AUTO:
                    attackCD = 0.1f;
                    animator.SetBool("UseGun", true);
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// 显示对应的游戏物体
    /// </summary>
    private void ChangeWeaponGameObject()
    {
        for (int i = 0; i < weaponGos.Length; i++)
        {
            weaponGos[i].SetActive(false);
        }
        weaponGos[(int)weaponType].SetActive(true);
    }
    /// <summary>
    /// 换子弹
    /// </summary>
    private void Reload()
    {
        if (!Input.GetKeyDown(KeyCode.R))
        {
            return;
        }
        bool canReload = false;//是否可以往弹夹里装填子弹 默认不可以
        switch (weaponType)
        {
            case WEAPONTYPE.FLASHLIGHT:
                if (clip[weaponType]<clipFlashLigtBattery)//手电筒里没放电池
                {
                    canReload = true;
                }
                break;
            case WEAPONTYPE.SINGLESHOT:
                if (clip[weaponType]<clipMaxSingleShootBullet)//点射枪弹夹里子弹没满
                {
                    canReload = true;
                }
                break;
            case WEAPONTYPE.AUTO:
                if (clip[weaponType]<clipMaxAutoShootBullet)
                {
                    canReload = true;
                }
                break;
            default:
                break;
        }
        if (canReload)//经过判断可以填充
        {
            soundAudio.PlayOneShot(reloadSound);
            if (bag[weaponType]>0)//背包里有子弹，可以填充
            {
                isReloading = true;
                Invoke("RecoverAttackState",3);
                animator.SetTrigger("Reload");
                switch (weaponType)
                {
                    case WEAPONTYPE.FLASHLIGHT:
                        if (bag[weaponType] >= clipFlashLigtBattery)//背包里的剩余子弹是足够填充满弹夹的
                        {
                            if (clip[weaponType] > 0)//如果弹夹里有剩余，补满
                            {
                                //具体需要补几发
                                int addNum = clipFlashLigtBattery - clip[weaponType];
                                bag[weaponType] -= addNum;
                                clip[weaponType] += addNum;
                            }
                            else//弹夹没剩余，则需要装入最大数量
                            {
                                bag[weaponType] -= clipFlashLigtBattery;
                                clip[weaponType] += clipFlashLigtBattery;
                            }
                        }
                        else//背包里的剩余子弹是不够填满子弹夹的，那么就把剩下的都填进去
                        {
                            clip[weaponType] += bag[weaponType];
                            bag[weaponType] = 0;
                        }
                        break;
                    case WEAPONTYPE.SINGLESHOT:
                        if (bag[weaponType]>=clipMaxSingleShootBullet)//背包里的剩余子弹是足够填充满弹夹的
                        {
                            if (clip[weaponType]>0)//如果弹夹里有剩余，补满
                            {
                                //具体需要补几发
                                int addNum= clipMaxSingleShootBullet - clip[weaponType];
                                bag[weaponType] -= addNum;
                                clip[weaponType] += addNum;
                            }
                            else//弹夹没剩余，则需要装入最大数量
                            {
                                bag[weaponType] -= clipMaxSingleShootBullet;
                                clip[weaponType] += clipMaxSingleShootBullet;
                            }
                        }
                        else//背包里的剩余子弹是不够填满子弹夹的，那么就把剩下的都填进去
                        {
                            clip[weaponType] += bag[weaponType];
                            bag[weaponType] = 0;
                        }
                        break;
                    case WEAPONTYPE.AUTO:
                        if (bag[weaponType] >= clipMaxAutoShootBullet)//背包里的剩余子弹是足够填充满弹夹的
                        {
                            if (clip[weaponType] > 0)//如果弹夹里有剩余，补满
                            {
                                //具体需要补几发
                                int addNum = clipMaxAutoShootBullet - clip[weaponType];
                                bag[weaponType] -= addNum;
                                clip[weaponType] += addNum;
                            }
                            else//弹夹没剩余，则需要装入最大数量
                            {
                                bag[weaponType] -= clipMaxAutoShootBullet;
                                clip[weaponType] += clipMaxAutoShootBullet;
                            }
                        }
                        else//背包里的剩余子弹是不够填满子弹夹的，那么就把剩下的都填进去
                        {
                            clip[weaponType] += bag[weaponType];
                            bag[weaponType] = 0;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
    /// <summary>
    /// 恢复攻击状态，即转换为非填充状态
    /// </summary>
    private void RecoverAttackState()
    {
        isReloading = false;
    }
    /// <summary>
    /// 攻击
    /// </summary>
    private void Attack()
    {
        if (!isReloading)
        {
            switch (weaponType)
            {
                case WEAPONTYPE.FLASHLIGHT:
                    UseFlashLight();
                    break;
                case WEAPONTYPE.SINGLESHOT:
                    SingleShootAttack();
                    break;
                case WEAPONTYPE.AUTO:
                    AutoShootAtttack();
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// 点射枪攻击
    /// </summary>
    private void SingleShootAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GunAttack();
        }
    }
    /// <summary>
    /// 机关枪攻击
    /// </summary>
    private void AutoShootAtttack()
    {
        if (Input.GetMouseButton(0))
        {
            GunAttack();
        }
    }
    /// <summary>
    /// 枪的攻击
    /// </summary>
    private void GunAttack()
    {
        if (Time.time - attackTimer >= attackCD)
        {
            if (clip[weaponType] > 0)//弹夹有子弹，可以攻击
            {
                //子弹减少             
                clip[weaponType]--;
                //记录当前攻击时间
                attackTimer = Time.time;
                //生成闪光特效
                GameObject go = Instantiate(attackEffectGo, attackEffectTrans);
                go.transform.localPosition = Vector3.zero;
                //开始播放后坐力动画效果
                isPlayingMoveAnimation = true;
                //音效播放
                if (weaponType==WEAPONTYPE.SINGLESHOT)
                {
                    soundAudio.PlayOneShot(singleShootSound);
                }
                else
                {
                    soundAudio.PlayOneShot(autoShootSound);
                }
                //具体检测射击到鬼
                RaycastHit hit;
                if (Physics.Raycast(attackEffectTrans.position, attackEffectTrans.forward, out hit, 5))
                {
                    if (hit.collider.tag=="Enemy")
                    {
                        hit.collider.GetComponent<Enemy>().TakeDamage(4);
                    }
                }
            }
            else//没有子弹，自动填充
            {
                Reload();
            }
        }       
    }
    /// <summary>
    /// 播放枪攻击动画
    /// </summary>
    private void PlayGunAnimation()
    {
        if (isPlayingMoveAnimation)
        {
            if (movingForward)//朝前移动
            {
                MoveGun(initPos);
            }
            else
            {
                MoveGun(targetPos);
            }
        }
    }
    /// <summary>
    /// 移动枪的动画
    /// </summary>
    private void MoveGun(Vector3 target)
    {
        //计算一下每次插值的位置，把位置更新给手部模型
        handTrans.localPosition = Vector3.Lerp(handTrans.localPosition,target,elapsedTime/(attackCD/2));
        elapsedTime += Time.deltaTime;
        if (Vector3.Distance(handTrans.localPosition,target)<=0.01)//达到终点
        {
            movingForward = !movingForward;
            elapsedTime = 0;
            if (!movingForward)//如果当前达到位置是起始点
            {
                isPlayingMoveAnimation = false;
            }
        }
    }
    public void TakeDamge()
    {
        HP -= 10;
        if (HP<=0)
        {
            Invoke("Replay", 5);//延时5S重新加载游戏
        }
    }
    /// <summary>
    /// 重玩
    /// </summary>
    private void Replay()
    {
        SceneManager.LoadScene(0);
    }
}
/// <summary>
/// 武器类型
/// </summary>
public enum WEAPONTYPE
{ 
    FLASHLIGHT,//手电筒
    SINGLESHOT,//点射枪
    AUTO//自动射击
}
