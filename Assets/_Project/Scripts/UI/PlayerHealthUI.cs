using UnityEngine;
using UnityEngine.UI; // Necesario para trabajar con UI (Slider)
using TMPro; // Si usas TextMeshPro para el texto (opcional)

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    // [SerializeField] private TextMeshProUGUI healthText; // Opcional: para mostrar "100/100"

    [Header("Player Reference")]
    // Asigna el GameObject del Jugador aquí en el Inspector
    [SerializeField] private HealthManager playerHealthManager;

    void Awake()
    {
        // --- Validación ---
        if (healthSlider == null)
        {
            Debug.LogError("Health Slider no asignado en PlayerHealthUI!", this);
            enabled = false;
            return;
        }
        if (playerHealthManager == null)
        {
            Debug.LogError("Player Health Manager no asignado en PlayerHealthUI! Intentando encontrar Player...", this);
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealthManager = player.GetComponent<HealthManager>();
                if (playerHealthManager == null)
                {
                    Debug.LogError("Player encontrado, pero no tiene HealthManager!", this);
                    enabled = false;
                    return;
                }
                else
                {
                    Debug.LogWarning("Player Health Manager asignado automáticamente.", this);
                }
            }
            else
            {
                Debug.LogError("No se pudo encontrar el Player con la etiqueta 'Player'.", this);
                enabled = false;
                return;
            }
        }

        // --- Suscripción al Evento ---
        // Cuando la salud del jugador cambie (evento OnHealthChanged), llamaremos a UpdateUI.
        playerHealthManager.OnHealthChanged += UpdateUI;
        // Opcional: si también quieres reaccionar a la muerte (ej. mostrar pantalla game over)
        // playerHealthManager.OnDied += HandlePlayerDeath;
    }

    void Start()
    {
        // Establecer los valores iniciales de la UI al empezar
        InitializeUI();
    }

    void OnDestroy()
    {
        // --- Desuscripción del Evento ---
        // ¡MUY IMPORTANTE! Desuscribirse cuando este objeto se destruya para evitar errores.
        if (playerHealthManager != null)
        {
            playerHealthManager.OnHealthChanged -= UpdateUI;
            // playerHealthManager.OnDied -= HandlePlayerDeath;
        }
    }

    private void InitializeUI()
    {
        if (playerHealthManager != null && healthSlider != null)
        {
            healthSlider.maxValue = playerHealthManager.MaxHealth;
            healthSlider.value = playerHealthManager.CurrentHealth;
            // if (healthText != null) healthText.text = $"{playerHealthManager.CurrentHealth}/{playerHealthManager.MaxHealth}";
        }
    }

    // Esta función será llamada automáticamente por el evento OnHealthChanged
    private void UpdateUI(float currentHealth) // Recibe la vida actual del evento
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        // if (healthText != null && playerHealthManager != null)
        // {
        //    healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{playerHealthManager.MaxHealth}"; // CeilToInt para redondear
        // }
    }

    // private void HandlePlayerDeath() {
    // Aquí podrías activar una pantalla de Game Over, etc.
    // Debug.Log("UI: Jugador Muerto!");
    // }
}