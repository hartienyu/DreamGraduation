using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class HuaHuoBehavior : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 10f;
    public float gravity = -9.81f;

    private Animator animator;
    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. 严格只监听键盘的 上下左右 箭头键 (完全排除 WASD)
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) vertical = -1f;

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
}