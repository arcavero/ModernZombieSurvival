using UnityEngine;
using UnityEngine.AI; // Necesario para NavMeshAgent

public class EnemyInitializer : MonoBehaviour
{
    [Tooltip("Asigna el ScriptableObject EnemyData aqu� para definir las estad�sticas de este enemigo.")]
    [SerializeField] private EnemyData enemyData;

    void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyData no asignado en {gameObject.name}! No se pueden aplicar estad�sticas. El enemigo usar� valores por defecto de sus componentes si los tienen.", this);
            // Permitir que el objeto contin�e con sus valores por defecto si es posible.
            // No desactivamos 'enabled' aqu� para que otros scripts en el objeto puedan funcionar con defaults.
            // El nombre del objeto se establecer� a uno gen�rico.
            gameObject.name = $"{this.GetType().Name}_DefaultInstance_{Random.Range(1000, 9999)}";
            return; // Salir si no hay EnemyData, las funciones de abajo fallar�an.
        }

        // --- Obtener Componentes ---
        HealthManager healthManager = GetComponent<HealthManager>();
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        EnemyAI enemyAI = GetComponent<EnemyAI>(); // Asumiendo que EnemyAI est� en el mismo objeto
        Renderer enemyRenderer = GetComponentInChildren<Renderer>(); // Para cambiar material, buscar en hijos

        // --- Aplicar Estad�sticas desde EnemyData ---

        // 1. Salud (a trav�s de HealthManager)
        if (healthManager != null)
        {
            healthManager.SetInitialHealth(enemyData.maxHealth, enemyData.maxHealth);
            healthManager.SetEnemyDataReference(enemyData); // <-- �L�NEA CLAVE A�ADIDA/ASEGURADA!
        }
        else { Debug.LogWarning($"No se encontr� HealthManager en {gameObject.name} para inicializar salud.", this); }

        // 2. NavMeshAgent (Velocidad, Giro)
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = enemyData.moveSpeed;
            navMeshAgent.angularSpeed = enemyData.angularSpeed;
            // El stoppingDistance (rango de ataque efectivo) se configurar� a trav�s de EnemyAI
            // ya que EnemyAI.InitializeAttackParameters lo ajusta.
        }
        else { Debug.LogWarning($"No se encontr� NavMeshAgent en {gameObject.name} para inicializar movimiento.", this); }

        // 3. Par�metros de Ataque (a trav�s de EnemyAI)
        if (enemyAI != null)
        {
            enemyAI.InitializeAttackParameters(enemyData.attackDamage, enemyData.attackRate, enemyData.attackRange);
        }
        else { Debug.LogWarning($"No se encontr� EnemyAI en {gameObject.name} para inicializar par�metros de ataque.", this); }

        // 4. Material Visual (Opcional)
        if (enemyRenderer != null && enemyData.enemyMaterial != null)
        {
            enemyRenderer.material = enemyData.enemyMaterial;
        }
        else if (enemyRenderer != null && enemyData.enemyMaterial == null)
        {
            // Debug.LogWarning($"EnemyData ({enemyData.name}) no tiene un material asignado para {gameObject.name}. Se usar� el material existente.", this);
        }
        else if (enemyRenderer == null)
        {
            // Debug.LogWarning($"No se encontr� Renderer en los hijos de {gameObject.name} para aplicar material.", this);
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