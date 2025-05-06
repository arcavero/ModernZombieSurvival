using UnityEngine;
using UnityEngine.UI; // Todavía necesitamos esto para Image

public class EnemyHealthUI : MonoBehaviour
{
    // [SerializeField] private Slider healthSlider; // <-- ELIMINA ESTA LÍNEA
    [Header("UI References")]
    [SerializeField] private Image fillImage; // <-- AÑADE ESTA LÍNEA

    [Header("References")]
    [SerializeField] private HealthManager linkedHealthManager;
    private Transform cameraTransform;

    [Header("Settings")]
    [SerializeField] private bool hideWhenFull = true;

    void Awake()
    {
        if (fillImage == null)
        {
            // Podrías intentar encontrarla por nombre si la estructura es fija
            // o asegurarte de que esté asignada en el Inspector del prefab.
            // Por ahora, asumimos que se asignará en el Inspector del prefab.
            Debug.LogError("Fill Image no asignado en EnemyHealthUI!", this);
            enabled = false;
            return;
        }

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No se encontró la cámara principal (MainCamera tag).", this);
        }
    }

    public void Setup(HealthManager healthManager)
    {
        linkedHealthManager = healthManager;
        if (linkedHealthManager == null)
        {
            Debug.LogError("Se intentó configurar EnemyHealthUI sin un HealthManager válido.", this);
            gameObject.SetActive(false);
            return;
        }

        linkedHealthManager.OnHealthChanged += UpdateUI;
        linkedHealthManager.OnDied += HandleDeath;

        InitializeUI();
        // La visibilidad inicial se maneja en InitializeUI y UpdateUI
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
        if (cameraTransform != null)
        {
            // Mantener la barra mirando a la cámara
            transform.rotation = cameraTransform.rotation;
        }
    }

    private void InitializeUI()
    {
        if (linkedHealthManager != null && fillImage != null)
        {
            // Actualizar la cantidad de llenado
            fillImage.fillAmount = linkedHealthManager.CurrentHealth / linkedHealthManager.MaxHealth;
            UpdateVisibility();
        }
    }

    private void UpdateUI(float currentHealth) // Recibe la vida actual del evento
    {
        if (fillImage != null && linkedHealthManager != null)
        {
            // Calcular la proporción y actualizar fillAmount
            fillImage.fillAmount = currentHealth / linkedHealthManager.MaxHealth;
            UpdateVisibility();
        }
    }

    private void UpdateVisibility()
    {
        if (linkedHealthManager == null) return;

        bool shouldBeActive = !hideWhenFull || linkedHealthManager.CurrentHealth < linkedHealthManager.MaxHealth;
        if (gameObject.activeSelf != shouldBeActive && !linkedHealthManager.IsDead)
        {
            gameObject.SetActive(shouldBeActive);
        }
    }

    private void HandleDeath()
    {
        gameObject.SetActive(false);
    }
}