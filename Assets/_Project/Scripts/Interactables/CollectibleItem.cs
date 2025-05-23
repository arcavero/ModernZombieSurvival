// Assets/_Project/Scripts/Interactables/CollectibleItem.cs
using UnityEngine;

public abstract class CollectibleItem : MonoBehaviour
{
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffectPrefab; // Opcional, para un efecto visual al recoger
    [SerializeField] private bool destroyOnPickup = true;

    // OnTriggerEnter se llama cuando otro Collider entra en el trigger
    private void OnTriggerEnter(Collider other)
    {
        // Comprobar si el objeto que entr� es el jugador (usando su Tag)
        if (other.CompareTag("Player"))
        {
            // Intentar aplicar el efecto del coleccionable
            if (ApplyEffect(other.gameObject)) // Pasar el GameObject del jugador
            {
                PlayFeedback();
                if (destroyOnPickup)
                {
                    Destroy(gameObject); // Destruir el objeto recogible
                }
                else
                {
                    // Podr�as desactivarlo o manejarlo de otra forma si no se destruye
                    gameObject.SetActive(false);
                    // Podr�as a�adir una corutina para reactivarlo despu�s de un tiempo
                }
            }
        }
    }

    // M�todo abstracto que las clases hijas DEBEN implementar
    // Devuelve true si el efecto se aplic� con �xito (ej. si el jugador no estaba ya al m�ximo)
    protected abstract bool ApplyEffect(GameObject player);

    private void PlayFeedback()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }
        Debug.Log($"Item {gameObject.name} recogido por el jugador.");
    }
}