using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BedActivator : MonoBehaviour
{
    public BedSleepTrigger bedScript;
    public float interactionDistance = 3f;
    public Transform playerCamera;

    private void Awake()
    {
        if (playerCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null) playerCamera = cam.transform;
        }
    }

    private void Update()
    {
        if (!InteractPressed()) return;
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (bedScript != null)
                    bedScript.enabled = true;
            }
        }
    }

    private bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
