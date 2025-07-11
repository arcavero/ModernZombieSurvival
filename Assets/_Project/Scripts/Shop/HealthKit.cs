using UnityEngine;

public class HealthKit : ShopItem
{
    protected override bool PerformPurchase()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player no encontrado!");
            return false;
        }

        HealthManager playerHealth = player.GetComponent<HealthManager>();
        if (playerHealth == null)
        {
            Debug.LogError("HealthManager del jugador no encontrado!");
            return false;
        }

        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
        {
            Debug.Log("Ya tienes la salud al m�ximo.");
            return false;
        }

        playerHealth.Heal(playerHealth.MaxHealth); // Curar al m�ximo
        Debug.Log("�Salud restaurada al m�ximo!");
        return true;
    }
}