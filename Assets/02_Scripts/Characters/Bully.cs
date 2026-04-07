using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bully : MonoBehaviour
{
    // 1. 巡逻，随机方向、动或者不动
    // 2. 与玩家交互（判断距离）

    public GameObject Player;
    public Animator animator;

    public float Hp = 100;  // 敌人生命值
    public float Speed = 3;  // 敌人移动速度
    public float warningDis = 5;  // 警戒距离
    public float attackDis = 2;  // 攻击距离

    private float viewAngle = 120;  // Bully视野范围

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Hp > 0)
        {
            float dis = Vector3.Distance(Player.transform.position, this.transform.position);
            if (dis <= warningDis)
            {
                // Debug.Log("Run after player!");

                Vector3 dir = Player.transform.position - this.transform.position;
                float angle = Vector3.Angle(dir, this.transform.forward);
                if (angle <= viewAngle / 2)  // 玩家在视野范围内
                {
                    // Debug.Log("Discover player!");

                    // transform.LookAt(Player.transform);  // 反应过快，不适合人物
                    Vector3 player = new Vector3(Player.transform.position.x, 0, Player.transform.position.z);
                    Vector3 self = new Vector3(this.transform.position.x, 0, this.transform.position.z);
                    transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(player - self), 0.02f);
                    // 以每次0.02f秒的频率，从起始向量 this.transform.rotation 旋转到目标向量 player - self，（Quaternion.LookRotation 让物体旋转到目标向量），实现丝滑转向

                    if (dis <= attackDis)
                    {
                        // Debug.Log("Attack the player!");
                        PlayerAnimation("IsAttack");  // 攻击时停下
                    }
                    else
                    {
                        AvoidWall();  // 避免撞墙
                        transform.Translate(new Vector3(0, 0, 1) * Speed * Time.deltaTime);  // 非攻击时才行走
                        PlayerAnimation("IsWalking");  // 播放行走动画
                    }
                }
            }
            else
            {
                // 巡逻
                // Debug.Log("Patroling...");
                PatrolUpdate();
            }
        }
        else
        {
            // Debug.Log("Dead.");
            PlayerAnimation("IsDie");  // 播放死亡动画
        }
    }

    [Header("碰撞设置")]
    public float turnSpeed = 5f;          // 转向速度
    public float rayDistance = 2f;        // 射线检测距离
    private bool isTurning = false;
    private void PatrolUpdate()
    {
        // 定时随机改变方向和状态
        RandomEulerAndResetTime();

        // 正在转向时，不移动
        if (isTurning)
            return;

        if (moveState == 0)
        {
            PlayerAnimation("IsWalking");
            AvoidWall();  // 避免撞墙
            transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, 0.02f);  // 丝滑转向
            transform.Translate(Vector3.forward * Speed * Time.deltaTime);  // 沿着丝滑转向后的方向往前走
        }
        else
        {
            PlayerAnimation("IsIdle");
        }
    }
    private void AvoidWall()
    {
        if (Physics.Raycast(transform.position, Vector3.forward, rayDistance))  // 从 Bully坐标 向 前方 发射一条 rayDistance 长度的射线，如果碰到东西就返回true
        {
            transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(GetRandomEulerAngle("y")), 0.02f);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Bullet")
        {
            Hp -= 10;  // 当带有 "Bullet" 标签的物体（在unity里自行添加）碰到this（enemy），就会扣血
            Debug.Log("Enemy is under attacked!");
        }
        else  // 比如撞墙
        {
            // AvoidWall();
            transform.Rotate(0, 45, 0); // 掉头
        }
    }

    /*
    void AvoidWall()
    {
        // 方法1：简单掉头
        // transform.forward = -transform.forward;

        // 方法2：智能计算转向角度（推荐）
        Vector3 avoidDirection = GetBestAvoidDirection();
        targetAngle = Quaternion.LookRotation(avoidDirection).eulerAngles.y;
        isTurning = true;

        // 立即停止当前移动（如果使用NavMeshAgent）
        // NavMeshAgent agent = GetComponent<NavMeshAgent>();
        // if (agent != null) agent.velocity = Vector3.zero;
    }
    */


    private void PlayerAnimation(string aniName)
    {
        ResetAllTriggers(animator);  // 先行清空所有trigger状态（否则会有bug）
        animator.SetTrigger(aniName);  // 播放目标动画
    }


    # region 随机巡逻

    private float lastTime = 0;  // 上一次时间
    private float resetTime = 4;  // 间隔时间
    private int moveState = 1;  // 1 表示Idle；0 表示Walk
    private Quaternion targetRotation;

    private void RandomEulerAndResetTime()
    {
        if(Time.time - lastTime > resetTime)
        {
            lastTime = Time.time;  // 上一次时间更新为当前时间

            resetTime = UnityEngine.Random.Range(1f, 5f);  // 间隔时间随机
            moveState = UnityEngine.Random.Range(0, 2);  // 运动状态随机
            targetRotation = Quaternion.Euler(GetRandomEulerAngle("y"));
        }
    }

    // 随机旋转
    private Vector3 GetRandomEulerAngle(string axis, int step = 45)  // 这里step表示转动角度步幅
    {
        float x = 0, y = 0, z = 0;

        int r = 360 / step;
        if (axis.Equals("x"))
        {
            x = UnityEngine.Random.Range(1, r) * step;  // 意思是每次都以step的倍数转向（因转动角度太小会看不出来）
        }
        if (axis.Equals("y"))
        {
            y = UnityEngine.Random.Range(1, r) * step;
        }
        if (axis.Equals("z"))
        {
            z = UnityEngine.Random.Range(1, r) * step;
        }

        return new Vector3(x, y, z);
    }

    //随即旋转
    //private Vector3 GetRandomEuler(string axis, int step = 45)  // 转向的步幅
    //{
    //    float x = 0, y = 0, z = 0;

    //    if(axis.Equals("x"))
    //    {

    //    }
    //    if(axis.Equals("y"))
    //    {

    //    }
    //}

    # endregion


    // 清除所有的激活中的trigger缓存
    public void ResetAllTriggers(Animator animator)
    {
        AnimatorControllerParameter[] aps = animator.parameters;
        for (int i = 0; i < aps.Length; i++)
        {
            AnimatorControllerParameter paramItem = aps[i];
            if (paramItem.type == AnimatorControllerParameterType.Trigger)
            {
                string triggerName = paramItem.name;
                bool isActive = animator.GetBool(triggerName);
                if (isActive)
                {
                    animator.ResetTrigger(triggerName);
                }
            }
        }
    }
}