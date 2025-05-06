using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Necesario para la corutina de disparo o temporizador

// Ya no necesitamos RequireComponent para Camera aquí
public class PlayerShooting : MonoBehaviour
{
    // Añadimos un campo para asignar la cámara en el Inspector
    [Header("Setup")]
    [SerializeField] private Camera playerCamera;
    // Referencia al ScriptableObject que define el arma actual
    [SerializeField] private WeaponData currentWeaponData;

    // Variables para la cadencia de fuego
    private bool canShoot = true;
    private float nextTimeToShoot = 0f;

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

        if (currentWeaponData == null)
        {
            Debug.LogError("ERROR: No hay Weapon Data asignado en PlayerShooting!", this);
            enabled = false; // No podemos disparar sin datos de arma
            return;
        }

        playerInputActions = new PlayerInputActions();

        // --- Suscripción a Input ---
        // Para disparo simple (un clic, un disparo):
        playerInputActions.Player.Fire.performed += ctx => TryShoot();

    }

    void OnEnable()
    {
        playerInputActions.Player.Fire.Enable();
    }

    void OnDisable()
    {
        playerInputActions.Player.Fire.Disable();
    }

    // Intenta disparar, respetando la cadencia
    private void TryShoot()
    {
        // Comprobar si ha pasado suficiente tiempo desde el último disparo
        if (Time.time >= nextTimeToShoot)
        {
            // Actualizar el tiempo para el próximo disparo permitido
            nextTimeToShoot = Time.time + currentWeaponData.fireRate;
            // Realizar el disparo
            Shoot();
        }
        // else { Debug.Log("Cooldown..."); } // Opcional: Log si intenta disparar muy rápido
    }

    // Ejecuta la lógica del disparo usando los datos del arma actual
    private void Shoot()
    {
        // Debug.Log($"Disparando {currentWeaponData.weaponName}!");

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitInfo;

        // Usar el rango del WeaponData
        if (Physics.Raycast(ray, out hitInfo, currentWeaponData.range))
        {
            // Debug.Log($"Golpe: {hitInfo.transform.name}"); // Log más corto
            Debug.DrawRay(ray.origin, ray.direction * hitInfo.distance, Color.red, 0.1f); // Duración más corta

            HealthManager targetHealth = hitInfo.transform.GetComponent<HealthManager>();
            if (targetHealth != null)
            {
                // Usar el daño del WeaponData
                targetHealth.TakeDamage(currentWeaponData.damage);
                // Debug.Log($"Daño {currentWeaponData.damage} aplicado a {hitInfo.transform.name}");
            }
        }
        else
        {
            // Visualizar hasta el rango máximo si no golpea nada
            Debug.DrawRay(ray.origin, ray.direction * currentWeaponData.range, Color.green, 0.1f);
        }

        // --- Aquí irían efectos de sonido/visuales basados en WeaponData ---
        // if(currentWeaponData.fireSound != null) AudioSource.PlayClipAtPoint(currentWeaponData.fireSound, transform.position);
        // if(currentWeaponData.muzzleFlashPrefab != null) Instantiate(currentWeaponData.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
    }
}