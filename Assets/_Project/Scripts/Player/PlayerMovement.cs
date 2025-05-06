// PlayerMovement.cs
using UnityEngine;
using UnityEngine.InputSystem; // ¡Importante! Usar el nuevo namespace

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] // Añadir Headers mejora la organización en el Inspector
    [SerializeField] private float speed = 6.0f;
    [SerializeField] private float gravity = -9.81f * 2;

    [Header("Look")]
    [SerializeField] private float lookSensitivityX = 0.2f; // Sensibilidad horizontal

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private Vector2 moveInput; // Variable para almacenar el Vector2 de la acción "Move"
    private PlayerInputActions playerInputActions; // Referencia al asset generado

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Instanciar el objeto de acciones generado
        playerInputActions = new PlayerInputActions();

        // --- Suscripciones de Input ---
        // --- Movimiento ---
        playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // --- Bloquear y Ocultar Cursor ---
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Es crucial activar las acciones
    void OnEnable()
    {
        playerInputActions.Player.Move.Enable();
        playerInputActions.Player.Look.Enable();
    }

    // Y desactivarlas para evitar problemas
    void OnDisable()
    {
        playerInputActions.Player.Move.Disable();
        playerInputActions.Player.Look.Disable();
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleLook();
    }

    private void HandleMovement()
    {
        // Usamos el valor de moveInput (Vector2) que se actualiza mediante los eventos.
        Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        Vector3 move = moveDirection * speed * Time.deltaTime;
        controller.Move(move);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        // Polling: leer directamente el delta del ratón en cada frame
        Vector2 lookDelta = playerInputActions.Player.Look.ReadValue<Vector2>();

        // Usamos solo el componente X del input del ratón (movimiento izquierda/derecha)
        float mouseX = lookDelta.x * lookSensitivityX;

        // Rotamos el transform del jugador alrededor del eje Y (vertical) global.
        transform.Rotate(Vector3.up * mouseX);

        // La rotación vertical (mirar arriba/abajo) se manejará en CameraLook.
    }
}
