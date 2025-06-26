using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthSlider;
    public TMP_Text healthAmount;
    public int maxHealth = 100;
    public int currentHealth;
    public static bool IsAlive { get; private set; }
    
    // Add flag to prevent multiple death calls
    private bool deathCalled = false;

    void Start()
    {
        currentHealth = maxHealth;
        IsAlive = true;
        deathCalled = false; // Reset flag

        UpdateHealthSlider();
    }

    void Update()
    {
        // Only call LevelLost once when health reaches 0
        if (currentHealth <= 0 && !deathCalled)
        {
            deathCalled = true;
            IsAlive = false;
            
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                Debug.Log("PlayerHealth: Calling LevelLost due to health reaching 0");
                levelManager.LevelLost();
            }
            else
            {
                Debug.LogError("PlayerHealth: Could not find LevelManager!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Don't take damage if already dead
        if (!IsAlive || deathCalled) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthSlider();

        Debug.Log("Damage Taken, current health: " + currentHealth);
    }

    public void TakeHealth(int health)
    {
        // Don't heal if dead
        if (!IsAlive || deathCalled) return;
        
        currentHealth += health;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthSlider();
    }

    public void TakeBonusHealth(int bonusHealth)
    {
        // Don't give bonus health if dead
        if (!IsAlive || deathCalled) return;
        
        maxHealth += bonusHealth;
        currentHealth = maxHealth;
        maxHealth = Mathf.Clamp(maxHealth, 0, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthSlider();
    }

    void UpdateHealthSlider()
    {
        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            if (healthAmount != null)
                healthAmount.text = currentHealth.ToString();
        }
    }
}