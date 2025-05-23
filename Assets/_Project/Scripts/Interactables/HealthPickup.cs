using UnityEngine;

public class HealthPickup : CollectibleItem // Hereda de CollectibleItem
{
    [Header("Health Settings")]
    [SerializeField] private float healthAmount = 25f; // Cuánta salud restaura

    protected override bool ApplyEffect(GameObject player)
    {
        HealthManager playerHealth = player.GetComponent<HealthManager>();
        if (playerHealth != null)
        {
            // Comprobar si el jugador ya tiene la salud al máximo
            if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            {
                Debug.Log("Salud del jugador ya al máximo.");
                return false; // No aplicar si ya está al máximo
            }
            playerHealth.Heal(healthAmount); // Usar el método Heal existente
            return true; // Se aplicó el efecto
        }
        return false; // No se encontró HealthManager
    }
}