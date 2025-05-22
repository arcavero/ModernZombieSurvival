using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; // Necesario para sonidos de idle si los tienes

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))] // Para sonidos de idle/ataque del enemigo
public class EnemyAI : MonoBehaviour
{
    [Header("AI Detection Settings")]
    [SerializeField] private float detectionRange = 20f;
    [Tooltip("Con qué frecuencia la IA actualiza su lógica de detección/movimiento (segundos).")]
    [SerializeField] private float aiUpdateRate = 0.25f;

    // --- MODIFICADO: Los valores de ataque ahora se pueden configurar desde fuera ---
    // Se usarán como defaults si EnemyInitializer no los sobrescribe.
    [Header("Default Attack Settings (Sobrescritos por EnemyData si se usa EnemyInitializer)")]
    [SerializeField] private int defaultAttackDamage = 10;
    [SerializeField] private float defaultAttackRate = 1f;
    [SerializeField] private float defaultAttackRange = 1.5f; // NavMeshAgent.stoppingDistance se ajustará a esto

    private int currentAttackDamage;
    private float currentAttackRate;
    // currentAttackRange se refleja en agent.stoppingDistance

    [Header("Audio Settings")]
    [SerializeField] private List<AudioClip> idleGrunts;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private float minTimeBetweenGrunts = 3f;
    [SerializeField] private float maxTimeBetweenGrunts = 7f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private HealthManager playerHealthManager;
    private AudioSource audioSource;

    private float timeSinceLastAttack = 0f;
    private float nextAiUpdateTime = 0f;
    private float nextGruntTime = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerHealthManager = playerObject.GetComponent<HealthManager>();
            if (playerHealthManager == null) Debug.LogError("EnemyAI: Player no tiene HealthManager.", this);
        }
        else
        {
            Debug.LogError("EnemyAI: No se encontró el Player.", this);
            enabled = false; return;
        }

        // --- MODIFICADO: Inicializar valores de ataque con los defaults ---
        // EnemyInitializer los sobrescribirá si está presente y tiene un EnemyData.
        InitializeAttackParameters(defaultAttackDamage, defaultAttackRate, defaultAttackRange);

        ScheduleNextGrunt();
    }

    // --- NUEVO: Método público para que EnemyInitializer configure los parámetros de ataque ---
    public void InitializeAttackParameters(int damage, float rate, float range)
    {
        currentAttackDamage = damage;
        currentAttackRate = rate;
        // El rango de ataque se establece en el NavMeshAgent.stoppingDistance
        if (agent != null)
        {
            agent.stoppingDistance = range;
        }
        Debug.Log($"{gameObject.name} AI params: Dmg={currentAttackDamage}, Rate={currentAttackRate}, Range(StopDist)={range}");
    }


    void Update()
    {
        if (playerTransform == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        if (Time.time < nextAiUpdateTime) return;
        nextAiUpdateTime = Time.time + aiUpdateRate;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            agent.SetDestination(playerTransform.position);

            // Comprobar si está en rango de ataque (usando stoppingDistance que refleja currentAttackRange)
            // agent.remainingDistance puede ser un poco inestable, así que también comprobamos distanceToPlayer
            if (distanceToPlayer <= agent.stoppingDistance && !agent.pathPending)
            {
                Attack();
            }
        }
        // else { /* Lógica de Patrulla Futura */ }

        HandleIdleGrunts();
    }

    void Attack()
    {
        if (Time.time < timeSinceLastAttack + currentAttackRate) return; // Usar currentAttackRate

        LookAtPlayer(); // Mirar primero, luego intentar atacar

        // Asegurar que sigamos en rango después de girar (opcional, pero puede ser bueno)
        if (Vector3.Distance(transform.position, playerTransform.position) <= agent.stoppingDistance + 0.1f) // Pequeño umbral
        {
            if (attackSound != null && audioSource != null && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(attackSound);
            }

            if (playerHealthManager != null)
            {
                playerHealthManager.TakeDamage(currentAttackDamage); // Usar currentAttackDamage
            }
            timeSinceLastAttack = Time.time;
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        // Para rotación instantánea: transform.rotation = lookRotation;
        // Para rotación suave (asegúrate de que agent.angularSpeed tenga un valor razonable):
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, agent.angularSpeed * Time.deltaTime);
    }

    void HandleIdleGrunts()
    {
        if (Time.time >= nextGruntTime && audioSource != null && !audioSource.isPlaying && idleGrunts.Count > 0)
        {
            int randomIndex = Random.Range(0, idleGrunts.Count);
            audioSource.PlayOneShot(idleGrunts[randomIndex]);
            ScheduleNextGrunt();
        }
    }

    void ScheduleNextGrunt()
    {
        nextGruntTime = Time.time + Random.Range(minTimeBetweenGrunts, maxTimeBetweenGrunts);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        // Dibujar usando el stoppingDistance actual del agente, que refleja el attackRange
        if (agent != null) Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
        else Gizmos.DrawWireSphere(transform.position, defaultAttackRange); // Fallback si el agente no está listo
    }
}