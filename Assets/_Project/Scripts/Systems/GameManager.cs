using UnityEngine;
using UnityEngine.SceneManagement; // Si más adelante deseas recargar la escena

public class GameManager : MonoBehaviour
{
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
        // 1. Asegurar HealthManager
        if (playerHealthManager == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                playerHealthManager = playerGO.GetComponent<HealthManager>();

            if (playerHealthManager == null)
                Debug.LogError("GameManager: No se encontró HealthManager del Player!", this);
        }

        // 2. Asegurar Game Over UI
        if (gameOverUI == null)
            Debug.LogError("GameManager: La UI de Game Over no está asignada!", this);
        else
            gameOverUI.SetActive(false);

        // 3. Asegurar CameraLook
        if (cameraLook == null)
        {
            cameraLook = FindObjectOfType<CameraLook>();
            if (cameraLook == null)
                Debug.LogError("GameManager: No se encontró CameraLook en la escena!", this);
        }

        // 4. Asegurar EnemySpawner (opcional)
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
            if (enemySpawner == null)
                Debug.LogWarning("GameManager: No se encontró EnemySpawner en la escena.", this);
        }

        // 5. Suscribir al evento de muerte del jugador
        if (playerHealthManager != null)
            playerHealthManager.OnDied += HandlePlayerDeath;
        else
            enabled = false;
    }

    void OnDestroy()
    {
        if (playerHealthManager != null)
            playerHealthManager.OnDied -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("GAME OVER!");

        // Mostrar UI de Game Over
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        // Detener spawneo de enemigos
        if (enemySpawner != null)
        {
            enemySpawner.StopAllCoroutines();
            enemySpawner.enabled = false;
        }

        // Desactivar controles del jugador
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            if (playerGO.TryGetComponent<PlayerMovement>(out var movement))
                movement.enabled = false;
            if (playerGO.TryGetComponent<PlayerShooting>(out var shooting))
                shooting.enabled = false;

            // Liberar cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Desactivar solo el script de look, no la cámara
        if (cameraLook != null)
            cameraLook.enabled = false;

        // Detener IA de enemigos activos
        var enemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            enemy.enabled = false;
        }
    }

    // --- Public function to be called by the UI Button ---
    public void RestartGame()
    {
        Debug.Log("GameManager: Intentando reiniciar el juego...");

        // 1. Resetear la escala de tiempo (MUY IMPORTANTE si alguna vez pausas el juego o lo ralentizas)
        //    Asegura que la nueva escena empiece con tiempo normal.
        Time.timeScale = 1f;

        // 2. Cargar la escena activa actualmente por su índice en la configuración de Build.
        //    Esto recarga el nivel actual.
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);

        // Nota: No necesitamos reactivar controles, mostrar cursor, etc., aquí.
        // Eso lo manejarán los scripts en sus Awake/Start cuando la escena se recargue.
    }
}
