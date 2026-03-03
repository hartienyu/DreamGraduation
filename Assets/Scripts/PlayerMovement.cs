using UnityEngine;
using Cinemachine; // 必须引入 Cinemachine 命名空间

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;      // Shift 跑步速度
    public float turnSmoothTime = 0.1f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("冲刺设置")]
    public float dashSpeed = 15f;    // 右键冲刺速度
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.8f;

    [Header("Cinemachine 视角缩放")]
    public CinemachineFreeLook freeLookCamera; // 这里需要在 Unity 面板里拖入你的 CM FreeLook1 物体
    public float zoomSpeed = 20f;
    public float minFOV = 30f;       // 滚轮放大极限
    public float maxFOV = 70f;       // 滚轮缩小极限

    public Animator anim;

    private float turnSmoothVelocity;
    private Vector3 velocity;
    private CharacterController controller;
    private Transform cam;

    private float dashTime;
    private float dashCooldownTimer;
    private bool isDashing;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ========== 1. 鼠标滚轮操控 Cinemachine 视角缩放 ==========
        HandleCinemachineZoom();

        // ========== 重力处理 ==========
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            if (anim != null) anim.SetBool("isJumping", false);
        }

        // ========== 冲刺冷却 ==========
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // ========== 2. 冲刺输入检测 (鼠标右键) ==========
        if (Input.GetMouseButtonDown(1) && dashCooldownTimer <= 0 && !isDashing)
        {
            isDashing = true;
            dashTime = dashDuration;
            dashCooldownTimer = dashCooldown;
            if (anim != null) anim.SetTrigger("Dash");
        }

        // ========== 移动输入获取 ==========
        float horizontal = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float vertical = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // ========== 3. 自由移动与冲刺逻辑 ==========
        if (isDashing)
        {
            // 冲刺时沿角色当前正前方高速移动
            controller.Move(transform.forward * dashSpeed * Time.deltaTime);
            dashTime -= Time.deltaTime;
            if (dashTime <= 0)
            {
                isDashing = false;
            }
        }
        else
        {
            // 跳跃
            if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (anim != null) anim.SetBool("isJumping", true);
            }

            // 检测 Shift 跑步
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentMoveSpeed = isRunning ? runSpeed : walkSpeed;

            // 基于摄像机视角的移动逻辑
            if (direction.magnitude >= 0.1f)
            {
                // 计算目标角度：输入方向的夹角 + 摄像机当前水平旋转角
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                // 平滑转动角色自身
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                // 算出实际移动方向并移动
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * currentMoveSpeed * Time.deltaTime);
            }

            // 动画控制
            if (anim != null)
            {
                float speedPercent = direction.magnitude * (isRunning ? 1f : 0.5f);
                anim.SetFloat("Speed", speedPercent);
            }
        }

        // ========== 应用重力 ==========
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ========== Cinemachine 缩放控制 ==========
    private void HandleCinemachineZoom()
    {
        if (freeLookCamera == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            // 修改 FreeLook 摄像机的视野 (FOV)
            float currentFOV = freeLookCamera.m_Lens.FieldOfView;
            currentFOV -= scroll * zoomSpeed;
            freeLookCamera.m_Lens.FieldOfView = Mathf.Clamp(currentFOV, minFOV, maxFOV);
        }
    }

    // ========== 商店升级接口 ==========
    public void UpgradeSpeed(float walkBonus, float runBonus, float dashBonus)
    {
        walkSpeed += walkBonus;
        runSpeed += runBonus;
        dashSpeed += dashBonus;
        Debug.Log($"移速已永久提升！当前走路:{walkSpeed}, 跑步:{runSpeed}, 冲刺:{dashSpeed}");
    }
}