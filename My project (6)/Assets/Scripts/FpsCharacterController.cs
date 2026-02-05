using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Simple, performant FPS controller built for CharacterController.
/// - WASD move, Shift sprint, Space jump, Ctrl crouch
/// - Smooth gravity + reliable ground detection
/// - Mouse look with vertical clamp
///
/// Input:
/// - Uses the new Input System when active (ENABLE_INPUT_SYSTEM)
/// - Falls back to legacy Input Manager otherwise
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class FpsCharacterController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera pivot used for vertical look (usually the player Camera transform).")]
    [SerializeField] private Transform cameraPivot;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float verticalLookLimit = 85f; // Clamp for pitch (up/down)
    [Tooltip("If enabled, locks and hides the cursor on Start.")]
    [SerializeField] private bool lockCursorOnStart = true;

    [Header("Movement Speeds")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 7.0f;
    [SerializeField] private float crouchSpeed = 2.75f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.25f;
    [Tooltip("Gravity acceleration (negative). Default -9.81.")]
    [SerializeField] private float gravity = -18f;
    [Tooltip("Extra downward force when grounded for snappy ground sticking.")]
    [SerializeField] private float groundedStickForce = -2f;

    [Header("Ground Detection")]
    [Tooltip("Distance below the CharacterController bottom for ground probing.")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [Tooltip("Layers considered as ground.")]
    [SerializeField] private LayerMask groundLayers = ~0;

    [Header("Crouch")]
    [Tooltip("Standing CharacterController height.")]
    [SerializeField] private float standingHeight = 1.8f;
    [Tooltip("Crouched CharacterController height.")]
    [SerializeField] private float crouchHeight = 1.1f;
    [Tooltip("How quickly the controller changes height.")]
    [SerializeField] private float crouchLerpSpeed = 12f;
    [Tooltip("Prevents standing up if something blocks above the head.")]
    [SerializeField] private bool preventStandUnderCeiling = true;

    [Header("Smoothing (Optional)")]
    [Tooltip("Higher = snappier input. Lower = smoother acceleration.")]
    [SerializeField] private float moveResponsiveness = 14f;

    private CharacterController _controller;

    // Look state
    private float _pitch;

    // Movement state
    private Vector3 _velocity; // y used for gravity/jump
    private Vector3 _currentPlanarMove; // smoothed xz movement vector
    private bool _isGrounded;
    private bool _isCrouching;

    // Cached values for crouch center adjustments
    private Vector3 _standingCenter;
    private Vector3 _crouchingCenter;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        // If no camera pivot set, try to find Camera in children (common FPS setup).
        if (cameraPivot == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraPivot = cam.transform;
        }

        // Initialize CharacterController dimensions.
        standingHeight = Mathf.Max(standingHeight, 0.5f);
        crouchHeight = Mathf.Clamp(crouchHeight, 0.5f, standingHeight);

        _controller.height = standingHeight;

        // Keep bottom of capsule stable by adjusting center as height changes.
        float standHalf = standingHeight * 0.5f;
        float crouchHalf = crouchHeight * 0.5f;
        _standingCenter = new Vector3(_controller.center.x, standHalf, _controller.center.z);
        _crouchingCenter = new Vector3(_controller.center.x, crouchHalf, _controller.center.z);
        _controller.center = _standingCenter;
    }

    private void Start()
    {
        // Cursor lock is typical for FPS and avoids accidental focus loss while mousing around.
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // --- 1) Ground detection (reliable + cheap) ---
        UpdateGroundedState();

        // --- 2) Mouse look (yaw on body, pitch on camera) ---
        UpdateMouseLook();

        // --- 3) Crouch (changes CharacterController height smoothly) ---
        UpdateCrouch();

        // --- 4) Planar movement (WASD + sprint/crouch speeds) ---
        UpdateMovement();

        // --- 5) Jump & gravity (smooth, with grounded stick) ---
        UpdateVerticalMotion();
    }

    /// <summary>
    /// Uses a short sphere cast from just above the controller bottom to detect ground.
    /// More stable than relying solely on CharacterController.isGrounded on slopes/edges.
    /// </summary>
    private void UpdateGroundedState()
    {
        Vector3 origin = transform.position + Vector3.up * (_controller.radius + 0.02f);

        float castDistance = (_controller.height * 0.5f) - _controller.radius + groundCheckDistance;
        _isGrounded = Physics.SphereCast(
            origin,
            _controller.radius * 0.95f,
            Vector3.down,
            out _,
            castDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );

        // If grounded and falling, keep a small downward velocity so we "stick" to the ground.
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = groundedStickForce;
    }

    /// <summary>
    /// Mouse X rotates the player body (yaw).
    /// Mouse Y rotates the camera pivot (pitch), clamped to avoid flipping.
    /// </summary>
    private void UpdateMouseLook()
    {
        Vector2 lookDelta = ReadLookDelta() * mouseSensitivity;

        // Yaw: rotate player left/right.
        transform.Rotate(0f, lookDelta.x, 0f, Space.Self);

        // Pitch: rotate camera up/down with clamp.
        _pitch -= lookDelta.y;
        _pitch = Mathf.Clamp(_pitch, -verticalLookLimit, verticalLookLimit);

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    /// <summary>
    /// Ctrl toggles crouch while held. Smoothly lerps height/center.
    /// Optionally prevents standing up if something blocks above.
    /// </summary>
    private void UpdateCrouch()
    {
        bool crouchHeld = ReadCrouchHeld();

        // Determine desired crouch state.
        bool wantsCrouch = crouchHeld;

        // If trying to stand, optionally ensure there's headroom.
        if (!wantsCrouch && _isCrouching && preventStandUnderCeiling)
        {
            if (IsCeilingBlocked())
                wantsCrouch = true;
        }

        _isCrouching = wantsCrouch;

        float targetHeight = _isCrouching ? crouchHeight : standingHeight;
        Vector3 targetCenter = _isCrouching ? _crouchingCenter : _standingCenter;

        _controller.height = Mathf.Lerp(_controller.height, targetHeight, Time.deltaTime * crouchLerpSpeed);
        _controller.center = Vector3.Lerp(_controller.center, targetCenter, Time.deltaTime * crouchLerpSpeed);
    }

    /// <summary>
    /// Checks if there is something above that would prevent standing up.
    /// </summary>
    private bool IsCeilingBlocked()
    {
        float extra = standingHeight - _controller.height;
        if (extra <= 0.01f) return false;

        float radius = _controller.radius * 0.95f;
        Vector3 currentTop = transform.position + Vector3.up * (_controller.center.y + (_controller.height * 0.5f) - radius);

        return Physics.SphereCast(
            currentTop,
            radius,
            Vector3.up,
            out _,
            extra,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    /// <summary>
    /// Computes desired planar movement from WASD in local space, then applies smoothing.
    /// CharacterController.Move is used for collision-safe movement.
    /// </summary>
    private void UpdateMovement()
    {
        Vector2 moveInput = ReadMove();

        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        bool sprintHeld = ReadSprintHeld();

        float speed =
            _isCrouching ? crouchSpeed :
            sprintHeld ? sprintSpeed :
            moveSpeed;

        Vector3 desiredMove = transform.TransformDirection(inputDir) * speed;

        _currentPlanarMove = Vector3.Lerp(_currentPlanarMove, desiredMove, Time.deltaTime * moveResponsiveness);

        Vector3 move = new Vector3(_currentPlanarMove.x, 0f, _currentPlanarMove.z) * Time.deltaTime;
        _controller.Move(move);
    }

    /// <summary>
    /// Applies jump impulse and continuous gravity, then moves the controller vertically.
    /// </summary>
    private void UpdateVerticalMotion()
    {
        if (_isGrounded && ReadJumpPressedThisFrame())
        {
            // v = sqrt(h * -2g). gravity is negative, so -2g is positive.
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
    }

    // -------------------------
    // Input abstraction layer
    // -------------------------

    /// <summary>
    /// Returns Move input as (x=left/right, y=forward/back) in range [-1..1].
    /// </summary>
    private static Vector2 ReadMove()
    {
#if ENABLE_INPUT_SYSTEM
        // New Input System
        Keyboard kb = Keyboard.current;
        if (kb == null) return Vector2.zero;

        float x = 0f;
        float y = 0f;

        if (kb.aKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed) x += 1f;
        if (kb.wKey.isPressed) y += 1f;
        if (kb.sKey.isPressed) y -= 1f;

        Vector2 v = new Vector2(x, y);
        if (v.sqrMagnitude > 1f) v.Normalize();
        return v;
#else
        // Legacy Input Manager
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    /// <summary>
    /// Returns raw mouse delta since last frame.
    /// </summary>
    private static Vector2 ReadLookDelta()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse m = Mouse.current;
        if (m == null) return Vector2.zero;
        return m.delta.ReadValue();
#else
        return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#endif
    }

    private static bool ReadSprintHeld()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;
        return kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
    }

    private static bool ReadCrouchHeld()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;
        return kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif
    }

    private static bool ReadJumpPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;
        return kb.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space);
#endif
    }
}
