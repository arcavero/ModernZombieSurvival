using UnityEngine;
using UnityEngine.InputSystem; // ¡Importante! Usar el nuevo namespace

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 6.0f;
    [SerializeField] private float gravity = -9.81f * 2;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private Vector2 moveInput; // Variable para almacenar el Vector2 de la acción "Move"

    private PlayerInputActions playerInputActions; // Referencia al asset generado

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Instanciar el objeto de acciones generado
        playerInputActions = new PlayerInputActions();

        // Acceder al Action Map "Player" y luego a la acción "Move".
        // Suscribirse al evento 'performed': Se dispara cuando la acción se activa (tecla presionada, stick movido).
        // ctx (context) contiene el valor leído. Lo asignamos a nuestra variable moveInput.
        playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();

        // Suscribirse al evento 'canceled': Se dispara cuando la acción se desactiva (tecla soltada, stick vuelve al centro).
        // Reseteamos moveInput a cero.
        playerInputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    // Es crucial activar las acciones
    void OnEnable()
    {
        playerInputActions.Player.Move.Enable();
    }

    // Y desactivarlas para evitar problemas
    void OnDisable()
    {
        playerInputActions.Player.Move.Disable();
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        // Usamos el valor de moveInput (Vector2) que se actualiza mediante los eventos.
        // moveInput.x corresponde al eje horizontal (A/D)
        // moveInput.y corresponde al eje vertical (W/S)
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
}