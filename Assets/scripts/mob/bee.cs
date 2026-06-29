using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class bee : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 150;
    public int damage = 20;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolRadius = 4f;
    public float changeDirectionTime = 2f;

    [Header("Combat")]
    [Tooltip("Khoảng cách phát hiện Player để tấn công")]
    public float attackRange = 3f;
    [Tooltip("Thời gian hồi chiêu giữa các lần tấn công")]
    public float attackCooldown = 1.5f;
    [Tooltip("Khoảng cách dừng lại trước khi tấn công")]
    public float stopDistance = 1.5f;

    [Header("Respawn")]
    public float respawnTime = 20f;

    [Header("UI")]
    [Tooltip("Kéo Slider (trong Canvas World Space đặt trên đầu Bee) vào đây")]
    public Slider hpSlider;

    private int currentHP;

    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;

    private Vector3 spawnPoint;
    private Vector3 targetPoint;

    private float timer;
    private bool isDead;
    private bool isAttacking;
    private float attackTimer;

    // Biến để theo dõi Player
    private Transform player;
    private bool isChasingPlayer;

    private void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        spawnPoint = transform.position;
        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        // Tìm Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        ChooseTarget();
    }

    private void Update()
    {
        if (isDead)
            return;

        // Kiểm tra cooldown tấn công
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        // Kiểm tra Player trong phạm vi
        CheckForPlayer();

        // Di chuyển hoặc tuần tra
        if (isChasingPlayer && player != null)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void CheckForPlayer()
    {
        if (player == null)
        {
            isChasingPlayer = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // Nếu Player trong phạm vi tấn công
        if (distance <= attackRange)
        {
            isChasingPlayer = true;

            // Tấn công nếu đã hết cooldown và không đang tấn công
            if (attackTimer <= 0f && !isAttacking)
            {
                StartCoroutine(PerformAttack());
            }
        }
        else
        {
            isChasingPlayer = false;
        }
    }

    private void ChasePlayer()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Nếu Player ở xa hơn stopDistance thì di chuyển đến
        if (distance > stopDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime);

            // Quay mặt về hướng Player
            if (player.position.x > transform.position.x)
                sr.flipX = false;
            else
                sr.flipX = true;
        }
        else
        {
            // Đứng yên khi đã đến gần Player
            // Quay mặt về hướng Player
            if (player.position.x > transform.position.x)
                sr.flipX = false;
            else
                sr.flipX = true;
        }
    }

    private void Patrol()
    {
        timer += Time.deltaTime;

        if (timer >= changeDirectionTime)
        {
            timer = 0;
            ChooseTarget();
        }

        Fly();
    }

    private void Fly()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPoint,
            moveSpeed * Time.deltaTime);

        if (targetPoint.x > transform.position.x)
            sr.flipX = false;
        else
            sr.flipX = true;
    }

    private void ChooseTarget()
    {
        targetPoint = spawnPoint + new Vector3(
            Random.Range(-patrolRadius, patrolRadius),
            Random.Range(-patrolRadius, patrolRadius),
            0);
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

        if (currentHP <= 0)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        isDead = true;
        isChasingPlayer = false;
        isAttacking = false;

        if (hpSlider != null)
            hpSlider.gameObject.SetActive(false);

        // Tắt Collider để không va chạm khi chết
        if (col != null)
            col.enabled = false;

        yield return new WaitForSeconds(0.4f);

        gameObject.SetActive(false);

        Debug.Log($"Bee sẽ hồi sinh sau {respawnTime} giây");

        // Chờ hồi sinh
        yield return new WaitForSeconds(respawnTime);

        // Hồi sinh
        transform.position = spawnPoint;

        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
            hpSlider.gameObject.SetActive(true);
        }

        // Bật lại Collider
        if (col != null)
            col.enabled = true;

        gameObject.SetActive(true);

        isDead = false;
        isAttacking = false;
        attackTimer = 0f;

        // Reset animation
        if (anim != null)
            anim.Rebind();

        ChooseTarget();

        Debug.Log("Bee đã hồi sinh!");
    }

    IEnumerator PerformAttack()
    {
        if (isAttacking || isDead)
            yield break;

        isAttacking = true;
        attackTimer = attackCooldown;

        // Phát animation tấn công
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // Đợi animation tấn công (thời gian animation)
        yield return new WaitForSeconds(0.3f);

        // Gây sát thương nếu Player vẫn trong phạm vi
        if (player != null && !isDead)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                PlayerHealth hp = player.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.TakeDamage(damage);
                    Debug.Log($"Bee tấn công Player, gây {damage} sát thương!");
                }
            }
        }

        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Không dùng Trigger nữa, thay bằng CheckForPlayer trong Update
        // Giữ lại để tương thích nếu cần
    }

    // Vẽ phạm vi tấn công trong Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}