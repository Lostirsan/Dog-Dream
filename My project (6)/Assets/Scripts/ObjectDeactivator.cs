using UnityEngine;

public class ObjectDeactivator : MonoBehaviour
{
    public MonoBehaviour scriptToActivate;
    public GameObject objectToActivate;
    public bool triggerOnce = true;

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            if (objectToActivate != null)
                objectToActivate.SetActive(true);

            if (scriptToActivate != null)
                scriptToActivate.enabled = true;

            activated = true;
        }
    }
}
