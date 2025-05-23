using UnityEngine;

public class AmmoPickup : CollectibleItem // Hereda de CollectibleItem
{
    [Header("Ammo Settings")]
    [SerializeField] private int ammoAmount = 30; // Cuánta munición da
    // Opcional: [SerializeField] private WeaponData specificWeaponType; // Si quieres que sea para un arma específica

    protected override bool ApplyEffect(GameObject player)
    {
        PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
        if (playerShooting != null)
        {
            // Necesitamos un método público en PlayerShooting para añadir munición
            // Asumamos que existe: playerShooting.AddAmmo(ammoAmount, specificWeaponType);
            // Por ahora, haremos una simplificación si PlayerShooting solo maneja la munición del arma actual.
            // O, mejor, si PlayerShooting tiene un método para añadir a la reserva general del arma actual.

            // Versión simple: Añade a la reserva del arma actual
            // Esto requiere que PlayerShooting tenga un método público AddReserveAmmo
            bool ammoAdded = playerShooting.AddReserveAmmoToCurrentWeapon(ammoAmount);
            if (ammoAdded)
            {
                Debug.Log($"Munición añadida: {ammoAmount}");
                return true; // Se aplicó el efecto
            }
            else
            {
                Debug.Log("No se pudo añadir munición (quizás ya al máximo o arma no válida).");
                return false; // No se aplicó si ya estaba al máximo
            }
        }
        return false; // No se encontró PlayerShooting
    }
}