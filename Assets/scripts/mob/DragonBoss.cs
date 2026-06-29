using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragonBoss : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 1000;
    private int currentHP;
    private bool isDead;

    [Header("UI")]
    [Tooltip("Kéo Slider (Canvas World Space trên đầu Boss) vào đây")]
    [SerializeField] private Slider hpSlider;
    [Tooltip("Kéo Canvas (cha của Slider) vào đây để ẩn/hiện cả thanh máu khi chết")]
    [SerializeField] private GameObject hpCanvas;

    [Header("Tấn công")]
    [Tooltip("Khoảng cách để Boss phát hiện Player và bắt đầu tấn công")]
    [SerializeField] private float attackRange = 4f;
    [Tooltip("Bán kính vùng lửa gây sát thương trên mặt đất, tính từ vị trí Boss")]
    [SerializeField] private float damageRadius = 2.5f;
    [Tooltip("Sát thương gây ra MỘT LẦN khi lửa chạm đất (không phải damage theo thời gian)")]
    [SerializeField] private int attackDamage = 50;
    [Tooltip("Thời gian (giây) từ lúc bắt đầu animation Attack đến lúc lửa thực sự chạm đất gây damage")]
    [SerializeField] private float fireDelay = 0.6f;
    [Tooltip("Tổng thời gian animation Attack chạy xong, trước khi được tấn công lại")]
    [SerializeField] private float attackDuration = 1.2f;
    [Tooltip("Thời gian chờ giữa 2 lần tấn công liên tiếp")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Hit / Death")]
    [Tooltip("Thời gian (giây) animation Death chạy xong trước khi ẩn Boss")]
    [SerializeField] private float deathDelay = 1.5f;

    [Header("Flip Settings")]
    [Tooltip("Tick nếu sprite đang quay mặt phải, bỏ tick nếu quay mặt trái")]
    [SerializeField] private bool isFacingRight = true;
    [Tooltip("Tick để đảo ngược hướng (dùng khi animation bị ngược)")]
    [SerializeField] private bool invertFlip = false;

    private Animator anim;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    private Transform player;
    private bool isAttacking;
    private float cooldownTimer;

    private bool currentFacingRight = true;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (anim == null)
            Debug.LogError("[DragonBoss] Thiếu component Animator!");

        if (playerLayer.value == 0)
            Debug.LogWarning("[DragonBoss] Player Layer đang để trống (Nothing). " +
                "Sẽ không phát hiện được Player.");

        currentFacingRight = isFacingRight;
    }

    private void Start()
    {
        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
        else
        {
            Debug.LogWarning("[DragonBoss] Chưa gán Hp Slider trong Inspector cho " + gameObject.name);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("[DragonBoss] Không tìm thấy GameObject có Tag 'Player' trong Scene.");
    }

    private void Update()
    {
        if (isDead || player == null)
            return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (isAttacking)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && cooldownTimer <= 0f)
        {
            FlipTowardsPlayer();
            StartAttack();
        }
    }

    private void FlipTowardsPlayer()
    {
        if (player == null)
            return;

        bool isPlayerOnRight = player.position.x > transform.position.x;

        // Logic lật cơ bản
        bool shouldFaceRight = isPlayerOnRight;

        // Áp dụng đảo ngược nếu animation bị ngược
        if (invertFlip)
        {
            shouldFaceRight = !shouldFaceRight;
        }

        // Áp dụng hướng mặc định
        if (!isFacingRight)
        {
            shouldFaceRight = !shouldFaceRight;
        }

        // Chỉ lật khi cần thay đổi
        if (shouldFaceRight == currentFacingRight)
            return;

        currentFacingRight = shouldFaceRight;

        // Lật Sprite
        Vector3 newScale = transform.localScale;
        newScale.x = shouldFaceRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x);
        transform.localScale = newScale;

        // Nếu có Animator riêng, lật luôn Animator
        if (anim != null)
        {
            Vector3 animScale = anim.transform.localScale;
            animScale.x = shouldFaceRight ? Mathf.Abs(animScale.x) : -Mathf.Abs(animScale.x);
            anim.transform.localScale = animScale;
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;

        anim.SetTrigger("Attack");

        Invoke(nameof(DealFireDamage), fireDelay);
        Invoke(nameof(EndAttack), attackDuration);
    }

    private void DealFireDamage()
    {
        if (isDead)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            damageRadius,
            playerLayer);

        HashSet<GameObject> alreadyHit = new HashSet<GameObject>();

        foreach (Collider2D hit in hits)
        {
            if (alreadyHit.Contains(hit.gameObject))
                continue;

            alreadyHit.Add(hit.gameObject);

            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Dragon Boss gay " + attackDamage + " damage cho Player");
            }
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        if (hpSlider != null)
            hpSlider.value = currentHP;

        anim.SetTrigger("Hit");

        Debug.Log("Dragon Boss HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        anim.SetTrigger("Death");

        if (col != null)
            col.enabled = false;

        CancelInvoke();

        Invoke(nameof(FreezeAtLastFrame), deathDelay);
    }

    private void FreezeAtLastFrame()
    {
        if (hpCanvas != null)
            hpCanvas.SetActive(false);

        if (anim != null)
            anim.enabled = false;
    }

    private void HideAfterDeath()
    {
        if (hpCanvas != null)
            hpCanvas.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}