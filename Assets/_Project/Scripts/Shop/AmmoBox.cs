using UnityEngine;

public class AmmoBox : ShopItem
{
    protected override bool PerformPurchase()
    {
        PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting == null)
        {
            Debug.LogError("PlayerShooting no encontrado!");
            return false;
        }

        // Recargar todas las armas al m�ximo
        bool ammoAdded = playerShooting.RefillAllWeaponsAmmo();

        if (ammoAdded)
        {
            Debug.Log("Toda la munici�n ha sido recargada al m�ximo!");
            return true;
        }
        else
        {
            Debug.Log("Ya tienes toda la munici�n al m�ximo.");
            return false;
        }
    }
}