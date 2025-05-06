using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Necesario para la corutina de disparo o temporizador
using TMPro;                    // <- ¡Asegúrate de tener esto!

// Ya no necesitamos RequireComponent para Camera aquí
public class PlayerShooting : MonoBehaviour
{
    // Añadimos un campo para asignar la cámara en el Inspector
    [Header("Setup")]
    [SerializeField] private Camera playerCamera;
    // Referencia al ScriptableObject que define el arma actual
    [SerializeField] private WeaponData currentWeaponData;

    [Header("UI")]
    // Asigna aquí el TextMeshPro de la munición
    [SerializeField] private TextMeshProUGUI ammoTextElement;

    // Estado interno del arma
    private int currentAmmoInMagazine;
    private int currentTotalAmmo; // Balas en reserva
    private bool isReloading = false;

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

        if (ammoTextElement == null)
        {
            Debug.LogWarning("Ammo Text Element no asignado en PlayerShooting. La UI no se actualizará.", this);
        }

        playerInputActions = new PlayerInputActions();

        // --- Suscripción a Input ---
        // Para disparo simple (un clic, un disparo):
        playerInputActions.Player.Fire.performed += ctx => TryShoot();
        // Nueva suscripción para recargar
        playerInputActions.Player.Reload.performed += ctx => StartReload();

    }

    void Start()
    {
        // Inicializar munición al empezar (o al cambiar de arma)
        currentAmmoInMagazine = currentWeaponData.magazineSize;
        currentTotalAmmo = currentWeaponData.maxTotalAmmo;
        UpdateAmmoUI();
        isReloading = false; // Asegurarse de no empezar recargando
    }

    void OnEnable()
    {
        playerInputActions.Player.Fire.Enable();
        playerInputActions.Player.Reload.Enable(); // Activar acción de recarga
        isReloading = false; // Resetear estado si se reactiva el componente
    }

    void OnDisable()
    {
        playerInputActions.Player.Fire.Disable();
        playerInputActions.Player.Reload.Disable(); // Desactivar acción de recarga
    }

    // Intenta disparar, respetando la cadencia
    private void TryShoot()
    {
        // No disparar si está recargando o si no ha pasado el tiempo de cadencia
        if (isReloading || Time.time < nextTimeToShoot)
        {
            return;
        }

        // Comprobar si hay balas en el cargador
        if (currentAmmoInMagazine <= 0)
        {
            Debug.Log("¡Sin munición! Necesitas recargar.");
            // Podríamos iniciar la recarga automáticamente aquí o reproducir un sonido de "clic vacío"
            // StartReload(); // <-- Opcional: Recarga automática al intentar disparar vacío
            return;
        }

        // Actualizar el tiempo para el próximo disparo permitido
        nextTimeToShoot = Time.time + currentWeaponData.fireRate;

        // Realizar el disparo
        Shoot();

        // Reducir munición y actualizar UI
        currentAmmoInMagazine--;
        UpdateAmmoUI();
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

    // Inicia el proceso de recarga si es posible
    private void StartReload()
    {
        // No recargar si ya estamos recargando, si el cargador está lleno o si no hay munición de reserva
        if (isReloading || currentAmmoInMagazine == currentWeaponData.magazineSize || currentTotalAmmo <= 0)
        {
            return;
        }

        // Iniciar la corutina de recarga
        StartCoroutine(ReloadCoroutine());
    }

    // Corutina que maneja el tiempo de recarga
    private IEnumerator ReloadCoroutine()
    {
        Debug.Log("Recargando...");
        isReloading = true;
        // Aquí podrías reproducir sonido de inicio de recarga o animación

        // Esperar el tiempo definido en WeaponData
        yield return new WaitForSeconds(currentWeaponData.reloadTime);

        // Calcular cuántas balas necesitamos y cuántas tenemos disponibles
        int ammoNeeded = currentWeaponData.magazineSize - currentAmmoInMagazine;
        int ammoToTransfer = Mathf.Min(ammoNeeded, currentTotalAmmo); // Tomar lo necesario o lo que quede

        // Transferir munición
        currentAmmoInMagazine += ammoToTransfer;
        currentTotalAmmo -= ammoToTransfer;

        // Finalizar estado de recarga
        isReloading = false;
        UpdateAmmoUI();
        Debug.Log("¡Recarga completa!");
        // Aquí podrías reproducir sonido de fin de recarga
    }

    // Actualiza el elemento TextMeshPro con la munición actual
    private void UpdateAmmoUI()
    {
        if (ammoTextElement != null)
        {
            ammoTextElement.text = $"AMMO: {currentAmmoInMagazine} / {currentTotalAmmo}";
        }
    }
}