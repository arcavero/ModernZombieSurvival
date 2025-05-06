using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Necesario para List

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("El prefab del enemigo que se va a instanciar.")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("Lista de Transforms donde los enemigos pueden aparecer.")]
    [SerializeField] private List<Transform> spawnPoints;

    [Header("Spawning Settings")]
    [Tooltip("Tiempo en segundos antes de que empiece el primer spawn.")]
    [SerializeField] private float initialSpawnDelay = 3f;
    [Tooltip("Tiempo en segundos entre cada intento de spawn.")]
    [SerializeField] private float timeBetweenSpawns = 5f;
    [Tooltip("Cuántos enemigos intentar generar en cada intervalo de spawn.")]
    [SerializeField] private int enemiesPerSpawnWave = 1;

    [Header("Limits")]
    [Tooltip("Número máximo de enemigos activos en la escena a la vez.")]
    [SerializeField] private int maxActiveEnemies = 10;

    // Contador interno para saber cuántos enemigos están activos
    private int currentActiveEnemies = 0;
    // Variable para guardar la referencia a la corutina
    private Coroutine spawnCoroutine;

    void Start()
    {
        // --- Validación de configuración ---
        if (enemyPrefab == null)
        {
            Debug.LogError("ERROR: Enemy Prefab no está asignado en el EnemySpawner!", this);
            enabled = false; // Desactivar el spawner si falta el prefab
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("ERROR: No se han asignado Spawn Points en el EnemySpawner!", this);
            enabled = false; // Desactivar si no hay dónde spawnear
            return;
        }

        // Asegurarse de que el contador empieza en 0
        currentActiveEnemies = 0;

        // Iniciar la rutina de spawneo
        spawnCoroutine = StartCoroutine(SpawnRoutine());
        Debug.Log("Enemy Spawner iniciado.");
    }

    // Se llama cuando este componente se desactiva o destruye
    void OnDisable()
    {
        // Detener la corutina si el spawner se desactiva para evitar errores
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            Debug.Log("Enemy Spawner detenido.");
        }
    }


    private IEnumerator SpawnRoutine()
    {
        // Esperar el retraso inicial antes de empezar
        yield return new WaitForSeconds(initialSpawnDelay);

        // Bucle principal del spawner
        while (true) // Se ejecutará mientras el spawner esté activo
        {
            // Intentar spawnear una oleada
            TrySpawnWave();

            // Esperar el tiempo definido antes del siguiente intento
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private void TrySpawnWave()
    {
        // Intentar generar el número de enemigos por oleada
        for (int i = 0; i < enemiesPerSpawnWave; i++)
        {
            // Comprobar si ya hemos alcanzado el límite
            if (currentActiveEnemies >= maxActiveEnemies)
            {
                // Debug.Log("Límite máximo de enemigos alcanzado, esperando a que mueran...");
                return; // No spawnear más en esta oleada si estamos al límite
            }

            // Proceder a spawnear un enemigo
            SpawnSingleEnemy();
        }
    }

    private void SpawnSingleEnemy()
    {
        // 1. Elegir un punto de spawn aleatorio
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform chosenSpawnPoint = spawnPoints[randomIndex];

        if (chosenSpawnPoint == null)
        {
            Debug.LogError("ERROR: Uno de los spawn points en la lista es nulo!", this);
            return; // Evitar error si un punto es nulo
        }

        // 2. Instanciar el prefab del enemigo
        GameObject newEnemyGO = Instantiate(enemyPrefab, chosenSpawnPoint.position, chosenSpawnPoint.rotation);
        newEnemyGO.name = enemyPrefab.name + "_" + Time.time; // Darle un nombre único (opcional)

        // 3. Obtener su HealthManager para suscribirse al evento de muerte
        HealthManager enemyHealth = newEnemyGO.GetComponent<HealthManager>();
        if (enemyHealth != null)
        {
            // Cuando este enemigo específico muera, llamará a HandleEnemyDeath
            enemyHealth.OnDied += HandleEnemyDeath;
        }
        else
        {
            // Si no hay HealthManager, no podemos rastrear su muerte para el contador.
            // Esto podría ser un problema o intencional si algunos prefabs no tienen vida.
            Debug.LogWarning($"Enemigo spawneado ({newEnemyGO.name}) no tiene HealthManager. No se rastreará su muerte.", newEnemyGO);
        }

        // 4. Incrementar el contador de enemigos activos
        currentActiveEnemies++;
        // Debug.Log($"Enemigo spawneado. Activos: {currentActiveEnemies}/{maxActiveEnemies}");
    }

    // Esta función se suscribe al evento OnDied de cada HealthManager enemigo
    private void HandleEnemyDeath()
    {
        // Decrementar el contador cuando un enemigo muere
        currentActiveEnemies--;
        currentActiveEnemies = Mathf.Max(0, currentActiveEnemies); // Asegurar que no baje de 0
        // Debug.Log($"Un enemigo murió. Activos: {currentActiveEnemies}/{maxActiveEnemies}");

        // Importante: No necesitamos desuscribirnos manualmente aquí (`enemyHealth.OnDied -= HandleEnemyDeath;`)
        // porque el evento lo lanza el enemigo que está muriendo (y presumiblemente
        // será destruido o desactivado), lo que rompe la referencia al evento automáticamente.
        // Si el enemigo *no* se destruyera, necesitaríamos una forma de pasar la referencia
        // del HealthManager a esta función para poder desuscribirnos.
    }
}