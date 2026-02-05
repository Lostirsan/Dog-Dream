using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Wall-walking character controller that ignores gravity and allows the player
/// to walk on any surface - floors, walls, and ceilings.
/// 
/// The player's "up" direction is determined by the surface normal they're standing on.
/// When the player walks onto a new surface, they smoothly rotate to align with it.
/// The camera smoothly animates to match the new orientation for a natural walking feel.
/// 
/// Controls:
/// - WASD: Move
/// - Shift: Sprint
/// - Mouse: Look around
/// - Space: Jump (in the direction of current "up")
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class WallWalkingController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera pivot used for vertical look (usually the player Camera transform).")]
    [SerializeField] private Transform cameraPivot;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float verticalLookLimit = 85f;
    [Tooltip("If enabled, locks and hides the cursor on Start.")]
    [SerializeField] private bool lockCursorOnStart = true;

    [Header("Movement Speeds")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 7.0f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.25f;
    [Tooltip("How long the jump velocity is applied before surface detection resumes.")]
    [SerializeField] private float jumpCooldown = 0.2f;

    [Header("Surface Detection")]
    [Tooltip("Distance to check for surfaces below the player.")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [Tooltip("Distance to check for new surfaces in movement direction.")]
    [SerializeField] private float surfaceCheckDistance = 1.0f;
    [Tooltip("Layers considered as walkable surfaces.")]
    [SerializeField] private LayerMask surfaceLayers = ~0;
    [Tooltip("How quickly the player rotates to align with new surfaces.")]
    [SerializeField] private float rotationSpeed = 10.0f;
    [Tooltip("Force applied to stick to surfaces.")]
    [SerializeField] private float surfaceStickForce = 2.0f;

    [Header("Camera Animation")]
    [Tooltip("How smoothly the camera transitions when changing surfaces. Lower = smoother.")]
    [SerializeField] private float cameraTransitionSpeed = 8.0f;
    [Tooltip("Additional smoothing for camera roll to reduce motion sickness.")]
    [SerializeField] private float cameraRollSmoothing = 5.0f;
    [Tooltip("Enable camera tilt when strafing for more dynamic feel.")]
    [SerializeField] private bool enableStrafeTilt = true;
    [Tooltip("Maximum strafe tilt angle in degrees.")]
    [SerializeField] private float maxStrafeTilt = 3.0f;

    [Header("Smoothing")]
    [SerializeField] private float moveResponsiveness = 14f;

    private CharacterController _controller;

    // Current "up" direction (surface normal)
    private Vector3 _currentUp = Vector3.up;
    private Vector3 _targetUp = Vector3.up;

    // Look state
    private float _pitch;
    private float _yaw;

    // Camera animation state
    private Quaternion _cameraTargetRotation;
    private Quaternion _cameraCurrentRotation;
    private float _currentStrafeTilt;
    private Vector3 _smoothedUp;

    // Movement state
    private Vector3 _velocity;
    private Vector3 _currentPlanarMove;
    private bool _isGrounded;
    private float _jumpTimer;
    
    // Jump calculation constant (simulates gravity for jump height calculation)
    private const float JumpGravity = -18f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        if (cameraPivot == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraPivot = cam.transform;
        }

        _currentUp = transform.up;
        _targetUp = transform.up;
        _smoothedUp = transform.up;
        
        if (cameraPivot != null)
        {
            _cameraCurrentRotation = cameraPivot.localRotation;
            _cameraTargetRotation = cameraPivot.localRotation;
        }
    }

    private void Start()
    {
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // Update jump cooldown
        if (_jumpTimer > 0)
            _jumpTimer -= Time.deltaTime;

        // 1) Detect surface and update orientation
        UpdateSurfaceDetection();

        // 2) Smoothly rotate to align with surface
        UpdateOrientation();

        // 3) Mouse look
        UpdateMouseLook();

        // 4) Animate camera for smooth transitions
        UpdateCameraAnimation();

        // 5) Movement
        UpdateMovement();

        // 6) Jump
        UpdateJump();

        // 7) Apply velocity and stick force
        ApplyMovement();
    }

    /// <summary>
    /// Detects the surface below the player and updates the target up direction.
    /// Also checks for surfaces in the movement direction for smooth wall transitions.
    /// </summary>
    private void UpdateSurfaceDetection()
    {
        if (_jumpTimer > 0)
        {
            _isGrounded = false;
            return;
        }

        Vector3 checkOrigin = transform.position;
        float checkDist = (_controller.height * 0.5f) + groundCheckDistance;

        // Check directly below (relative to current up)
        if (Physics.Raycast(checkOrigin, -_currentUp, out RaycastHit groundHit, checkDist, surfaceLayers, QueryTriggerInteraction.Ignore))
        {
            _isGrounded = true;
            _targetUp = groundHit.normal;

            // Reset vertical velocity when grounded
            float upVelocity = Vector3.Dot(_velocity, _currentUp);
            if (upVelocity < 0)
            {
                _velocity -= _currentUp * upVelocity;
            }
        }
        else
        {
            _isGrounded = false;
        }

        // Check for surfaces in movement direction (for wall transitions)
        Vector2 moveInput = ReadMove();
        if (moveInput.sqrMagnitude > 0.1f)
        {
            Vector3 moveDir = GetMoveDirection(moveInput);
            
            // Check forward and down for surface transitions
            Vector3 forwardCheckOrigin = transform.position + moveDir * (_controller.radius + 0.1f);
            
            // Check if there's a wall in front
            if (Physics.Raycast(forwardCheckOrigin, moveDir, out RaycastHit wallHit, surfaceCheckDistance, surfaceLayers, QueryTriggerInteraction.Ignore))
            {
                // Found a wall - prepare to walk on it
                float angle = Vector3.Angle(_currentUp, wallHit.normal);
                if (angle > 30f && angle < 150f)
                {
                    _targetUp = wallHit.normal;
                }
            }
            
            // Check if we're about to walk off an edge (look for surface below in front)
            Vector3 edgeCheckOrigin = transform.position + moveDir * (_controller.radius + 0.3f);
            if (Physics.Raycast(edgeCheckOrigin, -_currentUp, out RaycastHit edgeHit, checkDist * 2f, surfaceLayers, QueryTriggerInteraction.Ignore))
            {
                // Surface continues - check if it curves
                float angle = Vector3.Angle(_currentUp, edgeHit.normal);
                if (angle > 10f)
                {
                    _targetUp = edgeHit.normal;
                }
            }
            else
            {
                // No surface below in front - check for surface wrapping around (like going over an edge to a wall)
                Vector3 wrapCheckOrigin = edgeCheckOrigin - _currentUp * (_controller.height * 0.5f);
                if (Physics.Raycast(wrapCheckOrigin, -moveDir, out RaycastHit wrapHit, surfaceCheckDistance, surfaceLayers, QueryTriggerInteraction.Ignore))
                {
                    _targetUp = wrapHit.normal;
                }
            }
            
            // Additional check for walking DOWN slopes/ramps
            // Cast a ray diagonally down in the movement direction to find sloped surfaces
            Vector3 slopeCheckDir = (moveDir - _currentUp).normalized;
            Vector3 slopeCheckOrigin = transform.position + moveDir * (_controller.radius * 0.5f);
            if (Physics.Raycast(slopeCheckOrigin, slopeCheckDir, out RaycastHit slopeHit, checkDist * 1.5f, surfaceLayers, QueryTriggerInteraction.Ignore))
            {
                float angle = Vector3.Angle(_currentUp, slopeHit.normal);
                if (angle > 5f && angle < 80f)
                {
                    _targetUp = slopeHit.normal;
                    
                    // If we found a slope and we're not grounded, snap to it
                    if (!_isGrounded)
                    {
                        _isGrounded = true;
                    }
                }
            }
        }
        
        // When grounded, also do a spherecast to better detect surface changes beneath us
        if (_isGrounded)
        {
            Vector3 sphereOrigin = transform.position;
            float sphereRadius = _controller.radius * 0.8f;
            if (Physics.SphereCast(sphereOrigin, sphereRadius, -_currentUp, out RaycastHit sphereHit, checkDist, surfaceLayers, QueryTriggerInteraction.Ignore))
            {
                float angle = Vector3.Angle(_currentUp, sphereHit.normal);
                if (angle > 5f && angle < 80f)
                {
                    // Blend towards the new normal for smoother transitions
                    _targetUp = Vector3.Slerp(_targetUp, sphereHit.normal, 0.5f).normalized;
                }
            }
        }
    }

    /// <summary>
    /// Smoothly rotates the player to align with the target surface normal.
    /// Uses smooth interpolation for a natural walking feel.
    /// </summary>
    private void UpdateOrientation()
    {
        // Smoothly interpolate current up towards target up with variable speed
        // Faster when angle is large, slower for fine adjustments
        float angleDiff = Vector3.Angle(_currentUp, _targetUp);
        float adaptiveSpeed = rotationSpeed * Mathf.Clamp01(angleDiff / 45f + 0.3f);
        
        _currentUp = Vector3.Slerp(_currentUp, _targetUp, Time.deltaTime * adaptiveSpeed);
        _currentUp.Normalize();

        // Calculate the rotation to align transform.up with _currentUp while preserving forward direction
        Vector3 currentForward = transform.forward;
        
        // Project forward onto the plane perpendicular to the new up
        Vector3 newForward = Vector3.ProjectOnPlane(currentForward, _currentUp);
        if (newForward.sqrMagnitude < 0.001f)
        {
            // Forward is parallel to up, use right as reference
            newForward = Vector3.ProjectOnPlane(transform.right, _currentUp);
        }
        newForward.Normalize();

        // Apply yaw rotation around the current up axis
        Quaternion yawRotation = Quaternion.AngleAxis(_yaw, _currentUp);
        newForward = yawRotation * Vector3.ProjectOnPlane(Vector3.forward, _currentUp).normalized;
        
        if (newForward.sqrMagnitude < 0.001f)
        {
            newForward = yawRotation * Vector3.ProjectOnPlane(Vector3.right, _currentUp).normalized;
        }
        newForward.Normalize();

        // Create rotation from up and forward
        if (newForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(newForward, _currentUp);
            // Use smooth damp-like interpolation for more natural feel
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * adaptiveSpeed);
        }
    }

    /// <summary>
    /// Handles mouse look - yaw rotates around current up, pitch rotates camera.
    /// </summary>
    private void UpdateMouseLook()
    {
        Vector2 lookDelta = ReadLookDelta() * mouseSensitivity;

        // Yaw: accumulate and apply in UpdateOrientation
        _yaw += lookDelta.x;

        // Pitch: rotate camera up/down with clamp
        _pitch -= lookDelta.y;
        _pitch = Mathf.Clamp(_pitch, -verticalLookLimit, verticalLookLimit);

        // Camera rotation is now handled in UpdateCameraAnimation for smooth transitions
    }

    /// <summary>
    /// Smoothly animates the camera to create a natural walking feel on any surface.
    /// Handles pitch, roll compensation, and optional strafe tilt.
    /// </summary>
    private void UpdateCameraAnimation()
    {
        if (cameraPivot == null) return;

        // Smoothly interpolate the "up" direction for camera to reduce jarring transitions
        _smoothedUp = Vector3.Slerp(_smoothedUp, _currentUp, Time.deltaTime * cameraRollSmoothing);

        // Calculate strafe tilt based on horizontal input
        float strafeTilt = 0f;
        if (enableStrafeTilt)
        {
            Vector2 moveInput = ReadMove();
            float targetTilt = -moveInput.x * maxStrafeTilt;
            _currentStrafeTilt = Mathf.Lerp(_currentStrafeTilt, targetTilt, Time.deltaTime * 10f);
            strafeTilt = _currentStrafeTilt;
        }

        // Build the target camera rotation
        // Pitch (looking up/down) + Roll (strafe tilt + surface alignment compensation)
        _cameraTargetRotation = Quaternion.Euler(_pitch, 0f, strafeTilt);

        // Smoothly interpolate to target rotation
        _cameraCurrentRotation = Quaternion.Slerp(
            _cameraCurrentRotation, 
            _cameraTargetRotation, 
            Time.deltaTime * cameraTransitionSpeed
        );

        cameraPivot.localRotation = _cameraCurrentRotation;
    }

    /// <summary>
    /// Calculates movement direction based on input and current orientation.
    /// </summary>
    private Vector3 GetMoveDirection(Vector2 moveInput)
    {
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        // Transform input to world space based on player orientation
        return transform.TransformDirection(inputDir);
    }

    /// <summary>
    /// Updates horizontal movement based on input.
    /// </summary>
    private void UpdateMovement()
    {
        Vector2 moveInput = ReadMove();
        Vector3 moveDir = GetMoveDirection(moveInput);

        bool sprintHeld = ReadSprintHeld();
        float speed = sprintHeld ? sprintSpeed : moveSpeed;

        Vector3 desiredMove = moveDir * speed;

        _currentPlanarMove = Vector3.Lerp(_currentPlanarMove, desiredMove, Time.deltaTime * moveResponsiveness);
    }

    /// <summary>
    /// Handles jumping in the direction of current up.
    /// </summary>
    private void UpdateJump()
    {
        if (_isGrounded && ReadJumpPressedThisFrame())
        {
            // Calculate jump velocity from height: v = sqrt(h * -2g)
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * JumpGravity);
            _velocity = _currentUp * jumpVelocity;
            _jumpTimer = jumpCooldown;
            _isGrounded = false;
        }
    }

    /// <summary>
    /// Applies all movement to the character controller.
    /// </summary>
    private void ApplyMovement()
    {
        Vector3 totalMove = Vector3.zero;

        // Add planar movement
        totalMove += _currentPlanarMove * Time.deltaTime;

        // Add velocity (from jumping)
        totalMove += _velocity * Time.deltaTime;

        // Apply stick force when grounded (pulls player towards surface)
        if (_isGrounded && _jumpTimer <= 0)
        {
            totalMove -= _currentUp * surfaceStickForce * Time.deltaTime;
        }

        // Decay velocity when grounded
        if (_isGrounded)
        {
            _velocity = Vector3.Lerp(_velocity, Vector3.zero, Time.deltaTime * 5f);
        }

        _controller.Move(totalMove);
    }

    // -------------------------
    // Input abstraction layer
    // -------------------------

    private static Vector2 ReadMove()
    {
#if ENABLE_INPUT_SYSTEM
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
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

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

    // -------------------------
    // Debug visualization
    // -------------------------

    private void OnDrawGizmosSelected()
    {
        // Draw current up direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, _currentUp * 2f);

        // Draw target up direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, _targetUp * 2f);

        // Draw ground check
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        float checkDist = _controller != null ? (_controller.height * 0.5f) + groundCheckDistance : 1.2f;
        Gizmos.DrawRay(transform.position, -_currentUp * checkDist);
    }
}
