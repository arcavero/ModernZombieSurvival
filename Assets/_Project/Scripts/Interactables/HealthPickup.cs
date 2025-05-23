using UnityEngine;

public class HealthPickup : CollectibleItem // Hereda de CollectibleItem
{
    [Header("Health Settings")]
    [SerializeField] private float healthAmount = 25f; // Cu�nta salud restaura

    protected override bool ApplyEffect(GameObject player)
    {
        HealthManager playerHealth = player.GetComponent<HealthManager>();
        if (playerHealth != null)
        {
            // Comprobar si el jugador ya tiene la salud al m�ximo
            if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            {
                Debug.Log("Salud del jugador ya al m�ximo.");
                return false; // No aplicar si ya est� al m�ximo
            }
            playerHealth.Heal(healthAmount); // Usar el m�todo Heal existente
            return true; // Se aplic� el efecto
        }
        return false; // No se encontr� HealthManager
    }
}