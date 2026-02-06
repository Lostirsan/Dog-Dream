using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Game.Doors
{
    /// <summary>
    /// Raycasts from the camera forward and toggles DoorInteractable on E.
    /// Works with Unity Input System (Keyboard.current).
    /// </summary>
    public class PlayerDoorInteractor : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private LayerMask raycastMask = ~0;

        private void Reset()
        {
            // Try common FPS setups.
            var cam = Camera.main;
            if (cam != null) cameraTransform = cam.transform;
        }

        private void Update()
        {
            if (!WasInteractPressed())
                return;

            if (cameraTransform == null)
                return;

            if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, interactDistance, raycastMask, QueryTriggerInteraction.Ignore))
                return;

            var door = hit.transform.GetComponentInParent<DoorInteractable>();
            if (door == null)
                return;

            door.Toggle();
        }

        private static bool WasInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }
    }
}
