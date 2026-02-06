using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BecomeDogTrigger : MonoBehaviour
{
    public Image fadeImage;
    public float interactDistance = 3f;
    public float fadeDuration = 1.5f;
    public float holdDuration = 0.5f;

    public float dogScaleMultiplier = 0.5f;
    public float dogCameraFOV = 75f;

    public Transform cameraAttachPoint;

    private Transform playerTransform;
    private CharacterController playerController;
    private Camera playerCamera;

    private bool isRunning = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<CharacterController>();
            playerCamera = player.GetComponentInChildren<Camera>();
        }

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isRunning) return;
        if (playerCamera == null) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    StartCoroutine(BecomeDogRoutine());
                }
            }
        }
    }

    private IEnumerator BecomeDogRoutine()
    {
        isRunning = true;

        gameObject.SetActive(false);

        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }

        if (playerController != null) playerController.enabled = false;

        Vector3 scale = playerTransform.localScale;
        scale *= dogScaleMultiplier;
        playerTransform.localScale = scale;

        if (playerController != null)
        {
            playerController.height *= dogScaleMultiplier;
            playerController.radius *= dogScaleMultiplier;
            playerController.center *= dogScaleMultiplier;
            playerController.enabled = true;
        }

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = dogCameraFOV;

            if (cameraAttachPoint != null)
            {
                playerCamera.transform.SetParent(cameraAttachPoint);
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;
            }
        }

        yield return new WaitForSeconds(holdDuration);

        isRunning = false;
    }
}
