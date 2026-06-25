using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Fix")]
    [Tooltip("Thời gian (giây) bỏ qua ground check ngay sau khi nhảy, để anim Jump không bị cắt ngay")]
    [SerializeField] private float jumpIgnoreGroundTime = 0.1f;

    [Header("Jump Anim Lock")]
    [Tooltip("Thời gian (giây) animation Jump bắt buộc phải hiển thị khi nhảy đơn")]
    [SerializeField] private float singleJumpAnimDuration = 0.2f;
    [Tooltip("Thời gian (giây) animation Jump bắt buộc phải hiển thị khi nhảy đúp (lần 2)")]
    [SerializeField] private float doubleJumpAnimDuration = 0.4f;

    private bool isAttacking;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private float moveInput;

    private bool isGrounded;
    private int groundContactCount; // số collider đất đang chạm cùng lúc

    private int jumpCount;
    private const int MAX_JUMPS = 2;

    private float ignoreGroundTimer; // chặn false-positive ground check ngay lúc vừa nhảy
    private float animLockTimer;     // khóa animation Jump trong 1 khoảng thời gian cố định

    private void HandleAttack()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            anim.SetTrigger("Attack");

            Debug.Log("Attack");
        }
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (groundLayer.value == 0)
            Debug.LogWarning("[Player] Ground Layer đang để trống (Nothing). " +
                "Sẽ KHÔNG BAO GIỜ phát hiện mặt đất.");
    }

    private void Update()
    {
        UpdateIgnoreGroundTimer();
        UpdateAnimLockTimer();
        HandleInput();
        HandleJump();
        UpdateAnimator();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void UpdateIgnoreGroundTimer()
    {
        if (ignoreGroundTimer > 0f)
        {
            ignoreGroundTimer -= Time.deltaTime;

            // Chỉ ép isGrounded = false khi vẫn CHƯA có va chạm đất thật.
            // Nếu OnCollisionEnter2D đã xác nhận chạm đất, ignoreGroundTimer
            // đã được set về 0 ở đó, nên đoạn này sẽ không ghi đè isGrounded.
            if (!isGrounded)
            {
                isGrounded = false;
            }
        }
    }

    private void UpdateAnimLockTimer()
    {
        if (animLockTimer > 0f)
        {
            animLockTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsGroundLayer(collision.gameObject.layer))
            return;

        groundContactCount++;

        // Va chạm vật lý là CHẮC CHẮN đã chạm đất -> hủy luôn timer "ignore"
        // để Update() không ghi đè isGrounded về false ở frame sau.
        ignoreGroundTimer = 0f;

        isGrounded = true;
        jumpCount = 0;
        Debug.Log("Landed (Collision) -> Reset jumpCount");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsGroundLayer(collision.gameObject.layer))
            return;

        groundContactCount = Mathf.Max(0, groundContactCount - 1);

        if (groundContactCount == 0)
        {
            isGrounded = false;
        }
    }

    private bool IsGroundLayer(int layer)
    {
        return groundLayer.value == (groundLayer.value | (1 << layer));
    }

    private void HandleInput()
    {
        if (isAttacking)
        {
            moveInput = 0f;
            return;
        }
        moveInput = 0f;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput = -1f;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput = 1f;
        }

        if (moveInput > 0)
            sr.flipX = false;
        else if (moveInput < 0)
            sr.flipX = true;
    }

    private void HandleJump()
    {
        bool jumpPressed =
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame;

        if (jumpPressed && jumpCount < MAX_JUMPS)
        {
            jumpCount++;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                0f
            );

            rb.AddForce(
                Vector2.up * jumpForce,
                ForceMode2D.Impulse
            );

            ignoreGroundTimer = jumpIgnoreGroundTime;
            isGrounded = false;

            // Khóa animation Jump theo thời gian riêng cho từng lần nhảy
            animLockTimer = (jumpCount >= 2) ? doubleJumpAnimDuration : singleJumpAnimDuration;

            anim.SetTrigger("JumpTrigger");

            Debug.Log("Jump Count = " + jumpCount + " | AnimLock = " + animLockTimer + "s");
        }
    }

    private void UpdateAnimator()
    {
        anim.SetFloat("Speed", Mathf.Abs(moveInput));

        // Gameplay isGrounded vẫn đúng (cho jumpCount, vật lý...).
        // Nhưng Animator chỉ nhận IsGrounded = true khi animLockTimer đã hết,
        // để animation Jump luôn hiển thị đủ thời gian quy định.
        bool animGrounded = isGrounded && animLockTimer <= 0f;
        anim.SetBool("IsGrounded", animGrounded);
    }
}