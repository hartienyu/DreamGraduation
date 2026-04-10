using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class XiaDieBehavior : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 10f;
    public float gravity = -9.81f;

    // 转身相关变量
    public float lookAtDuration = 2f;
    private Quaternion originalRotation;
    private bool isLookingAtPlayer = false;
    private Coroutine lookAtCoroutine = null;

    private Animator animator;
    private CharacterController controller;
    private Vector3 velocity;
    private Transform playerTransform;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        // 查找玩家（假设玩家有 "Player" tag）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // 保存初始旋转
        originalRotation = transform.rotation;

        // 订阅对话动作事件（需要找到 DialogueManager 实例）
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueMotion += OnDialogueMotion;
        }
    }

    void OnDestroy()
    {
        // 取消订阅
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueMotion -= OnDialogueMotion;
        }
    }

    void Update()
    {
        // 如果正在转头看玩家，禁止移动控制
        if (isLookingAtPlayer)
            return;

        // 1. 严格只监听键盘的 上下左右 箭头键 (完全排除 WASD)
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.J)) horizontal = -1f;
        if (Input.GetKey(KeyCode.L)) horizontal = 1f;
        if (Input.GetKey(KeyCode.I)) vertical = 1f;
        if (Input.GetKey(KeyCode.K)) vertical = -1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // 2. 移动与转身逻辑
        if (direction.magnitude >= 0.1f)
        {
            // 平滑转身
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // 移动
            controller.Move(direction * moveSpeed * Time.deltaTime);

            // 播放走路动画
            if (animator != null) animator.SetBool("isWalking", true);
        }
        else
        {
            // 停下时切回待机动画
            if (animator != null) animator.SetBool("isWalking", false);
        }

        // 3. 应用重力
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }


    //响应对话中的动作事件
    private void OnDialogueMotion(string speaker)
    {
        // 如果是花火说话时触发的动作
        if (speaker == "花火" || speaker == "Huohua")
        {
            LookAtPlayer();
        }
    }

    //转身看向玩家
    public void LookAtPlayer()
    {
        if (lookAtCoroutine != null)
            StopCoroutine(lookAtCoroutine);

        lookAtCoroutine = StartCoroutine(LookAtPlayerCoroutine());
    }

    private IEnumerator LookAtPlayerCoroutine()
    {
        isLookingAtPlayer = true;

        // 保存当前旋转
        Quaternion startRotation = transform.rotation;

        // 计算朝向玩家的方向
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // 平滑转头
            float elapsedTime = 0;
            while (elapsedTime < 0.3f) // 转头动画时间
            {
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / 0.3f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.rotation = targetRotation;

            // 保持看向玩家一段时间
            yield return new WaitForSeconds(lookAtDuration);

            // 转回原始方向
            elapsedTime = 0;
            Quaternion currentRotation = transform.rotation;
            while (elapsedTime < 0.3f)
            {
                transform.rotation = Quaternion.Slerp(currentRotation, originalRotation, elapsedTime / 0.3f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.rotation = originalRotation;
        }

        isLookingAtPlayer = false;
        lookAtCoroutine = null;
    }
}