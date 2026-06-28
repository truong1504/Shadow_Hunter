using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 300;
    private int currentHP;

    [Header("UI")]
    [Tooltip("Kéo Slider (trong Canvas World Space đặt trên đầu Player) vào đây")]
    public Slider hpSlider;

    private void Start()
    {
        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        if (hpSlider != null)
            hpSlider.value = currentHP;

        Debug.Log("Player HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");
    }
}