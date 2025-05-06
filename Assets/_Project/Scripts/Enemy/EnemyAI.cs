using UnityEngine;
using UnityEngine.AI; // ¡Muy importante incluir el namespace de AI!

[RequireComponent(typeof(NavMeshAgent))] // Asegura que siempre tengamos el agente
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 20f; // Rango opcional para empezar a seguir
    [SerializeField] private float attackRange = 1.5f;  // Distancia a la que se detiene (para atacar luego)
    [SerializeField] private float updateRate = 0.25f; // Con qué frecuencia actualizamos el destino (optimización)

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float lastUpdateTime = 0f; // Para controlar la tasa de actualización

    // Podríamos añadir estados más adelante (Patrolling, Chasing, Attacking)
    // private enum AIState { Patrolling, Chasing, Attacking }
    // private AIState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Buscar al jugador por Tag al inicio
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("EnemyAI: No se pudo encontrar el GameObject del Jugador con la etiqueta 'Player'.", this);
            enabled = false; // Desactivar este script si no hay jugador
            return;
        }

        // Configurar la distancia de parada del agente
        agent.stoppingDistance = attackRange;
        // currentState = AIState.Patrolling; // Estado inicial (si tuviéramos patrulla)
    }

    void Update()
    {
        // Si no tenemos referencia al jugador, no hacer nada
        if (playerTransform == null) return;

        // Optimización: Solo actualizamos la lógica de la IA cada 'updateRate' segundos
        if (Time.time - lastUpdateTime < updateRate)
        {
            return; // Salir si no ha pasado suficiente tiempo
        }
        lastUpdateTime = Time.time;


        // --- Lógica de Comportamiento Simple ---

        // Calcular distancia al jugador
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // --- Estado de Persecución (Chasing) ---
        // Si el jugador está dentro del rango de detección (o siempre lo perseguimos por ahora)
        if (distanceToPlayer <= detectionRange)
        {
            // Establecer el destino del agente a la posición actual del jugador
            // NavMeshAgent calculará la ruta automáticamente
            agent.SetDestination(playerTransform.position);

            // Si el agente se ha detenido porque ha llegado cerca del jugador (dentro de stoppingDistance)
            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                // --- Aquí iría la LÓGICA DE ATAQUE ---
                // Debug.Log("Enemy en rango de ataque!");
                // Podríamos llamar a una función Attack()
                // Por ahora, podríamos simplemente hacer que mire al jugador
                LookAtPlayer();
            }
        }
        // --- (Opcional) Estado de Patrulla (Patrolling) ---
        // else
        // {
        // Si el jugador está fuera de rango, podríamos hacer que patrulle
        // if (currentState != AIState.Patrolling) {
        //    currentState = AIState.Patrolling;
        //    // Buscar un punto de patrulla aleatorio, etc.
        // }
        // Patrol();
        // }
    }

    void LookAtPlayer()
    {
        // Hacer que el enemigo mire hacia el jugador (solo en el eje Y)
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed / 40); // Usamos Slerp para suavizar la rotación
    }

    // --- (Opcional) Visualización del Rango en el Editor ---
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}