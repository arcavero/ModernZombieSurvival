using UnityEngine;
using UnityEngine.UI; // Necesario para Image
using System.Collections; // Necesario para Coroutines

public class PlayerDamageFlash : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image damageFlashImage; // Asigna tu panel rojo aqu�

    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.15f; // Duraci�n del destello
    [SerializeField] private float maxAlpha = 0.5f;     // Qu� tan opaco se vuelve el destello

    [Header("Player Reference")]
    [SerializeField] private HealthManager playerHealthManager;

    private Coroutine currentFlashRoutine = null;

    void Awake()
    {
        // --- Validaci�n ---
        if (damageFlashImage == null)
        {
            Debug.LogError("Damage Flash Image no asignado en PlayerDamageFlash!", this);
            enabled = false; return;
        }
        if (playerHealthManager == null)
        {
            Debug.LogError("Player Health Manager no asignado en PlayerDamageFlash! Intentando encontrar Player...", this);
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealthManager = player.GetComponent<HealthManager>();
                if (playerHealthManager == null)
                {
                    Debug.LogError("Player encontrado, pero no tiene HealthManager!", this);
                    enabled = false; return;
                }
            }
            else
            {
                Debug.LogError("No se pudo encontrar el Player.", this);
                enabled = false; return;
            }
        }

        // Asegurarse de que el alfa est� a 0 al inicio
        Color startColor = damageFlashImage.color;
        startColor.a = 0f;
        damageFlashImage.color = startColor;

        // Suscribirse al evento OnHealthChanged (podr�amos usar OnDamaged si tuvi�ramos uno espec�fico)
        playerHealthManager.OnHealthChanged += HandleDamageTaken;
    }

    void OnDestroy()
    {
        if (playerHealthManager != null)
        {
            playerHealthManager.OnHealthChanged -= HandleDamageTaken;
        }
    }

    // Esta funci�n se llamar� cada vez que cambie la salud del jugador.
    // Necesitamos comprobar si la salud *disminuy�* para considerarlo da�o.
    // (HealthManager no tiene un evento OnDamaged separado, as� que lo inferimos)
    private float previousHealth;
    void Start() // Usamos Start para asegurar que Awake de HealthManager ya se haya ejecutado
    {
        if (playerHealthManager != null)
        {
            previousHealth = playerHealthManager.CurrentHealth;
        }
    }

    private void HandleDamageTaken(float newHealth)
    {
        if (playerHealthManager == null) return;

        // Solo activar el flash si la salud disminuy�
        if (newHealth < previousHealth)
        {
            TriggerFlash();
        }
        previousHealth = newHealth;
    }


    public void TriggerFlash()
    {
        if (damageFlashImage == null) return;

        // Si ya hay un destello en curso, lo detenemos para empezar uno nuevo
        if (currentFlashRoutine != null)
        {
            StopCoroutine(currentFlashRoutine);
        }
        currentFlashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Fase de aparici�n (Fade In)
        float elapsedTime = 0f;
        Color flashColor = damageFlashImage.color;

        while (elapsedTime < flashDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            flashColor.a = Mathf.Lerp(0f, maxAlpha, elapsedTime / (flashDuration / 2));
            damageFlashImage.color = flashColor;
            yield return null;
        }

        // Fase de desaparici�n (Fade Out)
        elapsedTime = 0f; // Resetear para la segunda mitad
        while (elapsedTime < flashDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            flashColor.a = Mathf.Lerp(maxAlpha, 0f, elapsedTime / (flashDuration / 2));
            damageFlashImage.color = flashColor;
            yield return null;
        }

        // Asegurar que el alfa sea 0 al final
        flashColor.a = 0f;
        damageFlashImage.color = flashColor;
        currentFlashRoutine = null; // Marcar que la rutina termin�
    }
}