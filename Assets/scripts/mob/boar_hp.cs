using UnityEngine;
using UnityEngine.UI;

public class boar_hp : MonoBehaviour
{
    //==========================
    // HP tối đa
    //==========================
    public int maxHP = 100;

    // HP hiện tại
    private int currentHP;

    // Thanh máu
    public Slider hpSlider;

    // Có còn sống không
    private bool isDead = false;

    // Lưu vị trí sinh ra
    private Vector3 spawnPosition;

    private Rigidbody2D rb;

    void Start()
    {
        // Lưu vị trí ban đầu
        spawnPosition = transform.position;

        // Ban đầu HP = Max HP
        currentHP = maxHP;

        rb = GetComponent<Rigidbody2D>();

        // Cập nhật thanh máu (chỉ khi đã gán Slider)
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
        else
        {
            Debug.LogWarning("[boar_hp] Chưa gán Hp Slider trong Inspector cho " + gameObject.name);
        }
    }

    //----------------------------------------------------
    // Hàm nhận sát thương
    //----------------------------------------------------
    public void TakeDamage(int damage)
    {
        // Nếu chết rồi thì không nhận damage nữa
        if (isDead)
            return;

        currentHP -= damage;

        if (hpSlider != null)
            hpSlider.value = currentHP;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    //----------------------------------------------------
    // Quái chết
    //----------------------------------------------------
    void Die()
    {
        isDead = true;

        // Tắt hình ảnh
        GetComponent<SpriteRenderer>().enabled = false;

        // Tắt collider
        GetComponent<Collider2D>().enabled = false;

        // Tắt script di chuyển
        GetComponent<boar_run>().enabled = false;

        // Tắt Rigidbody2D để không bị rơi do trọng lực
        // khi mất Collider (nguyên nhân thanh máu bị "rơi xuống")
        if (rb != null)
        {
            rb.simulated = false;
        }

        // Ẩn hẳn Canvas (HP bar), không chỉ ẩn Slider riêng,
        // để toàn bộ thanh máu biến mất theo boar
        if (hpSlider != null)
            hpSlider.transform.root.gameObject.SetActive(false);

        // Coroutine
        // Chờ 15 giây rồi hồi sinh
        StartCoroutine(Respawn());
    }

    //----------------------------------------------------
    // Coroutine
    //----------------------------------------------------
    // Kiến thức:
    // Coroutine là hàm có thể tạm dừng rồi chạy tiếp.
    // Unity dùng rất nhiều.
    //----------------------------------------------------
    System.Collections.IEnumerator Respawn()
    {
        yield return new WaitForSeconds(15f);

        transform.position = spawnPosition;

        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
            hpSlider.transform.root.gameObject.SetActive(true);
        }

        GetComponent<SpriteRenderer>().enabled = true;

        GetComponent<Collider2D>().enabled = true;

        GetComponent<boar_run>().enabled = true;

        // Bật lại Rigidbody2D khi hồi sinh
        if (rb != null)
        {
            rb.simulated = true;
        }

        isDead = false;
    }
}