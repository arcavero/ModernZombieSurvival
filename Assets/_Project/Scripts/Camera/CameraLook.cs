// CameraLook.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    [SerializeField] private float lookSensitivityY = 0.2f; // Sensibilidad vertical
    [SerializeField] private float minVerticalAngle = -85f; // Ángulo mínimo para mirar abajo
    [SerializeField] private float maxVerticalAngle = 85f;  // Ángulo máximo para mirar arriba

    private float xRotation = 0f; // Almacena la rotación vertical actual
    private PlayerInputActions playerInputActions;

    void Awake()
    {
        // Instanciar el objeto de acciones generado
        playerInputActions = new PlayerInputActions();
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
    void LateUpdate()
    {
        HandleVerticalLook();
    }

    private void HandleVerticalLook()
    {
        // Polling: leer directamente el delta del ratón en cada frame
        float mouseY = playerInputActions.Player.Look.ReadValue<Vector2>().y * lookSensitivityY;

        // Restamos mouseY a xRotation porque el eje Y del ratón normalmente se invierte
        xRotation -= mouseY;

        // Clamp (limitar) la rotación vertical para evitar que la cámara dé vueltas completas.
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        // Aplicamos la rotación SOLO al eje X local de la cámara.
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
