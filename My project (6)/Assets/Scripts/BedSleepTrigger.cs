using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BedSleepTrigger : MonoBehaviour
{
    public Image fadeImage;
    public Transform teleportDestination;
    public string teleportObjectName = "TP coridor";

    public float fadeDuration = 1.5f;
    public float holdDuration = 1f;
    public float fadeOutDuration = 1.5f;

    public bool triggerOnce = true;
    public float interactionDistance = 3f;

    public float playerScaleMultiplier = 0.5f;

    public float newCameraFOV = 75f;
    public float cameraYOffset = -0.25f;

    private bool hasTriggered = false;
    private bool isEffectRunning = false;
    private Transform playerTransform;
    private CharacterController playerController;
    private Camera playerCamera;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<CharacterController>();
            playerCamera = player.GetComponentInChildren<Camera>();
        }

        if (teleportDestination == null)
        {
            GameObject tpObj = GameObject.Find(teleportObjectName);
            if (tpObj != null) teleportDestination = tpObj.transform;
        }

        if (fadeImage != null)
        {
            fadeImage.material = null;
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isEffectRunning) return;
        if (triggerOnce && hasTriggered) return;
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionDistance)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartCoroutine(FadeAndTeleportRoutine());
            }
        }
    }

    private IEnumerator FadeAndTeleportRoutine()
    {
        if (fadeImage == null) yield break;

        isEffectRunning = true;
        hasTriggered = true;

        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            c = fadeImage.color;
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }

        c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;

        if (teleportDestination != null && playerTransform != null)
        {
            if (playerController != null) playerController.enabled = false;

            playerTransform.position = teleportDestination.position;
            playerTransform.rotation = teleportDestination.rotation;

            Vector3 scale = playerTransform.localScale;
            scale *= playerScaleMultiplier;
            playerTransform.localScale = scale;

            if (playerController != null)
            {
                playerController.height *= playerScaleMultiplier;
                playerController.radius *= playerScaleMultiplier;
                playerController.center *= playerScaleMultiplier;
                playerController.enabled = true;
            }

            if (playerCamera != null)
            {
                playerCamera.fieldOfView = newCameraFOV;

                Transform camTransform = playerCamera.transform;
                Vector3 camPos = camTransform.localPosition;
                camPos.y += cameraYOffset;
                camTransform.localPosition = camPos;
            }
        }

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            c = fadeImage.color;
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }

        c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        fadeImage.gameObject.SetActive(false);
        isEffectRunning = false;
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        if (teleportDestination != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, teleportDestination.position);
            Gizmos.DrawWireSphere(teleportDestination.position, 0.5f);
        }
    }
}
