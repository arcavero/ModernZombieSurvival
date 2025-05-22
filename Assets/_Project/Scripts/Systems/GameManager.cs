// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Asumiendo que lo hiciste Singleton

    [Header("Player References")]
    [SerializeField] private HealthManager playerHealthManager;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverUI;

    [Header("Camera Reference")]
    [Tooltip("Arrastra aquí tu componente CameraLook desde la Main Camera")]
    [SerializeField] private CameraLook cameraLook;

    [Header("Other Systems")]
    [SerializeField] private EnemySpawner enemySpawner;

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        if (playerHealthManager == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) playerHealthManager = playerGO.GetComponent<HealthManager>();
            if (playerHealthManager == null) Debug.LogError("GameManager: No se encontró HealthManager del Player!", this);
        }

        if (gameOverUI == null) Debug.LogError("GameManager: La UI de Game Over no está asignada!", this);
        else gameOverUI.SetActive(false);

        if (cameraLook == null)
        {
            cameraLook = FindObjectOfType<CameraLook>();
            if (cameraLook == null) Debug.LogError("GameManager: No se encontró CameraLook en la escena!", this);
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
            // if (enemySpawner == null) Debug.LogWarning("GameManager: No se encontró EnemySpawner en la escena.", this); // Puede ser opcional
        }

        if (playerHealthManager != null)
            playerHealthManager.OnDied += HandlePlayerDeath; // La suscripción sigue igual
        else
        {
            Debug.LogError("GameManager no puede suscribirse a OnDied del jugador porque playerHealthManager es nulo.");
            enabled = false;
        }
    }

    void OnDestroy()
    {
        if (playerHealthManager != null)
            playerHealthManager.OnDied -= HandlePlayerDeath;
    }

    // --- MODIFICADO: Añadir el parámetro HealthManager ---
    private void HandlePlayerDeath(HealthManager deceasedPlayerHealthManager) // 'deceasedPlayerHealthManager' es el HealthManager del jugador que murió
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("GAME OVER! Jugador ha muerto.");

        if (gameOverUI != null) gameOverUI.SetActive(true);

        if (enemySpawner != null)
        {
            // Detener la corutina de spawneo si está activa
            // enemySpawner.StopAllCoroutines(); // Ya se hace en OnDisable del spawner si se desactiva el spawner
            enemySpawner.enabled = false; // Desactivar el spawner es más limpio
        }

        // Desactivar controles del jugador y cámara
        // No necesitamos el 'deceasedPlayerHealthManager' aquí porque ya tenemos 'playerHealthManager'
        // y asumimos que es el mismo. Si no, usaríamos el parámetro.
        GameObject playerGO = playerHealthManager.gameObject; // Obtener el GameObject del jugador desde su HealthManager
        if (playerGO != null)
        {
            if (playerGO.TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = false;
            if (playerGO.TryGetComponent<PlayerShooting>(out var shooting)) shooting.enabled = false;

            // El CameraLook suele estar en un hijo de Player (la cámara)
            CameraLook playerCamLook = playerGO.GetComponentInChildren<CameraLook>();
            if (playerCamLook != null) playerCamLook.enabled = false;
            // O si tienes la referencia directa 'cameraLook' (que ya tenías)
            else if (cameraLook != null) cameraLook.enabled = false;


            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        var enemies = FindObjectsOfType<EnemyAI>(); // Encuentra todos los EnemyAI activos
        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
            {
                if (agent.isOnNavMesh) // Solo si está en un navmesh válido
                {
                    agent.isStopped = true;
                    // agent.ResetPath(); // ResetPath puede dar errores si el agente no está en un NavMesh o está siendo destruido
                }
            }
            enemy.enabled = false; // Desactivar la lógica de IA
        }
    }

    public void RestartGame()
    {
        // ... (Tu código de RestartGame permanece igual) ...
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}