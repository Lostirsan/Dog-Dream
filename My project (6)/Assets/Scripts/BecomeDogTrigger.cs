using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BecomeDogTrigger : MonoBehaviour
{
    public Image fadeImage;
    public float interactDistance = 3f;

    public float fadeDuration = 1.5f;
    public float holdDuration = 1f;

    public float dogScaleMultiplier = 0.5f;

    public Camera playerCamera;
    public Camera dogCamera;

    public GameObject doorToDelete;

    private Transform playerTransform;
    private CharacterController playerController;

    private DogController dogController;
    private FpsCharacterController playerFpsController;

    private bool isRunning = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<CharacterController>();
            playerFpsController = player.GetComponent<FpsCharacterController>();

            if (playerCamera == null)
                playerCamera = player.GetComponentInChildren<Camera>();
        }

        if (dogCamera != null)
        {
            dogController = dogCamera.GetComponentInParent<DogController>();
            dogCamera.gameObject.SetActive(false);
        }

        if (dogController != null)
            dogController.enabled = false;

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
        if (fadeImage == null) yield break;

        isRunning = true;

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

        c.a = 1f;
        fadeImage.color = c;

        if (playerController != null) playerController.enabled = false;
        if (playerFpsController != null) playerFpsController.enabled = false;

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
            playerCamera.gameObject.SetActive(false);

        if (dogCamera != null)
            dogCamera.gameObject.SetActive(true);

        if (dogController != null)
            dogController.enabled = true;

        if (doorToDelete != null)
            doorToDelete.SetActive(false);

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);

        gameObject.SetActive(false);

        isRunning = false;
    }
}
