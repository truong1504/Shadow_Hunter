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

    private Animator anim;
    private Collider2D col;

    private Transform player;
    private bool isAttacking;
    private float cooldownTimer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        if (anim == null)
            Debug.LogError("[DragonBoss] Thiếu component Animator!");

        if (playerLayer.value == 0)
            Debug.LogWarning("[DragonBoss] Player Layer đang để trống (Nothing). " +
                "Sẽ không phát hiện được Player.");
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

        // Tìm Player bằng Tag, chỉ cần tìm 1 lần vì Boss đứng yên không cần track liên tục bằng OverlapCircle mỗi frame
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
            StartAttack();
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;

        anim.SetTrigger("Attack");

        // Lửa rơi xuống đất và gây damage đúng vào thời điểm khớp với animation
        Invoke(nameof(DealFireDamage), fireDelay);

        // Kết thúc trạng thái tấn công, cho phép tấn công lại sau khi animation xong
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

        // Dùng HashSet để tránh trường hợp Player có nhiều Collider2D
        // khiến bị trừ máu nhiều lần trong 1 lần lửa rơi (chỉ gây damage 1 LẦN DUY NHẤT)
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

        // Dừng Animator để sprite đứng yên tại khung hình cuối của animation Death,
        // KHÔNG ẩn hay xóa GameObject -> xác Boss vẫn hiển thị trên màn hình.
        if (anim != null)
            anim.enabled = false;
    }
    private void HideAfterDeath()
    {
        if (hpCanvas != null)
            hpCanvas.SetActive(false);

        // Boss trùm thường không respawn như quái thường, nên ẩn hẳn sau khi chết.
        // Nếu bạn muốn Boss biến mất hoàn toàn khỏi Scene, đổi dòng dưới thành Destroy(gameObject);
        gameObject.SetActive(false);
    }

    // Vẽ vùng phát hiện và vùng damage trong Scene view để dễ chỉnh thông số
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}