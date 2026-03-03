using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//*****************************************
//创建人： Trigger 
//功能说明：僵尸
//***************************************** 
public class Enemy : MonoBehaviour
{
    private Animator animator;
    public int HP = 20;
    private FirstPersonController fpc;
    private NavMeshAgent nma;
    private bool isDead;
    private bool hasTarget;
    private float attackCD=2;
    private float attackTimer;

    void Start()
    {
        animator = GetComponent<Animator>();
        fpc = GameObject.Find("Player").GetComponent<FirstPersonController>();
        nma = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        //死亡
        if (isDead)
        {
            return;
        }
        //视野范围外
        if (Vector3.Distance(transform.position,fpc.transform.position)>=5)
        {
            return;
        }
        else
        {
            hasTarget = true;
        }
        //有目标后开始攻击
        if (hasTarget)
        {
            //到达攻击范围
            if (Vector3.Distance(transform.position,fpc.transform.position)<=2)
            {
                Vector3 targetPos = fpc.transform.position;
                transform.LookAt(new Vector3(targetPos.x,transform.position.y,targetPos.z));
                Attack();
            }
            //移动
            else
            {
                nma.isStopped = false;
                nma.SetDestination(fpc.transform.position);
                animator.SetBool("Move",true);
            }
        }
    }
    /// <summary>
    /// 僵尸受到伤害
    /// </summary>
    /// <param name="attackValue">伤害值</param>
    public void TakeDamage(int attackValue)
    {
        HP -= attackValue;
        animator.SetTrigger("Hit");
        if (HP<=0)
        {
            isDead = true;
            animator.SetBool("Die",true);
        }
    }
    /// <summary>
    /// 攻击
    /// </summary>
    private void Attack()
    {
        nma.isStopped = true;
        animator.SetBool("Move", false);
        if (Time.time-attackTimer>=attackCD)
        {
            animator.SetTrigger("Attack");
            fpc.TakeDamge();
            attackTimer = Time.time;
        }
       
       
    }
}
