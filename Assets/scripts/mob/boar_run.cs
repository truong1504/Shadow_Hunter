using UnityEngine;
using System.Collections;

public class boar_run : MonoBehaviour
{
    [Header("Di chuyển")]
    public float speed = 2f;
    public float distance = 4f;
    public float waitTime = 2f;

    [Header("Hướng sprite")]
    // Tick nếu sprite gốc nhìn sang PHẢI
    // Bỏ tick nếu sprite gốc nhìn sang TRÁI
    public bool spriteFaceRight = false;

    private Vector3 startPos;
    private Vector3 originalScale;

    private int direction = 1;     // 1 = đi phải, -1 = đi trái
    private bool isWaiting = false;

    private Animator animator;

    void Start()
    {
        startPos = transform.position;
        originalScale = transform.localScale;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isWaiting)
            return;

        // Di chuyển
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

        // Chuyển animation Run
        animator.SetFloat("Speed", 1);

        // Đổi hướng nhìn
        Flip();

        // Đến điểm bên phải
        if (direction == 1 && transform.position.x >= startPos.x + distance)
        {
            StartCoroutine(ChangeDirection());
        }

        // Đến điểm bên trái
        if (direction == -1 && transform.position.x <= startPos.x)
        {
            StartCoroutine(ChangeDirection());
        }
    }

    void Flip()
    {
        float x = Mathf.Abs(originalScale.x);

        if (spriteFaceRight)
        {
            // Sprite gốc nhìn phải
            if (direction == 1)
                transform.localScale = new Vector3(x, originalScale.y, originalScale.z);
            else
                transform.localScale = new Vector3(-x, originalScale.y, originalScale.z);
        }
        else
        {
            // Sprite gốc nhìn trái
            if (direction == 1)
                transform.localScale = new Vector3(-x, originalScale.y, originalScale.z);
            else
                transform.localScale = new Vector3(x, originalScale.y, originalScale.z);
        }
    }

    IEnumerator ChangeDirection()
    {
        isWaiting = true;

        // Chuyển Idle
        animator.SetFloat("Speed", 0);

        // Đứng yên
        yield return new WaitForSeconds(waitTime);

        // Đổi hướng
        direction *= -1;

        isWaiting = false;
    }
}