using UnityEngine;

public class AmmoPickup : CollectibleItem // Hereda de CollectibleItem
{
    [Header("Ammo Settings")]
    [SerializeField] private int ammoAmount = 30; // Cu�nta munici�n da
    // Opcional: [SerializeField] private WeaponData specificWeaponType; // Si quieres que sea para un arma espec�fica

    protected override bool ApplyEffect(GameObject player)
    {
        PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
        if (playerShooting != null)
        {
            // Necesitamos un m�todo p�blico en PlayerShooting para a�adir munici�n
            // Asumamos que existe: playerShooting.AddAmmo(ammoAmount, specificWeaponType);
            // Por ahora, haremos una simplificaci�n si PlayerShooting solo maneja la munici�n del arma actual.
            // O, mejor, si PlayerShooting tiene un m�todo para a�adir a la reserva general del arma actual.

            // Versi�n simple: A�ade a la reserva del arma actual
            // Esto requiere que PlayerShooting tenga un m�todo p�blico AddReserveAmmo
            bool ammoAdded = playerShooting.AddReserveAmmoToCurrentWeapon(ammoAmount);
            if (ammoAdded)
            {
                Debug.Log($"Munici�n a�adida: {ammoAmount}");
                return true; // Se aplic� el efecto
            }
            else
            {
                Debug.Log("No se pudo a�adir munici�n (quiz�s ya al m�ximo o arma no v�lida).");
                return false; // No se aplic� si ya estaba al m�ximo
            }
        }
        return false; // No se encontr� PlayerShooting
    }
}