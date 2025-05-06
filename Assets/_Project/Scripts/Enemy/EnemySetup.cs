using UnityEngine;

// Este script se encarga de encontrar y conectar los componentes esenciales
// del enemigo cuando se inicializa (especialmente la UI de salud).
public class EnemySetup : MonoBehaviour
{
    private HealthManager healthManager;
    private EnemyHealthUI healthBarUI;

    void Awake()
    {
        // 1. Capturar el HealthManager en Awake (no llamamos aún a Setup)
        healthManager = GetComponent<HealthManager>();
        if (healthManager == null)
        {
            Debug.LogError($"EnemySetup: No se encontró HealthManager en {gameObject.name}.", this);
            enabled = false;
        }
    }

    void Start()
    {
        if (!enabled) return;

        // 2. Buscar el EnemyHealthUI en hijos (incluso si está inactivo)
        healthBarUI = GetComponentInChildren<EnemyHealthUI>(true);

        if (healthBarUI != null)
        {
            // 3. Ahora sí llamamos a Setup, una vez que Awake() de EnemyHealthUI ya corrió
            healthBarUI.Setup(healthManager);
            Debug.Log($"EnemySetup: Barra de vida configurada para {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"EnemySetup: No se encontró EnemyHealthUI en {gameObject.name}.", this);
        }
    }
}
