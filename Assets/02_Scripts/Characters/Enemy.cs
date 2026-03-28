using UnityEngine;
using UnityEngine.AI;

//*****************************************
//创建人： Trigger 
//功能说明：敌对实体（僵尸/恶灵）AI控制与伤害逻辑
//*****************************************
public class Enemy : MonoBehaviour
{
    private Animator animator;
    public int HP = 20;

    private PlayerHealth playerHealth;
    private Transform playerTransform;

    private NavMeshAgent nma;
    private bool isDead;
    private bool hasTarget;

    public float attackCD = 2f;
    private float attackTimer;

    void Start()
    {
        animator = GetComponent<Animator>();
        nma = GetComponent<NavMeshAgent>();

        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null)
        {
            playerTransform = playerGo.transform;
            playerHealth = playerGo.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (isDead || playerTransform == null) return;

        float sqrDistanceToPlayer = (transform.position - playerTransform.position).sqrMagnitude;

        if (sqrDistanceToPlayer >= 25f)
        {
            return;
        }
        else
        {
            hasTarget = true;
        }

        if (hasTarget)
        {
            if (sqrDistanceToPlayer <= 4f)
            {
                Vector3 targetPos = playerTransform.position;
                transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));
                Attack();
            }
            else
            {
                nma.isStopped = false;
                nma.SetDestination(playerTransform.position);
                animator.SetBool("Move", true);
            }
        }
    }

    public void TakeDamage(int attackValue)
    {
        if (isDead) return;

        HP -= attackValue;

        if (HP <= 0)
        {
            isDead = true;
            animator.SetBool("Die", true);

            nma.isStopped = true;
            nma.enabled = false;
            Collider coll = GetComponent<Collider>();
            if (coll != null) coll.enabled = false;
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }

    private void Attack()
    {
        nma.isStopped = true;
        animator.SetBool("Move", false);

        if (Time.time - attackTimer >= attackCD)
        {
            animator.SetTrigger("Attack");

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
            }

            attackTimer = Time.time;
        }
    }
}