using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float gravity = -9.81f * 2f;

    [Header("Look")]
    [SerializeField] private float lookSensitivityX = 0.2f;

    [Header("Footsteps (Distance-Based)")]
    [SerializeField] private List<AudioClip> footstepSounds;
    [SerializeField]
    [Tooltip("Distancia recorrida para disparar un paso al caminar.")]
    private float stepDistanceWalking = 2f;
    [SerializeField]
    [Tooltip("Distancia recorrida para disparar un paso al sprintar.")]
    private float stepDistanceSprinting = 1.2f;
    [SerializeField][Range(0.1f, 1f)] private float footstepVolume = 0.7f;

    private CharacterController controller;
    private AudioSource audioSource;
    private Vector3 verticalVelocity;
    private Vector2 moveInput;
    private PlayerInputActions playerInputActions;

    // para acumular la distancia desde el último paso
    private float footstepDistanceAccumulator = 0f;
    private bool isSprinting = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        playerInputActions = new PlayerInputActions();

        // Inputs
        playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        playerInputActions.Player.Sprint.performed += ctx => isSprinting = true;
        playerInputActions.Player.Sprint.canceled += ctx => isSprinting = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (audioSource == null)
            Debug.LogWarning("No AudioSource en PlayerMovement, los pasos no se oirán.", this);
        if (footstepSounds == null || footstepSounds.Count == 0)
            Debug.LogWarning("No hay AudioClips de paso asignados.", this);
    }

    void OnEnable()
    {
        playerInputActions.Player.Move.Enable();
        playerInputActions.Player.Look.Enable();
        playerInputActions.Player.Sprint.Enable();
    }

    void OnDisable()
    {
        playerInputActions.Player.Move.Disable();
        playerInputActions.Player.Look.Disable();
        playerInputActions.Player.Sprint.Disable();
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleLook();
        HandleFootsteps();  // ahora por distancia
    }

    private void HandleMovement()
    {
        float currentSpeed = isSprinting ? speed * sprintSpeedMultiplier : speed;
        Vector3 dir = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        controller.Move(dir * currentSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;
        else
            verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        Vector2 lookDelta = playerInputActions.Player.Look.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * lookDelta.x * lookSensitivityX);
    }

    private void HandleFootsteps()
    {
        if (audioSource == null || footstepSounds == null || footstepSounds.Count == 0)
            return;

        // Solo cuando está en el suelo y hay input de movimiento
        if (!controller.isGrounded || moveInput == Vector2.zero)
        {
            // Opcional: resetear al saltar o dejar de moverse
            footstepDistanceAccumulator = 0f;
            return;
        }

        // Calcula la velocidad horizontal real del jugador
        float currentSpeed = isSprinting ? speed * sprintSpeedMultiplier : speed;

        // Acumula distancia recorrida este frame
        footstepDistanceAccumulator += currentSpeed * Time.deltaTime;

        // Define cuánto hay que recorrer para cada paso
        float requiredDistance = isSprinting ? stepDistanceSprinting : stepDistanceWalking;

        // Cuando supere el umbral, dispara el sonido y descuenta la distancia usada
        if (footstepDistanceAccumulator >= requiredDistance)
        {
            int idx = Random.Range(0, footstepSounds.Count);
            audioSource.PlayOneShot(footstepSounds[idx], footstepVolume);
            footstepDistanceAccumulator -= requiredDistance;
        }
    }
}
