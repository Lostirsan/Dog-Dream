using UnityEngine;

public class ActivateObjectTrigger : MonoBehaviour
{
    public GameObject targetObject;

    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        if (targetObject != null)
            targetObject.SetActive(true);

        used = true;
        gameObject.SetActive(false);
    }
}
