// EnemyHealthUI.cs
using UnityEngine;
using UnityEngine.UI; // Necesario para Image

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;

    [Header("References")]
    // No necesitamos serializar linkedHealthManager si lo pasamos en Setup
    private HealthManager linkedHealthManager; // Hacerlo privado
    private Transform cameraTransform;

    [Header("Settings")]
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private float healthToActuallyHide = 0.99f; // Para evitar que se oculte por errores de precisión

    void Awake()
    {
        if (fillImage == null)
        {
            fillImage = GetComponentInChildren<Image>(); // Intenta encontrarlo en hijos si es una estructura de Slider
            if (fillImage == null)
            {
                Debug.LogError("Fill Image no asignado ni encontrado en hijos en EnemyHealthUI!", this);
                enabled = false; return;
            }
        }

        if (Camera.main != null) { cameraTransform = Camera.main.transform; }
        else { Debug.LogError("No se encontró la cámara principal (MainCamera tag). La barra de vida flotante podría no orientarse.", this); }
    }

    public void Setup(HealthManager healthManagerToLink) // Renombrar parámetro para claridad
    {
        linkedHealthManager = healthManagerToLink;
        if (linkedHealthManager == null)
        {
            Debug.LogError("Se intentó configurar EnemyHealthUI sin un HealthManager válido.", this);
            gameObject.SetActive(false); return;
        }

        // Suscribirse a los eventos del HealthManager vinculado
        linkedHealthManager.OnHealthChanged += UpdateUI;
        linkedHealthManager.OnDied += HandleDeath; // La suscripción sigue igual

        InitializeUI();
    }

    void OnDestroy()
    {
        if (linkedHealthManager != null)
        {
            linkedHealthManager.OnHealthChanged -= UpdateUI;
            linkedHealthManager.OnDied -= HandleDeath;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform != null && gameObject.activeSelf) // Solo rotar si está activo y hay cámara
        {
            transform.rotation = cameraTransform.rotation;
        }
    }

    private void InitializeUI()
    {
        if (linkedHealthManager != null && fillImage != null)
        {
            fillImage.fillAmount = linkedHealthManager.CurrentHealth / linkedHealthManager.MaxHealth;
            UpdateVisibility();
        }
    }

    private void UpdateUI(float currentHealth)
    {
        if (fillImage != null && linkedHealthManager != null)
        {
            fillImage.fillAmount = currentHealth / linkedHealthManager.MaxHealth;
            UpdateVisibility();
        }
    }

    private void UpdateVisibility()
    {
        if (linkedHealthManager == null || fillImage == null) return;

        // Si está muerto, siempre oculto (manejado por HandleDeath)
        if (linkedHealthManager.IsDead)
        {
            gameObject.SetActive(false);
            return;
        }

        // Determinar si debe estar activo
        bool shouldBeActive = !hideWhenFull || fillImage.fillAmount < healthToActuallyHide; // Usar un umbral pequeño

        if (gameObject.activeSelf != shouldBeActive)
        {
            gameObject.SetActive(shouldBeActive);
        }
    }

    // --- MODIFICADO: Añadir el parámetro HealthManager ---
    private void HandleDeath(HealthManager deceasedEnemyHealthManager) // 'deceasedEnemyHealthManager' es el HealthManager del enemigo que murió
    {
        // No necesitamos usar el parámetro aquí ya que este script está en la UI del propio enemigo
        // y ya tiene 'linkedHealthManager'. Pero la firma del método debe coincidir.
        gameObject.SetActive(false);
    }
}