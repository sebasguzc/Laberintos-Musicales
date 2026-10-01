using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Referencia a la UI")]
    public TextMeshProUGUI healthText;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthText == null)
        {
            healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Vida: {currentHealth}";
        }
    }

    private void Die()
    {
        Debug.Log("¡El jugador ha muerto!");

        // Busca el administrador de Game Over en la escena y activa el panel
        GameOverManager gameOverManager = FindAnyObjectByType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }

        gameObject.SetActive(false);
    }
}