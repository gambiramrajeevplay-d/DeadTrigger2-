using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("VFX")]
    public ParticleSystem hitEffect;
    public ParticleSystem deathEffect;

    private Image healthFill;
    private TextMeshProUGUI healthText;


    void Start()
    {
        currentHealth = maxHealth;

        GameObject fillObj = GameObject.FindGameObjectWithTag("Health_Fill");
        if (fillObj != null)
            healthFill = fillObj.GetComponent<Image>();

        GameObject textObj = GameObject.FindGameObjectWithTag("Health_Text");
        if (textObj != null)
            healthText = textObj.GetComponent<TextMeshProUGUI>();

        UpdateHealthUI();
    }

    public void TakeDamage(int damage, string type)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 💥 HIT EFFECT
       

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        float percent = (float)currentHealth / maxHealth;

        if (healthFill != null)
            healthFill.fillAmount = percent;

        if (healthText != null)
            healthText.text = currentHealth.ToString();
    }
    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }

    public void Heal(int amount)
    {
        if (currentHealth >= maxHealth)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }
    void Die()
    {
        Debug.Log("Player Died!");

        // Prevent dying twice
        if (currentHealth > 0)
            return;

        if (GameManager_Temp.Instance != null)
            GameManager_Temp.Instance.ShowFinishPanel();

        // Death VFX
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Destroy all enemy bullets
        Bullet.DestroyAllEnemyBullets();

        // Stop all enemies
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
                enemy.enabled = false;
        }

        // Kill player
        PlayerAutoMove player = GetComponentInParent<PlayerAutoMove>();

        if (player == null)
            player = GetComponent<PlayerAutoMove>();

        if (player != null)
            player.KillPlayer();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied();
        }

    }
}