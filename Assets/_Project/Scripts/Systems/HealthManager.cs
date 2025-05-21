using UnityEngine;
using System; // Necesario para Action (eventos)

public class HealthManager : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Audio Feedback (Optional)")]
    [SerializeField] private AudioClip damageSound; // Asigna tu sonido de daño aquí en el Inspector
    // [SerializeField] private AudioClip deathSound;  // Podrías tener un sonido de muerte separado

    private AudioSource audioSource; // Referencia al componente AudioSource

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

        // Obtener la referencia al AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Si no está, y hemos puesto RequireComponent, Unity lo añadirá.
            // Pero es buena práctica tener una advertencia si olvidamos el RequireComponent.
            Debug.LogWarning($"HealthManager en {gameObject.name} no tiene un AudioSource. El sonido de daño no se reproducirá.", this);
        }

    }

    /// <summary>
    /// Aplica daño a esta entidad.
    /// </summary>
    /// <param name="damageAmount">La cantidad de daño a aplicar (debe ser positiva).</param>
    public void TakeDamage(float damageAmount)
    {
        if (IsDead || damageAmount <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"{gameObject.name} recibió {damageAmount} de daño. Vida restante: {currentHealth}");

        // --- Reproducir Sonido de Daño ---
        if (damageSound != null && audioSource != null)
        {
            // audioSource.PlayOneShot(damageSound); // PlayOneShot es bueno para efectos rápidos
            // O si quieres más control (ej. sobre el volumen del clip específico):
            audioSource.PlayOneShot(damageSound, 1.0f); // El segundo parámetro es volumeScale
        }
        // --- Fin Sonido de Daño ---

        OnHealthChanged?.Invoke(currentHealth);

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