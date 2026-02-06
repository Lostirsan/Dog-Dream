using UnityEngine;

public class ReturnPlayerTrigger : MonoBehaviour
{
    public string teleportObjectName = "TP back";
    public float interactionDistance = 3f;

    private Transform playerTransform;
    private CharacterController playerController;
    private Transform teleportDestination;

    private Vector3 originalScale;
    private float originalHeight;
    private float originalRadius;
    private Vector3 originalCenter;

    private bool initialized = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<CharacterController>();

            originalScale = playerTransform.localScale;

            if (playerController != null)
            {
                originalHeight = playerController.height;
                originalRadius = playerController.radius;
                originalCenter = playerController.center;
            }
        }

        GameObject tpObj = GameObject.Find(teleportObjectName);
        if (tpObj != null) teleportDestination = tpObj.transform;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionDistance)
        {
            TeleportBack();
            gameObject.SetActive(false);
        }
    }

    private void TeleportBack()
    {
        if (teleportDestination == null) return;

        if (playerController != null) playerController.enabled = false;

        playerTransform.position = teleportDestination.position;
        playerTransform.rotation = teleportDestination.rotation;

        playerTransform.localScale = originalScale;

        if (playerController != null)
        {
            playerController.height = originalHeight;
            playerController.radius = originalRadius;
            playerController.center = originalCenter;
            playerController.enabled = true;
        }
    }
}
