using UnityEngine;
using UnityEngine.AI; // Necesario para NavMeshAgent

public class EnemyInitializer : MonoBehaviour
{
    [Tooltip("Asigna el ScriptableObject EnemyData aquí para definir las estadísticas de este enemigo.")]
    [SerializeField] private EnemyData enemyData;

    void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyData no asignado en {gameObject.name}! No se pueden aplicar estadísticas. El enemigo usará valores por defecto de sus componentes si los tienen.", this);
            // Permitir que el objeto continúe con sus valores por defecto si es posible.
            // No desactivamos 'enabled' aquí para que otros scripts en el objeto puedan funcionar con defaults.
            // El nombre del objeto se establecerá a uno genérico.
            gameObject.name = $"{this.GetType().Name}_DefaultInstance_{Random.Range(1000, 9999)}";
            return; // Salir si no hay EnemyData, las funciones de abajo fallarían.
        }

        // --- Obtener Componentes ---
        HealthManager healthManager = GetComponent<HealthManager>();
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        EnemyAI enemyAI = GetComponent<EnemyAI>(); // Asumiendo que EnemyAI está en el mismo objeto
        Renderer enemyRenderer = GetComponentInChildren<Renderer>(); // Para cambiar material, buscar en hijos

        // --- Aplicar Estadísticas desde EnemyData ---

        // 1. Salud (a través de HealthManager)
        if (healthManager != null)
        {
            healthManager.SetInitialHealth(enemyData.maxHealth, enemyData.maxHealth);
            healthManager.SetEnemyDataReference(enemyData); // <-- ¡LÍNEA CLAVE AÑADIDA/ASEGURADA!
        }
        else { Debug.LogWarning($"No se encontró HealthManager en {gameObject.name} para inicializar salud.", this); }

        // 2. NavMeshAgent (Velocidad, Giro)
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = enemyData.moveSpeed;
            navMeshAgent.angularSpeed = enemyData.angularSpeed;
            // El stoppingDistance (rango de ataque efectivo) se configurará a través de EnemyAI
            // ya que EnemyAI.InitializeAttackParameters lo ajusta.
        }
        else { Debug.LogWarning($"No se encontró NavMeshAgent en {gameObject.name} para inicializar movimiento.", this); }

        // 3. Parámetros de Ataque (a través de EnemyAI)
        if (enemyAI != null)
        {
            enemyAI.InitializeAttackParameters(enemyData.attackDamage, enemyData.attackRate, enemyData.attackRange);
        }
        else { Debug.LogWarning($"No se encontró EnemyAI en {gameObject.name} para inicializar parámetros de ataque.", this); }

        // 4. Material Visual (Opcional)
        if (enemyRenderer != null && enemyData.enemyMaterial != null)
        {
            enemyRenderer.material = enemyData.enemyMaterial;
        }
        else if (enemyRenderer != null && enemyData.enemyMaterial == null)
        {
            // Debug.LogWarning($"EnemyData ({enemyData.name}) no tiene un material asignado para {gameObject.name}. Se usará el material existente.", this);
        }
        else if (enemyRenderer == null)
        {
            // Debug.LogWarning($"No se encontró Renderer en los hijos de {gameObject.name} para aplicar material.", this);
        }


        // 5. Nombre del GameObject (para debugging en la Hierarchy)
        if (!string.IsNullOrEmpty(enemyData.name))
        {
            gameObject.name = $"{enemyData.name}_Instance_{Random.Range(1000, 9999)}";
        }
        else // Fallback si EnemyData no tiene nombre
        {
            gameObject.name = $"Enemy_FromInitializer_Instance_{Random.Range(1000, 9999)}";
        }

        Debug.Log($"{gameObject.name} inicializado con datos de {(enemyData.name ?? "Unnamed EnemyData")}.");
    }
}