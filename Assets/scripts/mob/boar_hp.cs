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

    void Start()
    {
        // Lưu vị trí ban đầu
        spawnPosition = transform.position;

        // Ban đầu HP = Max HP
        currentHP = maxHP;

        // Cập nhật thanh máu
        hpSlider.maxValue = maxHP;
        hpSlider.value = currentHP;
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

        // Ẩn thanh máu
        hpSlider.gameObject.SetActive(false);

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

        hpSlider.value = currentHP;

        hpSlider.gameObject.SetActive(true);

        GetComponent<SpriteRenderer>().enabled = true;

        GetComponent<Collider2D>().enabled = true;

        GetComponent<boar_run>().enabled = true;

        isDead = false;
    }
}