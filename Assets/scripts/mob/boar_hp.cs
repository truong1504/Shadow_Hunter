using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    // Canvas chứa thanh máu (kéo vào Inspector)
    public GameObject hpCanvas;

    // Có còn sống không
    private bool isDead = false;

    // Lưu vị trí sinh ra
    private Vector3 spawnPosition;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;

    void Start()
    {
        // Lưu vị trí ban đầu
        spawnPosition = transform.position;

        // Lấy components
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        // Ban đầu HP = Max HP
        currentHP = maxHP;

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

        // Nếu chưa kéo hpCanvas thì tự tìm
        if (hpCanvas == null && hpSlider != null)
        {
            hpCanvas = hpSlider.transform.root.gameObject;
            Debug.Log("Tự động tìm Canvas: " + hpCanvas.name);
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

        // Trừ máu
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        // Cập nhật thanh máu
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
            Debug.Log($"Boar HP: {currentHP}/{maxHP}");
        }

        // Animation Hit
        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }

        // Kiểm tra chết
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
        if (sr != null)
            sr.enabled = false;

        // Tắt collider
        if (col != null)
            col.enabled = false;

        // Tắt script di chuyển
        boar_run boarRun = GetComponent<boar_run>();
        if (boarRun != null)
            boarRun.enabled = false;

        // Tắt Rigidbody2D
        if (rb != null)
            rb.simulated = false;

        // Ẩn thanh máu (cả Canvas)
        if (hpCanvas != null)
        {
            hpCanvas.SetActive(false);
            Debug.Log("Đã ẩn thanh máu Boar");
        }
        else if (hpSlider != null)
        {
            // Fallback: ẩn Slider nếu không có Canvas
            hpSlider.gameObject.SetActive(false);
            Debug.Log("Đã ẩn Slider (không có Canvas)");
        }

        // Bắt đầu hồi sinh
        StartCoroutine(Respawn());
    }

    //----------------------------------------------------
    // Coroutine hồi sinh
    //----------------------------------------------------
    IEnumerator Respawn()
    {
        // Đợi 15 giây
        yield return new WaitForSeconds(15f);

        // Reset vị trí
        transform.position = spawnPosition;

        // Reset HP
        currentHP = maxHP;

        // Cập nhật thanh máu
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }

        // Hiện thanh máu
        if (hpCanvas != null)
        {
            hpCanvas.SetActive(true);
            Debug.Log("Đã hiện lại thanh máu Boar");
        }
        else if (hpSlider != null)
        {
            hpSlider.gameObject.SetActive(true);
            Debug.Log("Đã hiện lại Slider");
        }

        // Hiện lại hình ảnh
        if (sr != null)
            sr.enabled = true;

        // Bật lại collider
        if (col != null)
            col.enabled = true;

        // Bật lại script di chuyển
        boar_run boarRun = GetComponent<boar_run>();
        if (boarRun != null)
            boarRun.enabled = true;

        // Bật lại Rigidbody2D
        if (rb != null)
            rb.simulated = true;

        // Reset Animator
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
            anim.SetFloat("Speed", 0);
        }

        // Reset trạng thái
        isDead = false;

        Debug.Log("Boar đã hồi sinh!");
    }
}