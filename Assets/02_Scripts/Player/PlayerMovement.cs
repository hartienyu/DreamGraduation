using UnityEngine;
using Cinemachine;

//*****************************************
//创建人： Trigger 
//功能说明：玩家移动、跳跃、冲刺与Cinemachine视角控制
//*****************************************
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("状态控制")]
    public bool canMove = true;

    [Header("移动设置")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float turnSmoothTime = 0.1f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("冲刺设置")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.8f;

    [Header("Cinemachine 视角")]
    public CinemachineFreeLook freeLookCamera;
    public float zoomSpeed = 20f;
    public float minFOV = 30f;
    public float maxFOV = 70f;

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
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (!canMove)
        {
            isDashing = false;
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
                anim.SetBool("isJumping", false);
            }

            if (controller.isGrounded && velocity.y < 0)
                velocity.y = -2f;

            velocity.y += gravity * Time.deltaTime;
            controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
            return;
        }

        HandleCinemachineZoom();

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            if (anim != null) anim.SetBool("isJumping", false);
        }

        if (Input.GetMouseButtonDown(1) && dashCooldownTimer <= 0 && !isDashing)
        {
            isDashing = true;
            dashTime = dashDuration;
            dashCooldownTimer = dashCooldown;
            if (anim != null) anim.SetTrigger("Dash");
        }

        float horizontal = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float vertical = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (isDashing)
        {
            controller.Move(transform.forward * dashSpeed * Time.deltaTime);
            dashTime -= Time.deltaTime;
            if (dashTime <= 0) isDashing = false;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (anim != null) anim.SetBool("isJumping", true);
            }

            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentMoveSpeed = isRunning ? runSpeed : walkSpeed;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * currentMoveSpeed * Time.deltaTime);
            }

            if (anim != null)
            {
                float speedPercent = direction.magnitude * (isRunning ? 1f : 0.5f);
                anim.SetFloat("Speed", speedPercent);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCinemachineZoom()
    {
        if (freeLookCamera == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float currentFOV = freeLookCamera.m_Lens.FieldOfView;
            currentFOV -= scroll * zoomSpeed;
            freeLookCamera.m_Lens.FieldOfView = Mathf.Clamp(currentFOV, minFOV, maxFOV);
        }
    }

    public void UpgradeSpeed(float walkBonus, float runBonus, float dashBonus)
    {
        walkSpeed += walkBonus;
        runSpeed += runBonus;
        dashSpeed += dashBonus;
    }
}