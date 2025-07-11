using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static CurrencyManager Instance { get; private set; }

    [Header("Initial Settings")]
    [SerializeField] private int startingCurrency = 0;

    private int currentPlayerCurrency;

    public event Action<int> OnCurrencyChanged; // Evento para notificar a la UI, etc.

    public int CurrentPlayerCurrency => currentPlayerCurrency;

    void Awake()
    {
        // --- Singleton Setup ---
        if (Instance == null)
        {
            Instance = this;
            // Opcional: si quieres que persista entre cargas de escena
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Debug.LogWarning("CurrencyManager: Ya existe una instancia. Destruyendo esta.", this);
            Destroy(gameObject);
            return; // Importante salir aqu� para no ejecutar el resto del Awake en la instancia duplicada
        }

        currentPlayerCurrency = startingCurrency;
    }

    void Start()
    {
        // Notificar el valor inicial a cualquier UI que est� escuchando
        // Es bueno hacerlo en Start para dar tiempo a otras UI a suscribirse en su Awake.
        OnCurrencyChanged?.Invoke(currentPlayerCurrency);
    }

    public void AddCurrency(int amount)
    {
        if (amount <= 0)
        {
            // Debug.LogWarning("Se intent� a�adir una cantidad no positiva o cero de moneda.", this); // Podr�a ser v�lido querer a�adir 0
            return;
        }
        currentPlayerCurrency += amount;
        OnCurrencyChanged?.Invoke(currentPlayerCurrency);
        Debug.Log($"Moneda a�adida: {amount}. Total actual: {currentPlayerCurrency}");
    }

    public int GetCurrentCurrency()
    {
        return currentPlayerCurrency;
    }

    public bool SpendCurrency(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Se intent� gastar una cantidad no positiva o cero de moneda.", this);
            return false;
        }

        if (currentPlayerCurrency >= amount)
        {
            currentPlayerCurrency -= amount;
            OnCurrencyChanged?.Invoke(currentPlayerCurrency);
            Debug.Log($"Moneda gastada: {amount}. Total actual: {currentPlayerCurrency}");
            return true; // Transacci�n exitosa
        }
        else
        {
            Debug.Log("Fondos insuficientes para gastar " + amount + ". Actual: " + currentPlayerCurrency);
            return false; // Fondos insuficientes
        }
    }
}