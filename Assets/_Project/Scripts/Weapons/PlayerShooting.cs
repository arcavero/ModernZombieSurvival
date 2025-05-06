using UnityEngine;
using UnityEngine.InputSystem;

// Ya no necesitamos RequireComponent para Camera aquí
public class PlayerShooting : MonoBehaviour
{
    // Añadimos un campo para asignar la cámara en el Inspector
    [Header("Setup")]
    [SerializeField] private Camera playerCamera;

    private PlayerInputActions playerInputActions;
    private bool fireActionPerformed = false;

    void Awake()
    {
        // VALIDACIÓN IMPORTANTE: Asegurarse de que la cámara está asignada
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera no está asignada en el script PlayerShooting!", this);
            // Intentar encontrarla como hijo (común en setups FPS)
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("No se pudo encontrar una Camera en los hijos. ¡El script PlayerShooting no funcionará!", this);
                enabled = false; // Desactivar script
                return;
            }
            else
            {
                Debug.LogWarning("Player Camera no estaba asignada, se encontró automáticamente en un hijo.", this);
            }
        }

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Fire.performed += ctx => fireActionPerformed = true;
    }

    void OnEnable()
    {
        playerInputActions.Player.Fire.Enable();
    }

    void OnDisable()
    {
        playerInputActions.Player.Fire.Disable();
    }

    void Update()
    {
        // Comprobamos si la acción de disparar se realizó en este frame
        if (fireActionPerformed)
        {
            Shoot(); // Llamamos a la función de disparo

            // Reseteamos la variable para que solo dispare una vez por click
            // (Si quisiéramos fuego automático, manejaríamos esto diferente,
            // quizás comprobando playerInputActions.Player.Fire.IsPressed() aquí)
            fireActionPerformed = false;
        }
    }

    void Shoot()
    {
        Debug.Log("Disparo!"); // Podemos comentar esto si ya funciona

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            Debug.Log("Golpe detectado en: " + hitInfo.transform.name + " en el punto " + hitInfo.point);
            Debug.DrawRay(ray.origin, ray.direction * hitInfo.distance, Color.red, 1f);

            // --- INTENTAR APLICAR DAÑO ---
            // Intenta obtener el componente HealthManager del objeto golpeado o de sus padres
            // (GetComponentInParent es útil si golpeamos una hitbox hija de un enemigo).
            // Por ahora, GetComponent es suficiente si el HealthManager está en el mismo objeto que el Collider.
            HealthManager targetHealth = hitInfo.transform.GetComponent<HealthManager>();

            if (targetHealth != null) // Comprueba si el objeto golpeado TIENE un HealthManager
            {
                // ¡Sí tiene! Le aplicamos daño.
                float damageToApply = 10f; // Definir cuánto daño hace nuestro disparo
                targetHealth.TakeDamage(damageToApply);
                Debug.Log($"Aplicando {damageToApply} de daño a {hitInfo.transform.name}");
            }
            else
            {
                // El objeto golpeado no tiene componente HealthManager.
                Debug.Log($"{hitInfo.transform.name} no tiene componente HealthManager.");
            }
            // --- FIN LÓGICA DE DAÑO ---
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 1f);
        }
    }
}