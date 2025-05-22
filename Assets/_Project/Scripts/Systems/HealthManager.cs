using UnityEngine;
using System;

public class HealthManager : MonoBehaviour
{
    [Header("Default Health Settings (Usado si no se inicializa desde otro script)")]
    [SerializeField] private float initialMaxHealthValue = 100f;
    private float currentHealth;
    private float maxHealth;

    [Header("Audio Feedback (Optional)")]
    [SerializeField] private AudioClip damageSound;
    private AudioSource audioSource;

    // --- MODIFICADO: Evento OnDied ahora puede pasar el propio HealthManager ---
    // Esto permite a los suscriptores acceder a cualquier información pública del HealthManager
    // o de componentes en el mismo GameObject, como un EnemyData si lo guardamos aquí.
    public event Action<HealthManager> OnDied; // Pasa la instancia de este HealthManager

    public event Action<float> OnHealthChanged;


    // --- NUEVO: Referencia opcional a EnemyData para la recompensa ---
    // EnemyInitializer se encargará de asignar esto.
    private EnemyData associatedEnemyData;


    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public EnemyData GetAssociatedEnemyData() => associatedEnemyData; // Getter para el EnemyData


    void Awake()
    {
        if (maxHealth <= 0)
        {
            SetInitialHealth(initialMaxHealthValue, initialMaxHealthValue);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && damageSound != null) // Solo advertir si hay un damageSound asignado pero no AudioSource
        {
            Debug.LogWarning($"HealthManager en {gameObject.name} tiene un 'damageSound' asignado pero no tiene AudioSource. El sonido de daño no se reproducirá.", this);
        }
    }

    public void SetInitialHealth(float health, float maxHealthValue)
    {
        this.maxHealth = Mathf.Max(1f, maxHealthValue);
        this.currentHealth = Mathf.Clamp(health, 0f, this.maxHealth);
        OnHealthChanged?.Invoke(this.currentHealth);
        // Debug.Log($"{gameObject.name} salud inicializada a {currentHealth}/{this.maxHealth}");
    }

    // --- NUEVO: Método para que EnemyInitializer asigne el EnemyData ---
    public void SetEnemyDataReference(EnemyData data)
    {
        associatedEnemyData = data;
        // Podrías querer actualizar maxHealth aquí también si el EnemyData es la fuente de verdad
        // y SetInitialHealth no fue llamado por el initializer con los datos del EnemyData.
        // Pero si EnemyInitializer llama a SetInitialHealth DESPUÉS de asignar enemyData,
        // o si llama a SetInitialHealth con los valores de enemyData, no es necesario aquí.
        // Por ahora, asumimos que EnemyInitializer gestiona SetInitialHealth correctamente.
    }


    public void TakeDamage(float damageAmount)
    {
        if (IsDead || damageAmount <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (damageSound != null && audioSource != null && audioSource.isActiveAndEnabled)
        {
            audioSource.PlayOneShot(damageSound, 1.0f);
        }

        OnHealthChanged?.Invoke(currentHealth);

        if (IsDead)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        // ... (sin cambios) ...
        if (IsDead || healAmount <= 0) return;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        // Debug.Log($"{gameObject.name} ha muerto.");

        // --- MODIFICADO: Invocar OnDied pasando 'this' (la instancia actual de HealthManager) ---
        OnDied?.Invoke(this); // 'this' se refiere a este componente HealthManager

        gameObject.SetActive(false); // O Destroy(gameObject);
    }
}