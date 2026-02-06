using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BedSleepTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("UI Image for the fade effect (should be a full-screen black panel)")]
    public Image fadeImage;
    
    [Header("Effect Settings")]
    [Tooltip("Duration of the fade to black")]
    public float fadeDuration = 1.5f;
    
    [Tooltip("Duration to hold the black screen")]
    public float holdDuration = 1f;
    
    [Tooltip("Duration of the fade back")]
    public float fadeOutDuration = 1.5f;
    
    [Tooltip("Can only be triggered once")]
    public bool triggerOnce = true;
    
    [Header("Interaction Settings")]
    [Tooltip("Distance from which player can interact")]
    public float interactionDistance = 3f;
    
    private bool hasTriggered = false;
    private bool isEffectRunning = false;
    private Transform playerTransform;
    
    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Ensure fade image starts hidden
        if (fadeImage != null)
        {
            // Remove any custom material, use default UI
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
        
        // Check distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distance <= interactionDistance)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("BedSleepTrigger: E pressed, starting effect");
                StartCoroutine(FadeRoutine());
            }
        }
    }
    
    private IEnumerator FadeRoutine()
    {
        if (fadeImage == null)
        {
            Debug.LogError("BedSleepTrigger: fadeImage is not assigned!");
            yield break;
        }
        
        isEffectRunning = true;
        hasTriggered = true;
        
        Debug.Log("BedSleepTrigger: Starting fade to black");
        
        // Activate fade image and set to transparent
        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        
        // Phase 1: Fade to black
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
        
        // Ensure fully black
        c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
        
        Debug.Log("BedSleepTrigger: Holding black screen");
        
        // Phase 2: Hold black screen
        yield return new WaitForSeconds(holdDuration);
        
        // Phase 3: Fade back from black
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
        
        // Ensure fully transparent
        c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        
        // Deactivate fade image
        fadeImage.gameObject.SetActive(false);
        
        Debug.Log("BedSleepTrigger: Effect complete");
        
        isEffectRunning = false;
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
    
    // Draw interaction radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
