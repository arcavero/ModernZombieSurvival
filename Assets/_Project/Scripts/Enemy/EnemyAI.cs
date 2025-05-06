// EnemyAI.cs
using UnityEngine;
using UnityEngine.AI; // ¡Muy importante incluir el namespace de AI!

[RequireComponent(typeof(NavMeshAgent))] // Asegura que siempre tengamos el agente
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 20f; // Rango opcional para empezar a seguir
    [SerializeField] private float attackRange = 1.5f;  // Distancia a la que se detiene (para atacar luego)
    [SerializeField] private float updateRate = 0.25f; // Con qué frecuencia actualizamos el destino (optimización)

    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 10;   // Cantidad de daño por ataque
    [SerializeField] private float attackRate = 1f;   // Segundos entre ataques
    private float timeSinceLastAttack = 0f;   // Hora del último ataque

    private NavMeshAgent agent;
    private Transform playerTransform;
    private HealthManager playerHealthManager; // Referencia al HealthManager del jugador
    private float lastUpdateTime = 0f; // Para controlar la tasa de actualización

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Buscar al jugador por Tag al inicio
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerHealthManager = playerObject.GetComponent<HealthManager>();
            if (playerHealthManager == null)
                Debug.LogError("EnemyAI: El Player no tiene HealthManager. Añade el componente HealthManager.cs.", this);
        }
        else
        {
            Debug.LogError("EnemyAI: No se pudo encontrar el GameObject del Jugador con la etiqueta 'Player'.", this);
            enabled = false; // Desactivar este script si no hay jugador
            return;
        }

        // Configurar la distancia de parada del agente
        agent.stoppingDistance = attackRange;
    }

    void Update()
    {
        // Si no tenemos referencia al jugador, no hacer nada
        if (playerTransform == null) return;

        // Optimización: Solo actualizamos la lógica de la IA cada 'updateRate' segundos
        if (Time.time - lastUpdateTime < updateRate)
            return; // Salir si no ha pasado suficiente tiempo

        lastUpdateTime = Time.time;

        // --- Lógica de Comportamiento Simple ---

        // Calcular distancia al jugador
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // --- Estado de Persecución (Chasing) ---
        if (distanceToPlayer <= detectionRange)
        {
            // Establecer el destino del agente a la posición actual del jugador
            agent.SetDestination(playerTransform.position);

            // Si el agente se ha detenido porque ha llegado cerca del jugador (dentro de stoppingDistance)
            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                // --- LÓGICA DE ATAQUE ---
                Attack();
            }
        }
    }

    void Attack()
    {
        // Comprobar cadencia de ataque
        if (Time.time < timeSinceLastAttack + attackRate)
            return;

        // Infligir daño al jugador
        if (playerHealthManager != null)
        {
            playerHealthManager.TakeDamage(attackDamage);
            // (Opcional) Aquí podrías reproducir un sonido o animación de ataque:
            // AudioSource.PlayClipAtPoint(attackSound, transform.position);
        }

        // Actualizar el tiempo del último ataque
        timeSinceLastAttack = Time.time;

        // Seguir mirando al jugador mientras ataca
        LookAtPlayer();
    }

    void LookAtPlayer()
    {
        // Hacer que el enemigo mire hacia el jugador (solo en el eje Y)
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * agent.angularSpeed / 40
        ); // Suavizar la rotación
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
