using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;      // El Player
    [SerializeField] private float lookSensitivityY = 0.2f;  // Pitch
    [SerializeField] private float minVerticalAngle = -85f;
    [SerializeField] private float maxVerticalAngle = 85f;

    private Vector3 offset;
    private float xRotation;
    private PlayerInputActions inputActions;

    void Awake()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        offset = transform.position - playerTransform.position;
    }

    void OnEnable() => inputActions.Player.Look.Enable();
    void OnDisable() => inputActions.Player.Look.Disable();

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 1) Seguir posición
        transform.position = playerTransform.position + offset;

        // 2) Leer del ratón
        Vector2 lookDelta = inputActions.Player.Look.ReadValue<Vector2>();

        // 3) Pitch (X)
        xRotation = Mathf.Clamp(
            xRotation - lookDelta.y * lookSensitivityY,
            minVerticalAngle,
            maxVerticalAngle
        );

        // 4) Yaw (Y) viene del jugador
        float yaw = playerTransform.eulerAngles.y;

        // 5) Aplicar combo pitch+yaw
        transform.rotation = Quaternion.Euler(xRotation, yaw, 0f);
    }
}
