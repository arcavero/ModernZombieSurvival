using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    [SerializeField] private float lookSensitivityY = 0.2f; // Sensibilidad vertical
    [SerializeField] private float minVerticalAngle = -85f; // Ángulo mínimo para mirar abajo
    [SerializeField] private float maxVerticalAngle = 85f;  // Ángulo máximo para mirar arriba

    private Vector2 lookInput;
    private float xRotation = 0f; // Almacena la rotación vertical actual

    private PlayerInputActions playerInputActions;

    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
    }

    void OnEnable()
    {
        playerInputActions.Player.Look.Enable();
    }

    void OnDisable()
    {
        playerInputActions.Player.Look.Disable();
    }

    // Usamos LateUpdate para la cámara. Esto asegura que la cámara se actualice
    // DESPUÉS de que el jugador se haya movido y rotado en Update.
    // Ayuda a evitar tirones visuales.
    void LateUpdate()
    {
        HandleVerticalLook();
    }

    private void HandleVerticalLook()
    {
        // Usamos solo el componente Y del input del ratón (movimiento arriba/abajo)
        float mouseY = lookInput.y * lookSensitivityY;

        // Restamos mouseY a xRotation porque el eje Y del ratón normalmente se invierte
        // para la rotación de cámara (mover ratón arriba -> mirar arriba -> rotación negativa en X).
        // Si prefieres controles NO invertidos, suma mouseY en lugar de restarlo.
        xRotation -= mouseY;

        // Clamp (limitar) la rotación vertical para evitar que la cámara dé vueltas completas.
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        // Aplicamos la rotación SOLO al eje X local de la cámara.
        // No queremos rotar en Y o Z aquí.
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}