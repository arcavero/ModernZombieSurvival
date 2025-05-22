using UnityEngine;
using TMPro; // Necesario para TextMeshProUGUI

public class CurrencyUIUpdater : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Arrastra aquí el TextMeshProUGUI que mostrará la moneda.")]
    [SerializeField] private TextMeshProUGUI currencyTextElement;

    void Awake()
    {
        // Validación
        if (currencyTextElement == null)
        {
            Debug.LogError("Currency Text Element no asignado en CurrencyUIUpdater!", this);
            enabled = false; // Desactivar si no hay UI que actualizar
            return;
        }
    }

    void Start()
    {
        // Suscribirse al evento OnCurrencyChanged del CurrencyManager
        // Asegurarse de que CurrencyManager.Instance ya exista (normalmente sí en Start)
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyText;
            // Actualizar el texto una vez al inicio con el valor inicial
            UpdateCurrencyText(CurrencyManager.Instance.CurrentPlayerCurrency);
        }
        else
        {
            Debug.LogError("CurrencyManager.Instance no encontrado en Start de CurrencyUIUpdater. La UI de moneda no se actualizará.", this);
            // Podrías intentar suscribirte en Update hasta que esté disponible, pero es menos ideal.
        }
    }

    void OnDestroy()
    {
        // ¡MUY IMPORTANTE! Desuscribirse del evento cuando este objeto se destruya
        // para evitar errores y fugas de memoria.
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrencyText;
        }
    }

    // Esta función será llamada automáticamente por el evento OnCurrencyChanged
    private void UpdateCurrencyText(int newCurrencyValue)
    {
        if (currencyTextElement != null)
        {
            currencyTextElement.text = $"PUNTOS: {newCurrencyValue}";
            // O el formato que prefieras, ej: "$ " + newCurrencyValue
        }
    }
}