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
    
    private bool hasTriggered = false;
    private bool playerInTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        playerInTrigger = true;
        
        // If not requiring key press, show dialog immediately
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
        // Check for E key press when player is in trigger zone
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
        ShowDialog();
    }
    
    private void ShowDialog()
    {
        if (dogDialogUI == null)
        {
            Debug.LogError("DogDialogTrigger: dogDialogUI is not assigned!");
            return;
        }
        
        // Get the DogDialogUI component
        var dogDialogComponent = dogDialogUI.GetComponent<Game.UI.DogDialogUI>();
        
        // Activate the dialog UI first
        dogDialogUI.SetActive(true);
        
        // Set the text and start typing animation using the public method
        if (dogDialogComponent != null)
        {
            dogDialogComponent.SetTextAndStartTyping(dialogMessage);
        }
        
        // Destroy the object if configured to do so
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
