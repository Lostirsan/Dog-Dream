using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BedSleepEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player's Transform to scale down")]
    public Transform playerTransform;
    
    [Tooltip("UI Image for screen fade (should be a full-screen black panel)")]
    public Image fadeImage;
    
    [Header("Effect Settings")]
    [Tooltip("Duration of the fade to black")]
    public float fadeDuration = 2f;
    
    [Tooltip("Duration to hold the black screen")]
    public float holdDuration = 1f;
    
    [Tooltip("Duration of the fade back from black")]
    public float fadeOutDuration = 2f;
    
    [Tooltip("Scale multiplier for the player (0.5 = half size)")]
    public float playerScaleMultiplier = 0.5f;
    
    [Tooltip("Can only be triggered once")]
    public bool triggerOnce = true;
    
    private bool hasTriggered = false;
    private bool playerInTrigger = false;
    private bool isEffectRunning = false;
    
    private void Start()
    {
        // Ensure fade image starts fully transparent
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
            
        playerInTrigger = true;
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
        if (playerInTrigger && !isEffectRunning && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (triggerOnce && hasTriggered)
                return;
                
            StartCoroutine(SleepEffectRoutine());
        }
    }
    
    private IEnumerator SleepEffectRoutine()
    {
        if (fadeImage == null || playerTransform == null)
        {
            Debug.LogError("BedSleepEffect: fadeImage or playerTransform is not assigned!");
            yield break;
        }
        
        isEffectRunning = true;
        hasTriggered = true;
        
        // Activate fade image
        fadeImage.gameObject.SetActive(true);
        
        // Store original player scale
        Vector3 originalScale = playerTransform.localScale;
        Vector3 targetScale = new Vector3(
            originalScale.x * playerScaleMultiplier,
            originalScale.y * playerScaleMultiplier,
            originalScale.z * playerScaleMultiplier
        );
        
        // Phase 1: Fade to black
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
            
            yield return null;
        }
        
        // Ensure fully black
        Color fullBlack = fadeImage.color;
        fullBlack.a = 1f;
        fadeImage.color = fullBlack;
        
        // Phase 2: Hold black screen and shrink player
        elapsed = 0f;
        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / holdDuration);
            
            // Smoothly scale down the player
            playerTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            
            yield return null;
        }
        
        // Ensure player is at target scale
        playerTransform.localScale = targetScale;
        
        // Phase 3: Fade back from black
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
            
            yield return null;
        }
        
        // Ensure fully transparent
        Color transparent = fadeImage.color;
        transparent.a = 0f;
        fadeImage.color = transparent;
        
        // Deactivate fade image
        fadeImage.gameObject.SetActive(false);
        
        isEffectRunning = false;
    }
    
    public void ResetEffect()
    {
        hasTriggered = false;
    }
}
