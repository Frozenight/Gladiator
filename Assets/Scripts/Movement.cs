using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public PlayerInputActions playerControls;
    private Vector2 moveDirection = Vector2.zero;
    private InputAction move;
    private InputAction jump;
    private InputAction run;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private float jumpHeight = 2.0f;
    private float gravityValue = -9.81f;

    float runningSpeed = 12f;
    float walkingSpeed = 6f;
    float speed;

    private float jumpCooldown = 2f;
    private bool canJump = true;
    bool _canMove = true;
    public Transform groundCheck;
    public LayerMask groundMask;
    private float groundDistance = 0.6f;
    public bool IsJumpAvailable
    {
        get { return isJumpAvalable; }
        set
        {
            if (value != isJumpAvalable)
            {
                isJumpAvalable = value;
                if (isJumpAvalable)
                {
                    StartJumpCD();
                }
            }
        }
    }

    private bool isJumpAvalable;

    public bool isGrounded
    {
        get { return isGround; }
        set
        {
            if (value != isGround)
            {
                isGround = value;
                if (isGround)
                {
                    StartMovementCD(2);
                }
            }
        }
    }

    private bool isGround;

    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    public Transform cam;

    private Animator m_Animator;

    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }
    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();

        jump = playerControls.Player.Jump;
        jump.Enable();
        jump.performed += Jump;

        run = playerControls.Player.Run;
        run.Enable();
        run.started += Run;
        run.canceled += Run;
    }
    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        run.Disable();
    }

    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        m_Animator = gameObject.GetComponentInChildren<Animator>();
        speed = walkingSpeed;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        // on Ground
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
            IsJumpAvailable = true;
            m_Animator.SetBool("isGrounded", true);
            m_Animator.SetBool("isFalling", false);
            m_Animator.SetBool("isJumping", false);
        }
        // not Touching Ground
        else
        {
            isJumpAvalable = false;
            m_Animator.SetBool("isGrounded", false);
            if (playerVelocity.y < -2)
            {
                m_Animator.SetBool("isFalling", true);
            }
        }
        //gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
        moveDirection = move.ReadValue<Vector2>();

        if (moveDirection.magnitude >= 0.1f && _canMove)
        {
            m_Animator.SetBool("isWalking", true);
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else {
            m_Animator.SetBool("isWalking", false);
        }
    }

    void StartJumpCD()
    {
        StartCoroutine(JumpCooldown(jumpCooldown));
    }

    void StartMovementCD(float cooldown)
    {
        StartCoroutine(MovementCooldown(cooldown));
    }

    public IEnumerator JumpCooldown(float cooldown)
    {
        canJump = false;
        yield return new WaitForSeconds(cooldown);
        canJump = true;
    }

    public IEnumerator MovementCooldown(float cooldown)
    {
        _canMove = false;
        yield return new WaitForSeconds(cooldown);
        _canMove = true;
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (!canJump || !isGrounded)
            return;

        playerVelocity.y = Mathf.Sqrt(-jumpHeight * gravityValue);
        m_Animator.SetBool("isJumping", true);
    }

    void Run(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            speed = runningSpeed;
            m_Animator.SetBool("isRunning", true);
        }
        else if (context.canceled)
        {
            speed = walkingSpeed;
            m_Animator.SetBool("isRunning", false);
            m_Animator.SetBool("isWalking", true);
        }
    }    
}