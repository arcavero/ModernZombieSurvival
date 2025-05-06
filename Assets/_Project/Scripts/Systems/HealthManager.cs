using UnityEngine;
using System; // Necesario para Action (eventos)

public class HealthManager : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    // Eventos para notificar a otros scripts sobre cambios en la salud o muerte
    // Action<float>: Evento que pasa la vida actual como parámetro
    // Action: Evento simple sin parámetros
    public event Action<float> OnHealthChanged;
    public event Action OnDied;

    public float CurrentHealth => currentHealth; // Propiedad pública de solo lectura (getter)
    public float MaxHealth => maxHealth;       // Propiedad pública de solo lectura
    public bool IsDead => currentHealth <= 0; // Propiedad que calcula si está muerto

    void Awake()
    {
        // Inicializar la vida al máximo al despertar el objeto
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Aplica daño a esta entidad.
    /// </summary>
    /// <param name="damageAmount">La cantidad de daño a aplicar (debe ser positiva).</param>
    public void TakeDamage(float damageAmount)
    {
        if (IsDead || damageAmount <= 0)
        {
            // No hacer nada si ya está muerto o el daño es inválido
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0f); // Asegurar que la vida no baje de 0

        Debug.Log($"{gameObject.name} recibió {damageAmount} de daño. Vida restante: {currentHealth}");

        // Lanzar el evento para notificar a otros scripts (como barras de vida, etc.)
        OnHealthChanged?.Invoke(currentHealth);

        // Comprobar si ha muerto después de recibir el daño
        if (IsDead)
        {
            Die();
        }
    }

    /// <summary>
    /// Sana a esta entidad.
    /// </summary>
    /// <param name="healAmount">La cantidad a curar (debe ser positiva).</param>
    public void Heal(float healAmount)
    {
        if (IsDead || healAmount <= 0)
        {
            return;
        }

        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Asegurar que la vida no supere el máximo

        Debug.Log($"{gameObject.name} se curó {healAmount}. Vida actual: {currentHealth}");

        // Lanzar el evento de cambio de salud
        OnHealthChanged?.Invoke(currentHealth);
    }

    // Método privado llamado cuando la vida llega a 0
    private void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto.");

        // Lanzar el evento de muerte para que otros scripts reaccionen
        OnDied?.Invoke();

        // --- Lógica de muerte básica (podemos mejorarla después) ---
        // Por ahora, simplemente desactivaremos el objeto para que desaparezca.
        // Más adelante podríamos instanciar efectos, activar ragdoll, etc.
        // Destroy(gameObject, 2f); // Destruye el objeto después de 2 segundos
        gameObject.SetActive(false); // O simplemente lo desactiva
    }

    // (Opcional: Método para establecer la vida inicial si es necesario desde fuera)
    public void SetInitialHealth(float initialHealth, float initialMaxHealth)
    {
        maxHealth = Mathf.Max(1f, initialMaxHealth); // Max health debe ser al menos 1
        currentHealth = Mathf.Clamp(initialHealth, 0f, maxHealth);
        // Lanzar evento por si la UI necesita actualizarse al inicio
        OnHealthChanged?.Invoke(currentHealth);
    }
}