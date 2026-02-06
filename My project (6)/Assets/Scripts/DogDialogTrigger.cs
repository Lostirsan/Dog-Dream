using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DogDialogTrigger : MonoBehaviour
{
    [Header("Dialog Settings")]
    [TextArea(3, 5)]
    public string dialogMessage = "I love you";

    [Header("References")]
    public GameObject dogDialogUI;
    public TextMeshProUGUI dialogText;

    [Header("Trigger Settings")]
    public bool triggerOnce = true;
    public bool requireKeyPress = false;
    public bool destroyAfterTrigger = false;

    [Header("Delayed Trigger")]
    public bool delayedTrigger = false;
    public float delaySeconds = 2f;

    private bool hasTriggered = false;
    private bool playerInTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInTrigger = true;

        if (!requireKeyPress)
        {
            TryShowDialog();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInTrigger = false;
    }

    private void Update()
    {
        if (requireKeyPress && playerInTrigger && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryShowDialog();
        }
    }

    private void TryShowDialog()
    {
        if (triggerOnce && hasTriggered)
            return;

        hasTriggered = true;

        if (delayedTrigger)
        {
            StartCoroutine(DelayedShowRoutine());
        }
        else
        {
            ShowDialog();
        }
    }

    private IEnumerator DelayedShowRoutine()
    {
        yield return new WaitForSeconds(delaySeconds);
        ShowDialog();
    }

    private void ShowDialog()
    {
        if (dogDialogUI == null)
        {
            Debug.LogError("DogDialogTrigger: dogDialogUI is not assigned!");
            return;
        }

        var dogDialogComponent = dogDialogUI.GetComponent<Game.UI.DogDialogUI>();

        dogDialogUI.SetActive(true);

        if (dogDialogComponent != null)
        {
            dogDialogComponent.SetTextAndStartTyping(dialogMessage);
        }

        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
