using UnityEngine;
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
    
    private bool hasTriggered = false;


    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
            return;
            
        if (!other.CompareTag("Player"))
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
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
