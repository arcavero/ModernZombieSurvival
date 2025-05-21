using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string name;
        public int numberOfEnemies;
        public float timeBetweenIndividualSpawns = 1f;
    }

    [Header("References")]
    [SerializeField] private GameObject defaultEnemyPrefab;
    [SerializeField] private Transform spawnPointsContainer;

    [Header("Wave Settings")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float timeBetweenWaves = 5f;
    private int currentWaveIndex = 0;

    private int enemiesToSpawnInCurrentWave;
    private int enemiesAliveFromCurrentWave;

    [Header("UI Feedback (Optional)")]
    [SerializeField] private TextMeshProUGUI waveTextElement;
    [SerializeField] private TextMeshProUGUI enemiesRemainingTextElement;

    private List<Transform> activeSpawnPoints = new List<Transform>();
    private Coroutine waveSpawnCoroutine;

    void Awake()
    {
        // Validaciones...
        if (defaultEnemyPrefab == null)
        {
            Debug.LogError("ERROR: Default Enemy Prefab no está asignado!", this);
            enabled = false;
            return;
        }
        if (spawnPointsContainer == null)
        {
            Debug.LogError("ERROR: Spawn Points Container no está asignado!", this);
            enabled = false;
            return;
        }
        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("ERROR: No hay oleadas definidas!", this);
            enabled = false;
            return;
        }

        // Poblar puntos de spawn...
        PopulateSpawnPointsFromContainer();
        if (activeSpawnPoints.Count == 0)
        {
            Debug.LogError("ERROR: No hay puntos de spawn activos!", this);
            enabled = false;
        }
    }

    void Start()
    {
        waveSpawnCoroutine = StartCoroutine(WaveSpawnRoutine());
    }

    void OnDisable()
    {
        if (waveSpawnCoroutine != null)
            StopCoroutine(waveSpawnCoroutine);
    }

    private void PopulateSpawnPointsFromContainer()
    {
        activeSpawnPoints.Clear();
        foreach (Transform child in spawnPointsContainer)
            if (child.gameObject.activeSelf)
                activeSpawnPoints.Add(child);

        Debug.Log($"Encontrados {activeSpawnPoints.Count} puntos de spawn activos.");
    }

    private IEnumerator WaveSpawnRoutine()
    {
        // Delay inicial antes de la primera oleada
        yield return new WaitForSeconds(timeBetweenWaves);

        while (currentWaveIndex < waves.Length)
        {
            Wave currentWave = waves[currentWaveIndex];
            enemiesToSpawnInCurrentWave = currentWave.numberOfEnemies;
            enemiesAliveFromCurrentWave = 0;
            UpdateWaveUI();

            Debug.Log($"-- Iniciando Oleada {currentWaveIndex + 1} --");

            // Spawneo
            for (int i = 0; i < currentWave.numberOfEnemies; i++)
            {
                SpawnSingleEnemyForCurrentWave();
                enemiesToSpawnInCurrentWave--;

                if (i < currentWave.numberOfEnemies - 1)
                    yield return new WaitForSeconds(currentWave.timeBetweenIndividualSpawns);
            }

            // Esperar hasta que todos los vivos mueran
            while (enemiesAliveFromCurrentWave > 0)
                yield return null;

            Debug.Log($"-- Oleada {currentWaveIndex + 1} completada --");
            currentWaveIndex++;

            if (currentWaveIndex < waves.Length)
            {
                UpdateWaveUI(true);
                yield return new WaitForSeconds(timeBetweenWaves);
            }
            else
            {
                UpdateWaveUI(false, true);
                enabled = false;
            }
        }
    }

    private void SpawnSingleEnemyForCurrentWave()
    {
        if (activeSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No hay puntos de spawn activos.");
            return;
        }

        var spawnPt = activeSpawnPoints[Random.Range(0, activeSpawnPoints.Count)];
        GameObject newEnemyGO = Instantiate(defaultEnemyPrefab, spawnPt.position, spawnPt.rotation);

        // ** Asegurarnos de que esté activo**
        if (!newEnemyGO.activeSelf)
        {
            newEnemyGO.SetActive(true);
            Debug.LogWarning($"Enemy instanciado en inactivo, activando durante spawn.");
        }

        newEnemyGO.name = defaultEnemyPrefab.name + "_Wave" + (currentWaveIndex + 1) + "_" + Time.frameCount;
        Debug.Log($"[Spawner] Spawned enemy at {spawnPt.position}");
        Debug.DrawRay(spawnPt.position, Vector3.up * 2f, Color.red, 2f);

        enemiesAliveFromCurrentWave++;
        UpdateWaveUI();

        var health = newEnemyGO.GetComponent<HealthManager>();
        if (health != null)
            health.OnDied += HandleEnemyDeathInWave;
        else
            Debug.LogWarning($"El prefab {newEnemyGO.name} no tiene HealthManager.", newEnemyGO);
    }

    private void HandleEnemyDeathInWave()
    {
        enemiesAliveFromCurrentWave = Mathf.Max(0, enemiesAliveFromCurrentWave - 1);
        Debug.Log($"Un enemigo murió. Vivos: {enemiesAliveFromCurrentWave}");
        UpdateWaveUI();
    }

    private void UpdateWaveUI(bool betweenWaves = false, bool allCompleted = false)
    {
        if (allCompleted)
        {
            if (waveTextElement) waveTextElement.text = "¡TODAS LAS OLEADAS SUPERADAS!";
            if (enemiesRemainingTextElement) enemiesRemainingTextElement.text = "";
            return;
        }

        if (betweenWaves)
        {
            if (waveTextElement) waveTextElement.text = $"Preparando Oleada: {currentWaveIndex + 1}";
            if (enemiesRemainingTextElement) enemiesRemainingTextElement.text = "¡Prepárate!";
        }
        else
        {
            if (waveTextElement) waveTextElement.text = $"Oleada: {currentWaveIndex + 1} / {waves.Length}";
            if (enemiesRemainingTextElement) enemiesRemainingTextElement.text = $"Enemigos Restantes: {enemiesAliveFromCurrentWave}";
        }
    }
}
