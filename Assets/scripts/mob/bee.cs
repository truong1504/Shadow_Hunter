using UnityEngine;
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

    [Header("Respawn")]
    public float respawnTime = 20f;

    private int currentHP;

    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;

    private Vector3 spawnPoint;
    private Vector3 targetPoint;

    private float timer;
    private bool isDead;

    private void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        spawnPoint = transform.position;
        currentHP = maxHP;

        ChooseTarget();
    }

    private void Update()
    {
        if (isDead)
            return;

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

        anim.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        isDead = true;

        yield return new WaitForSeconds(0.4f);

        gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnTime);

        transform.position = spawnPoint;

        currentHP = maxHP;

        gameObject.SetActive(true);

        isDead = false;

        ChooseTarget();
    }

    public void Attack()
    {
        if (isDead)
            return;

        anim.SetTrigger("Attack");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead)
            return;

        if (other.CompareTag("Player"))
        {
            Attack();

            PlayerHealth hp = other.GetComponent<PlayerHealth>();

            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
        }
    }
}